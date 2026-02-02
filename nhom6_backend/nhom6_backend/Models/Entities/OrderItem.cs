using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Chi tiết sản phẩm trong đơn hàng
    /// </summary>
    public class OrderItem : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Order
        /// </summary>
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        /// <summary>
        /// Khóa ngoại đến Product
        /// </summary>
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        /// <summary>
        /// Khóa ngoại đến ProductVariant
        /// </summary>
        public int? ProductVariantId { get; set; }

        [ForeignKey("ProductVariantId")]
        public virtual ProductVariant? ProductVariant { get; set; }

        /// <summary>
        /// Mã SKU (snapshot)
        /// </summary>
        [MaxLength(50)]
        public string? SKU { get; set; }

        /// <summary>
        /// Tên sản phẩm (snapshot)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Tên biến thể (snapshot)
        /// </summary>
        [MaxLength(100)]
        public string? VariantName { get; set; }

        /// <summary>
        /// Hình ảnh sản phẩm (snapshot)
        /// </summary>
        [MaxLength(500)]
        public string? ProductImageUrl { get; set; }

        /// <summary>
        /// Giá gốc
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal OriginalPrice { get; set; }

        /// <summary>
        /// Giá bán (tại thời điểm mua)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Số lượng
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Giảm giá trên item
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DiscountAmount { get; set; } = 0;

        /// <summary>
        /// Tổng tiền (Quantity * UnitPrice - Discount)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Trọng lượng (gram)
        /// </summary>
        public int? Weight { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        [MaxLength(300)]
        public string? Notes { get; set; }

        /// <summary>
        /// Trạng thái: Normal, Returned, Refunded
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Normal";

        /// <summary>
        /// Số lượng đã trả lại
        /// </summary>
        public int ReturnedQuantity { get; set; } = 0;

        /// <summary>
        /// Lý do trả hàng
        /// </summary>
        [MaxLength(500)]
        public string? ReturnReason { get; set; }

        /// <summary>
        /// Đã đánh giá chưa
        /// </summary>
        public bool IsReviewed { get; set; } = false;

        // Navigation Properties
        public virtual ICollection<ProductReview>? ProductReviews { get; set; }
    }
}
