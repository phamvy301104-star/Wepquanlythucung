using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Models;
using nhom6_admin.Models.DTOs;
using nhom6_admin.Models.Entities;
using nhom6_admin.Services;
using nhom6_admin.Hubs;

namespace nhom6_admin.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class AppointmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IAdminNotificationService _notificationService;
        private readonly ILogger<AppointmentController> _logger;

        public AppointmentController(
            ApplicationDbContext context,
            IEmailService emailService,
            IAdminNotificationService notificationService,
            ILogger<AppointmentController> logger)
        {
            _context = context;
            _emailService = emailService;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Get all appointments with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<AppointmentListDto>>>> GetAppointments(
            [FromQuery] AppointmentFilterRequest filter)
        {
            try
            {
                var query = _context.Appointments
                    .Include(a => a.User)
                    .Include(a => a.Staff)
                    .Include(a => a.AppointmentServices)
                    .Where(a => !a.IsDeleted)
                    .AsQueryable();

                // Search
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    var search = filter.Search.ToLower();
                    query = query.Where(a =>
                        a.AppointmentCode.ToLower().Contains(search) ||
                        (a.User != null && a.User.FullName != null && a.User.FullName.ToLower().Contains(search)) ||
                        (a.GuestName != null && a.GuestName.ToLower().Contains(search)) ||
                        (a.GuestPhone != null && a.GuestPhone.Contains(search)));
                }

                // Filter by status
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    query = query.Where(a => a.Status == filter.Status);
                }

                // Filter by staff
                if (filter.StaffId.HasValue)
                {
                    query = query.Where(a => a.StaffId == filter.StaffId.Value);
                }

                // Filter by date range
                if (filter.FromDate.HasValue)
                {
                    query = query.Where(a => a.AppointmentDate >= filter.FromDate.Value);
                }
                if (filter.ToDate.HasValue)
                {
                    query = query.Where(a => a.AppointmentDate <= filter.ToDate.Value);
                }

                // Filter by booking source
                if (!string.IsNullOrEmpty(filter.BookingSource))
                {
                    query = query.Where(a => a.BookingSource == filter.BookingSource);
                }

                var totalCount = await query.CountAsync();

                // Sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "appointmentcode" => filter.SortDesc ? query.OrderByDescending(a => a.AppointmentCode) : query.OrderBy(a => a.AppointmentCode),
                    "status" => filter.SortDesc ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
                    "appointmentdate" => filter.SortDesc ? query.OrderByDescending(a => a.AppointmentDate) : query.OrderBy(a => a.AppointmentDate),
                    _ => query.OrderByDescending(a => a.AppointmentDate).ThenBy(a => a.StartTime)
                };

                var appointments = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(a => new AppointmentListDto
                    {
                        Id = a.Id,
                        AppointmentCode = a.AppointmentCode,
                        CustomerName = a.User != null ? (a.User.FullName ?? "Khách hàng") : (a.GuestName ?? "Khách vãng lai"),
                        CustomerPhone = a.User != null ? (a.User.PhoneNumber ?? "") : (a.GuestPhone ?? ""),
                        StaffName = a.Staff != null ? a.Staff.FullName : null,
                        AppointmentDate = a.AppointmentDate,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        TotalDurationMinutes = a.TotalDurationMinutes,
                        TotalAmount = a.TotalAmount,
                        Status = a.Status,
                        BookingSource = a.BookingSource,
                        ServiceCount = a.AppointmentServices != null ? a.AppointmentServices.Count : 0,
                        CreatedAt = a.CreatedAt
                    })
                    .ToListAsync();

                var response = new PaginatedResponse<AppointmentListDto>
                {
                    Items = appointments,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

                return Ok(ApiResponse<PaginatedResponse<AppointmentListDto>>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<AppointmentListDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get today's appointments
        /// </summary>
        [HttpGet("today")]
        public async Task<ActionResult<ApiResponse<List<AppointmentListDto>>>> GetTodayAppointments([FromQuery] int? staffId)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var query = _context.Appointments
                    .Include(a => a.User)
                    .Include(a => a.Staff)
                    .Include(a => a.AppointmentServices)
                    .Where(a => !a.IsDeleted && a.AppointmentDate.Date == today);

                if (staffId.HasValue)
                {
                    query = query.Where(a => a.StaffId == staffId.Value);
                }

                var appointments = await query
                    .OrderBy(a => a.StartTime)
                    .Select(a => new AppointmentListDto
                    {
                        Id = a.Id,
                        AppointmentCode = a.AppointmentCode,
                        CustomerName = a.User != null ? (a.User.FullName ?? "Khách hàng") : (a.GuestName ?? "Khách vãng lai"),
                        CustomerPhone = a.User != null ? (a.User.PhoneNumber ?? "") : (a.GuestPhone ?? ""),
                        StaffName = a.Staff != null ? a.Staff.FullName : null,
                        AppointmentDate = a.AppointmentDate,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        TotalDurationMinutes = a.TotalDurationMinutes,
                        TotalAmount = a.TotalAmount,
                        Status = a.Status,
                        BookingSource = a.BookingSource,
                        ServiceCount = a.AppointmentServices != null ? a.AppointmentServices.Count : 0,
                        CreatedAt = a.CreatedAt
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<AppointmentListDto>>.SuccessResponse(appointments));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AppointmentListDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get appointment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AppointmentDetailDto>>> GetAppointment(int id)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.User)
                    .Include(a => a.Staff)
                    .Include(a => a.AppointmentServices!)
                        .ThenInclude(s => s.Service)
                    .Where(a => a.Id == id && !a.IsDeleted)
                    .FirstOrDefaultAsync();

                if (appointment == null)
                {
                    return NotFound(ApiResponse<AppointmentDetailDto>.ErrorResponse("Appointment not found"));
                }

                var appointmentDto = new AppointmentDetailDto
                {
                    Id = appointment.Id,
                    AppointmentCode = appointment.AppointmentCode,
                    UserId = appointment.UserId,
                    UserEmail = appointment.User?.Email,
                    GuestName = appointment.GuestName,
                    GuestPhone = appointment.GuestPhone,
                    GuestEmail = appointment.GuestEmail,
                    CustomerName = appointment.User?.FullName ?? appointment.GuestName ?? "Khách vãng lai",
                    CustomerPhone = appointment.User?.PhoneNumber ?? appointment.GuestPhone,
                    CustomerEmail = appointment.User?.Email ?? appointment.GuestEmail,
                    StaffId = appointment.StaffId,
                    StaffName = appointment.Staff?.FullName,
                    StaffAvatarUrl = appointment.Staff?.AvatarUrl,
                    AppointmentDate = appointment.AppointmentDate,
                    StartTime = appointment.StartTime,
                    EndTime = appointment.EndTime,
                    TotalDurationMinutes = appointment.TotalDurationMinutes,
                    TotalAmount = appointment.TotalAmount,
                    DiscountAmount = appointment.DiscountAmount,
                    PaidAmount = appointment.PaidAmount,
                    Status = appointment.Status,
                    CancellationReason = appointment.CancellationReason,
                    CancelledBy = appointment.CancelledBy,
                    CancelledAt = appointment.CancelledAt,
                    CustomerNotes = appointment.CustomerNotes,
                    InternalNotes = appointment.InternalNotes,
                    BookingSource = appointment.BookingSource,
                    ReminderSent = appointment.ReminderSent,
                    ReminderSentAt = appointment.ReminderSentAt,
                    CustomerConfirmed = appointment.CustomerConfirmed,
                    ConfirmedAt = appointment.ConfirmedAt,
                    CheckedInAt = appointment.CheckedInAt,
                    CompletedAt = appointment.CompletedAt,
                    CreatedAt = appointment.CreatedAt,
                    UpdatedAt = appointment.UpdatedAt,
                    Services = appointment.AppointmentServices?.Select(s => new AppointmentServiceDto
                    {
                        Id = s.Id,
                        ServiceId = s.ServiceId,
                        ServiceName = s.Service?.Name ?? "",
                        ServiceImageUrl = s.Service?.ImageUrl,
                        Quantity = s.Quantity,
                        UnitPrice = s.UnitPrice,
                        TotalPrice = s.TotalPrice,
                        DurationMinutes = s.DurationMinutes,
                        Notes = s.Notes
                    }).ToList()
                };

                return Ok(ApiResponse<AppointmentDetailDto>.SuccessResponse(appointmentDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AppointmentDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create new appointment
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<AppointmentDetailDto>>> CreateAppointment([FromBody] CreateAppointmentRequest request)
        {
            try
            {
                // Validate staff
                var staff = await _context.Staff.FindAsync(request.StaffId);
                if (staff == null || staff.IsDeleted)
                {
                    return BadRequest(ApiResponse<AppointmentDetailDto>.ErrorResponse("Staff not found"));
                }

                // Get services and calculate total
                var serviceIds = request.Services.Select(s => s.ServiceId).ToList();
                var services = await _context.Services
                    .Where(s => serviceIds.Contains(s.Id) && !s.IsDeleted && s.IsActive)
                    .ToListAsync();

                if (services.Count != serviceIds.Count)
                {
                    return BadRequest(ApiResponse<AppointmentDetailDto>.ErrorResponse("Some services not found or inactive"));
                }

                // Calculate totals
                int totalDuration = 0;
                decimal totalAmount = 0;
                foreach (var reqService in request.Services)
                {
                    var service = services.First(s => s.Id == reqService.ServiceId);
                    totalDuration += service.DurationMinutes * reqService.Quantity;
                    totalAmount += service.Price * reqService.Quantity;
                }

                var endTime = request.StartTime.Add(TimeSpan.FromMinutes(totalDuration));

                // Generate appointment code
                var appointmentCode = $"APT{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(100, 999)}";

                var appointment = new Appointment
                {
                    AppointmentCode = appointmentCode,
                    UserId = request.UserId,
                    GuestName = request.GuestName,
                    GuestPhone = request.GuestPhone,
                    GuestEmail = request.GuestEmail,
                    StaffId = request.StaffId,
                    AppointmentDate = request.AppointmentDate,
                    StartTime = request.StartTime,
                    EndTime = endTime,
                    TotalDurationMinutes = totalDuration,
                    TotalAmount = totalAmount,
                    Status = "Pending",
                    CustomerNotes = request.CustomerNotes,
                    InternalNotes = request.InternalNotes,
                    BookingSource = request.BookingSource,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                // Add appointment services
                foreach (var reqService in request.Services)
                {
                    var service = services.First(s => s.Id == reqService.ServiceId);
                    var appointmentService = new AppointmentService
                    {
                        AppointmentId = appointment.Id,
                        ServiceId = reqService.ServiceId,
                        Quantity = reqService.Quantity,
                        UnitPrice = service.Price,
                        TotalPrice = service.Price * reqService.Quantity,
                        DurationMinutes = service.DurationMinutes * reqService.Quantity,
                        Notes = reqService.Notes,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.AppointmentServices.Add(appointmentService);
                }

                await _context.SaveChangesAsync();

                return await GetAppointment(appointment.Id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AppointmentDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update appointment
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<AppointmentDetailDto>>> UpdateAppointment(int id, [FromBody] UpdateAppointmentRequest request)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.AppointmentServices)
                    .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

                if (appointment == null)
                {
                    return NotFound(ApiResponse<AppointmentDetailDto>.ErrorResponse("Appointment not found"));
                }

                // Cannot update completed or cancelled appointments
                if (appointment.Status == "Completed" || appointment.Status == "Cancelled")
                {
                    return BadRequest(ApiResponse<AppointmentDetailDto>.ErrorResponse("Cannot update completed or cancelled appointment"));
                }

                // Update fields
                if (request.StaffId.HasValue)
                {
                    var staff = await _context.Staff.FindAsync(request.StaffId.Value);
                    if (staff == null || staff.IsDeleted)
                    {
                        return BadRequest(ApiResponse<AppointmentDetailDto>.ErrorResponse("Staff not found"));
                    }
                    appointment.StaffId = request.StaffId.Value;
                }

                if (request.AppointmentDate.HasValue)
                    appointment.AppointmentDate = request.AppointmentDate.Value;

                if (request.StartTime.HasValue)
                    appointment.StartTime = request.StartTime.Value;

                if (request.CustomerNotes != null)
                    appointment.CustomerNotes = request.CustomerNotes;

                if (request.InternalNotes != null)
                    appointment.InternalNotes = request.InternalNotes;

                // Update services if provided
                if (request.Services != null && request.Services.Any())
                {
                    // Remove old services
                    _context.AppointmentServices.RemoveRange(appointment.AppointmentServices!);

                    // Get new services
                    var serviceIds = request.Services.Select(s => s.ServiceId).ToList();
                    var services = await _context.Services
                        .Where(s => serviceIds.Contains(s.Id) && !s.IsDeleted && s.IsActive)
                        .ToListAsync();

                    if (services.Count != serviceIds.Count)
                    {
                        return BadRequest(ApiResponse<AppointmentDetailDto>.ErrorResponse("Some services not found or inactive"));
                    }

                    // Recalculate totals
                    int totalDuration = 0;
                    decimal totalAmount = 0;

                    foreach (var reqService in request.Services)
                    {
                        var service = services.First(s => s.Id == reqService.ServiceId);
                        totalDuration += service.DurationMinutes * reqService.Quantity;
                        totalAmount += service.Price * reqService.Quantity;

                        var appointmentService = new AppointmentService
                        {
                            AppointmentId = appointment.Id,
                            ServiceId = reqService.ServiceId,
                            Quantity = reqService.Quantity,
                            UnitPrice = service.Price,
                            TotalPrice = service.Price * reqService.Quantity,
                            DurationMinutes = service.DurationMinutes * reqService.Quantity,
                            Notes = reqService.Notes,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.AppointmentServices.Add(appointmentService);
                    }

                    appointment.TotalDurationMinutes = totalDuration;
                    appointment.TotalAmount = totalAmount;
                    appointment.EndTime = appointment.StartTime.Add(TimeSpan.FromMinutes(totalDuration));
                }

                appointment.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return await GetAppointment(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AppointmentDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update appointment status
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<ActionResult<ApiResponse<AppointmentDetailDto>>> UpdateAppointmentStatus(int id, [FromBody] UpdateAppointmentStatusRequest request)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.User)
                    .Include(a => a.AppointmentServices!)
                        .ThenInclude(s => s.Service)
                    .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
                    
                if (appointment == null)
                {
                    return NotFound(ApiResponse<AppointmentDetailDto>.ErrorResponse("Appointment not found"));
                }

                var validStatuses = new[] { "Pending", "Confirmed", "InProgress", "Completed", "Cancelled", "NoShow" };
                if (!validStatuses.Contains(request.Status))
                {
                    return BadRequest(ApiResponse<AppointmentDetailDto>.ErrorResponse("Invalid status"));
                }

                var previousStatus = appointment.Status;
                appointment.Status = request.Status;
                if (request.Notes != null)
                    appointment.InternalNotes = request.Notes;

                // Set timestamps based on status
                switch (request.Status)
                {
                    case "Confirmed":
                        appointment.ConfirmedAt = DateTime.UtcNow;
                        appointment.CustomerConfirmed = true;
                        break;
                    case "InProgress":
                        appointment.CheckedInAt = DateTime.UtcNow;
                        break;
                    case "Completed":
                        appointment.CompletedAt = DateTime.UtcNow;
                        break;
                }

                appointment.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Gửi thông báo khi status thay đổi sang Confirmed
                if (request.Status == "Confirmed" && previousStatus == "Pending")
                {
                    // Tạo notification cho user nếu có UserId
                    if (!string.IsNullOrEmpty(appointment.UserId))
                    {
                        var userNotification = new Notification
                        {
                            UserId = appointment.UserId,
                            Type = "Appointment",
                            Title = "✅ Lịch hẹn đã được xác nhận",
                            Content = $"Lịch hẹn #{appointment.AppointmentCode} đã được shop xác nhận. Ngày: {appointment.AppointmentDate:dd/MM/yyyy}, Giờ: {appointment.StartTime:hh\\:mm}. Hẹn gặp bạn tại UME Barbershop!",
                            ActionUrl = $"/appointments/{appointment.Id}",
                            ReferenceType = "Appointment",
                            ReferenceId = appointment.Id.ToString(),
                            Priority = "High",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.Notifications.Add(userNotification);
                        await _context.SaveChangesAsync();
                    }

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            _logger.LogInformation("📧 Sending confirmation email for appointment {AppointmentId}", appointment.Id);
                            
                            // 1. Gửi email xác nhận cho customer
                            var emailResult = await _emailService.SendAppointmentConfirmationToCustomer(appointment);
                            _logger.LogInformation("📧 Confirmation email sent: {Result}", emailResult);

                            // 2. Gửi SignalR notification đến Admin Dashboard để refresh
                            var appointmentData = new
                            {
                                Id = appointment.Id,
                                AppointmentCode = appointment.AppointmentCode,
                                CustomerName = appointment.GuestName ?? appointment.User?.FullName ?? "Khách hàng",
                                Status = "Confirmed",
                                AppointmentDate = appointment.AppointmentDate.ToString("dd/MM/yyyy"),
                                StartTime = appointment.StartTime.ToString(@"hh\:mm")
                            };
                            await _notificationService.NotifyAppointmentStatusChanged(appointmentData);
                            _logger.LogInformation("🔔 Status change notification sent successfully");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "❌ Error sending confirmation notifications for appointment {AppointmentId}", appointment.Id);
                        }
                    });
                }

                return await GetAppointment(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AppointmentDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cancel appointment
        /// </summary>
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<ApiResponse<AppointmentDetailDto>>> CancelAppointment(int id, [FromBody] CancelAppointmentRequest request)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null || appointment.IsDeleted)
                {
                    return NotFound(ApiResponse<AppointmentDetailDto>.ErrorResponse("Appointment not found"));
                }

                if (appointment.Status == "Completed")
                {
                    return BadRequest(ApiResponse<AppointmentDetailDto>.ErrorResponse("Cannot cancel completed appointment"));
                }

                appointment.Status = "Cancelled";
                appointment.CancellationReason = request.Reason;
                appointment.CancelledBy = "Admin";
                appointment.CancelledAt = DateTime.UtcNow;
                appointment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await GetAppointment(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AppointmentDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete appointment (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAppointment(int id)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null || appointment.IsDeleted)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse("Appointment not found"));
                }

                appointment.IsDeleted = true;
                appointment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Appointment deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get appointment statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<object>>> GetAppointmentStatistics([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var startDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var endDate = toDate ?? DateTime.UtcNow;

                var appointments = await _context.Appointments
                    .Where(a => !a.IsDeleted &&
                               a.AppointmentDate >= startDate &&
                               a.AppointmentDate <= endDate)
                    .ToListAsync();

                var statistics = new
                {
                    TotalAppointments = appointments.Count,
                    PendingAppointments = appointments.Count(a => a.Status == "Pending"),
                    ConfirmedAppointments = appointments.Count(a => a.Status == "Confirmed"),
                    InProgressAppointments = appointments.Count(a => a.Status == "InProgress"),
                    CompletedAppointments = appointments.Count(a => a.Status == "Completed"),
                    CancelledAppointments = appointments.Count(a => a.Status == "Cancelled"),
                    NoShowAppointments = appointments.Count(a => a.Status == "NoShow"),
                    TotalRevenue = appointments.Where(a => a.Status == "Completed").Sum(a => a.TotalAmount),
                    AverageAmount = appointments.Where(a => a.Status == "Completed").Any()
                        ? appointments.Where(a => a.Status == "Completed").Average(a => a.TotalAmount) : 0
                };

                return Ok(ApiResponse<object>.SuccessResponse(statistics));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get calendar appointments
        /// </summary>
        [HttpGet("calendar")]
        public async Task<ActionResult<ApiResponse<List<CalendarEventDto>>>> GetCalendarAppointments(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? staffId)
        {
            try
            {
                var query = _context.Appointments
                    .Include(a => a.User)
                    .Include(a => a.Staff)
                    .Where(a => !a.IsDeleted &&
                               a.AppointmentDate >= startDate.Date &&
                               a.AppointmentDate <= endDate.Date);

                if (staffId.HasValue)
                {
                    query = query.Where(a => a.StaffId == staffId.Value);
                }

                var appointments = await query
                    .Select(a => new CalendarEventDto
                    {
                        Id = a.Id,
                        Title = a.User != null ? (a.User.FullName ?? "Khách") : (a.GuestName ?? "Khách"),
                        Start = a.AppointmentDate.Add(a.StartTime),
                        End = a.AppointmentDate.Add(a.EndTime),
                        Status = a.Status,
                        StaffId = a.StaffId,
                        StaffName = a.Staff != null ? a.Staff.FullName : null,
                        CustomerName = a.User != null ? (a.User.FullName ?? "Khách") : (a.GuestName ?? "Khách"),
                        CustomerPhone = a.User != null ? (a.User.PhoneNumber ?? "") : (a.GuestPhone ?? ""),
                        TotalAmount = a.TotalAmount
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<CalendarEventDto>>.SuccessResponse(appointments));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<CalendarEventDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }
    }
}
