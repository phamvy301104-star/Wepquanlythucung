using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Hình ảnh dịch vụ
    /// </summary>
    public class ServiceImage : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Service
        /// </summary>
        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }

        /// <summary>
        /// Đường dẫn hình ảnh
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Loại hình: Before, After, Process, Result
        /// </summary>
        [MaxLength(20)]
        public string ImageType { get; set; } = "Result";

        /// <summary>
        /// Tiêu đề
        /// </summary>
        [MaxLength(200)]
        public string? Title { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Là hình ảnh chính
        /// </summary>
        public bool IsPrimary { get; set; } = false;
    }
}
