using System.ComponentModel.DataAnnotations;

namespace nhom6_admin.Models.DTOs
{
    // ==================== ORDER DTOs ====================
    public class OrderListDto
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public int ItemCount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string? PaymentMethodName { get; set; }
        public string OrderSource { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class OrderDetailDto
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public string? ShippingAddressText { get; set; }
        public string? ReceiverName { get; set; }
        public string? ReceiverPhone { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string? CouponCode { get; set; }
        public string? PaymentMethodName { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string? ShippingMethodName { get; set; }
        public string? TrackingNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CustomerNotes { get; set; }
        public string? InternalNotes { get; set; }
        public string? CancellationReason { get; set; }
        public string? CancelledBy { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public string OrderSource { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDto>? OrderItems { get; set; }
        public List<OrderStatusHistoryDto>? StatusHistories { get; set; }
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductSKU { get; set; }
        public string? ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public string? VariantOptions { get; set; }
        public string? Notes { get; set; }
    }

    public class OrderStatusHistoryDto
    {
        public int Id { get; set; }
        public string? FromStatus { get; set; }
        public string ToStatus { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? ChangedBy { get; set; }
        public string? ChangedByType { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OrderFilterRequest
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public string? OrderSource { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = true;
    }

    public class UpdateOrderStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? TrackingNumber { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
    }

    public class CancelOrderRequest
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    public class UpdatePaymentStatusRequest
    {
        [Required]
        public string PaymentStatus { get; set; } = string.Empty;
    }

    public class CreateOrderRequest
    {
        public string? UserId { get; set; }

        [Required]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        public string CustomerPhone { get; set; } = string.Empty;

        public string? CustomerEmail { get; set; }
        public string? ShippingAddressText { get; set; }
        public string? ReceiverName { get; set; }
        public string? ReceiverPhone { get; set; }
        public decimal ShippingFee { get; set; }
        public string? CouponCode { get; set; }
        public string? PaymentMethodName { get; set; }
        public string? ShippingMethodName { get; set; }
        public string? CustomerNotes { get; set; }
        public string? InternalNotes { get; set; }
        public string OrderSource { get; set; } = "Admin";

        [Required]
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }

    public class CreateOrderItemRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        public string? Notes { get; set; }
    }
}
