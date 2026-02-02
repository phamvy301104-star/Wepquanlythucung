using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Dịch vụ trong lịch hẹn (Many-to-Many)
    /// </summary>
    public class AppointmentService : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Appointment
        /// </summary>
        public int AppointmentId { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual Appointment? Appointment { get; set; }

        /// <summary>
        /// Khóa ngoại đến Service
        /// </summary>
        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }

        /// <summary>
        /// Tên dịch vụ (snapshot)
        /// </summary>
        [MaxLength(200)]
        public string? ServiceName { get; set; }

        /// <summary>
        /// Giá dịch vụ tại thời điểm đặt
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// Số lượng
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Thời gian thực hiện (phút)
        /// </summary>
        public int DurationMinutes { get; set; }

        /// <summary>
        /// Thứ tự thực hiện
        /// </summary>
        public int ServiceOrder { get; set; } = 0;

        /// <summary>
        /// Ghi chú cho dịch vụ
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Nhân viên thực hiện riêng (nếu khác với staff chính)
        /// </summary>
        public int? PerformedByStaffId { get; set; }

        /// <summary>
        /// Trạng thái: Pending, InProgress, Completed, Skipped
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";
    }
}
