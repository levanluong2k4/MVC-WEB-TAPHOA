using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using web_levanluong_64131236.Models;

namespace web_levanluong_64131236.Controllers
{
    public class DanhGiaSanPhams_64131236Controller : Controller
    {
        private webbanhang64131236Entities1 db = new webbanhang64131236Entities1();

        // GET: DanhGiaSanPhams_64131236
        public ActionResult Index()
        {
            var danhGiaSanPhams = db.DanhGiaSanPhams.Include(d => d.ChiTietDonHang).Include(d => d.DonHang).Include(d => d.HangHoa);
            return View(danhGiaSanPhams.ToList());
        }

        // GET: DanhGiaSanPhams_64131236/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DanhGiaSanPham danhGiaSanPham = db.DanhGiaSanPhams.Find(id);
            if (danhGiaSanPham == null)
            {
                return HttpNotFound();
            }
            return View(danhGiaSanPham);
        }

        // GET: DanhGiaSanPhams_64131236/Create
        public ActionResult Create()
        {
            ViewBag.MaDH = new SelectList(db.ChiTietDonHangs, "MaDH", "MaHH");
            ViewBag.MaDH = new SelectList(db.DonHangs, "MaDH", "PhuongThucThanhToan");
            ViewBag.MaHH = new SelectList(db.HangHoas, "MaHH", "MaLH");
            return View();
        }

        // POST: DanhGiaSanPhams_64131236/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create(DanhGiaSanPham model)
        {
            try
            {
                // Kiểm tra xem đã đánh giá chưa
                var existingRating = db.DanhGiaSanPhams.FirstOrDefault(r =>
                    r.MaDH == model.MaDH && r.MaHH == model.MaHH);

                if (existingRating != null)
                {
                    return Json(new { success = false, message = "Bạn đã đánh giá sản phẩm này" });
                }

                // Kiểm tra đơn hàng đã nhận chưa
                var order = db.DonHangs.FirstOrDefault(d =>
                    d.MaDH == model.MaDH && d.TrangThaiGiaoHang == "Đã nhận hàng");

                if (order == null)
                {
                    return Json(new { success = false, message = "Chỉ có thể đánh giá sản phẩm đã nhận" });
                }

                model.NgayDanhGia = DateTime.Now;
                db.DanhGiaSanPhams.Add(model);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // GET: DanhGiaSanPhams_64131236/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DanhGiaSanPham danhGiaSanPham = db.DanhGiaSanPhams.Find(id);
            if (danhGiaSanPham == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaDH = new SelectList(db.ChiTietDonHangs, "MaDH", "MaHH", danhGiaSanPham.MaDH);
            ViewBag.MaDH = new SelectList(db.DonHangs, "MaDH", "PhuongThucThanhToan", danhGiaSanPham.MaDH);
            ViewBag.MaHH = new SelectList(db.HangHoas, "MaHH", "MaLH", danhGiaSanPham.MaHH);
            return View(danhGiaSanPham);
        }

        // POST: DanhGiaSanPhams_64131236/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaDanhGia,MaDH,MaHH,SoSao,NoiDungDanhGia,NgayDanhGia")] DanhGiaSanPham danhGiaSanPham)
        {
            if (ModelState.IsValid)
            {
                db.Entry(danhGiaSanPham).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaDH = new SelectList(db.ChiTietDonHangs, "MaDH", "MaHH", danhGiaSanPham.MaDH);
            ViewBag.MaDH = new SelectList(db.DonHangs, "MaDH", "PhuongThucThanhToan", danhGiaSanPham.MaDH);
            ViewBag.MaHH = new SelectList(db.HangHoas, "MaHH", "MaLH", danhGiaSanPham.MaHH);
            return View(danhGiaSanPham);
        }

        // GET: DanhGiaSanPhams_64131236/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DanhGiaSanPham danhGiaSanPham = db.DanhGiaSanPhams.Find(id);
            if (danhGiaSanPham == null)
            {
                return HttpNotFound();
            }
            return View(danhGiaSanPham);
        }

        // POST: DanhGiaSanPhams_64131236/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DanhGiaSanPham danhGiaSanPham = db.DanhGiaSanPhams.Find(id);
            db.DanhGiaSanPhams.Remove(danhGiaSanPham);
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
    }
}
