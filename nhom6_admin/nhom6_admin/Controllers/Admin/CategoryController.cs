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
    [Authorize(Roles = "Admin")]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetCategories(
            [FromQuery] bool? isActive = null,
            [FromQuery] bool tree = false)
        {
            try
            {
                var query = _context.Categories
                    .Include(c => c.ParentCategory)
                    .Include(c => c.Products)
                    .Where(c => !c.IsDeleted)
                    .AsQueryable();

                if (isActive.HasValue)
                {
                    query = query.Where(c => c.IsActive == isActive.Value);
                }

                if (tree)
                {
                    // Return only root categories with children
                    var categories = await query
                        .Where(c => c.ParentCategoryId == null)
                        .OrderBy(c => c.DisplayOrder)
                        .Select(c => new CategoryDto
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Slug = c.Slug,
                            Description = c.Description,
                            ImageUrl = c.ImageUrl,
                            Icon = c.Icon,
                            ParentCategoryId = null,
                            ParentCategoryName = null,
                            DisplayOrder = c.DisplayOrder,
                            IsActive = c.IsActive,
                            ProductCount = c.Products != null ? c.Products.Count(p => !p.IsDeleted) : 0,
                            ChildCategories = c.ChildCategories != null
                                ? c.ChildCategories.Where(cc => !cc.IsDeleted).OrderBy(cc => cc.DisplayOrder).Select(cc => new CategoryDto
                                {
                                    Id = cc.Id,
                                    Name = cc.Name,
                                    Slug = cc.Slug,
                                    Description = cc.Description,
                                    ImageUrl = cc.ImageUrl,
                                    Icon = cc.Icon,
                                    ParentCategoryId = cc.ParentCategoryId,
                                    ParentCategoryName = c.Name,
                                    DisplayOrder = cc.DisplayOrder,
                                    IsActive = cc.IsActive,
                                    ProductCount = cc.Products != null ? cc.Products.Count(p => !p.IsDeleted) : 0
                                }).ToList()
                                : null
                        })
                        .ToListAsync();

                    return Ok(ApiResponse<List<CategoryDto>>.SuccessResponse(categories));
                }
                else
                {
                    var categories = await query
                        .OrderBy(c => c.DisplayOrder)
                        .Select(c => new CategoryDto
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Slug = c.Slug,
                            Description = c.Description,
                            ImageUrl = c.ImageUrl,
                            Icon = c.Icon,
                            ParentCategoryId = c.ParentCategoryId,
                            ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null,
                            DisplayOrder = c.DisplayOrder,
                            IsActive = c.IsActive,
                            ProductCount = c.Products != null ? c.Products.Count(p => !p.IsDeleted) : 0
                        })
                        .ToListAsync();

                    return Ok(ApiResponse<List<CategoryDto>>.SuccessResponse(categories));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<CategoryDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.ParentCategory)
                    .Include(c => c.Products)
                    .Include(c => c.ChildCategories)
                    .Where(c => c.Id == id && !c.IsDeleted)
                    .FirstOrDefaultAsync();

                if (category == null)
                {
                    return NotFound(ApiResponse<CategoryDto>.ErrorResponse("Category not found"));
                }

                var categoryDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Slug = category.Slug,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    Icon = category.Icon,
                    ParentCategoryId = category.ParentCategoryId,
                    ParentCategoryName = category.ParentCategory?.Name,
                    DisplayOrder = category.DisplayOrder,
                    IsActive = category.IsActive,
                    ProductCount = category.Products?.Count(p => !p.IsDeleted) ?? 0,
                    ChildCategories = category.ChildCategories?.Where(c => !c.IsDeleted).OrderBy(c => c.DisplayOrder).Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Slug = c.Slug,
                        Description = c.Description,
                        ImageUrl = c.ImageUrl,
                        Icon = c.Icon,
                        ParentCategoryId = c.ParentCategoryId,
                        ParentCategoryName = category.Name,
                        DisplayOrder = c.DisplayOrder,
                        IsActive = c.IsActive,
                        ProductCount = c.Products?.Count(p => !p.IsDeleted) ?? 0
                    }).ToList()
                };

                return Ok(ApiResponse<CategoryDto>.SuccessResponse(categoryDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create new category
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            try
            {
                // Validate parent category if provided
                if (request.ParentCategoryId.HasValue)
                {
                    var parentExists = await _context.Categories.AnyAsync(c => c.Id == request.ParentCategoryId.Value && !c.IsDeleted);
                    if (!parentExists)
                    {
                        return BadRequest(ApiResponse<CategoryDto>.ErrorResponse("Parent category not found"));
                    }
                }

                // Check duplicate name
                var nameExists = await _context.Categories.AnyAsync(c =>
                    c.Name.ToLower() == request.Name.ToLower() && !c.IsDeleted);
                if (nameExists)
                {
                    return BadRequest(ApiResponse<CategoryDto>.ErrorResponse("Category name already exists"));
                }

                var category = new Category
                {
                    Name = request.Name,
                    Slug = request.Slug ?? GenerateSlug(request.Name),
                    Description = request.Description,
                    ImageUrl = request.ImageUrl,
                    Icon = request.Icon,
                    ParentCategoryId = request.ParentCategoryId,
                    DisplayOrder = request.DisplayOrder,
                    IsActive = request.IsActive,
                    ShowOnHomePage = request.ShowOnHomePage,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return await GetCategory(category.Id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update category
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null || category.IsDeleted)
                {
                    return NotFound(ApiResponse<CategoryDto>.ErrorResponse("Category not found"));
                }

                // Validate parent category if changing
                if (request.ParentCategoryId.HasValue && request.ParentCategoryId != category.ParentCategoryId)
                {
                    if (request.ParentCategoryId == id)
                    {
                        return BadRequest(ApiResponse<CategoryDto>.ErrorResponse("Category cannot be its own parent"));
                    }

                    var parentExists = await _context.Categories.AnyAsync(c => c.Id == request.ParentCategoryId.Value && !c.IsDeleted);
                    if (!parentExists)
                    {
                        return BadRequest(ApiResponse<CategoryDto>.ErrorResponse("Parent category not found"));
                    }
                }

                // Check duplicate name
                if (request.Name != null && request.Name.ToLower() != category.Name.ToLower())
                {
                    var nameExists = await _context.Categories.AnyAsync(c =>
                        c.Name.ToLower() == request.Name.ToLower() && c.Id != id && !c.IsDeleted);
                    if (nameExists)
                    {
                        return BadRequest(ApiResponse<CategoryDto>.ErrorResponse("Category name already exists"));
                    }
                }

                // Update fields
                if (request.Name != null) category.Name = request.Name;
                if (request.Slug != null) category.Slug = request.Slug;
                if (request.Description != null) category.Description = request.Description;
                if (request.ImageUrl != null) category.ImageUrl = request.ImageUrl;
                if (request.Icon != null) category.Icon = request.Icon;
                if (request.ParentCategoryId.HasValue) category.ParentCategoryId = request.ParentCategoryId;
                if (request.DisplayOrder.HasValue) category.DisplayOrder = request.DisplayOrder.Value;
                if (request.IsActive.HasValue) category.IsActive = request.IsActive.Value;
                if (request.ShowOnHomePage.HasValue) category.ShowOnHomePage = request.ShowOnHomePage.Value;

                category.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return await GetCategory(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Toggle category active status
        /// </summary>
        [HttpPut("{id}/toggle-active")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> ToggleActive(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null || category.IsDeleted)
                {
                    return NotFound(ApiResponse<CategoryDto>.ErrorResponse("Category not found"));
                }

                category.IsActive = !category.IsActive;
                category.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return await GetCategory(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete category (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .Include(c => c.ChildCategories)
                    .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

                if (category == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse("Category not found"));
                }

                // Check if category has active products
                if (category.Products?.Any(p => !p.IsDeleted) == true)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot delete category with active products"));
                }

                // Check if category has child categories
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
