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
    /// Admin Brand Management Controller
    /// Quản lý thương hiệu: CRUD đầy đủ
    /// </summary>
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class BrandController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BrandController> _logger;

        public BrandController(ApplicationDbContext context, ILogger<BrandController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách thương hiệu với filter và phân trang
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<AdminApiResponse<PagedResult<BrandListDto>>>> GetBrands(
            [FromQuery] BrandFilterRequest filter)
        {
            try
            {
                var query = _context.Brands.AsQueryable();

                // Filter by status
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    var isActive = filter.Status.ToLower() == "active";
                    query = query.Where(b => b.IsActive == isActive);
                }

                // Filter by featured
                if (filter.IsFeatured.HasValue)
                {
                    query = query.Where(b => b.IsFeatured == filter.IsFeatured.Value);
                }

                // Search by name
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    var search = filter.SearchTerm.ToLower();
                    query = query.Where(b => b.Name.ToLower().Contains(search) ||
                                            (b.Description != null && b.Description.ToLower().Contains(search)) ||
                                            (b.CountryOfOrigin != null && b.CountryOfOrigin.ToLower().Contains(search)));
                }

                var totalCount = await query.CountAsync();

                // Sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "name" => filter.SortDesc ? query.OrderByDescending(b => b.Name) : query.OrderBy(b => b.Name),
                    "productcount" => filter.SortDesc ? query.OrderByDescending(b => b.ProductCount) : query.OrderBy(b => b.ProductCount),
                    "createdat" => filter.SortDesc ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt),
                    _ => filter.SortDesc ? query.OrderByDescending(b => b.DisplayOrder) : query.OrderBy(b => b.DisplayOrder)
                };

                var brands = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(b => new BrandListDto
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Slug = b.Slug,
                        Description = b.Description,
                        LogoUrl = b.LogoUrl,
                        CountryOfOrigin = b.CountryOfOrigin,
                        DisplayOrder = b.DisplayOrder,
                        IsActive = b.IsActive,
                        IsFeatured = b.IsFeatured,
                        ProductCount = b.ProductCount,
                        CreatedAt = b.CreatedAt
                    })
                    .ToListAsync();

                var result = new PagedResult<BrandListDto>
                {
                    Items = brands,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

                return Ok(AdminApiResponse<PagedResult<BrandListDto>>.Ok(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brands");
                return StatusCode(500, AdminApiResponse<PagedResult<BrandListDto>>.Fail("Lỗi server khi lấy danh sách thương hiệu"));
            }
        }

        /// <summary>
        /// Lấy chi tiết thương hiệu
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminApiResponse<BrandDetailDto>>> GetBrandDetail(int id)
        {
            try
            {
                var brand = await _context.Brands.FindAsync(id);

                if (brand == null)
                    return NotFound(AdminApiResponse<BrandDetailDto>.Fail("Thương hiệu không tồn tại"));

                var dto = new BrandDetailDto
                {
                    Id = brand.Id,
                    Name = brand.Name,
                    Slug = brand.Slug,
                    Description = brand.Description,
                    LogoUrl = brand.LogoUrl,
                    BannerUrl = brand.BannerUrl,
                    WebsiteUrl = brand.WebsiteUrl,
                    CountryOfOrigin = brand.CountryOfOrigin,
                    YearEstablished = brand.YearEstablished,
                    DisplayOrder = brand.DisplayOrder,
                    IsActive = brand.IsActive,
                    IsFeatured = brand.IsFeatured,
                    ProductCount = brand.ProductCount,
                    MetaTitle = brand.MetaTitle,
                    MetaDescription = brand.MetaDescription,
                    CreatedAt = brand.CreatedAt,
                    UpdatedAt = brand.UpdatedAt
                };

                return Ok(AdminApiResponse<BrandDetailDto>.Ok(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brand detail for {BrandId}", id);
                return StatusCode(500, AdminApiResponse<BrandDetailDto>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Tạo thương hiệu mới
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<int>>> CreateBrand([FromBody] CreateBrandRequest request)
        {
            try
            {
                // Check duplicate name
                var existingName = await _context.Brands.AnyAsync(b => b.Name.ToLower() == request.Name.ToLower());
                if (existingName)
                    return BadRequest(AdminApiResponse<int>.Fail("Tên thương hiệu đã tồn tại"));

                // Generate slug
                var slug = GenerateSlug(request.Name);
                var existingSlug = await _context.Brands.AnyAsync(b => b.Slug == slug);
                if (existingSlug)
                    slug = $"{slug}-{DateTime.Now.Ticks}";

                var brand = new Brand
                {
                    Name = request.Name,
                    Slug = slug,
                    Description = request.Description,
                    LogoUrl = request.LogoUrl,
                    BannerUrl = request.BannerUrl,
                    WebsiteUrl = request.WebsiteUrl,
                    CountryOfOrigin = request.CountryOfOrigin,
                    YearEstablished = request.YearEstablished,
                    DisplayOrder = request.DisplayOrder,
                    IsActive = request.IsActive,
                    IsFeatured = request.IsFeatured,
                    MetaTitle = request.MetaTitle,
                    MetaDescription = request.MetaDescription,
                    ProductCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Brands.Add(brand);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Brand created: {BrandId} - {BrandName}", brand.Id, brand.Name);

                return Ok(AdminApiResponse<int>.Ok(brand.Id, "Tạo thương hiệu thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating brand");
                return StatusCode(500, AdminApiResponse<int>.Fail("Lỗi server khi tạo thương hiệu"));
            }
        }

        /// <summary>
        /// Cập nhật thương hiệu
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<bool>>> UpdateBrand(int id, [FromBody] UpdateBrandRequest request)
        {
            try
            {
                var brand = await _context.Brands.FindAsync(id);
                if (brand == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Thương hiệu không tồn tại"));

                // Check duplicate name (exclude current)
                var existingName = await _context.Brands.AnyAsync(b => b.Name.ToLower() == request.Name.ToLower() && b.Id != id);
                if (existingName)
                    return BadRequest(AdminApiResponse<bool>.Fail("Tên thương hiệu đã tồn tại"));

                // Update slug if name changed
                if (brand.Name != request.Name)
                {
                    var slug = GenerateSlug(request.Name);
                    var existingSlug = await _context.Brands.AnyAsync(b => b.Slug == slug && b.Id != id);
                    brand.Slug = existingSlug ? $"{slug}-{DateTime.Now.Ticks}" : slug;
                }

                brand.Name = request.Name;
                brand.Description = request.Description;
                brand.LogoUrl = request.LogoUrl;
                brand.BannerUrl = request.BannerUrl;
                brand.WebsiteUrl = request.WebsiteUrl;
                brand.CountryOfOrigin = request.CountryOfOrigin;
                brand.YearEstablished = request.YearEstablished;
                brand.DisplayOrder = request.DisplayOrder;
                brand.IsActive = request.IsActive;
                brand.IsFeatured = request.IsFeatured;
                brand.MetaTitle = request.MetaTitle;
                brand.MetaDescription = request.MetaDescription;
                brand.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Brand updated: {BrandId}", id);

                return Ok(AdminApiResponse<bool>.Ok(true, "Cập nhật thương hiệu thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating brand {BrandId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server khi cập nhật thương hiệu"));
            }
        }

        /// <summary>
        /// Xóa thương hiệu
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<bool>>> DeleteBrand(int id)
        {
            try
            {
                var brand = await _context.Brands
                    .Include(b => b.Products)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (brand == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Thương hiệu không tồn tại"));

                // Check if has products
                if (brand.Products != null && brand.Products.Any())
                {
                    // Soft delete - deactivate
                    brand.IsActive = false;
                    brand.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return Ok(AdminApiResponse<bool>.Ok(true, "Thương hiệu đã được vô hiệu hóa (có sản phẩm liên quan)"));
                }

                // Hard delete
                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Brand deleted: {BrandId}", id);

                return Ok(AdminApiResponse<bool>.Ok(true, "Xóa thương hiệu thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting brand {BrandId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server khi xóa thương hiệu"));
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
                var brand = await _context.Brands.FindAsync(id);
                if (brand == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Thương hiệu không tồn tại"));

                brand.IsActive = !brand.IsActive;
                brand.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var status = brand.IsActive ? "kích hoạt" : "vô hiệu hóa";
                return Ok(AdminApiResponse<bool>.Ok(true, $"Đã {status} thương hiệu"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling brand active state for {BrandId}", id);
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
                var brand = await _context.Brands.FindAsync(id);
                if (brand == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Thương hiệu không tồn tại"));

                brand.IsFeatured = !brand.IsFeatured;
                brand.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var status = brand.IsFeatured ? "đánh dấu nổi bật" : "bỏ đánh dấu nổi bật";
                return Ok(AdminApiResponse<bool>.Ok(true, $"Đã {status} thương hiệu"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling brand featured state for {BrandId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Lấy danh sách thương hiệu cho dropdown (không phân trang)
        /// </summary>
        [HttpGet("dropdown")]
        [AllowAnonymous]
        public async Task<ActionResult<AdminApiResponse<List<object>>>> GetBrandsForDropdown()
        {
            try
            {
                var brands = await _context.Brands
                    .Where(b => b.IsActive)
                    .OrderBy(b => b.DisplayOrder)
                    .ThenBy(b => b.Name)
                    .Select(b => new
                    {
                        b.Id,
                        b.Name,
                        b.LogoUrl,
                        b.CountryOfOrigin
                    })
                    .ToListAsync();

                return Ok(AdminApiResponse<List<object>>.Ok(brands.Cast<object>().ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brands for dropdown");
                return StatusCode(500, AdminApiResponse<List<object>>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Generate URL-friendly slug từ tên
        /// </summary>
        private static string GenerateSlug(string name)
        {
            var slug = name.ToLower();

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

            slug = sb.ToString();
            while (slug.Contains("--"))
                slug = slug.Replace("--", "-");

            return slug.Trim('-');
        }
    }
}
