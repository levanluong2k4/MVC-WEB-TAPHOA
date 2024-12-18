using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using web_levanluong_64131236.Models;

namespace web_levanluong_64131236.Controllers
{
    public class GioHangs_64131236Controller : Controller
    {
        private webbanhang64131236Entities1 db = new webbanhang64131236Entities1();
        
        // GET: GioHangs_64131236
        public ActionResult Index()
        {
            string maKH = Session["MaKH"]?.ToString();
            if (string.IsNullOrEmpty(maKH))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var gioHang = db.GioHangs
                .Include(g => g.ChiTietGioHangs.Select(ct => ct.HangHoa))
                .FirstOrDefault(g => g.MaKH == maKH);

            return View(gioHang);
        }
   


        // POST: Add to Cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddToCart(string maHH, int soLuong)
        {
            try
            {
                string maKH = Session["MaKH"]?.ToString();
                if (string.IsNullOrEmpty(maKH))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để thêm vào giỏ hàng" });
                }

                var hangHoa = db.HangHoas.Find(maHH);
                if (hangHoa == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                }

                if (hangHoa.SoLuongHangTon < soLuong)
                {
                    return Json(new { success = false, message = "Số lượng hàng trong kho không đủ" });
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Tìm giỏ hàng hiện tại
                        var gioHang = db.GioHangs.FirstOrDefault(g => g.MaKH == maKH);

                        // Nếu chưa có giỏ hàng, tạo mới
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

                        // Kiểm tra sản phẩm trong giỏ
                        var chiTietGioHang = db.ChiTietGioHangs
                            .FirstOrDefault(ct => ct.MaGioHang == gioHang.MaGioHang && ct.MaHH == maHH);

                        if (chiTietGioHang != null)
                        {
                            // Cập nhật số lượng nếu đã có
                            chiTietGioHang.SoLuong += soLuong;
                        }
                        else
                        {
                            // Thêm mới nếu chưa có
                            chiTietGioHang = new ChiTietGioHang
                            {
                                MaGioHang = gioHang.MaGioHang,
                                MaHH = maHH,
                                SoLuong = soLuong
                            };
                            db.ChiTietGioHangs.Add(chiTietGioHang);
                        }

                        db.SaveChanges();
                        transaction.Commit();

                        // Cập nhật số lượng giỏ hàng trong ViewBag
                        var cartCount = db.ChiTietGioHangs
                            .Where(ct => ct.GioHang.MaKH == maKH)
                            .Sum(ct => ct.SoLuong);

                        return Json(new
                        {
                            success = true,
                            message = "Đã thêm vào giỏ hàng thành công",
                            cartCount = cartCount
                        });
                    }
                    catch (Exception ex)
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


        public class CartResult
        {
            public string Status { get; set; }
            public string Message { get; set; }
        }
        // POST: Update Cart Item Quantity
        [HttpPost]
        public JsonResult UpdateQuantity(string maHH, int soLuong)
        {
            try
            {
                string maKH = Session["MaKH"]?.ToString();
                if (string.IsNullOrEmpty(maKH))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var gioHang = db.GioHangs.FirstOrDefault(g => g.MaKH == maKH);
                if (gioHang == null)
                {
                    return Json(new { success = false, message = "Giỏ hàng không tồn tại" });
                }

                var chiTietGioHang = db.ChiTietGioHangs
                    .FirstOrDefault(ct => ct.MaGioHang == gioHang.MaGioHang && ct.MaHH == maHH);

                if (chiTietGioHang == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không có trong giỏ hàng" });
                }

                var hangHoa = db.HangHoas.Find(maHH);
                if (hangHoa.SoLuongHangTon < soLuong)
                {
                    return Json(new { success = false, message = "Số lượng hàng trong kho không đủ" });
                }

                chiTietGioHang.SoLuong = soLuong;
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Cập nhật số lượng thành công",
                    newTotal = chiTietGioHang.SoLuong * hangHoa.GiaBan
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Remove Item from Cart
        [HttpPost]
        public JsonResult RemoveItem(string maHH)
        {
            try
            {
                string maKH = Session["MaKH"]?.ToString();
                if (string.IsNullOrEmpty(maKH))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var gioHang = db.GioHangs.FirstOrDefault(g => g.MaKH == maKH);
                if (gioHang == null)
                {
                    return Json(new { success = false, message = "Giỏ hàng không tồn tại" });
                }

                var chiTietGioHang = db.ChiTietGioHangs
                    .FirstOrDefault(ct => ct.MaGioHang == gioHang.MaGioHang && ct.MaHH == maHH);

                if (chiTietGioHang != null)
                {
                    db.ChiTietGioHangs.Remove(chiTietGioHang);
                    db.SaveChanges();
                }

                return Json(new { success = true, message = "Đã xóa sản phẩm khỏi giỏ hàng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Cart Count
        // In GioHangsController.cs
        public JsonResult GetCartCount()
        {
            string maKH = Session["MaKH"]?.ToString();
            if (string.IsNullOrEmpty(maKH))
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }

            var cartCount = db.GioHangs
         .Where(g => g.MaKH == maKH)
         .SelectMany(g => g.ChiTietGioHangs) // Use SelectMany to flatten the collection
         .Select(ct => (int?)ct.SoLuong)  // Cast to nullable int
         .DefaultIfEmpty(0) // <--- Add this!  Default to 0 if empty
         .Sum();

            return Json(cartCount, JsonRequestBehavior.AllowGet);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            string maKH = Session["MaKH"]?.ToString();

            if (!string.IsNullOrEmpty(maKH))
            {
                var cartCount = db.GioHangs
        .Where(g => g.MaKH == maKH)
        .SelectMany(g => g.ChiTietGioHangs)
        .Select(ct => (int?)ct.SoLuong)
        .DefaultIfEmpty(0)  // <--- Add here as well
        .Sum();

                ViewBag.CartCount = cartCount;
            }
            else
            {
                ViewBag.CartCount = 0;
            }
        }
    }
}
