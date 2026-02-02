using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Tin nhắn
    /// </summary>
    public class Message : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Conversation
        /// </summary>
        public int ConversationId { get; set; }

        [ForeignKey("ConversationId")]
        public virtual Conversation? Conversation { get; set; }

        /// <summary>
        /// Khóa ngoại đến User (người gửi)
        /// </summary>
        [Required]
        public string SenderId { get; set; } = string.Empty;

        [ForeignKey("SenderId")]
        public virtual User? Sender { get; set; }

        /// <summary>
        /// Loại tin nhắn: Text, Image, File, Audio, Video, Sticker, Location
        /// </summary>
        [MaxLength(20)]
        public string MessageType { get; set; } = "Text";

        /// <summary>
        /// Nội dung tin nhắn
        /// </summary>
        [MaxLength(4000)]
        public string? Content { get; set; }

        /// <summary>
        /// URL media (hình, video, file)
        /// </summary>
        [MaxLength(500)]
        public string? MediaUrl { get; set; }

        /// <summary>
        /// URL thumbnail
        /// </summary>
        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }

        /// <summary>
        /// Tên file
        /// </summary>
        [MaxLength(200)]
        public string? FileName { get; set; }

        /// <summary>
        /// Kích thước file (bytes)
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// Thời lượng audio/video (giây)
        /// </summary>
        public int? Duration { get; set; }

        /// <summary>
        /// Tin nhắn trả lời (reply to)
        /// </summary>
        public int? ReplyToMessageId { get; set; }

        [ForeignKey("ReplyToMessageId")]
        public virtual Message? ReplyToMessage { get; set; }

        /// <summary>
        /// Tin nhắn được forward từ
        /// </summary>
        public int? ForwardedFromMessageId { get; set; }

        /// <summary>
        /// Đã chỉnh sửa
        /// </summary>
        public bool IsEdited { get; set; } = false;

        /// <summary>
        /// Ngày chỉnh sửa
        /// </summary>
        public DateTime? EditedAt { get; set; }

        /// <summary>
        /// Đã xóa (cho người gửi)
        /// </summary>
        public bool IsDeletedBySender { get; set; } = false;

        /// <summary>
        /// Đã xóa (cho tất cả)
        /// </summary>
        public bool IsDeletedForAll { get; set; } = false;

        /// <summary>
        /// Ngày xóa
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Tin nhắn hệ thống (thông báo thêm/rời nhóm, etc.)
        /// </summary>
        public bool IsSystemMessage { get; set; } = false;

        // Navigation Properties
        public virtual ICollection<MessageReadStatus>? ReadStatuses { get; set; }
    }
}
