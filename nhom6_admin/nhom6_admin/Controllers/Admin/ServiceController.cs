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
    public class ServiceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all services with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<ServiceListDto>>>> GetServices(
            [FromQuery] ServiceFilterRequest filter)
        {
            try
            {
                var query = _context.Services
                    .Where(s => !s.IsDeleted)
                    .AsQueryable();

                // Search
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    var search = filter.Search.ToLower();
                    query = query.Where(s =>
                        s.Name.ToLower().Contains(search) ||
                        s.ServiceCode.ToLower().Contains(search) ||
                        (s.ShortDescription != null && s.ShortDescription.ToLower().Contains(search)));
                }

                // Filter by price
                if (filter.MinPrice.HasValue)
                {
                    query = query.Where(s => s.Price >= filter.MinPrice.Value);
                }
                if (filter.MaxPrice.HasValue)
                {
                    query = query.Where(s => s.Price <= filter.MaxPrice.Value);
                }

                // Filter by status
                if (filter.IsActive.HasValue)
                {
                    query = query.Where(s => s.IsActive == filter.IsActive.Value);
                }
                if (filter.IsFeatured.HasValue)
                {
                    query = query.Where(s => s.IsFeatured == filter.IsFeatured.Value);
                }

                // Filter by gender
                if (!string.IsNullOrEmpty(filter.Gender))
                {
                    query = query.Where(s => s.Gender == filter.Gender || s.Gender == "All");
                }

                var totalCount = await query.CountAsync();

                // Sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "name" => filter.SortDesc ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
                    "price" => filter.SortDesc ? query.OrderByDescending(s => s.Price) : query.OrderBy(s => s.Price),
                    "duration" => filter.SortDesc ? query.OrderByDescending(s => s.DurationMinutes) : query.OrderBy(s => s.DurationMinutes),
                    "rating" => filter.SortDesc ? query.OrderByDescending(s => s.AverageRating) : query.OrderBy(s => s.AverageRating),
                    "bookings" => filter.SortDesc ? query.OrderByDescending(s => s.TotalBookings) : query.OrderBy(s => s.TotalBookings),
                    _ => query.OrderBy(s => s.DisplayOrder).ThenBy(s => s.Name)
                };

                var services = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(s => new ServiceListDto
                    {
                        Id = s.Id,
                        ServiceCode = s.ServiceCode,
                        Name = s.Name,
                        ImageUrl = s.ImageUrl,
                        Price = s.Price,
                        OriginalPrice = s.OriginalPrice,
                        DurationMinutes = s.DurationMinutes,
                        IsActive = s.IsActive,
                        IsFeatured = s.IsFeatured,
                        AverageRating = s.AverageRating,
                        TotalBookings = s.TotalBookings,
                        CreatedAt = s.CreatedAt
                    })
                    .ToListAsync();

                var response = new PaginatedResponse<ServiceListDto>
                {
                    Items = services,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

                return Ok(ApiResponse<PaginatedResponse<ServiceListDto>>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<ServiceListDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get service by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ServiceDetailDto>>> GetService(int id)
        {
            try
            {
                var service = await _context.Services
                    .Include(s => s.StaffServices!)
                        .ThenInclude(ss => ss.Staff)
                    .Where(s => s.Id == id && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (service == null)
                {
                    return NotFound(ApiResponse<ServiceDetailDto>.ErrorResponse("Service not found"));
                }

                var serviceDto = new ServiceDetailDto
                {
                    Id = service.Id,
                    ServiceCode = service.ServiceCode,
                    Name = service.Name,
                    Slug = service.Slug,
                    ShortDescription = service.ShortDescription,
                    Description = service.Description,
                    ImageUrl = service.ImageUrl,
                    GalleryImages = service.GalleryImages,
                    VideoUrl = service.VideoUrl,
                    Price = service.Price,
                    OriginalPrice = service.OriginalPrice,
                    MinPrice = service.MinPrice,
                    MaxPrice = service.MaxPrice,
                    DurationMinutes = service.DurationMinutes,
                    BufferMinutes = service.BufferMinutes,
                    RequiredStaff = service.RequiredStaff,
                    Gender = service.Gender,
                    RequiredAdvanceBookingHours = service.RequiredAdvanceBookingHours,
                    CancellationHours = service.CancellationHours,
                    IsFeatured = service.IsFeatured,
                    IsPopular = service.IsPopular,
                    IsNew = service.IsNew,
                    IsActive = service.IsActive,
                    DisplayOrder = service.DisplayOrder,
                    AverageRating = service.AverageRating,
                    TotalReviews = service.TotalReviews,
                    TotalBookings = service.TotalBookings,
                    CreatedAt = service.CreatedAt,
                    UpdatedAt = service.UpdatedAt,
                    AssignedStaff = service.StaffServices?.Where(ss => ss.Staff != null && !ss.Staff.IsDeleted)
                        .Select(ss => new StaffSimpleDto
                        {
                            Id = ss.Staff!.Id,
                            FullName = ss.Staff.FullName,
                            AvatarUrl = ss.Staff.AvatarUrl,
                            Position = ss.Staff.Position
                        }).ToList()
                };

                return Ok(ApiResponse<ServiceDetailDto>.SuccessResponse(serviceDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create new service
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ServiceDetailDto>>> CreateService([FromBody] CreateServiceRequest request)
        {
            try
            {
                // Check duplicate code
                var codeExists = await _context.Services.AnyAsync(s =>
                    s.ServiceCode.ToLower() == request.ServiceCode.ToLower() && !s.IsDeleted);
                if (codeExists)
                {
                    return BadRequest(ApiResponse<ServiceDetailDto>.ErrorResponse("Service code already exists"));
                }

                var service = new Service
                {
                    ServiceCode = request.ServiceCode,
                    Name = request.Name,
                    Slug = request.Slug ?? GenerateSlug(request.Name),
                    ShortDescription = request.ShortDescription,
                    Description = request.Description,
                    ImageUrl = request.ImageUrl,
                    GalleryImages = request.GalleryImages,
                    VideoUrl = request.VideoUrl,
                    Price = request.Price,
                    OriginalPrice = request.OriginalPrice,
                    MinPrice = request.MinPrice,
                    MaxPrice = request.MaxPrice,
                    DurationMinutes = request.DurationMinutes,
                    BufferMinutes = request.BufferMinutes,
                    RequiredStaff = request.RequiredStaff,
                    Gender = request.Gender,
                    RequiredAdvanceBookingHours = request.RequiredAdvanceBookingHours,
                    CancellationHours = request.CancellationHours,
                    IsFeatured = request.IsFeatured,
                    IsNew = request.IsNew,
                    IsActive = request.IsActive,
                    DisplayOrder = request.DisplayOrder,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Services.Add(service);
                await _context.SaveChangesAsync();

                // Add staff assignments
                if (request.StaffIds != null && request.StaffIds.Any())
                {
                    foreach (var staffId in request.StaffIds)
                    {
                        var staffExists = await _context.Staff.AnyAsync(s => s.Id == staffId && !s.IsDeleted);
                        if (staffExists)
                        {
                            var staffService = new StaffService
                            {
                                StaffId = staffId,
                                ServiceId = service.Id,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.StaffServices.Add(staffService);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return await GetService(service.Id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update service
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ServiceDetailDto>>> UpdateService(int id, [FromBody] UpdateServiceRequest request)
        {
            try
            {
                var service = await _context.Services
                    .Include(s => s.StaffServices)
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

                if (service == null)
                {
                    return NotFound(ApiResponse<ServiceDetailDto>.ErrorResponse("Service not found"));
                }

                // Check duplicate code
                if (request.ServiceCode != null && request.ServiceCode.ToLower() != service.ServiceCode.ToLower())
                {
                    var codeExists = await _context.Services.AnyAsync(s =>
                        s.ServiceCode.ToLower() == request.ServiceCode.ToLower() && s.Id != id && !s.IsDeleted);
                    if (codeExists)
                    {
                        return BadRequest(ApiResponse<ServiceDetailDto>.ErrorResponse("Service code already exists"));
                    }
                }

                // Validate category
                if (request.ServiceCategoryId.HasValue)
                {
                    var categoryExists = await _context.ServiceCategories.AnyAsync(c =>
                        c.Id == request.ServiceCategoryId.Value && !c.IsDeleted);
                    if (!categoryExists)
                    {
                        return BadRequest(ApiResponse<ServiceDetailDto>.ErrorResponse("Service category not found"));
                    }
                }

                // Update fields
                if (request.ServiceCode != null) service.ServiceCode = request.ServiceCode;
                if (request.Name != null) service.Name = request.Name;
                if (request.Slug != null) service.Slug = request.Slug;
                if (request.ShortDescription != null) service.ShortDescription = request.ShortDescription;
                if (request.Description != null) service.Description = request.Description;
                if (request.ImageUrl != null) service.ImageUrl = request.ImageUrl;
                if (request.GalleryImages != null) service.GalleryImages = request.GalleryImages;
                if (request.VideoUrl != null) service.VideoUrl = request.VideoUrl;
                if (request.Price.HasValue) service.Price = request.Price.Value;
                if (request.OriginalPrice.HasValue) service.OriginalPrice = request.OriginalPrice;
                if (request.MinPrice.HasValue) service.MinPrice = request.MinPrice;
                if (request.MaxPrice.HasValue) service.MaxPrice = request.MaxPrice;
                if (request.DurationMinutes.HasValue) service.DurationMinutes = request.DurationMinutes.Value;
                if (request.BufferMinutes.HasValue) service.BufferMinutes = request.BufferMinutes.Value;
                if (request.RequiredStaff.HasValue) service.RequiredStaff = request.RequiredStaff.Value;
                if (request.Gender != null) service.Gender = request.Gender;
                if (request.RequiredAdvanceBookingHours.HasValue) service.RequiredAdvanceBookingHours = request.RequiredAdvanceBookingHours;
                if (request.CancellationHours.HasValue) service.CancellationHours = request.CancellationHours;
                if (request.IsFeatured.HasValue) service.IsFeatured = request.IsFeatured.Value;
                if (request.IsPopular.HasValue) service.IsPopular = request.IsPopular.Value;
                if (request.IsNew.HasValue) service.IsNew = request.IsNew.Value;
                if (request.IsActive.HasValue) service.IsActive = request.IsActive.Value;
                if (request.DisplayOrder.HasValue) service.DisplayOrder = request.DisplayOrder.Value;

                // Update staff assignments
                if (request.StaffIds != null)
                {
                    // Remove old assignments
                    _context.StaffServices.RemoveRange(service.StaffServices!);

                    // Add new assignments
                    foreach (var staffId in request.StaffIds)
                    {
                        var staffExists = await _context.Staff.AnyAsync(s => s.Id == staffId && !s.IsDeleted);
                        if (staffExists)
                        {
                            var staffService = new StaffService
                            {
                                StaffId = staffId,
                                ServiceId = service.Id,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.StaffServices.Add(staffService);
                        }
                    }
                }

                service.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return await GetService(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Toggle service active status
        /// </summary>
        [HttpPut("{id}/toggle-active")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ServiceDetailDto>>> ToggleActive(int id)
        {
            try
            {
                var service = await _context.Services.FindAsync(id);
                if (service == null || service.IsDeleted)
                {
                    return NotFound(ApiResponse<ServiceDetailDto>.ErrorResponse("Service not found"));
                }

                service.IsActive = !service.IsActive;
                service.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return await GetService(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Toggle service featured status
        /// </summary>
        [HttpPut("{id}/toggle-featured")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ServiceDetailDto>>> ToggleFeatured(int id)
        {
            try
            {
                var service = await _context.Services.FindAsync(id);
                if (service == null || service.IsDeleted)
                {
                    return NotFound(ApiResponse<ServiceDetailDto>.ErrorResponse("Service not found"));
                }

                service.IsFeatured = !service.IsFeatured;
                service.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return await GetService(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete service (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteService(int id)
        {
            try
            {
                var service = await _context.Services.FindAsync(id);
                if (service == null || service.IsDeleted)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse("Service not found"));
                }

                service.IsDeleted = true;
                service.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Service deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        // ==================== SERVICE CATEGORIES ====================

        /// <summary>
        /// Get all service categories
        /// </summary>
        [HttpGet("categories")]
        public async Task<ActionResult<ApiResponse<List<ServiceCategoryDto>>>> GetServiceCategories(
            [FromQuery] bool? isActive = null,
            [FromQuery] bool tree = false)
        {
            try
            {
                var query = _context.ServiceCategories
                    .Include(c => c.ParentCategory)
                    .Include(c => c.Services)
                    .Where(c => !c.IsDeleted)
                    .AsQueryable();

                if (isActive.HasValue)
                {
                    query = query.Where(c => c.IsActive == isActive.Value);
                }

                if (tree)
                {
                    var categories = await query
                        .Where(c => c.ParentCategoryId == null)
                        .OrderBy(c => c.DisplayOrder)
                        .Select(c => new ServiceCategoryDto
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Slug = c.Slug,
                            Description = c.Description,
                            Icon = c.Icon,
                            ImageUrl = c.ImageUrl,
                            ParentCategoryId = null,
                            ParentCategoryName = null,
                            DisplayOrder = c.DisplayOrder,
                            IsActive = c.IsActive,
                            ServiceCount = c.Services != null ? c.Services.Count(s => !s.IsDeleted) : 0,
                            ChildCategories = c.ChildCategories != null
                                ? c.ChildCategories.Where(cc => !cc.IsDeleted).OrderBy(cc => cc.DisplayOrder).Select(cc => new ServiceCategoryDto
                                {
                                    Id = cc.Id,
                                    Name = cc.Name,
                                    Slug = cc.Slug,
                                    Description = cc.Description,
                                    Icon = cc.Icon,
                                    ImageUrl = cc.ImageUrl,
                                    ParentCategoryId = cc.ParentCategoryId,
                                    ParentCategoryName = c.Name,
                                    DisplayOrder = cc.DisplayOrder,
                                    IsActive = cc.IsActive,
                                    ServiceCount = cc.Services != null ? cc.Services.Count(s => !s.IsDeleted) : 0
                                }).ToList()
                                : null
                        })
                        .ToListAsync();

                    return Ok(ApiResponse<List<ServiceCategoryDto>>.SuccessResponse(categories));
                }
                else
                {
                    var categories = await query
                        .OrderBy(c => c.DisplayOrder)
                        .Select(c => new ServiceCategoryDto
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Slug = c.Slug,
                            Description = c.Description,
                            Icon = c.Icon,
                            ImageUrl = c.ImageUrl,
                            ParentCategoryId = c.ParentCategoryId,
                            ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null,
                            DisplayOrder = c.DisplayOrder,
                            IsActive = c.IsActive,
                            ServiceCount = c.Services != null ? c.Services.Count(s => !s.IsDeleted) : 0
                        })
                        .ToListAsync();

                    return Ok(ApiResponse<List<ServiceCategoryDto>>.SuccessResponse(categories));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ServiceCategoryDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get service category by ID
        /// </summary>
        [HttpGet("categories/{id}")]
        public async Task<ActionResult<ApiResponse<ServiceCategoryDto>>> GetServiceCategory(int id)
        {
            try
            {
                var category = await _context.ServiceCategories
                    .Include(c => c.ParentCategory)
                    .Include(c => c.Services)
                    .Include(c => c.ChildCategories)
                    .Where(c => c.Id == id && !c.IsDeleted)
                    .FirstOrDefaultAsync();

                if (category == null)
                {
                    return NotFound(ApiResponse<ServiceCategoryDto>.ErrorResponse("Service category not found"));
                }

                var categoryDto = new ServiceCategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Slug = category.Slug,
                    Description = category.Description,
                    Icon = category.Icon,
                    ImageUrl = category.ImageUrl,
                    ParentCategoryId = category.ParentCategoryId,
                    ParentCategoryName = category.ParentCategory?.Name,
                    DisplayOrder = category.DisplayOrder,
                    IsActive = category.IsActive,
                    ServiceCount = category.Services?.Count(s => !s.IsDeleted) ?? 0,
                    ChildCategories = category.ChildCategories?.Where(c => !c.IsDeleted).OrderBy(c => c.DisplayOrder).Select(c => new ServiceCategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Slug = c.Slug,
                        Description = c.Description,
                        Icon = c.Icon,
                        ImageUrl = c.ImageUrl,
                        ParentCategoryId = c.ParentCategoryId,
                        ParentCategoryName = category.Name,
                        DisplayOrder = c.DisplayOrder,
                        IsActive = c.IsActive,
                        ServiceCount = c.Services?.Count(s => !s.IsDeleted) ?? 0
                    }).ToList()
                };

                return Ok(ApiResponse<ServiceCategoryDto>.SuccessResponse(categoryDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceCategoryDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create service category
        /// </summary>
        [HttpPost("categories")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ServiceCategoryDto>>> CreateServiceCategory([FromBody] CreateServiceCategoryRequest request)
        {
            try
            {
                // Check duplicate name
                var nameExists = await _context.ServiceCategories.AnyAsync(c =>
                    c.Name.ToLower() == request.Name.ToLower() && !c.IsDeleted);
                if (nameExists)
                {
                    return BadRequest(ApiResponse<ServiceCategoryDto>.ErrorResponse("Category name already exists"));
                }

                // Validate parent
                if (request.ParentCategoryId.HasValue)
                {
                    var parentExists = await _context.ServiceCategories.AnyAsync(c =>
                        c.Id == request.ParentCategoryId.Value && !c.IsDeleted);
                    if (!parentExists)
                    {
                        return BadRequest(ApiResponse<ServiceCategoryDto>.ErrorResponse("Parent category not found"));
                    }
                }

                var category = new ServiceCategory
                {
                    Name = request.Name,
                    Slug = request.Slug ?? GenerateSlug(request.Name),
                    Description = request.Description,
                    Icon = request.Icon,
                    ImageUrl = request.ImageUrl,
                    ParentCategoryId = request.ParentCategoryId,
                    DisplayOrder = request.DisplayOrder,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ServiceCategories.Add(category);
                await _context.SaveChangesAsync();

                return await GetServiceCategory(category.Id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceCategoryDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update service category
        /// </summary>
        [HttpPut("categories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ServiceCategoryDto>>> UpdateServiceCategory(int id, [FromBody] UpdateServiceCategoryRequest request)
        {
            try
            {
                var category = await _context.ServiceCategories.FindAsync(id);
                if (category == null || category.IsDeleted)
                {
                    return NotFound(ApiResponse<ServiceCategoryDto>.ErrorResponse("Category not found"));
                }

                // Check duplicate name
                if (request.Name != null && request.Name.ToLower() != category.Name.ToLower())
                {
                    var nameExists = await _context.ServiceCategories.AnyAsync(c =>
                        c.Name.ToLower() == request.Name.ToLower() && c.Id != id && !c.IsDeleted);
                    if (nameExists)
                    {
                        return BadRequest(ApiResponse<ServiceCategoryDto>.ErrorResponse("Category name already exists"));
                    }
                }

                // Update fields
                if (request.Name != null) category.Name = request.Name;
                if (request.Slug != null) category.Slug = request.Slug;
                if (request.Description != null) category.Description = request.Description;
                if (request.Icon != null) category.Icon = request.Icon;
                if (request.ImageUrl != null) category.ImageUrl = request.ImageUrl;
                if (request.ParentCategoryId.HasValue) category.ParentCategoryId = request.ParentCategoryId;
                if (request.DisplayOrder.HasValue) category.DisplayOrder = request.DisplayOrder.Value;
                if (request.IsActive.HasValue) category.IsActive = request.IsActive.Value;

                category.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return await GetServiceCategory(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ServiceCategoryDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete service category
        /// </summary>
        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteServiceCategory(int id)
        {
            try
            {
                var category = await _context.ServiceCategories
                    .Include(c => c.Services)
                    .Include(c => c.ChildCategories)
                    .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

                if (category == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse("Category not found"));
                }

                if (category.Services?.Any(s => !s.IsDeleted) == true)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot delete category with active services"));
                }

                if (category.ChildCategories?.Any(c => !c.IsDeleted) == true)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot delete category with child categories"));
                }

                category.IsDeleted = true;
                category.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Category deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        private static string GenerateSlug(string name)
        {
            var slug = name.ToLower()
                .Replace(" ", "-")
                .Replace("đ", "d")
                .Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
                .Replace("ă", "a").Replace("ắ", "a").Replace("ằ", "a").Replace("ẳ", "a").Replace("ẵ", "a").Replace("ặ", "a")
                .Replace("â", "a").Replace("ấ", "a").Replace("ầ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a")
                .Replace("é", "e").Replace("è", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
                .Replace("ê", "e").Replace("ế", "e").Replace("ề", "e").Replace("ể", "e").Replace("ễ", "e").Replace("ệ", "e")
                .Replace("í", "i").Replace("ì", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i")
                .Replace("ó", "o").Replace("ò", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
                .Replace("ô", "o").Replace("ố", "o").Replace("ồ", "o").Replace("ổ", "o").Replace("ỗ", "o").Replace("ộ", "o")
                .Replace("ơ", "o").Replace("ớ", "o").Replace("ờ", "o").Replace("ở", "o").Replace("ỡ", "o").Replace("ợ", "o")
                .Replace("ú", "u").Replace("ù", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u")
                .Replace("ư", "u").Replace("ứ", "u").Replace("ừ", "u").Replace("ử", "u").Replace("ữ", "u").Replace("ự", "u")
                .Replace("ý", "y").Replace("ỳ", "y").Replace("ỷ", "y").Replace("ỹ", "y").Replace("ỵ", "y");

            return System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        }
    }
}
