using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ThuctapCS.Models;

namespace ThuctapCS.Controllers
{
    public class EmployeesController : Controller
    {
        private ThucTapCSEntities db = new ThucTapCSEntities();

        // GET: Employees
        //public ActionResult Index()
        //{
        //    return View(db.Employees.ToList());
        //}
        public ActionResult Index(string searchName, string gender, string phoneNumber, string address, string roleName, string sortOrder, int page = 1)
        {
            var employees = db.Employees.AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
            {
                employees = employees.Where(e => (e.last_name + " " + e.first_name).Contains(searchName));
            }

            if (!string.IsNullOrEmpty(gender))
            {
                employees = employees.Where(e => e.gender == gender);
            }

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                employees = employees.Where(e => e.phone == phoneNumber);
            }

            if (!string.IsNullOrEmpty(address))
            {
                employees = employees.Where(e => e.address.Contains(address));
            }

            if (!string.IsNullOrEmpty(roleName))
            {
                employees = employees.Where(e => e.role_name == roleName);
            }

            // Sắp xếp
            switch (sortOrder)
            {
                case "Name_asc":
                    employees = employees.OrderBy(e => e.last_name + " " + e.first_name);
                    break;
                case "Name_desc":
                    employees = employees.OrderBy(e => e.last_name + " " + e.first_name);
                    break;
                default:
                    employees = employees.OrderBy(e => e.employee_id); // Mặc định
                    break;
            }

            int pageSize = 5; // Số nhân viên trên mỗi trang
            var totalItems = employees.Count(); // Tổng số nhân viên
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize); // Tổng số trang

            employees = employees.Skip((page - 1) * pageSize).Take(pageSize); // Lấy nhân viên cho trang hiện tại

            // Kiểm tra yêu cầu AJAX
            if (Request.IsAjaxRequest())
            {
                ViewBag.TotalPages = totalPages;
                ViewBag.CurrentPage = page;
                return PartialView("_EmpList", employees.ToList());
            }

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            return View(employees.ToList());
        }


        // GET: Employees/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // GET: Employees/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "employee_id,first_name,last_name,date_of_birth,gender,email,password,phone,address,role_name,hire_date,salary")] Employee employee)
        {
            // Kiểm tra các trường bắt buộc
            if (string.IsNullOrWhiteSpace(employee.first_name))
            {
                ModelState.AddModelError("first_name", "Tên không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(employee.last_name))
            {
                ModelState.AddModelError("last_name", "Họ không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(employee.gender))
            {
                ModelState.AddModelError("gender", "Giới tính không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(employee.email))
            {
                ModelState.AddModelError("email", "Email không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(employee.password))
            {
                ModelState.AddModelError("password", "Mật khẩu không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(employee.role_name))
            {
                ModelState.AddModelError("role_name", "Vai trò không được để trống.");
            }

            if (employee.salary == 0)
            {
                ModelState.AddModelError("salary", "Lương không được để trống.");
            }
            else if (employee.salary < 0)
            {
                ModelState.AddModelError("salary", "Lương phải là số dương.");
            }

            // Kiểm tra xem email đã tồn tại
            var existingEmail = db.Employees.FirstOrDefault(e => e.email == employee.email);
            if (existingEmail != null)
            {
                ModelState.AddModelError("email", "Email này đã tồn tại.");
            }

            // Kiểm tra xem số điện thoại đã tồn tại
            var existingPhone = db.Employees.FirstOrDefault(e => e.phone == employee.phone);
            if (existingPhone != null)
            {
                ModelState.AddModelError("phone", "Số điện thoại này đã tồn tại.");
            }

            // Nếu không có lỗi, thêm nhân viên mới
            if (ModelState.IsValid)
            {
                employee.password = GetMD5(employee.password);
                db.Employees.Add(employee);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Nhân viên đã được thêm thành công!";
                return RedirectToAction("Index");
            }

            return View(employee);
        }

        private string GetMD5(string str)
        {
            using (var md5 = MD5.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                byte[] hash = md5.ComputeHash(bytes);

                return string.Concat(hash.Select(b => b.ToString("x2")));
            }
        }

        // GET: Employees/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "employee_id,first_name,last_name,date_of_birth,gender,email,password,phone,address,role_name,hire_date,salary")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                db.Entry(employee).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Nhân viên đã được chỉnh sửa thành công!";
                return RedirectToAction("Index");
            }
            return View(employee);
        }

        // GET: Employees/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Employee employee = db.Employees.Find(id);
            db.Employees.Remove(employee);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Nhân viên đã được xóa thành công!";
            return RedirectToAction("Index");
        }
        public ActionResult GetEmployeeInfo()
        {
            // Lấy ID người dùng từ session
            var userId = (long?)Session["idUser"];
            if (userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var employee = db.Employees.Find(userId);
            if (employee == null)
            {
                return HttpNotFound();
            }

            return Json(new
            {
                fullName = employee.last_name + " " + employee.first_name,
                email = employee.email,
                phone = employee.phone,
                address = employee.address,
                roleName = employee.role_name
            }, JsonRequestBehavior.AllowGet);
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
