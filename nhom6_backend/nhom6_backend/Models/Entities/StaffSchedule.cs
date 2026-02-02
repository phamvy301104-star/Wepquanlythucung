using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Lịch làm việc của nhân viên
    /// </summary>
    public class StaffSchedule : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Staff
        /// </summary>
        public int StaffId { get; set; }

        [ForeignKey("StaffId")]
        public virtual Staff? Staff { get; set; }

        /// <summary>
        /// Ngày trong tuần: 0-6 (Chủ nhật - Thứ 7)
        /// </summary>
        public int DayOfWeek { get; set; }

        /// <summary>
        /// Ngày cụ thể (nếu là lịch đặc biệt)
        /// </summary>
        public DateTime? SpecificDate { get; set; }

        /// <summary>
        /// Giờ bắt đầu làm việc
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Giờ kết thúc làm việc
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Giờ nghỉ trưa bắt đầu
        /// </summary>
        public TimeSpan? BreakStartTime { get; set; }

        /// <summary>
        /// Giờ nghỉ trưa kết thúc
        /// </summary>
        public TimeSpan? BreakEndTime { get; set; }

        /// <summary>
        /// Làm việc ngày này
        /// </summary>
        public bool IsWorking { get; set; } = true;

        /// <summary>
        /// Ngày nghỉ phép
        /// </summary>
        public bool IsLeave { get; set; } = false;

        /// <summary>
        /// Lý do nghỉ
        /// </summary>
        [MaxLength(200)]
        public string? LeaveReason { get; set; }

        /// <summary>
        /// Số slot tối đa trong ngày
        /// </summary>
        public int MaxAppointmentsPerDay { get; set; } = 20;

        /// <summary>
        /// Ghi chú
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
