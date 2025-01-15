using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ThuctapCS.Filters;
using ThuctapCS.Models;

namespace ThuctapCS.Controllers
{
    public class OrderAssignmentController : Controller
    {
        private readonly ThucTapCSEntities db = new ThucTapCSEntities();

        // GET: OrderAssignment/Assign
        [CustomAuthorize("Quản lý")]
        public ActionResult Assign(string minDate, string maxDate, string district, int? pageSize, int page = 1)
        {
            // Thiết lập giá trị ngày mặc định
            DateTime startDate = string.IsNullOrEmpty(minDate) ? DateTime.MinValue : DateTime.Parse(minDate);
            DateTime endDate = string.IsNullOrEmpty(maxDate) ? DateTime.MaxValue : DateTime.Parse(maxDate);

            // Khởi tạo truy vấn đơn hàng với điều kiện lọc
            var ordersQuery = db.Orders.AsQueryable()
                              .Where(o => o.warehouse_date >= startDate &&
                                     o.warehouse_date <= endDate &&
                                     o.status == "Processing");

            // Lọc theo phường/xã nếu có
            if (!string.IsNullOrEmpty(district))
            {
                ordersQuery = ordersQuery.Where(o => o.Ward.ward_name == district);
            }

            // Lấy số lượng đơn hàng và tính toán số trang
            var totalItems = ordersQuery.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / (pageSize ?? 5));
            

            // Phân trang
            var orders = ordersQuery
                .OrderBy(o => o.order_id) // Sắp xếp theo mã đơn hàng
                .Skip((page - 1) * (pageSize ?? 5))
                .Take(pageSize ?? 5)
                .ToList();
            // Kiểm tra nếu yêu cầu là AJAX
            if (Request.IsAjaxRequest())
            {
                ViewBag.TotalPages = totalPages;
                ViewBag.CurrentPage = page;

                return PartialView("_OrderAssign", orders);
            }
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            // Lấy danh sách nhân viên
            ViewBag.Employees = db.Employees.Where(e => e.role_name == "Nhân viên").ToList();

            return View(orders);
        }

        // POST: OrderAssignment/AssignOrders
        [HttpPost]
        [CustomAuthorize("Quản lý")]
        public ActionResult AssignOrders(long employeeId, long[] selectedOrders)
        {
            try
            {
                if (selectedOrders != null && selectedOrders.Length > 0)
                {
                    foreach (var orderId in selectedOrders)
                    {
                        var orderAssignment = new OrderAssignment
                        {
                            order_id = orderId,
                            employee_id = employeeId,
                            assigned_date = DateTime.Now
                        };

                        db.OrderAssignments.Add(orderAssignment);

                        var order = db.Orders.Find(orderId);
                        if (order != null)
                        {
                            order.status = "Shipping";
                        }
                    }

                    db.SaveChanges();
                    return Json(new { success = true, message = "Đơn hàng đã được phân công thành công!" });
                }
                return Json(new { success = false, message = "Vui lòng chọn ít nhất một đơn hàng." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
    }
}