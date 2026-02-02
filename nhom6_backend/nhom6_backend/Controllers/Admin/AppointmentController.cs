using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_backend.Models;
using nhom6_backend.Models.DTOs.Admin;
using nhom6_backend.Models.Entities;
using System.Text;

namespace nhom6_backend.Controllers.Admin
{
    /// <summary>
    /// Admin Appointment Management Controller
    /// Quản lý lịch hẹn: list, filter, update status, assign staff
    /// </summary>
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class AppointmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentController> _logger;

        public AppointmentController(ApplicationDbContext context, ILogger<AppointmentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách lịch hẹn với filter và phân trang
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<AdminApiResponse<PagedResult<AppointmentListDto>>>> GetAppointments(
            [FromQuery] AppointmentFilterRequest filter)
        {
            try
            {
                var query = _context.Appointments
                    .Include(a => a.User)
                    .Include(a => a.Staff)
                    .Include(a => a.AppointmentServices!)
                        .ThenInclude(asp => asp.Service)
                    .AsQueryable();

                // Filter by status
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    query = query.Where(a => a.Status == filter.Status);
                }

                // Filter by date
                if (filter.Date.HasValue)
                {
                    query = query.Where(a => a.AppointmentDate.Date == filter.Date.Value.Date);
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

                // Filter by staff
                if (filter.StaffId.HasValue)
                {
                    query = query.Where(a => a.StaffId == filter.StaffId);
                }

                // Search by customer name, phone, code
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    var search = filter.SearchTerm.ToLower();
                    query = query.Where(a =>
                        (a.GuestName != null && a.GuestName.ToLower().Contains(search)) ||
                        (a.GuestPhone != null && a.GuestPhone.Contains(search)) ||
                        (a.AppointmentCode != null && a.AppointmentCode.ToLower().Contains(search)) ||
                        (a.User != null && a.User.FullName != null && a.User.FullName.ToLower().Contains(search)));
                }

                var totalCount = await query.CountAsync();

                // Sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "date" => filter.SortDesc ? query.OrderByDescending(a => a.AppointmentDate) : query.OrderBy(a => a.AppointmentDate),
                    "status" => filter.SortDesc ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
                    "total" => filter.SortDesc ? query.OrderByDescending(a => a.TotalAmount) : query.OrderBy(a => a.TotalAmount),
                    _ => filter.SortDesc ? query.OrderBy(a => a.AppointmentDate) : query.OrderByDescending(a => a.AppointmentDate)
                };

                var appointments = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(a => new AppointmentListDto
                    {
                        Id = a.Id,
                        AppointmentCode = a.AppointmentCode ?? "",
                        CustomerName = a.UserId != null && a.User != null ? a.User.FullName ?? a.GuestName ?? "N/A" : a.GuestName ?? "N/A",
                        CustomerPhone = a.UserId != null && a.User != null ? a.User.PhoneNumber ?? a.GuestPhone : a.GuestPhone,
                        AppointmentDate = a.AppointmentDate,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        TotalAmount = a.TotalAmount,
                        Status = a.Status,
                        StaffName = a.Staff != null ? a.Staff.FullName : null,
                        ServiceNames = a.AppointmentServices != null 
                            ? a.AppointmentServices.Select(s => s.Service!.Name).ToList() 
                            : new List<string>(),
                        CreatedAt = a.CreatedAt
                    })
                    .ToListAsync();

                var result = new PagedResult<AppointmentListDto>
                {
                    Items = appointments,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

                return Ok(AdminApiResponse<PagedResult<AppointmentListDto>>.Ok(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointments");
                return StatusCode(500, AdminApiResponse<PagedResult<AppointmentListDto>>.Fail("Lỗi server khi lấy danh sách lịch hẹn"));
            }
        }

        /// <summary>
        /// Lấy chi tiết lịch hẹn
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminApiResponse<AppointmentDetailDto>>> GetAppointmentDetail(int id)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.User)
                    .Include(a => a.Staff)
                    .Include(a => a.AppointmentServices!)
                        .ThenInclude(asp => asp.Service)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (appointment == null)
                    return NotFound(AdminApiResponse<AppointmentDetailDto>.Fail("Lịch hẹn không tồn tại"));

                var dto = new AppointmentDetailDto
                {
                    Id = appointment.Id,
                    AppointmentCode = appointment.AppointmentCode ?? "",
                    CustomerName = appointment.UserId != null && appointment.User != null 
                        ? appointment.User.FullName ?? appointment.GuestName ?? "N/A" 
                        : appointment.GuestName ?? "N/A",
                    CustomerPhone = appointment.UserId != null && appointment.User != null 
                        ? appointment.User.PhoneNumber ?? appointment.GuestPhone 
                        : appointment.GuestPhone,
                    AppointmentDate = appointment.AppointmentDate,
                    StartTime = appointment.StartTime,
                    EndTime = appointment.EndTime,
                    TotalAmount = appointment.TotalAmount,
                    Status = appointment.Status,
                    StaffName = appointment.Staff?.FullName,
                    ServiceNames = appointment.AppointmentServices?.Select(s => s.Service!.Name).ToList() ?? new List<string>(),
                    CreatedAt = appointment.CreatedAt,
                    UserId = appointment.UserId,
                    CustomerEmail = appointment.UserId != null && appointment.User != null 
                        ? appointment.User.Email ?? appointment.GuestEmail 
                        : appointment.GuestEmail,
                    StaffId = appointment.StaffId,
                    Notes = appointment.CustomerNotes,
                    InternalNotes = appointment.InternalNotes,
                    ConfirmedAt = appointment.ConfirmedAt,
                    CancelledAt = appointment.CancelledAt,
                    CancelReason = appointment.CancellationReason,
                    Services = appointment.AppointmentServices?.Select(asp => new AppointmentServiceDetailDto
                    {
                        ServiceId = asp.ServiceId,
                        ServiceName = asp.Service?.Name ?? "",
                        Price = asp.Price,
                        DurationMinutes = asp.Service?.DurationMinutes ?? 0
                    }).ToList() ?? new List<AppointmentServiceDetailDto>()
                };

                return Ok(AdminApiResponse<AppointmentDetailDto>.Ok(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointment detail for {AppointmentId}", id);
                return StatusCode(500, AdminApiResponse<AppointmentDetailDto>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Cập nhật trạng thái lịch hẹn
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<ActionResult<AdminApiResponse<bool>>> UpdateAppointmentStatus(
            int id,
            [FromBody] UpdateAppointmentStatusRequest request)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Lịch hẹn không tồn tại"));

                var validStatuses = new[] { "Pending", "Confirmed", "InProgress", "Completed", "Cancelled", "NoShow" };
                if (!validStatuses.Contains(request.Status))
                    return BadRequest(AdminApiResponse<bool>.Fail($"Trạng thái không hợp lệ. Các trạng thái: {string.Join(", ", validStatuses)}"));

                var oldStatus = appointment.Status;
                appointment.Status = request.Status;
                appointment.UpdatedAt = DateTime.UtcNow;

                // Update timestamps based on status
                switch (request.Status)
                {
                    case "Confirmed":
                        appointment.ConfirmedAt = DateTime.UtcNow;
                        break;
                    case "Completed":
                        // Update service booking count
                        var services = await _context.AppointmentServices
                            .Where(asp => asp.AppointmentId == id)
                            .Select(asp => asp.ServiceId)
                            .ToListAsync();
                        foreach (var serviceId in services)
                        {
                            var service = await _context.Services.FindAsync(serviceId);
                            if (service != null)
                                service.TotalBookings++;
                        }
                        break;
                    case "Cancelled":
                        appointment.CancelledAt = DateTime.UtcNow;
                        appointment.CancellationReason = request.Notes;
                        break;
                }

                if (!string.IsNullOrEmpty(request.Notes))
                    appointment.InternalNotes = request.Notes;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Appointment {AppointmentId} status updated from {OldStatus} to {NewStatus}", id, oldStatus, request.Status);

                return Ok(AdminApiResponse<bool>.Ok(true, $"Đã cập nhật trạng thái thành {request.Status}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment status for {AppointmentId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server khi cập nhật trạng thái"));
            }
        }

        /// <summary>
        /// Gán nhân viên cho lịch hẹn
        /// </summary>
        [HttpPut("{id}/staff")]
        public async Task<ActionResult<AdminApiResponse<bool>>> AssignStaff(
            int id,
            [FromBody] AssignStaffToAppointmentRequest request)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Lịch hẹn không tồn tại"));

                // Validate staff exists
                var staff = await _context.Staff.FindAsync(request.StaffId);
                if (staff == null)
                    return BadRequest(AdminApiResponse<bool>.Fail("Nhân viên không tồn tại"));

                // Check staff availability
                var conflictingAppointment = await _context.Appointments
                    .Where(a => a.StaffId == request.StaffId)
                    .Where(a => a.Id != id)
                    .Where(a => a.AppointmentDate.Date == appointment.AppointmentDate.Date)
                    .Where(a => a.Status != "Cancelled" && a.Status != "Completed")
                    .Where(a => 
                        (a.StartTime < appointment.EndTime && a.EndTime > appointment.StartTime))
                    .FirstOrDefaultAsync();

                if (conflictingAppointment != null)
                    return BadRequest(AdminApiResponse<bool>.Fail($"Nhân viên đã có lịch hẹn khác trong khoảng thời gian này"));

                appointment.StaffId = request.StaffId;
                appointment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(AdminApiResponse<bool>.Ok(true, "Đã gán nhân viên cho lịch hẹn"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning staff to appointment {AppointmentId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Tạo lịch hẹn mới (từ admin hoặc app)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Staff,Customer")] // Cho phép Customer tạo appointment
        public async Task<ActionResult<AdminApiResponse<int>>> CreateAppointment([FromBody] CreateAppointmentRequest request)
        {
            try
            {
                // Lấy user ID từ token nếu là Customer
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (User.IsInRole("Customer") && !string.IsNullOrEmpty(userId))
                {
                    request.UserId = userId;
                }

                // Kiểm tra staff conflict nếu có chọn staff
                if (request.StaffId.HasValue)
                {
                    var totalDurationCheck = await _context.Services
                        .Where(s => request.ServiceIds.Contains(s.Id))
                        .SumAsync(s => s.DurationMinutes);
                    
                    var endTimeCheck = request.StartTime.Add(TimeSpan.FromMinutes(totalDurationCheck));
                    
                    var hasConflict = await _context.Appointments
                        .Where(a => a.StaffId == request.StaffId)
                        .Where(a => a.AppointmentDate.Date == request.AppointmentDate.Date)
                        .Where(a => a.Status != "Cancelled" && a.Status != "Completed")
                        .Where(a => a.StartTime < endTimeCheck && a.EndTime > request.StartTime)
                        .AnyAsync();

                    if (hasConflict)
                    {
                        return StatusCode(409, AdminApiResponse<int>.Fail(
                            "Nhân viên đang bận vào thời gian này, vui lòng chọn nhân viên khác hoặc thời gian khác"));
                    }
                }

                // Generate appointment code
                var appointmentCode = $"APT{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(100, 999)}";

                // Calculate total amount
                decimal totalAmount = 0;
                var services = new List<AppointmentService>();
                
                foreach (var serviceId in request.ServiceIds)
                {
                    var service = await _context.Services.FindAsync(serviceId);
                    if (service != null)
                    {
                        totalAmount += service.Price;
                        services.Add(new AppointmentService
                        {
                            ServiceId = serviceId,
                            Price = service.Price
                        });
                    }
                }

                // Calculate end time based on total duration
                var totalDuration = await _context.Services
                    .Where(s => request.ServiceIds.Contains(s.Id))
                    .SumAsync(s => s.DurationMinutes);
                
                var endTime = request.StartTime.Add(TimeSpan.FromMinutes(totalDuration));

                var appointment = new Appointment
                {
                    AppointmentCode = appointmentCode,
                    UserId = request.UserId,
                    StaffId = request.StaffId,
                    AppointmentDate = request.AppointmentDate,
                    StartTime = request.StartTime,
                    EndTime = endTime,
                    TotalAmount = totalAmount,
                    Status = "Pending",
                    BookingSource = request.BookingSource ?? "App",
                    GuestName = request.GuestName,
                    GuestPhone = request.GuestPhone,
                    GuestEmail = request.GuestEmail,
                    CustomerNotes = request.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                // Add appointment services
                foreach (var service in services)
                {
                    service.AppointmentId = appointment.Id;
                    _context.AppointmentServices.Add(service);
                }
                await _context.SaveChangesAsync();

                _logger.LogInformation("Appointment created: {AppointmentId} - {AppointmentCode}", appointment.Id, appointmentCode);

                return Ok(AdminApiResponse<int>.Ok(appointment.Id, "Tạo lịch hẹn thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment");
                return StatusCode(500, AdminApiResponse<int>.Fail("Lỗi server khi tạo lịch hẹn"));
            }
        }

        /// <summary>
        /// Lấy lịch hẹn theo ngày (cho calendar view)
        /// </summary>
        [HttpGet("calendar")]
        public async Task<ActionResult<AdminApiResponse<List<CalendarAppointmentDto>>>> GetCalendarAppointments(
            [FromQuery] DateTime date,
            [FromQuery] int? staffId)
        {
            try
            {
                var query = _context.Appointments
                    .Include(a => a.User)
                    .Include(a => a.Staff)
                    .Include(a => a.AppointmentServices!)
                        .ThenInclude(asp => asp.Service)
                    .Where(a => a.AppointmentDate.Date == date.Date)
                    .Where(a => a.Status != "Cancelled");

                if (staffId.HasValue)
                    query = query.Where(a => a.StaffId == staffId);

                var appointments = await query
                    .OrderBy(a => a.StartTime)
                    .Select(a => new CalendarAppointmentDto
                    {
                        Id = a.Id,
                        AppointmentCode = a.AppointmentCode ?? "",
                        CustomerName = a.UserId != null && a.User != null 
                            ? a.User.FullName ?? a.GuestName ?? "N/A" 
                            : a.GuestName ?? "N/A",
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        Status = a.Status,
                        StaffId = a.StaffId,
                        StaffName = a.Staff != null ? a.Staff.FullName : null,
                        Services = a.AppointmentServices != null 
                            ? a.AppointmentServices.Select(s => s.Service!.Name).ToList() 
                            : new List<string>()
                    })
                    .ToListAsync();

                return Ok(AdminApiResponse<List<CalendarAppointmentDto>>.Ok(appointments));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting calendar appointments");
                return StatusCode(500, AdminApiResponse<List<CalendarAppointmentDto>>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Lấy thống kê lịch hẹn
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<AdminApiResponse<object>>> GetAppointmentStats()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                var stats = new
                {
                    TotalAppointments = await _context.Appointments.CountAsync(),
                    PendingAppointments = await _context.Appointments.CountAsync(a => a.Status == "Pending"),
                    ConfirmedAppointments = await _context.Appointments.CountAsync(a => a.Status == "Confirmed"),
                    InProgressAppointments = await _context.Appointments.CountAsync(a => a.Status == "InProgress"),
                    CompletedAppointments = await _context.Appointments.CountAsync(a => a.Status == "Completed"),
                    CancelledAppointments = await _context.Appointments.CountAsync(a => a.Status == "Cancelled"),
                    TodayAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate.Date == today),
                    TodayRevenue = await _context.Appointments
                        .Where(a => a.AppointmentDate.Date == today && a.Status == "Completed")
                        .SumAsync(a => a.TotalAmount),
                    MonthAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate >= thisMonth),
                    MonthRevenue = await _context.Appointments
                        .Where(a => a.AppointmentDate >= thisMonth && a.Status == "Completed")
                        .SumAsync(a => a.TotalAmount)
                };

                return Ok(AdminApiResponse<object>.Ok(stats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointment stats");
                return StatusCode(500, AdminApiResponse<object>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Lấy danh sách nhân viên available cho booking
        /// </summary>
        [HttpGet("available-staff")]
        [AllowAnonymous] // Cho phép khách xem nhân viên rảnh
        public async Task<ActionResult<AdminApiResponse<List<object>>>> GetAvailableStaff(
            [FromQuery] DateTime date,
            [FromQuery] string startTime,
            [FromQuery] int durationMinutes)
        {
            try
            {
                // Parse startTime từ string "HH:mm" sang TimeSpan
                if (!TimeSpan.TryParse(startTime, out var parsedStartTime))
                {
                    return BadRequest(AdminApiResponse<List<object>>.Fail("Giờ không hợp lệ"));
                }

                var endTime = parsedStartTime.Add(TimeSpan.FromMinutes(durationMinutes));

                // Get all active staff
                var allStaff = await _context.Staff
                    .Where(s => s.Status == "Active")
                    .ToListAsync();

                // Get busy staff at that time
                var busyStaffIds = await _context.Appointments
                    .Where(a => a.AppointmentDate.Date == date.Date)
                    .Where(a => a.Status != "Cancelled" && a.Status != "Completed")
                    .Where(a => a.StartTime < endTime && a.EndTime > parsedStartTime)
                    .Select(a => a.StaffId)
                    .ToListAsync();

                var availableStaff = allStaff
                    .Where(s => !busyStaffIds.Contains(s.Id))
                    .Select(s => new { 
                        s.Id, 
                        s.FullName, 
                        Name = s.FullName,
                        s.Position,
                        s.Level,
                        s.AvatarUrl
                    })
                    .ToList();

                return Ok(AdminApiResponse<List<object>>.Ok(availableStaff.Cast<object>().ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available staff");
                return StatusCode(500, AdminApiResponse<List<object>>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Kiểm tra nhân viên có rảnh không
        /// </summary>
        [HttpGet("check-staff-availability")]
        [AllowAnonymous]
        public async Task<ActionResult> CheckStaffAvailability(
            [FromQuery] int staffId,
            [FromQuery] DateTime date,
            [FromQuery] string startTime,
            [FromQuery] int durationMinutes)
        {
            try
            {
                if (!TimeSpan.TryParse(startTime, out var parsedStartTime))
                {
                    return BadRequest(new { available = false, message = "Giờ không hợp lệ" });
                }

                var endTime = parsedStartTime.Add(TimeSpan.FromMinutes(durationMinutes));

                // Kiểm tra nhân viên tồn tại
                var staff = await _context.Staff.FindAsync(staffId);
                if (staff == null || staff.Status != "Active")
                {
                    return Ok(new { available = false, message = "Nhân viên không tồn tại hoặc không hoạt động" });
                }

                // Kiểm tra lịch hẹn conflict
                var hasConflict = await _context.Appointments
                    .Where(a => a.StaffId == staffId)
                    .Where(a => a.AppointmentDate.Date == date.Date)
                    .Where(a => a.Status != "Cancelled" && a.Status != "Completed")
                    .Where(a => a.StartTime < endTime && a.EndTime > parsedStartTime)
                    .AnyAsync();

                if (hasConflict)
                {
                    return Ok(new { 
                        available = false, 
                        message = $"Nhân viên {staff.FullName} đang bận vào thời gian này, vui lòng chọn nhân viên khác hoặc thời gian khác" 
                    });
                }

                return Ok(new { available = true, message = "Nhân viên rảnh" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking staff availability");
                return StatusCode(500, new { available = false, message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Lấy danh sách trạng thái hợp lệ
        /// </summary>
        [HttpGet("statuses")]
        public ActionResult<AdminApiResponse<List<string>>> GetAppointmentStatuses()
        {
            var statuses = new List<string>
            {
                "Pending",
                "Confirmed",
                "InProgress",
                "Completed",
                "Cancelled",
                "NoShow"
            };

            return Ok(AdminApiResponse<List<string>>.Ok(statuses));
        }

        /// <summary>
        /// Export danh sách lịch hẹn
        /// </summary>
        [HttpGet("export")]
        public async Task<IActionResult> ExportAppointments(
            [FromQuery] string? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var query = _context.Appointments
                    .Include(a => a.User)
                    .Include(a => a.Staff)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(a => a.Status == status);

                if (fromDate.HasValue)
                    query = query.Where(a => a.AppointmentDate >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(a => a.AppointmentDate <= toDate.Value);

                var appointments = await query
                    .OrderByDescending(a => a.AppointmentDate)
                    .Take(10000)
                    .ToListAsync();

                var csv = new StringBuilder();
                csv.AppendLine("Mã,Khách hàng,SĐT,Ngày hẹn,Giờ bắt đầu,Giờ kết thúc,Nhân viên,Tổng tiền,Trạng thái,Ngày tạo");

                foreach (var a in appointments)
                {
                    var customerName = a.UserId != null && a.User != null 
                        ? a.User.FullName ?? a.GuestName 
                        : a.GuestName;
                    var customerPhone = a.UserId != null && a.User != null 
                        ? a.User.PhoneNumber ?? a.GuestPhone 
                        : a.GuestPhone;
                        
                    csv.AppendLine($"\"{a.AppointmentCode}\"," +
                        $"\"{customerName}\"," +
                        $"\"{customerPhone}\"," +
                        $"\"{a.AppointmentDate:yyyy-MM-dd}\"," +
                        $"\"{a.StartTime}\"," +
                        $"\"{a.EndTime}\"," +
                        $"\"{a.Staff?.FullName}\"," +
                        $"{a.TotalAmount}," +
                        $"\"{a.Status}\"," +
                        $"\"{a.CreatedAt:yyyy-MM-dd HH:mm}\"");
                }

                var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
                return File(bytes, "text/csv; charset=utf-8", $"appointments_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting appointments");
                return StatusCode(500, "Lỗi server khi export lịch hẹn");
            }
        }
    }

    #region Appointment DTOs

    public class AppointmentDetailDto : AppointmentListDto
    {
        public string? UserId { get; set; }
        public string? CustomerEmail { get; set; }
        public int? StaffId { get; set; }
        public string? Notes { get; set; }
        public string? InternalNotes { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }
        public List<AppointmentServiceDetailDto> Services { get; set; } = new();
    }

    public class AppointmentServiceDetailDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = "";
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
    }

    public class UpdateAppointmentStatusRequest
    {
        public string Status { get; set; } = "";
        public string? Notes { get; set; }
    }

    public class AssignStaffToAppointmentRequest
    {
        public int StaffId { get; set; }
    }

    public class CreateAppointmentRequest
    {
        public string? UserId { get; set; }
        public int? StaffId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public List<int> ServiceIds { get; set; } = new();
        public string? GuestName { get; set; }
        public string? GuestPhone { get; set; }
        public string? GuestEmail { get; set; }
        public string? Notes { get; set; }
        public string? BookingSource { get; set; } // App, Website, Phone, WalkIn
    }

    public class CalendarAppointmentDto
    {
        public int Id { get; set; }
        public string AppointmentCode { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } = "";
        public int? StaffId { get; set; }
        public string? StaffName { get; set; }
        public List<string> Services { get; set; } = new();
    }

    #endregion
}
