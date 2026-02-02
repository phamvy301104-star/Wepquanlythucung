using System.ComponentModel.DataAnnotations;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Phương thức thanh toán (COD, VietQR, Momo, etc.)
    /// </summary>
    public class PaymentMethod : BaseEntity
    {
        /// <summary>
        /// Mã phương thức thanh toán
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên phương thức
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Hướng dẫn sử dụng
        /// </summary>
        [MaxLength(1000)]
        public string? Instructions { get; set; }

        /// <summary>
        /// Icon/Logo
        /// </summary>
        [MaxLength(500)]
        public string? IconUrl { get; set; }

        /// <summary>
        /// Loại: COD, BankTransfer, EWallet, CreditCard
        /// </summary>
        [MaxLength(30)]
        public string Type { get; set; } = "COD";

        /// <summary>
        /// Phí giao dịch (%)
        /// </summary>
        public decimal TransactionFeePercent { get; set; } = 0;

        /// <summary>
        /// Phí cố định
        /// </summary>
        public decimal FixedFee { get; set; } = 0;

        /// <summary>
        /// Số tiền tối thiểu
        /// </summary>
        public decimal? MinAmount { get; set; }

        /// <summary>
        /// Số tiền tối đa
        /// </summary>
        public decimal? MaxAmount { get; set; }

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Trạng thái active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Cấu hình (JSON) - API keys, etc.
        /// </summary>
        public string? Configuration { get; set; }

        // Navigation Properties
        public virtual ICollection<Order>? Orders { get; set; }
        public virtual ICollection<Payment>? Payments { get; set; }
    }
}
