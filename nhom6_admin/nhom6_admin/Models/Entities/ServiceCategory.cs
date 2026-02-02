using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_admin.Models.Entities
{
    /// <summary>
    /// Danh mục dịch vụ (Cắt tóc, Nhuộm, Uốn, etc.)
    /// </summary>
    public class ServiceCategory : BaseEntity
    {
        /// <summary>
        /// Tên danh mục
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
        /// Icon
        /// </summary>
        [MaxLength(50)]
        public string? Icon { get; set; }

        /// <summary>
        /// Hình ảnh
        /// </summary>
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Danh mục cha
        /// </summary>
        public int? ParentCategoryId { get; set; }

        [ForeignKey("ParentCategoryId")]
        public virtual ServiceCategory? ParentCategory { get; set; }

        /// <summary>
        /// Danh mục con
        /// </summary>
        public virtual ICollection<ServiceCategory>? ChildCategories { get; set; }

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Trạng thái
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Danh sách dịch vụ
        /// </summary>
        public virtual ICollection<Service>? Services { get; set; }
    }
}
