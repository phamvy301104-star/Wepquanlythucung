using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_admin.Models.Entities
{
    /// <summary>
    /// Dịch vụ trong lịch hẹn
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
        /// Số lượng
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Đơn giá tại thời điểm đặt
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Thành tiền
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Thời lượng (phút)
        /// </summary>
        public int DurationMinutes { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
