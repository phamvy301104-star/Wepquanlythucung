using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Tin nhắn trong phòng chat nội bộ nhân viên
    /// </summary>
    public class StaffChatMessage : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến StaffChatRoom
        /// </summary>
        public int ChatRoomId { get; set; }

        [ForeignKey("ChatRoomId")]
        public virtual StaffChatRoom? ChatRoom { get; set; }

        /// <summary>
        /// ID nhân viên gửi tin
        /// </summary>
        public int SenderId { get; set; }

        [ForeignKey("SenderId")]
        public virtual Staff? Sender { get; set; }

        /// <summary>
        /// Nội dung tin nhắn
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Loại tin nhắn: text, image, file
        /// </summary>
        [MaxLength(20)]
        public string MessageType { get; set; } = "text";

        /// <summary>
        /// URL file đính kèm (nếu có)
        /// </summary>
        [MaxLength(500)]
        public string? AttachmentUrl { get; set; }

        /// <summary>
        /// Tên file gốc (nếu là file)
        /// </summary>
        [MaxLength(255)]
        public string? FileName { get; set; }

        /// <summary>
        /// Kích thước file (bytes)
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// Đã đọc chưa
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Thời gian đọc
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// Đã xóa (xóa mềm)
        /// </summary>
        public new bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Thời gian xóa
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Người xóa
        /// </summary>
        public int? DeletedBy { get; set; }
    }
}
