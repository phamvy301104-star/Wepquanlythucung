using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Hình ảnh sản phẩm (nhiều hình cho mỗi sản phẩm)
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
        /// Đường dẫn hình ảnh
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Tên hình ảnh
        /// </summary>
        [MaxLength(200)]
        public string? ImageName { get; set; }

        /// <summary>
        /// Alt text cho SEO
        /// </summary>
        [MaxLength(200)]
        public string? AltText { get; set; }

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Là hình ảnh chính
        /// </summary>
        public bool IsPrimary { get; set; } = false;

        /// <summary>
        /// Kích thước file (bytes)
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// Chiều rộng (pixels)
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Chiều cao (pixels)
        /// </summary>
        public int? Height { get; set; }
    }
}
