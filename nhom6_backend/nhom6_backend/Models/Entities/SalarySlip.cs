using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Bảng lương nhân viên theo tháng - Tính tự động từ Attendance
    /// </summary>
    public class SalarySlip : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Staff
        /// </summary>
        public int StaffId { get; set; }

        [ForeignKey("StaffId")]
        public virtual Staff? Staff { get; set; }

        /// <summary>
        /// Tháng
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Năm
        /// </summary>
        public int Year { get; set; }

        // ==========================================
        // THÔNG TIN CHẤM CÔNG
        // ==========================================

        /// <summary>
        /// Số ngày công chuẩn trong tháng
        /// </summary>
        public int WorkDays { get; set; } = 26;

        /// <summary>
        /// Số ngày công thực tế
        /// </summary>
        public int ActualWorkDays { get; set; } = 0;

        /// <summary>
        /// Số ngày nghỉ có phép
        /// </summary>
        public int PaidLeaveDays { get; set; } = 0;

        /// <summary>
        /// Số ngày nghỉ không phép
        /// </summary>
        public int UnpaidLeaveDays { get; set; } = 0;

        /// <summary>
        /// Tổng số phút đi trễ trong tháng
        /// </summary>
        public int TotalLateMinutes { get; set; } = 0;

        /// <summary>
        /// Số lần đi trễ
        /// </summary>
        public int LateCount { get; set; } = 0;

        /// <summary>
        /// Số ngày thiếu chấm công
        /// </summary>
        public int MissedCheckDays { get; set; } = 0;

        /// <summary>
        /// Tổng số phút làm thêm giờ
        /// </summary>
        public int TotalOvertimeMinutes { get; set; } = 0;

        // ==========================================
        // THU NHẬP
        // ==========================================

        /// <summary>
        /// Lương cơ bản
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BaseSalary { get; set; } = 0;

        /// <summary>
        /// Tiền làm thêm giờ
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal OvertimeBonus { get; set; } = 0;

        /// <summary>
        /// Tiền hoa hồng dịch vụ
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CommissionBonus { get; set; } = 0;

        /// <summary>
        /// Phụ cấp khác
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal OtherAllowance { get; set; } = 0;

        /// <summary>
        /// Tổng thu nhập (trước trừ)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal GrossIncome { get; set; } = 0;

        // ==========================================
        // KHẤU TRỪ
        // ==========================================

        /// <summary>
        /// Tiền phạt đi trễ (= TotalLateMinutes × 8000 VND)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal LatePenalty { get; set; } = 0;

        /// <summary>
        /// Tiền phạt thiếu chấm công (= MissedCheckDays × 50000 VND)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MissedCheckPenalty { get; set; } = 0;

        /// <summary>
        /// Trừ nghỉ không phép
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal AbsentDeduction { get; set; } = 0;

        /// <summary>
        /// BHXH (8% lương cơ bản)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BHXH { get; set; } = 0;

        /// <summary>
        /// BHYT (1.5% lương cơ bản)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BHYT { get; set; } = 0;

        /// <summary>
        /// BHTN (1% lương cơ bản)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BHTN { get; set; } = 0;

        /// <summary>
        /// Khấu trừ khác
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal OtherDeduction { get; set; } = 0;

        /// <summary>
        /// Tổng khấu trừ
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalDeductions { get; set; } = 0;

        // ==========================================
        // THỰC LÃNH
        // ==========================================

        /// <summary>
        /// Lương thực nhận
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal NetSalary { get; set; } = 0;

        // ==========================================
        // TRẠNG THÁI
        // ==========================================

        /// <summary>
        /// Trạng thái: Draft, Confirmed, Paid
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Draft";

        /// <summary>
        /// Ngày xác nhận bảng lương
        /// </summary>
        public DateTime? ConfirmedAt { get; set; }

        /// <summary>
        /// Người xác nhận
        /// </summary>
        [MaxLength(100)]
        public string? ConfirmedBy { get; set; }

        /// <summary>
        /// Ngày chi trả
        /// </summary>
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// Phương thức thanh toán
        /// </summary>
        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Mã giao dịch
        /// </summary>
        [MaxLength(100)]
        public string? TransactionId { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        [MaxLength(500)]
        public string? Note { get; set; }

        // ==========================================
        // CONSTANTS
        // ==========================================

        /// <summary>
        /// Tỷ lệ BHXH (8%)
        /// </summary>
        [NotMapped]
        public const decimal BHXH_RATE = 0.08m;

        /// <summary>
        /// Tỷ lệ BHYT (1.5%)
        /// </summary>
        [NotMapped]
        public const decimal BHYT_RATE = 0.015m;

        /// <summary>
        /// Tỷ lệ BHTN (1%)
        /// </summary>
        [NotMapped]
        public const decimal BHTN_RATE = 0.01m;

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
