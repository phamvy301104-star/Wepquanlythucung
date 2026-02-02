using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_admin.Models.Entities
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
        /// Ngày trong tuần (0 = CN, 1 = T2, ..., 6 = T7)
        /// </summary>
        public int DayOfWeek { get; set; }

        /// <summary>
        /// Giờ bắt đầu
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Giờ kết thúc
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
        /// Có làm việc không
        /// </summary>
        public bool IsWorkingDay { get; set; } = true;

        /// <summary>
        /// Ghi chú
        /// </summary>
        [MaxLength(200)]
        public string? Notes { get; set; }
    }
}
