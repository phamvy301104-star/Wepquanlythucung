using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Tin nhắn trong phiên chat AI
    /// </summary>
    public class ChatMessage : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến ChatSession
        /// </summary>
        public int ChatSessionId { get; set; }

        [ForeignKey("ChatSessionId")]
        public virtual ChatSession? ChatSession { get; set; }

        /// <summary>
        /// Vai trò: User, Assistant, System
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "User";

        /// <summary>
        /// Nội dung tin nhắn
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Loại tin nhắn: Text, Image, Product, Service, Appointment
        /// </summary>
        [MaxLength(30)]
        public string MessageType { get; set; } = "Text";

        /// <summary>
        /// Hình ảnh đính kèm (JSON array)
        /// </summary>
        public string? Images { get; set; }

        /// <summary>
        /// Sản phẩm được gợi ý (JSON array of product IDs)
        /// </summary>
        public string? SuggestedProductIds { get; set; }

        /// <summary>
        /// Dịch vụ được gợi ý (JSON array of service IDs)
        /// </summary>
        public string? SuggestedServiceIds { get; set; }

        /// <summary>
        /// Action buttons (JSON) - ví dụ: đặt lịch, xem sản phẩm
        /// </summary>
        public string? ActionButtons { get; set; }

        /// <summary>
        /// Số tokens sử dụng
        /// </summary>
        public int? TokensUsed { get; set; }

        /// <summary>
        /// Thời gian phản hồi (ms) - cho AI response
        /// </summary>
        public int? ResponseTimeMs { get; set; }

        /// <summary>
        /// Intent được nhận diện
        /// </summary>
        [MaxLength(100)]
        public string? DetectedIntent { get; set; }

        /// <summary>
        /// Confidence score của intent
        /// </summary>
        public decimal? IntentConfidence { get; set; }

        /// <summary>
        /// Đánh giá tin nhắn (thumbs up/down)
        /// </summary>
        public bool? IsHelpful { get; set; }
    }
}
