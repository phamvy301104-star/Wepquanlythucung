using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_admin.Models.Entities
{
    /// <summary>
    /// Chi tiết đơn hàng (sản phẩm trong đơn)
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
        /// Tên sản phẩm (snapshot)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// SKU sản phẩm (snapshot)
        /// </summary>
        [MaxLength(50)]
        public string? ProductSKU { get; set; }

        /// <summary>
        /// Hình ảnh sản phẩm (snapshot)
        /// </summary>
        [MaxLength(500)]
        public string? ProductImageUrl { get; set; }

        /// <summary>
        /// Số lượng
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Đơn giá gốc
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal OriginalPrice { get; set; }

        /// <summary>
        /// Đơn giá bán
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Giảm giá
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DiscountAmount { get; set; } = 0;

        /// <summary>
        /// Thành tiền
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Trọng lượng
        /// </summary>
        public int? Weight { get; set; }

        /// <summary>
        /// Biến thể (JSON)
        /// </summary>
        [MaxLength(500)]
        public string? VariantOptions { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
