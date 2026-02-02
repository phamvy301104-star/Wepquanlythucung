using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Lịch hẹn đặt dịch vụ của khách hàng
    /// </summary>
    public class Appointment : BaseEntity
    {
        /// <summary>
        /// Mã lịch hẹn
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string AppointmentCode { get; set; } = string.Empty;

        /// <summary>
        /// Khóa ngoại đến User (khách hàng)
        /// </summary>
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Tên khách vãng lai (nếu không có tài khoản)
        /// </summary>
        [MaxLength(100)]
        public string? GuestName { get; set; }

        /// <summary>
        /// SĐT khách vãng lai
        /// </summary>
        [MaxLength(15)]
        public string? GuestPhone { get; set; }

        /// <summary>
        /// Email khách vãng lai
        /// </summary>
        [MaxLength(100)]
        public string? GuestEmail { get; set; }

        /// <summary>
        /// Khóa ngoại đến Staff (thợ cắt tóc)
        /// </summary>
        public int? StaffId { get; set; }

        [ForeignKey("StaffId")]
        public virtual Staff? Staff { get; set; }

        /// <summary>
        /// Ngày hẹn
        /// </summary>
        public DateTime AppointmentDate { get; set; }

        /// <summary>
        /// Giờ bắt đầu
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Giờ kết thúc dự kiến
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Tổng thời gian dịch vụ (phút)
        /// </summary>
        public int TotalDurationMinutes { get; set; }

        /// <summary>
        /// Tổng tiền dịch vụ
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Giảm giá
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DiscountAmount { get; set; } = 0;

        /// <summary>
        /// Tiền đã thanh toán (đặt cọc)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PaidAmount { get; set; } = 0;

        /// <summary>
        /// Trạng thái: Pending, Confirmed, InProgress, Completed, Cancelled, NoShow
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Lý do hủy
        /// </summary>
        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        /// <summary>
        /// Người hủy: Customer, Staff, Admin, System
        /// </summary>
        [MaxLength(20)]
        public string? CancelledBy { get; set; }

        /// <summary>
        /// Ngày hủy
        /// </summary>
        public DateTime? CancelledAt { get; set; }

        /// <summary>
        /// Ghi chú của khách
        /// </summary>
        [MaxLength(500)]
        public string? CustomerNotes { get; set; }

        /// <summary>
        /// Ghi chú nội bộ
        /// </summary>
        [MaxLength(500)]
        public string? InternalNotes { get; set; }

        /// <summary>
        /// Nguồn đặt lịch: App, Website, Phone, WalkIn
        /// </summary>
        [MaxLength(20)]
        public string BookingSource { get; set; } = "App";

        /// <summary>
        /// Nhắc nhở đã gửi
        /// </summary>
        public bool ReminderSent { get; set; } = false;

        /// <summary>
        /// Ngày gửi nhắc nhở
        /// </summary>
        public DateTime? ReminderSentAt { get; set; }

        /// <summary>
        /// Xác nhận từ khách
        /// </summary>
        public bool CustomerConfirmed { get; set; } = false;

        /// <summary>
        /// Ngày xác nhận
        /// </summary>
        public DateTime? ConfirmedAt { get; set; }

        /// <summary>
        /// Giờ check-in thực tế
        /// </summary>
        public DateTime? CheckInTime { get; set; }

        /// <summary>
        /// Giờ check-out thực tế
        /// </summary>
        public DateTime? CheckOutTime { get; set; }

        /// <summary>
        /// Đã đánh giá chưa
        /// </summary>
        public bool IsReviewed { get; set; } = false;

        /// <summary>
        /// Lịch hẹn lặp lại: None, Weekly, Biweekly, Monthly
        /// </summary>
        [MaxLength(20)]
        public string RecurrenceType { get; set; } = "None";

        /// <summary>
        /// ID của lịch hẹn gốc (nếu là recurring)
        /// </summary>
        public int? ParentAppointmentId { get; set; }

        // Navigation Properties
        public virtual ICollection<AppointmentService>? AppointmentServices { get; set; }
    }
}
