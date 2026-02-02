using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Areas.Admin.Models;
using nhom6_admin.Models;

namespace nhom6_admin.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard";

            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);
            var last7Days = today.AddDays(-6);

            // Get statistics
            var viewModel = new DashboardViewModel
            {
                // Revenue this month
                TotalRevenueThisMonth = await _context.Orders
                    .Where(o => !o.IsDeleted && o.Status == "Completed" && 
                           o.CreatedAt >= startOfMonth)
                    .SumAsync(o => o.TotalAmount),

                // Revenue last month for comparison
                TotalRevenueLastMonth = await _context.Orders
                    .Where(o => !o.IsDeleted && o.Status == "Completed" && 
                           o.CreatedAt >= startOfLastMonth && o.CreatedAt < startOfMonth)
                    .SumAsync(o => o.TotalAmount),

                // Revenue today
                TotalRevenueToday = await _context.Orders
                    .Where(o => !o.IsDeleted && o.Status == "Completed" && 
                           o.CreatedAt.Date == today)
                    .SumAsync(o => o.TotalAmount),

                // Orders today
                OrdersToday = await _context.Orders
                    .CountAsync(o => !o.IsDeleted && o.CreatedAt.Date == today),

                // Pending orders
                PendingOrders = await _context.Orders
                    .CountAsync(o => !o.IsDeleted && o.Status == "Pending"),

                // Total orders this month
                TotalOrdersThisMonth = await _context.Orders
                    .CountAsync(o => !o.IsDeleted && o.CreatedAt >= startOfMonth),

                // Appointments today
                AppointmentsToday = await _context.Appointments
                    .CountAsync(a => !a.IsDeleted && a.AppointmentDate.Date == today),

                // Pending appointments
                PendingAppointments = await _context.Appointments
                    .CountAsync(a => !a.IsDeleted && a.Status == "Pending"),

                // Total appointments this month
                TotalAppointmentsThisMonth = await _context.Appointments
                    .CountAsync(a => !a.IsDeleted && a.AppointmentDate >= startOfMonth),

                // New customers this month
                NewCustomersThisMonth = await _context.Users
                    .CountAsync(u => u.CreatedAt >= startOfMonth),

                // Total customers
                TotalCustomers = await _context.Users.CountAsync(),

                // Total products
                TotalProducts = await _context.Products.CountAsync(p => !p.IsDeleted),

                // Low stock products (products with stock <= 10)
                LowStockProducts = await _context.Products
                    .CountAsync(p => !p.IsDeleted && p.StockQuantity <= 10),

                // Total services
                TotalServices = await _context.Services.CountAsync(s => !s.IsDeleted),

                // Active staff
                ActiveStaff = await _context.Staff.CountAsync(s => !s.IsDeleted && s.Status == "Active"),

                // Today's appointments list
                TodayAppointments = await _context.Appointments
                    .Include(a => a.Staff)
                    .Include(a => a.User)
                    .Include(a => a.AppointmentServices!)
                        .ThenInclude(aps => aps.Service)
                    .Where(a => !a.IsDeleted && a.AppointmentDate.Date == today)
                    .OrderBy(a => a.StartTime)
                    .Take(10)
                    .Select(a => new AppointmentSummary
                    {
                        Id = a.Id,
                        CustomerName = a.User != null ? (a.User.FullName ?? "Khách hàng") : (a.GuestName ?? "Khách vãng lai"),
                        CustomerPhone = a.User != null ? (a.User.PhoneNumber ?? "") : (a.GuestPhone ?? ""),
                        StaffName = a.Staff != null ? a.Staff.FullName : "Chưa phân công",
                        ServiceName = a.AppointmentServices != null && a.AppointmentServices.Any() 
                            ? a.AppointmentServices.First().Service!.Name : "N/A",
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        Status = a.Status,
                        TotalAmount = a.TotalAmount
                    })
                    .ToListAsync(),

                // Recent orders
                RecentOrders = await _context.Orders
                    .Where(o => !o.IsDeleted)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .Select(o => new OrderSummary
                    {
                        Id = o.Id,
                        OrderCode = o.OrderCode,
                        CustomerName = o.CustomerName ?? "Khách vãng lai",
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        PaymentStatus = o.PaymentStatus,
                        CreatedAt = o.CreatedAt
                    })
                    .ToListAsync(),

                // Order status chart data
                OrderStatusChart = new OrderStatusChartData
                {
                    Pending = await _context.Orders.CountAsync(o => !o.IsDeleted && o.Status == "Pending"),
                    Confirmed = await _context.Orders.CountAsync(o => !o.IsDeleted && o.Status == "Confirmed"),
                    Processing = await _context.Orders.CountAsync(o => !o.IsDeleted && o.Status == "Processing"),
                    Shipping = await _context.Orders.CountAsync(o => !o.IsDeleted && o.Status == "Shipping"),
                    Completed = await _context.Orders.CountAsync(o => !o.IsDeleted && o.Status == "Completed"),
                    Cancelled = await _context.Orders.CountAsync(o => !o.IsDeleted && o.Status == "Cancelled")
                }
            };

            // Get revenue chart data for last 7 days
            viewModel.RevenueChartData = await GetRevenueChartData(last7Days, today);

            // Get top products
            viewModel.TopProducts = await GetTopProducts(startOfMonth);

            // Get top services
            viewModel.TopServices = await GetTopServices(startOfMonth);

            // Calculate revenue growth percentage
            if (viewModel.TotalRevenueLastMonth > 0)
            {
                viewModel.RevenueGrowthPercent = (double)Math.Round(
                    ((viewModel.TotalRevenueThisMonth - viewModel.TotalRevenueLastMonth) / viewModel.TotalRevenueLastMonth) * 100, 1);
            }

            return View(viewModel);
        }

        private async Task<List<RevenueChartData>> GetRevenueChartData(DateTime startDate, DateTime endDate)
        {
            var chartData = new List<RevenueChartData>();
            var dayNames = new[] { "CN", "T2", "T3", "T4", "T5", "T6", "T7" };

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var orderRevenue = await _context.Orders
                    .Where(o => !o.IsDeleted && o.Status == "Completed" && o.CreatedAt.Date == date)
                    .SumAsync(o => o.TotalAmount);

                var serviceRevenue = await _context.Appointments
                    .Where(a => !a.IsDeleted && a.Status == "Completed" && a.AppointmentDate.Date == date)
                    .SumAsync(a => a.TotalAmount);

                chartData.Add(new RevenueChartData
                {
                    Label = $"{dayNames[(int)date.DayOfWeek]} ({date:dd/MM})",
                    OrderRevenue = orderRevenue,
                    ServiceRevenue = serviceRevenue,
                    TotalRevenue = orderRevenue + serviceRevenue
                });
            }

            return chartData;
        }

        private async Task<List<TopProductDto>> GetTopProducts(DateTime startDate)
        {
            return await _context.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => oi.Order != null && !oi.Order.IsDeleted && 
                       oi.Order.Status == "Completed" && oi.Order.CreatedAt >= startDate &&
                       oi.Product != null && !oi.Product.IsDeleted)
                .GroupBy(oi => new { oi.ProductId, oi.Product!.Name, oi.Product.ImageUrl })
                .Select(g => new TopProductDto
                {
                    Id = g.Key.ProductId,
                    Name = g.Key.Name,
                    ImageUrl = g.Key.ImageUrl,
                    TotalSold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync();
        }

        private async Task<List<TopServiceDto>> GetTopServices(DateTime startDate)
        {
            return await _context.AppointmentServices
                .Include(aps => aps.Service)
                .Include(aps => aps.Appointment)
                .Include(aps => aps.Service)
                .Where(aps => aps.Appointment != null && !aps.Appointment.IsDeleted &&
                       aps.Appointment.Status == "Completed" && aps.Appointment.AppointmentDate >= startDate &&
                       aps.Service != null && !aps.Service.IsDeleted)
                .GroupBy(aps => new { aps.ServiceId, aps.Service!.Name, aps.Service.ImageUrl, aps.Service.Price })
                .Select(g => new TopServiceDto
                {
                    Id = g.Key.ServiceId,
                    Name = g.Key.Name,
                    ImageUrl = g.Key.ImageUrl,
                    TotalBookings = g.Count(),
                    Revenue = g.Sum(x => x.Quantity) * g.Key.Price
                })
                .OrderByDescending(x => x.TotalBookings)
                .Take(5)
                .ToListAsync();
        }
    }
}
