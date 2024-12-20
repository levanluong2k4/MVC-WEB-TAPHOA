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

namespace web_levanluong_64131236.Controllers
{
    public class DonHangs_64131236Controller : Controller
    {
        private webbanhang64131236Entities1 db = new webbanhang64131236Entities1();


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
                ModelState.AddModelError("", "Phiên đăng nhập đã hết hạn");
                return View(donHang);
            }

            var gioHang = db.GioHangs
                .Include(g => g.ChiTietGioHangs.Select(ct => ct.HangHoa))
                .FirstOrDefault(g => g.MaKH == maKH);

            if (gioHang == null || !gioHang.ChiTietGioHangs.Any())
            {
                ModelState.AddModelError("", "Giỏ hàng trống hoặc không tồn tại");
                return View(donHang);
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
                    // Tạm thời set 0, trigger sẽ tính lại sau


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
                        PopulateViewBagData(originalTotal, donHang.MaGiamGia);
                        return View(donHang);
                    }

                    // 2. Lưu đơn hàng
                    db.DonHangs.Add(donHang);
                    db.SaveChanges();

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
                    db.SaveChanges();

                  
                    transaction.Commit();

                    return RedirectToAction("OrderConfirmation", new { id = donHang.MaDH });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"Error: {ex.ToString()}");
                    ModelState.AddModelError("", $"Lỗi khi tạo đơn hàng: {ex.Message}");
                    PopulateViewBagData(originalTotal, donHang.MaGiamGia);
                    return View(donHang);
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

        // AJAX method to calculate discounted total

        [HttpPost]
        public JsonResult ApplyDiscount(string maGiamGia, decimal tongTien)
        {
            try
            {
                var discount = db.Discounts.FirstOrDefault(d => d.MaGiamGia == maGiamGia);

                if (discount != null)
                {
                    decimal tongTienSauGiam = Math.Max(0, tongTien - discount.GiaTriGiamGia);
                    return Json(new
                    {
                        isValid = true,
                        tongTienSauGiam = tongTienSauGiam,
                        giaTriGiam = discount.GiaTriGiamGia
                    });
                }

                return Json(new
                {
                    isValid = false,
                    tongTienSauGiam = tongTien,
                    giaTriGiam = 0
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

                    // Find and delete the customer's cart
                    var gioHang = db.GioHangs.FirstOrDefault(g => g.MaKH == donHang.MaKH);
                    if (gioHang != null)
                    {
                        // Delete cart details first
                        var chiTietGioHang = db.ChiTietGioHangs.Where(ct => ct.MaGioHang == gioHang.MaGioHang);
                        db.ChiTietGioHangs.RemoveRange(chiTietGioHang);

                        // Then delete the cart
                        db.GioHangs.Remove(gioHang);
                    }

                    // Update order status
                    donHang.TrangThai = "Đã xác nhận";
                    donHang.TrangThaiGiaoHang = "Chưa giao hàng";

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
        public ActionResult QuanLyDonHang()
        {
            string maNV = Session["Admin"]?.ToString();
            if (string.IsNullOrEmpty(maNV))
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

            var donHangs = db.DonHangs
               .Include(d => d.ChiTietDonHangs.Select(ct => ct.HangHoa))
               .Include(d => d.KhachHang)
               .Include(d => d.Discount)
               .Include(d => d.AnhGiaoHangs)
               .OrderByDescending(d => d.NgayDat)
               .ToList();

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
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        // doanh thu 
        public ActionResult DoanhThu()
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

            // Get current date
            var currentDate = DateTime.Now;

            // Daily revenue
            var revenueToday = db.DonHangs
                .Where(d => d.TrangThai == "Đã xác nhận" &&
                       d.NgayDat.Year == currentDate.Year &&
                       d.NgayDat.Month == currentDate.Month &&
                       d.NgayDat.Day == currentDate.Day)
                .Sum(d => (decimal?)d.TongTien) ?? 0;

            // Monthly revenue
            var revenueThisMonth = db.DonHangs
                .Where(d => d.TrangThai == "Đã xác nhận" &&
                       d.NgayDat.Year == currentDate.Year &&
                       d.NgayDat.Month == currentDate.Month)
                .Sum(d => (decimal?)d.TongTien) ?? 0;

            // Yearly revenue
            var revenueThisYear = db.DonHangs
                .Where(d => d.TrangThai == "Đã xác nhận" &&
                       d.NgayDat.Year == currentDate.Year)
                .Sum(d => (decimal?)d.TongTien) ?? 0;

            // Best selling products
            var bestSellingProducts = db.ChiTietDonHangs
                .Where(ct => ct.DonHang.TrangThai == "Đã xác nhận")
                .GroupBy(ct => ct.HangHoa)
                .Select(g => new BestSellingProductViewModel
                {
                    MaHH = g.Key.MaHH,
                    TenHH = g.Key.TenHH,
                    TotalQuantity = g.Sum(ct => ct.SoLuong),
                    TotalRevenue = g.Sum(ct => ct.ThanhTien ?? 0),
                    AnhHH = g.Key.AnhHH
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToList();

            // Monthly revenue chart data
            var monthlyRevenue = db.DonHangs
                .Where(d => d.TrangThai == "Đã xác nhận" &&
                       d.NgayDat.Year == currentDate.Year)
                .GroupBy(d => d.NgayDat.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(d => d.TongTien)
                })
                .OrderBy(x => x.Month)
                .ToList();

            var monthlyRevenueData = Enumerable.Range(1, 12)
                .Select(month => new
                {
                    Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                    Revenue = monthlyRevenue.FirstOrDefault(x => x.Month == month)?.Revenue ?? 0
                })
                .ToList();

            // Prepare view model
            var viewModel = new RevenueViewModel
            {
                RevenueToday = revenueToday,
                RevenueThisMonth = revenueThisMonth,
                RevenueThisYear = revenueThisYear,
                BestSellingProducts = bestSellingProducts,
                MonthlyRevenueData = monthlyRevenueData
            };

            return View(viewModel);
        }



    }
}

       
    

