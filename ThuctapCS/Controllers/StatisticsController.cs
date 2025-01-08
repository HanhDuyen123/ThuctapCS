using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using ThuctapCS.Models;
using ThuctapCS.Filters;

namespace ThuctapCS.Controllers
{
    [CustomAuthorize("Quản lý")]
    public class StatisticsController : Controller
    {
        private readonly ThucTapCSEntities db = new ThucTapCSEntities();

        public ActionResult Index1()
        {
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }

        // Thống kê số đơn theo trạng thái
        [HttpGet]
        public JsonResult GetOrderStatusStats(DateTime? startDate, DateTime? endDate)
        {
            var query = db.Orders.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(o => o.warehouse_date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(o => o.warehouse_date <= endDate.Value);

            var stats = query
                .GroupBy(o => o.status)
                .Select(g => new
                {
                    Status = g.Key,
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.OrderCount)
                .ToList();

            return Json(stats, JsonRequestBehavior.AllowGet);
        }

        // Thống kê tổng số đơn theo từng xã
        [HttpGet]
        public JsonResult GetDistrictOrderStats(DateTime? startDate, DateTime? endDate)
        {
            var totalOrders = db.Orders.AsQueryable();

            if (startDate.HasValue)
                totalOrders = totalOrders.Where(o => o.warehouse_date >= startDate.Value);
            if (endDate.HasValue)
                totalOrders = totalOrders.Where(o => o.warehouse_date <= endDate.Value);

            var totalOrdersCount = totalOrders.Count();

            var stats = totalOrders
                .Join(db.Wards,
                    o => o.ward_id,
                    w => w.ward_id,
                    (o, w) => new { Order = o, Ward = w })
                .GroupBy(x => x.Ward.ward_name)
                .Select(g => new
                {
                    Ward = g.Key,
                    OrderCount = g.Count(),
                    Percentage = Math.Round((double)g.Count() * 100 / totalOrdersCount, 2)
                })
                .OrderByDescending(x => x.OrderCount)
                .ToList();

            return Json(stats, JsonRequestBehavior.AllowGet);
        }
        // Thống kê tổng doanh thu từ phí ship trong khoảng thời gian
        [HttpGet]
        public JsonResult GetTotalRevenue(DateTime? startDate, DateTime? endDate)
        {
            var query = db.Orders.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(o => o.warehouse_date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(o => o.warehouse_date <= endDate.Value);

            var totalRevenue = query.Sum(o => o.shipping_fee);

            return Json(new { TotalRevenue = totalRevenue }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetEmployeesByRole(string roleName)
        {
            var employees = db.Employees
                .Where(e => e.role_name == roleName)
                .Select(e => new
                {
                    e.employee_id,
                    FullName = e.last_name + " " + e.first_name
                })
                .ToList();

            return Json(employees, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetEmployeeOrderStats(long employeeId, DateTime? startDate, DateTime? endDate)
        {
            // Lấy danh sách đơn hàng theo nhân viên
            var assignedOrdersQuery = db.OrderAssignments
                .Where(assignment => assignment.employee_id == employeeId);

            // Kiểm tra điều kiện ngày tháng
            if (startDate.HasValue)
            {
                assignedOrdersQuery = assignedOrdersQuery
                    .Where(assignment => assignment.assigned_date >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                assignedOrdersQuery = assignedOrdersQuery
                    .Where(assignment => assignment.assigned_date <= endDate.Value);
            }

            // Số đơn đã được phân công
            var assignedCount = assignedOrdersQuery.Count();

            // Số đơn đã giao
            var deliveredCount = db.Orders
                .Where(order => order.status == "Delivered" &&
                                assignedOrdersQuery.Select(a => a.order_id).Contains(order.order_id))
                .Count();

            return Json(new { AssignedCount = assignedCount, DeliveredCount = deliveredCount }, JsonRequestBehavior.AllowGet);
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
