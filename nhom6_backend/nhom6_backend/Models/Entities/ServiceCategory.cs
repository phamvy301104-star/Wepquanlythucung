using System.ComponentModel.DataAnnotations;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Danh mục dịch vụ (Cắt tóc, Uốn, Nhuộm, Massage, etc.)
    /// </summary>
    public class ServiceCategory : BaseEntity
    {
        /// <summary>
        /// Tên danh mục dịch vụ
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Slug URL-friendly
        /// </summary>
        [MaxLength(150)]
        public string? Slug { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Hình ảnh
        /// </summary>
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Icon (emoji hoặc icon name)
        /// </summary>
        [MaxLength(50)]
        public string? Icon { get; set; }

        // TODO: Uncomment after adding Color column to database
        // /// <summary>
        // /// Màu đại diện (hex color)
        // /// </summary>
        // [MaxLength(10)]
        // public string? Color { get; set; }

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Trạng thái hiển thị
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Danh sách dịch vụ thuộc danh mục
        /// </summary>
        public virtual ICollection<Service>? Services { get; set; }
    }
}
