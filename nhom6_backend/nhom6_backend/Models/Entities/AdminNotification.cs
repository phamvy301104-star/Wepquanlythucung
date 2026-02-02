using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Thông báo dành cho Admin Dashboard (lưu trữ persistent)
    /// </summary>
    public class AdminNotification : BaseEntity
    {
        /// <summary>
        /// Loại thông báo: NewAppointment, NewOrder, LowStock, NewReview, System
        /// </summary>
        [Required]
        [MaxLength(50)]
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
        [MaxLength(1000)]
        public string? Content { get; set; }

        /// <summary>
        /// Dữ liệu JSON đính kèm (appointment data, order data, etc.)
        /// </summary>
        public string? Data { get; set; }

        /// <summary>
        /// Đường dẫn khi click
        /// </summary>
        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        /// <summary>
        /// Trạng thái đã đọc hay chưa
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Thời gian đọc
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// ID của entity liên quan (AppointmentId, OrderId, etc.)
        /// </summary>
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Loại entity liên quan: Appointment, Order, Product, Review
        /// </summary>
        [MaxLength(50)]
        public string? RelatedEntityType { get; set; }
    }
}
