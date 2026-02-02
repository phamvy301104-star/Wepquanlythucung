using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Thành viên trong cuộc hội thoại
    /// </summary>
    public class ConversationParticipant : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Conversation
        /// </summary>
        public int ConversationId { get; set; }

        [ForeignKey("ConversationId")]
        public virtual Conversation? Conversation { get; set; }

        /// <summary>
        /// Khóa ngoại đến User
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Vai trò: Member, Admin, Owner
        /// </summary>
        [MaxLength(20)]
        public string Role { get; set; } = "Member";

        /// <summary>
        /// Biệt danh trong nhóm
        /// </summary>
        [MaxLength(50)]
        public string? Nickname { get; set; }

        /// <summary>
        /// Ngày tham gia
        /// </summary>
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ngày đọc tin nhắn cuối cùng
        /// </summary>
        public DateTime? LastReadAt { get; set; }

        /// <summary>
        /// ID tin nhắn cuối cùng đã đọc
        /// </summary>
        public int? LastReadMessageId { get; set; }

        /// <summary>
        /// Số tin nhắn chưa đọc
        /// </summary>
        public int UnreadCount { get; set; } = 0;

        /// <summary>
        /// Tắt thông báo
        /// </summary>
        public bool IsMuted { get; set; } = false;

        /// <summary>
        /// Đã rời nhóm
        /// </summary>
        public bool HasLeft { get; set; } = false;

        /// <summary>
        /// Ngày rời nhóm
        /// </summary>
        public DateTime? LeftAt { get; set; }
    }
}
