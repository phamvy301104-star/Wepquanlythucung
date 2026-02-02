using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Models;
using nhom6_admin.Models.DTOs;
using nhom6_admin.Models.Entities;

namespace nhom6_admin.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class StaffController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StaffController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all staff with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<StaffListDto>>>> GetStaff(
            [FromQuery] StaffFilterRequest filter)
        {
            try
            {
                var query = _context.Staff
                    .Where(s => !s.IsDeleted)
                    .AsQueryable();

                // Search
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    var search = filter.Search.ToLower();
                    query = query.Where(s =>
                        s.FullName.ToLower().Contains(search) ||
                        s.StaffCode.ToLower().Contains(search) ||
                        (s.NickName != null && s.NickName.ToLower().Contains(search)) ||
                        (s.Email != null && s.Email.ToLower().Contains(search)) ||
                        (s.PhoneNumber != null && s.PhoneNumber.Contains(search)));
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

                // Filter by status
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    query = query.Where(s => s.Status == filter.Status);
                }

                var totalCount = await query.CountAsync();

                // Sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "name" => filter.SortDesc ? query.OrderByDescending(s => s.FullName) : query.OrderBy(s => s.FullName),
                    "rating" => filter.SortDesc ? query.OrderByDescending(s => s.AverageRating) : query.OrderBy(s => s.AverageRating),
                    "experience" => filter.SortDesc ? query.OrderByDescending(s => s.YearsOfExperience) : query.OrderBy(s => s.YearsOfExperience),
                    "customers" => filter.SortDesc ? query.OrderByDescending(s => s.TotalCustomersServed) : query.OrderBy(s => s.TotalCustomersServed),
                    _ => query.OrderBy(s => s.FullName)
                };

                var staffList = await query
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
                        AverageRating = s.AverageRating,
                        TotalReviews = s.TotalReviews,
                        TotalCustomersServed = s.TotalCustomersServed,
                        Status = s.Status,
                        CreatedAt = s.CreatedAt
                    })
                    .ToListAsync();

                var response = new PaginatedResponse<StaffListDto>
                {
                    Items = staffList,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

                return Ok(ApiResponse<PaginatedResponse<StaffListDto>>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<StaffListDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get staff by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<StaffDetailDto>>> GetStaffById(int id)
        {
            try
            {
                var staff = await _context.Staff
                    .Include(s => s.StaffServices!)
                        .ThenInclude(ss => ss.Service)
                    .Include(s => s.Schedules)
                    .Where(s => s.Id == id && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (staff == null)
                {
                    return NotFound(ApiResponse<StaffDetailDto>.ErrorResponse("Staff not found"));
                }

                var dayNames = new[] { "Chủ nhật", "Thứ hai", "Thứ ba", "Thứ tư", "Thứ năm", "Thứ sáu", "Thứ bảy" };

                var staffDto = new StaffDetailDto
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
                    TiktokUrl = staff.TiktokUrl,
                    Status = staff.Status,
                    CreatedAt = staff.CreatedAt,
                    UpdatedAt = staff.UpdatedAt,
                    Services = staff.StaffServices?.Where(ss => ss.Service != null && !ss.Service.IsDeleted)
                        .Select(ss => new ServiceSimpleDto
                        {
                            Id = ss.Service!.Id,
                            Name = ss.Service.Name,
                            Price = ss.Service.Price,
                            DurationMinutes = ss.Service.DurationMinutes
                        }).ToList(),
                    Schedules = staff.Schedules?.OrderBy(sch => sch.DayOfWeek)
                        .Select(sch => new StaffScheduleDto
                        {
                            Id = sch.Id,
                            DayOfWeek = sch.DayOfWeek,
                            DayOfWeekName = dayNames[sch.DayOfWeek],
                            StartTime = sch.StartTime,
                            EndTime = sch.EndTime,
                            BreakStartTime = sch.BreakStartTime,
                            BreakEndTime = sch.BreakEndTime,
                            IsWorkingDay = sch.IsWorkingDay,
                            Notes = sch.Notes
                        }).ToList()
                };

                return Ok(ApiResponse<StaffDetailDto>.SuccessResponse(staffDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<StaffDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create new staff
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<StaffDetailDto>>> CreateStaff([FromBody] CreateStaffRequest request)
        {
            try
            {
                // Check duplicate code
                var codeExists = await _context.Staff.AnyAsync(s =>
                    s.StaffCode.ToLower() == request.StaffCode.ToLower() && !s.IsDeleted);
                if (codeExists)
                {
                    return BadRequest(ApiResponse<StaffDetailDto>.ErrorResponse("Staff code already exists"));
                }

                var staff = new Staff
                {
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
                    TiktokUrl = request.TiktokUrl,
                    Status = request.Status,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Staff.Add(staff);
                await _context.SaveChangesAsync();

                // Add service assignments
                if (request.ServiceIds != null && request.ServiceIds.Any())
                {
                    foreach (var serviceId in request.ServiceIds)
                    {
                        var serviceExists = await _context.Services.AnyAsync(s => s.Id == serviceId && !s.IsDeleted);
                        if (serviceExists)
                        {
                            var staffService = new StaffService
                            {
                                StaffId = staff.Id,
                                ServiceId = serviceId,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.StaffServices.Add(staffService);
                        }
                    }
                }

                // Add schedules
                if (request.Schedules != null && request.Schedules.Any())
                {
                    foreach (var scheduleReq in request.Schedules)
                    {
                        var schedule = new StaffSchedule
                        {
                            StaffId = staff.Id,
                            DayOfWeek = scheduleReq.DayOfWeek,
                            StartTime = scheduleReq.StartTime,
                            EndTime = scheduleReq.EndTime,
                            BreakStartTime = scheduleReq.BreakStartTime,
                            BreakEndTime = scheduleReq.BreakEndTime,
                            IsWorkingDay = scheduleReq.IsWorkingDay,
                            Notes = scheduleReq.Notes,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.StaffSchedules.Add(schedule);
                    }
                }

                await _context.SaveChangesAsync();

                return await GetStaffById(staff.Id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<StaffDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update staff
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<StaffDetailDto>>> UpdateStaff(int id, [FromBody] UpdateStaffRequest request)
        {
            try
            {
                var staff = await _context.Staff
                    .Include(s => s.StaffServices)
                    .Include(s => s.Schedules)
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

                if (staff == null)
                {
                    return NotFound(ApiResponse<StaffDetailDto>.ErrorResponse("Staff not found"));
                }

                // Check duplicate code
                if (request.StaffCode != null && request.StaffCode.ToLower() != staff.StaffCode.ToLower())
                {
                    var codeExists = await _context.Staff.AnyAsync(s =>
                        s.StaffCode.ToLower() == request.StaffCode.ToLower() && s.Id != id && !s.IsDeleted);
                    if (codeExists)
                    {
                        return BadRequest(ApiResponse<StaffDetailDto>.ErrorResponse("Staff code already exists"));
                    }
                }

                // Update fields
                if (request.StaffCode != null) staff.StaffCode = request.StaffCode;
                if (request.FullName != null) staff.FullName = request.FullName;
                if (request.NickName != null) staff.NickName = request.NickName;
                if (request.Email != null) staff.Email = request.Email;
                if (request.PhoneNumber != null) staff.PhoneNumber = request.PhoneNumber;
                if (request.AvatarUrl != null) staff.AvatarUrl = request.AvatarUrl;
                if (request.CoverImageUrl != null) staff.CoverImageUrl = request.CoverImageUrl;
                if (request.Bio != null) staff.Bio = request.Bio;
                if (request.Position != null) staff.Position = request.Position;
                if (request.Level != null) staff.Level = request.Level;
                if (request.Specialties != null) staff.Specialties = request.Specialties;
                if (request.YearsOfExperience.HasValue) staff.YearsOfExperience = request.YearsOfExperience.Value;
                if (request.DateOfBirth.HasValue) staff.DateOfBirth = request.DateOfBirth;
                if (request.Gender != null) staff.Gender = request.Gender;
                if (request.HireDate.HasValue) staff.HireDate = request.HireDate.Value;
                if (request.BaseSalary.HasValue) staff.BaseSalary = request.BaseSalary;
                if (request.CommissionPercent.HasValue) staff.CommissionPercent = request.CommissionPercent.Value;
                if (request.FacebookUrl != null) staff.FacebookUrl = request.FacebookUrl;
                if (request.InstagramUrl != null) staff.InstagramUrl = request.InstagramUrl;
                if (request.TiktokUrl != null) staff.TiktokUrl = request.TiktokUrl;
                if (request.Status != null) staff.Status = request.Status;

                // Update service assignments
                if (request.ServiceIds != null)
                {
                    _context.StaffServices.RemoveRange(staff.StaffServices!);

                    foreach (var serviceId in request.ServiceIds)
                    {
                        var serviceExists = await _context.Services.AnyAsync(s => s.Id == serviceId && !s.IsDeleted);
                        if (serviceExists)
                        {
                            var staffService = new StaffService
                            {
                                StaffId = staff.Id,
                                ServiceId = serviceId,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.StaffServices.Add(staffService);
                        }
                    }
                }

                // Update schedules
                if (request.Schedules != null)
                {
                    _context.StaffSchedules.RemoveRange(staff.Schedules!);

                    foreach (var scheduleReq in request.Schedules)
                    {
                        var schedule = new StaffSchedule
                        {
                            StaffId = staff.Id,
                            DayOfWeek = scheduleReq.DayOfWeek,
                            StartTime = scheduleReq.StartTime,
                            EndTime = scheduleReq.EndTime,
                            BreakStartTime = scheduleReq.BreakStartTime,
                            BreakEndTime = scheduleReq.BreakEndTime,
                            IsWorkingDay = scheduleReq.IsWorkingDay,
                            Notes = scheduleReq.Notes,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.StaffSchedules.Add(schedule);
                    }
                }

                staff.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return await GetStaffById(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<StaffDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update staff status
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<StaffDetailDto>>> UpdateStaffStatus(int id, [FromBody] UpdateStaffStatusRequest request)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(id);
                if (staff == null || staff.IsDeleted)
                {
                    return NotFound(ApiResponse<StaffDetailDto>.ErrorResponse("Staff not found"));
                }

                var validStatuses = new[] { "Active", "OnLeave", "Resigned" };
                if (!validStatuses.Contains(request.Status))
                {
                    return BadRequest(ApiResponse<StaffDetailDto>.ErrorResponse("Invalid status"));
                }

                staff.Status = request.Status;
                staff.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await GetStaffById(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<StaffDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete staff (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteStaff(int id)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(id);
                if (staff == null || staff.IsDeleted)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse("Staff not found"));
                }

                staff.IsDeleted = true;
                staff.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Staff deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get staff services
        /// </summary>
        [HttpGet("{id}/services")]
        public async Task<ActionResult<ApiResponse<List<ServiceSimpleDto>>>> GetStaffServices(int id)
        {
            try
            {
                var staff = await _context.Staff
                    .Include(s => s.StaffServices!)
                        .ThenInclude(ss => ss.Service)
                    .Where(s => s.Id == id && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (staff == null)
                {
                    return NotFound(ApiResponse<List<ServiceSimpleDto>>.ErrorResponse("Staff not found"));
                }

                var services = staff.StaffServices?
                    .Where(ss => ss.Service != null && !ss.Service.IsDeleted)
                    .Select(ss => new ServiceSimpleDto
                    {
                        Id = ss.Service!.Id,
                        Name = ss.Service.Name,
                        Price = ss.Service.Price,
                        DurationMinutes = ss.Service.DurationMinutes
                    })
                    .ToList() ?? new List<ServiceSimpleDto>();

                return Ok(ApiResponse<List<ServiceSimpleDto>>.SuccessResponse(services));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ServiceSimpleDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get staff schedules
        /// </summary>
        [HttpGet("{id}/schedules")]
        public async Task<ActionResult<ApiResponse<List<StaffScheduleDto>>>> GetStaffSchedules(int id)
        {
            try
            {
                var staff = await _context.Staff
                    .Include(s => s.Schedules)
                    .Where(s => s.Id == id && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (staff == null)
                {
                    return NotFound(ApiResponse<List<StaffScheduleDto>>.ErrorResponse("Staff not found"));
                }

                var dayNames = new[] { "Chủ nhật", "Thứ hai", "Thứ ba", "Thứ tư", "Thứ năm", "Thứ sáu", "Thứ bảy" };

                var schedules = staff.Schedules?
                    .OrderBy(sch => sch.DayOfWeek)
                    .Select(sch => new StaffScheduleDto
                    {
                        Id = sch.Id,
                        DayOfWeek = sch.DayOfWeek,
                        DayOfWeekName = dayNames[sch.DayOfWeek],
                        StartTime = sch.StartTime,
                        EndTime = sch.EndTime,
                        BreakStartTime = sch.BreakStartTime,
                        BreakEndTime = sch.BreakEndTime,
                        IsWorkingDay = sch.IsWorkingDay,
                        Notes = sch.Notes
                    })
                    .ToList() ?? new List<StaffScheduleDto>();

                return Ok(ApiResponse<List<StaffScheduleDto>>.SuccessResponse(schedules));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<StaffScheduleDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Check staff availability
        /// </summary>
        [HttpGet("{id}/availability")]
        public async Task<ActionResult<ApiResponse<StaffAvailabilityDto>>> CheckStaffAvailability(
            int id,
            [FromQuery] DateTime date)
        {
            try
            {
                var staff = await _context.Staff
                    .Include(s => s.Schedules)
                    .Include(s => s.Appointments)
                    .Where(s => s.Id == id && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (staff == null)
                {
                    return NotFound(ApiResponse<StaffAvailabilityDto>.ErrorResponse("Staff not found"));
                }

                var dayOfWeek = (int)date.DayOfWeek;
                var schedule = staff.Schedules?.FirstOrDefault(s => s.DayOfWeek == dayOfWeek);

                if (schedule == null || !schedule.IsWorkingDay)
                {
                    return Ok(ApiResponse<StaffAvailabilityDto>.SuccessResponse(new StaffAvailabilityDto
                    {
                        StaffId = staff.Id,
                        StaffName = staff.FullName,
                        AvatarUrl = staff.AvatarUrl,
                        AvailableSlots = new List<TimeSlotDto>()
                    }));
                }

                // Get appointments for the date
                var appointments = staff.Appointments?
                    .Where(a => !a.IsDeleted && a.AppointmentDate.Date == date.Date &&
                               a.Status != "Cancelled" && a.Status != "NoShow")
                    .ToList() ?? new List<Appointment>();

                // Generate time slots
                var slots = new List<TimeSlotDto>();
                var currentTime = schedule.StartTime;
                var slotDuration = TimeSpan.FromMinutes(30);

                while (currentTime < schedule.EndTime)
                {
                    var slotEnd = currentTime.Add(slotDuration);
                    var isAvailable = true;

                    // Check break time
                    if (schedule.BreakStartTime.HasValue && schedule.BreakEndTime.HasValue)
                    {
                        if (currentTime >= schedule.BreakStartTime.Value && currentTime < schedule.BreakEndTime.Value)
                        {
                            isAvailable = false;
                        }
                    }

                    // Check existing appointments
                    foreach (var apt in appointments)
                    {
                        if (currentTime < apt.EndTime && slotEnd > apt.StartTime)
                        {
                            isAvailable = false;
                            break;
                        }
                    }

                    slots.Add(new TimeSlotDto
                    {
                        StartTime = currentTime,
                        EndTime = slotEnd,
                        IsAvailable = isAvailable
                    });

                    currentTime = slotEnd;
                }

                return Ok(ApiResponse<StaffAvailabilityDto>.SuccessResponse(new StaffAvailabilityDto
                {
                    StaffId = staff.Id,
                    StaffName = staff.FullName,
                    AvatarUrl = staff.AvatarUrl,
                    AvailableSlots = slots
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<StaffAvailabilityDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all active staff for selection
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<List<StaffSimpleDto>>>> GetActiveStaff()
        {
            try
            {
                var staffList = await _context.Staff
                    .Where(s => !s.IsDeleted && s.Status == "Active")
                    .OrderBy(s => s.FullName)
                    .Select(s => new StaffSimpleDto
                    {
                        Id = s.Id,
                        FullName = s.FullName,
                        AvatarUrl = s.AvatarUrl,
                        Position = s.Position,
                        Level = s.Level,
                        AverageRating = s.AverageRating
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<StaffSimpleDto>>.SuccessResponse(staffList));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<StaffSimpleDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }
    }
}
