using System.ComponentModel.DataAnnotations;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Danh mục sản phẩm (Dầu gội, Dầu xả, Keo vuốt tóc, etc.)
    /// </summary>
    public class Category : BaseEntity
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
        /// Mô tả danh mục
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Đường dẫn hình ảnh
        /// </summary>
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Icon (emoji hoặc icon name)
        /// </summary>
        [MaxLength(50)]
        public string? Icon { get; set; }

        /// <summary>
        /// Danh mục cha (null nếu là root)
        /// </summary>
        public int? ParentCategoryId { get; set; }

        public virtual Category? ParentCategory { get; set; }

        /// <summary>
        /// Danh mục con
        /// </summary>
        public virtual ICollection<Category>? ChildCategories { get; set; }

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Trạng thái hiển thị
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Hiển thị trên trang chủ
        /// </summary>
        public bool ShowOnHomePage { get; set; } = false;

        /// <summary>
        /// Số lượng sản phẩm trong danh mục
        /// </summary>
        public int ProductCount { get; set; } = 0;

        /// <summary>
        /// Danh sách sản phẩm thuộc danh mục
        /// </summary>
        public virtual ICollection<Product>? Products { get; set; }

        /// <summary>
        /// Meta Title cho SEO
        /// </summary>
        [MaxLength(200)]
        public string? MetaTitle { get; set; }

        /// <summary>
        /// Meta Description cho SEO
        /// </summary>
        [MaxLength(500)]
        public string? MetaDescription { get; set; }

        /// <summary>
        /// Meta Keywords cho SEO
        /// </summary>
        [MaxLength(300)]
        public string? MetaKeywords { get; set; }
    }
}
