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
    public class LoaiHangs_64131236Controller : Controller
    {
        private webbanhang64131236Entities1 db = new webbanhang64131236Entities1();







        public ActionResult AdminIndex(string searchString)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

            var categories = from c in db.LoaiHangs
                             select c;

            if (!String.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(c => c.TenLH.Contains(searchString)
                                             || c.MaLH.Contains(searchString));
            }

            return View(categories.ToList());
        }

        // GET: Admin Create Category
        public ActionResult AdminCreate()
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }
            return View();
        }

        // POST: Admin Create Category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdminCreate([Bind(Include = "TenLH")] LoaiHang loaiHang, HttpPostedFileBase imageFile)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

            if (ModelState.IsValid)
            {
                // Generate new MaLH
                var lastCategory = db.LoaiHangs
                    .OrderByDescending(x => x.MaLH)
                    .FirstOrDefault();

                string newMaLH;
                if (lastCategory == null)
                {
                    newMaLH = "LH001";
                }
                else
                {
                    // Extract the number from the last MaLH and increment it
                    int lastNumber = int.Parse(lastCategory.MaLH.Substring(2));
                    newMaLH = $"LH{(lastNumber + 1):D3}";
                }

                loaiHang.MaLH = newMaLH;

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(imageFile.FileName);
                    var path = Path.Combine(Server.MapPath("~/img/"), fileName);
                    imageFile.SaveAs(path);
                    loaiHang.AnhLH = fileName;
                }

              
                    db.LoaiHangs.Add(loaiHang);
                    db.SaveChanges();
                    TempData["Success"] = "Thêm danh mục thành công!";
                    return RedirectToAction("AdminIndex");
                

             
            }

            return View(loaiHang);
        }

        // GET: Admin Edit Category
        public ActionResult AdminEdit(string id)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

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

        // POST: Admin Edit Category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdminEdit([Bind(Include = "MaLH,AnhLH,TenLH")] LoaiHang loaiHang, HttpPostedFileBase imageFile)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thông tin loại hàng hiện tại từ database
                    var existingLoaiHang = db.LoaiHangs.Find(loaiHang.MaLH);

                    if (existingLoaiHang == null)
                    {
                        return HttpNotFound();
                    }

                    // Cập nhật tên
                    existingLoaiHang.TenLH = loaiHang.TenLH;

                    // Xử lý upload ảnh mới
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(imageFile.FileName);
                        var path = Path.Combine(Server.MapPath("~/img/"), fileName);
                        imageFile.SaveAs(path);
                        existingLoaiHang.AnhLH = fileName;
                    }
                    else
                    {
                        // Giữ nguyên ảnh cũ nếu không có ảnh mới
                        existingLoaiHang.AnhLH = loaiHang.AnhLH;
                    }

                    db.Entry(existingLoaiHang).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Cập nhật danh mục thành công!";
                    return RedirectToAction("AdminIndex");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật: " + ex.Message;
                    return View(loaiHang);
                }
            }
            return View(loaiHang);
        }

        // GET: Admin Delete Category
        public ActionResult AdminDelete(string id)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

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

        // POST: Admin Delete Category
        // POST: Admin Delete Category
        [HttpPost, ActionName("AdminDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult AdminDeleteConfirmed(string id)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

            try
            {
                LoaiHang loaiHang = db.LoaiHangs.Find(id);
                if (loaiHang == null)
                {
                    return HttpNotFound();
                }

                // Lấy danh sách các sản phẩm thuộc loại hàng
                var relatedProducts = loaiHang.HangHoas.ToList();

                if (relatedProducts.Any())
                {
                    // Xóa tất cả sản phẩm thuộc loại hàng
                    foreach (var product in relatedProducts)
                    {
                        db.HangHoas.Remove(product);
                    }

                    // Lưu thay đổi sau khi xóa sản phẩm
                    db.SaveChanges();
                }

                // Xóa loại hàng
                db.LoaiHangs.Remove(loaiHang);
                db.SaveChanges();

                TempData["Success"] = "Đã xóa danh mục và tất cả sản phẩm liên quan!";
                return RedirectToAction("AdminIndex");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa danh mục: " + ex.Message;
                return RedirectToAction("AdminIndex");
            }
        }
        // GET: Admin View Category Details
        public ActionResult AdminDetails(string id)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

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

        // Keep existing methods...

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