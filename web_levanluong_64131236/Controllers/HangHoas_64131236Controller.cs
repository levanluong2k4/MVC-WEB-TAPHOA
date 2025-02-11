using PagedList;
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
    decimal? minPrice, decimal? maxPrice, DateTime? fromDate, DateTime? toDate, string filterType, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.PriceSortParm = sortOrder == "Price" ? "price_desc" : "Price";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.CurrentFilter = filterType;

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var hangHoas = db.HangHoas.Include(h => h.LoaiHang);

            // Calculate sales data for bestsellers
            var salesData = db.ChiTietDonHangs
                .GroupBy(x => x.MaHH)
                .Select(g => new
                {
                    MaHH = g.Key,
                    TotalQuantity = g.Sum(x => x.SoLuong)
                })
                .ToDictionary(x => x.MaHH, x => x.TotalQuantity);

            // Apply existing filters
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

            // Get products list
            var products = hangHoas.ToList();

            // Apply special filters
            switch (filterType)
            {
                case "nearExpiry":
                    DateTime warningDate = DateTime.Now.AddDays(15);
                    products = products.Where(p => p.HSD <= warningDate && p.HSD > DateTime.Now)
                                     .OrderBy(p => p.HSD).ToList();
                    break;

                case "expired":
                    products = products.Where(p => p.HSD < DateTime.Now)
                                     .OrderBy(p => p.HSD).ToList();
                    break;

                case "bestsellers":
                    products = products
                        .Where(p => salesData.ContainsKey(p.MaHH))
                        .OrderByDescending(p => salesData[p.MaHH])
                        .ToList();
                    break;

                default:
                    switch (sortOrder)
                    {
                        case "name_desc":
                            products = products.OrderByDescending(s => s.TenHH).ToList();
                            break;
                        case "Price":
                            products = products.OrderBy(s => s.GiaBan).ToList();
                            break;
                        case "price_desc":
                            products = products.OrderByDescending(s => s.GiaBan).ToList();
                            break;
                        case "Date":
                            products = products.OrderBy(s => s.NSX).ToList();
                            break;
                        case "date_desc":
                            products = products.OrderByDescending(s => s.NSX).ToList();
                            break;
                        default:
                            products = products.OrderBy(s => s.TenHH).ToList();
                            break;
                    }
                    break;
            }

            ViewBag.SalesData = salesData;
            ViewBag.ExpiredCount = hangHoas.Count(p => p.HSD < DateTime.Now);
            ViewBag.LoaiHangs = new SelectList(db.LoaiHangs, "MaLH", "TenLH");

            // Pagination
            int pageSize = 8; // Number of items per page
            int pageNumber = (page ?? 1);

            return View(products.ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
       
        public ActionResult DeleteExpired()
        {
            try
            {
                var expiredProducts = db.HangHoas.Where(p => p.HSD < DateTime.Now).ToList();
                foreach (var product in expiredProducts)
                {
                    // Delete related order details first
                    var relatedOrders = db.ChiTietDonHangs.Where(ct => ct.MaHH == product.MaHH);
                    db.ChiTietDonHangs.RemoveRange(relatedOrders);
                }
                // Then delete the expired products
                db.HangHoas.RemoveRange(expiredProducts);
                db.SaveChanges();
                TempData["SuccessMessage"] = $"Đã xóa {expiredProducts.Count} sản phẩm hết hạn!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa sản phẩm hết hạn: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
        private string GenerateNewProductCode()
        {
            var lastProduct = db.HangHoas
                .OrderByDescending(p => p.MaHH)
                .FirstOrDefault();

            if (lastProduct == null)
            {
                return "HH001";
            }

            if (lastProduct.MaHH.Length >= 5)
            {
                string numericPart = lastProduct.MaHH.Substring(2);
                if (int.TryParse(numericPart, out int currentNumber))
                {
                    return $"HH{(currentNumber + 1):D3}";
                }
            }

            return $"HH{DateTime.Now.Ticks % 1000:D3}";
        }

        // GET: HangHoas_64131236/Create
        [AdminAuthorize]
        [HttpGet]
        public ActionResult Create()
        {
            string newProductCode = GenerateNewProductCode();
            var hangHoa = new HangHoa { MaHH = newProductCode };

            ViewBag.NewProductCode = newProductCode;
            ViewBag.MaLH = new SelectList(db.LoaiHangs, "MaLH", "TenLH");
            return View(hangHoa);
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

                try
                {
                    db.HangHoas.Add(hangHoa);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
                }
            }

            ViewBag.MaLH = new SelectList(db.LoaiHangs, "MaLH", "TenLH", hangHoa.MaLH);
            ViewBag.NewProductCode = hangHoa.MaHH;
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
        public ActionResult Edit([Bind(Include = "MaHH,MaLH,TenHH,AnhHH,SoLuongHangTon,GiaBan,HSD,NSX")] HangHoa hangHoa,
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

                db.Entry(hangHoa).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaLH = new SelectList(db.LoaiHangs, "MaLH", "TenLH", hangHoa.MaLH);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                HangHoa hangHoa = db.HangHoas.Find(id);
                if (hangHoa == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });
                }

                // First delete related records in ChiTietDonHang
                var relatedOrders = db.ChiTietDonHangs.Where(ct => ct.MaHH == id);
                db.ChiTietDonHangs.RemoveRange(relatedOrders);

                // Then delete the product
                db.HangHoas.Remove(hangHoa);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa sản phẩm: " + ex.Message });
            }
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
        decimal? minPrice, decimal? maxPrice, string filterType, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.PriceSortParm = sortOrder == "Price" ? "price_desc" : "Price";
            ViewBag.SalesSortParm = sortOrder == "Sales" ? "sales_desc" : "Sales";
            ViewBag.CurrentFilter = filterType;

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            var hangHoas = db.HangHoas.Include(h => h.LoaiHang);

            // Calculate sales data for all products
            var salesData = db.ChiTietDonHangs
                .GroupBy(x => x.MaHH)
                .Select(g => new
                {
                    MaHH = g.Key,
                    TotalQuantity = g.Sum(x => x.SoLuong),
                    TotalRevenue = g.Sum(x => x.SoLuong * x.DonGia)
                })
                .ToDictionary(x => x.MaHH, x => x);

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

            // Get products list
            var products = hangHoas.ToList();

            // Apply best sellers filter if selected
            if (filterType == "bestsellers")
            {
                products = products
                    .Where(p => salesData.ContainsKey(p.MaHH))
                    .OrderByDescending(p => salesData[p.MaHH].TotalQuantity)
                    .ToList();
            }
            else
            {
                // Apply regular sorting
                switch (sortOrder)
                {
                    case "name_desc":
                        products = products.OrderByDescending(s => s.TenHH).ToList();
                        break;
                    case "Price":
                        products = products.OrderBy(s => s.GiaBan).ToList();
                        break;
                    case "price_desc":
                        products = products.OrderByDescending(s => s.GiaBan).ToList();
                        break;
                    case "Sales":
                        products = products
                            .OrderBy(p => salesData.ContainsKey(p.MaHH) ? salesData[p.MaHH].TotalQuantity : 0)
                            .ToList();
                        break;
                    case "sales_desc":
                        products = products
                            .OrderByDescending(p => salesData.ContainsKey(p.MaHH) ? salesData[p.MaHH].TotalQuantity : 0)
                            .ToList();
                        break;
                    default:
                        products = products.OrderBy(s => s.TenHH).ToList();
                        break;
                }
            }

            // Calculate sold quantities for display
            ViewBag.SoldQuantities = products.ToDictionary(
                p => p.MaHH,
                p => salesData.ContainsKey(p.MaHH) ? salesData[p.MaHH].TotalQuantity : 0
            );

            int pageSize = 8;
            int pageNumber = (page ?? 1);

            ViewBag.LoaiHangs = new SelectList(db.LoaiHangs, "MaLH", "TenLH");
            return View(products.ToPagedList(pageNumber, pageSize));
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

            ViewBag.LoaiHangs = new SelectList(db.LoaiHangs, "MaLH", "TenLH");
            ViewBag.SearchTerm = searchTerm; // Add this to preserve the search term
            return View("ProductList", query.ToList());
        }

        // Add this to HangHoas_64131236Controller
        public JsonResult GetSearchSuggestions(string term)
        {
            var suggestions = db.HangHoas
                .Where(h => h.TenHH.Contains(term) || h.MaHH.Contains(term))
                .Select(h => new
                {
                    label = h.TenHH,
                    value = h.TenHH,
                    id = h.MaHH,
                    image = h.AnhHH,
                    price = h.GiaBan
                })
                .Take(5)
                .ToList();

            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }


    }

 

}
