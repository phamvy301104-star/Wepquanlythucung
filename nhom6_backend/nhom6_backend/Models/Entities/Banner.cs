using System.ComponentModel.DataAnnotations;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Banner quảng cáo/Slider
    /// </summary>
    public class Banner : BaseEntity
    {
        /// <summary>
        /// Tiêu đề banner
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Phụ đề
        /// </summary>
        [MaxLength(300)]
        public string? Subtitle { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// URL hình ảnh
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// URL hình ảnh mobile
        /// </summary>
        [MaxLength(500)]
        public string? MobileImageUrl { get; set; }

        /// <summary>
        /// Link khi click
        /// </summary>
        [MaxLength(500)]
        public string? LinkUrl { get; set; }

        /// <summary>
        /// Mở tab mới
        /// </summary>
        public bool OpenInNewTab { get; set; } = false;

        /// <summary>
        /// Text nút CTA
        /// </summary>
        [MaxLength(50)]
        public string? ButtonText { get; set; }

        /// <summary>
        /// Màu nút CTA
        /// </summary>
        [MaxLength(10)]
        public string? ButtonColor { get; set; }

        /// <summary>
        /// Vị trí hiển thị: HomeSlider, HomePopup, ProductPage, CategoryPage, etc.
        /// </summary>
        [MaxLength(50)]
        public string Position { get; set; } = "HomeSlider";

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Ngày bắt đầu hiển thị
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc hiển thị
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Trạng thái active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Số lượt view
        /// </summary>
        public int ViewCount { get; set; } = 0;

        /// <summary>
        /// Số lượt click
        /// </summary>
        public int ClickCount { get; set; } = 0;

        /// <summary>
        /// Alt text cho SEO
        /// </summary>
        [MaxLength(200)]
        public string? AltText { get; set; }
    }
}
