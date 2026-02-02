using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_backend.Models;
using nhom6_backend.Models.DTOs.Admin;
using nhom6_backend.Models.Entities;

namespace nhom6_backend.Controllers.Admin
{
    /// <summary>
    /// Admin Staff Management Controller
    /// Quản lý nhân viên salon: CRUD đầy đủ
    /// </summary>
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class StaffController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StaffController> _logger;

        public StaffController(ApplicationDbContext context, ILogger<StaffController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách nhân viên với filter và phân trang
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<AdminApiResponse<PagedResult<StaffListDto>>>> GetStaffs(
            [FromQuery] StaffFilterRequest filter)
        {
            try
            {
                var query = _context.Staff.AsQueryable();

                // Filter by status
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    query = query.Where(s => s.Status == filter.Status);
                }

                // Filter by position
                if (!string.IsNullOrEmpty(filter.Position))
                {
                    query = query.Where(s => s.Position == filter.Position);
                }

                // Filter by level
                if (!string.IsNullOrEmpty(filter.Level))
                {
                    query = query.Where(s => s.Level == filter.Level);
                }

                // Filter by availability
                if (filter.IsAvailable.HasValue)
                {
                    query = query.Where(s => s.IsAvailable == filter.IsAvailable.Value);
                }

                // Filter by online booking
                if (filter.AcceptOnlineBooking.HasValue)
                {
                    query = query.Where(s => s.AcceptOnlineBooking == filter.AcceptOnlineBooking.Value);
                }

                // Search by name, phone, email, code
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    var search = filter.SearchTerm.ToLower();
                    query = query.Where(s =>
                        s.FullName.ToLower().Contains(search) ||
                        s.StaffCode.ToLower().Contains(search) ||
                        (s.NickName != null && s.NickName.ToLower().Contains(search)) ||
                        (s.PhoneNumber != null && s.PhoneNumber.Contains(search)) ||
                        (s.Email != null && s.Email.ToLower().Contains(search)));
                }

                var totalCount = await query.CountAsync();

                // Sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "name" => filter.SortDesc ? query.OrderByDescending(s => s.FullName) : query.OrderBy(s => s.FullName),
                    "rating" => filter.SortDesc ? query.OrderByDescending(s => s.AverageRating) : query.OrderBy(s => s.AverageRating),
                    "customers" => filter.SortDesc ? query.OrderByDescending(s => s.TotalCustomersServed) : query.OrderBy(s => s.TotalCustomersServed),
                    "experience" => filter.SortDesc ? query.OrderByDescending(s => s.YearsOfExperience) : query.OrderBy(s => s.YearsOfExperience),
                    "createdat" => filter.SortDesc ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
                    _ => filter.SortDesc ? query.OrderByDescending(s => s.DisplayOrder) : query.OrderBy(s => s.DisplayOrder)
                };

                var staffs = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(s => new StaffListDto
                    {
                        Id = s.Id,
                        StaffCode = s.StaffCode,
                        FullName = s.FullName,
                        NickName = s.NickName,
                        Email = s.Email,
                        PhoneNumber = s.PhoneNumber,
                        AvatarUrl = s.AvatarUrl,
                        Position = s.Position,
                        Level = s.Level,
                        YearsOfExperience = s.YearsOfExperience,
                        Status = s.Status,
                        IsAvailable = s.IsAvailable,
                        AcceptOnlineBooking = s.AcceptOnlineBooking,
                        AverageRating = s.AverageRating,
                        TotalReviews = s.TotalReviews,
                        TotalCustomersServed = s.TotalCustomersServed,
                        DisplayOrder = s.DisplayOrder,
                        CreatedAt = s.CreatedAt
                    })
                    .ToListAsync();

                var result = new PagedResult<StaffListDto>
                {
                    Items = staffs,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

                return Ok(AdminApiResponse<PagedResult<StaffListDto>>.Ok(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staffs");
                return StatusCode(500, AdminApiResponse<PagedResult<StaffListDto>>.Fail("Lỗi server khi lấy danh sách nhân viên"));
            }
        }

        /// <summary>
        /// Lấy chi tiết nhân viên
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminApiResponse<StaffDetailDto>>> GetStaffDetail(int id)
        {
            try
            {
                var staff = await _context.Staff
                    .Include(s => s.StaffServices!)
                        .ThenInclude(ss => ss.Service)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (staff == null)
                    return NotFound(AdminApiResponse<StaffDetailDto>.Fail("Nhân viên không tồn tại"));

                var dto = new StaffDetailDto
                {
                    Id = staff.Id,
                    UserId = staff.UserId,
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
                    BaseSalary = staff.BaseSalary,
                    CommissionPercent = staff.CommissionPercent,
                    AverageRating = staff.AverageRating,
                    TotalReviews = staff.TotalReviews,
                    TotalCustomersServed = staff.TotalCustomersServed,
                    TotalRevenue = staff.TotalRevenue,
                    FacebookUrl = staff.FacebookUrl,
                    InstagramUrl = staff.InstagramUrl,
                    TikTokUrl = staff.TikTokUrl,
                    Status = staff.Status,
                    IsAvailable = staff.IsAvailable,
                    AcceptOnlineBooking = staff.AcceptOnlineBooking,
                    DisplayOrder = staff.DisplayOrder,
                    CreatedAt = staff.CreatedAt,
                    UpdatedAt = staff.UpdatedAt,
                    ServiceIds = staff.StaffServices?.Select(ss => ss.ServiceId).ToList(),
                    ServiceNames = staff.StaffServices?.Select(ss => ss.Service?.Name ?? "").ToList()
                };

                return Ok(AdminApiResponse<StaffDetailDto>.Ok(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staff detail for {StaffId}", id);
                return StatusCode(500, AdminApiResponse<StaffDetailDto>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Tạo nhân viên mới
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<int>>> CreateStaff([FromBody] CreateStaffRequest request)
        {
            try
            {
                // Check duplicate staff code
                var existingCode = await _context.Staff.AnyAsync(s => s.StaffCode == request.StaffCode);
                if (existingCode)
                    return BadRequest(AdminApiResponse<int>.Fail("Mã nhân viên đã tồn tại"));

                // Check duplicate email
                if (!string.IsNullOrEmpty(request.Email))
                {
                    var existingEmail = await _context.Staff.AnyAsync(s => s.Email == request.Email);
                    if (existingEmail)
                        return BadRequest(AdminApiResponse<int>.Fail("Email đã được sử dụng"));
                }

                // Check duplicate phone
                if (!string.IsNullOrEmpty(request.PhoneNumber))
                {
                    var existingPhone = await _context.Staff.AnyAsync(s => s.PhoneNumber == request.PhoneNumber);
                    if (existingPhone)
                        return BadRequest(AdminApiResponse<int>.Fail("Số điện thoại đã được sử dụng"));
                }

                var staff = new Staff
                {
                    UserId = request.UserId,
                    StaffCode = request.StaffCode,
                    FullName = request.FullName,
                    NickName = request.NickName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    AvatarUrl = request.AvatarUrl,
                    CoverImageUrl = request.CoverImageUrl,
                    Bio = request.Bio,
                    Position = request.Position,
                    Level = request.Level,
                    Specialties = request.Specialties,
                    YearsOfExperience = request.YearsOfExperience,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                    HireDate = request.HireDate ?? DateTime.UtcNow,
                    BaseSalary = request.BaseSalary,
                    CommissionPercent = request.CommissionPercent,
                    FacebookUrl = request.FacebookUrl,
                    InstagramUrl = request.InstagramUrl,
                    TikTokUrl = request.TikTokUrl,
                    Status = request.Status,
                    IsAvailable = request.IsAvailable,
                    AcceptOnlineBooking = request.AcceptOnlineBooking,
                    DisplayOrder = request.DisplayOrder,
                    AverageRating = 0,
                    TotalReviews = 0,
                    TotalCustomersServed = 0,
                    TotalRevenue = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Staff.Add(staff);
                await _context.SaveChangesAsync();

                // Assign services
                if (request.ServiceIds != null && request.ServiceIds.Count > 0)
                {
                    foreach (var serviceId in request.ServiceIds)
                    {
                        var serviceExists = await _context.Services.AnyAsync(s => s.Id == serviceId);
                        if (serviceExists)
                        {
                            _context.StaffServices.Add(new StaffService
                            {
                                StaffId = staff.Id,
                                ServiceId = serviceId
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Staff created: {StaffId} - {StaffName}", staff.Id, staff.FullName);

                return Ok(AdminApiResponse<int>.Ok(staff.Id, "Tạo nhân viên thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating staff");
                return StatusCode(500, AdminApiResponse<int>.Fail("Lỗi server khi tạo nhân viên"));
            }
        }

        /// <summary>
        /// Cập nhật nhân viên
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<bool>>> UpdateStaff(int id, [FromBody] UpdateStaffRequest request)
        {
            try
            {
                var staff = await _context.Staff
                    .Include(s => s.StaffServices)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (staff == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Nhân viên không tồn tại"));

                // Check duplicate staff code (exclude current)
                var existingCode = await _context.Staff.AnyAsync(s => s.StaffCode == request.StaffCode && s.Id != id);
                if (existingCode)
                    return BadRequest(AdminApiResponse<bool>.Fail("Mã nhân viên đã tồn tại"));

                // Check duplicate email (exclude current)
                if (!string.IsNullOrEmpty(request.Email))
                {
                    var existingEmail = await _context.Staff.AnyAsync(s => s.Email == request.Email && s.Id != id);
                    if (existingEmail)
                        return BadRequest(AdminApiResponse<bool>.Fail("Email đã được sử dụng"));
                }

                // Check duplicate phone (exclude current)
                if (!string.IsNullOrEmpty(request.PhoneNumber))
                {
                    var existingPhone = await _context.Staff.AnyAsync(s => s.PhoneNumber == request.PhoneNumber && s.Id != id);
                    if (existingPhone)
                        return BadRequest(AdminApiResponse<bool>.Fail("Số điện thoại đã được sử dụng"));
                }

                staff.UserId = request.UserId;
                staff.StaffCode = request.StaffCode;
                staff.FullName = request.FullName;
                staff.NickName = request.NickName;
                staff.Email = request.Email;
                staff.PhoneNumber = request.PhoneNumber;
                staff.AvatarUrl = request.AvatarUrl;
                staff.CoverImageUrl = request.CoverImageUrl;
                staff.Bio = request.Bio;
                staff.Position = request.Position;
                staff.Level = request.Level;
                staff.Specialties = request.Specialties;
                staff.YearsOfExperience = request.YearsOfExperience;
                staff.DateOfBirth = request.DateOfBirth;
                staff.Gender = request.Gender;
                if (request.HireDate.HasValue)
                    staff.HireDate = request.HireDate.Value;
                staff.BaseSalary = request.BaseSalary;
                staff.CommissionPercent = request.CommissionPercent;
                staff.FacebookUrl = request.FacebookUrl;
                staff.InstagramUrl = request.InstagramUrl;
                staff.TikTokUrl = request.TikTokUrl;
                staff.Status = request.Status;
                staff.IsAvailable = request.IsAvailable;
                staff.AcceptOnlineBooking = request.AcceptOnlineBooking;
                staff.DisplayOrder = request.DisplayOrder;
                staff.UpdatedAt = DateTime.UtcNow;

                // Update services
                if (request.ServiceIds != null)
                {
                    // Remove existing assignments
                    if (staff.StaffServices != null)
                        _context.StaffServices.RemoveRange(staff.StaffServices);

                    // Add new assignments
                    foreach (var serviceId in request.ServiceIds)
                    {
                        var serviceExists = await _context.Services.AnyAsync(s => s.Id == serviceId);
                        if (serviceExists)
                        {
                            _context.StaffServices.Add(new StaffService
                            {
                                StaffId = staff.Id,
                                ServiceId = serviceId
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Staff updated: {StaffId}", id);

                return Ok(AdminApiResponse<bool>.Ok(true, "Cập nhật nhân viên thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff {StaffId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server khi cập nhật nhân viên"));
            }
        }

        /// <summary>
        /// Xóa nhân viên (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<bool>>> DeleteStaff(int id)
        {
            try
            {
                var staff = await _context.Staff
                    .Include(s => s.Appointments)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (staff == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Nhân viên không tồn tại"));

                // Check if has appointments
                var hasActiveAppointments = staff.Appointments?.Any(a =>
                    a.Status != "Completed" && a.Status != "Cancelled") ?? false;

                if (hasActiveAppointments)
                {
                    // Soft delete - change status to Resigned
                    staff.Status = "Resigned";
                    staff.IsAvailable = false;
                    staff.AcceptOnlineBooking = false;
                    staff.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return Ok(AdminApiResponse<bool>.Ok(true, "Nhân viên đã được chuyển sang trạng thái nghỉ việc (có lịch hẹn đang xử lý)"));
                }

                // Check if ever had appointments (for audit)
                var hasAnyAppointments = staff.Appointments?.Any() ?? false;
                if (hasAnyAppointments)
                {
                    // Soft delete
                    staff.Status = "Resigned";
                    staff.IsAvailable = false;
                    staff.AcceptOnlineBooking = false;
                    staff.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return Ok(AdminApiResponse<bool>.Ok(true, "Nhân viên đã được chuyển sang trạng thái nghỉ việc"));
                }

                // Hard delete if never had any appointments
                // Remove staff services first
                var staffServices = await _context.StaffServices.Where(ss => ss.StaffId == id).ToListAsync();
                _context.StaffServices.RemoveRange(staffServices);

                _context.Staff.Remove(staff);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Staff deleted: {StaffId}", id);

                return Ok(AdminApiResponse<bool>.Ok(true, "Xóa nhân viên thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting staff {StaffId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server khi xóa nhân viên"));
            }
        }

        /// <summary>
        /// Toggle trạng thái available
        /// </summary>
        [HttpPatch("{id}/toggle-available")]
        public async Task<ActionResult<AdminApiResponse<bool>>> ToggleAvailable(int id)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(id);
                if (staff == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Nhân viên không tồn tại"));

                staff.IsAvailable = !staff.IsAvailable;
                staff.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var status = staff.IsAvailable ? "sẵn sàng" : "không sẵn sàng";
                return Ok(AdminApiResponse<bool>.Ok(true, $"Nhân viên đã được đánh dấu {status}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling staff available state for {StaffId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Toggle trạng thái accept online booking
        /// </summary>
        [HttpPatch("{id}/toggle-online-booking")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<bool>>> ToggleOnlineBooking(int id)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(id);
                if (staff == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Nhân viên không tồn tại"));

                staff.AcceptOnlineBooking = !staff.AcceptOnlineBooking;
                staff.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var status = staff.AcceptOnlineBooking ? "nhận" : "không nhận";
                return Ok(AdminApiResponse<bool>.Ok(true, $"Nhân viên đã được thiết lập {status} đặt lịch online"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling staff online booking for {StaffId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Lấy danh sách nhân viên đang available
        /// </summary>
        [HttpGet("available")]
        [AllowAnonymous]
        public async Task<ActionResult<AdminApiResponse<List<StaffListDto>>>> GetAvailableStaffs()
        {
            try
            {
                var staffs = await _context.Staff
                    .Where(s => s.Status == "Active" && s.IsAvailable && s.AcceptOnlineBooking)
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.FullName)
                    .Select(s => new StaffListDto
                    {
                        Id = s.Id,
                        StaffCode = s.StaffCode,
                        FullName = s.FullName,
                        NickName = s.NickName,
                        AvatarUrl = s.AvatarUrl,
                        Position = s.Position,
                        Level = s.Level,
                        YearsOfExperience = s.YearsOfExperience,
                        AverageRating = s.AverageRating,
                        TotalReviews = s.TotalReviews,
                        TotalCustomersServed = s.TotalCustomersServed,
                        IsAvailable = s.IsAvailable,
                        AcceptOnlineBooking = s.AcceptOnlineBooking,
                        Status = s.Status,
                        DisplayOrder = s.DisplayOrder,
                        CreatedAt = s.CreatedAt
                    })
                    .ToListAsync();

                return Ok(AdminApiResponse<List<StaffListDto>>.Ok(staffs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available staffs");
                return StatusCode(500, AdminApiResponse<List<StaffListDto>>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Lấy danh sách nhân viên cho dropdown
        /// </summary>
        [HttpGet("dropdown")]
        [AllowAnonymous]
        public async Task<ActionResult<AdminApiResponse<List<object>>>> GetStaffsForDropdown()
        {
            try
            {
                var staffs = await _context.Staff
                    .Where(s => s.Status == "Active")
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.FullName)
                    .Select(s => new
                    {
                        s.Id,
                        s.StaffCode,
                        s.FullName,
                        s.NickName,
                        s.AvatarUrl,
                        s.Position,
                        s.IsAvailable
                    })
                    .ToListAsync();

                return Ok(AdminApiResponse<List<object>>.Ok(staffs.Cast<object>().ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staffs for dropdown");
                return StatusCode(500, AdminApiResponse<List<object>>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Cập nhật trạng thái nhân viên
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<bool>>> UpdateStatus(int id, [FromBody] string status)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(id);
                if (staff == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Nhân viên không tồn tại"));

                var validStatuses = new[] { "Active", "OnLeave", "Resigned" };
                if (!validStatuses.Contains(status))
                    return BadRequest(AdminApiResponse<bool>.Fail($"Trạng thái không hợp lệ. Các trạng thái: {string.Join(", ", validStatuses)}"));

                staff.Status = status;
                
                // Auto update availability based on status
                if (status != "Active")
                {
                    staff.IsAvailable = false;
                    staff.AcceptOnlineBooking = false;
                }

                staff.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(AdminApiResponse<bool>.Ok(true, $"Đã cập nhật trạng thái thành {status}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff status for {StaffId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server"));
            }
        }
    }
}
