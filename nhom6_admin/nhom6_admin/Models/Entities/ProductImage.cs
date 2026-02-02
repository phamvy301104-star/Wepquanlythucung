using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_admin.Models.Entities
{
    /// <summary>
    /// Hình ảnh sản phẩm
    /// </summary>
    public class ProductImage : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Product
        /// </summary>
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        /// <summary>
        /// URL hình ảnh
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Text thay thế
        /// </summary>
        [MaxLength(200)]
        public string? AltText { get; set; }

        /// <summary>
        /// Tiêu đề hình ảnh
        /// </summary>
        [MaxLength(200)]
        public string? Title { get; set; }

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Là ảnh chính
        /// </summary>
        public bool IsPrimary { get; set; } = false;
    }
}
