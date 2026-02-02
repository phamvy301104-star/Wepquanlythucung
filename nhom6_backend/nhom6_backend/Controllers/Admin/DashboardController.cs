using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_backend.Models;
using nhom6_backend.Models.DTOs.Admin;

namespace nhom6_backend.Controllers.Admin
{
    /// <summary>
    /// Admin Dashboard Controller
    /// Cung cấp dữ liệu thống kê cho trang Dashboard admin
    /// </summary>
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy tất cả thống kê cho Dashboard
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<AdminApiResponse<DashboardStatsDto>>> GetDashboardStats()
        {
            try
            {
                var now = DateTime.UtcNow;
                var today = now.Date;
                var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                var startOfMonth = new DateTime(today.Year, today.Month, 1);

                // Revenue Stats
                var completedOrders = await _context.Orders
                    .Where(o => o.Status == "Completed" || o.Status == "Delivered")
                    .ToListAsync();

                var totalRevenue = completedOrders.Sum(o => o.TotalAmount);
                var revenueToday = completedOrders.Where(o => o.CreatedAt.Date == today).Sum(o => o.TotalAmount);
                var revenueThisWeek = completedOrders.Where(o => o.CreatedAt >= startOfWeek).Sum(o => o.TotalAmount);
                var revenueThisMonth = completedOrders.Where(o => o.CreatedAt >= startOfMonth).Sum(o => o.TotalAmount);

                // Order Stats
                var totalOrders = await _context.Orders.CountAsync();
                var ordersToday = await _context.Orders.CountAsync(o => o.CreatedAt.Date == today);
                var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
                var processingOrders = await _context.Orders.CountAsync(o => o.Status == "Processing" || o.Status == "Confirmed");
                var shippingOrders = await _context.Orders.CountAsync(o => o.Status == "Shipping");
                var orderCompleted = await _context.Orders.CountAsync(o => o.Status == "Completed" || o.Status == "Delivered");
                var cancelledOrders = await _context.Orders.CountAsync(o => o.Status == "Cancelled");

                // Customer Stats
                var totalCustomers = await _context.Users.CountAsync();
                var newCustomersToday = await _context.Users.CountAsync(u => u.CreatedAt.Date == today);
                var newCustomersThisWeek = await _context.Users.CountAsync(u => u.CreatedAt >= startOfWeek);
                var newCustomersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= startOfMonth);

                // Appointment Stats
                var totalAppointments = await _context.Appointments.CountAsync();
                var appointmentsToday = await _context.Appointments.CountAsync(a => a.AppointmentDate.Date == today);
                var pendingAppointments = await _context.Appointments.CountAsync(a => a.Status == "Pending");

                // Product Stats
                var totalProducts = await _context.Products.CountAsync();
                var activeProducts = await _context.Products.CountAsync(p => p.IsActive);
                var outOfStockProducts = await _context.Products.CountAsync(p => p.StockQuantity <= 0);

                // Service Stats
                var totalServices = await _context.Services.CountAsync();
                var activeServices = await _context.Services.CountAsync(s => s.IsActive);

                var stats = new DashboardStatsDto
                {
                    TotalRevenue = totalRevenue,
                    RevenueToday = revenueToday,
                    RevenueThisWeek = revenueThisWeek,
                    RevenueThisMonth = revenueThisMonth,
                    TotalOrders = totalOrders,
                    OrdersToday = ordersToday,
                    PendingOrders = pendingOrders,
                    ProcessingOrders = processingOrders,
                    ShippingOrders = shippingOrders,
                    CompletedOrders = orderCompleted,
                    CancelledOrders = cancelledOrders,
                    TotalCustomers = totalCustomers,
                    NewCustomersToday = newCustomersToday,
                    NewCustomersThisWeek = newCustomersThisWeek,
                    NewCustomersThisMonth = newCustomersThisMonth,
                    TotalAppointments = totalAppointments,
                    AppointmentsToday = appointmentsToday,
                    PendingAppointments = pendingAppointments,
                    TotalProducts = totalProducts,
                    ActiveProducts = activeProducts,
                    OutOfStockProducts = outOfStockProducts,
                    TotalServices = totalServices,
                    ActiveServices = activeServices,
                    TopSellingProducts = await GetTopSellingProducts(10),
                    TopServices = await GetTopServices(10),
                    LoyalCustomers = await GetLoyalCustomers(10),
                    RecentOrders = await GetRecentOrders(10),
                    RevenueChart = await GetRevenueChartData(7)
                };

                return Ok(AdminApiResponse<DashboardStatsDto>.Ok(stats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return StatusCode(500, AdminApiResponse<DashboardStatsDto>.Fail("Lỗi server khi lấy thống kê"));
            }
        }

        /// <summary>
        /// Lấy thống kê nhanh cho widget cards
        /// </summary>
        [HttpGet("quick-stats")]
        public async Task<ActionResult<AdminApiResponse<object>>> GetQuickStats()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                var stats = new
                {
                    TodayOrders = await _context.Orders.CountAsync(o => o.CreatedAt.Date == today),
                    TodayRevenue = await _context.Orders
                        .Where(o => o.CreatedAt.Date == today && (o.Status == "Completed" || o.Status == "Delivered"))
                        .SumAsync(o => o.TotalAmount),
                    TodayAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate.Date == today),
                    PendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending"),
                    PendingAppointments = await _context.Appointments.CountAsync(a => a.Status == "Pending"),
                    LowStockProducts = await _context.Products.CountAsync(p => p.StockQuantity > 0 && p.StockQuantity <= p.LowStockThreshold),
                    MonthRevenue = await _context.Orders
                        .Where(o => o.CreatedAt >= thisMonth && (o.Status == "Completed" || o.Status == "Delivered"))
                        .SumAsync(o => o.TotalAmount)
                };

                return Ok(AdminApiResponse<object>.Ok(stats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quick stats");
                return StatusCode(500, AdminApiResponse<object>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Lấy dữ liệu biểu đồ doanh thu
        /// </summary>
        [HttpGet("revenue-chart")]
        public async Task<ActionResult<AdminApiResponse<List<RevenueChartDataDto>>>> GetRevenueChart([FromQuery] int days = 7)
        {
            try
            {
                var chartData = await GetRevenueChartData(days);
                return Ok(AdminApiResponse<List<RevenueChartDataDto>>.Ok(chartData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue chart data");
                return StatusCode(500, AdminApiResponse<List<RevenueChartDataDto>>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Lấy top sản phẩm bán chạy
        /// </summary>
        [HttpGet("top-products")]
        public async Task<ActionResult<AdminApiResponse<List<TopSellingProductDto>>>> GetTopProductsEndpoint([FromQuery] int count = 10)
        {
            try
            {
                var products = await GetTopSellingProducts(count);
                return Ok(AdminApiResponse<List<TopSellingProductDto>>.Ok(products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top products");
                return StatusCode(500, AdminApiResponse<List<TopSellingProductDto>>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Lấy top dịch vụ được đặt nhiều
        /// </summary>
        [HttpGet("top-services")]
        public async Task<ActionResult<AdminApiResponse<List<TopServiceDto>>>> GetTopServicesEndpoint([FromQuery] int count = 10)
        {
            try
            {
                var services = await GetTopServices(count);
                return Ok(AdminApiResponse<List<TopServiceDto>>.Ok(services));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top services");
                return StatusCode(500, AdminApiResponse<List<TopServiceDto>>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Lấy khách hàng thân thiết
        /// </summary>
        [HttpGet("loyal-customers")]
        public async Task<ActionResult<AdminApiResponse<List<LoyalCustomerDto>>>> GetLoyalCustomersEndpoint([FromQuery] int count = 10)
        {
            try
            {
                var customers = await GetLoyalCustomers(count);
                return Ok(AdminApiResponse<List<LoyalCustomerDto>>.Ok(customers));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loyal customers");
                return StatusCode(500, AdminApiResponse<List<LoyalCustomerDto>>.Fail("Lỗi server"));
            }
        }

        #region Helper Methods

        private async Task<List<TopSellingProductDto>> GetTopSellingProducts(int count)
        {
            return await _context.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => oi.Order != null && (oi.Order.Status == "Completed" || oi.Order.Status == "Delivered"))
                .GroupBy(oi => new { oi.ProductId, oi.Product!.Name, oi.Product.ImageUrl })
                .Select(g => new TopSellingProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name ?? "N/A",
                    ImageUrl = g.Key.ImageUrl,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(count)
                .ToListAsync();
        }

        private async Task<List<TopServiceDto>> GetTopServices(int count)
        {
            return await _context.AppointmentServices
                .Include(s => s.Service)
                .Where(s => s.Appointment != null && s.Appointment.Status == "Completed")
                .GroupBy(s => new { s.ServiceId, s.Service!.Name, s.Service.ImageUrl })
                .Select(g => new TopServiceDto
                {
                    ServiceId = g.Key.ServiceId,
                    ServiceName = g.Key.Name ?? "N/A",
                    ImageUrl = g.Key.ImageUrl,
                    BookingCount = g.Count(),
                    Revenue = g.Sum(s => s.Price)
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(count)
                .ToListAsync();
        }

        private async Task<List<LoyalCustomerDto>> GetLoyalCustomers(int count)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Where(o => o.UserId != null && (o.Status == "Completed" || o.Status == "Delivered"))
                .GroupBy(o => new { o.UserId, o.User!.FullName, o.User.Email, o.User.AvatarUrl })
                .Select(g => new LoyalCustomerDto
                {
                    UserId = g.Key.UserId ?? "",
                    FullName = g.Key.FullName ?? "N/A",
                    Email = g.Key.Email,
                    AvatarUrl = g.Key.AvatarUrl,
                    OrderCount = g.Count(),
                    TotalSpent = g.Sum(o => o.TotalAmount),
                    LastOrderDate = g.Max(o => o.CreatedAt)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(count)
                .ToListAsync();
        }

        private async Task<List<RecentOrderDto>> GetRecentOrders(int count)
        {
            return await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(count)
                .Select(o => new RecentOrderDto
                {
                    OrderId = o.Id,
                    OrderCode = o.OrderCode,
                    CustomerName = o.CustomerName,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();
        }

        private async Task<List<RevenueChartDataDto>> GetRevenueChartData(int days)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);
            var orders = await _context.Orders
                .Where(o => o.CreatedAt >= startDate && (o.Status == "Completed" || o.Status == "Delivered"))
                .ToListAsync();

            var chartData = new List<RevenueChartDataDto>();
            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var dayOrders = orders.Where(o => o.CreatedAt.Date == date);
                chartData.Add(new RevenueChartDataDto
                {
                    Label = date.ToString("dd/MM"),
                    Revenue = dayOrders.Sum(o => o.TotalAmount),
                    OrderCount = dayOrders.Count()
                });
            }

            return chartData;
        }

        #endregion
    }
}
