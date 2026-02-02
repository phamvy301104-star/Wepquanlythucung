using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Models;
using nhom6_admin.Models.Entities;
using nhom6_admin.Hubs;

namespace nhom6_admin.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAdminNotificationService _notificationService;

        public OrdersController(ApplicationDbContext context, IAdminNotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET: Admin/Orders
        public IActionResult Index(string? status)
        {
            ViewBag.StatusList = GetStatusList();
            ViewBag.CurrentStatus = status;
            return View();
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.StatusList = GetStatusList();
            return View(order);
        }

        // GET: Admin/Orders/GetData
        [HttpGet]
        public async Task<IActionResult> GetData(
            int draw, int start, int length,
            string? search, string? status, string? dateFrom, string? dateTo)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Where(o => !o.IsDeleted)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(o =>
                    o.OrderCode.ToLower().Contains(searchLower) ||
                    (o.User != null && o.User.FullName != null && o.User.FullName.ToLower().Contains(searchLower)) ||
                    (o.User != null && o.User.PhoneNumber != null && o.User.PhoneNumber.Contains(search)));
            }

            // Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            // Filter by date
            if (!string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out var fromDate))
            {
                query = query.Where(o => o.CreatedAt >= fromDate);
            }
            if (!string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out var toDate))
            {
                query = query.Where(o => o.CreatedAt <= toDate.AddDays(1));
            }

            var totalRecords = await _context.Orders.CountAsync(o => !o.IsDeleted);
            var filteredRecords = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip(start)
                .Take(length)
                .Select(o => new
                {
                    o.Id,
                    o.OrderCode,
                    CustomerName = o.User != null ? o.User.FullName : o.CustomerName,
                    CustomerPhone = o.User != null ? o.User.PhoneNumber : o.CustomerPhone,
                    o.TotalAmount,
                    o.Status,
                    PaymentMethod = o.PaymentMethodName,
                    o.PaymentStatus,
                    ItemCount = o.OrderItems != null ? o.OrderItems.Count : 0,
                    CreatedAt = o.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            return Json(new
            {
                draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = orders
            });
        }

        // POST: Admin/Orders/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status, string? note)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return Json(new { success = false, message = "Đơn hàng không tồn tại" });
            }

            var oldStatus = order.Status;
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            // Update payment status based on order status
            if (status == "Completed" && order.PaymentStatus != "Paid")
            {
                order.PaymentStatus = "Paid";
            }

            // Add status history if needed
            // You can implement OrderStatusHistory table here

            await _context.SaveChangesAsync();

            // Send real-time notification
            await _notificationService.NotifyOrderStatusChanged(new
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                Status = status,
                OldStatus = oldStatus
            });

            return Json(new { success = true, message = $"Đã cập nhật trạng thái từ {oldStatus} sang {status}" });
        }

        // POST: Admin/Orders/Cancel
        [HttpPost]
        public async Task<IActionResult> Cancel(int id, string? reason)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return Json(new { success = false, message = "Đơn hàng không tồn tại" });
            }

            if (order.Status == "Completed" || order.Status == "Shipping")
            {
                return Json(new { success = false, message = "Không thể hủy đơn hàng đã giao hoặc đang giao" });
            }

            order.Status = "Cancelled";
            order.CancellationReason = reason;
            order.CancelledAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            // Restore stock
            if (order.OrderItems != null)
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null)
                    {
                        item.Product.StockQuantity += item.Quantity;
                        item.Product.SoldCount -= item.Quantity;
                    }
                }
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã hủy đơn hàng" });
        }

        // GET: Admin/Orders/Print/5
        public async Task<IActionResult> Print(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Helper: Get status list
        private List<SelectListItem> GetStatusList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Pending", Text = "Chờ xử lý" },
                new SelectListItem { Value = "Confirmed", Text = "Đã xác nhận" },
                new SelectListItem { Value = "Processing", Text = "Đang xử lý" },
                new SelectListItem { Value = "Shipping", Text = "Đang giao" },
                new SelectListItem { Value = "Completed", Text = "Hoàn thành" },
                new SelectListItem { Value = "Cancelled", Text = "Đã hủy" }
            };
        }
    }
}
