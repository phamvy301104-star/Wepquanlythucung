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
    /// API Chấm công - 4 lần/ca với ảnh khuôn mặt
    /// </summary>
    [Route("api/attendance")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AttendanceController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<User> _userManager;

        public AttendanceController(
            ApplicationDbContext db, 
            ILogger<AttendanceController> logger,
            IWebHostEnvironment env,
            UserManager<User> userManager)
        {
            _db = db;
            _logger = logger;
            _env = env;
            _userManager = userManager;
        }

        /// <summary>
        /// Lấy trạng thái chấm công hôm nay
        /// </summary>
        [HttpGet("today")]
        public async Task<IActionResult> GetToday()
        {
            try
            {
                var staff = await GetCurrentStaff();
                if (staff == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                var today = DateTime.UtcNow.Date;
                var attendance = await _db.Attendances
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.StaffId == staff.Id && a.WorkDate.Date == today);

                // Lấy lịch làm việc hôm nay
                var dayOfWeek = (int)today.DayOfWeek;
                var schedule = await _db.StaffSchedules
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.StaffId == staff.Id && s.DayOfWeek == dayOfWeek && !s.IsDeleted);

                if (attendance == null)
                {
                    // Tạo record mới nếu chưa có
                    attendance = new Attendance
                    {
                        StaffId = staff.Id,
                        WorkDate = today,
                        ScheduledStart = schedule?.StartTime ?? new TimeSpan(8, 0, 0),
                        ScheduledBreakStart = schedule?.BreakStartTime,
                        ScheduledBreakEnd = schedule?.BreakEndTime,
                        ScheduledEnd = schedule?.EndTime ?? new TimeSpan(17, 0, 0),
                        Status = "Incomplete",
                        CheckCount = 0
                    };
                    _db.Attendances.Add(attendance);
                    await _db.SaveChangesAsync();
                }

                var result = MapToTodayDto(attendance);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy attendance today");
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        /// <summary>
        /// Chấm công với ảnh khuôn mặt
        /// </summary>
        [HttpPost("check")]
        public async Task<IActionResult> Check([FromBody] AttendanceCheckRequest request)
        {
            try
            {
                var staff = await GetCurrentStaff();
                if (staff == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                // Parse device time
                if (!DateTimeOffset.TryParse(request.DeviceTime, out var deviceTime))
                {
                    deviceTime = DateTimeOffset.UtcNow;
                }
                
                // Convert to Vietnam time (UTC+7)
                var vietnamTime = deviceTime.ToOffset(TimeSpan.FromHours(7));
                var today = vietnamTime.Date;
                var currentTime = vietnamTime.TimeOfDay;

                // Lấy hoặc tạo record attendance
                var attendance = await _db.Attendances
                    .FirstOrDefaultAsync(a => a.StaffId == staff.Id && a.WorkDate.Date == today);

                if (attendance == null)
                {
                    var dayOfWeek = (int)today.DayOfWeek;
                    var schedule = await _db.StaffSchedules
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.StaffId == staff.Id && s.DayOfWeek == dayOfWeek && !s.IsDeleted);

                    attendance = new Attendance
                    {
                        StaffId = staff.Id,
                        WorkDate = today,
                        ScheduledStart = schedule?.StartTime ?? new TimeSpan(8, 0, 0),
                        ScheduledBreakStart = schedule?.BreakStartTime,
                        ScheduledBreakEnd = schedule?.BreakEndTime,
                        ScheduledEnd = schedule?.EndTime ?? new TimeSpan(17, 0, 0),
                        Status = "Incomplete"
                    };
                    _db.Attendances.Add(attendance);
                }

                // Kiểm tra đã chấm đủ 4 lần chưa
                if (attendance.CheckCount >= 4)
                {
                    return BadRequest(new { message = "Đã chấm công đủ 4 lần trong ngày" });
                }

                // Validate check type
                var expectedCheckType = attendance.CheckCount + 1;
                if (request.CheckType != expectedCheckType)
                {
                    return BadRequest(new { 
                        message = $"Lần chấm công tiếp theo phải là lần {expectedCheckType}",
                        expectedCheckType = expectedCheckType,
                        currentCheckCount = attendance.CheckCount
                    });
                }

                // Lưu ảnh khuôn mặt
                string? photoUrl = null;
                if (!string.IsNullOrEmpty(request.PhotoBase64))
                {
                    photoUrl = await SaveFacePhoto(staff.Id, today, request.CheckType, request.PhotoBase64);
                }

                // Cập nhật theo loại check
                var checkTime = vietnamTime.DateTime;
                switch (request.CheckType)
                {
                    case 1: // Vào ca
                        attendance.CheckIn1_Time = checkTime;
                        attendance.CheckIn1_PhotoUrl = photoUrl;
                        
                        // Tính trễ
                        var scheduledStart = today.Add(attendance.ScheduledStart);
                        if (checkTime > scheduledStart)
                        {
                            attendance.LateMinutes = (int)(checkTime - scheduledStart).TotalMinutes;
                            attendance.LatePenalty = attendance.LateMinutes * Attendance.PENALTY_PER_MINUTE;
                        }
                        break;

                    case 2: // Vào nghỉ
                        attendance.CheckIn2_Time = checkTime;
                        attendance.CheckIn2_PhotoUrl = photoUrl;
                        break;

                    case 3: // Hết nghỉ
                        attendance.CheckIn3_Time = checkTime;
                        attendance.CheckIn3_PhotoUrl = photoUrl;
                        
                        // Tính nghỉ quá giờ
                        if (attendance.ScheduledBreakEnd.HasValue)
                        {
                            var scheduledBreakEnd = today.Add(attendance.ScheduledBreakEnd.Value);
                            if (checkTime > scheduledBreakEnd)
                            {
                                attendance.OverBreakMinutes = (int)(checkTime - scheduledBreakEnd).TotalMinutes;
                                attendance.OverBreakPenalty = attendance.OverBreakMinutes * Attendance.PENALTY_PER_MINUTE;
                            }
                        }
                        break;

                    case 4: // Về
                        attendance.CheckIn4_Time = checkTime;
                        attendance.CheckIn4_PhotoUrl = photoUrl;
                        
                        // Tính về sớm
                        var scheduledEnd = today.Add(attendance.ScheduledEnd);
                        if (checkTime < scheduledEnd)
                        {
                            attendance.EarlyLeaveMinutes = (int)(scheduledEnd - checkTime).TotalMinutes;
                            attendance.EarlyLeavePenalty = attendance.EarlyLeaveMinutes * Attendance.PENALTY_PER_MINUTE;
                        }
                        
                        // Tính làm thêm giờ
                        if (checkTime > scheduledEnd)
                        {
                            attendance.OvertimeMinutes = (int)(checkTime - scheduledEnd).TotalMinutes;
                        }
                        
                        // Tính tổng giờ làm
                        if (attendance.CheckIn1_Time.HasValue)
                        {
                            var workMinutes = (checkTime - attendance.CheckIn1_Time.Value).TotalMinutes;
                            
                            // Trừ thời gian nghỉ
                            if (attendance.CheckIn2_Time.HasValue && attendance.CheckIn3_Time.HasValue)
                            {
                                var breakMinutes = (attendance.CheckIn3_Time.Value - attendance.CheckIn2_Time.Value).TotalMinutes;
                                workMinutes -= breakMinutes;
                            }
                            
                            attendance.TotalWorkMinutes = (int)Math.Max(0, workMinutes);
                        }
                        
                        // Đánh dấu hoàn thành
                        attendance.Status = "Complete";
                        break;
                }

                attendance.CheckCount++;
                attendance.TotalPenalty = attendance.LatePenalty + attendance.OverBreakPenalty + attendance.EarlyLeavePenalty;
                attendance.UpdatedAt = DateTime.UtcNow;

                // Thêm ghi chú nếu có
                if (!string.IsNullOrEmpty(request.Note))
                {
                    attendance.Note = string.IsNullOrEmpty(attendance.Note) 
                        ? request.Note 
                        : $"{attendance.Note}; {request.Note}";
                }

                await _db.SaveChangesAsync();

                var result = MapToTodayDto(attendance);
                return Ok(new { 
                    success = true, 
                    message = GetCheckMessage(request.CheckType, attendance),
                    data = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi chấm công");
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy lịch sử chấm công
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int? month = null, [FromQuery] int? year = null)
        {
            try
            {
                var staff = await GetCurrentStaff();
                if (staff == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                var targetMonth = month ?? DateTime.UtcNow.Month;
                var targetYear = year ?? DateTime.UtcNow.Year;
                var startDate = new DateTime(targetYear, targetMonth, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var attendances = await _db.Attendances
                    .AsNoTracking()
                    .Where(a => a.StaffId == staff.Id 
                        && a.WorkDate >= startDate 
                        && a.WorkDate <= endDate)
                    .OrderByDescending(a => a.WorkDate)
                    .ToListAsync();

                var dayNames = new[] { "CN", "T2", "T3", "T4", "T5", "T6", "T7" };

                var result = attendances.Select(a => new AttendanceHistoryDto
                {
                    Id = a.Id,
                    WorkDate = a.WorkDate,
                    WorkDateString = a.WorkDate.ToString("dd/MM/yyyy"),
                    DayOfWeek = dayNames[(int)a.WorkDate.DayOfWeek],
                    CheckCount = a.CheckCount,
                    LateMinutes = a.LateMinutes,
                    TotalPenalty = a.TotalPenalty,
                    Status = a.Status,
                    StatusLabel = GetStatusLabel(a),
                    Check1Time = a.CheckIn1_Time?.ToString("HH:mm"),
                    Check2Time = a.CheckIn2_Time?.ToString("HH:mm"),
                    Check3Time = a.CheckIn3_Time?.ToString("HH:mm"),
                    Check4Time = a.CheckIn4_Time?.ToString("HH:mm")
                }).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy attendance history");
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thống kê chấm công tháng
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetAttendanceStats([FromQuery] int? month = null, [FromQuery] int? year = null)
        {
            try
            {
                var staff = await GetCurrentStaff();
                if (staff == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                var targetMonth = month ?? DateTime.UtcNow.Month;
                var targetYear = year ?? DateTime.UtcNow.Year;
                var startDate = new DateTime(targetYear, targetMonth, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var attendances = await _db.Attendances
                    .AsNoTracking()
                    .Where(a => a.StaffId == staff.Id 
                        && a.WorkDate >= startDate 
                        && a.WorkDate <= endDate)
                    .ToListAsync();

                var stats = new
                {
                    TotalDays = attendances.Count,
                    CompleteDays = attendances.Count(a => a.Status == "Complete"),
                    IncompleteDays = attendances.Count(a => a.Status == "Incomplete" && a.CheckCount > 0),
                    AbsentDays = attendances.Count(a => a.Status == "Absent"),
                    LateDays = attendances.Count(a => a.LateMinutes > 0),
                    TotalLateMinutes = attendances.Sum(a => a.LateMinutes),
                    TotalOvertimeMinutes = attendances.Sum(a => a.OvertimeMinutes),
                    TotalPenalty = attendances.Sum(a => a.TotalPenalty),
                    TotalWorkMinutes = attendances.Sum(a => a.TotalWorkMinutes)
                };

                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy attendance stats");
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        #region Private Methods

        private async Task<Staff?> GetCurrentStaff()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return null;

            // Tìm Staff record hiện có
            var staff = await _db.Staff
                .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);

            if (staff != null) return staff;

            // Auto-create Staff record nếu user có role Staff
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Staff") && !roles.Contains("Admin")) return null;

            _logger.LogInformation("🔄 [Attendance] Auto-creating Staff record for user {UserId}", userId);

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

            _logger.LogInformation("✅ [Attendance] Created Staff record Id={StaffId}", staff.Id);

            return staff;
        }

        private async Task<string?> SaveFacePhoto(int staffId, DateTime date, int checkType, string base64Data)
        {
            try
            {
                // Remove data URI prefix if present
                var base64 = base64Data;
                if (base64.Contains(","))
                {
                    base64 = base64.Split(',')[1];
                }

                var bytes = Convert.FromBase64String(base64);
                var fileName = $"{staffId}_{date:yyyyMMdd}_{checkType}.jpg";
                var relativePath = Path.Combine("uploads", "attendance", staffId.ToString());
                var fullPath = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, relativePath);

                // Create directory if not exists
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                var filePath = Path.Combine(fullPath, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                return $"/{relativePath.Replace("\\", "/")}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu ảnh khuôn mặt");
                return null;
            }
        }

        private AttendanceTodayDto MapToTodayDto(Attendance a)
        {
            var checkLabels = new[] { "", "Vào ca", "Vào nghỉ", "Hết nghỉ", "Về" };
            var nextCheckType = Math.Min(a.CheckCount + 1, 4);
            var canCheck = a.CheckCount < 4;

            return new AttendanceTodayDto
            {
                Id = a.Id,
                WorkDate = a.WorkDate,
                Check1 = a.CheckIn1_Time.HasValue ? new CheckTimeDto
                {
                    Time = a.CheckIn1_Time.Value,
                    TimeString = a.CheckIn1_Time.Value.ToString("HH:mm"),
                    PhotoUrl = a.CheckIn1_PhotoUrl,
                    IsLate = a.LateMinutes > 0,
                    LateMinutes = a.LateMinutes
                } : null,
                Check2 = a.CheckIn2_Time.HasValue ? new CheckTimeDto
                {
                    Time = a.CheckIn2_Time.Value,
                    TimeString = a.CheckIn2_Time.Value.ToString("HH:mm"),
                    PhotoUrl = a.CheckIn2_PhotoUrl
                } : null,
                Check3 = a.CheckIn3_Time.HasValue ? new CheckTimeDto
                {
                    Time = a.CheckIn3_Time.Value,
                    TimeString = a.CheckIn3_Time.Value.ToString("HH:mm"),
                    PhotoUrl = a.CheckIn3_PhotoUrl,
                    IsLate = a.OverBreakMinutes > 0,
                    LateMinutes = a.OverBreakMinutes
                } : null,
                Check4 = a.CheckIn4_Time.HasValue ? new CheckTimeDto
                {
                    Time = a.CheckIn4_Time.Value,
                    TimeString = a.CheckIn4_Time.Value.ToString("HH:mm"),
                    PhotoUrl = a.CheckIn4_PhotoUrl
                } : null,
                ScheduledStart = a.ScheduledStart.ToString(@"hh\:mm"),
                ScheduledBreakStart = a.ScheduledBreakStart?.ToString(@"hh\:mm"),
                ScheduledBreakEnd = a.ScheduledBreakEnd?.ToString(@"hh\:mm"),
                ScheduledEnd = a.ScheduledEnd.ToString(@"hh\:mm"),
                CheckCount = a.CheckCount,
                LateMinutes = a.LateMinutes,
                OverBreakMinutes = a.OverBreakMinutes,
                EarlyLeaveMinutes = a.EarlyLeaveMinutes,
                TotalPenalty = a.TotalPenalty,
                Status = a.Status,
                NextCheckType = nextCheckType,
                NextCheckLabel = canCheck ? checkLabels[nextCheckType] : "Đã hoàn thành",
                CanCheck = canCheck
            };
        }

        private string GetCheckMessage(int checkType, Attendance a)
        {
            return checkType switch
            {
                1 => a.LateMinutes > 0 
                    ? $"Đã chấm công vào ca. Trễ {a.LateMinutes} phút (-{a.LatePenalty:N0}đ)" 
                    : "Đã chấm công vào ca đúng giờ! ✅",
                2 => "Đã chấm công bắt đầu nghỉ trưa 🍜",
                3 => a.OverBreakMinutes > 0 
                    ? $"Đã chấm công hết nghỉ. Nghỉ quá {a.OverBreakMinutes} phút (-{a.OverBreakPenalty:N0}đ)"
                    : "Đã chấm công hết nghỉ đúng giờ! ✅",
                4 => $"Đã chấm công về. Tổng làm việc: {a.TotalWorkMinutes / 60}h{a.TotalWorkMinutes % 60}p 🏠",
                _ => "Đã chấm công"
            };
        }

        private string GetStatusLabel(Attendance a)
        {
            if (a.Status == "Complete")
            {
                if (a.LateMinutes > 0 || a.OverBreakMinutes > 0 || a.EarlyLeaveMinutes > 0)
                {
                    return $"⚠️ Phạt -{a.TotalPenalty:N0}đ";
                }
                return "✅ Đúng giờ";
            }
            if (a.Status == "Absent") return "❌ Vắng";
            if (a.CheckCount > 0) return $"🔄 {a.CheckCount}/4";
            return "⏳ Chưa chấm";
        }

        #endregion
    }
}
