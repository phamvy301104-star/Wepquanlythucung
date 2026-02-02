using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Models;
using nhom6_admin.Models.DTOs;

namespace nhom6_admin.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get sales report
        /// </summary>
        [HttpGet("sales")]
        public async Task<ActionResult<ApiResponse<SalesReportDto>>> GetSalesReport(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? groupBy = "day")
        {
            try
            {
                var startDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var endDate = toDate ?? DateTime.UtcNow;

                // Orders
                var ordersQuery = _context.Orders
                    .Where(o => !o.IsDeleted &&
                               o.CreatedAt >= startDate &&
                               o.CreatedAt <= endDate.AddDays(1));

                var deliveredOrders = ordersQuery.Where(o => o.Status == "Delivered" || o.Status == "Completed");

                var orderStats = new
                {
                    TotalOrders = await ordersQuery.CountAsync(),
                    TotalRevenue = await deliveredOrders.SumAsync(o => o.TotalAmount),
                    AverageOrderValue = await deliveredOrders.AverageAsync(o => (decimal?)o.TotalAmount) ?? 0,
                    DeliveredOrders = await deliveredOrders.CountAsync(),
                    CancelledOrders = await ordersQuery.CountAsync(o => o.Status == "Cancelled")
                };

                // Appointments
                var appointmentsQuery = _context.Appointments
                    .Where(a => !a.IsDeleted &&
                               a.AppointmentDate >= startDate &&
                               a.AppointmentDate <= endDate);

                var completedAppointments = appointmentsQuery.Where(a => a.Status == "Completed");

                var appointmentStats = new
                {
                    TotalAppointments = await appointmentsQuery.CountAsync(),
                    TotalServiceRevenue = await completedAppointments.SumAsync(a => a.TotalAmount),
                    AverageServiceValue = await completedAppointments.AverageAsync(a => (decimal?)a.TotalAmount) ?? 0,
                    CompletedAppointments = await completedAppointments.CountAsync(),
                    CancelledAppointments = await appointmentsQuery.CountAsync(a => a.Status == "Cancelled")
                };

                // Chart data
                var chartData = new List<SalesChartDataDto>();

                if (groupBy == "month")
                {
                    var ordersByMonth = await deliveredOrders
                        .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                        .Select(g => new
                        {
                            g.Key.Year,
                            g.Key.Month,
                            Revenue = g.Sum(o => o.TotalAmount),
                            Count = g.Count()
                        })
                        .ToListAsync();

                    var appointmentsByMonth = await completedAppointments
                        .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
                        .Select(g => new
                        {
                            g.Key.Year,
                            g.Key.Month,
                            Revenue = g.Sum(a => a.TotalAmount),
                            Count = g.Count()
                        })
                        .ToListAsync();

                    var allMonths = ordersByMonth.Select(o => new { o.Year, o.Month })
                        .Union(appointmentsByMonth.Select(a => new { a.Year, a.Month }))
                        .OrderBy(x => x.Year).ThenBy(x => x.Month);

                    foreach (var month in allMonths)
                    {
                        var orderData = ordersByMonth.FirstOrDefault(o => o.Year == month.Year && o.Month == month.Month);
                        var appointmentData = appointmentsByMonth.FirstOrDefault(a => a.Year == month.Year && a.Month == month.Month);

                        chartData.Add(new SalesChartDataDto
                        {
                            Label = $"{month.Month}/{month.Year}",
                            OrderRevenue = orderData?.Revenue ?? 0,
                            ServiceRevenue = appointmentData?.Revenue ?? 0,
                            OrderCount = orderData?.Count ?? 0,
                            AppointmentCount = appointmentData?.Count ?? 0
                        });
                    }
                }
                else
                {
                    var ordersByDay = await deliveredOrders
                        .GroupBy(o => o.CreatedAt.Date)
                        .Select(g => new
                        {
                            Date = g.Key,
                            Revenue = g.Sum(o => o.TotalAmount),
                            Count = g.Count()
                        })
                        .ToListAsync();

                    var appointmentsByDay = await completedAppointments
                        .GroupBy(a => a.AppointmentDate.Date)
                        .Select(g => new
                        {
                            Date = g.Key,
                            Revenue = g.Sum(a => a.TotalAmount),
                            Count = g.Count()
                        })
                        .ToListAsync();

                    for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                    {
                        var orderData = ordersByDay.FirstOrDefault(o => o.Date == date);
                        var appointmentData = appointmentsByDay.FirstOrDefault(a => a.Date == date);

                        chartData.Add(new SalesChartDataDto
                        {
                            Label = date.ToString("dd/MM"),
                            OrderRevenue = orderData?.Revenue ?? 0,
                            ServiceRevenue = appointmentData?.Revenue ?? 0,
                            OrderCount = orderData?.Count ?? 0,
                            AppointmentCount = appointmentData?.Count ?? 0
                        });
                    }
                }

                var report = new SalesReportDto
                {
                    FromDate = startDate,
                    ToDate = endDate,
                    TotalRevenue = orderStats.TotalRevenue + appointmentStats.TotalServiceRevenue,
                    OrderRevenue = orderStats.TotalRevenue,
                    ServiceRevenue = appointmentStats.TotalServiceRevenue,
                    TotalOrders = orderStats.TotalOrders,
                    DeliveredOrders = orderStats.DeliveredOrders,
                    CancelledOrders = orderStats.CancelledOrders,
                    AverageOrderValue = orderStats.AverageOrderValue,
                    TotalAppointments = appointmentStats.TotalAppointments,
                    CompletedAppointments = appointmentStats.CompletedAppointments,
                    CancelledAppointments = appointmentStats.CancelledAppointments,
                    AverageServiceValue = appointmentStats.AverageServiceValue,
                    ChartData = chartData
                };

                return Ok(ApiResponse<SalesReportDto>.SuccessResponse(report));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<SalesReportDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get product report
        /// </summary>
        [HttpGet("products")]
        public async Task<ActionResult<ApiResponse<ProductReportDto>>> GetProductReport(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int top = 10)
        {
            try
            {
                var startDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var endDate = toDate ?? DateTime.UtcNow;

                // Product stats
                var totalProducts = await _context.Products.CountAsync(p => !p.IsDeleted);
                var activeProducts = await _context.Products.CountAsync(p => !p.IsDeleted && p.IsActive);
                var lowStockProducts = await _context.Products.CountAsync(p => !p.IsDeleted && p.IsActive && p.StockQuantity <= p.LowStockThreshold);
                var outOfStockProducts = await _context.Products.CountAsync(p => !p.IsDeleted && p.IsActive && p.StockQuantity == 0);

                // Top selling products
                var topSellingProducts = await _context.OrderItems
                    .Include(i => i.Product)
                    .Include(i => i.Order)
                    .Where(i => i.Order != null && !i.Order.IsDeleted &&
                               (i.Order.Status == "Delivered" || i.Order.Status == "Completed") &&
                               i.Order.CreatedAt >= startDate &&
                               i.Order.CreatedAt <= endDate.AddDays(1))
                    .GroupBy(i => new { i.ProductId, ProductName = i.Product != null ? i.Product.Name : "", ProductSKU = i.Product != null ? i.Product.SKU : "" })
                    .Select(g => new TopProductDto
                    {
                        Id = g.Key.ProductId,
                        Name = g.Key.ProductName,
                        SoldCount = g.Sum(i => i.Quantity),
                        Revenue = g.Sum(i => i.TotalPrice)
                    })
                    .OrderByDescending(p => p.SoldCount)
                    .Take(top)
                    .ToListAsync();

                // Top revenue products
                var topRevenueProducts = await _context.OrderItems
                    .Include(i => i.Product)
                    .Include(i => i.Order)
                    .Where(i => i.Order != null && !i.Order.IsDeleted &&
                               (i.Order.Status == "Delivered" || i.Order.Status == "Completed") &&
                               i.Order.CreatedAt >= startDate &&
                               i.Order.CreatedAt <= endDate.AddDays(1))
                    .GroupBy(i => new { i.ProductId, ProductName = i.Product != null ? i.Product.Name : "", ProductSKU = i.Product != null ? i.Product.SKU : "" })
                    .Select(g => new TopProductDto
                    {
                        Id = g.Key.ProductId,
                        Name = g.Key.ProductName,
                        SoldCount = g.Sum(i => i.Quantity),
                        Revenue = g.Sum(i => i.TotalPrice)
                    })
                    .OrderByDescending(p => p.Revenue)
                    .Take(top)
                    .ToListAsync();

                // Category distribution
                var categoryStats = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => !p.IsDeleted && p.CategoryId != null)
                    .GroupBy(p => new { p.CategoryId, CategoryName = p.Category!.Name })
                    .Select(g => new CategoryStatsDto
                    {
                        CategoryId = g.Key.CategoryId ?? 0,
                        CategoryName = g.Key.CategoryName,
                        ProductCount = g.Count(),
                        TotalStock = g.Sum(p => p.StockQuantity)
                    })
                    .OrderByDescending(c => c.ProductCount)
                    .ToListAsync();

                var report = new ProductReportDto
                {
                    FromDate = startDate,
                    ToDate = endDate,
                    TotalProducts = totalProducts,
                    ActiveProducts = activeProducts,
                    LowStockProducts = lowStockProducts,
                    OutOfStockProducts = outOfStockProducts,
                    TopSellingProducts = topSellingProducts,
                    TopRevenueProducts = topRevenueProducts,
                    CategoryStats = categoryStats
                };

                return Ok(ApiResponse<ProductReportDto>.SuccessResponse(report));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductReportDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get customer report
        /// </summary>
        [HttpGet("customers")]
        public async Task<ActionResult<ApiResponse<CustomerReportDto>>> GetCustomerReport(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int top = 10)
        {
            try
            {
                var startDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var endDate = toDate ?? DateTime.UtcNow;

                var totalCustomers = await _context.Users.CountAsync();
                var newCustomers = await _context.Users.CountAsync(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate.AddDays(1));
                var activeCustomers = await _context.Users.CountAsync(u => u.IsActive);

                // Customers with orders in period
                var customersWithOrders = await _context.Orders
                    .Where(o => !o.IsDeleted && o.UserId != null &&
                               o.CreatedAt >= startDate &&
                               o.CreatedAt <= endDate.AddDays(1))
                    .Select(o => o.UserId)
                    .Distinct()
                    .CountAsync();

                // Top customers by order value
                var topCustomersByOrder = await _context.Orders
                    .Include(o => o.User)
                    .Where(o => !o.IsDeleted && o.UserId != null &&
                               (o.Status == "Delivered" || o.Status == "Completed") &&
                               o.CreatedAt >= startDate &&
                               o.CreatedAt <= endDate.AddDays(1))
                    .GroupBy(o => new { o.UserId, CustomerName = o.User != null ? (o.User.FullName ?? "Unknown") : "Unknown" })
                    .Select(g => new TopCustomerDto
                    {
                        CustomerId = g.Key.UserId!,
                        CustomerName = g.Key.CustomerName,
                        TotalOrders = g.Count(),
                        TotalSpent = g.Sum(o => o.TotalAmount)
                    })
                    .OrderByDescending(c => c.TotalSpent)
                    .Take(top)
                    .ToListAsync();

                // Top customers by appointments
                var topCustomersByAppointment = await _context.Appointments
                    .Include(a => a.User)
                    .Where(a => !a.IsDeleted && a.UserId != null &&
                               a.Status == "Completed" &&
                               a.AppointmentDate >= startDate &&
                               a.AppointmentDate <= endDate)
                    .GroupBy(a => new { a.UserId, CustomerName = a.User != null ? (a.User.FullName ?? "Unknown") : "Unknown" })
                    .Select(g => new TopCustomerDto
                    {
                        CustomerId = g.Key.UserId!,
                        CustomerName = g.Key.CustomerName,
                        TotalAppointments = g.Count(),
                        TotalSpent = g.Sum(a => a.TotalAmount)
                    })
                    .OrderByDescending(c => c.TotalSpent)
                    .Take(top)
                    .ToListAsync();

                // New customers by month
                var newCustomersByMonth = await _context.Users
                    .Where(u => u.CreatedAt >= startDate.AddMonths(-6))
                    .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
                    .Select(g => new NewCustomerChartDto
                    {
                        Month = $"{g.Key.Month}/{g.Key.Year}",
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Month)
                    .ToListAsync();

                var report = new CustomerReportDto
                {
                    FromDate = startDate,
                    ToDate = endDate,
                    TotalCustomers = totalCustomers,
                    NewCustomers = newCustomers,
                    ActiveCustomers = activeCustomers,
                    CustomersWithOrders = customersWithOrders,
                    TopCustomersByOrder = topCustomersByOrder,
                    TopCustomersByAppointment = topCustomersByAppointment,
                    NewCustomersByMonth = newCustomersByMonth
                };

                return Ok(ApiResponse<CustomerReportDto>.SuccessResponse(report));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CustomerReportDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get staff report
        /// </summary>
        [HttpGet("staff")]
        public async Task<ActionResult<ApiResponse<StaffReportDto>>> GetStaffReport(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int top = 10)
        {
            try
            {
                var startDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var endDate = toDate ?? DateTime.UtcNow;

                var totalStaff = await _context.Staff.CountAsync(s => !s.IsDeleted);
                var activeStaff = await _context.Staff.CountAsync(s => !s.IsDeleted && s.Status == "Active");
                var onLeaveStaff = await _context.Staff.CountAsync(s => !s.IsDeleted && s.Status == "OnLeave");

                // Staff performance
                var staffPerformance = await _context.Appointments
                    .Include(a => a.Staff)
                    .Where(a => !a.IsDeleted && a.StaffId != null &&
                               a.Status == "Completed" &&
                               a.AppointmentDate >= startDate &&
                               a.AppointmentDate <= endDate)
                    .GroupBy(a => new { a.StaffId, StaffName = a.Staff != null ? a.Staff.FullName : "Unknown" })
                    .Select(g => new StaffPerformanceDto
                    {
                        StaffId = g.Key.StaffId ?? 0,
                        StaffName = g.Key.StaffName,
                        CompletedAppointments = g.Count(),
                        TotalRevenue = g.Sum(a => a.TotalAmount),
                        AverageRating = 0 // Will be set from Staff entity
                    })
                    .OrderByDescending(s => s.TotalRevenue)
                    .Take(top)
                    .ToListAsync();

                // Appointments by staff
                var appointmentsByStaff = await _context.Appointments
                    .Include(a => a.Staff)
                    .Where(a => !a.IsDeleted && a.StaffId != null &&
                               a.AppointmentDate >= startDate &&
                               a.AppointmentDate <= endDate)
                    .GroupBy(a => new { a.StaffId, StaffName = a.Staff != null ? a.Staff.FullName : "Unknown" })
                    .Select(g => new StaffAppointmentStatsDto
                    {
                        StaffId = g.Key.StaffId ?? 0,
                        StaffName = g.Key.StaffName,
                        TotalAppointments = g.Count(),
                        CompletedAppointments = g.Count(a => a.Status == "Completed"),
                        CancelledAppointments = g.Count(a => a.Status == "Cancelled"),
                        NoShowAppointments = g.Count(a => a.Status == "NoShow")
                    })
                    .OrderByDescending(s => s.TotalAppointments)
                    .ToListAsync();

                var report = new StaffReportDto
                {
                    FromDate = startDate,
                    ToDate = endDate,
                    TotalStaff = totalStaff,
                    ActiveStaff = activeStaff,
                    OnLeaveStaff = onLeaveStaff,
                    StaffPerformance = staffPerformance,
                    AppointmentsByStaff = appointmentsByStaff
                };

                return Ok(ApiResponse<StaffReportDto>.SuccessResponse(report));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<StaffReportDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get service report
        /// </summary>
        [HttpGet("services")]
        public async Task<ActionResult<ApiResponse<ServiceReportDto>>> GetServiceReport(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int top = 10)
        {
            try
            {
                var startDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var endDate = toDate ?? DateTime.UtcNow;

                var totalServices = await _context.Services.CountAsync(s => !s.IsDeleted);
                var activeServices = await _context.Services.CountAsync(s => !s.IsDeleted && s.IsActive);

                // Top booked services
                var topBookedServices = await _context.AppointmentServices
                    .Include(s => s.Service)
                    .Include(s => s.Appointment)
                    .Where(s => s.Appointment != null && !s.Appointment.IsDeleted &&
                               s.Appointment.Status == "Completed" &&
                               s.Appointment.AppointmentDate >= startDate &&
                               s.Appointment.AppointmentDate <= endDate)
                    .GroupBy(s => new { s.ServiceId, ServiceName = s.Service != null ? s.Service.Name : "Unknown" })
                    .Select(g => new TopServiceDto
                    {
                        Id = g.Key.ServiceId,
                        Name = g.Key.ServiceName,
                        BookingCount = g.Count(),
                        Revenue = g.Sum(s => s.TotalPrice)
                    })
                    .OrderByDescending(s => s.BookingCount)
                    .Take(top)
                    .ToListAsync();

                // Revenue by service (removed category grouping since ServiceCategory is removed)
                var topServices = await _context.AppointmentServices
                    .Include(s => s.Service)
                    .Include(s => s.Appointment)
                    .Where(s => s.Appointment != null && !s.Appointment.IsDeleted &&
                               s.Appointment.Status == "Completed" &&
                               s.Service != null &&
                               s.Appointment.AppointmentDate >= startDate &&
                               s.Appointment.AppointmentDate <= endDate)
                    .GroupBy(s => new { s.Service!.Id, s.Service.Name })
                    .Select(g => new
                    {
                        ServiceId = g.Key.Id,
                        ServiceName = g.Key.Name,
                        BookingCount = g.Count(),
                        Revenue = g.Sum(s => s.TotalPrice)
                    })
                    .OrderByDescending(s => s.Revenue)
                    .Take(top)
                    .ToListAsync();

                var report = new ServiceReportDto
                {
                    FromDate = startDate,
                    ToDate = endDate,
                    TotalServices = totalServices,
                    ActiveServices = activeServices,
                    TotalServiceRevenue = topBookedServices.Sum(s => s.Revenue),
                    TotalBookings = topBookedServices.Sum(s => s.BookingCount),
                    TopBookedServices = topBookedServices
                };

                return Ok(ApiResponse<ServiceReportDto>.SuccessResponse(report));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceReportDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }
    }
}
