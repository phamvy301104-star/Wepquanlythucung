using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_backend.Models;
using nhom6_backend.Models.Entities;
using System.Security.Claims;

namespace nhom6_backend.Controllers
{
    /// <summary>
    /// Order API Controller - Handle order operations for users
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrderApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderApiController> _logger;

        public OrderApiController(ApplicationDbContext context, ILogger<OrderApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get user's orders with pagination and filtering
        /// GET /api/OrderApi/my-orders
        /// </summary>
        [HttpGet("my-orders")]
        [Authorize]
        public async Task<IActionResult> GetMyOrders(
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "Chưa đăng nhập" });
                }

                _logger.LogInformation("GetMyOrders: UserId={UserId}, Status={Status}, Page={Page}", userId, status, page);

                var query = _context.Orders
                    .AsNoTracking()
                    .Where(o => o.UserId == userId && !o.IsDeleted)
                    .AsQueryable();

                // Filter by status
                if (!string.IsNullOrEmpty(status) && status != "All")
                {
                    query = query.Where(o => o.Status == status);
                }

                // Get total count
                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                // Use projection directly - more efficient and avoids EF Core issues
                var response = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(o => new
                    {
                        o.Id,
                        o.OrderCode,
                        o.UserId,
                        o.CustomerName,
                        o.CustomerEmail,
                        o.CustomerPhone,
                        o.ShippingAddressText,
                        o.ReceiverName,
                        o.ReceiverPhone,
                        o.SubTotal,
                        o.ShippingFee,
                        o.DiscountAmount,
                        o.TaxAmount,
                        o.TotalAmount,
                        o.PaidAmount,
                        o.CouponCode,
                        o.PaymentMethodName,
                        o.PaymentStatus,
                        o.ShippingMethodName,
                        o.TrackingNumber,
                        o.Status,
                        o.CustomerNotes,
                        o.EstimatedDeliveryDate,
                        o.DeliveredAt,
                        o.CancelledAt,
                        o.CancellationReason,
                        o.CreatedAt,
                        o.UpdatedAt,
                        Items = o.OrderItems!.Select(oi => new
                        {
                            oi.Id,
                            oi.OrderId,
                            oi.ProductId,
                            oi.ProductVariantId,
                            oi.SKU,
                            oi.ProductName,
                            oi.VariantName,
                            ProductImageUrl = oi.Product != null ? oi.Product.ImageUrl : oi.ProductImageUrl,
                            oi.UnitPrice,
                            oi.Quantity,
                            oi.DiscountAmount,
                            oi.TotalPrice,
                            oi.Notes,
                            oi.CreatedAt
                        }).ToList(),
                        ItemCount = o.OrderItems!.Sum(oi => oi.Quantity)
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        items = response,
                        totalCount,
                        page,
                        pageSize,
                        totalPages
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMyOrders: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        /// <summary>
        /// Get order detail by ID
        /// GET /api/OrderApi/{id}
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "Chưa đăng nhập" });
                }

                // Use projection for efficient query
                var response = await _context.Orders
                    .AsNoTracking()
                    .Where(o => o.Id == id && o.UserId == userId && !o.IsDeleted)
                    .Select(o => new
                    {
                        o.Id,
                        o.OrderCode,
                        o.UserId,
                        o.CustomerName,
                        o.CustomerEmail,
                        o.CustomerPhone,
                        o.ShippingAddressText,
                        o.ReceiverName,
                        o.ReceiverPhone,
                        o.SubTotal,
                        o.ShippingFee,
                        o.DiscountAmount,
                        o.TaxAmount,
                        o.TotalAmount,
                        o.PaidAmount,
                        o.CouponCode,
                        o.PaymentMethodName,
                        o.PaymentStatus,
                        o.ShippingMethodName,
                        o.TrackingNumber,
                        o.Status,
                        o.CustomerNotes,
                        StaffNotes = o.InternalNotes,
                        o.EstimatedDeliveryDate,
                        o.DeliveredAt,
                        o.CancelledAt,
                        o.CancellationReason,
                        o.CreatedAt,
                        o.UpdatedAt,
                        Items = o.OrderItems!.Select(oi => new
                        {
                            oi.Id,
                            oi.OrderId,
                            oi.ProductId,
                            oi.ProductVariantId,
                            oi.SKU,
                            oi.ProductName,
                            oi.VariantName,
                            ProductImageUrl = oi.Product != null ? oi.Product.ImageUrl : oi.ProductImageUrl,
                            oi.UnitPrice,
                            oi.Quantity,
                            oi.DiscountAmount,
                            oi.TotalPrice,
                            oi.Notes,
                            oi.CreatedAt
                        }).ToList(),
                        StatusHistory = o.StatusHistories!.OrderByDescending(s => s.CreatedAt).Select(s => new
                        {
                            s.Id,
                            s.OrderId,
                            s.FromStatus,
                            s.ToStatus,
                            s.Notes,
                            ChangedBy = s.ChangedByUserId,
                            s.CreatedAt
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (response == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                return Ok(new { success = true, data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrderDetail for ID {OrderId}: {Message}", id, ex.Message);
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        /// <summary>
        /// Cancel order
        /// POST /api/OrderApi/{id}/cancel
        /// </summary>
        [HttpPost("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] CancelOrderRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "Chưa đăng nhập" });
                }

                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId && !o.IsDeleted);

                if (order == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                // Only allow cancellation for Pending or Confirmed orders
                if (order.Status != "Pending" && order.Status != "Confirmed")
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Chỉ có thể hủy đơn hàng ở trạng thái 'Chờ xác nhận' hoặc 'Đã xác nhận'" 
                    });
                }

                var previousStatus = order.Status;

                // Update order status
                order.Status = "Cancelled";
                order.CancellationReason = request.Reason;
                order.CancelledBy = "Customer";
                order.CancelledAt = DateTime.Now;
                order.UpdatedAt = DateTime.Now;

                // Add status history
                var statusHistory = new OrderStatusHistory
                {
                    OrderId = order.Id,
                    FromStatus = previousStatus,
                    ToStatus = "Cancelled",
                    Notes = request.Reason,
                    ChangedByUserId = userId, // Use actual user ID
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.OrderStatusHistories.Add(statusHistory);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} cancelled by user {UserId}. Reason: {Reason}", 
                    id, userId, request.Reason);

                return Ok(new { success = true, message = "Đã hủy đơn hàng thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", id);
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }

        /// <summary>
        /// Get order count by status (for badges)
        /// GET /api/OrderApi/count-by-status
        /// </summary>
        [HttpGet("count-by-status")]
        [Authorize]
        public async Task<IActionResult> GetOrderCountByStatus()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "Chưa đăng nhập" });
                }

                var counts = await _context.Orders
                    .Where(o => o.UserId == userId && !o.IsDeleted)
                    .GroupBy(o => o.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                var result = new
                {
                    all = counts.Sum(c => c.Count),
                    pending = counts.FirstOrDefault(c => c.Status == "Pending")?.Count ?? 0,
                    confirmed = counts.FirstOrDefault(c => c.Status == "Confirmed")?.Count ?? 0,
                    processing = counts.FirstOrDefault(c => c.Status == "Processing")?.Count ?? 0,
                    shipping = counts.FirstOrDefault(c => c.Status == "Shipping")?.Count ?? 0,
                    delivered = counts.FirstOrDefault(c => c.Status == "Delivered")?.Count ?? 0,
                    completed = counts.FirstOrDefault(c => c.Status == "Completed")?.Count ?? 0,
                    cancelled = counts.FirstOrDefault(c => c.Status == "Cancelled")?.Count ?? 0
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrderCountByStatus");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }
    }

    /// <summary>
    /// Request model for cancelling an order
    /// </summary>
    public class CancelOrderRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
