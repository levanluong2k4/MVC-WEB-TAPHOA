using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using web_levanluong_64131236.Models;

namespace web_levanluong_64131236.Controllers
{
    public class TraHangs_64131236Controller : Controller
    {
        private webbanhang64131236Entities1 db = new webbanhang64131236Entities1();

        // GET: TraHangs_64131236
     
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult TaoYeuCauTraHang(int id)
        {
            string maKH = Session["MaKH"]?.ToString();
            if (string.IsNullOrEmpty(maKH))
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

            var donHang = db.DonHangs
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.HangHoa))
                .FirstOrDefault(d => d.MaDH == id && d.MaKH == maKH);

            if (donHang == null || donHang.TrangThaiGiaoHang != "Đã nhận hàng")
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng hoặc đơn hàng không đủ điều kiện để trả hàng";
                return RedirectToAction("SuccessOrders", "DonHangs_64131236");
            }

            var traHang = new TraHang
            {
                MaDH = donHang.MaDH,
                NgayTraHang = DateTime.Now,
                TrangThaiTraHang = "Chờ xác nhận"
            };

            ViewBag.DonHang = donHang;
            return View(traHang);
        }

        public ActionResult TaoYeuCauTraHangmoi(int id)
        {
            string maKH = Session["MaKH"]?.ToString();
            if (string.IsNullOrEmpty(maKH))
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

            // Kiểm tra xem đơn hàng đã có yêu cầu trả hàng chưa
            bool isAlreadyReturned = db.TraHangs.Any(t => t.MaDH == id);
            if (isAlreadyReturned)
            {
                TempData["ErrorMessage"] = "Đơn hàng này đã được yêu cầu trả hàng, không thể tạo thêm yêu cầu.";
                return RedirectToAction("SuccessOrders", "DonHangs_64131236");
            }

            var donHang = db.DonHangs
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.HangHoa))
                .FirstOrDefault(d => d.MaDH == id && d.MaKH == maKH);

            if (donHang == null || donHang.TrangThaiGiaoHang != "Đã nhận hàng")
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng hoặc đơn hàng không đủ điều kiện để trả hàng.";
                return RedirectToAction("SuccessOrders", "DonHangs_64131236");
            }

            var traHang = new TraHang
            {
                MaDH = donHang.MaDH,
                NgayTraHang = DateTime.Now,
                TrangThaiTraHang = "Chờ xác nhận"
            };

            ViewBag.DonHang = donHang;
            return View(traHang);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TaoYeuCauTraHang([Bind(Include = "MaDH,LyDoTraHang,NoiDungTraHang")] TraHang traHang,
      HttpPostedFileBase[] anhTraHang, string[] SelectedProducts, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Set up the TraHang record
                        traHang.NgayTraHang = DateTime.Now;
                        traHang.TrangThaiTraHang = "Chờ xác nhận";

                        db.TraHangs.Add(traHang);
                        db.SaveChanges();

                        // Process selected products and their quantities
                        if (SelectedProducts != null && SelectedProducts.Length > 0)
                        {
                            foreach (var selectedProduct in SelectedProducts)
                            {
                                // Split the composite key
                                var keys = selectedProduct.Split('_');
                                if (keys.Length == 2)
                                {
                                    int maDH = int.Parse(keys[0]);
                                    string maHH = keys[1];

                                    // Get the return quantity from the form
                                    string quantityKey = $"SoLuongTra_{maDH}_{maHH}";
                                    int soLuongTra;
                                    if (int.TryParse(form[quantityKey], out soLuongTra) && soLuongTra > 0)
                                    {
                                        // Get the original order detail
                                        var chiTietDH = db.ChiTietDonHangs
                                            .FirstOrDefault(ct => ct.MaDH == maDH && ct.MaHH == maHH);

                                        if (chiTietDH != null && soLuongTra <= chiTietDH.SoLuong)
                                        {
                                            // Create ChiTietTraHang record
                                            var chiTietTraHang = new ChiTietTraHang
                                            {
                                                MaTraHang = traHang.MaTraHang,
                                                MaHH = maHH,
                                                SoLuongTra = soLuongTra,
                                                LyDoTraChiTiet = traHang.LyDoTraHang
                                            };
                                            db.ChiTietTraHangs.Add(chiTietTraHang);
                                        }
                                    }
                                }
                            }
                            db.SaveChanges();
                        }

                        // Handle image uploads
                        if (anhTraHang != null && anhTraHang.Any(f => f != null))
                        {
                            string uploadPath = Server.MapPath("~/img/");
                            if (!Directory.Exists(uploadPath))
                            {
                                Directory.CreateDirectory(uploadPath);
                            }

                            foreach (var file in anhTraHang.Where(f => f != null))
                            {
                                string fileName = Path.GetFileName(file.FileName);
                                string uniqueFileName = $"{DateTime.Now.Ticks}_{fileName}";
                                string filePath = Path.Combine(uploadPath, uniqueFileName);
                                file.SaveAs(filePath);

                                var anhMoi = new AnhTraHang
                                {
                                    MaTraHang = traHang.MaTraHang,
                                    DuongDanAnh = $"/img/{uniqueFileName}",
                                    NgayThem = DateTime.Now
                                };
                                db.AnhTraHangs.Add(anhMoi);
                            }
                            db.SaveChanges();
                        }

                        transaction.Commit();
                        TempData["SuccessMessage"] = "Yêu cầu trả hàng đã được gửi thành công!";
                        return RedirectToAction("ChiTietTraHang", new { id = traHang.MaTraHang });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "Có lỗi xảy ra khi xử lý yêu cầu trả hàng: " + ex.Message);
                    }
                }
            }

            var donHang = db.DonHangs
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.HangHoa))
                .FirstOrDefault(d => d.MaDH == traHang.MaDH);
            ViewBag.DonHang = donHang;

            return View(traHang);
        }
        // GET: View return request details
        public ActionResult ChiTietTraHang(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TraHang traHang = db.TraHangs
                .Include(t => t.DonHang)
                .Include(t => t.DonHang.KhachHang)
                .Include(t => t.AnhTraHangs)
                .FirstOrDefault(t => t.MaTraHang == id);

            if (traHang == null)
            {
                return HttpNotFound();
            }

            string maKH = Session["MaKH"]?.ToString();
            if (!string.IsNullOrEmpty(maKH) && traHang.DonHang.MaKH != maKH)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            return View(traHang);
        }

        // Admin methods
        [AdminAuthorize]
        public ActionResult QuanLyTraHang(string trangThai)
        {
            var query = db.TraHangs
                .Include(t => t.DonHang)
                .Include(t => t.AnhTraHangs)
                .AsQueryable();

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(t => t.TrangThaiTraHang == trangThai);
            }

            ViewBag.TrangThaiList = new SelectList(
                new[] { "Chờ xác nhận", "Đã xác nhận", "Đã từ chối", "Hoàn thành" },
                trangThai
            );

            return View(query.OrderByDescending(t => t.NgayTraHang).ToList());
        }

        [AdminAuthorize]
        [HttpPost]
        public JsonResult CapNhatTrangThaiTraHang(int id, string trangThai, string ghiChu)
        {
            try
            {
                var traHang = db.TraHangs.Find(id);
                if (traHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy yêu cầu trả hàng" });
                }

                traHang.TrangThaiTraHang = trangThai;
                traHang.GhiChu = ghiChu;
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult TheoDoiTraHang()
        {
            string maKH = Session["MaKH"]?.ToString(); // Lấy mã khách hàng từ session
            if (string.IsNullOrEmpty(maKH))
            {
                return RedirectToAction("Login", "QuanTris_64131236"); // Yêu cầu đăng nhập nếu chưa đăng nhập
            }

            var traHangs = db.TraHangs
                .Include(t => t.DonHang)
                .Where(t => t.DonHang.MaKH == maKH)
                .OrderByDescending(t => t.NgayTraHang)
                .ToList();

            return View(traHangs);
        }





    }
}
