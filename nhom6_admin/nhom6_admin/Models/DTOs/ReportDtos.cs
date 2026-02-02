namespace nhom6_admin.Models.DTOs
{
    // ==================== SALES REPORT DTOs ====================
    public class SalesReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal OrderRevenue { get; set; }
        public decimal ServiceRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public decimal AverageServiceValue { get; set; }
        public List<SalesChartDataDto> ChartData { get; set; } = new();
    }

    public class SalesChartDataDto
    {
        public string Label { get; set; } = string.Empty;
        public decimal OrderRevenue { get; set; }
        public decimal ServiceRevenue { get; set; }
        public int OrderCount { get; set; }
        public int AppointmentCount { get; set; }
    }

    // ==================== PRODUCT REPORT DTOs ====================
    public class ProductReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public List<TopProductDto> TopSellingProducts { get; set; } = new();
        public List<TopProductDto> TopRevenueProducts { get; set; } = new();
        public List<CategoryStatsDto> CategoryStats { get; set; } = new();
    }

    public class TopProductReportDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class CategoryStatsDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public int TotalStock { get; set; }
    }

    // ==================== CUSTOMER REPORT DTOs ====================
    public class CustomerReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int CustomersWithOrders { get; set; }
        public List<TopCustomerDto> TopCustomersByOrder { get; set; } = new();
        public List<TopCustomerDto> TopCustomersByAppointment { get; set; } = new();
        public List<NewCustomerChartDto> NewCustomersByMonth { get; set; } = new();
    }

    public class TopCustomerDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public int TotalAppointments { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class NewCustomerChartDto
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    // ==================== STAFF REPORT DTOs ====================
    public class StaffReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalStaff { get; set; }
        public int ActiveStaff { get; set; }
        public int OnLeaveStaff { get; set; }
        public List<StaffPerformanceDto> StaffPerformance { get; set; } = new();
        public List<StaffAppointmentStatsDto> AppointmentsByStaff { get; set; } = new();
    }

    public class StaffPerformanceDto
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public int CompletedAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageRating { get; set; }
    }

    public class StaffAppointmentStatsDto
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int NoShowAppointments { get; set; }
    }

    // ==================== SERVICE REPORT DTOs ====================
    public class ServiceReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalServices { get; set; }
        public int ActiveServices { get; set; }
        public decimal TotalServiceRevenue { get; set; }
        public int TotalBookings { get; set; }
        public List<TopServiceDto> TopBookedServices { get; set; } = new();
        public List<ServiceCategoryRevenueDto> RevenueByCategory { get; set; } = new();
    }

    public class TopServiceReportDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ServiceCategoryRevenueDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
    }

    // ==================== REPORT FILTER ====================
    public class ReportFilterRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? GroupBy { get; set; } // day, week, month
        public int? CategoryId { get; set; }
        public int? StaffId { get; set; }
        public int Top { get; set; } = 10;
    }
}
