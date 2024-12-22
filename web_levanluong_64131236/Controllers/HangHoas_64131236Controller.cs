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
    public class HangHoas_64131236Controller : Controller
    {
        private webbanhang64131236Entities1 db = new webbanhang64131236Entities1();

        // GET: HangHoas_64131236
        [AdminAuthorize]
        public ActionResult Index(string searchString, string currentFilter, string sortOrder, string maLH,
        decimal? minPrice, decimal? maxPrice, DateTime? fromDate, DateTime? toDate)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.PriceSortParm = sortOrder == "Price" ? "price_desc" : "Price";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                ViewBag.CurrentFilter = searchString;
            }
            else
            {
                searchString = currentFilter;
            }

            var hangHoas = db.HangHoas.Include(h => h.LoaiHang);

            // Apply filters
            if (!String.IsNullOrEmpty(searchString))
            {
                hangHoas = hangHoas.Where(s => s.TenHH.Contains(searchString) || s.MaHH.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(maLH))
            {
                hangHoas = hangHoas.Where(s => s.MaLH == maLH);
            }

            if (minPrice.HasValue)
            {
                hangHoas = hangHoas.Where(s => s.GiaBan >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                hangHoas = hangHoas.Where(s => s.GiaBan <= maxPrice.Value);
            }

            if (fromDate.HasValue)
            {
                hangHoas = hangHoas.Where(s => s.NSX >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                hangHoas = hangHoas.Where(s => s.NSX <= toDate.Value);
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "name_desc":
                    hangHoas = hangHoas.OrderByDescending(s => s.TenHH);
                    break;
                case "Price":
                    hangHoas = hangHoas.OrderBy(s => s.GiaBan);
                    break;
                case "price_desc":
                    hangHoas = hangHoas.OrderByDescending(s => s.GiaBan);
                    break;
                case "Date":
                    hangHoas = hangHoas.OrderBy(s => s.NSX);
                    break;
                case "date_desc":
                    hangHoas = hangHoas.OrderByDescending(s => s.NSX);
                    break;
                default:
                    hangHoas = hangHoas.OrderBy(s => s.TenHH);
                    break;
            }

            ViewBag.LoaiHangs = new SelectList(db.LoaiHangs, "MaLH", "TenLH");
            return View(hangHoas.ToList());
        }

        // GET: HangHoas_64131236/Create
        [AdminAuthorize]
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.MaLH = new SelectList(db.LoaiHangs, "MaLH", "TenLH");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaHH,MaLH,TenHH,AnhHH,SoLuongHangTon,GiaBan,HSD,NSX")] HangHoa hangHoa,
            HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(imageFile.FileName);
                    var path = Path.Combine(Server.MapPath("~/img/"), fileName);
                    imageFile.SaveAs(path);
                    hangHoa.AnhHH = fileName;
                }

                db.HangHoas.Add(hangHoa);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MaLH = new SelectList(db.LoaiHangs, "MaLH", "TenLH", hangHoa.MaLH);
            return View(hangHoa);
        }

        // GET: HangHoas_64131236/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HangHoa hangHoa = db.HangHoas.Find(id);
            if (hangHoa == null)
            {
                return HttpNotFound();
            }
            return View(hangHoa);
        }

      

        // GET: HangHoas_64131236/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HangHoa hangHoa = db.HangHoas.Find(id);
            if (hangHoa == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaLH = new SelectList(db.LoaiHangs, "MaLH", "AnhLH", hangHoa.MaLH);
            return View(hangHoa);
        }

        // POST: HangHoas_64131236/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaHH,MaLH,TenHH,AnhHH,SoLuongHangTon,GiaBan,HSD,NSX")] HangHoa hangHoa)
        {
            if (ModelState.IsValid)
            {
                db.Entry(hangHoa).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaLH = new SelectList(db.LoaiHangs, "MaLH", "AnhLH", hangHoa.MaLH);
            return View(hangHoa);
        }

        // GET: HangHoas_64131236/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HangHoa hangHoa = db.HangHoas.Find(id);
            if (hangHoa == null)
            {
                return HttpNotFound();
            }
            return View(hangHoa);
        }

        // POST: HangHoas_64131236/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            HangHoa hangHoa = db.HangHoas.Find(id);
            db.HangHoas.Remove(hangHoa);
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


        // khách hàng 
        public ActionResult ProductList(string searchString, string currentFilter, string sortOrder, string maLH,
    decimal? minPrice, decimal? maxPrice, DateTime? fromDate, DateTime? toDate)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.PriceSortParm = sortOrder == "Price" ? "price_desc" : "Price";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                ViewBag.CurrentFilter = searchString;
            }
            else
            {
                searchString = currentFilter;
            }

            var hangHoas = db.HangHoas.Include(h => h.LoaiHang);

            // Apply filters
            if (!String.IsNullOrEmpty(searchString))
            {
                hangHoas = hangHoas.Where(s => s.TenHH.Contains(searchString) || s.MaHH.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(maLH))
            {
                hangHoas = hangHoas.Where(s => s.MaLH == maLH);
            }

            if (minPrice.HasValue)
            {
                hangHoas = hangHoas.Where(s => s.GiaBan >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                hangHoas = hangHoas.Where(s => s.GiaBan <= maxPrice.Value);
            }

            if (fromDate.HasValue)
            {
                hangHoas = hangHoas.Where(s => s.NSX >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                hangHoas = hangHoas.Where(s => s.NSX <= toDate.Value);
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "name_desc":
                    hangHoas = hangHoas.OrderByDescending(s => s.TenHH);
                    break;
                case "Price":
                    hangHoas = hangHoas.OrderBy(s => s.GiaBan);
                    break;
                case "price_desc":
                    hangHoas = hangHoas.OrderByDescending(s => s.GiaBan);
                    break;
                case "Date":
                    hangHoas = hangHoas.OrderBy(s => s.NSX);
                    break;
                case "date_desc":
                    hangHoas = hangHoas.OrderByDescending(s => s.NSX);
                    break;
                default:
                    hangHoas = hangHoas.OrderBy(s => s.TenHH);
                    break;
            }

            ViewBag.LoaiHangs = new SelectList(db.LoaiHangs, "MaLH", "TenLH");
            return View(hangHoas.ToList());
        }


        public ActionResult Search(string searchTerm)
        {
            var query = db.HangHoas.Include(h => h.LoaiHang).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.TenHH.Contains(searchTerm)
                                     || p.MaHH.Contains(searchTerm)
                                     || p.LoaiHang.TenLH.Contains(searchTerm));
            }

            return View("ProductList", query.ToList());
        }
    }
}
