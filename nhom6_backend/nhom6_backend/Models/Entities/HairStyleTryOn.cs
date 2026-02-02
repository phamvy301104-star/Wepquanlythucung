using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Kết quả ghép mặt + kiểu tóc từ AI
    /// </summary>
    public class HairStyleTryOn : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến User
        /// </summary>
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Khóa ngoại đến ChatSession
        /// </summary>
        public int? ChatSessionId { get; set; }

        [ForeignKey("ChatSessionId")]
        public virtual ChatSession? ChatSession { get; set; }

        /// <summary>
        /// URL ảnh khuôn mặt gốc
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string FaceImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// URL ảnh kiểu tóc mong muốn
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string HairStyleImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// URL ảnh kết quả
        /// </summary>
        [MaxLength(500)]
        public string? ResultImageUrl { get; set; }

        /// <summary>
        /// Tên kiểu tóc (nếu chọn từ library)
        /// </summary>
        [MaxLength(100)]
        public string? HairStyleName { get; set; }

        /// <summary>
        /// Màu tóc được chọn
        /// </summary>
        [MaxLength(50)]
        public string? HairColor { get; set; }

        /// <summary>
        /// Thời gian xử lý (ms)
        /// </summary>
        public int? ProcessingTimeMs { get; set; }

        /// <summary>
        /// Model AI đã sử dụng
        /// </summary>
        [MaxLength(50)]
        public string? AiModel { get; set; }

        /// <summary>
        /// Trạng thái: Processing, Completed, Failed
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Processing";

        /// <summary>
        /// Lỗi (nếu có)
        /// </summary>
        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Đã lưu vào bộ sưu tập
        /// </summary>
        public bool IsSaved { get; set; } = false;

        /// <summary>
        /// Đã chia sẻ
        /// </summary>
        public bool IsShared { get; set; } = false;

        /// <summary>
        /// Đánh giá kết quả (1-5)
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// Có đặt lịch làm kiểu tóc này không
        /// </summary>
        public int? RelatedAppointmentId { get; set; }
    }
}
