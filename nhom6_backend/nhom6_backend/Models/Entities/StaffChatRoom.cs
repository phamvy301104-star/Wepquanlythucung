using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Phòng chat nội bộ giữa 2 nhân viên
    /// </summary>
    public class StaffChatRoom : BaseEntity
    {
        /// <summary>
        /// Nhân viên 1 (người tạo phòng)
        /// </summary>
        public int Staff1Id { get; set; }

        [ForeignKey("Staff1Id")]
        public virtual Staff? Staff1 { get; set; }

        /// <summary>
        /// Nhân viên 2
        /// </summary>
        public int Staff2Id { get; set; }

        [ForeignKey("Staff2Id")]
        public virtual Staff? Staff2 { get; set; }

        /// <summary>
        /// Nội dung tin nhắn cuối
        /// </summary>
        [MaxLength(200)]
        public string? LastMessageText { get; set; }

        /// <summary>
        /// ID người gửi tin nhắn cuối
        /// </summary>
        public int? LastMessageSenderId { get; set; }

        /// <summary>
        /// Thời gian tin nhắn cuối
        /// </summary>
        public DateTime? LastMessageAt { get; set; }

        /// <summary>
        /// Số tin nhắn chưa đọc của Staff1
        /// </summary>
        public int Staff1UnreadCount { get; set; } = 0;

        /// <summary>
        /// Số tin nhắn chưa đọc của Staff2
        /// </summary>
        public int Staff2UnreadCount { get; set; } = 0;

        /// <summary>
        /// Staff1 đã mute phòng chat
        /// </summary>
        public bool Staff1Muted { get; set; } = false;

        /// <summary>
        /// Staff2 đã mute phòng chat
        /// </summary>
        public bool Staff2Muted { get; set; } = false;

        // Navigation Properties
        public virtual ICollection<StaffChatMessage>? Messages { get; set; }
    }
}
