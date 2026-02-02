using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Models;
using nhom6_admin.Services;

namespace nhom6_admin.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ExcelExportService _excelService;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
            _excelService = new ExcelExportService();
        }

        // GET: Reports/Revenue
        public IActionResult Revenue() => View();

        // GET: Reports/Sales (alias for Revenue)
        public IActionResult Sales() => RedirectToAction(nameof(Revenue));

        // GET: Reports/Products
        public IActionResult Products() => View();

        // GET: Reports/Staff
        public IActionResult Staff() => View();

        // API: Get Revenue Data
        [HttpGet]
        public async Task<IActionResult> GetRevenueData(string period = "month", DateTime? startDate = null, DateTime? endDate = null)
        {
            var end = endDate ?? DateTime.Now;
            var start = startDate ?? (period switch
            {
                "week" => end.AddDays(-7),
                "month" => end.AddMonths(-1),
                "year" => end.AddYears(-1),
                _ => end.AddMonths(-1)
            });

            // Orders Revenue
            var ordersQuery = _context.Orders
                .Where(o => !o.IsDeleted && o.Status != "Cancelled" && o.CreatedAt >= start && o.CreatedAt <= end);

            var ordersRevenue = await ordersQuery.SumAsync(o => o.TotalAmount);
            var ordersCount = await ordersQuery.CountAsync();

            // Appointments Revenue  
            var appointmentsQuery = _context.Appointments
                .Where(a => !a.IsDeleted && a.Status == "Completed" && a.CreatedAt >= start && a.CreatedAt <= end);

            var appointmentsRevenue = await appointmentsQuery.SumAsync(a => a.TotalAmount);
            var appointmentsCount = await appointmentsQuery.CountAsync();

            // Daily/Monthly breakdown
            var groupFormat = period == "year" ? "yyyy-MM" : "yyyy-MM-dd";
            
            var ordersByDate = await ordersQuery
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.TotalAmount), Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var appointmentsByDate = await appointmentsQuery
                .GroupBy(a => a.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.TotalAmount), Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Merge data
            var allDates = ordersByDate.Select(x => x.Date)
                .Union(appointmentsByDate.Select(x => x.Date))
                .OrderBy(x => x)
                .ToList();

            var chartData = allDates.Select(date => new
            {
                Date = date.ToString("dd/MM"),
                Orders = ordersByDate.FirstOrDefault(x => x.Date == date)?.Revenue ?? 0,
                Appointments = appointmentsByDate.FirstOrDefault(x => x.Date == date)?.Revenue ?? 0
            });

            // Payment methods breakdown
            var paymentMethods = await ordersQuery
                .GroupBy(o => o.PaymentMethodName ?? "COD")
                .Select(g => new { Method = g.Key, Amount = g.Sum(x => x.TotalAmount), Count = g.Count() })
                .ToListAsync();

            return Json(new
            {
                summary = new
                {
                    totalRevenue = ordersRevenue + appointmentsRevenue,
                    ordersRevenue,
                    ordersCount,
                    appointmentsRevenue,
                    appointmentsCount,
                    avgOrderValue = ordersCount > 0 ? ordersRevenue / ordersCount : 0
                },
                chartData,
                paymentMethods
            });
        }

        // API: Get Products Report
        [HttpGet]
        public async Task<IActionResult> GetProductsData(string type = "bestseller", int top = 10)
        {
            if (type == "bestseller")
            {
                var products = await _context.Products
                    .Where(p => !p.IsDeleted)
                    .OrderByDescending(p => p.SoldCount)
                    .Take(top)
                    .Select(p => new
                    {
                        p.Id, p.Name, p.ImageUrl, p.Price, p.SoldCount,
                        Revenue = p.SoldCount * p.Price,
                        CategoryName = p.Category != null ? p.Category.Name : "-"
                    })
                    .ToListAsync();
                return Json(new { data = products });
            }
            else if (type == "lowstock")
            {
                var products = await _context.Products
                    .Where(p => !p.IsDeleted && p.StockQuantity <= p.LowStockThreshold)
                    .OrderBy(p => p.StockQuantity)
                    .Take(top)
                    .Select(p => new
                    {
                        p.Id, p.Name, p.ImageUrl, p.Price, p.StockQuantity, p.LowStockThreshold,
                        CategoryName = p.Category != null ? p.Category.Name : "-"
                    })
                    .ToListAsync();
                return Json(new { data = products });
            }
            else // category
            {
                var categories = await _context.Categories
                    .Where(c => !c.IsDeleted)
                    .Select(c => new
                    {
                        c.Name,
                        ProductCount = c.Products!.Count(p => !p.IsDeleted),
                        TotalSold = c.Products!.Where(p => !p.IsDeleted).Sum(p => p.SoldCount),
                        Revenue = c.Products!.Where(p => !p.IsDeleted).Sum(p => p.SoldCount * p.Price)
                    })
                    .OrderByDescending(c => c.Revenue)
                    .ToListAsync();
                return Json(new { data = categories });
            }
        }

        // API: Get Staff Report
        [HttpGet]
        public async Task<IActionResult> GetStaffData(DateTime? startDate = null, DateTime? endDate = null)
        {
            var end = endDate ?? DateTime.Now;
            var start = startDate ?? end.AddMonths(-1);

            var staffData = await _context.Staff
                .Where(s => !s.IsDeleted)
                .Select(s => new
                {
                    s.Id, s.FullName, s.AvatarUrl, s.Position, s.Level,
                    s.AverageRating, s.TotalCustomersServed,
                    AppointmentsCount = s.Appointments!.Count(a => !a.IsDeleted && a.Status == "Completed" && a.CreatedAt >= start && a.CreatedAt <= end),
                    Revenue = s.Appointments!.Where(a => !a.IsDeleted && a.Status == "Completed" && a.CreatedAt >= start && a.CreatedAt <= end).Sum(a => a.TotalAmount)
                })
                .OrderByDescending(s => s.Revenue)
                .ToListAsync();

            var topPerformers = staffData.Take(5).ToList();
            var totalRevenue = staffData.Sum(s => s.Revenue);
            var totalAppointments = staffData.Sum(s => s.AppointmentsCount);

            return Json(new
            {
                summary = new { totalRevenue, totalAppointments, staffCount = staffData.Count },
                staffData,
                topPerformers
            });
        }

        // Export to Excel - Doanh thu
        [HttpGet]
        public async Task<IActionResult> ExportRevenue(string period = "month", DateTime? startDate = null, DateTime? endDate = null)
        {
            var end = endDate ?? DateTime.Now;
            var start = startDate ?? (period switch
            {
                "week" => end.AddDays(-7),
                "month" => end.AddMonths(-1),
                "year" => end.AddYears(-1),
                _ => end.AddMonths(-1)
            });

            // Get data same as GetRevenueData
            var ordersQuery = _context.Orders
                .Where(o => !o.IsDeleted && o.Status != "Cancelled" && o.CreatedAt >= start && o.CreatedAt <= end);

            var ordersRevenue = await ordersQuery.SumAsync(o => o.TotalAmount);
            var ordersCount = await ordersQuery.CountAsync();

            var appointmentsQuery = _context.Appointments
                .Where(a => !a.IsDeleted && a.Status == "Completed" && a.CreatedAt >= start && a.CreatedAt <= end);

            var appointmentsRevenue = await appointmentsQuery.SumAsync(a => a.TotalAmount);
            var appointmentsCount = await appointmentsQuery.CountAsync();

            var ordersByDate = await ordersQuery
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.TotalAmount), Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var appointmentsByDate = await appointmentsQuery
                .GroupBy(a => a.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.TotalAmount), Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var allDates = ordersByDate.Select(x => x.Date)
                .Union(appointmentsByDate.Select(x => x.Date))
                .OrderBy(x => x)
                .ToList();

            var chartData = allDates.Select(date => new
            {
                Date = date.ToString("dd/MM"),
                Orders = ordersByDate.FirstOrDefault(x => x.Date == date)?.Revenue ?? 0,
                Appointments = appointmentsByDate.FirstOrDefault(x => x.Date == date)?.Revenue ?? 0
            }).ToList();

            var paymentMethods = await ordersQuery
                .GroupBy(o => o.PaymentMethodName ?? "COD")
                .Select(g => new { Method = g.Key, Amount = g.Sum(x => x.TotalAmount), Count = g.Count() })
                .ToListAsync();

            var summary = new
            {
                totalRevenue = ordersRevenue + appointmentsRevenue,
                ordersRevenue,
                ordersCount,
                appointmentsRevenue,
                appointmentsCount,
                avgOrderValue = ordersCount > 0 ? ordersRevenue / ordersCount : 0
            };

            var fileBytes = _excelService.ExportRevenueReport(
                summary,
                chartData.Cast<dynamic>(),
                paymentMethods.Cast<dynamic>(),
                period,
                start,
                end
            );

            return File(fileBytes, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BaoCaoDoanhThu_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx");
        }

        // Export to Excel - Sản phẩm
        [HttpGet]
        public async Task<IActionResult> ExportProducts()
        {
            var bestsellers = await _context.Products
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.SoldCount)
                .Take(20)
                .Select(p => new
                {
                    p.Id, p.Name, p.ImageUrl, p.Price, p.SoldCount,
                    Revenue = p.SoldCount * p.Price,
                    CategoryName = p.Category != null ? p.Category.Name : "-"
                })
                .ToListAsync();

            var lowstock = await _context.Products
                .Where(p => !p.IsDeleted && p.StockQuantity <= p.LowStockThreshold)
                .OrderBy(p => p.StockQuantity)
                .Take(20)
                .Select(p => new
                {
                    p.Id, p.Name, p.ImageUrl, p.Price, p.StockQuantity, p.LowStockThreshold,
                    CategoryName = p.Category != null ? p.Category.Name : "-"
                })
                .ToListAsync();

            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => new
                {
                    c.Name,
                    ProductCount = c.Products!.Count(p => !p.IsDeleted),
                    TotalSold = c.Products!.Where(p => !p.IsDeleted).Sum(p => p.SoldCount),
                    Revenue = c.Products!.Where(p => !p.IsDeleted).Sum(p => p.SoldCount * p.Price)
                })
                .OrderByDescending(c => c.Revenue)
                .ToListAsync();

            var fileBytes = _excelService.ExportProductsReport(
                bestsellers.Cast<dynamic>(),
                lowstock.Cast<dynamic>(),
                categories.Cast<dynamic>()
            );

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BaoCaoSanPham_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // Export to Excel - Nhân viên
        [HttpGet]
        public async Task<IActionResult> ExportStaff(DateTime? startDate = null, DateTime? endDate = null)
        {
            var end = endDate ?? DateTime.Now;
            var start = startDate ?? end.AddMonths(-1);

            var staffData = await _context.Staff
                .Where(s => !s.IsDeleted)
                .Select(s => new
                {
                    s.Id, s.FullName, s.AvatarUrl, s.Position, s.Level,
                    s.AverageRating, s.TotalCustomersServed,
                    AppointmentsCount = s.Appointments!.Count(a => !a.IsDeleted && a.Status == "Completed" && a.CreatedAt >= start && a.CreatedAt <= end),
                    Revenue = s.Appointments!.Where(a => !a.IsDeleted && a.Status == "Completed" && a.CreatedAt >= start && a.CreatedAt <= end).Sum(a => a.TotalAmount)
                })
                .OrderByDescending(s => s.Revenue)
                .ToListAsync();

            var summary = new
            {
                totalRevenue = staffData.Sum(s => s.Revenue),
                totalAppointments = staffData.Sum(s => s.AppointmentsCount),
                staffCount = staffData.Count
            };

            var fileBytes = _excelService.ExportStaffReport(
                summary,
                staffData.Cast<dynamic>(),
                start,
                end
            );

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BaoCaoNhanVien_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx");
        }
    }
}
