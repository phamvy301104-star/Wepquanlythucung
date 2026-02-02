using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_backend.Models;
using nhom6_backend.Models.DTOs.Admin;
using nhom6_backend.Models.Entities;
using System.Text;

namespace nhom6_backend.Controllers.Admin
{
    /// <summary>
    /// Admin Order Management Controller
    /// Quản lý đơn hàng: list, filter, update status, export
    /// </summary>
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(ApplicationDbContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách đơn hàng với filter và phân trang
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<AdminApiResponse<PagedResult<OrderListDto>>>> GetOrders(
            [FromQuery] OrderFilterRequest filter)
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                    .AsQueryable();

                // Filter by status
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    query = query.Where(o => o.Status == filter.Status);
                }

                // Filter by date range
                if (filter.FromDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);
                }
                if (filter.ToDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt <= filter.ToDate.Value);
                }

                // Filter by payment status
                if (!string.IsNullOrEmpty(filter.PaymentStatus))
                {
                    query = query.Where(o => o.PaymentStatus == filter.PaymentStatus);
                }

                // Search by order code, customer name, phone, email
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    var search = filter.SearchTerm.ToLower();
                    query = query.Where(o =>
                        o.OrderCode.ToLower().Contains(search) ||
                        o.CustomerName.ToLower().Contains(search) ||
                        o.CustomerPhone.Contains(search) ||
                        (o.CustomerEmail != null && o.CustomerEmail.ToLower().Contains(search)));
                }

                var totalCount = await query.CountAsync();

                // Sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "total" => filter.SortDesc ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount),
                    "status" => filter.SortDesc ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
                    _ => filter.SortDesc ? query.OrderBy(o => o.CreatedAt) : query.OrderByDescending(o => o.CreatedAt)
                };

                var orders = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(o => new OrderListDto
                    {
                        Id = o.Id,
                        OrderCode = o.OrderCode,
                        CustomerName = o.CustomerName,
                        CustomerPhone = o.CustomerPhone,
                        CustomerEmail = o.CustomerEmail,
                        SubTotal = o.SubTotal,
                        ShippingFee = o.ShippingFee,
                        DiscountAmount = o.DiscountAmount,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        PaymentStatus = o.PaymentStatus,
                        PaymentMethodName = o.PaymentMethodName,
                        ItemCount = o.OrderItems != null ? o.OrderItems.Count : 0,
                        CreatedAt = o.CreatedAt
                    })
                    .ToListAsync();

                var result = new PagedResult<OrderListDto>
                {
                    Items = orders,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

                return Ok(AdminApiResponse<PagedResult<OrderListDto>>.Ok(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders");
                return StatusCode(500, AdminApiResponse<PagedResult<OrderListDto>>.Fail("Lỗi server khi lấy danh sách đơn hàng"));
            }
        }

        /// <summary>
        /// Lấy chi tiết một đơn hàng
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminApiResponse<OrderDetailDto>>> GetOrderDetail(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems!)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.Coupon)
                    .Include(o => o.StatusHistories)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                    return NotFound(AdminApiResponse<OrderDetailDto>.Fail("Đơn hàng không tồn tại"));

                var dto = new OrderDetailDto
                {
                    Id = order.Id,
                    OrderCode = order.OrderCode,
                    CustomerName = order.CustomerName,
                    CustomerPhone = order.CustomerPhone,
                    CustomerEmail = order.CustomerEmail,
                    SubTotal = order.SubTotal,
                    ShippingFee = order.ShippingFee,
                    DiscountAmount = order.DiscountAmount,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    PaymentStatus = order.PaymentStatus,
                    PaymentMethodName = order.PaymentMethodName,
                    ItemCount = order.OrderItems?.Count ?? 0,
                    CreatedAt = order.CreatedAt,
                    UserId = order.UserId,
                    ShippingAddressText = order.ShippingAddressText,
                    ReceiverName = order.ReceiverName,
                    ReceiverPhone = order.ReceiverPhone,
                    TrackingNumber = order.TrackingNumber,
                    ShippingMethodName = order.ShippingMethodName,
                    CouponCode = order.CouponCode,
                    CustomerNotes = order.CustomerNotes,
                    InternalNotes = order.InternalNotes,
                    ConfirmedAt = order.ConfirmedAt,
                    ShippedAt = order.ShippedAt,
                    DeliveredAt = order.DeliveredAt,
                    CancelledAt = order.CancelledAt,
                    CancelReason = order.CancellationReason,
                    Items = order.OrderItems?.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName,
                        ProductImage = oi.ProductImageUrl,
                        SKU = oi.SKU,
                        UnitPrice = oi.UnitPrice,
                        Quantity = oi.Quantity,
                        TotalPrice = oi.TotalPrice
                    }).ToList() ?? new List<OrderItemDto>(),
                    StatusHistory = order.StatusHistories?.Select(h => new OrderStatusHistoryDto
                    {
                        Status = h.ToStatus ?? "",
                        Notes = h.Notes,
                        ChangedBy = h.ChangedByUserId,
                        ChangedAt = h.CreatedAt
                    }).ToList() ?? new List<OrderStatusHistoryDto>()
                };

                return Ok(AdminApiResponse<OrderDetailDto>.Ok(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order detail for {OrderId}", id);
                return StatusCode(500, AdminApiResponse<OrderDetailDto>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<ActionResult<AdminApiResponse<bool>>> UpdateOrderStatus(
            int id,
            [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id);
                    
                if (order == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Đơn hàng không tồn tại"));

                var validStatuses = new[] { "Pending", "Confirmed", "Processing", "Shipping", "Delivered", "Completed", "Cancelled", "Returned" };
                if (!validStatuses.Contains(request.Status))
                    return BadRequest(AdminApiResponse<bool>.Fail($"Trạng thái không hợp lệ. Các trạng thái: {string.Join(", ", validStatuses)}"));

                var oldStatus = order.Status;
                order.Status = request.Status;
                order.UpdatedAt = DateTime.UtcNow;

                // Update timestamps based on status
                switch (request.Status)
                {
                    case "Confirmed":
                        order.ConfirmedAt = DateTime.UtcNow;
                        break;
                    case "Shipping":
                        order.ShippedAt = DateTime.UtcNow;
                        if (!string.IsNullOrEmpty(request.TrackingNumber))
                            order.TrackingNumber = request.TrackingNumber;
                        break;
                    case "Delivered":
                        order.DeliveredAt = DateTime.UtcNow;
                        break;
                    case "Completed":
                        order.CompletedAt = DateTime.UtcNow;
                        if (order.PaymentMethodName == "COD")
                            order.PaymentStatus = "Paid";
                        break;
                    case "Cancelled":
                        order.CancelledAt = DateTime.UtcNow;
                        order.CancellationReason = request.Notes;
                        order.CancelledBy = "Admin";
                        // Restore stock
                        if (order.OrderItems != null)
                        {
                            foreach (var item in order.OrderItems)
                            {
                                var product = await _context.Products.FindAsync(item.ProductId);
                                if (product != null)
                                    product.StockQuantity += item.Quantity;
                            }
                        }
                        break;
                }

                // Add status history
                var statusHistory = new OrderStatusHistory
                {
                    OrderId = id,
                    FromStatus = oldStatus,
                    ToStatus = request.Status,
                    Notes = request.Notes,
                    ChangedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    CreatedAt = DateTime.UtcNow
                };
                _context.OrderStatusHistories.Add(statusHistory);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} status updated from {OldStatus} to {NewStatus}", id, oldStatus, request.Status);

                return Ok(AdminApiResponse<bool>.Ok(true, $"Đã cập nhật trạng thái thành {request.Status}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for {OrderId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server khi cập nhật trạng thái"));
            }
        }

        /// <summary>
        /// Cập nhật trạng thái thanh toán
        /// </summary>
        [HttpPut("{id}/payment-status")]
        public async Task<ActionResult<AdminApiResponse<bool>>> UpdatePaymentStatus(
            int id,
            [FromBody] UpdatePaymentStatusDto request)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Đơn hàng không tồn tại"));

                var validStatuses = new[] { "Pending", "Paid", "Failed", "Refunded" };
                if (!validStatuses.Contains(request.Status))
                    return BadRequest(AdminApiResponse<bool>.Fail("Trạng thái thanh toán không hợp lệ"));

                order.PaymentStatus = request.Status;
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(AdminApiResponse<bool>.Ok(true, "Đã cập nhật trạng thái thanh toán"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for {OrderId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Lấy thống kê đơn hàng
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<AdminApiResponse<object>>> GetOrderStats()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                var stats = new
                {
                    TotalOrders = await _context.Orders.CountAsync(),
                    PendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending"),
                    ProcessingOrders = await _context.Orders.CountAsync(o => o.Status == "Processing" || o.Status == "Confirmed"),
                    ShippingOrders = await _context.Orders.CountAsync(o => o.Status == "Shipping"),
                    CompletedOrders = await _context.Orders.CountAsync(o => o.Status == "Completed" || o.Status == "Delivered"),
                    CancelledOrders = await _context.Orders.CountAsync(o => o.Status == "Cancelled"),
                    TodayOrders = await _context.Orders.CountAsync(o => o.CreatedAt.Date == today),
                    TodayRevenue = await _context.Orders
                        .Where(o => o.CreatedAt.Date == today && (o.Status == "Completed" || o.Status == "Delivered"))
                        .SumAsync(o => o.TotalAmount),
                    MonthOrders = await _context.Orders.CountAsync(o => o.CreatedAt >= thisMonth),
                    MonthRevenue = await _context.Orders
                        .Where(o => o.CreatedAt >= thisMonth && (o.Status == "Completed" || o.Status == "Delivered"))
                        .SumAsync(o => o.TotalAmount)
                };

                return Ok(AdminApiResponse<object>.Ok(stats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order stats");
                return StatusCode(500, AdminApiResponse<object>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Export danh sách đơn hàng ra CSV
        /// </summary>
        [HttpGet("export")]
        public async Task<IActionResult> ExportOrders(
            [FromQuery] string? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.User)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(o => o.Status == status);

                if (fromDate.HasValue)
                    query = query.Where(o => o.CreatedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(o => o.CreatedAt <= toDate.Value);

                var orders = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(10000)
                    .ToListAsync();

                var csv = new StringBuilder();
                csv.AppendLine("Mã đơn,Khách hàng,SĐT,Email,Địa chỉ,Tổng tiền,Trạng thái,Thanh toán,PTTT,Ngày đặt");

                foreach (var order in orders)
                {
                    csv.AppendLine($"\"{order.OrderCode}\"," +
                        $"\"{order.CustomerName}\"," +
                        $"\"{order.CustomerPhone}\"," +
                        $"\"{order.CustomerEmail}\"," +
                        $"\"{order.ShippingAddressText?.Replace("\"", "\"\"")}\"," +
                        $"{order.TotalAmount}," +
                        $"\"{order.Status}\"," +
                        $"\"{order.PaymentStatus}\"," +
                        $"\"{order.PaymentMethodName}\"," +
                        $"\"{order.CreatedAt:yyyy-MM-dd HH:mm}\"");
                }

                var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
                return File(bytes, "text/csv; charset=utf-8", $"orders_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting orders");
                return StatusCode(500, "Lỗi server khi export đơn hàng");
            }
        }

        /// <summary>
        /// Lấy danh sách trạng thái hợp lệ
        /// </summary>
        [HttpGet("statuses")]
        public ActionResult<AdminApiResponse<List<string>>> GetOrderStatuses()
        {
            var statuses = new List<string>
            {
                "Pending",
                "Confirmed",
                "Processing",
                "Shipping",
                "Delivered",
                "Completed",
                "Cancelled",
                "Returned"
            };

            return Ok(AdminApiResponse<List<string>>.Ok(statuses));
        }
    }

    /// <summary>
    /// DTO cho cập nhật payment status
    /// </summary>
    public class UpdatePaymentStatusDto
    {
        public string Status { get; set; } = "";
    }
}
