using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
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




        ///Backend Nhân viên 


        [HttpPost]
        [ValidateAntiForgeryToken]
       [AdminAuthorize]
        public JsonResult XacNhanDonHang(int id)
        {
            try
            {
                // Kiểm tra đăng nhập là nhân viên
                string maNV = Session["Admin"]?.ToString();
                if (string.IsNullOrEmpty(maNV))
                {
                    
                    if (maNV != "NhanVien")
                    {
                        return Json(new { success = false, message = "Bạn không có quyền thực hiện chức năng này" });
                    }
                }

                var donHang = db.DonHangs.Find(id);
                if (donHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                // Chỉ xác nhận được đơn hàng đang ở trạng thái chờ xác nhận
                if (donHang.TrangThai != "Chờ xác nhận")
                {
                    return Json(new { success = false, message = "Không thể xác nhận đơn hàng này" });
                }

                donHang.TrangThai = "Đã xác nhận";
                donHang.TrangThaiGiaoHang = "Đang giao hàng";
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Action cho trang quản lý đơn hàng của nhân viên
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
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            return View(donHangs);
        }



    }
}

       
    

