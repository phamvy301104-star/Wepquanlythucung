using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Cuộc hội thoại (tin nhắn)
    /// </summary>
    public class Conversation : BaseEntity
    {
        /// <summary>
        /// Loại: Private (1-1), Group
        /// </summary>
        [MaxLength(20)]
        public string Type { get; set; } = "Private";

        /// <summary>
        /// Tên nhóm (nếu là group chat)
        /// </summary>
        [MaxLength(100)]
        public string? Name { get; set; }

        /// <summary>
        /// Ảnh nhóm
        /// </summary>
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Người tạo (cho group)
        /// </summary>
        public string? CreatorId { get; set; }

        [ForeignKey("CreatorId")]
        public virtual User? Creator { get; set; }

        /// <summary>
        /// Tin nhắn cuối cùng
        /// </summary>
        public int? LastMessageId { get; set; }

        /// <summary>
        /// Nội dung tin nhắn cuối (để hiển thị nhanh)
        /// </summary>
        [MaxLength(200)]
        public string? LastMessageContent { get; set; }

        /// <summary>
        /// Thời gian tin nhắn cuối
        /// </summary>
        public DateTime? LastMessageAt { get; set; }

        /// <summary>
        /// Trạng thái: Active, Archived, Deleted
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        // Navigation Properties
        public virtual ICollection<ConversationParticipant>? Participants { get; set; }
        public virtual ICollection<Message>? Messages { get; set; }
    }
}
