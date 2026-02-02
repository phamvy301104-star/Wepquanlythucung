using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Trạng thái đọc tin nhắn
    /// </summary>
    public class MessageReadStatus : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Message
        /// </summary>
        public int MessageId { get; set; }

        [ForeignKey("MessageId")]
        public virtual Message? Message { get; set; }

        /// <summary>
        /// Khóa ngoại đến User
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Ngày đọc
        /// </summary>
        public DateTime ReadAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Đã nhận (delivered)
        /// </summary>
        public bool IsDelivered { get; set; } = false;

        /// <summary>
        /// Ngày nhận
        /// </summary>
        public DateTime? DeliveredAt { get; set; }
    }
}
