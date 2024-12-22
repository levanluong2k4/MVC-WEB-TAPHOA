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
    public class LoaiHangs_64131236Controller : Controller
    {
        private webbanhang64131236Entities1 db = new webbanhang64131236Entities1();
        

        public ActionResult Index()
        {
            return View(db.LoaiHangs.ToList());
        }

        // GET: LoaiHangs/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LoaiHang loaiHang = db.LoaiHangs.Find(id);
            if (loaiHang == null)
            {
                return HttpNotFound();
            }
            return View(loaiHang);
        }

        // GET: LoaiHangs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LoaiHangs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaLH,AnhLH,TenLH")] LoaiHang loaiHang)
        {
            if (ModelState.IsValid)
            {
                db.LoaiHangs.Add(loaiHang);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(loaiHang);
        }

        // GET: LoaiHangs/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LoaiHang loaiHang = db.LoaiHangs.Find(id);
            if (loaiHang == null)
            {
                return HttpNotFound();
            }
            return View(loaiHang);
        }

        // POST: LoaiHangs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaLH,AnhLH,TenLH")] LoaiHang loaiHang)
        {
            if (ModelState.IsValid)
            {
                db.Entry(loaiHang).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(loaiHang);
        }

        // GET: LoaiHangs/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LoaiHang loaiHang = db.LoaiHangs.Find(id);
            if (loaiHang == null)
            {
                return HttpNotFound();
            }
            return View(loaiHang);
        }

        // POST: LoaiHangs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            LoaiHang loaiHang = db.LoaiHangs.Find(id);
            db.LoaiHangs.Remove(loaiHang);
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

        [CustomAuthorize]
        public ActionResult ShowProducts(string id)
        {
            if (Session["Admin"] != null && (bool)Session["Admin"])
            {
                TempData["Warning"] = "Đây là trang dành cho khách hàng!";
                return RedirectToAction("LoaiHangs_64131236", "LoaiHangs_64131236");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var products = db.HangHoas.Where(p => p.MaLH == id).ToList();
            if (products == null)
            {
                return HttpNotFound();
            }

            // Get the category name and store it
            var category = db.LoaiHangs.Find(id);
            ViewBag.CategoryName = category?.TenLH;
            ViewBag.CurrentCategoryId = id; // Add this to track current category

            return View(products);
        }

        // Update the CategoryMenu action:
        public ActionResult CategoryMenu()
        {
            var categories = db.LoaiHangs.ToList();
            ViewBag.CurrentCategoryId = TempData["CurrentCategoryId"]; // Add this
            return PartialView("_CategoryMenu", categories);
        }

        // Action để hiển thị tất cả danh mục cho người dùng
        [CustomAuthorize]
        public ActionResult Categories()
        {
            if (Session["Admin"] != null && (bool)Session["Admin"])
            {
                TempData["Warning"] = "Đây là trang dành cho khách hàng!";
                return RedirectToAction("Categories", "LoaiHangs_64131236");
            }
            var categories = db.LoaiHangs.ToList();
            return View(categories);
        }
    }
}