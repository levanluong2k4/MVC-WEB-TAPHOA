using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using web_levanluong_64131236.Models;

namespace web_levanluong_64131236.Controllers
{
    public class ChiTietDonHangs_64131236Controller : Controller
    {
        private webbanhang64131236Entities1 db = new webbanhang64131236Entities1();

        // GET: ChiTietDonHangs_64131236
        public ActionResult Index()
        {
            var chiTietDonHangs = db.ChiTietDonHangs.Include(c => c.DonHang).Include(c => c.HangHoa);
            return View(chiTietDonHangs.ToList());
        }

        // GET: ChiTietDonHangs_64131236/Details/5
        // Thêm các action sau vào controller
        public ActionResult Details(int? maDH, string maHH)
        {
            if (maDH == null || string.IsNullOrEmpty(maHH))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var chiTietDonHang = db.ChiTietDonHangs
                .Include(c => c.HangHoa)
                .Include(c => c.DonHang)
                .FirstOrDefault(c => c.MaDH == maDH && c.MaHH == maHH);

            if (chiTietDonHang == null)
            {
                return HttpNotFound();
            }

            return View(chiTietDonHang);
        }

        // GET: ChiTietDonHangs_64131236/Create
        public ActionResult Create()
        {
            ViewBag.MaDH = new SelectList(db.DonHangs, "MaDH", "PhuongThucThanhToan");
            ViewBag.MaHH = new SelectList(db.HangHoas, "MaHH", "MaLH");
            return View();
        }

        // POST: ChiTietDonHangs_64131236/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaDH,MaHH,SoLuong,DonGia,ThanhTien")] ChiTietDonHang chiTietDonHang)
        {
            if (ModelState.IsValid)
            {
                db.ChiTietDonHangs.Add(chiTietDonHang);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MaDH = new SelectList(db.DonHangs, "MaDH", "PhuongThucThanhToan", chiTietDonHang.MaDH);
            ViewBag.MaHH = new SelectList(db.HangHoas, "MaHH", "MaLH", chiTietDonHang.MaHH);
            return View(chiTietDonHang);
        }

        // GET: ChiTietDonHangs_64131236/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChiTietDonHang chiTietDonHang = db.ChiTietDonHangs.Find(id);
            if (chiTietDonHang == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaDH = new SelectList(db.DonHangs, "MaDH", "PhuongThucThanhToan", chiTietDonHang.MaDH);
            ViewBag.MaHH = new SelectList(db.HangHoas, "MaHH", "MaLH", chiTietDonHang.MaHH);
            return View(chiTietDonHang);
        }

        // POST: ChiTietDonHangs_64131236/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaDH,MaHH,SoLuong,DonGia,ThanhTien")] ChiTietDonHang chiTietDonHang)
        {
            if (ModelState.IsValid)
            {
                db.Entry(chiTietDonHang).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaDH = new SelectList(db.DonHangs, "MaDH", "PhuongThucThanhToan", chiTietDonHang.MaDH);
            ViewBag.MaHH = new SelectList(db.HangHoas, "MaHH", "MaLH", chiTietDonHang.MaHH);
            return View(chiTietDonHang);
        }

        // GET: ChiTietDonHangs_64131236/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChiTietDonHang chiTietDonHang = db.ChiTietDonHangs.Find(id);
            if (chiTietDonHang == null)
            {
                return HttpNotFound();
            }
            return View(chiTietDonHang);
        }

        // POST: ChiTietDonHangs_64131236/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ChiTietDonHang chiTietDonHang = db.ChiTietDonHangs.Find(id);
            db.ChiTietDonHangs.Remove(chiTietDonHang);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult CreateFromCart(int maDH)
        {
            string maKH = Session["MaKH"]?.ToString();
            if (string.IsNullOrEmpty(maKH))
            {
                return RedirectToAction("Login", "QuanTris");
            }

            var gioHang = db.GioHangs
                .Include(g => g.ChiTietGioHangs.Select(ct => ct.HangHoa))
                .FirstOrDefault(g => g.MaKH == maKH);

            if (gioHang == null || !gioHang.ChiTietGioHangs.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống hoặc không tồn tại";
                return RedirectToAction("Index", "GioHangs_64131236");
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Verify the order exists
                    var donHang = db.DonHangs.Find(maDH);
                    if (donHang == null)
                    {
                        throw new Exception("Không tìm thấy đơn hàng");
                    }

                    // Create order details
                    foreach (var cartItem in gioHang.ChiTietGioHangs.ToList())
                    {
                        var orderDetail = new ChiTietDonHang
                        {
                            MaDH = maDH,
                            MaHH = cartItem.MaHH,
                            SoLuong = cartItem.SoLuong,
                            DonGia = cartItem.HangHoa.GiaBan,
                            ThanhTien = cartItem.SoLuong * cartItem.HangHoa.GiaBan
                        };
                        db.ChiTietDonHangs.Add(orderDetail);
                    }
                    db.SaveChanges();

                    // Clear cart after successful order creation
                    db.ChiTietGioHangs.RemoveRange(gioHang.ChiTietGioHangs);
                    db.GioHangs.Remove(gioHang);
                    db.SaveChanges();

                    transaction.Commit();
                    return RedirectToAction("OrderConfirmation", "DonHangs_64131236", new { id = maDH });
                }
                catch (DbUpdateException ex)
                {
                    transaction.Rollback();
                    // Log the error details
                    var innerException = ex.InnerException?.InnerException as System.Data.SqlClient.SqlException;
                    TempData["ErrorMessage"] = innerException?.Message ?? "Lỗi cập nhật dữ liệu";
                    return RedirectToAction("Index", "GioHangs_64131236");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["ErrorMessage"] = $"Lỗi xử lý đơn hàng: {ex.Message}";
                    return RedirectToAction("Index", "GioHangs_64131236");
                }
            }
        }
    }
}
