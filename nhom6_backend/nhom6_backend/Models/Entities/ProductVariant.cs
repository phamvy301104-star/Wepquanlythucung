using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Biến thể sản phẩm (Size, Mùi hương, etc.)
    /// </summary>
    public class ProductVariant : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Product
        /// </summary>
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        /// <summary>
        /// Mã SKU của biến thể
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string SKU { get; set; } = string.Empty;

        /// <summary>
        /// Tên biến thể
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Loại biến thể: Size, Color, Scent (mùi hương)
        /// </summary>
        [MaxLength(50)]
        public string VariantType { get; set; } = "Size";

        /// <summary>
        /// Giá trị biến thể (VD: "100ml", "250ml", "Mint")
        /// </summary>
        [MaxLength(100)]
        public string? VariantValue { get; set; }

        /// <summary>
        /// Giá gốc biến thể
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal OriginalPrice { get; set; }

        /// <summary>
        /// Giá bán biến thể
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// Số lượng tồn kho của biến thể
        /// </summary>
        public int StockQuantity { get; set; } = 0;

        /// <summary>
        /// Trọng lượng (gram)
        /// </summary>
        public int? Weight { get; set; }

        /// <summary>
        /// Dung tích (ml)
        /// </summary>
        public int? Volume { get; set; }

        /// <summary>
        /// Hình ảnh biến thể
        /// </summary>
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Mã vạch biến thể
        /// </summary>
        [MaxLength(50)]
        public string? Barcode { get; set; }

        /// <summary>
        /// Trạng thái hiển thị
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; } = 0;
    }
}
