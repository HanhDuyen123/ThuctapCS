using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThuctapCS.Models
{
    // Models/StatisticsModels.cs
    public class OrderStatusStatistic
    {
        public string Status { get; set; }
        public int OrderCount { get; set; }
    }

    public class EmployeePerformanceStatistic
    {
        public string EmployeeName { get; set; }
        public int CompletedOrders { get; set; }
    }

    public class MonthlyRevenueStatistic
    {
        public string Month { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class DistrictOrderStatistic
    {
        public string District { get; set; }
        public int OrderCount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class EmployeeEfficiencyStatistic
    {
        public string EmployeeName { get; set; }
        public double AverageProcessingHours { get; set; }
        public int ProcessedOrders { get; set; }
        public int ReturnedOrders { get; set; }
    }
}