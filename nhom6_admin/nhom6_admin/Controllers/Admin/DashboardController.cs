using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Models;
using nhom6_admin.Models.DTOs;

namespace nhom6_admin.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetStats()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

                var stats = new DashboardStatsDto
                {
                    TotalUsers = await _context.Users.CountAsync(),
                    TotalProducts = await _context.Products.CountAsync(p => !p.IsDeleted),
                    TotalOrders = await _context.Orders.CountAsync(o => !o.IsDeleted),
                    TotalAppointments = await _context.Appointments.CountAsync(a => !a.IsDeleted),

                    TotalRevenue = await _context.Orders
                        .Where(o => o.Status == "Completed" && !o.IsDeleted)
                        .SumAsync(o => o.TotalAmount),

                    TodayRevenue = await _context.Orders
                        .Where(o => o.Status == "Completed" && !o.IsDeleted && o.CreatedAt.Date == today)
                        .SumAsync(o => o.TotalAmount),

                    MonthlyRevenue = await _context.Orders
                        .Where(o => o.Status == "Completed" && !o.IsDeleted && o.CreatedAt >= firstDayOfMonth)
                        .SumAsync(o => o.TotalAmount),

                    PendingOrders = await _context.Orders
                        .CountAsync(o => o.Status == "Pending" && !o.IsDeleted),

                    TodayAppointments = await _context.Appointments
                        .CountAsync(a => a.AppointmentDate.Date == today && !a.IsDeleted),

                    LowStockProducts = await _context.Products
                        .CountAsync(p => p.StockQuantity <= p.LowStockThreshold && !p.IsDeleted && p.IsActive),

                    NewUsersToday = await _context.Users
                        .CountAsync(u => u.CreatedAt.Date == today),

                    NewUsersThisMonth = await _context.Users
                        .CountAsync(u => u.CreatedAt >= firstDayOfMonth)
                };

                return Ok(ApiResponse<DashboardStatsDto>.SuccessResponse(stats));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<DashboardStatsDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get revenue chart data
        /// </summary>
        [HttpGet("revenue-chart")]
        public async Task<ActionResult<ApiResponse<List<RevenueChartDto>>>> GetRevenueChart(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string groupBy = "day")
        {
            try
            {
                var startDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var endDate = toDate ?? DateTime.UtcNow;

                var orders = await _context.Orders
                    .Where(o => o.Status == "Completed" && !o.IsDeleted)
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                    .ToListAsync();

                var chartData = new List<RevenueChartDto>();

                if (groupBy == "day")
                {
                    chartData = orders
                        .GroupBy(o => o.CreatedAt.Date)
                        .OrderBy(g => g.Key)
                        .Select(g => new RevenueChartDto
                        {
                            Label = g.Key.ToString("dd/MM"),
                            Revenue = g.Sum(o => o.TotalAmount),
                            OrderCount = g.Count()
                        })
                        .ToList();
                }
                else if (groupBy == "month")
                {
                    chartData = orders
                        .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                        .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                        .Select(g => new RevenueChartDto
                        {
                            Label = $"{g.Key.Month:D2}/{g.Key.Year}",
                            Revenue = g.Sum(o => o.TotalAmount),
                            OrderCount = g.Count()
                        })
                        .ToList();
                }

                return Ok(ApiResponse<List<RevenueChartDto>>.SuccessResponse(chartData));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<RevenueChartDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get top selling products
        /// </summary>
        [HttpGet("top-products")]
        public async Task<ActionResult<ApiResponse<List<TopProductDto>>>> GetTopProducts([FromQuery] int top = 10)
        {
            try
            {
                var topProducts = await _context.Products
                    .Where(p => !p.IsDeleted && p.IsActive)
                    .OrderByDescending(p => p.SoldCount)
                    .Take(top)
                    .Select(p => new TopProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        ImageUrl = p.ImageUrl,
                        SoldCount = p.SoldCount,
                        Revenue = p.SoldCount * p.Price
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<TopProductDto>>.SuccessResponse(topProducts));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<TopProductDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get top services
        /// </summary>
        [HttpGet("top-services")]
        public async Task<ActionResult<ApiResponse<List<TopServiceDto>>>> GetTopServices([FromQuery] int top = 10)
        {
            try
            {
                var topServices = await _context.Services
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .OrderByDescending(s => s.TotalBookings)
                    .Take(top)
                    .Select(s => new TopServiceDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        ImageUrl = s.ImageUrl,
                        BookingCount = s.TotalBookings,
                        Revenue = s.TotalBookings * s.Price
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<TopServiceDto>>.SuccessResponse(topServices));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<TopServiceDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get recent orders
        /// </summary>
        [HttpGet("recent-orders")]
        public async Task<ActionResult<ApiResponse<List<RecentOrderDto>>>> GetRecentOrders([FromQuery] int count = 10)
        {
            try
            {
                var recentOrders = await _context.Orders
                    .Where(o => !o.IsDeleted)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(count)
                    .Select(o => new RecentOrderDto
                    {
                        Id = o.Id,
                        OrderCode = o.OrderCode,
                        CustomerName = o.CustomerName,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        CreatedAt = o.CreatedAt
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<RecentOrderDto>>.SuccessResponse(recentOrders));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<RecentOrderDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get recent appointments
        /// </summary>
        [HttpGet("recent-appointments")]
        public async Task<ActionResult<ApiResponse<List<RecentAppointmentDto>>>> GetRecentAppointments([FromQuery] int count = 10)
        {
            try
            {
                var recentAppointments = await _context.Appointments
                    .Include(a => a.Staff)
                    .Include(a => a.User)
                    .Where(a => !a.IsDeleted)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(count)
                    .Select(a => new RecentAppointmentDto
                    {
                        Id = a.Id,
                        AppointmentCode = a.AppointmentCode,
                        CustomerName = a.User != null ? (a.User.FullName ?? a.User.Email ?? "") : (a.GuestName ?? ""),
                        StaffName = a.Staff != null ? a.Staff.FullName : null,
                        AppointmentDate = a.AppointmentDate,
                        StartTime = a.StartTime,
                        Status = a.Status
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<RecentAppointmentDto>>.SuccessResponse(recentAppointments));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<RecentAppointmentDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get today's appointments
        /// </summary>
        [HttpGet("today-appointments")]
        public async Task<ActionResult<ApiResponse<List<RecentAppointmentDto>>>> GetTodayAppointments()
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                var appointments = await _context.Appointments
                    .Include(a => a.Staff)
                    .Include(a => a.User)
                    .Where(a => !a.IsDeleted && a.AppointmentDate.Date == today)
                    .OrderBy(a => a.StartTime)
                    .Select(a => new RecentAppointmentDto
                    {
                        Id = a.Id,
                        AppointmentCode = a.AppointmentCode,
                        CustomerName = a.User != null ? (a.User.FullName ?? a.User.Email ?? "") : (a.GuestName ?? ""),
                        StaffName = a.Staff != null ? a.Staff.FullName : null,
                        AppointmentDate = a.AppointmentDate,
                        StartTime = a.StartTime,
                        Status = a.Status
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<RecentAppointmentDto>>.SuccessResponse(appointments));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<RecentAppointmentDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get order status summary
        /// </summary>
        [HttpGet("order-status-summary")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, int>>>> GetOrderStatusSummary()
        {
            try
            {
                var summary = await _context.Orders
                    .Where(o => !o.IsDeleted)
                    .GroupBy(o => o.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Status, x => x.Count);

                return Ok(ApiResponse<Dictionary<string, int>>.SuccessResponse(summary));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<Dictionary<string, int>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get appointment status summary
        /// </summary>
        [HttpGet("appointment-status-summary")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, int>>>> GetAppointmentStatusSummary()
        {
            try
            {
                var summary = await _context.Appointments
                    .Where(a => !a.IsDeleted)
                    .GroupBy(a => a.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Status, x => x.Count);

                return Ok(ApiResponse<Dictionary<string, int>>.SuccessResponse(summary));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<Dictionary<string, int>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }
    }
}
