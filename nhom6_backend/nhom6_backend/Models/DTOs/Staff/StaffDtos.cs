using System.ComponentModel.DataAnnotations;

namespace nhom6_backend.Models.DTOs.Staff
{
    // ==========================================
    // ATTENDANCE DTOs
    // ==========================================

    /// <summary>
    /// Request chấm công với ảnh khuôn mặt
    /// </summary>
    public class AttendanceCheckRequest
    {
        /// <summary>
        /// Loại chấm công: 1=Vào ca, 2=Vào nghỉ, 3=Hết nghỉ, 4=Về
        /// </summary>
        [Required]
        [Range(1, 4)]
        public int CheckType { get; set; }

        /// <summary>
        /// Ảnh khuôn mặt base64
        /// </summary>
        [Required]
        public string PhotoBase64 { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian thiết bị (ISO 8601 với timezone)
        /// </summary>
        [Required]
        public string DeviceTime { get; set; } = string.Empty;

        /// <summary>
        /// Ghi chú (nếu có)
        /// </summary>
        [MaxLength(200)]
        public string? Note { get; set; }
    }

    /// <summary>
    /// Response trạng thái chấm công hôm nay
    /// </summary>
    public class AttendanceTodayDto
    {
        public int Id { get; set; }
        public DateTime WorkDate { get; set; }
        
        // 4 lần chấm công
        public CheckTimeDto? Check1 { get; set; }
        public CheckTimeDto? Check2 { get; set; }
        public CheckTimeDto? Check3 { get; set; }
        public CheckTimeDto? Check4 { get; set; }
        
        // Lịch trình
        public string ScheduledStart { get; set; } = string.Empty;
        public string? ScheduledBreakStart { get; set; }
        public string? ScheduledBreakEnd { get; set; }
        public string ScheduledEnd { get; set; } = string.Empty;
        
        // Thống kê
        public int CheckCount { get; set; }
        public int LateMinutes { get; set; }
        public int OverBreakMinutes { get; set; }
        public int EarlyLeaveMinutes { get; set; }
        public decimal TotalPenalty { get; set; }
        public string Status { get; set; } = string.Empty;
        
        // Check tiếp theo
        public int NextCheckType { get; set; }
        public string NextCheckLabel { get; set; } = string.Empty;
        public bool CanCheck { get; set; }
    }

    public class CheckTimeDto
    {
        public DateTime Time { get; set; }
        public string TimeString { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public bool IsLate { get; set; }
        public int LateMinutes { get; set; }
    }

    /// <summary>
    /// Lịch sử chấm công
    /// </summary>
    public class AttendanceHistoryDto
    {
        public int Id { get; set; }
        public DateTime WorkDate { get; set; }
        public string WorkDateString { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
        public int CheckCount { get; set; }
        public int LateMinutes { get; set; }
        public decimal TotalPenalty { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        
        // Chi tiết 4 lần
        public string? Check1Time { get; set; }
        public string? Check2Time { get; set; }
        public string? Check3Time { get; set; }
        public string? Check4Time { get; set; }
    }

    // ==========================================
    // SALARY DTOs
    // ==========================================

    /// <summary>
    /// Bảng lương chi tiết
    /// </summary>
    public class SalarySlipDto
    {
        public int Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthYearString { get; set; } = string.Empty;
        
        // Thông tin chấm công
        public int WorkDays { get; set; }
        public int ActualWorkDays { get; set; }
        public int TotalLateMinutes { get; set; }
        public int LateCount { get; set; }
        public int MissedCheckDays { get; set; }
        public int TotalOvertimeMinutes { get; set; }
        
        // Thu nhập
        public decimal BaseSalary { get; set; }
        public decimal OvertimeBonus { get; set; }
        public decimal CommissionBonus { get; set; }
        public decimal OtherAllowance { get; set; }
        public decimal GrossIncome { get; set; }
        
        // Khấu trừ
        public decimal LatePenalty { get; set; }
        public decimal MissedCheckPenalty { get; set; }
        public decimal AbsentDeduction { get; set; }
        public decimal BHXH { get; set; }
        public decimal BHYT { get; set; }
        public decimal BHTN { get; set; }
        public decimal OtherDeduction { get; set; }
        public decimal TotalDeductions { get; set; }
        
        // Thực lãnh
        public decimal NetSalary { get; set; }
        
        // Trạng thái
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
    }

    /// <summary>
    /// Lịch sử lương (danh sách ngắn gọn)
    /// </summary>
    public class SalaryHistoryItemDto
    {
        public int Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthYearString { get; set; } = string.Empty;
        public decimal NetSalary { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
    }

    // ==========================================
    // STAFF PROFILE DTOs
    // ==========================================

    /// <summary>
    /// Thông tin profile staff
    /// </summary>
    public class StaffProfileDto
    {
        public int Id { get; set; }
        public string StaffCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? NickName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Bio { get; set; }
        public string Position { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string? Specialties { get; set; }
        public int YearsOfExperience { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public DateTime HireDate { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalCustomersServed { get; set; }
        public decimal TotalRevenue { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
    }

    /// <summary>
    /// Thống kê của staff
    /// </summary>
    public class StaffStatsDto
    {
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalCustomersServed { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalAppointmentsThisMonth { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int CustomersThisMonth { get; set; }
    }

    /// <summary>
    /// Lịch làm việc
    /// </summary>
    public class StaffScheduleDto
    {
        public int DayOfWeek { get; set; }
        public string DayName { get; set; } = string.Empty;
        public DateTime? SpecificDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string? BreakStartTime { get; set; }
        public string? BreakEndTime { get; set; }
        public bool IsWorking { get; set; }
        public bool IsLeave { get; set; }
        public string? LeaveReason { get; set; }
    }

    /// <summary>
    /// Lịch hẹn được assign
    /// </summary>
    public class StaffAppointmentDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public List<string> Services { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    // ==========================================
    // STAFF CHAT DTOs
    // ==========================================

    /// <summary>
    /// Phòng chat
    /// </summary>
    public class StaffChatRoomDto
    {
        public int Id { get; set; }
        public StaffColleagueDto OtherStaff { get; set; } = new();
        public string? LastMessageText { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public string? LastMessageTimeAgo { get; set; }
        public int UnreadCount { get; set; }
        public bool IsMuted { get; set; }
    }

    /// <summary>
    /// Đồng nghiệp
    /// </summary>
    public class StaffColleagueDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? NickName { get; set; }
        public string? AvatarUrl { get; set; }
        public string Position { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public string? LastSeenAt { get; set; }
    }

    /// <summary>
    /// Tin nhắn
    /// </summary>
    public class StaffChatMessageDto
    {
        public int Id { get; set; }
        public int ChatRoomId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string? SenderAvatar { get; set; }
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = "text";
        public string? AttachmentUrl { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsMe { get; set; }
    }

    /// <summary>
    /// Request gửi tin nhắn
    /// </summary>
    public class SendStaffMessageRequest
    {
        [Required]
        public int ChatRoomId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        public string MessageType { get; set; } = "text";
        public string? AttachmentUrl { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
    }

    /// <summary>
    /// Request tạo/lấy phòng chat
    /// </summary>
    public class CreateChatRoomRequest
    {
        [Required]
        public int TargetStaffId { get; set; }
    }
}
