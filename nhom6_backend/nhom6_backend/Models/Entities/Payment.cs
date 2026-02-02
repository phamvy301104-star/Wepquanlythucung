using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Giao dịch thanh toán
    /// </summary>
    public class Payment : BaseEntity
    {
        /// <summary>
        /// Mã giao dịch
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TransactionCode { get; set; } = string.Empty;

        /// <summary>
        /// Khóa ngoại đến Order
        /// </summary>
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        /// <summary>
        /// Khóa ngoại đến PaymentMethod
        /// </summary>
        public int PaymentMethodId { get; set; }

        [ForeignKey("PaymentMethodId")]
        public virtual PaymentMethod? PaymentMethod { get; set; }

        /// <summary>
        /// Số tiền thanh toán
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Phí giao dịch
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Fee { get; set; } = 0;

        /// <summary>
        /// Số tiền thực nhận
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal NetAmount { get; set; }

        /// <summary>
        /// Loại tiền tệ
        /// </summary>
        [MaxLength(3)]
        public string Currency { get; set; } = "VND";

        /// <summary>
        /// Trạng thái: Pending, Processing, Success, Failed, Refunded, Cancelled
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Mã giao dịch từ cổng thanh toán
        /// </summary>
        [MaxLength(100)]
        public string? GatewayTransactionId { get; set; }

        /// <summary>
        /// Response từ cổng thanh toán (JSON)
        /// </summary>
        public string? GatewayResponse { get; set; }

        /// <summary>
        /// Mã lỗi (nếu có)
        /// </summary>
        [MaxLength(50)]
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Thông báo lỗi
        /// </summary>
        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Ngày thanh toán thành công
        /// </summary>
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// Số tài khoản ngân hàng (nếu chuyển khoản)
        /// </summary>
        [MaxLength(50)]
        public string? BankAccountNumber { get; set; }

        /// <summary>
        /// Tên ngân hàng
        /// </summary>
        [MaxLength(100)]
        public string? BankName { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// IP Address
        /// </summary>
        [MaxLength(50)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// Người thực hiện hoàn tiền
        /// </summary>
        public string? RefundedByUserId { get; set; }

        /// <summary>
        /// Ngày hoàn tiền
        /// </summary>
        public DateTime? RefundedAt { get; set; }

        /// <summary>
        /// Lý do hoàn tiền
        /// </summary>
        [MaxLength(500)]
        public string? RefundReason { get; set; }
    }
}
