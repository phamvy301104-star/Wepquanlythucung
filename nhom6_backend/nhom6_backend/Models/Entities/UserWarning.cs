using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Cảnh báo vi phạm của người dùng
    /// </summary>
    public class UserWarning : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến User bị cảnh báo
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Admin/Staff đưa ra cảnh báo
        /// </summary>
        public string? WarnedByUserId { get; set; }

        [ForeignKey("WarnedByUserId")]
        public virtual User? WarnedByUser { get; set; }

        /// <summary>
        /// Loại vi phạm: Spam, Harassment, InappropriateContent, FakeReview, Other
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ViolationType { get; set; } = string.Empty;

        /// <summary>
        /// Lý do cảnh báo
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chi tiết
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Mức độ nghiêm trọng: 1-5 (1: nhẹ, 5: nghiêm trọng)
        /// </summary>
        public int SeverityLevel { get; set; } = 1;

        /// <summary>
        /// Điểm phạt
        /// </summary>
        public int PenaltyPoints { get; set; } = 0;

        /// <summary>
        /// Liên kết đến nội dung vi phạm (Post, Comment ID)
        /// </summary>
        [MaxLength(100)]
        public string? RelatedContentType { get; set; }

        /// <summary>
        /// ID của nội dung vi phạm
        /// </summary>
        public int? RelatedContentId { get; set; }

        /// <summary>
        /// Đã đọc cảnh báo chưa
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Ngày đọc cảnh báo
        /// </summary>
        public DateTime? ReadAt { get; set; }
    }
}
