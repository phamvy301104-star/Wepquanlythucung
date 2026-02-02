using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Models;
using nhom6_admin.Models.DTOs;
using nhom6_admin.Models.Entities;

namespace nhom6_admin.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all orders with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<OrderListDto>>>> GetOrders(
            [FromQuery] OrderFilterRequest filter)
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                    .Where(o => !o.IsDeleted)
                    .AsQueryable();

                // Search
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    var search = filter.Search.ToLower();
                    query = query.Where(o =>
                        o.OrderCode.ToLower().Contains(search) ||
                        o.CustomerName.ToLower().Contains(search) ||
                        (o.CustomerPhone != null && o.CustomerPhone.Contains(search)) ||
                        (o.CustomerEmail != null && o.CustomerEmail.ToLower().Contains(search)));
                }

                // Filter by status
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    query = query.Where(o => o.Status == filter.Status);
                }

                // Filter by payment status
                if (!string.IsNullOrEmpty(filter.PaymentStatus))
                {
                    query = query.Where(o => o.PaymentStatus == filter.PaymentStatus);
                }

                // Filter by order source
                if (!string.IsNullOrEmpty(filter.OrderSource))
                {
                    query = query.Where(o => o.OrderSource == filter.OrderSource);
                }

                // Filter by date range
                if (filter.FromDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);
                }
                if (filter.ToDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt <= filter.ToDate.Value.AddDays(1));
                }

                // Filter by amount
                if (filter.MinAmount.HasValue)
                {
                    query = query.Where(o => o.TotalAmount >= filter.MinAmount.Value);
                }
                if (filter.MaxAmount.HasValue)
                {
                    query = query.Where(o => o.TotalAmount <= filter.MaxAmount.Value);
                }

                var totalCount = await query.CountAsync();

                // Sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "ordercode" => filter.SortDesc ? query.OrderByDescending(o => o.OrderCode) : query.OrderBy(o => o.OrderCode),
                    "total" => filter.SortDesc ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount),
                    "status" => filter.SortDesc ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
                    _ => query.OrderByDescending(o => o.CreatedAt)
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
                        ItemCount = o.OrderItems != null ? o.OrderItems.Count : 0,
                        SubTotal = o.SubTotal,
                        ShippingFee = o.ShippingFee,
                        DiscountAmount = o.DiscountAmount,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        PaymentStatus = o.PaymentStatus,
                        PaymentMethodName = o.PaymentMethodName,
                        OrderSource = o.OrderSource,
                        CreatedAt = o.CreatedAt
                    })
                    .ToListAsync();

                var response = new PaginatedResponse<OrderListDto>
                {
                    Items = orders,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

                return Ok(ApiResponse<PaginatedResponse<OrderListDto>>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<OrderListDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<OrderDetailDto>>> GetOrder(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems!)
                        .ThenInclude(i => i.Product)
                    .Include(o => o.StatusHistories)
                    .Where(o => o.Id == id && !o.IsDeleted)
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    return NotFound(ApiResponse<OrderDetailDto>.ErrorResponse("Order not found"));
                }

                var orderDto = new OrderDetailDto
                {
                    Id = order.Id,
                    OrderCode = order.OrderCode,
                    UserId = order.UserId,
                    CustomerName = order.CustomerName,
                    CustomerPhone = order.CustomerPhone,
                    CustomerEmail = order.CustomerEmail,
                    ShippingAddressText = order.ShippingAddressText,
                    ReceiverName = order.ReceiverName,
                    ReceiverPhone = order.ReceiverPhone,
                    SubTotal = order.SubTotal,
                    ShippingFee = order.ShippingFee,
                    DiscountAmount = order.DiscountAmount,
                    TaxAmount = order.TaxAmount,
                    TotalAmount = order.TotalAmount,
                    PaidAmount = order.PaidAmount,
                    CouponCode = order.CouponCode,
                    PaymentMethodName = order.PaymentMethodName,
                    PaymentStatus = order.PaymentStatus,
                    ShippingMethodName = order.ShippingMethodName,
                    TrackingNumber = order.TrackingNumber,
                    Status = order.Status,
                    CustomerNotes = order.CustomerNotes,
                    InternalNotes = order.InternalNotes,
                    CancellationReason = order.CancellationReason,
                    CancelledBy = order.CancelledBy,
                    CancelledAt = order.CancelledAt,
                    ConfirmedAt = order.ConfirmedAt,
                    ShippedAt = order.ShippedAt,
                    DeliveredAt = order.DeliveredAt,
                    CompletedAt = order.CompletedAt,
                    EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                    OrderSource = order.OrderSource,
                    CreatedAt = order.CreatedAt,
                    OrderItems = order.OrderItems?.Select(i => new OrderItemDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        ProductSKU = i.ProductSKU,
                        ProductImageUrl = i.ProductImageUrl,
                        Quantity = i.Quantity,
                        OriginalPrice = i.OriginalPrice,
                        UnitPrice = i.UnitPrice,
                        DiscountAmount = i.DiscountAmount,
                        TotalPrice = i.TotalPrice,
                        VariantOptions = i.VariantOptions,
                        Notes = i.Notes
                    }).ToList(),
                    StatusHistories = order.StatusHistories?.OrderByDescending(h => h.CreatedAt).Select(h => new OrderStatusHistoryDto
                    {
                        Id = h.Id,
                        FromStatus = h.FromStatus,
                        ToStatus = h.ToStatus,
                        Notes = h.Notes,
                        ChangedBy = h.ChangedBy,
                        ChangedByType = h.ChangedByType,
                        CreatedAt = h.CreatedAt
                    }).ToList()
                };

                return Ok(ApiResponse<OrderDetailDto>.SuccessResponse(orderDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create new order
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<OrderDetailDto>>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get products and validate
                var productIds = request.Items.Select(i => i.ProductId).ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id) && !p.IsDeleted && p.IsActive)
                    .ToListAsync();

                if (products.Count != productIds.Count)
                {
                    return BadRequest(ApiResponse<OrderDetailDto>.ErrorResponse("Some products not found or inactive"));
                }

                // Check stock
                foreach (var item in request.Items)
                {
                    var product = products.First(p => p.Id == item.ProductId);
                    if (product.StockQuantity < item.Quantity)
                    {
                        return BadRequest(ApiResponse<OrderDetailDto>.ErrorResponse($"Product '{product.Name}' is out of stock"));
                    }
                }

                // Calculate totals
                decimal subTotal = 0;
                foreach (var item in request.Items)
                {
                    var product = products.First(p => p.Id == item.ProductId);
                    subTotal += product.Price * item.Quantity;
                }

                var orderCode = $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(100, 999)}";

                var order = new Order
                {
                    OrderCode = orderCode,
                    UserId = request.UserId,
                    CustomerName = request.CustomerName,
                    CustomerPhone = request.CustomerPhone,
                    CustomerEmail = request.CustomerEmail,
                    ShippingAddressText = request.ShippingAddressText,
                    ReceiverName = request.ReceiverName ?? request.CustomerName,
                    ReceiverPhone = request.ReceiverPhone ?? request.CustomerPhone,
                    SubTotal = subTotal,
                    ShippingFee = request.ShippingFee,
                    DiscountAmount = 0,
                    TotalAmount = subTotal + request.ShippingFee,
                    CouponCode = request.CouponCode,
                    PaymentMethodName = request.PaymentMethodName,
                    PaymentStatus = "Pending",
                    ShippingMethodName = request.ShippingMethodName,
                    Status = "Pending",
                    CustomerNotes = request.CustomerNotes,
                    InternalNotes = request.InternalNotes,
                    OrderSource = request.OrderSource,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Add order items and update stock
                foreach (var item in request.Items)
                {
                    var product = products.First(p => p.Id == item.ProductId);

                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        ProductSKU = product.SKU,
                        ProductImageUrl = product.ImageUrl,
                        Quantity = item.Quantity,
                        OriginalPrice = product.OriginalPrice,
                        UnitPrice = product.Price,
                        DiscountAmount = 0,
                        TotalPrice = product.Price * item.Quantity,
                        Notes = item.Notes,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.OrderItems.Add(orderItem);

                    // Update stock
                    product.StockQuantity -= item.Quantity;
                    product.SoldCount += item.Quantity;
                }

                // Add status history
                var statusHistory = new OrderStatusHistory
                {
                    OrderId = order.Id,
                    FromStatus = null,
                    ToStatus = "Pending",
                    Notes = "Order created",
                    ChangedBy = "Admin",
                    ChangedByType = "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                _context.OrderStatusHistories.Add(statusHistory);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetOrder(order.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ApiResponse<OrderDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<ActionResult<ApiResponse<OrderDetailDto>>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null || order.IsDeleted)
                {
                    return NotFound(ApiResponse<OrderDetailDto>.ErrorResponse("Order not found"));
                }

                var validStatuses = new[] { "Pending", "Confirmed", "Processing", "Shipping", "Delivered", "Completed", "Cancelled", "Returned" };
                if (!validStatuses.Contains(request.Status))
                {
                    return BadRequest(ApiResponse<OrderDetailDto>.ErrorResponse("Invalid status"));
                }

                var oldStatus = order.Status;
                order.Status = request.Status;

                if (request.TrackingNumber != null)
                    order.TrackingNumber = request.TrackingNumber;

                if (request.EstimatedDeliveryDate.HasValue)
                    order.EstimatedDeliveryDate = request.EstimatedDeliveryDate;

                // Set timestamps based on status
                switch (request.Status)
                {
                    case "Confirmed":
                        order.ConfirmedAt = DateTime.UtcNow;
                        break;
                    case "Shipping":
                        order.ShippedAt = DateTime.UtcNow;
                        break;
                    case "Delivered":
                        order.DeliveredAt = DateTime.UtcNow;
                        break;
                    case "Completed":
                        order.CompletedAt = DateTime.UtcNow;
                        if (order.PaymentStatus == "Pending")
                            order.PaymentStatus = "Paid";
                        break;
                }

                // Add status history
                var statusHistory = new OrderStatusHistory
                {
                    OrderId = order.Id,
                    FromStatus = oldStatus,
                    ToStatus = request.Status,
                    Notes = request.Notes,
                    ChangedBy = "Admin",
                    ChangedByType = "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                _context.OrderStatusHistories.Add(statusHistory);

                order.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return await GetOrder(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update payment status
        /// </summary>
        [HttpPut("{id}/payment-status")]
        public async Task<ActionResult<ApiResponse<OrderDetailDto>>> UpdatePaymentStatus(int id, [FromBody] UpdatePaymentStatusRequest request)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null || order.IsDeleted)
                {
                    return NotFound(ApiResponse<OrderDetailDto>.ErrorResponse("Order not found"));
                }

                var validStatuses = new[] { "Pending", "Paid", "Failed", "Refunded" };
                if (!validStatuses.Contains(request.PaymentStatus))
                {
                    return BadRequest(ApiResponse<OrderDetailDto>.ErrorResponse("Invalid payment status"));
                }

                order.PaymentStatus = request.PaymentStatus;
                if (request.PaymentStatus == "Paid")
                {
                    order.PaidAmount = order.TotalAmount;
                }

                order.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return await GetOrder(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cancel order
        /// </summary>
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<ApiResponse<OrderDetailDto>>> CancelOrder(int id, [FromBody] CancelOrderRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

                if (order == null)
                {
                    return NotFound(ApiResponse<OrderDetailDto>.ErrorResponse("Order not found"));
                }

                if (order.Status == "Completed" || order.Status == "Delivered")
                {
                    return BadRequest(ApiResponse<OrderDetailDto>.ErrorResponse("Cannot cancel delivered or completed order"));
                }

                var oldStatus = order.Status;
                order.Status = "Cancelled";
                order.CancellationReason = request.Reason;
                order.CancelledBy = "Admin";
                order.CancelledAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;

                // Restore stock
                foreach (var item in order.OrderItems!)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        product.SoldCount -= item.Quantity;
                    }
                }

                // Add status history
                var statusHistory = new OrderStatusHistory
                {
                    OrderId = order.Id,
                    FromStatus = oldStatus,
                    ToStatus = "Cancelled",
                    Notes = request.Reason,
                    ChangedBy = "Admin",
                    ChangedByType = "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                _context.OrderStatusHistories.Add(statusHistory);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetOrder(id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ApiResponse<OrderDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete order (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteOrder(int id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null || order.IsDeleted)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse("Order not found"));
                }

                order.IsDeleted = true;
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Order deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get order statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<object>>> GetOrderStatistics([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var startDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var endDate = toDate ?? DateTime.UtcNow;

                var orders = await _context.Orders
                    .Where(o => !o.IsDeleted &&
                               o.CreatedAt >= startDate &&
                               o.CreatedAt <= endDate.AddDays(1))
                    .ToListAsync();

                var statistics = new
                {
                    TotalOrders = orders.Count,
                    PendingOrders = orders.Count(o => o.Status == "Pending"),
                    ConfirmedOrders = orders.Count(o => o.Status == "Confirmed"),
                    ProcessingOrders = orders.Count(o => o.Status == "Processing"),
                    ShippingOrders = orders.Count(o => o.Status == "Shipping"),
                    DeliveredOrders = orders.Count(o => o.Status == "Delivered"),
                    CompletedOrders = orders.Count(o => o.Status == "Completed"),
                    CancelledOrders = orders.Count(o => o.Status == "Cancelled"),
                    TotalRevenue = orders.Where(o => o.Status == "Completed" || o.Status == "Delivered").Sum(o => o.TotalAmount),
                    AverageOrderValue = orders.Where(o => o.Status == "Completed" || o.Status == "Delivered").Any()
                        ? orders.Where(o => o.Status == "Completed" || o.Status == "Delivered").Average(o => o.TotalAmount) : 0
                };

                return Ok(ApiResponse<object>.SuccessResponse(statistics));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }
    }
}
