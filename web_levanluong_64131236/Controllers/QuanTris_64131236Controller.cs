using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using web_levanluong_64131236.Models;



namespace web_levanluong_64131236.Controllers
{
    public class QuanTris_64131236Controller : Controller
    {
        private webbanhang64131236Entities1 db = new webbanhang64131236Entities1();

        // GET: QuanTris_64131236
        public ActionResult Index()
        {
            return View(db.QuanTris.ToList());
        }

        // GET: QuanTris_64131236/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuanTri quanTri = db.QuanTris.Find(id);
            if (quanTri == null)
            {
                return HttpNotFound();
            }
            return View(quanTri);
        }

        // GET: QuanTris_64131236/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: QuanTris_64131236/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "username,Password,Admin")] QuanTri quanTri)
        {
            if (ModelState.IsValid)
            {
                db.QuanTris.Add(quanTri);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(quanTri);
        }

        // GET: QuanTris_64131236/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuanTri quanTri = db.QuanTris.Find(id);
            if (quanTri == null)
            {
                return HttpNotFound();
            }
            return View(quanTri);
        }

        // POST: QuanTris_64131236/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "username,Password,Admin")] QuanTri quanTri)
        {
            if (ModelState.IsValid)
            {
                db.Entry(quanTri).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(quanTri);
        }

        // GET: QuanTris_64131236/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuanTri quanTri = db.QuanTris.Find(id);
            if (quanTri == null)
            {
                return HttpNotFound();
            }
            return View(quanTri);
        }

        // POST: QuanTris_64131236/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            QuanTri quanTri = db.QuanTris.Find(id);
            db.QuanTris.Remove(quanTri);
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




        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(QuanTri qt)
        {
            var user = db.QuanTris.FirstOrDefault(u => u.username == qt.username && u.Password == qt.Password);
            if (user != null)
            {
                Session["username"] = user.username;
                Session["Admin"] = user.Admin;

                // Nếu không phải admin, lấy MaKH từ bảng KhachHang
                if (user.Admin != true)
                {
                    var khachHang = db.KhachHangs.FirstOrDefault(k => k.username == user.username);
                    if (khachHang != null)
                    {
                        Session["MaKH"] = khachHang.MaKH;
                    }
                }

                TempData["SuccessMessage"] = "Đăng nhập thành công!";

                if (user.Admin == true)
                {
                    return RedirectToAction("Index", "QuanTris_64131236");
                }
                else
                {
                    return RedirectToAction("Categories", "LoaiHangs_64131236");
                }
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng";
            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register([Bind(Include = "username,Password")] QuanTri qt)
        {
            if (ModelState.IsValid)
            {
                var exists = db.QuanTris.Any(x => x.username == qt.username);
                if (exists)
                {
                    ViewBag.Error = "Username already exists";
                    return View();
                }

                qt.Admin = false;
                db.QuanTris.Add(qt);
                db.SaveChanges();

                // Store username in TempData to use it in customer creation
                TempData["NewUsername"] = qt.username;

                // Redirect to customer information form
                return RedirectToAction("CreateCustomerInfo", "KhachHangs_64131236");
            }
            return View(qt);
        }
    








    public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
        public ActionResult AccessDenied()
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }


        // GET: QuanTris_64131236/ChangePassword
        public ActionResult ChangePassword()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel2 model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string username = Session["username"] as string;
            var user = db.QuanTris.FirstOrDefault(u => u.username == username);

            if (user == null || user.Password != model.CurrentPassword)
            {
                ModelState.AddModelError("", "Mật khẩu hiện tại không đúng");
                return View(model);
            }

            user.Password = model.NewPassword;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";

            // Chuyển hướng dựa vào loại người dùng
            if (user.Admin == true)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("MyProfile", "KhachHangs_64131236");
            }
        }
        // xử lý quên mật khẩu
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
    
        public ActionResult ForgotPassword(ForgotPasswordViewModel2 model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var khachHang = db.KhachHangs.FirstOrDefault(k => k.Email == model.Email);
                if (khachHang == null)
                {
                    ModelState.AddModelError("", "Email không tồn tại trong hệ thống");
                    return View(model);
                }

                var quanTri = db.QuanTris.Find(khachHang.username);
                if (quanTri == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy tài khoản liên kết");
                    return View(model);
                }

                string newPassword = GenerateRandomPassword();
                quanTri.Password = newPassword;
                db.SaveChanges();

                try
                {
                    var mail = new System.Net.Mail.MailMessage();
                    mail.From = new System.Net.Mail.MailAddress("levanluong18t@gmail.com");
                    mail.To.Add(khachHang.Email);
                    mail.Subject = "Đặt lại mật khẩu";
                    mail.Body = $"Xin chào {khachHang.TenKH},<br/>Mật khẩu mới của bạn là: {newPassword}";
                    mail.IsBodyHtml = true;

                    var smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587);
                    smtp.Credentials = new System.Net.NetworkCredential("levanluong18t@gmail.com", "uoemvardtldkpuxe");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
                catch (Exception ex)
                {
                    // Log chi tiết lỗi
                    System.Diagnostics.Debug.WriteLine("Mail error: " + ex.Message);
                    throw;
                }

                TempData["SuccessMessage"] = "Mật khẩu mới đã được gửi đến email của bạn";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi gửi email: " + ex.Message);
                return View(model);
            }
        }

        private string GenerateRandomPassword()
        {
            // Tạo mật khẩu ngẫu nhiên 8 ký tự
            const string chars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*?";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void SendPasswordResetEmail(string email, string customerName, string newPassword)
        {
            var fromAddress = new MailAddress("levanluong18t@gmail.com@gmail.com", "uoemvardtldkpuxe");
            var toAddress = new MailAddress(email);
            const string subject = "Đặt lại mật khẩu";
            string body = $@"
        <html>
        <body>
            <h2>Xin chào {customerName},</h2>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu của bạn.</p>
            <p>Mật khẩu mới của bạn là: <strong>{newPassword}</strong></p>
            <p>Vui lòng đăng nhập và đổi mật khẩu ngay để đảm bảo an toàn.</p>
            <p>Trân trọng,<br>Your Store Name</p>
        </body>
        </html>";

            using (var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("levanluong18t@gmail.com", "uoemvardtldkpuxe")
            })
            {
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
            }
        }

    }


}
