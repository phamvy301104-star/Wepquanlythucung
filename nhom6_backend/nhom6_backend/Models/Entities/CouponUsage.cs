using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Lịch sử sử dụng coupon
    /// </summary>
    public class CouponUsage : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Coupon
        /// </summary>
        public int CouponId { get; set; }

        [ForeignKey("CouponId")]
        public virtual Coupon? Coupon { get; set; }

        /// <summary>
        /// Khóa ngoại đến User
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Khóa ngoại đến Order
        /// </summary>
        public int? OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        /// <summary>
        /// Số tiền giảm được
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Ngày sử dụng
        /// </summary>
        public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    }
}
