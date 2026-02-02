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
    /// API Bảng lương - Tính tự động từ Attendance
    /// </summary>
    [Route("api/salary")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    public class SalaryController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<SalaryController> _logger;
        private readonly UserManager<User> _userManager;

        public SalaryController(
            ApplicationDbContext db, 
            ILogger<SalaryController> logger,
            UserManager<User> userManager)
        {
            _db = db;
            _logger = logger;
            _userManager = userManager;
        }

        /// <summary>
        /// Lấy bảng lương tháng hiện tại (tính real-time)
        /// </summary>
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentSalary()
        {
            try
            {
                var staff = await GetCurrentStaff();
                if (staff == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                var now = DateTime.UtcNow;
                var salarySlip = await CalculateSalary(staff, now.Month, now.Year);

                return Ok(new { success = true, data = salarySlip });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy current salary");
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy bảng lương theo tháng/năm
        /// </summary>
        [HttpGet("{month}/{year}")]
        public async Task<IActionResult> GetSalaryByMonth(int month, int year)
        {
            try
            {
                var staff = await GetCurrentStaff();
                if (staff == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                // Kiểm tra có bảng lương đã lưu không
                var existingSalary = await _db.SalarySlips
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.StaffId == staff.Id && s.Month == month && s.Year == year);

                if (existingSalary != null)
                {
                    var dto = MapToDto(existingSalary);
                    return Ok(new { success = true, data = dto });
                }

                // Tính toán real-time
                var salarySlip = await CalculateSalary(staff, month, year);
                return Ok(new { success = true, data = salarySlip });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy salary by month");
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy lịch sử bảng lương
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetSalaryHistory([FromQuery] int limit = 12)
        {
            try
            {
                var staff = await GetCurrentStaff();
                if (staff == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                var salaries = await _db.SalarySlips
                    .AsNoTracking()
                    .Where(s => s.StaffId == staff.Id)
                    .OrderByDescending(s => s.Year)
                    .ThenByDescending(s => s.Month)
                    .Take(limit)
                    .ToListAsync();

                var monthNames = new[] { "", "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6",
                    "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12" };

                var result = salaries.Select(s => new SalaryHistoryItemDto
                {
                    Id = s.Id,
                    Month = s.Month,
                    Year = s.Year,
                    MonthYearString = $"{monthNames[s.Month]}/{s.Year}",
                    NetSalary = s.NetSalary,
                    Status = s.Status,
                    StatusLabel = GetStatusLabel(s.Status)
                }).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy salary history");
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

            _logger.LogInformation("🔄 [Salary] Auto-creating Staff record for user {UserId}", userId);

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
                BaseSalary = 15000000m, // Default base salary
                CommissionPercent = 10,
                CreatedAt = DateTime.UtcNow
            };

            _db.Staff.Add(staff);
            await _db.SaveChangesAsync();

            _logger.LogInformation("✅ [Salary] Created Staff record Id={StaffId}", staff.Id);

            return staff;
        }

        private async Task<SalarySlipDto> CalculateSalary(Staff staff, int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Lấy dữ liệu chấm công
            var attendances = await _db.Attendances
                .AsNoTracking()
                .Where(a => a.StaffId == staff.Id && a.WorkDate >= startDate && a.WorkDate <= endDate)
                .ToListAsync();

            // Tính số ngày công
            var workDays = 26; // Mặc định 26 ngày/tháng
            var actualWorkDays = attendances.Count(a => a.Status == "Complete");
            var incompleteDays = attendances.Count(a => a.Status == "Incomplete" && a.CheckCount > 0);
            
            // Tính trễ
            var totalLateMinutes = attendances.Sum(a => a.LateMinutes + a.OverBreakMinutes + a.EarlyLeaveMinutes);
            var lateCount = attendances.Count(a => a.LateMinutes > 0 || a.OverBreakMinutes > 0);
            
            // Tính thiếu chấm công (ngày có chấm nhưng < 4 lần)
            var missedCheckDays = attendances.Count(a => a.CheckCount > 0 && a.CheckCount < 4);
            
            // Tính làm thêm giờ
            var totalOvertimeMinutes = attendances.Sum(a => a.OvertimeMinutes);

            // Lương cơ bản
            var baseSalary = staff.BaseSalary ?? 15000000m; // Default 15 triệu

            // Tính hoa hồng từ appointments
            var completedAppointments = await _db.Appointments
                .AsNoTracking()
                .Where(a => a.StaffId == staff.Id 
                    && a.AppointmentDate >= startDate 
                    && a.AppointmentDate <= endDate
                    && a.Status == "Completed")
                .ToListAsync();

            var totalRevenue = completedAppointments.Sum(a => a.TotalAmount);
            var commissionPercent = staff.CommissionPercent > 0 ? staff.CommissionPercent : 10; // Default 10%
            var commissionBonus = totalRevenue * commissionPercent / 100;

            // Tính làm thêm giờ (50k/giờ)
            var overtimeBonus = (totalOvertimeMinutes / 60m) * 50000m;

            // Tính các khoản trừ
            var latePenalty = totalLateMinutes * SalarySlip.PENALTY_PER_MINUTE;
            var missedCheckPenalty = missedCheckDays * SalarySlip.MISSED_CHECK_PENALTY;
            
            // Bảo hiểm
            var bhxh = baseSalary * SalarySlip.BHXH_RATE;
            var bhyt = baseSalary * SalarySlip.BHYT_RATE;
            var bhtn = baseSalary * SalarySlip.BHTN_RATE;

            // Tổng
            var grossIncome = baseSalary + commissionBonus + overtimeBonus;
            var totalDeductions = latePenalty + missedCheckPenalty + bhxh + bhyt + bhtn;
            var netSalary = grossIncome - totalDeductions;

            var monthNames = new[] { "", "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6",
                "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12" };

            return new SalarySlipDto
            {
                Month = month,
                Year = year,
                MonthYearString = $"{monthNames[month]}/{year}",
                WorkDays = workDays,
                ActualWorkDays = actualWorkDays,
                TotalLateMinutes = totalLateMinutes,
                LateCount = lateCount,
                MissedCheckDays = missedCheckDays,
                TotalOvertimeMinutes = totalOvertimeMinutes,
                BaseSalary = baseSalary,
                OvertimeBonus = overtimeBonus,
                CommissionBonus = commissionBonus,
                OtherAllowance = 0,
                GrossIncome = grossIncome,
                LatePenalty = latePenalty,
                MissedCheckPenalty = missedCheckPenalty,
                AbsentDeduction = 0,
                BHXH = bhxh,
                BHYT = bhyt,
                BHTN = bhtn,
                OtherDeduction = 0,
                TotalDeductions = totalDeductions,
                NetSalary = Math.Max(0, netSalary),
                Status = "Draft",
                StatusLabel = "Tạm tính"
            };
        }

        private SalarySlipDto MapToDto(SalarySlip s)
        {
            var monthNames = new[] { "", "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6",
                "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12" };

            return new SalarySlipDto
            {
                Id = s.Id,
                Month = s.Month,
                Year = s.Year,
                MonthYearString = $"{monthNames[s.Month]}/{s.Year}",
                WorkDays = s.WorkDays,
                ActualWorkDays = s.ActualWorkDays,
                TotalLateMinutes = s.TotalLateMinutes,
                LateCount = s.LateCount,
                MissedCheckDays = s.MissedCheckDays,
                TotalOvertimeMinutes = s.TotalOvertimeMinutes,
                BaseSalary = s.BaseSalary,
                OvertimeBonus = s.OvertimeBonus,
                CommissionBonus = s.CommissionBonus,
                OtherAllowance = s.OtherAllowance,
                GrossIncome = s.GrossIncome,
                LatePenalty = s.LatePenalty,
                MissedCheckPenalty = s.MissedCheckPenalty,
                AbsentDeduction = s.AbsentDeduction,
                BHXH = s.BHXH,
                BHYT = s.BHYT,
                BHTN = s.BHTN,
                OtherDeduction = s.OtherDeduction,
                TotalDeductions = s.TotalDeductions,
                NetSalary = s.NetSalary,
                Status = s.Status,
                StatusLabel = GetStatusLabel(s.Status),
                PaidAt = s.PaidAt
            };
        }

        private string GetStatusLabel(string status)
        {
            return status switch
            {
                "Draft" => "Tạm tính",
                "Confirmed" => "Đã xác nhận",
                "Paid" => "Đã thanh toán",
                _ => status
            };
        }

        #endregion
    }
}
