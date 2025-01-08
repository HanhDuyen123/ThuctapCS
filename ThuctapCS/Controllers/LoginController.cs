using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ThuctapCS.Models;

namespace ThuctapCS.Controllers
{
    public class LoginController : Controller
    {
            private ThucTapCSEntities db = new ThucTapCSEntities();
            // GET: Account
            public ActionResult Login()
            {
                return View();
            }

            // POST: Đăng nhập
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Login(string email, string password)
            {
                if (ModelState.IsValid)
                {
                    var hashedPassword = GetMD5(password);

                    // Kiểm tra nhân viên
                    var employee = db.Employees
                        .FirstOrDefault(e => e.email == email && e.password == hashedPassword);

                    if (employee != null)
                    {
                        // Đăng nhập nhân viên
                        Session["idUser"] = employee.employee_id;
                        Session["FullName"] = employee.last_name + " " + employee.first_name;
                        Session["Role"] = employee.role_name;

                        // Đặt cookie xác thực và lưu vai trò
                        FormsAuthentication.SetAuthCookie(employee.email, false);

                        // Tạo FormsAuthenticationTicket
                        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                            1, // Version
                            employee.email, // Email
                            DateTime.Now, // Thời gian cấp
                            DateTime.Now.AddMinutes(30), // Hết hạn sau 30 phút
                            false, // Không duy trì phiên đăng nhập (persistent)
                            employee.role_name // Vai trò của người dùng
                        );
                        // Mã hóa ticket và lưu vào cookie
                        string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                        var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                        Response.Cookies.Add(authCookie);


                        switch (employee.role_name)
                        {
                            case "Quản lý":
                                return RedirectToAction("Index", "Dashboard");
                            case "Nhân viên":
                                return RedirectToAction("Index", "Dashboard");
                            default:
                                ViewBag.Error = "Vai trò không hợp lệ!";
                                break;
                        }

                        foreach (string key in Session.Keys)
                        {
                            var value = Session[key];
                            System.Diagnostics.Debug.WriteLine($"Session Key: {key}, Value: {value}");
                        }

                        return RedirectToAction("Index", "Home");
                    }

                    ViewBag.Error = "Email hoặc mật khẩu không đúng!";

                }

                return View();
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

            // GET: Logout
            public ActionResult Logout()
            {
                // Xóa toàn bộ session
                Session.Clear();

                // Xóa cookie xác thực FormsAuthentication
                FormsAuthentication.SignOut();

                // Chuyển hướng đến màn hình đăng nhập
                return RedirectToAction("Login", "Login");
            }
    }
}