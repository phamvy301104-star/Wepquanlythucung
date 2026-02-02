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
    public class BrandController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BrandController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all brands
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<BrandDto>>>> GetBrands([FromQuery] bool? isActive = null)
        {
            try
            {
                var query = _context.Brands
                    .Include(b => b.Products)
                    .Where(b => !b.IsDeleted)
                    .AsQueryable();

                if (isActive.HasValue)
                {
                    query = query.Where(b => b.IsActive == isActive.Value);
                }

                var brands = await query
                    .OrderBy(b => b.DisplayOrder)
                    .Select(b => new BrandDto
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
                        ProductCount = b.Products != null ? b.Products.Count(p => !p.IsDeleted) : 0
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<BrandDto>>.SuccessResponse(brands));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<BrandDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get brand by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<BrandDto>>> GetBrand(int id)
        {
            try
            {
                var brand = await _context.Brands
                    .Include(b => b.Products)
                    .Where(b => b.Id == id && !b.IsDeleted)
                    .FirstOrDefaultAsync();

                if (brand == null)
                {
                    return NotFound(ApiResponse<BrandDto>.ErrorResponse("Brand not found"));
                }

                var brandDto = new BrandDto
                {
                    Id = brand.Id,
                    Name = brand.Name,
                    Slug = brand.Slug,
                    Description = brand.Description,
                    LogoUrl = brand.LogoUrl,
                    CountryOfOrigin = brand.CountryOfOrigin,
                    DisplayOrder = brand.DisplayOrder,
                    IsActive = brand.IsActive,
                    IsFeatured = brand.IsFeatured,
                    ProductCount = brand.Products?.Count(p => !p.IsDeleted) ?? 0
                };

                return Ok(ApiResponse<BrandDto>.SuccessResponse(brandDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BrandDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create new brand
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<BrandDto>>> CreateBrand([FromBody] CreateBrandRequest request)
        {
            try
            {
                // Check duplicate name
                var nameExists = await _context.Brands.AnyAsync(b =>
                    b.Name.ToLower() == request.Name.ToLower() && !b.IsDeleted);
                if (nameExists)
                {
                    return BadRequest(ApiResponse<BrandDto>.ErrorResponse("Brand name already exists"));
                }

                var brand = new Brand
                {
                    Name = request.Name,
                    Slug = request.Slug ?? GenerateSlug(request.Name),
                    Description = request.Description,
                    LogoUrl = request.LogoUrl,
                    BannerUrl = request.BannerUrl,
                    WebsiteUrl = request.WebsiteUrl,
                    CountryOfOrigin = request.CountryOfOrigin,
                    YearEstablished = request.YearEstablished,
                    DisplayOrder = request.DisplayOrder,
                    IsActive = request.IsActive,
                    IsFeatured = request.IsFeatured,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Brands.Add(brand);
                await _context.SaveChangesAsync();

                return await GetBrand(brand.Id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BrandDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update brand
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<BrandDto>>> UpdateBrand(int id, [FromBody] UpdateBrandRequest request)
        {
            try
            {
                var brand = await _context.Brands.FindAsync(id);
                if (brand == null || brand.IsDeleted)
                {
                    return NotFound(ApiResponse<BrandDto>.ErrorResponse("Brand not found"));
                }

                // Check duplicate name
                if (request.Name != null && request.Name.ToLower() != brand.Name.ToLower())
                {
                    var nameExists = await _context.Brands.AnyAsync(b =>
                        b.Name.ToLower() == request.Name.ToLower() && b.Id != id && !b.IsDeleted);
                    if (nameExists)
                    {
                        return BadRequest(ApiResponse<BrandDto>.ErrorResponse("Brand name already exists"));
                    }
                }

                // Update fields
                if (request.Name != null) brand.Name = request.Name;
                if (request.Slug != null) brand.Slug = request.Slug;
                if (request.Description != null) brand.Description = request.Description;
                if (request.LogoUrl != null) brand.LogoUrl = request.LogoUrl;
                if (request.BannerUrl != null) brand.BannerUrl = request.BannerUrl;
                if (request.WebsiteUrl != null) brand.WebsiteUrl = request.WebsiteUrl;
                if (request.CountryOfOrigin != null) brand.CountryOfOrigin = request.CountryOfOrigin;
                if (request.YearEstablished.HasValue) brand.YearEstablished = request.YearEstablished;
                if (request.DisplayOrder.HasValue) brand.DisplayOrder = request.DisplayOrder.Value;
                if (request.IsActive.HasValue) brand.IsActive = request.IsActive.Value;
                if (request.IsFeatured.HasValue) brand.IsFeatured = request.IsFeatured.Value;

                brand.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return await GetBrand(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BrandDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Toggle brand active status
        /// </summary>
        [HttpPut("{id}/toggle-active")]
        public async Task<ActionResult<ApiResponse<BrandDto>>> ToggleActive(int id)
        {
            try
            {
                var brand = await _context.Brands.FindAsync(id);
                if (brand == null || brand.IsDeleted)
                {
                    return NotFound(ApiResponse<BrandDto>.ErrorResponse("Brand not found"));
                }

                brand.IsActive = !brand.IsActive;
                brand.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return await GetBrand(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BrandDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Toggle brand featured status
        /// </summary>
        [HttpPut("{id}/toggle-featured")]
        public async Task<ActionResult<ApiResponse<BrandDto>>> ToggleFeatured(int id)
        {
            try
            {
                var brand = await _context.Brands.FindAsync(id);
                if (brand == null || brand.IsDeleted)
                {
                    return NotFound(ApiResponse<BrandDto>.ErrorResponse("Brand not found"));
                }

                brand.IsFeatured = !brand.IsFeatured;
                brand.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return await GetBrand(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BrandDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete brand (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteBrand(int id)
        {
            try
            {
                var brand = await _context.Brands
                    .Include(b => b.Products)
                    .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

                if (brand == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse("Brand not found"));
                }

                // Check if brand has active products
                if (brand.Products?.Any(p => !p.IsDeleted) == true)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot delete brand with active products"));
                }

                brand.IsDeleted = true;
                brand.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Brand deleted successfully"));
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
