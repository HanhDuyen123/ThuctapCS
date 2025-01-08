using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ThuctapCS.Filters;
using ThuctapCS.Models;

namespace ThuctapCS.Controllers
{
    public class OrdersController : Controller
    {
        private ThucTapCSEntities db = new ThucTapCSEntities();

        // GET: Orders
        //public ActionResult Index()
        //{
        //    var orders = db.Orders.Include(o => o.Customer).Include(o => o.Ward);
        //    return View(orders.ToList());
        //}

        public ActionResult Index(string searchName, string status, decimal? minTotalAmount, decimal? maxTotalAmount,
                                  DateTime? minWarehouseDate, DateTime? maxWarehouseDate,
                                  DateTime? minShippingDate, DateTime? maxShippingDate, 
                                  string sortOrder, int page = 1)
        {
            var userRole = Session["Role"];
            var userId = Session["idUser"] as long?;
            var orders = db.Orders.Include(o => o.Customer).AsQueryable();

            // Phân quyền hiển thị đơn hàng
            if (userRole != null && userRole.ToString() == "Quản lý")
            {
                // Quản lý xem tất cả đơn hàng
            }
            else if (userRole != null && userRole.ToString() == "Nhân viên" && userId.HasValue)
            {
                // Nhân viên chỉ xem đơn hàng mà mình đã được giao
                var assignedOrderIds = db.OrderAssignments
                                          .Where(oa => oa.employee_id == userId.Value)
                                          .Select(oa => oa.order_id);
                orders = orders.Where(o => assignedOrderIds.Contains(o.order_id));
            }
            // Lọc theo tên khách hàng
            if (!string.IsNullOrEmpty(searchName))
            {
                orders = orders.Where(o => o.receiver_name.Contains(searchName));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.status == status);
            }
            
            // Lọc theo tổng tiền
            if (minTotalAmount.HasValue)
            {
                orders = orders.Where(o => (o.order_price + o.shipping_fee) >= minTotalAmount.Value);
            }
            if (maxTotalAmount.HasValue)
            {
                orders = orders.Where(o => (o.order_price + o.shipping_fee) <= maxTotalAmount.Value);
            }

            // Lọc theo ngày nhập kho
            if (minWarehouseDate.HasValue)
            {
                orders = orders.Where(o => o.warehouse_date >= minWarehouseDate.Value);
            }
            if (maxWarehouseDate.HasValue)
            {
                orders = orders.Where(o => o.warehouse_date <= maxWarehouseDate.Value);
            }

            // Lọc theo ngày giao hàng
            if (minShippingDate.HasValue)
            {
                orders = orders.Where(o => o.shipping_date >= minShippingDate.Value);
            }
            if (maxShippingDate.HasValue)
            {
                orders = orders.Where(o => o.shipping_date <= maxShippingDate.Value);
            }

            // Sắp xếp sản phẩm
            switch (sortOrder)
            {
                case "Name_asc":
                    orders = orders.OrderBy(o => o.receiver_name);
                    break;
                case "Name_desc":
                    orders = orders.OrderByDescending(o => o.receiver_name);
                    break;
                case "TotalAmount_asc":
                    orders = orders.OrderBy(o => (o.order_price + o.shipping_fee));
                    break;
                case "TotalAmount_desc":
                    orders = orders.OrderByDescending(o => (o.order_price + o.shipping_fee));
                    break;
                default:
                    orders = orders.OrderBy(p => p.order_id); // Mặc định
                    break;
            }

            int pageSize = 5; // Số sản phẩm trên mỗi trang
            var totalItems = orders.Count(); // Tổng số sản phẩm
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize); // Tổng số trang

            orders = orders.Skip((page - 1) * pageSize).Take(pageSize); // Lấy sản phẩm cho trang hiện tại

            // Kiểm tra yêu cầu AJAX
            if (Request.IsAjaxRequest())
            {
                ViewBag.TotalPages = totalPages;
                ViewBag.CurrentPage = page;
                return PartialView("_OrderList", orders.ToList());
            }

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            return View(orders.ToList());
        }

        [HttpPost]
        public JsonResult UpdateStatus(long id, string status)
        {
            var order = db.Orders.Find(id);
            if (order != null)
            {
                order.status = status;
                // Cập nhật shipping_date nếu trạng thái là "Delivered"
                if (status == "Delivered")
                {
                    order.shipping_date = DateTime.Now; // Cập nhật ngày hiện tại
                }
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        [CustomAuthorize("Quản lý")]
        // GET: Orders/Assign
        public ActionResult Assign()
        {
            // Lấy tất cả đơn hàng chưa được phân công
            var assignedOrderIds = db.OrderAssignments.Select(oa => oa.order_id);
            var unassignedOrders = db.Orders
                                     .Where(o => !assignedOrderIds.Contains(o.order_id))
                                     .ToList();

            // Lấy danh sách nhân viên có vai trò "Nhân viên"
            var employees = db.Employees
                              .Where(e => e.role_name == "Nhân viên") 
                              .ToList();

            ViewBag.Employees = employees;
            return View(unassignedOrders);
        }
        [CustomAuthorize("Quản lý")]
        // POST: Orders/Assign
        [HttpPost]
        public ActionResult AssignOrder(long orderId, long employeeId)
        {
            var orderAssignment = new OrderAssignment
            {
                order_id = orderId,
                employee_id = employeeId,
                assigned_date = DateTime.Now
            };

            db.OrderAssignments.Add(orderAssignment);
            db.SaveChanges();

            return RedirectToAction("Index"); // Quay lại danh sách đơn hàng
        }

        // GET: Orders/GetOrderDetails
        public JsonResult GetOrderDetails(long id)
        {
            var order = db.Orders.Include(o => o.Ward).FirstOrDefault(o => o.order_id == id);
            if (order != null)
            {
                // Tính tổng giá trị đơn hàng
                decimal totalValue = order.order_price + order.shipping_fee;

                return Json(new
                {
                    order.receiver_name,
                    order.receiver_street,
                    order.receiver_phone,
                    totalValue, // Trả về tổng giá trị
                    wardName = order.Ward != null ? order.Ward.ward_name : "Không có"
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        // GET: Orders/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: Orders/Create
        // GET: Orders/Create
        public ActionResult Create()
        {
            // Lấy danh sách khách hàng với tên đầy đủ
            ViewBag.customer_id = new SelectList(
                db.Customers.Select(c => new
                {
                    customer_id = c.customer_id,
                    full_name = c.last_name + " " + c.first_name // Kết hợp họ và tên
                }).ToList(),
                "customer_id",
                "full_name" // Sử dụng tên đầy đủ cho dropdown
            );

            ViewBag.ward_id = new SelectList(db.Wards, "ward_id", "ward_name");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "order_id,customer_id,receiver_name,receiver_street,ward_id,receiver_phone,order_price,shipping_fee,warehouse_date,shipping_date,status,return_reason")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Orders.Add(order);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đơn hàng đã được thêm thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.customer_id = new SelectList(db.Customers, "customer_id", "first_name", order.customer_id);
            ViewBag.ward_id = new SelectList(db.Wards, "ward_id", "ward_name", order.ward_id);
            return View(order);
        }

        // GET: Orders/Edit/5
        // GET: Orders/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            // Tạo danh sách khách hàng với tên đầy đủ
            ViewBag.customer_id = new SelectList(
                db.Customers.Select(c => new
                {
                    customer_id = c.customer_id,
                    full_name = c.last_name + " " + c.first_name
                }).ToList(),
                "customer_id",
                "full_name",
                order.customer_id
            );

            ViewBag.ward_id = new SelectList(db.Wards, "ward_id", "ward_name", order.ward_id);
            return View(order);
        }


        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "order_id,customer_id,receiver_name,receiver_street,ward_id,receiver_phone,order_price,shipping_fee,warehouse_date,shipping_date,status,return_reason")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đơn hàng đã được chỉnh sửa thành công!";
                return RedirectToAction("Index");
            }
            ViewBag.customer_id = new SelectList(db.Customers, "customer_id", "first_name", order.customer_id);
            ViewBag.ward_id = new SelectList(db.Wards, "ward_id", "ward_name", order.ward_id);
            return View(order);
        }

        // GET: Orders/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Order order = db.Orders.Find(id);
            db.Orders.Remove(order);
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
    }
}
