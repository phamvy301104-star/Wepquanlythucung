using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Chấm công nhân viên - 4 lần/ca với ảnh khuôn mặt
    /// </summary>
    public class Attendance : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Staff
        /// </summary>
        public int StaffId { get; set; }

        [ForeignKey("StaffId")]
        public virtual Staff? Staff { get; set; }

        /// <summary>
        /// Ngày làm việc
        /// </summary>
        public DateTime WorkDate { get; set; }

        // ==========================================
        // 4 LẦN CHẤM CÔNG
        // ==========================================

        /// <summary>
        /// Lần 1: Giờ vào ca
        /// </summary>
        public DateTime? CheckIn1_Time { get; set; }

        /// <summary>
        /// Ảnh khuôn mặt lần 1
        /// </summary>
        [MaxLength(500)]
        public string? CheckIn1_PhotoUrl { get; set; }

        /// <summary>
        /// Lần 2: Giờ bắt đầu nghỉ trưa
        /// </summary>
        public DateTime? CheckIn2_Time { get; set; }

        /// <summary>
        /// Ảnh khuôn mặt lần 2
        /// </summary>
        [MaxLength(500)]
        public string? CheckIn2_PhotoUrl { get; set; }

        /// <summary>
        /// Lần 3: Giờ hết nghỉ trưa
        /// </summary>
        public DateTime? CheckIn3_Time { get; set; }

        /// <summary>
        /// Ảnh khuôn mặt lần 3
        /// </summary>
        [MaxLength(500)]
        public string? CheckIn3_PhotoUrl { get; set; }

        /// <summary>
        /// Lần 4: Giờ về
        /// </summary>
        public DateTime? CheckIn4_Time { get; set; }

        /// <summary>
        /// Ảnh khuôn mặt lần 4
        /// </summary>
        [MaxLength(500)]
        public string? CheckIn4_PhotoUrl { get; set; }

        // ==========================================
        // LỊCH TRÌNH CHUẨN (từ StaffSchedule)
        // ==========================================

        /// <summary>
        /// Giờ bắt đầu ca theo lịch
        /// </summary>
        public TimeSpan ScheduledStart { get; set; }

        /// <summary>
        /// Giờ bắt đầu nghỉ theo lịch
        /// </summary>
        public TimeSpan? ScheduledBreakStart { get; set; }

        /// <summary>
        /// Giờ hết nghỉ theo lịch
        /// </summary>
        public TimeSpan? ScheduledBreakEnd { get; set; }

        /// <summary>
        /// Giờ kết thúc ca theo lịch
        /// </summary>
        public TimeSpan ScheduledEnd { get; set; }

        // ==========================================
        // TÍNH TOÁN TỰ ĐỘNG
        // ==========================================

        /// <summary>
        /// Số phút đi trễ (vào ca)
        /// </summary>
        public int LateMinutes { get; set; } = 0;

        /// <summary>
        /// Số phút về sớm
        /// </summary>
        public int EarlyLeaveMinutes { get; set; } = 0;

        /// <summary>
        /// Số phút nghỉ quá giờ
        /// </summary>
        public int OverBreakMinutes { get; set; } = 0;

        /// <summary>
        /// Tổng số phút làm việc thực tế
        /// </summary>
        public int TotalWorkMinutes { get; set; } = 0;

        /// <summary>
        /// Số phút làm thêm giờ
        /// </summary>
        public int OvertimeMinutes { get; set; } = 0;

        /// <summary>
        /// Số lần đã chấm công (max 4)
        /// </summary>
        public int CheckCount { get; set; } = 0;

        // ==========================================
        // TIỀN PHẠT
        // ==========================================

        /// <summary>
        /// Tiền phạt trễ (= LateMinutes × 8000 VND)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal LatePenalty { get; set; } = 0;

        /// <summary>
        /// Tiền phạt nghỉ quá giờ (= OverBreakMinutes × 8000 VND)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal OverBreakPenalty { get; set; } = 0;

        /// <summary>
        /// Tiền phạt về sớm (= EarlyLeaveMinutes × 8000 VND)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal EarlyLeavePenalty { get; set; } = 0;

        /// <summary>
        /// Tiền phạt thiếu/thừa lần chấm
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MissedCheckPenalty { get; set; } = 0;

        /// <summary>
        /// Tổng tiền phạt trong ngày
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPenalty { get; set; } = 0;

        // ==========================================
        // TRẠNG THÁI
        // ==========================================

        /// <summary>
        /// Trạng thái: Present, Absent, Late, Leave, Incomplete
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Incomplete";

        /// <summary>
        /// Ghi chú
        /// </summary>
        [MaxLength(500)]
        public string? Note { get; set; }

        // ==========================================
        // CONSTANTS
        // ==========================================
        
        /// <summary>
        /// Mức phạt mỗi phút trễ (8000 VND)
        /// </summary>
        [NotMapped]
        public const decimal PENALTY_PER_MINUTE = 8000m;

        /// <summary>
        /// Tiền phạt thiếu lần chấm công (50000 VND/ngày)
        /// </summary>
        [NotMapped]
        public const decimal MISSED_CHECK_PENALTY = 50000m;
    }
}
