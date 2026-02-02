namespace nhom6_admin.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        // Revenue Statistics
        public decimal TotalRevenueThisMonth { get; set; }
        public decimal TotalRevenueLastMonth { get; set; }
        public double RevenueGrowthPercent { get; set; }
        public decimal TotalRevenueToday { get; set; }

        // Orders Statistics
        public int OrdersToday { get; set; }
        public int PendingOrders { get; set; }
        public int TotalOrdersThisMonth { get; set; }

        // Appointments Statistics
        public int AppointmentsToday { get; set; }
        public int PendingAppointments { get; set; }
        public int TotalAppointmentsThisMonth { get; set; }

        // Customers Statistics
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }

        // Products & Services Statistics
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int TotalServices { get; set; }
        public int ActiveStaff { get; set; }

        // Lists for display
        public List<AppointmentSummary> TodayAppointments { get; set; } = new();
        public List<OrderSummary> RecentOrders { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = new();
        public List<TopServiceDto> TopServices { get; set; } = new();

        // Chart Data
        public List<RevenueChartData> RevenueChartData { get; set; } = new();
        public OrderStatusChartData OrderStatusChart { get; set; } = new();
    }

    public class AppointmentSummary
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }

    public class OrderSummary
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class TopProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopServiceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int TotalBookings { get; set; }
        public decimal Revenue { get; set; }
    }

    public class RevenueChartData
    {
        public string Label { get; set; } = string.Empty;
        public decimal OrderRevenue { get; set; }
        public decimal ServiceRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class OrderStatusChartData
    {
        public int Pending { get; set; }
        public int Confirmed { get; set; }
        public int Processing { get; set; }
        public int Shipping { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }
}
