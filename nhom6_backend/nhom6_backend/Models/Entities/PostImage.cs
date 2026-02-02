using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Hình ảnh trong bài viết
    /// </summary>
    public class PostImage : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Post
        /// </summary>
        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post? Post { get; set; }

        /// <summary>
        /// URL hình ảnh
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// URL thumbnail
        /// </summary>
        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }

        /// <summary>
        /// Alt text
        /// </summary>
        [MaxLength(200)]
        public string? AltText { get; set; }

        /// <summary>
        /// Caption
        /// </summary>
        [MaxLength(500)]
        public string? Caption { get; set; }

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Chiều rộng
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Chiều cao
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// Kích thước file (bytes)
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// Loại file: Image, Video, GIF
        /// </summary>
        [MaxLength(20)]
        public string MediaType { get; set; } = "Image";
    }
}
