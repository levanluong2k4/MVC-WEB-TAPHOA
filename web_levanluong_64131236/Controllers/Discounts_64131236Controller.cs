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
    public class Discounts_64131236Controller : Controller
    {
        private webbanhang64131236Entities1 db = new webbanhang64131236Entities1();

        public ActionResult Index(string searchString)
        {
            var discounts = from d in db.Discounts select d;

            if (!String.IsNullOrEmpty(searchString))
            {
                discounts = discounts.Where(d => d.TenGiamGia.Contains(searchString)
                                            || d.MaGiamGia.Contains(searchString));
            }

            return View(discounts.ToList());
        }

        // GET: Discounts_64131236/Create
        public ActionResult Create()
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }
            return View();
        }

        // POST: Discounts_64131236/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
     
        public ActionResult Create([Bind(Include = "TenGiamGia,LoaiGiamGia,GiaTriGiamGia,NgayBatDau,NgayKetThuc")] Discount discount)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Logic thêm mã giảm giá đã viết sẵn
                    var lastDiscount = db.Discounts.OrderByDescending(x => x.MaGiamGia).FirstOrDefault();
                    string newMaGiamGia = lastDiscount == null ? "GG001" : $"GG{(int.Parse(lastDiscount.MaGiamGia.Substring(2)) + 1):D3}";
                    discount.MaGiamGia = newMaGiamGia;

                    db.Discounts.Add(discount);
                    db.SaveChanges();
                    TempData["Success"] = "Thêm mã giảm giá thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                }
            }

            return View(discount);
        }


        // GET: Discounts_64131236/Edit/5
        public ActionResult Edit(string id)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Discount discount = db.Discounts.Find(id);
            if (discount == null)
            {
                return HttpNotFound();
            }
            return View(discount);
        }

        // POST: Discounts_64131236/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaGiamGia,TenGiamGia,LoaiGiamGia,GiaTriGiamGia,NgayBatDau,NgayKetThuc")] Discount discount)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(discount).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Cập nhật mã giảm giá thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                    return View(discount);
                }
            }
            return View(discount);
        }

        // GET: Discounts_64131236/Delete/5
        public ActionResult Delete(string id)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Discount discount = db.Discounts.Find(id);
            if (discount == null)
            {
                return HttpNotFound();
            }
            return View(discount);
        }

        // POST: Discounts_64131236/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

            try
            {
                Discount discount = db.Discounts.Find(id);
                db.Discounts.Remove(discount);
                db.SaveChanges();
                TempData["Success"] = "Xóa mã giảm giá thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Discounts_64131236/Details/5
        public ActionResult Details(string id)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Discount discount = db.Discounts.Find(id);
            if (discount == null)
            {
                return HttpNotFound();
            }
            return View(discount);
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
