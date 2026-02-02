using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Đơn hàng
    /// </summary>
    public class Order : BaseEntity
    {
        /// <summary>
        /// Mã đơn hàng
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string OrderCode { get; set; } = string.Empty;

        /// <summary>
        /// Khóa ngoại đến User
        /// </summary>
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Tên người đặt hàng
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Email người đặt
        /// </summary>
        [MaxLength(100)]
        public string? CustomerEmail { get; set; }

        /// <summary>
        /// SĐT người đặt
        /// </summary>
        [Required]
        [MaxLength(15)]
        public string CustomerPhone { get; set; } = string.Empty;

        /// <summary>
        /// Khóa ngoại đến UserAddress (địa chỉ giao hàng)
        /// </summary>
        public int? ShippingAddressId { get; set; }

        [ForeignKey("ShippingAddressId")]
        public virtual UserAddress? ShippingAddress { get; set; }

        /// <summary>
        /// Địa chỉ giao hàng (snapshot)
        /// </summary>
        [MaxLength(500)]
        public string? ShippingAddressText { get; set; }

        /// <summary>
        /// Tên người nhận
        /// </summary>
        [MaxLength(100)]
        public string? ReceiverName { get; set; }

        /// <summary>
        /// SĐT người nhận
        /// </summary>
        [MaxLength(15)]
        public string? ReceiverPhone { get; set; }

        /// <summary>
        /// Tổng tiền hàng (trước giảm giá, shipping)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SubTotal { get; set; }

        /// <summary>
        /// Phí vận chuyển
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ShippingFee { get; set; } = 0;

        /// <summary>
        /// Giảm giá
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DiscountAmount { get; set; } = 0;

        /// <summary>
        /// Thuế
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TaxAmount { get; set; } = 0;

        /// <summary>
        /// Tổng tiền thanh toán
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Tiền đã thanh toán
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PaidAmount { get; set; } = 0;

        /// <summary>
        /// Khóa ngoại đến Coupon
        /// </summary>
        public int? CouponId { get; set; }

        [ForeignKey("CouponId")]
        public virtual Coupon? Coupon { get; set; }

        /// <summary>
        /// Mã giảm giá (snapshot)
        /// </summary>
        [MaxLength(50)]
        public string? CouponCode { get; set; }

        /// <summary>
        /// Khóa ngoại đến PaymentMethod
        /// </summary>
        public int? PaymentMethodId { get; set; }

        [ForeignKey("PaymentMethodId")]
        public virtual PaymentMethod? PaymentMethod { get; set; }

        /// <summary>
        /// Tên phương thức thanh toán (snapshot)
        /// </summary>
        [MaxLength(50)]
        public string? PaymentMethodName { get; set; }

        /// <summary>
        /// Trạng thái thanh toán: Pending, Paid, Failed, Refunded
        /// </summary>
        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Pending";

        /// <summary>
        /// Khóa ngoại đến ShippingMethod
        /// </summary>
        public int? ShippingMethodId { get; set; }

        [ForeignKey("ShippingMethodId")]
        public virtual ShippingMethod? ShippingMethod { get; set; }

        /// <summary>
        /// Tên phương thức vận chuyển (snapshot)
        /// </summary>
        [MaxLength(100)]
        public string? ShippingMethodName { get; set; }

        /// <summary>
        /// Mã vận đơn
        /// </summary>
        [MaxLength(50)]
        public string? TrackingNumber { get; set; }

        /// <summary>
        /// Trạng thái đơn hàng: Pending, Confirmed, Processing, Shipping, Delivered, Completed, Cancelled, Returned
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Ghi chú của khách hàng
        /// </summary>
        [MaxLength(500)]
        public string? CustomerNotes { get; set; }

        /// <summary>
        /// Ghi chú nội bộ (admin)
        /// </summary>
        [MaxLength(500)]
        public string? InternalNotes { get; set; }

        /// <summary>
        /// Lý do hủy
        /// </summary>
        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        /// <summary>
        /// Người hủy
        /// </summary>
        [MaxLength(20)]
        public string? CancelledBy { get; set; }

        /// <summary>
        /// Ngày hủy
        /// </summary>
        public DateTime? CancelledAt { get; set; }

        /// <summary>
        /// Ngày xác nhận
        /// </summary>
        public DateTime? ConfirmedAt { get; set; }

        /// <summary>
        /// Ngày gửi hàng
        /// </summary>
        public DateTime? ShippedAt { get; set; }

        /// <summary>
        /// Ngày giao hàng
        /// </summary>
        public DateTime? DeliveredAt { get; set; }

        /// <summary>
        /// Ngày hoàn thành
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Ngày giao dự kiến
        /// </summary>
        public DateTime? EstimatedDeliveryDate { get; set; }

        /// <summary>
        /// Nguồn đặt hàng: App, Website, Phone, InStore
        /// </summary>
        [MaxLength(20)]
        public string OrderSource { get; set; } = "App";

        /// <summary>
        /// IP Address
        /// </summary>
        [MaxLength(50)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// Trọng lượng tổng (gram)
        /// </summary>
        public int? TotalWeight { get; set; }

        /// <summary>
        /// Điểm thưởng sử dụng
        /// </summary>
        public int UsedLoyaltyPoints { get; set; } = 0;

        /// <summary>
        /// Điểm thưởng nhận được
        /// </summary>
        public int EarnedLoyaltyPoints { get; set; } = 0;

        /// <summary>
        /// Đã in hóa đơn
        /// </summary>
        public bool IsInvoicePrinted { get; set; } = false;

        // Navigation Properties
        public virtual ICollection<OrderItem>? OrderItems { get; set; }
        public virtual ICollection<OrderStatusHistory>? StatusHistories { get; set; }
        public virtual ICollection<Payment>? Payments { get; set; }
    }
}
