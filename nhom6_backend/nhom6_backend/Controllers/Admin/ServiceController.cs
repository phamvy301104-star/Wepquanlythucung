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
    /// Admin Service Management Controller
    /// Quản lý dịch vụ salon: CRUD, staff assignment, status
    /// </summary>
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class ServiceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ServiceController> _logger;

        public ServiceController(ApplicationDbContext context, ILogger<ServiceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách dịch vụ với filter và phân trang
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<AdminApiResponse<PagedResult<ServiceListDto>>>> GetServices(
            [FromQuery] ServiceFilterRequest filter)
        {
            try
            {
                var query = _context.Services
                    .AsQueryable();

                // Filter by status
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    var isActive = filter.Status.ToLower() == "active";
                    query = query.Where(s => s.IsActive == isActive);
                }

                // Search by name
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    var search = filter.SearchTerm.ToLower();
                    query = query.Where(s => s.Name.ToLower().Contains(search));
                }

                var totalCount = await query.CountAsync();

                // Sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "name" => filter.SortDesc ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
                    "price" => filter.SortDesc ? query.OrderByDescending(s => s.Price) : query.OrderBy(s => s.Price),
                    "duration" => filter.SortDesc ? query.OrderByDescending(s => s.DurationMinutes) : query.OrderBy(s => s.DurationMinutes),
                    "bookings" => filter.SortDesc ? query.OrderByDescending(s => s.TotalBookings) : query.OrderBy(s => s.TotalBookings),
                    _ => filter.SortDesc ? query.OrderBy(s => s.CreatedAt) : query.OrderByDescending(s => s.CreatedAt)
                };

                var services = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(s => new ServiceListDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        ImageUrl = s.ImageUrl,
                        Price = s.Price,
                        OriginalPrice = s.OriginalPrice,
                        DurationMinutes = s.DurationMinutes,
                        IsActive = s.IsActive,
                        IsFeatured = s.IsFeatured,
                        TotalBookings = s.TotalBookings,
                        CreatedAt = s.CreatedAt
                    })
                    .ToListAsync();

                var result = new PagedResult<ServiceListDto>
                {
                    Items = services,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

                return Ok(AdminApiResponse<PagedResult<ServiceListDto>>.Ok(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting services");
                return StatusCode(500, AdminApiResponse<PagedResult<ServiceListDto>>.Fail("Lỗi server khi lấy danh sách dịch vụ"));
            }
        }

        /// <summary>
        /// Lấy chi tiết dịch vụ
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminApiResponse<object>>> GetServiceDetail(int id)
        {
            try
            {
                var service = await _context.Services
                    .Include(s => s.StaffServices!)
                        .ThenInclude(ss => ss.Staff)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (service == null)
                    return NotFound(AdminApiResponse<object>.Fail("Dịch vụ không tồn tại"));

                var dto = new
                {
                    service.Id,
                    service.Name,
                    service.Slug,
                    service.Description,
                    service.ShortDescription,
                    service.Price,
                    service.OriginalPrice,
                    DiscountPercent = service.OriginalPrice.HasValue && service.OriginalPrice.Value > 0 
                        ? Math.Round((1 - service.Price / service.OriginalPrice.Value) * 100, 0) 
                        : 0,
                    service.DurationMinutes,
                    service.ImageUrl,
                    service.IsActive,
                    service.IsFeatured,
                    service.TotalBookings,
                    service.AverageRating,
                    service.TotalReviews,
                    AssignedStaff = service.StaffServices?.Select(ss => new
                    {
                        ss.StaffId,
                        StaffName = ss.Staff?.FullName ?? ss.Staff?.User?.FullName ?? "N/A"
                    }).ToList(),
                    service.CreatedAt,
                    service.UpdatedAt
                };

                return Ok(AdminApiResponse<object>.Ok(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service detail for {ServiceId}", id);
                return StatusCode(500, AdminApiResponse<object>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Tạo dịch vụ mới
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<int>>> CreateService([FromBody] CreateServiceRequest request)
        {
            try
            {
                // Validate category
                if (request.CategoryId.HasValue && !await _context.ServiceCategories.AnyAsync(c => c.Id == request.CategoryId))
                    return BadRequest(AdminApiResponse<int>.Fail("Danh mục dịch vụ không tồn tại"));

                // Generate slug
                var slug = GenerateSlug(request.Name);
                var existingSlug = await _context.Services.AnyAsync(s => s.Slug == slug);
                if (existingSlug)
                    slug = $"{slug}-{DateTime.Now.Ticks}";

                var service = new Service
                {
                    Name = request.Name,
                    Slug = slug,
                    Description = request.Description,
                    ShortDescription = request.ShortDescription,
                    Price = request.Price,
                    OriginalPrice = request.OriginalPrice,
                    DurationMinutes = request.DurationMinutes,
                    ImageUrl = request.ImageUrl,
                    IsActive = request.IsActive,
                    IsFeatured = request.IsFeatured,
                    TotalBookings = 0,
                    AverageRating = 0,
                    TotalReviews = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Services.Add(service);
                await _context.SaveChangesAsync();

                // Assign staff
                if (request.StaffIds != null && request.StaffIds.Count > 0)
                {
                    foreach (var staffId in request.StaffIds)
                    {
                        _context.StaffServices.Add(new StaffService
                        {
                            ServiceId = service.Id,
                            StaffId = staffId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Service created: {ServiceId} - {ServiceName}", service.Id, service.Name);

                return Ok(AdminApiResponse<int>.Ok(service.Id, "Tạo dịch vụ thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service");
                return StatusCode(500, AdminApiResponse<int>.Fail("Lỗi server khi tạo dịch vụ"));
            }
        }

        /// <summary>
        /// Cập nhật dịch vụ
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<bool>>> UpdateService(int id, [FromBody] UpdateServiceRequest request)
        {
            try
            {
                var service = await _context.Services
                    .Include(s => s.StaffServices)
                    .FirstOrDefaultAsync(s => s.Id == id);
                    
                if (service == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Dịch vụ không tồn tại"));

                // Validate category
                if (request.CategoryId.HasValue && !await _context.ServiceCategories.AnyAsync(c => c.Id == request.CategoryId))
                    return BadRequest(AdminApiResponse<bool>.Fail("Danh mục dịch vụ không tồn tại"));

                // Update slug if name changed
                if (service.Name != request.Name)
                {
                    var slug = GenerateSlug(request.Name);
                    var existingSlug = await _context.Services.AnyAsync(s => s.Slug == slug && s.Id != id);
                    service.Slug = existingSlug ? $"{slug}-{DateTime.Now.Ticks}" : slug;
                }

                service.Name = request.Name;
                service.Description = request.Description;
                service.ShortDescription = request.ShortDescription;
                service.Price = request.Price;
                service.OriginalPrice = request.OriginalPrice;
                service.DurationMinutes = request.DurationMinutes;
                service.ImageUrl = request.ImageUrl;
                service.IsActive = request.IsActive;
                service.IsFeatured = request.IsFeatured;
                service.UpdatedAt = DateTime.UtcNow;

                // Update staff assignments
                if (request.StaffIds != null)
                {
                    // Remove existing assignments
                    if (service.StaffServices != null)
                        _context.StaffServices.RemoveRange(service.StaffServices);

                    // Add new assignments
                    foreach (var staffId in request.StaffIds)
                    {
                        _context.StaffServices.Add(new StaffService
                        {
                            ServiceId = service.Id,
                            StaffId = staffId
                        });
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Service updated: {ServiceId}", id);

                return Ok(AdminApiResponse<bool>.Ok(true, "Cập nhật dịch vụ thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service {ServiceId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server khi cập nhật dịch vụ"));
            }
        }

        /// <summary>
        /// Xóa dịch vụ (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<bool>>> DeleteService(int id)
        {
            try
            {
                var service = await _context.Services.FindAsync(id);
                if (service == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Dịch vụ không tồn tại"));

                // Check if service has appointments
                var hasAppointments = await _context.AppointmentServices.AnyAsync(a => a.ServiceId == id);
                if (hasAppointments)
                {
                    // Soft delete - just deactivate
                    service.IsActive = false;
                    service.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return Ok(AdminApiResponse<bool>.Ok(true, "Dịch vụ đã được vô hiệu hóa (có lịch hẹn liên quan)"));
                }

                // Hard delete
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Service deleted: {ServiceId}", id);

                return Ok(AdminApiResponse<bool>.Ok(true, "Xóa dịch vụ thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service {ServiceId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server khi xóa dịch vụ"));
            }
        }

        /// <summary>
        /// Toggle trạng thái active
        /// </summary>
        [HttpPatch("{id}/toggle-active")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<bool>>> ToggleActive(int id)
        {
            try
            {
                var service = await _context.Services.FindAsync(id);
                if (service == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Dịch vụ không tồn tại"));

                service.IsActive = !service.IsActive;
                service.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var status = service.IsActive ? "kích hoạt" : "vô hiệu hóa";
                return Ok(AdminApiResponse<bool>.Ok(true, $"Đã {status} dịch vụ"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling service active state for {ServiceId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Toggle trạng thái featured
        /// </summary>
        [HttpPatch("{id}/toggle-featured")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<bool>>> ToggleFeatured(int id)
        {
            try
            {
                var service = await _context.Services.FindAsync(id);
                if (service == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Dịch vụ không tồn tại"));

                service.IsFeatured = !service.IsFeatured;
                service.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var status = service.IsFeatured ? "đánh dấu nổi bật" : "bỏ đánh dấu nổi bật";
                return Ok(AdminApiResponse<bool>.Ok(true, $"Đã {status} dịch vụ"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling service featured state for {ServiceId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Gán nhân viên cho dịch vụ
        /// </summary>
        [HttpPost("{id}/staff")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<bool>>> AssignStaff(int id, [FromBody] AssignStaffRequest request)
        {
            try
            {
                var service = await _context.Services.FindAsync(id);
                if (service == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Dịch vụ không tồn tại"));

                // Remove existing assignments
                var existingAssignments = await _context.StaffServices
                    .Where(ss => ss.ServiceId == id)
                    .ToListAsync();
                _context.StaffServices.RemoveRange(existingAssignments);

                // Add new assignments
                foreach (var staffId in request.StaffIds)
                {
                    _context.StaffServices.Add(new StaffService
                    {
                        ServiceId = id,
                        StaffId = staffId
                    });
                }

                await _context.SaveChangesAsync();

                return Ok(AdminApiResponse<bool>.Ok(true, "Đã cập nhật danh sách nhân viên"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning staff to service {ServiceId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Lấy thống kê dịch vụ
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<AdminApiResponse<object>>> GetServiceStats()
        {
            try
            {
                var stats = new
                {
                    TotalServices = await _context.Services.CountAsync(),
                    ActiveServices = await _context.Services.CountAsync(s => s.IsActive),
                    InactiveServices = await _context.Services.CountAsync(s => !s.IsActive),
                    FeaturedServices = await _context.Services.CountAsync(s => s.IsFeatured),
                    TotalBookings = await _context.Services.SumAsync(s => s.TotalBookings),
                    CategoriesCount = await _context.ServiceCategories.CountAsync(),
                    TopServices = await _context.Services
                        .OrderByDescending(s => s.TotalBookings)
                        .Take(5)
                        .Select(s => new { s.Id, s.Name, s.TotalBookings })
                        .ToListAsync()
                };

                return Ok(AdminApiResponse<object>.Ok(stats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service stats");
                return StatusCode(500, AdminApiResponse<object>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Lấy danh sách categories cho dropdown
        /// </summary>
        [HttpGet("categories")]
        public async Task<ActionResult<AdminApiResponse<List<object>>>> GetCategories()
        {
            try
            {
                var categories = await _context.ServiceCategories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .Select(c => new { c.Id, c.Name })
                    .ToListAsync();

                return Ok(AdminApiResponse<List<object>>.Ok(categories.Cast<object>().ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service categories");
                return StatusCode(500, AdminApiResponse<List<object>>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Lấy danh sách staff có thể gán cho dropdown
        /// </summary>
        [HttpGet("staff")]
        public async Task<ActionResult<AdminApiResponse<List<object>>>> GetStaff()
        {
            try
            {
                var staff = await _context.Staff
                    .Where(s => s.Status == "Active")
                    .OrderBy(s => s.FullName)
                    .Select(s => new { s.Id, Name = s.FullName })
                    .ToListAsync();

                return Ok(AdminApiResponse<List<object>>.Ok(staff.Cast<object>().ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staff list");
                return StatusCode(500, AdminApiResponse<List<object>>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Export danh sách dịch vụ
        /// </summary>
        [HttpGet("export")]
        public async Task<IActionResult> ExportServices()
        {
            try
            {
                var services = await _context.Services
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(10000)
                    .ToListAsync();

                var csv = new StringBuilder();
                csv.AppendLine("ID,Tên,Giá,Giá gốc,Thời gian (phút),Lượt đặt,Active,Featured,Ngày tạo");

                foreach (var s in services)
                {
                    csv.AppendLine($"{s.Id}," +
                        $"\"{s.Name.Replace("\"", "\"\"")}\"," +
                        $"{s.Price}," +
                        $"{s.OriginalPrice}," +
                        $"{s.DurationMinutes}," +
                        $"{s.TotalBookings}," +
                        $"{s.IsActive}," +
                        $"{s.IsFeatured}," +
                        $"\"{s.CreatedAt:yyyy-MM-dd}\"");
                }

                var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
                return File(bytes, "text/csv; charset=utf-8", $"services_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting services");
                return StatusCode(500, "Lỗi server khi export dịch vụ");
            }
        }

        private string GenerateSlug(string name)
        {
            var slug = name.ToLower()
                .Replace("đ", "d")
                .Replace(" ", "-");

            // Remove diacritics (Vietnamese accents)
            var normalizedString = slug.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            slug = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            // Remove special characters
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');

            return slug;
        }
    }

    /// <summary>
    /// Request gán nhân viên
    /// </summary>
    public class AssignStaffRequest
    {
        public List<int> StaffIds { get; set; } = new();
    }
}
