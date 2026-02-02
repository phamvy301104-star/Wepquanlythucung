using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_backend.Models;
using nhom6_backend.Models.Entities;
using nhom6_backend.Services;
using nhom6_backend.Hubs;
using System.Security.Claims;

namespace nhom6_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<AppointmentApiController> _logger;

        public AppointmentApiController(
            ApplicationDbContext context,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<AppointmentApiController> logger)
        {
            _context = context;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// Tạo lịch hẹn mới (User có thể chưa đăng nhập - Guest booking)
        /// POST /api/AppointmentApi/create
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateAppointment([FromBody] GuestCreateAppointmentRequest request)
        {
            try
            {
                _logger.LogInformation("CreateAppointment called with: GuestName={GuestName}, AppointmentDate={Date}, StartTime={Time}, ServiceIds={Services}",
                    request.GuestName, request.AppointmentDate, request.StartTime, string.Join(",", request.ServiceIds ?? new List<int>()));

                // Validate request
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });

                // Validate services
                if (request.ServiceIds == null || !request.ServiceIds.Any())
                    return BadRequest(new { success = false, message = "Vui lòng chọn ít nhất một dịch vụ" });

                // Get user ID if authenticated
                var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("UserId from token: {UserId}", userId ?? "null");

                // Validate date/time - use local time for Vietnam timezone
                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
                var appointmentDateTime = request.AppointmentDate.Date.Add(request.StartTime);
                
                _logger.LogInformation("Appointment DateTime: {AppointmentDateTime}, Vietnam Now: {VietnamNow}", appointmentDateTime, vietnamNow);
                
                if (appointmentDateTime <= vietnamNow)
                    return BadRequest(new { success = false, message = "Ngày giờ hẹn phải sau thời điểm hiện tại" });

                // Load services
                _logger.LogInformation("Loading services for IDs: {ServiceIds}", string.Join(",", request.ServiceIds));
                var services = await _context.Services
                    .Where(s => request.ServiceIds.Contains(s.Id) && !s.IsDeleted && s.IsActive)
                    .ToListAsync();
                _logger.LogInformation("Found {Count} services: {ServiceNames}", services.Count, string.Join(",", services.Select(s => s.Name)));

                if (services.Count != request.ServiceIds.Count)
                {
                    _logger.LogWarning("Service count mismatch. Requested: {Requested}, Found: {Found}", request.ServiceIds.Count, services.Count);
                    return BadRequest(new { success = false, message = "Một số dịch vụ không tồn tại hoặc không còn hoạt động" });
                }

                // Calculate total
                var totalAmount = services.Sum(s => s.Price);
                var totalDuration = services.Sum(s => s.DurationMinutes);
                var endTime = request.StartTime.Add(TimeSpan.FromMinutes(totalDuration));

                // Check time slot availability (optional - có thể bỏ qua nếu shop cho phép trùng)
                var isTimeSlotTaken = await _context.Appointments
                    .Where(a => !a.IsDeleted 
                        && a.AppointmentDate.Date == request.AppointmentDate.Date
                        && a.Status != "Cancelled"
                        && a.Status != "NoShow"
                        && ((a.StartTime < endTime && a.EndTime > request.StartTime)))
                    .AnyAsync();

                // if (isTimeSlotTaken)
                //     return BadRequest(new { success = false, message = "Khung giờ này đã có lịch hẹn khác" });

                // Generate appointment code
                var appointmentCode = $"APT{DateTime.Now:yyMMddHHmmss}";

                // Create appointment
                var appointment = new Appointment
                {
                    AppointmentCode = appointmentCode,
                    UserId = userId,
                    GuestName = request.GuestName,
                    GuestPhone = request.GuestPhone,
                    GuestEmail = request.GuestEmail,
                    AppointmentDate = request.AppointmentDate.Date,
                    StartTime = request.StartTime,
                    EndTime = endTime,
                    TotalDurationMinutes = totalDuration,
                    TotalAmount = totalAmount,
                    Status = "Pending",
                    CustomerNotes = request.CustomerNotes,
                    BookingSource = "App",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                // Add appointment services
                var appointmentServices = services.Select((service, index) => new AppointmentService
                {
                    AppointmentId = appointment.Id,
                    ServiceId = service.Id,
                    ServiceName = service.Name,
                    Price = service.Price,
                    Quantity = 1,
                    DurationMinutes = service.DurationMinutes,
                    ServiceOrder = index + 1,
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }).ToList();

                _context.AppointmentServices.AddRange(appointmentServices);
                await _context.SaveChangesAsync();

                // Load relationships for response
                await _context.Entry(appointment)
                    .Collection(a => a.AppointmentServices!)
                    .Query()
                    .Include(s => s.Service)
                    .LoadAsync();

                // Get service names for notification
                var serviceNames = appointment.AppointmentServices?
                    .Select(s => s.Service?.Name ?? s.ServiceName ?? "Dịch vụ")
                    .ToList() ?? new List<string>();

                // Send notifications (fire and forget) - Use IServiceScopeFactory for background task
                var serviceScopeFactory = _serviceScopeFactory;
                var appointmentData = new
                {
                    Id = appointment.Id,
                    AppointmentCode = appointment.AppointmentCode,
                    CustomerName = appointment.GuestName ?? "Khách hàng",
                    CustomerPhone = appointment.GuestPhone ?? "",
                    CustomerEmail = appointment.GuestEmail ?? "",
                    AppointmentDate = appointment.AppointmentDate.ToString("dd/MM/yyyy"),
                    StartTime = appointment.StartTime.ToString(@"hh\:mm"),
                    EndTime = appointment.EndTime.ToString(@"hh\:mm"),
                    TotalAmount = appointment.TotalAmount,
                    Status = appointment.Status,
                    Services = serviceNames,
                    CreatedAt = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy")
                };

                // 1. Lưu AdminNotification vào database (persistent)
                var adminNotification = new AdminNotification
                {
                    Type = "NewAppointment",
                    Title = $"Lịch hẹn mới #{appointment.AppointmentCode}",
                    Content = $"{appointment.GuestName ?? "Khách hàng"} đặt lịch lúc {appointment.StartTime:hh\\:mm} ngày {appointment.AppointmentDate:dd/MM/yyyy}. Dịch vụ: {string.Join(", ", serviceNames)}",
                    Data = System.Text.Json.JsonSerializer.Serialize(appointmentData),
                    ActionUrl = $"/Admin/Appointments/Details/{appointment.Id}",
                    RelatedEntityId = appointment.Id,
                    RelatedEntityType = "Appointment",
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.AdminNotifications.Add(adminNotification);

                // 2. Tạo Notification cho User để track trạng thái
                if (!string.IsNullOrEmpty(userId))
                {
                    var userNotification = new Notification
                    {
                        UserId = userId,
                        Type = "Appointment",
                        Title = "Đặt lịch thành công",
                        Content = $"Lịch hẹn #{appointment.AppointmentCode} đang chờ xác nhận từ shop. Ngày: {appointment.AppointmentDate:dd/MM/yyyy}, Giờ: {appointment.StartTime:hh\\:mm}",
                        ActionUrl = $"/appointments/{appointment.Id}",
                        ReferenceType = "Appointment",
                        ReferenceId = appointment.Id.ToString(),
                        Priority = "Normal",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.Notifications.Add(userNotification);
                }
                await _context.SaveChangesAsync();

                // Lưu thông tin cần thiết cho email/notification task
                var emailAppointmentId = appointment.Id;
                var emailAppointmentCode = appointment.AppointmentCode;
                var emailCustomerName = appointment.GuestName ?? "Khách hàng";
                var emailCustomerPhone = appointment.GuestPhone ?? "";
                var emailCustomerEmail = appointment.GuestEmail ?? "";
                var emailAppointmentDate = appointment.AppointmentDate;
                var emailStartTime = appointment.StartTime;
                var emailTotalAmount = appointment.TotalAmount;
                var emailServiceNames = serviceNames.ToList();

                // Send email and SignalR notification (synchronous but non-blocking response)
                // Use try-catch to ensure response is sent even if email fails
                try
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    
                    _logger.LogInformation("📧 Sending email notification for appointment {AppointmentId}", emailAppointmentId);
                    
                    // Send email to admin (this may take a few seconds)
                    var emailResult = await emailService.SendAppointmentNotificationToAdminSimple(
                        emailAppointmentId,
                        emailAppointmentCode,
                        emailCustomerName,
                        emailCustomerPhone,
                        emailCustomerEmail,
                        emailAppointmentDate,
                        emailStartTime,
                        emailTotalAmount,
                        emailServiceNames);
                    
                    if (emailResult)
                    {
                        _logger.LogInformation("✅ Email sent successfully for appointment {AppointmentId}", emailAppointmentId);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Email sending returned false for appointment {AppointmentId}", emailAppointmentId);
                    }

                    // Send SignalR real-time notification to Admin
                    _logger.LogInformation("🔔 Sending SignalR notification for appointment {AppointmentId}", emailAppointmentId);
                    await notificationService.NotifyNewAppointment(appointmentData);
                    _logger.LogInformation("✅ SignalR notification sent successfully");
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the response - user's booking was successful
                    _logger.LogError(ex, "❌ Error sending notifications for appointment {AppointmentId}. Error: {Message}", 
                        emailAppointmentId, ex.Message);
                }

                return Ok(new
                {
                    success = true,
                    message = "Đặt lịch thành công! Shop sẽ xác nhận lịch của bạn sớm.",
                    data = new
                    {
                        appointment.Id,
                        appointment.AppointmentCode,
                        appointment.AppointmentDate,
                        appointment.StartTime,
                        appointment.EndTime,
                        appointment.TotalDurationMinutes,
                        appointment.TotalAmount,
                        appointment.Status,
                        Services = appointment.AppointmentServices?.Select(s => new
                        {
                            s.ServiceId,
                            s.ServiceName,
                            s.Price,
                            s.DurationMinutes
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment. Request: {@Request}", request);
                // Return detailed error in development
                var errorMessage = "Lỗi hệ thống, vui lòng thử lại sau";
                #if DEBUG
                errorMessage = $"Lỗi: {ex.Message}. Inner: {ex.InnerException?.Message}";
                #endif
                return StatusCode(500, new { success = false, message = errorMessage, error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách lịch hẹn của user đang đăng nhập
        /// GET /api/AppointmentApi/my-appointments
        /// </summary>
        [HttpGet("my-appointments")]
        [Authorize]
        public async Task<IActionResult> GetMyAppointments(
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });

                var query = _context.Appointments
                    .Include(a => a.AppointmentServices!)
                        .ThenInclude(s => s.Service)
                    .Include(a => a.Staff)
                    .Where(a => a.UserId == userId && !a.IsDeleted)
                    .AsQueryable();

                // Filter by status
                if (!string.IsNullOrEmpty(status) && status != "All")
                {
                    query = query.Where(a => a.Status == status);
                }

                // Order by date desc
                query = query.OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.StartTime);

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var appointments = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new
                    {
                        a.Id,
                        a.AppointmentCode,
                        a.AppointmentDate,
                        a.StartTime,
                        a.EndTime,
                        a.TotalDurationMinutes,
                        a.TotalAmount,
                        a.Status,
                        a.CustomerNotes,
                        a.InternalNotes,
                        StaffName = a.Staff != null ? a.Staff.FullName : null,
                        Services = a.AppointmentServices!.Select(s => new
                        {
                            s.ServiceId,
                            s.ServiceName,
                            s.Price,
                            s.DurationMinutes
                        }),
                        a.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        items = appointments,
                        page,
                        pageSize,
                        totalItems,
                        totalPages
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my appointments");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }

        /// <summary>
        /// Lấy chi tiết lịch hẹn
        /// GET /api/AppointmentApi/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentById(int id)
        {
            try
            {
                var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var appointment = await _context.Appointments
                    .Include(a => a.AppointmentServices!)
                        .ThenInclude(s => s.Service)
                    .Include(a => a.Staff)
                    .Include(a => a.User)
                    .Where(a => a.Id == id && !a.IsDeleted)
                    .FirstOrDefaultAsync();

                if (appointment == null)
                    return NotFound(new { success = false, message = "Không tìm thấy lịch hẹn" });

                // Check permission (user chỉ xem được lịch của mình, trừ khi là admin/staff)
                var userRole = User?.FindFirst(ClaimTypes.Role)?.Value;
                if (appointment.UserId != userId 
                    && userRole != "Admin" 
                    && userRole != "Staff")
                {
                    return Forbid();
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        appointment.Id,
                        appointment.AppointmentCode,
                        appointment.UserId,
                        CustomerName = appointment.GuestName ?? appointment.User?.FullName,
                        appointment.GuestPhone,
                        appointment.GuestEmail,
                        appointment.AppointmentDate,
                        appointment.StartTime,
                        appointment.EndTime,
                        appointment.TotalDurationMinutes,
                        appointment.TotalAmount,
                        appointment.DiscountAmount,
                        appointment.PaidAmount,
                        appointment.Status,
                        appointment.CustomerNotes,
                        appointment.InternalNotes,
                        appointment.CancellationReason,
                        StaffName = appointment.Staff?.FullName,
                        Services = appointment.AppointmentServices?.Select(s => new
                        {
                            s.ServiceId,
                            ServiceName = s.Service?.Name ?? s.ServiceName,
                            s.Price,
                            s.DurationMinutes,
                            s.ServiceOrder
                        }),
                        appointment.CreatedAt,
                        appointment.ConfirmedAt,
                        appointment.CheckInTime,
                        appointment.CheckOutTime
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointment {AppointmentId}", id);
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }

        /// <summary>
        /// Hủy lịch hẹn (User)
        /// PATCH /api/AppointmentApi/{id}/cancel
        /// </summary>
        [HttpPatch("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelAppointment(int id, [FromBody] CancelAppointmentRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var appointment = await _context.Appointments
                    .Where(a => a.Id == id && a.UserId == userId && !a.IsDeleted)
                    .FirstOrDefaultAsync();

                if (appointment == null)
                    return NotFound(new { success = false, message = "Không tìm thấy lịch hẹn" });

                if (appointment.Status == "Cancelled")
                    return BadRequest(new { success = false, message = "Lịch hẹn đã bị hủy trước đó" });

                if (appointment.Status == "Completed")
                    return BadRequest(new { success = false, message = "Không thể hủy lịch hẹn đã hoàn thành" });

                // Update status
                appointment.Status = "Cancelled";
                appointment.CancellationReason = request.Reason;
                appointment.CancelledBy = "Customer";
                appointment.CancelledAt = DateTime.Now;
                appointment.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                // Lưu thông tin cần thiết cho background task
                var serviceScopeFactory = _serviceScopeFactory;
                var cancellationAppointmentCode = appointment.AppointmentCode;
                var cancellationCustomerName = appointment.GuestName ?? "Khách hàng";
                var cancellationCustomerPhone = appointment.GuestPhone ?? "";
                var cancellationDate = appointment.AppointmentDate;
                var cancellationTime = appointment.StartTime;
                var cancellationTotalAmount = appointment.TotalAmount;
                var cancellationReason = request.Reason;

                // Send notifications - Sử dụng IServiceScopeFactory để tạo scope mới
                _ = Task.Run(async () =>
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    
                    try
                    {
                        _logger.LogInformation("📧 Sending cancellation email for appointment {AppointmentCode}", cancellationAppointmentCode);
                        
                        // Notify admin - sử dụng method Simple không cần DbContext
                        await emailService.SendAppointmentCancellationEmailSimple(
                            cancellationAppointmentCode,
                            cancellationCustomerName,
                            cancellationCustomerPhone,
                            cancellationDate,
                            cancellationTime,
                            cancellationTotalAmount,
                            cancellationReason,
                            "bbaohan2212@gmail.com",
                            false
                        );
                        
                        _logger.LogInformation("📧 Cancellation email sent successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error sending cancellation notifications");
                    }
                });

                return Ok(new { success = true, message = "Đã hủy lịch hẹn thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment {AppointmentId}", id);
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }
    }

    #region DTOs

    public class GuestCreateAppointmentRequest
    {
        public string GuestName { get; set; } = string.Empty;
        public string GuestPhone { get; set; } = string.Empty;
        public string GuestEmail { get; set; } = string.Empty;
        public List<int> ServiceIds { get; set; } = new();
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public string? CustomerNotes { get; set; }
    }

    public class CancelAppointmentRequest
    {
        public string Reason { get; set; } = "Khách hàng hủy";
    }

    #endregion
}
