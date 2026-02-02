using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_admin.Models.Entities
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
        /// Địa chỉ giao hàng
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
        /// Mã giảm giá (snapshot)
        /// </summary>
        [MaxLength(50)]
        public string? CouponCode { get; set; }

        /// <summary>
        /// Tên phương thức thanh toán
        /// </summary>
        [MaxLength(50)]
        public string? PaymentMethodName { get; set; }

        /// <summary>
        /// Trạng thái thanh toán: Pending, Paid, Failed, Refunded
        /// </summary>
        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Pending";

        /// <summary>
        /// Tên phương thức vận chuyển
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
        /// Trọng lượng tổng (gram)
        /// </summary>
        public int? TotalWeight { get; set; }

        // Navigation Properties
        public virtual ICollection<OrderItem>? OrderItems { get; set; }
        public virtual ICollection<OrderStatusHistory>? StatusHistories { get; set; }
    }
}
