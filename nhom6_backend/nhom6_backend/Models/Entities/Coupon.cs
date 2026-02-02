using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Mã giảm giá (Coupon/Voucher)
    /// </summary>
    public class Coupon : BaseEntity
    {
        /// <summary>
        /// Mã giảm giá
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên coupon
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Loại giảm giá: Percent, FixedAmount
        /// </summary>
        [MaxLength(20)]
        public string DiscountType { get; set; } = "Percent";

        /// <summary>
        /// Giá trị giảm (% hoặc số tiền)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DiscountValue { get; set; }

        /// <summary>
        /// Giảm tối đa (cho loại Percent)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? MaxDiscountAmount { get; set; }

        /// <summary>
        /// Đơn hàng tối thiểu
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MinOrderAmount { get; set; } = 0;

        /// <summary>
        /// Ngày bắt đầu
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Số lần sử dụng tối đa (tổng)
        /// </summary>
        public int? MaxUsageCount { get; set; }

        /// <summary>
        /// Số lần đã sử dụng
        /// </summary>
        public int UsedCount { get; set; } = 0;

        /// <summary>
        /// Số lần sử dụng tối đa/user
        /// </summary>
        public int MaxUsagePerUser { get; set; } = 1;

        /// <summary>
        /// Áp dụng cho sản phẩm cụ thể (JSON array of product IDs)
        /// </summary>
        public string? ApplicableProductIds { get; set; }

        /// <summary>
        /// Áp dụng cho danh mục cụ thể (JSON array of category IDs)
        /// </summary>
        public string? ApplicableCategoryIds { get; set; }

        /// <summary>
        /// Áp dụng cho thương hiệu cụ thể (JSON array of brand IDs)
        /// </summary>
        public string? ApplicableBrandIds { get; set; }

        /// <summary>
        /// Áp dụng cho dịch vụ cụ thể (JSON array of service IDs)
        /// </summary>
        public string? ApplicableServiceIds { get; set; }

        /// <summary>
        /// Loại áp dụng: All, Products, Services, Both
        /// </summary>
        [MaxLength(20)]
        public string ApplicableType { get; set; } = "All";

        /// <summary>
        /// Dành cho khách hàng mới
        /// </summary>
        public bool ForNewCustomersOnly { get; set; } = false;

        /// <summary>
        /// Dành cho hạng thành viên (JSON array: Bronze, Silver, Gold, etc.)
        /// </summary>
        public string? ForMembershipTiers { get; set; }

        /// <summary>
        /// Trạng thái: Active, Inactive, Expired
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Có thể kết hợp với coupon khác
        /// </summary>
        public bool IsStackable { get; set; } = false;

        /// <summary>
        /// Công khai (hiển thị cho tất cả)
        /// </summary>
        public bool IsPublic { get; set; } = true;

        /// <summary>
        /// Tự động áp dụng khi đủ điều kiện
        /// </summary>
        public bool AutoApply { get; set; } = false;

        // Navigation Properties
        public virtual ICollection<CouponUsage>? CouponUsages { get; set; }
        public virtual ICollection<Order>? Orders { get; set; }
        public virtual ICollection<Cart>? Carts { get; set; }
    }
}
