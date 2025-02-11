using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using web_levanluong_64131236.Models;
using System.Globalization;

namespace web_levanluong_64131236.Controllers
{
    public class DonHangs_64131236Controller : Controller
    {
        private webbanhang64131236Entities1 db = new webbanhang64131236Entities1();

        private const int PageSize = 10;
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        private void PopulateViewBagData(decimal originalTotal, string selectedMaGiamGia = null)
        {
            ViewBag.OriginalTotal = originalTotal;

            // Get discount data and materialize it first before formatting
            var discounts = db.Discounts
                .Select(d => new { d.MaGiamGia, d.TenGiamGia, d.GiaTriGiamGia })
                .ToList()
                .Select(d => new SelectListItem
                {
                    Value = d.MaGiamGia,
                    Text = $"{d.TenGiamGia} (Giảm {d.GiaTriGiamGia:N0}đ)"
                })
                .ToList();

            ViewBag.DiscountList = new SelectList(discounts, "Value", "Text", selectedMaGiamGia);

            ViewBag.PhuongThucThanhToan = new List<SelectListItem>
    {
        new SelectListItem { Text = "Tiền mặt khi nhận hàng", Value = "COD" },
        new SelectListItem { Text = "Chuyển khoản ngân hàng", Value = "BankTransfer" }
    };
        }

        public ActionResult Checkout()
        {
            string maKH = Session["MaKH"]?.ToString();
            if (string.IsNullOrEmpty(maKH))
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }
            var khachHang = db.KhachHangs.FirstOrDefault(k => k.MaKH == maKH);
            ViewBag.KhachHang = khachHang;
            var gioHang = db.GioHangs
                .Include(g => g.ChiTietGioHangs.Select(ct => ct.HangHoa))
                .FirstOrDefault(g => g.MaKH == maKH);

            ViewBag.DiscountList = new SelectList(db.Discounts.ToList(), "MaGiamGia", "TenGiamGia");
            ViewBag.SelectedDiscountType = ""; // Giá trị mặc định là rỗng

            if (gioHang == null)
            {
                gioHang = new GioHang
                {
                    MaKH = maKH,
                    NgayTao = DateTime.Now
                };
                db.GioHangs.Add(gioHang);
                db.SaveChanges();
            }

            if (!gioHang.ChiTietGioHangs.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "GioHangs_64131236");
            }

            // Ensure OriginalTotal is always set
            decimal originalTotal = gioHang.ChiTietGioHangs
                .Sum(ct => ct.SoLuong * ct.HangHoa.GiaBan);
            ViewBag.OriginalTotal = originalTotal;

            PopulateViewBagData(originalTotal);
            ViewBag.GioHang = gioHang;

            var donHang = new DonHang
            {
                MaKH = maKH,
                NgayDat = DateTime.Now,
                TongTien = originalTotal,
             
                TrangThai = "Chờ xác nhận",
                TrangThaiGiaoHang = "Chờ xử lý"
            };

            return View(donHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]


        public ActionResult Checkout([Bind(Include = "MaDH,NgayDat,PhuongThucThanhToan,MaGiamGia,TongTien,TrangThai,MaKH,TrangThaiGiaoHang")] DonHang donHang)
        {
            string maKH = Session["MaKH"]?.ToString();
            if (string.IsNullOrEmpty(maKH))
            {
                return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn" });
            }
            var gioHang = db.GioHangs
                .Include(g => g.ChiTietGioHangs.Select(ct => ct.HangHoa))
                .FirstOrDefault(g => g.MaKH == maKH);
            if (gioHang == null || !gioHang.ChiTietGioHangs.Any())
            {
                return Json(new { success = false, message = "Giỏ hàng trống hoặc không tồn tại" });
            }
            decimal originalTotal = gioHang.ChiTietGioHangs.Sum(ct => ct.SoLuong * ct.HangHoa.GiaBan);

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Thiết lập thông tin đơn hàng
                    donHang.NgayDat = DateTime.Now;
                    donHang.TrangThai = "Chờ xác nhận";
                    donHang.TrangThaiGiaoHang = "Chưa giao hàng";
                    donHang.MaKH = maKH;

                    if (!string.IsNullOrEmpty(donHang.MaGiamGia))
                    {
                        var discount = db.Discounts.Find(donHang.MaGiamGia);
                        if (discount != null)
                        {
                            donHang.TongTien = Math.Max(0, originalTotal - discount.GiaTriGiamGia);
                        }
                        else
                        {
                            donHang.TongTien = originalTotal;
                        }
                    }
                    else
                    {
                        donHang.TongTien = originalTotal;
                    }

                    if (!ModelState.IsValid)
                    {
                        transaction.Rollback();
                        return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                    }

                    // 2. Lưu đơn hàng
                    db.DonHangs.Add(donHang);

                    // 3. Tạo chi tiết đơn hàng từ giỏ hàng
                    foreach (var item in gioHang.ChiTietGioHangs)
                    {
                        var chiTietDonHang = new ChiTietDonHang
                        {
                            MaDH = donHang.MaDH,
                            MaHH = item.MaHH,
                            SoLuong = item.SoLuong,
                            DonGia = item.HangHoa.GiaBan
                        };
                        db.ChiTietDonHangs.Add(chiTietDonHang);
                    }

                    // 4. Xóa giỏ hàng và chi tiết giỏ hàng
                    var chiTietGioHang = db.ChiTietGioHangs.Where(c => c.MaGioHang == gioHang.MaGioHang);
                    db.ChiTietGioHangs.RemoveRange(chiTietGioHang);
                    db.GioHangs.Remove(gioHang);

                    db.SaveChanges();
                    transaction.Commit();

                    return Json(new
                    {
                        success = true,
                        message = "Đặt hàng thành công!",
                        orderId = donHang.MaDH,
                        redirectUrl = Url.Action("OrderConfirmation", "DonHangs_64131236", new { id = donHang.MaDH })
                    });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"Error: {ex.ToString()}");
                    return Json(new { success = false, message = $"Lỗi khi tạo đơn hàng: {ex.Message}" });
                }
            }
        }
        // GET: DonHangs_64131236/OrderConfirmation/5
        public ActionResult OrderConfirmation(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            DonHang donHang = db.DonHangs
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.HangHoa))
                .Include(d => d.KhachHang)
                .Include(d => d.Discount)
                .FirstOrDefault(d => d.MaDH == id);

            if (donHang == null)
            {
                return HttpNotFound();
            }

            return View(donHang);
        }


        [HttpPost]
      
        public JsonResult HuyDonHang(int id)
        {
            try
            {
                var donHang = db.DonHangs.Find(id);
                if (donHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                if (donHang.TrangThai != "Chờ xác nhận")
                {
                    return Json(new { success = false, message = "Chỉ có thể hủy đơn hàng chưa được xác nhận" });
                }

                donHang.TrangThai = "Đã hủy";
                donHang.TrangThaiGiaoHang = "Chưa giao hàng";
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Add a new action to view order history
        public ActionResult OrderTracking()
        {
            string maKH = Session["MaKH"]?.ToString();
            if (string.IsNullOrEmpty(maKH))
            {
                return RedirectToAction("Login", "QuanTris");
            }

            var orders = db.DonHangs
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.HangHoa))
                .Include(d => d.KhachHang)
                .Include(d => d.Discount)
                .Where(d => d.MaKH == maKH &&
                       (d.TrangThai == "Chờ xác nhận" || d.TrangThai == "Đã xác nhận"))
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            return View(orders);
        }

        public ActionResult CanceledOrders()
        {
            string maKH = Session["MaKH"]?.ToString();
            if (string.IsNullOrEmpty(maKH))
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

            var canceledOrders = db.DonHangs
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.HangHoa))
                .Include(d => d.KhachHang)
                .Include(d => d.Discount)
                .Where(d => d.MaKH == maKH && d.TrangThai == "Đã hủy")
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            return View(canceledOrders);
        }

        public ActionResult SuccessOrders()
        {
            string maKH = Session["MaKH"]?.ToString();
            if (string.IsNullOrEmpty(maKH))
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

            var canceledOrders = db.DonHangs
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.HangHoa))
                .Include(d => d.KhachHang)
                .Include(d => d.Discount)
                .Where(d => d.MaKH == maKH && d.TrangThaiGiaoHang == "Đã nhận hàng")
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            return View(canceledOrders);
        }



        // AJAX method to calculate discounted total

        [HttpPost]
       public JsonResult ApplyDiscount(string maGiamGia, decimal tongTien)
{
    try
    {
        // Tìm mã giảm giá
        var discount = db.Discounts.FirstOrDefault(d => d.MaGiamGia == maGiamGia);

        // Kiểm tra mã giảm giá
        if (discount == null)
        {
            return Json(new
            {
                isValid = false,
                tongTienSauGiam = tongTien,
                giaTriGiam = 0
            });
        }

        // Kiểm tra giá trị giảm giá
        if (discount.GiaTriGiamGia <= 0)
        {
            return Json(new
            {
                isValid = false,
                tongTienSauGiam = tongTien,
                giaTriGiam = 0
            });
        }

        // Áp dụng giảm giá dựa trên loại
        decimal tongTienSauGiam = tongTien;
        if (discount.LoaiGiamGia == "amount")
        {
            // Giảm theo số tiền cố định
            tongTienSauGiam = Math.Max(0, tongTien - discount.GiaTriGiamGia);
        }
        else if (discount.LoaiGiamGia == "percent")
        {
            // Giảm theo phần trăm
            if (discount.GiaTriGiamGia > 100)
            {
                return Json(new
                {
                    isValid = false,
                    tongTienSauGiam = tongTien,
                    giaTriGiam = 0
                });
            }
            tongTienSauGiam = Math.Max(0, tongTien - (tongTien * discount.GiaTriGiamGia / 100));
        }

        // Trả về kết quả hợp lệ
        return Json(new
        {
            isValid = true,
            tongTienSauGiam = tongTienSauGiam,
            giaTriGiam = discount.GiaTriGiamGia
        });
    }
    catch (Exception)
    {
        return Json(new
        {
            isValid = false,
            tongTienSauGiam = tongTien,
            giaTriGiam = 0
        });
    }
}




        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
    
        public JsonResult XacNhanDonHang(int id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    string maNV = Session["Admin"]?.ToString();
                    if (string.IsNullOrEmpty(maNV))
                    {
                        return Json(new { success = false, message = "Bạn không có quyền thực hiện chức năng này" });
                    }

                    var donHang = db.DonHangs
                        .Include(d => d.ChiTietDonHangs)
                        .Include(d => d.KhachHang)
                        .FirstOrDefault(d => d.MaDH == id);

                    if (donHang == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                    }

                    if (donHang.TrangThai != "Chờ xác nhận")
                    {
                        return Json(new { success = false, message = "Không thể xác nhận đơn hàng này" });
                    }

                    // Update product quantities
                    foreach (var chiTiet in donHang.ChiTietDonHangs)
                    {
                        var hangHoa = db.HangHoas.Find(chiTiet.MaHH);
                        if (hangHoa != null)
                        {
                            if (hangHoa.SoLuongHangTon < chiTiet.SoLuong)
                            {
                                transaction.Rollback();
                                return Json(new { success = false, message = $"Sản phẩm {hangHoa.TenHH} không đủ số lượng trong kho" });
                            }
                            hangHoa.SoLuongHangTon -= chiTiet.SoLuong;
                        }
                    }

                    
                   

                    // Update order status
                    donHang.TrangThai = "Đã xác nhận";
                    donHang.TrangThaiGiaoHang = "Chưa giao hàng";
                    ThemLichSuCapNhat(donHang);
                    // Save all changes
                    db.SaveChanges();
                    transaction.Commit();

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"Error in XacNhanDonHang: {ex.Message}");
                    return Json(new { success = false, message = $"Lỗi xác nhận đơn hàng: {ex.Message}" });
                }
            }
        }

        // Action cho trang quản lý đơn hàng của nhân viên




        [AdminAuthorize]
        public ActionResult QuanLyDonHang(string trangThai, string trangThaiGiaoHang, DateTime? tuNgay, DateTime? denNgay, int? page)
        {
            var query = db.DonHangs
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.HangHoa))
                .Include(d => d.KhachHang)
                .Include(d => d.Discount)
                .Include(d => d.AnhGiaoHangs)
                .AsQueryable();

            // Filter by order status
            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(d => d.TrangThai == trangThai);
            }

            // Filter by delivery status
            if (!string.IsNullOrEmpty(trangThaiGiaoHang))
            {
                query = query.Where(d => d.TrangThaiGiaoHang == trangThaiGiaoHang);
            }

            // Filter by date range - Fixed version
            if (tuNgay.HasValue)
            {
                DateTime startDate = tuNgay.Value.Date;
                query = query.Where(d => DbFunctions.TruncateTime(d.NgayDat) >= startDate);
            }

            if (denNgay.HasValue)
            {
                DateTime endDate = denNgay.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(d => DbFunctions.TruncateTime(d.NgayDat) <= endDate);
            }

            // Prepare filter data for dropdowns
            ViewBag.TrangThaiList = new List<SelectListItem>
        {
            new SelectListItem { Text = "Tất cả", Value = "" },
            new SelectListItem { Text = "Chờ xác nhận", Value = "Chờ xác nhận" },
            new SelectListItem { Text = "Đã xác nhận", Value = "Đã xác nhận" },
            new SelectListItem { Text = "Đã hủy", Value = "Đã hủy" }
        };

            ViewBag.TrangThaiGiaoHangList = new List<SelectListItem>
        {
            new SelectListItem { Text = "Tất cả", Value = "" },
            new SelectListItem { Text = "Chưa giao hàng", Value = "Chưa giao hàng" },
            new SelectListItem { Text = "Đang giao hàng", Value = "Đang giao hàng" },
            new SelectListItem { Text = "Giao hàng thành công", Value = "Giao hàng thành công" },
            new SelectListItem { Text = "Đã nhận hàng", Value = "Đã nhận hàng" }
        };

            // Set current filter values for the view
            ViewBag.CurrentTrangThai = trangThai;
            ViewBag.CurrentTrangThaiGiaoHang = trangThaiGiaoHang;
            ViewBag.CurrentTuNgay = tuNgay?.ToString("yyyy-MM-dd");
            ViewBag.CurrentDenNgay = denNgay?.ToString("yyyy-MM-dd");

            // Order by date descending
            query = query.OrderByDescending(d => d.NgayDat);

            // Calculate total pages
            int pageNumber = (page ?? 1);
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            // Get paginated results
            var donHangs = query
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Set pagination info
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = pageNumber > 1;
            ViewBag.HasNextPage = pageNumber < totalPages;

            return View(donHangs);
        }

        public ActionResult InHoaDon(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var donHang = db.DonHangs
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.HangHoa))
                .Include(d => d.KhachHang)
                .Include(d => d.Discount)
                .FirstOrDefault(d => d.MaDH == id);

            if (donHang == null)
            {
                return HttpNotFound();
            }

            return View(donHang);
        }



        [HttpPost]
        [AdminAuthorize]
        public JsonResult UpdateDeliveryStatus(int id, string status)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var donHang = db.DonHangs.Find(id);
                    if (donHang == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                    }

                    // Validate status transition
                    if (!IsValidStatusTransition(donHang.TrangThaiGiaoHang, status))
                    {
                        return Json(new { success = false, message = "Trạng thái giao hàng không hợp lệ" });
                    }

                    donHang.TrangThaiGiaoHang = status;
                    ThemLichSuCapNhat(donHang);
                    db.SaveChanges();
                    transaction.Commit();

                    bool requiresImage = status == "Giao hàng thành công";
                    return Json(new { success = true, requiresImage = requiresImage });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = ex.Message });
                }
            }
        }

        private bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            var validTransitions = new Dictionary<string, string[]>
        {
            { "Chưa giao hàng", new[] { "Đang giao hàng" } },
            { "Đang giao hàng", new[] { "Giao hàng thành công" } },
            { "Giao hàng thành công", new[] { "Đã nhận hàng" } }
        };

            return validTransitions.ContainsKey(currentStatus) &&
                   validTransitions[currentStatus].Contains(newStatus);
        }

        [HttpPost]
        [AdminAuthorize]
        public JsonResult UploadDeliveryImage(int id, HttpPostedFileBase image)
        {
            if (image == null || image.ContentLength == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn hình ảnh" });
            }

            try
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(image.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    return Json(new { success = false, message = "Chỉ chấp nhận file ảnh định dạng .jpg, .jpeg, .png" });
                }

                // Save the image
                var fileName = $"delivery_{id}_{DateTime.Now.Ticks}{extension}";
                var path = Path.Combine(Server.MapPath("~/Content/DeliveryImages"), fileName);
                image.SaveAs(path);

                // Save to database
                var anhGiaoHang = new AnhGiaoHang
                {
                    MaDH = id,
                    DuongDanAnh = $"/Content/DeliveryImages/{fileName}",
                    NgayTao = DateTime.Now
                };

                db.AnhGiaoHangs.Add(anhGiaoHang);
                db.SaveChanges();

                return Json(new { success = true, imagePath = anhGiaoHang.DuongDanAnh });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Add this action for customers to confirm receipt
        [HttpPost]
        public JsonResult ConfirmDelivery(int id)
        {
            string maKH = Session["MaKH"]?.ToString();
            if (string.IsNullOrEmpty(maKH))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            try
            {
                var donHang = db.DonHangs.Find(id);
                if (donHang == null || donHang.MaKH != maKH)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                if (donHang.TrangThaiGiaoHang != "Giao hàng thành công")
                {
                    return Json(new { success = false, message = "Đơn hàng chưa được giao thành công" });
                }

                donHang.TrangThaiGiaoHang = "Đã nhận hàng";
                ThemLichSuCapNhat(donHang);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        // doanh thu 
        public ActionResult DoanhThu(string period = "day", DateTime? startDate = null, DateTime? endDate = null , int page = 1)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }
          

            var currentDate = DateTime.Now;
            startDate = startDate ?? currentDate.AddDays(-30);
            endDate = endDate ?? currentDate;
          

            var viewModel = new RevenueViewModel();
            viewModel.RevenueToday = CalculateRevenueForDate(endDate.Value);
            viewModel.RevenueThisMonth = CalculateRevenueForMonth(endDate.Value.Year, endDate.Value.Month);
            viewModel.RevenueThisYear = CalculateRevenueForYear(endDate.Value.Year);

            // Always get both daily and monthly data
            viewModel.DailyRevenue = GetDailyRevenue(startDate.Value, endDate.Value);
        

          

            return View(viewModel);
        }

        private decimal CalculateRevenueForDate(DateTime date)
        {
            return db.DonHangs
                .Where(d => d.TrangThaiGiaoHang == "Giao hàng thành công" &&
                       DbFunctions.TruncateTime(d.NgayDat) == DbFunctions.TruncateTime(date))
                .Select(d => d.TongTien)
                .DefaultIfEmpty(0m)
                .Sum();
        }

        private decimal CalculateRevenueForMonth(int year, int month)
        {
            return db.DonHangs
                .Where(d => d.TrangThaiGiaoHang == "Giao hàng thành công" &&
                       d.NgayDat.Year == year && d.NgayDat.Month == month)
                .Select(d => d.TongTien)
                .DefaultIfEmpty(0m)
                .Sum();
        }

        private decimal CalculateRevenueForYear(int year)
        {
            return db.DonHangs
                .Where(d => d.TrangThaiGiaoHang == "Giao hàng thành công" && d.NgayDat.Year == year)
                .Select(d => d.TongTien)
                .DefaultIfEmpty(0m)
                .Sum();
        }


        private List<BestSellingProductViewModel> GetBestSellingProducts()
        {
            var bestSellingProducts = db.ChiTietDonHangs
                .Join(db.HangHoas, ct => ct.MaHH, hh => hh.MaHH, (ct, hh) => new { ct, hh })
                .GroupBy(x => new { x.hh.MaHH, x.hh.TenHH, x.hh.AnhHH })
                .Select(g => new BestSellingProductViewModel
                {
                    MaHH = g.Key.MaHH,
                    TenHH = g.Key.TenHH,
                    TotalQuantity = g.Sum(x => x.ct.SoLuong),
                    TotalRevenue = g.Sum(x => x.ct.SoLuong * x.ct.DonGia),
                    AnhHH = g.Key.AnhHH
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(10) // Take top 10 best-selling products
                .ToList();

            return bestSellingProducts;
        }

        private List<DailyRevenueViewModel> GetDailyRevenue(DateTime startDate, DateTime endDate)
        {
            // First get all dates in the range
            var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(i => startDate.AddDays(i).Date)
                .ToList();

            // Get revenue data for dates that have orders
            var dailyRevenue = db.DonHangs
                .Where(d => d.TrangThaiGiaoHang == "Giao hàng thành công" &&
                       d.NgayDat >= startDate &&
                       d.NgayDat <= endDate)
                .GroupBy(d => DbFunctions.TruncateTime(d.NgayDat))
                .Select(g => new
                {
                    Date = g.Key.Value,
                    Revenue = g.Sum(d => (decimal?)d.TongTien) ?? 0m,
                    OrderCount = g.Count(),
                    ProductCount = g.Sum(d => d.ChiTietDonHangs.Sum(ct => (int?)ct.SoLuong)) ?? 0,
                    AverageOrderValue = g.Average(d => (decimal?)d.TongTien) ?? 0m
                })
                .ToDictionary(x => x.Date, x => x);

            // Combine all dates with revenue data, using 0 for dates with no orders
            var result = allDates.Select(date => new DailyRevenueViewModel
            {
                Date = date,
                Revenue = dailyRevenue.ContainsKey(date) ? dailyRevenue[date].Revenue : 0,
                OrderCount = dailyRevenue.ContainsKey(date) ? dailyRevenue[date].OrderCount : 0,
                ProductCount = dailyRevenue.ContainsKey(date) ? dailyRevenue[date].ProductCount : 0,
                AverageOrderValue = dailyRevenue.ContainsKey(date) ? dailyRevenue[date].AverageOrderValue : 0
            })
            .OrderByDescending(x => x.Date)
            .ToList();

            return result;
        }

        // Helper method to check if there are any orders for a given date
        private bool HasOrdersForDate(DateTime date)
        {
            return db.DonHangs
                .Any(d => d.TrangThaiGiaoHang == "Giao hàng thành công" &&
                         DbFunctions.TruncateTime(d.NgayDat) == date.Date);
        }

        // Helper method to format the message for days with no orders
        private string GetNoOrdersMessage(DateTime date)
        {
            return $"Không có đơn hàng nào được đặt vào ngày {date:dd/MM/yyyy}";
        }

        private List<MonthlyRevenueViewModel> GetMonthlyRevenue(int year)
        {
            // First get the data from database without month name
            var monthlyRevenue = db.DonHangs
                .Where(d => d.TrangThaiGiaoHang == "Giao hàng thành công" && d.NgayDat.Year == year)
                .GroupBy(d => new { Month = d.NgayDat.Month, Year = d.NgayDat.Year })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Revenue = g.Sum(d => d.TongTien),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList()  // Execute the query here
                .Select(x => new MonthlyRevenueViewModel  // Then process the results
                {
                    Month = x.Month,
                    Year = x.Year,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Month),
                    Revenue = x.Revenue,
                    OrderCount = x.OrderCount
                })
                .ToList();

            // Calculate growth rates
            for (int i = 1; i < monthlyRevenue.Count; i++)
            {
                var previousRevenue = monthlyRevenue[i - 1].Revenue;
                var currentRevenue = monthlyRevenue[i].Revenue;
                monthlyRevenue[i].GrowthRate = previousRevenue > 0
                    ? ((currentRevenue - previousRevenue) / previousRevenue) * 100
                    : 0;
            }

            return monthlyRevenue;
        }

        [HttpPost]
        public JsonResult BuyAgain(int orderId)
        {
            try
            {
                string maKH = Session["MaKH"]?.ToString();
                if (string.IsNullOrEmpty(maKH))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để thêm vào giỏ hàng" });
                }

                // Lấy thông tin đơn hàng
                var donHang = db.DonHangs
                    .Include(d => d.ChiTietDonHangs.Select(ct => ct.HangHoa))
                    .FirstOrDefault(d => d.MaDH == orderId);

                if (donHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                // Kiểm tra điều kiện trạng thái
                if (donHang.TrangThaiGiaoHang != "Đã nhận hàng" && donHang.TrangThai != "Đã hủy")
                {
                    return Json(new { success = false, message = "Đơn hàng không thể mua lại trong trạng thái hiện tại" });
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Tìm hoặc tạo giỏ hàng
                        var gioHang = db.GioHangs.FirstOrDefault(g => g.MaKH == maKH);
                        if (gioHang == null)
                        {
                            gioHang = new GioHang
                            {
                                MaKH = maKH,
                                NgayTao = DateTime.Now
                            };
                            db.GioHangs.Add(gioHang);
                            db.SaveChanges();
                        }

                        // Thêm từng sản phẩm vào giỏ hàng
                        foreach (var chiTietDonHang in donHang.ChiTietDonHangs)
                        {
                            var hangHoa = chiTietDonHang.HangHoa;

                            // Kiểm tra số lượng tồn
                            if (hangHoa.SoLuongHangTon < chiTietDonHang.SoLuong)
                            {
                                transaction.Rollback();
                                return Json(new
                                {
                                    success = false,
                                    message = $"Sản phẩm {hangHoa.TenHH} không đủ số lượng trong kho"
                                });
                            }

                            var chiTietGioHang = db.ChiTietGioHangs
                                .FirstOrDefault(ct => ct.MaGioHang == gioHang.MaGioHang &&
                                                    ct.MaHH == chiTietDonHang.MaHH);

                            if (chiTietGioHang != null)
                            {
                                chiTietGioHang.SoLuong += chiTietDonHang.SoLuong;
                            }
                            else
                            {
                                chiTietGioHang = new ChiTietGioHang
                                {
                                    MaGioHang = gioHang.MaGioHang,
                                    MaHH = chiTietDonHang.MaHH,
                                    SoLuong = chiTietDonHang.SoLuong
                                };
                                db.ChiTietGioHangs.Add(chiTietGioHang);
                            }
                        }

                        db.SaveChanges();
                        transaction.Commit();

                        return Json(new { success = true });
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi thêm vào giỏ hàng: " + ex.Message });
            }
        }


        //chỉnh sửa địa chỉ
        [HttpPost]
        public JsonResult UpdateAddress(int orderId, string newAddress)
        {
            try
            {
                var donHang = db.DonHangs.FirstOrDefault(d => d.MaDH == orderId);
                if (donHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                if (donHang.TrangThai != "Chờ xác nhận")
                {
                    return Json(new { success = false, message = "Chỉ có thể sửa địa chỉ khi đơn hàng đang chờ xác nhận" });
                }

                var khachHang = db.KhachHangs.FirstOrDefault(k => k.MaKH == donHang.MaKH);
                if (khachHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng" });
                }

                khachHang.DiaChi = newAddress;
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        private void ThemLichSuCapNhat(DonHang donHang)
        {
            var lichSuCapNhat = new ThoiGianCapNhatDonHang
            {
                MaDH = donHang.MaDH,
                TrangThai = donHang.TrangThai,
                TrangThaiGiaoHang = donHang.TrangThaiGiaoHang,
                ThoiGianCapNhat = DateTime.Now
            };
            db.ThoiGianCapNhatDonHangs.Add(lichSuCapNhat);
        }


    }
}

       
    

