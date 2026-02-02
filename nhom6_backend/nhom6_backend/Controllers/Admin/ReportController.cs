using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_backend.Models;
using nhom6_backend.Models.DTOs.Admin;
using System.Text;

namespace nhom6_backend.Controllers.Admin
{
    /// <summary>
    /// Admin Report Controller
    /// Báo cáo doanh thu, thống kê kinh doanh
    /// </summary>
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportController> _logger;

        public ReportController(ApplicationDbContext context, ILogger<ReportController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Báo cáo tổng quan doanh thu
        /// </summary>
        [HttpGet("revenue")]
        public async Task<ActionResult<AdminApiResponse<RevenueReportDto>>> GetRevenueReport(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
                var to = toDate ?? DateTime.UtcNow;

                // Product revenue
                var productOrders = await _context.Orders
                    .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
                    .Where(o => o.Status == "Completed" || o.Status == "Delivered")
                    .ToListAsync();

                var productRevenue = productOrders.Sum(o => o.TotalAmount);
                var productCount = productOrders.Count;

                // Service revenue (from appointments)
                var appointments = await _context.Appointments
                    .Where(a => a.AppointmentDate >= from && a.AppointmentDate <= to)
                    .Where(a => a.Status == "Completed")
                    .ToListAsync();

                var serviceRevenue = appointments.Sum(a => a.TotalAmount);
                var appointmentCount = appointments.Count;

                // Calculate growth (compare with previous period)
                var previousFrom = from.AddDays(-(to - from).Days);
                var previousTo = from;

                var previousProductRevenue = await _context.Orders
                    .Where(o => o.CreatedAt >= previousFrom && o.CreatedAt < previousTo)
                    .Where(o => o.Status == "Completed" || o.Status == "Delivered")
                    .SumAsync(o => o.TotalAmount);

                var previousServiceRevenue = await _context.Appointments
                    .Where(a => a.AppointmentDate >= previousFrom && a.AppointmentDate < previousTo)
                    .Where(a => a.Status == "Completed")
                    .SumAsync(a => a.TotalAmount);

                var totalRevenue = productRevenue + serviceRevenue;
                var previousTotalRevenue = previousProductRevenue + previousServiceRevenue;
                var revenueGrowth = previousTotalRevenue > 0
                    ? Math.Round((totalRevenue - previousTotalRevenue) / previousTotalRevenue * 100, 2)
                    : 0;

                var report = new RevenueReportDto
                {
                    FromDate = from,
                    ToDate = to,
                    TotalRevenue = totalRevenue,
                    ProductRevenue = productRevenue,
                    ServiceRevenue = serviceRevenue,
                    OrderCount = productCount,
                    AppointmentCount = appointmentCount,
                    AverageOrderValue = productCount > 0 ? Math.Round(productRevenue / productCount, 0) : 0,
                    AverageAppointmentValue = appointmentCount > 0 ? Math.Round(serviceRevenue / appointmentCount, 0) : 0,
                    RevenueGrowthPercent = revenueGrowth
                };

                return Ok(AdminApiResponse<RevenueReportDto>.Ok(report));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue report");
                return StatusCode(500, AdminApiResponse<RevenueReportDto>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Báo cáo doanh thu theo ngày
        /// </summary>
        [HttpGet("revenue/daily")]
        public async Task<ActionResult<AdminApiResponse<List<DailyRevenueDto>>>> GetDailyRevenue(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var to = toDate ?? DateTime.UtcNow;

                var orders = await _context.Orders
                    .Where(o => o.CreatedAt.Date >= from.Date && o.CreatedAt.Date <= to.Date)
                    .Where(o => o.Status == "Completed" || o.Status == "Delivered")
                    .GroupBy(o => o.CreatedAt.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        ProductRevenue = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    })
                    .ToListAsync();

                var appointments = await _context.Appointments
                    .Where(a => a.AppointmentDate.Date >= from.Date && a.AppointmentDate.Date <= to.Date)
                    .Where(a => a.Status == "Completed")
                    .GroupBy(a => a.AppointmentDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        ServiceRevenue = g.Sum(a => a.TotalAmount),
                        AppointmentCount = g.Count()
                    })
                    .ToListAsync();

                var result = new List<DailyRevenueDto>();
                for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
                {
                    var orderData = orders.FirstOrDefault(o => o.Date == date);
                    var appointmentData = appointments.FirstOrDefault(a => a.Date == date);

                    result.Add(new DailyRevenueDto
                    {
                        Date = date,
                        ProductRevenue = orderData?.ProductRevenue ?? 0,
                        ServiceRevenue = appointmentData?.ServiceRevenue ?? 0,
                        TotalRevenue = (orderData?.ProductRevenue ?? 0) + (appointmentData?.ServiceRevenue ?? 0),
                        OrderCount = orderData?.OrderCount ?? 0,
                        AppointmentCount = appointmentData?.AppointmentCount ?? 0
                    });
                }

                return Ok(AdminApiResponse<List<DailyRevenueDto>>.Ok(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily revenue");
                return StatusCode(500, AdminApiResponse<List<DailyRevenueDto>>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Báo cáo doanh thu theo tháng
        /// </summary>
        [HttpGet("revenue/monthly")]
        public async Task<ActionResult<AdminApiResponse<List<MonthlyRevenueDto>>>> GetMonthlyRevenue(
            [FromQuery] int? year)
        {
            try
            {
                var targetYear = year ?? DateTime.UtcNow.Year;

                var orders = await _context.Orders
                    .Where(o => o.CreatedAt.Year == targetYear)
                    .Where(o => o.Status == "Completed" || o.Status == "Delivered")
                    .GroupBy(o => o.CreatedAt.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        ProductRevenue = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    })
                    .ToListAsync();

                var appointments = await _context.Appointments
                    .Where(a => a.AppointmentDate.Year == targetYear)
                    .Where(a => a.Status == "Completed")
                    .GroupBy(a => a.AppointmentDate.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        ServiceRevenue = g.Sum(a => a.TotalAmount),
                        AppointmentCount = g.Count()
                    })
                    .ToListAsync();

                var result = new List<MonthlyRevenueDto>();
                for (var month = 1; month <= 12; month++)
                {
                    var orderData = orders.FirstOrDefault(o => o.Month == month);
                    var appointmentData = appointments.FirstOrDefault(a => a.Month == month);

                    result.Add(new MonthlyRevenueDto
                    {
                        Year = targetYear,
                        Month = month,
                        ProductRevenue = orderData?.ProductRevenue ?? 0,
                        ServiceRevenue = appointmentData?.ServiceRevenue ?? 0,
                        TotalRevenue = (orderData?.ProductRevenue ?? 0) + (appointmentData?.ServiceRevenue ?? 0),
                        OrderCount = orderData?.OrderCount ?? 0,
                        AppointmentCount = appointmentData?.AppointmentCount ?? 0
                    });
                }

                return Ok(AdminApiResponse<List<MonthlyRevenueDto>>.Ok(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly revenue");
                return StatusCode(500, AdminApiResponse<List<MonthlyRevenueDto>>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Báo cáo sản phẩm bán chạy
        /// </summary>
        [HttpGet("top-products")]
        public async Task<ActionResult<AdminApiResponse<List<TopProductReportDto>>>> GetTopProducts(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int top = 10)
        {
            try
            {
                var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
                var to = toDate ?? DateTime.UtcNow;

                var topProducts = await _context.OrderItems
                    .Include(oi => oi.Order)
                    .Include(oi => oi.Product)
                    .Where(oi => oi.Order!.CreatedAt >= from && oi.Order.CreatedAt <= to)
                    .Where(oi => oi.Order!.Status == "Completed" || oi.Order!.Status == "Delivered")
                    .GroupBy(oi => new { oi.ProductId, oi.Product!.Name, oi.Product.ImageUrl })
                    .Select(g => new TopProductReportDto
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.Name,
                        ImageUrl = g.Key.ImageUrl,
                        QuantitySold = g.Sum(oi => oi.Quantity),
                        Revenue = g.Sum(oi => oi.TotalPrice)
                    })
                    .OrderByDescending(p => p.QuantitySold)
                    .Take(top)
                    .ToListAsync();

                return Ok(AdminApiResponse<List<TopProductReportDto>>.Ok(topProducts));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top products");
                return StatusCode(500, AdminApiResponse<List<TopProductReportDto>>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Báo cáo dịch vụ phổ biến
        /// </summary>
        [HttpGet("top-services")]
        public async Task<ActionResult<AdminApiResponse<List<TopServiceReportDto>>>> GetTopServices(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int top = 10)
        {
            try
            {
                var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
                var to = toDate ?? DateTime.UtcNow;

                var topServices = await _context.AppointmentServices
                    .Include(asp => asp.Appointment)
                    .Include(asp => asp.Service)
                    .Where(asp => asp.Appointment!.AppointmentDate >= from && asp.Appointment.AppointmentDate <= to)
                    .Where(asp => asp.Appointment!.Status == "Completed")
                    .GroupBy(asp => new { asp.ServiceId, asp.Service!.Name, asp.Service.ImageUrl })
                    .Select(g => new TopServiceReportDto
                    {
                        ServiceId = g.Key.ServiceId,
                        ServiceName = g.Key.Name,
                        ImageUrl = g.Key.ImageUrl,
                        BookingCount = g.Count(),
                        Revenue = g.Sum(asp => asp.Price)
                    })
                    .OrderByDescending(s => s.BookingCount)
                    .Take(top)
                    .ToListAsync();

                return Ok(AdminApiResponse<List<TopServiceReportDto>>.Ok(topServices));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top services");
                return StatusCode(500, AdminApiResponse<List<TopServiceReportDto>>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Báo cáo khách hàng
        /// </summary>
        [HttpGet("customers")]
        public async Task<ActionResult<AdminApiResponse<CustomerReportDto>>> GetCustomerReport(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
                var to = toDate ?? DateTime.UtcNow;

                // Total customers
                var totalCustomers = await _context.Users.CountAsync();

                // New customers in period
                var newCustomers = await _context.Users
                    .CountAsync(u => u.CreatedAt >= from && u.CreatedAt <= to);

                // Active customers (made purchase or appointment)
                var activeOrderCustomers = await _context.Orders
                    .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
                    .Where(o => o.UserId != null)
                    .Select(o => o.UserId)
                    .Distinct()
                    .CountAsync();

                var activeAppointmentCustomers = await _context.Appointments
                    .Where(a => a.AppointmentDate >= from && a.AppointmentDate <= to)
                    .Where(a => a.UserId != null)
                    .Select(a => a.UserId)
                    .Distinct()
                    .CountAsync();

                // Top customers - get from orders grouped by user
                var topCustomers = await _context.Orders
                    .Where(o => o.UserId != null)
                    .Where(o => o.Status == "Completed" || o.Status == "Delivered")
                    .GroupBy(o => o.UserId)
                    .Select(g => new TopCustomerDto
                    {
                        UserId = g.Key!,
                        CustomerName = g.First().CustomerName ?? "N/A",
                        Email = g.First().CustomerEmail,
                        Phone = g.First().CustomerPhone,
                        OrderCount = g.Count(),
                        TotalSpent = g.Sum(o => o.TotalAmount),
                        LastOrderDate = g.Max(o => o.CreatedAt)
                    })
                    .OrderByDescending(c => c.TotalSpent)
                    .Take(10)
                    .ToListAsync();

                var report = new CustomerReportDto
                {
                    TotalCustomers = totalCustomers,
                    NewCustomers = newCustomers,
                    ActiveCustomers = activeOrderCustomers + activeAppointmentCustomers,
                    TopCustomers = topCustomers
                };

                return Ok(AdminApiResponse<CustomerReportDto>.Ok(report));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer report");
                return StatusCode(500, AdminApiResponse<CustomerReportDto>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Báo cáo nhân viên (staff performance)
        /// </summary>
        [HttpGet("staff")]
        public async Task<ActionResult<AdminApiResponse<List<StaffPerformanceDto>>>> GetStaffPerformance(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
                var to = toDate ?? DateTime.UtcNow;

                var staffPerformance = await _context.Staff
                    .Include(s => s.Appointments)
                    .Select(s => new StaffPerformanceDto
                    {
                        StaffId = s.Id,
                        StaffName = s.FullName,
                        TotalAppointments = s.Appointments != null 
                            ? s.Appointments.Count(a => a.AppointmentDate >= from && a.AppointmentDate <= to)
                            : 0,
                        CompletedAppointments = s.Appointments != null
                            ? s.Appointments.Count(a => a.AppointmentDate >= from && a.AppointmentDate <= to && a.Status == "Completed")
                            : 0,
                        CancelledAppointments = s.Appointments != null
                            ? s.Appointments.Count(a => a.AppointmentDate >= from && a.AppointmentDate <= to && a.Status == "Cancelled")
                            : 0,
                        Revenue = s.Appointments != null
                            ? s.Appointments.Where(a => a.AppointmentDate >= from && a.AppointmentDate <= to && a.Status == "Completed")
                                .Sum(a => a.TotalAmount)
                            : 0,
                        AverageRating = 0 // Would need reviews table
                    })
                    .OrderByDescending(s => s.CompletedAppointments)
                    .ToListAsync();

                return Ok(AdminApiResponse<List<StaffPerformanceDto>>.Ok(staffPerformance));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staff performance");
                return StatusCode(500, AdminApiResponse<List<StaffPerformanceDto>>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Export báo cáo doanh thu
        /// </summary>
        [HttpGet("export/revenue")]
        public async Task<IActionResult> ExportRevenueReport(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var to = toDate ?? DateTime.UtcNow;

                var orders = await _context.Orders
                    .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                var csv = new StringBuilder();
                csv.AppendLine("Ngày,Mã đơn,Khách hàng,Tổng tiền,Trạng thái,Thanh toán");

                foreach (var order in orders)
                {
                    csv.AppendLine($"\"{order.CreatedAt:yyyy-MM-dd}\"," +
                        $"\"{order.OrderCode}\"," +
                        $"\"{order.CustomerName}\"," +
                        $"{order.TotalAmount}," +
                        $"\"{order.Status}\"," +
                        $"\"{order.PaymentStatus}\"");
                }

                var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
                return File(bytes, "text/csv; charset=utf-8", $"revenue_report_{from:yyyyMMdd}_{to:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting revenue report");
                return StatusCode(500, "Lỗi server");
            }
        }
    }

    #region Report DTOs

    public class RevenueReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal ProductRevenue { get; set; }
        public decimal ServiceRevenue { get; set; }
        public int OrderCount { get; set; }
        public int AppointmentCount { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal AverageAppointmentValue { get; set; }
        public decimal RevenueGrowthPercent { get; set; }
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal ProductRevenue { get; set; }
        public decimal ServiceRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
        public int AppointmentCount { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal ProductRevenue { get; set; }
        public decimal ServiceRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
        public int AppointmentCount { get; set; }
    }

    public class TopProductReportDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string? ImageUrl { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopServiceReportDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = "";
        public string? ImageUrl { get; set; }
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class CustomerReportDto
    {
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public List<TopCustomerDto> TopCustomers { get; set; } = new();
    }

    public class TopCustomerDto
    {
        public string UserId { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastOrderDate { get; set; }
    }

    public class StaffPerformanceDto
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; } = "";
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageRating { get; set; }
    }

    #endregion
}
