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
    public class KhachHangs_64131236Controller : Controller
    {
        private webbanhang64131236Entities1 db = new webbanhang64131236Entities1();

        // GET: KhachHangs_64131236
        public ActionResult Index()
        {
            var khachHangs = db.KhachHangs.Include(k => k.QuanTri);
            return View(khachHangs.ToList());
        }

        // GET: KhachHangs_64131236/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            return View(khachHang);
        }

        // GET: KhachHangs_64131236/Create
        public ActionResult Create()
        {
            ViewBag.username = new SelectList(db.QuanTris, "username", "Password");
            return View();
        }

        // POST: KhachHangs_64131236/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaKH,TenKH,DiaChi,Email,username,SDT_KH")] KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                db.KhachHangs.Add(khachHang);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.username = new SelectList(db.QuanTris, "username", "Password", khachHang.username);
            return View(khachHang);
        }

        // GET: KhachHangs_64131236/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            ViewBag.username = new SelectList(db.QuanTris, "username", "Password", khachHang.username);
            return View(khachHang);
        }

        // POST: KhachHangs_64131236/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaKH,TenKH,DiaChi,Email,username,SDT_KH")] KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                db.Entry(khachHang).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.username = new SelectList(db.QuanTris, "username", "Password", khachHang.username);
            return View(khachHang);
        }

        // GET: KhachHangs_64131236/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            return View(khachHang);
        }

        // POST: KhachHangs_64131236/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            KhachHang khachHang = db.KhachHangs.Find(id);
            db.KhachHangs.Remove(khachHang);
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

        public ActionResult CreateCustomerInfo()
        {
            // Check if we have a new username from registration
            string username = TempData["NewUsername"] as string;
            if (username == null)
            {
                return RedirectToAction("Register", "QuanTris_64131236");
            }

            // Create a new customer with pre-filled username
            var customer = new KhachHang
            {
                username = username,
                MaKH = GenerateCustomerId()
            };

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCustomerInfo([Bind(Include = "TenKH,DiaChi,Email,SDT_KH")] KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                // Get username from hidden field
                khachHang.username = Request.Form["username"];
                khachHang.MaKH = GenerateCustomerId();

                db.KhachHangs.Add(khachHang);
                db.SaveChanges();

                // Redirect to login page
                return RedirectToAction("Login", "QuanTris_64131236");
            }

            return View(khachHang);
        }

        private string GenerateCustomerId()
        {
            // Get the current highest customer ID
            var lastCustomer = db.KhachHangs
                .OrderByDescending(k => k.MaKH)
                .FirstOrDefault();

            if (lastCustomer == null)
            {
                return "KH001";
            }

            // Extract the number from the last ID and increment it
            string currentNum = lastCustomer.MaKH.Substring(2);
            int nextNum = int.Parse(currentNum) + 1;
            return $"KH{nextNum:D3}";
        }


        // sửa thông tin khách hàng 
        // GET: KhachHangs_64131236/MyProfile
        public ActionResult MyProfile()
        {
            // Lấy username từ Session
            string username = Session["username"] as string;
            if (username == null)
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

            // Kiểm tra xem có phải là admin không
            bool? isAdmin = Session["Admin"] as bool?;
            if (isAdmin == true)
            {
                return RedirectToAction("AccessDenied", "QuanTris_64131236");
            }

            // Tìm thông tin khách hàng dựa trên username
            var khachHang = db.KhachHangs
                .Include(k => k.QuanTri)
                .FirstOrDefault(k => k.username == username);

            if (khachHang == null)
            {
                return HttpNotFound();
            }

            return View(khachHang);
        }

        // GET: KhachHangs_64131236/EditMyProfile
        public ActionResult EditMyProfile()
        {
            string username = Session["username"] as string;
            if (username == null)
            {
                return RedirectToAction("Login", "QuanTris_64131236");
            }

            var khachHang = db.KhachHangs.FirstOrDefault(k => k.username == username);
            if (khachHang == null)
            {
                return HttpNotFound();
            }

            return View(khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMyProfile([Bind(Include = "MaKH,TenKH,DiaChi,Email,username,SDT_KH")] KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                db.Entry(khachHang).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("MyProfile");
            }
            return View(khachHang);
        }
    }
}
