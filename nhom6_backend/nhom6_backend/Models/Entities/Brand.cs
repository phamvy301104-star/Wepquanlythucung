using System.ComponentModel.DataAnnotations;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Thương hiệu sản phẩm (Gatsby, Clear, Romano, etc.)
    /// </summary>
    public class Brand : BaseEntity
    {
        /// <summary>
        /// Tên thương hiệu
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
        /// Mô tả thương hiệu
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Logo thương hiệu
        /// </summary>
        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Banner thương hiệu
        /// </summary>
        [MaxLength(500)]
        public string? BannerUrl { get; set; }

        /// <summary>
        /// Website chính thức
        /// </summary>
        [MaxLength(200)]
        public string? WebsiteUrl { get; set; }

        /// <summary>
        /// Quốc gia xuất xứ
        /// </summary>
        [MaxLength(100)]
        public string? CountryOfOrigin { get; set; }

        /// <summary>
        /// Năm thành lập
        /// </summary>
        public int? YearEstablished { get; set; }

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Trạng thái hiển thị
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Thương hiệu nổi bật
        /// </summary>
        public bool IsFeatured { get; set; } = false;

        /// <summary>
        /// Số lượng sản phẩm của thương hiệu
        /// </summary>
        public int ProductCount { get; set; } = 0;

        /// <summary>
        /// Danh sách sản phẩm thuộc thương hiệu
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
    }
}
