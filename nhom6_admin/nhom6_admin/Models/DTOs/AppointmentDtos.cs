using System.ComponentModel.DataAnnotations;

namespace nhom6_admin.Models.DTOs
{
    // ==================== APPOINTMENT DTOs ====================
    public class AppointmentListDto
    {
        public int Id { get; set; }
        public string AppointmentCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string? StaffName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int TotalDurationMinutes { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string BookingSource { get; set; } = string.Empty;
        public int ServiceCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AppointmentDetailDto
    {
        public int Id { get; set; }
        public string AppointmentCode { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? GuestName { get; set; }
        public string? GuestPhone { get; set; }
        public string? GuestEmail { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public int? StaffId { get; set; }
        public string? StaffName { get; set; }
        public string? StaffAvatarUrl { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int TotalDurationMinutes { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CancellationReason { get; set; }
        public string? CancelledBy { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CustomerNotes { get; set; }
        public string? InternalNotes { get; set; }
        public string BookingSource { get; set; } = string.Empty;
        public bool ReminderSent { get; set; }
        public DateTime? ReminderSentAt { get; set; }
        public bool CustomerConfirmed { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CheckedInAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<AppointmentServiceDto>? Services { get; set; }
    }

    public class AppointmentServiceDto
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? ServiceImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int DurationMinutes { get; set; }
        public string? Notes { get; set; }
    }

    public class AppointmentFilterRequest
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public int? StaffId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? BookingSource { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = true;
    }

    public class CreateAppointmentRequest
    {
        public string? UserId { get; set; }
        public string? GuestName { get; set; }
        public string? GuestPhone { get; set; }
        public string? GuestEmail { get; set; }

        [Required]
        public int StaffId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        public string? CustomerNotes { get; set; }
        public string? InternalNotes { get; set; }
        public string BookingSource { get; set; } = "Admin";

        [Required]
        public List<CreateAppointmentServiceRequest> Services { get; set; } = new();
    }

    public class CreateAppointmentServiceRequest
    {
        [Required]
        public int ServiceId { get; set; }

        public int Quantity { get; set; } = 1;
        public string? Notes { get; set; }
    }

    public class UpdateAppointmentRequest
    {
        public int? StaffId { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public string? CustomerNotes { get; set; }
        public string? InternalNotes { get; set; }
        public List<CreateAppointmentServiceRequest>? Services { get; set; }
    }

    public class UpdateAppointmentStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class CancelAppointmentRequest
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    // ==================== CALENDAR DTOs ====================
    public class CalendarEventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string? Color { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? StaffName { get; set; }
        public int? StaffId { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class StaffAvailabilityDto
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public List<TimeSlotDto> AvailableSlots { get; set; } = new();
    }

    public class TimeSlotDto
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class CalendarAppointmentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int? StaffId { get; set; }
        public string? StaffName { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Color { get; set; }
    }
}
