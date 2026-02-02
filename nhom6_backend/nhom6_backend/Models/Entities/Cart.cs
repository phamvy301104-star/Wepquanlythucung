using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Giỏ hàng của người dùng
    /// </summary>
    public class Cart : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến User
        /// </summary>
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Session ID cho khách vãng lai
        /// </summary>
        [MaxLength(100)]
        public string? SessionId { get; set; }

        /// <summary>
        /// Tổng số sản phẩm
        /// </summary>
        public int TotalItems { get; set; } = 0;

        /// <summary>
        /// Tổng tiền
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; } = 0;

        /// <summary>
        /// Mã giảm giá đang áp dụng
        /// </summary>
        public int? CouponId { get; set; }

        [ForeignKey("CouponId")]
        public virtual Coupon? Coupon { get; set; }

        /// <summary>
        /// Số tiền giảm
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DiscountAmount { get; set; } = 0;

        /// <summary>
        /// Ghi chú
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Ngày hết hạn (cho session cart)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        // Navigation Properties
        public virtual ICollection<CartItem>? CartItems { get; set; }
    }
}
