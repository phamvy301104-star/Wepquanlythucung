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
    /// Admin Category Management Controller
    /// Quản lý danh mục sản phẩm: CRUD đầy đủ
    /// </summary>
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ApplicationDbContext context, ILogger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách danh mục với filter và phân trang
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<AdminApiResponse<PagedResult<CategoryListDto>>>> GetCategories(
            [FromQuery] CategoryFilterRequest filter)
        {
            try
            {
                var query = _context.Categories
                    .Include(c => c.ParentCategory)
                    .AsQueryable();

                // Filter by parent category
                if (filter.ParentCategoryId.HasValue)
                {
                    query = query.Where(c => c.ParentCategoryId == filter.ParentCategoryId);
                }

                // Filter by status
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    var isActive = filter.Status.ToLower() == "active";
                    query = query.Where(c => c.IsActive == isActive);
                }

                // Filter by show on home page
                if (filter.ShowOnHomePage.HasValue)
                {
                    query = query.Where(c => c.ShowOnHomePage == filter.ShowOnHomePage.Value);
                }

                // Search by name
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    var search = filter.SearchTerm.ToLower();
                    query = query.Where(c => c.Name.ToLower().Contains(search) ||
                                            (c.Description != null && c.Description.ToLower().Contains(search)));
                }

                var totalCount = await query.CountAsync();

                // Sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "name" => filter.SortDesc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                    "productcount" => filter.SortDesc ? query.OrderByDescending(c => c.ProductCount) : query.OrderBy(c => c.ProductCount),
                    "createdat" => filter.SortDesc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
                    _ => filter.SortDesc ? query.OrderByDescending(c => c.DisplayOrder) : query.OrderBy(c => c.DisplayOrder)
                };

                var categories = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(c => new CategoryListDto
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
                        ShowOnHomePage = c.ShowOnHomePage,
                        ProductCount = c.ProductCount,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();

                var result = new PagedResult<CategoryListDto>
                {
                    Items = categories,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

                return Ok(AdminApiResponse<PagedResult<CategoryListDto>>.Ok(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return StatusCode(500, AdminApiResponse<PagedResult<CategoryListDto>>.Fail("Lỗi server khi lấy danh sách danh mục"));
            }
        }

        /// <summary>
        /// Lấy chi tiết danh mục
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminApiResponse<CategoryDetailDto>>> GetCategoryDetail(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.ParentCategory)
                    .Include(c => c.ChildCategories)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                    return NotFound(AdminApiResponse<CategoryDetailDto>.Fail("Danh mục không tồn tại"));

                var dto = new CategoryDetailDto
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
                    ShowOnHomePage = category.ShowOnHomePage,
                    ProductCount = category.ProductCount,
                    MetaTitle = category.MetaTitle,
                    MetaDescription = category.MetaDescription,
                    MetaKeywords = category.MetaKeywords,
                    CreatedAt = category.CreatedAt,
                    UpdatedAt = category.UpdatedAt,
                    ChildCategories = category.ChildCategories?.Select(c => new CategoryListDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Slug = c.Slug,
                        ImageUrl = c.ImageUrl,
                        IsActive = c.IsActive,
                        ProductCount = c.ProductCount
                    }).ToList()
                };

                return Ok(AdminApiResponse<CategoryDetailDto>.Ok(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category detail for {CategoryId}", id);
                return StatusCode(500, AdminApiResponse<CategoryDetailDto>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Tạo danh mục mới
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<int>>> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            try
            {
                // Validate parent category
                if (request.ParentCategoryId.HasValue)
                {
                    var parentExists = await _context.Categories.AnyAsync(c => c.Id == request.ParentCategoryId);
                    if (!parentExists)
                        return BadRequest(AdminApiResponse<int>.Fail("Danh mục cha không tồn tại"));
                }

                // Generate slug
                var slug = GenerateSlug(request.Name);
                var existingSlug = await _context.Categories.AnyAsync(c => c.Slug == slug);
                if (existingSlug)
                    slug = $"{slug}-{DateTime.Now.Ticks}";

                var category = new Category
                {
                    Name = request.Name,
                    Slug = slug,
                    Description = request.Description,
                    ImageUrl = request.ImageUrl,
                    Icon = request.Icon,
                    ParentCategoryId = request.ParentCategoryId,
                    DisplayOrder = request.DisplayOrder,
                    IsActive = request.IsActive,
                    ShowOnHomePage = request.ShowOnHomePage,
                    MetaTitle = request.MetaTitle,
                    MetaDescription = request.MetaDescription,
                    MetaKeywords = request.MetaKeywords,
                    ProductCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Category created: {CategoryId} - {CategoryName}", category.Id, category.Name);

                return Ok(AdminApiResponse<int>.Ok(category.Id, "Tạo danh mục thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, AdminApiResponse<int>.Fail("Lỗi server khi tạo danh mục"));
            }
        }

        /// <summary>
        /// Cập nhật danh mục
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<bool>>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Danh mục không tồn tại"));

                // Validate parent category (không cho phép tự làm parent của chính mình)
                if (request.ParentCategoryId.HasValue)
                {
                    if (request.ParentCategoryId == id)
                        return BadRequest(AdminApiResponse<bool>.Fail("Danh mục không thể là cha của chính nó"));

                    var parentExists = await _context.Categories.AnyAsync(c => c.Id == request.ParentCategoryId);
                    if (!parentExists)
                        return BadRequest(AdminApiResponse<bool>.Fail("Danh mục cha không tồn tại"));
                }

                // Update slug if name changed
                if (category.Name != request.Name)
                {
                    var slug = GenerateSlug(request.Name);
                    var existingSlug = await _context.Categories.AnyAsync(c => c.Slug == slug && c.Id != id);
                    category.Slug = existingSlug ? $"{slug}-{DateTime.Now.Ticks}" : slug;
                }

                category.Name = request.Name;
                category.Description = request.Description;
                category.ImageUrl = request.ImageUrl;
                category.Icon = request.Icon;
                category.ParentCategoryId = request.ParentCategoryId;
                category.DisplayOrder = request.DisplayOrder;
                category.IsActive = request.IsActive;
                category.ShowOnHomePage = request.ShowOnHomePage;
                category.MetaTitle = request.MetaTitle;
                category.MetaDescription = request.MetaDescription;
                category.MetaKeywords = request.MetaKeywords;
                category.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Category updated: {CategoryId}", id);

                return Ok(AdminApiResponse<bool>.Ok(true, "Cập nhật danh mục thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server khi cập nhật danh mục"));
            }
        }

        /// <summary>
        /// Xóa danh mục
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<bool>>> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.ChildCategories)
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Danh mục không tồn tại"));

                // Check if has child categories
                if (category.ChildCategories != null && category.ChildCategories.Any())
                    return BadRequest(AdminApiResponse<bool>.Fail("Không thể xóa danh mục có danh mục con. Vui lòng xóa các danh mục con trước."));

                // Check if has products
                if (category.Products != null && category.Products.Any())
                {
                    // Soft delete - deactivate
                    category.IsActive = false;
                    category.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return Ok(AdminApiResponse<bool>.Ok(true, "Danh mục đã được vô hiệu hóa (có sản phẩm liên quan)"));
                }

                // Hard delete
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Category deleted: {CategoryId}", id);

                return Ok(AdminApiResponse<bool>.Ok(true, "Xóa danh mục thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server khi xóa danh mục"));
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
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Danh mục không tồn tại"));

                category.IsActive = !category.IsActive;
                category.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var status = category.IsActive ? "kích hoạt" : "vô hiệu hóa";
                return Ok(AdminApiResponse<bool>.Ok(true, $"Đã {status} danh mục"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category active state for {CategoryId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Lấy danh sách danh mục cho dropdown (không phân trang)
        /// </summary>
        [HttpGet("dropdown")]
        [AllowAnonymous]
        public async Task<ActionResult<AdminApiResponse<List<object>>>> GetCategoriesForDropdown()
        {
            try
            {
                var categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.ParentCategoryId,
                        c.Icon
                    })
                    .ToListAsync();

                return Ok(AdminApiResponse<List<object>>.Ok(categories.Cast<object>().ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories for dropdown");
                return StatusCode(500, AdminApiResponse<List<object>>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Generate URL-friendly slug từ tên
        /// </summary>
        private static string GenerateSlug(string name)
        {
            // Chuyển sang chữ thường
            var slug = name.ToLower();

            // Chuyển đổi tiếng Việt có dấu sang không dấu
            var vietnameseChars = new Dictionary<char, string>
            {
                {'à', "a"}, {'á', "a"}, {'ả', "a"}, {'ã', "a"}, {'ạ', "a"},
                {'ă', "a"}, {'ằ', "a"}, {'ắ', "a"}, {'ẳ', "a"}, {'ẵ', "a"}, {'ặ', "a"},
                {'â', "a"}, {'ầ', "a"}, {'ấ', "a"}, {'ẩ', "a"}, {'ẫ', "a"}, {'ậ', "a"},
                {'đ', "d"},
                {'è', "e"}, {'é', "e"}, {'ẻ', "e"}, {'ẽ', "e"}, {'ẹ', "e"},
                {'ê', "e"}, {'ề', "e"}, {'ế', "e"}, {'ể', "e"}, {'ễ', "e"}, {'ệ', "e"},
                {'ì', "i"}, {'í', "i"}, {'ỉ', "i"}, {'ĩ', "i"}, {'ị', "i"},
                {'ò', "o"}, {'ó', "o"}, {'ỏ', "o"}, {'õ', "o"}, {'ọ', "o"},
                {'ô', "o"}, {'ồ', "o"}, {'ố', "o"}, {'ổ', "o"}, {'ỗ', "o"}, {'ộ', "o"},
                {'ơ', "o"}, {'ờ', "o"}, {'ớ', "o"}, {'ở', "o"}, {'ỡ', "o"}, {'ợ', "o"},
                {'ù', "u"}, {'ú', "u"}, {'ủ', "u"}, {'ũ', "u"}, {'ụ', "u"},
                {'ư', "u"}, {'ừ', "u"}, {'ứ', "u"}, {'ử', "u"}, {'ữ', "u"}, {'ự', "u"},
                {'ỳ', "y"}, {'ý', "y"}, {'ỷ', "y"}, {'ỹ', "y"}, {'ỵ', "y"}
            };

            var sb = new StringBuilder();
            foreach (var c in slug)
            {
                if (vietnameseChars.TryGetValue(c, out var replacement))
                    sb.Append(replacement);
                else if (char.IsLetterOrDigit(c))
                    sb.Append(c);
                else if (c == ' ')
                    sb.Append('-');
            }

            // Loại bỏ các dấu gạch ngang liên tiếp
            slug = sb.ToString();
            while (slug.Contains("--"))
                slug = slug.Replace("--", "-");

            return slug.Trim('-');
        }
    }
}
