using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Phiên chat với AI Chatbot
    /// </summary>
    public class ChatSession : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến User
        /// </summary>
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Session ID cho khách vãng lai
        /// </summary>
        [MaxLength(100)]
        public string? GuestSessionId { get; set; }

        /// <summary>
        /// Tiêu đề phiên chat (tự động từ tin nhắn đầu)
        /// </summary>
        [MaxLength(200)]
        public string? Title { get; set; }

        /// <summary>
        /// Loại tư vấn: General, HairStyle, Product, Service
        /// </summary>
        [MaxLength(30)]
        public string SessionType { get; set; } = "General";

        /// <summary>
        /// Model AI đang sử dụng
        /// </summary>
        [MaxLength(50)]
        public string? AiModel { get; set; }

        /// <summary>
        /// Tổng số tin nhắn
        /// </summary>
        public int MessageCount { get; set; } = 0;

        /// <summary>
        /// Tổng số tokens đã sử dụng
        /// </summary>
        public int TotalTokensUsed { get; set; } = 0;

        /// <summary>
        /// Trạng thái: Active, Completed, Archived
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Thời gian tin nhắn cuối
        /// </summary>
        public DateTime? LastMessageAt { get; set; }

        /// <summary>
        /// Context được lưu (JSON) - để AI nhớ ngữ cảnh
        /// </summary>
        public string? Context { get; set; }

        /// <summary>
        /// Có liên kết đến đặt lịch không
        /// </summary>
        public int? RelatedAppointmentId { get; set; }

        /// <summary>
        /// Đánh giá phiên chat (1-5)
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// Feedback từ người dùng
        /// </summary>
        [MaxLength(500)]
        public string? Feedback { get; set; }

        // Navigation Properties
        public virtual ICollection<ChatMessage>? ChatMessages { get; set; }
    }
}
