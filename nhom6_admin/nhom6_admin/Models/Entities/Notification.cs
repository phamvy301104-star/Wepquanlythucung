using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_admin.Models.Entities
{
    /// <summary>
    /// Thông báo cho người dùng (Tương thích với nhom6_backend)
    /// </summary>
    public class Notification : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến User nhận thông báo
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Loại thông báo: Order, Appointment, Promotion, System, Social, Blog
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Tiêu đề thông báo
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Nội dung thông báo
        /// </summary>
        [MaxLength(500)]
        public string? Content { get; set; }

        /// <summary>
        /// Hình ảnh (icon, avatar, etc.)
        /// </summary>
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Đường dẫn khi click (deep link)
        /// </summary>
        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        /// <summary>
        /// Loại entity liên quan: Order, Appointment, Post, Product, etc.
        /// </summary>
        [MaxLength(50)]
        public string? ReferenceType { get; set; }

        /// <summary>
        /// ID của entity liên quan
        /// </summary>
        [MaxLength(50)]
        public string? ReferenceId { get; set; }

        /// <summary>
        /// Người gửi (nếu là social notification)
        /// </summary>
        public string? SenderUserId { get; set; }

        [ForeignKey("SenderUserId")]
        public virtual User? SenderUser { get; set; }

        /// <summary>
        /// Đã đọc
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Ngày đọc
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// Đã gửi push notification
        /// </summary>
        public bool IsPushSent { get; set; } = false;

        /// <summary>
        /// Ngày gửi push
        /// </summary>
        public DateTime? PushSentAt { get; set; }

        /// <summary>
        /// Đã gửi email
        /// </summary>
        public bool IsEmailSent { get; set; } = false;

        /// <summary>
        /// Ngày hết hạn (tự động xóa)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Metadata JSON
        /// </summary>
        public string? Metadata { get; set; }

        /// <summary>
        /// Mức độ ưu tiên: Low, Normal, High, Urgent
        /// </summary>
        [MaxLength(20)]
        public string Priority { get; set; } = "Normal";
    }
}
