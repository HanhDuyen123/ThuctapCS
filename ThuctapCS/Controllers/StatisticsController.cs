using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using ThuctapCS.Models;
using ThuctapCS.Filters;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using System.IO;
using iText.Layout;
using System.Collections.Generic;


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
            query = query.Where(o => o.status == "Delivered");
            //var totalRevenue = query.Sum(o => o.shipping_fee);
            var totalRevenue = query.Sum(o => (decimal?)o.shipping_fee) ?? 0;

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

        [HttpGet]
        public ActionResult ExportToPdf(DateTime? startDate, DateTime? endDate)
        {
            // Lấy dữ liệu thống kê số đơn theo trạng thái
            var orderStatusStats = GetOrderStatusStats(startDate, endDate).Data as List<dynamic>;
            Console.WriteLine($"StartDate: {startDate}, EndDate: {endDate}");

            if (orderStatusStats == null || !orderStatusStats.Any())
            {
                Console.WriteLine("Không có dữ liệu trong orderStatusStats.");
                return Json(new { success = false, message = "Không có dữ liệu để xuất." }, JsonRequestBehavior.AllowGet);
            }

            using (var stream = new MemoryStream())
            {
                // Tạo một tài liệu PDF mới
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Tạo font cho tiêu đề
                var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Thêm tiêu đề
                document.Add(new Paragraph("Thống kê đơn hàng").SetFont(titleFont).SetFontSize(16));
                document.Add(new Paragraph($"Thời gian: {startDate?.ToString("dd/MM/yyyy") ?? "Tất cả"} - {endDate?.ToString("dd/MM/yyyy") ?? "Tất cả"}").SetFont(normalFont).SetFontSize(12));
                document.Add(new Paragraph(" ")); // Thêm một khoảng trắng

                // Tạo bảng để hiển thị dữ liệu thống kê
                var table = new Table(2);
                table.AddCell("Trạng thái");
                table.AddCell("Số lượng");

                // Duyệt qua dữ liệu và thêm vào bảng
                foreach (var stat in orderStatusStats)
                {
                    table.AddCell(stat.Status);
                    table.AddCell(stat.OrderCount.ToString());
                }

                document.Add(table);
                document.Close();

                // Trả về file PDF
                return File(stream.ToArray(), "application/pdf", "ThongKeDonHang.pdf");
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
    }
}
