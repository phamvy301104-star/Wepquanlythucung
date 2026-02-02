using System.ComponentModel.DataAnnotations;

namespace nhom6_admin.Models.DTOs
{
    // ==================== STAFF DTOs ====================
    public class StaffListDto
    {
        public int Id { get; set; }
        public string StaffCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? NickName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string Position { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalCustomersServed { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class StaffDetailDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
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
        public decimal? BaseSalary { get; set; }
        public int CommissionPercent { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalCustomersServed { get; set; }
        public decimal TotalRevenue { get; set; }
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TiktokUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ServiceSimpleDto>? Services { get; set; }
        public List<StaffScheduleDto>? Schedules { get; set; }
    }

    public class StaffSimpleDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string Position { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public decimal AverageRating { get; set; }
    }

    public class ServiceSimpleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
    }

    public class StaffScheduleDto
    {
        public int Id { get; set; }
        public int DayOfWeek { get; set; }
        public string DayOfWeekName { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan? BreakStartTime { get; set; }
        public TimeSpan? BreakEndTime { get; set; }
        public bool IsWorkingDay { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateStaffRequest
    {
        [Required]
        [MaxLength(20)]
        public string StaffCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public string? NickName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Bio { get; set; }
        public string Position { get; set; } = "Barber";
        public string Level { get; set; } = "Junior";
        public string? Specialties { get; set; }
        public int YearsOfExperience { get; set; } = 0;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public DateTime? HireDate { get; set; }
        public decimal? BaseSalary { get; set; }
        public int CommissionPercent { get; set; } = 0;
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TiktokUrl { get; set; }
        public string Status { get; set; } = "Active";
        public List<int>? ServiceIds { get; set; }
        public List<CreateStaffScheduleRequest>? Schedules { get; set; }
    }

    public class UpdateStaffRequest
    {
        public string? StaffCode { get; set; }
        public string? FullName { get; set; }
        public string? NickName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Bio { get; set; }
        public string? Position { get; set; }
        public string? Level { get; set; }
        public string? Specialties { get; set; }
        public int? YearsOfExperience { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public DateTime? HireDate { get; set; }
        public decimal? BaseSalary { get; set; }
        public int? CommissionPercent { get; set; }
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TiktokUrl { get; set; }
        public string? Status { get; set; }
        public List<int>? ServiceIds { get; set; }
        public List<CreateStaffScheduleRequest>? Schedules { get; set; }
    }

    public class CreateStaffScheduleRequest
    {
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan? BreakStartTime { get; set; }
        public TimeSpan? BreakEndTime { get; set; }
        public bool IsWorkingDay { get; set; } = true;
        public string? Notes { get; set; }
    }

    public class StaffFilterRequest
    {
        public string? Search { get; set; }
        public string? Position { get; set; }
        public string? Level { get; set; }
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }

    public class UpdateStaffStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }

    public class UpdateScheduleRequest
    {
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsWorking { get; set; } = true;
    }
}
