using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using web_levanluong_64131236.Models;

namespace web_levanluong_64131236.Controllers
{
    public class NhanViens_64131236Controller : Controller
    {
        private readonly webbanhang64131236Entities1 db = new webbanhang64131236Entities1();

        // GET: NhanViens_64131236
        public ActionResult Index(string searchString, string searchBy, string sortOrder)
        {
            try
            {
                ViewBag.CurrentSort = sortOrder;
                ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
                ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

                var nhanViens = db.NhanViens.Include(n => n.QuanTri);

                // Search functionality
                if (!String.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.Trim().ToLower();
                    switch (searchBy)
                    {
                        case "MaNV":
                            nhanViens = nhanViens.Where(n => n.MaNV.ToLower().Contains(searchString));
                            break;
                        case "Ten":
                            nhanViens = nhanViens.Where(n => n.Ten.ToLower().Contains(searchString)
                                                        || n.Ho.ToLower().Contains(searchString));
                            break;
                        case "DiaChi":
                            nhanViens = nhanViens.Where(n => n.DiaChi.ToLower().Contains(searchString));
                            break;
                        case "DienThoai":
                            nhanViens = nhanViens.Where(n => n.DienThoai.Contains(searchString));
                            break;
                        default:
                            nhanViens = nhanViens.Where(n => n.MaNV.ToLower().Contains(searchString)
                                                        || n.Ten.ToLower().Contains(searchString)
                                                        || n.Ho.ToLower().Contains(searchString)
                                                        || n.DiaChi.ToLower().Contains(searchString)
                                                        || n.DienThoai.Contains(searchString));
                            break;
                    }
                }

                // Sorting
                switch (sortOrder)
                {
                    case "name_desc":
                        nhanViens = nhanViens.OrderByDescending(n => n.Ten);
                        break;
                    case "Date":
                        nhanViens = nhanViens.OrderBy(n => n.NgayLamViec);
                        break;
                    case "date_desc":
                        nhanViens = nhanViens.OrderByDescending(n => n.NgayLamViec);
                        break;
                    default:
                        nhanViens = nhanViens.OrderBy(n => n.Ten);
                        break;
                }

                ViewBag.SearchBy = new SelectList(new[]
                {
                    new { Value = "MaNV", Text = "Mã nhân viên" },
                    new { Value = "Ten", Text = "Họ tên" },
                    new { Value = "DiaChi", Text = "Địa chỉ" },
                    new { Value = "DienThoai", Text = "Điện thoại" }
                }, "Value", "Text");

                return View(nhanViens.ToList());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách nhân viên: " + ex.Message;
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: NhanViens_64131236/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien == null)
            {
                return HttpNotFound();
            }

            return View(nhanVien);
        }

        // GET: NhanViens_64131236/Create
        public ActionResult Create()
        {
            try
            {
                string newEmployeeId = GenerateEmployeeId();
                var nhanVien = new NhanVien
                {
                    MaNV = newEmployeeId,
                    NgayLamViec = DateTime.Now.Date
                };

                ViewBag.username = new SelectList(db.QuanTris, "username", "username");
                return View(nhanVien);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo nhân viên mới: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaNV,username,Ho,Ten,NgaySinh,NgayLamViec,DiaChi,DienThoai")] NhanVien nhanVien)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (!ValidateEmployee(nhanVien))
                    {
                        ViewBag.username = new SelectList(db.QuanTris, "username", "username", nhanVien.username);
                        return View(nhanVien);
                    }

                    db.NhanViens.Add(nhanVien);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
            }

            ViewBag.username = new SelectList(db.QuanTris, "username", "username", nhanVien.username);
            return View(nhanVien);
        }

        // GET: NhanViens_64131236/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            NhanVien nhanVien = db.NhanViens.Find(id);
            if (nhanVien == null)
            {
                return HttpNotFound();
            }

            ViewBag.username = new SelectList(db.QuanTris, "username", "username", nhanVien.username);
            return View(nhanVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaNV,username,Ho,Ten,NgaySinh,NgayLamViec,DiaChi,DienThoai")] NhanVien nhanVien)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (!ValidateEmployee(nhanVien))
                    {
                        ViewBag.username = new SelectList(db.QuanTris, "username", "username", nhanVien.username);
                        return View(nhanVien);
                    }

                    db.Entry(nhanVien).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật thông tin nhân viên thành công!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật dữ liệu: " + ex.Message);
            }

            ViewBag.username = new SelectList(db.QuanTris, "username", "username", nhanVien.username);
            return View(nhanVien);
        }

        // POST: NhanViens_64131236/Delete/5
        [HttpPost]
      
        public JsonResult DeleteMultiple(string[] selectedIds)
        {
            if (selectedIds == null || selectedIds.Length == 0)
            {
                return Json(new { success = false, message = "Không có nhân viên nào được chọn!" });
            }

            try
            {
                var employeesToDelete = db.NhanViens.Where(n => selectedIds.Contains(n.MaNV)).ToList();
                if (!employeesToDelete.Any())
                {
                    return Json(new { success = false, message = "Không tìm thấy nhân viên!" });
                }

                db.NhanViens.RemoveRange(employeesToDelete);
                db.SaveChanges();
                return Json(new { success = true, message = $"Đã xóa {employeesToDelete.Count} nhân viên thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa: " + ex.Message });
            }
        }
        private string GenerateEmployeeId()
        {
            try
            {
                var lastEmployee = db.NhanViens
                    .OrderByDescending(e => e.MaNV)
                    .FirstOrDefault();

                if (lastEmployee == null)
                {
                    return "NV001";
                }

                if (lastEmployee.MaNV.Length >= 5)
                {
                    string numericPart = lastEmployee.MaNV.Substring(2);
                    if (int.TryParse(numericPart, out int currentNumber))
                    {
                        return $"NV{(currentNumber + 1):D3}";
                    }
                }

                return $"NV{DateTime.Now.Ticks % 1000:D3}";
            }
            catch
            {
                // Fallback in case of any error
                return $"NV{DateTime.Now.Ticks % 1000:D3}";
            }
        }

        private bool ValidateEmployee(NhanVien nhanVien)
        {
            bool isValid = true;

            // Validate phone number
            if (!string.IsNullOrEmpty(nhanVien.DienThoai) && !Regex.IsMatch(nhanVien.DienThoai, @"^\d{10}$"))
            {
                ModelState.AddModelError("DienThoai", "Số điện thoại phải có 10 chữ số");
                isValid = false;
            }

            // Validate age (must be at least 18 years old)
            if (DateTime.Now.AddYears(-18) < nhanVien.NgaySinh)
            {
                ModelState.AddModelError("NgaySinh", "Nhân viên phải đủ 18 tuổi");
                isValid = false;
            }

            // Validate work date
            if (nhanVien.NgayLamViec < nhanVien.NgaySinh.AddYears(18))
            {
                ModelState.AddModelError("NgayLamViec", "Ngày làm việc phải sau ngày đủ 18 tuổi");
                isValid = false;
            }

            // Basic data validation
            if (string.IsNullOrWhiteSpace(nhanVien.Ho))
            {
                ModelState.AddModelError("Ho", "Họ không được để trống");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(nhanVien.Ten))
            {
                ModelState.AddModelError("Ten", "Tên không được để trống");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(nhanVien.DiaChi))
            {
                ModelState.AddModelError("DiaChi", "Địa chỉ không được để trống");
                isValid = false;
            }

            return isValid;
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