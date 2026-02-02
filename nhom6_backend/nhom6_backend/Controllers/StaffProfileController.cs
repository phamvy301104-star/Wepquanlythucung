using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_backend.Models;
using nhom6_backend.Models.DTOs.Staff;
using nhom6_backend.Models.Entities;
using System.Security.Claims;

namespace nhom6_backend.Controllers
{
    /// <summary>
    /// API cho Staff Profile
    /// </summary>
    [Route("api/staff")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    public class StaffProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<StaffProfileController> _logger;
        private readonly UserManager<User> _userManager;

        public StaffProfileController(
            ApplicationDbContext db, 
            ILogger<StaffProfileController> logger,
            UserManager<User> userManager)
        {
            _db = db;
            _logger = logger;
            _userManager = userManager;
        }

        /// <summary>
        /// Helper method: Lấy hoặc tự động tạo Staff record cho user hiện tại
        /// </summary>
        private async Task<Staff?> GetOrCreateStaffAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return null;

            // Tìm Staff record hiện có
            var staff = await _db.Staff
                .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);

            if (staff != null) return staff;

            // Kiểm tra user có role Staff không
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Staff") && !roles.Contains("Admin")) return null;

            // Auto-create Staff record
            _logger.LogInformation("🔄 Auto-creating Staff record for user {UserId} ({UserName})", userId, user.UserName);

            staff = new Staff
            {
                UserId = userId,
                StaffCode = $"STF{DateTime.UtcNow:yyyyMMddHHmmss}",
                FullName = user.FullName ?? user.UserName ?? "Staff",
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Position = "Barber",
                Level = "Junior",
                Status = "Active",
                IsAvailable = true,
                AcceptOnlineBooking = true,
                HireDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _db.Staff.Add(staff);
            await _db.SaveChangesAsync();

            _logger.LogInformation("✅ Created Staff record Id={StaffId} for user {UserId}", staff.Id, userId);

            return staff;
        }

        /// <summary>
        /// Lấy thông tin profile của staff hiện tại
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin user" });
                }

                // Sử dụng helper method để lấy hoặc tạo Staff
                var staff = await GetOrCreateStaffAsync();

                if (staff == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên. Vui lòng liên hệ Admin để được cấp quyền Staff." });
                }

                var profileDto = new StaffProfileDto
                {
                    Id = staff.Id,
                    StaffCode = staff.StaffCode,
                    FullName = staff.FullName,
                    NickName = staff.NickName,
                    Email = staff.Email,
                    PhoneNumber = staff.PhoneNumber,
                    AvatarUrl = staff.AvatarUrl,
                    CoverImageUrl = staff.CoverImageUrl,
                    Bio = staff.Bio,
                    Position = staff.Position,
                    Level = staff.Level,
                    Specialties = staff.Specialties,
                    YearsOfExperience = staff.YearsOfExperience,
                    DateOfBirth = staff.DateOfBirth,
                    Gender = staff.Gender,
                    HireDate = staff.HireDate,
                    AverageRating = staff.AverageRating,
                    TotalReviews = staff.TotalReviews,
                    TotalCustomersServed = staff.TotalCustomersServed,
                    TotalRevenue = staff.TotalRevenue,
                    Status = staff.Status,
                    IsAvailable = staff.IsAvailable
                };

                return Ok(new { success = true, data = profileDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy profile staff");
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thống kê của staff
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var staff = await GetOrCreateStaffAsync();

                if (staff == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                // Thống kê tháng này
                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var appointmentsThisMonth = await _db.Appointments
                    .AsNoTracking()
                    .Where(a => a.StaffId == staff.Id 
                        && a.AppointmentDate >= startOfMonth 
                        && a.AppointmentDate <= endOfMonth
                        && a.Status == "Completed")
                    .ToListAsync();

                var revenueThisMonth = appointmentsThisMonth.Sum(a => a.TotalAmount);
                var customersThisMonth = appointmentsThisMonth.Select(a => a.UserId).Distinct().Count();

                var stats = new StaffStatsDto
                {
                    AverageRating = staff.AverageRating,
                    TotalReviews = staff.TotalReviews,
                    TotalCustomersServed = staff.TotalCustomersServed,
                    TotalRevenue = staff.TotalRevenue,
                    TotalAppointmentsThisMonth = appointmentsThisMonth.Count,
                    RevenueThisMonth = revenueThisMonth,
                    CustomersThisMonth = customersThisMonth
                };

                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy stats staff");
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy lịch làm việc của staff
        /// </summary>
        [HttpGet("schedule")]
        public async Task<IActionResult> GetSchedule([FromQuery] DateTime? date = null)
        {
            try
            {
                var staff = await GetOrCreateStaffAsync();

                if (staff == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                var schedules = await _db.StaffSchedules
                    .AsNoTracking()
                    .Where(ss => ss.StaffId == staff.Id && !ss.IsDeleted)
                    .OrderBy(ss => ss.DayOfWeek)
                    .ToListAsync();

                var dayNames = new[] { "Chủ nhật", "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7" };

                var scheduleDtos = schedules.Select(s => new StaffScheduleDto
                {
                    DayOfWeek = s.DayOfWeek,
                    DayName = dayNames[s.DayOfWeek],
                    SpecificDate = s.SpecificDate,
                    StartTime = s.StartTime.ToString(@"hh\:mm"),
                    EndTime = s.EndTime.ToString(@"hh\:mm"),
                    BreakStartTime = s.BreakStartTime?.ToString(@"hh\:mm"),
                    BreakEndTime = s.BreakEndTime?.ToString(@"hh\:mm"),
                    IsWorking = s.IsWorking,
                    IsLeave = s.IsLeave,
                    LeaveReason = s.LeaveReason
                }).ToList();

                return Ok(new { success = true, data = scheduleDtos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy schedule staff");
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách lịch hẹn được assign cho staff
        /// </summary>
        [HttpGet("appointments")]
        public async Task<IActionResult> GetAppointments([FromQuery] DateTime? date = null, [FromQuery] string? status = null)
        {
            try
            {
                var staff = await GetOrCreateStaffAsync();

                if (staff == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                var query = _db.Appointments
                    .AsNoTracking()
                    .Include(a => a.User)
                    .Include(a => a.AppointmentServices!)
                        .ThenInclude(aps => aps.Service)
                    .Where(a => a.StaffId == staff.Id && !a.IsDeleted);

                if (date.HasValue)
                {
                    var targetDate = date.Value.Date;
                    query = query.Where(a => a.AppointmentDate.Date == targetDate);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(a => a.Status == status);
                }

                var appointments = await query
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.StartTime)
                    .Take(50)
                    .ToListAsync();

                var appointmentDtos = appointments.Select(a => new StaffAppointmentDto
                {
                    Id = a.Id,
                    CustomerName = a.GuestName ?? a.User?.FullName ?? "Khách vãng lai",
                    CustomerPhone = a.GuestPhone ?? a.User?.PhoneNumber,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime.ToString(@"hh\:mm"),
                    EndTime = a.EndTime.ToString(@"hh\:mm"),
                    Services = a.AppointmentServices?.Select(aps => aps.Service?.Name ?? "").ToList() ?? new List<string>(),
                    TotalAmount = a.TotalAmount,
                    Status = a.Status,
                    Note = a.CustomerNotes
                }).ToList();

                return Ok(new { success = true, data = appointmentDtos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy appointments staff");
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy lịch hẹn theo ngày (cho calendar view)
        /// </summary>
        [HttpGet("appointments/calendar")]
        public async Task<IActionResult> GetAppointmentsCalendar([FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                var staff = await GetOrCreateStaffAsync();

                if (staff == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var appointments = await _db.Appointments
                    .AsNoTracking()
                    .Where(a => a.StaffId == staff.Id 
                        && a.AppointmentDate >= startDate 
                        && a.AppointmentDate <= endDate
                        && !a.IsDeleted)
                    .GroupBy(a => a.AppointmentDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Count = g.Count(),
                        HasCompleted = g.Any(a => a.Status == "Completed"),
                        HasPending = g.Any(a => a.Status == "Pending" || a.Status == "Confirmed")
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = appointments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy appointments calendar");
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }
    }
}
