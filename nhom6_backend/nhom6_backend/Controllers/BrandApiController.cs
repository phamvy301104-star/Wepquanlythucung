using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_backend.Models;

namespace nhom6_backend.Controllers
{
    /// <summary>
    /// Public API Controller for Brands - No authentication required
    /// Used by mobile app to display brand list and filter products
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BrandApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BrandApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all active brands
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBrands()
        {
            try
            {
                var brands = await _context.Brands
                    .Where(b => b.IsActive && !b.IsDeleted)
                    .OrderBy(b => b.DisplayOrder)
                    .Select(b => new
                    {
                        b.Id,
                        b.Name,
                        b.Slug,
                        b.Description,
                        b.LogoUrl,
                        b.BannerUrl,
                        b.WebsiteUrl,
                        b.CountryOfOrigin,
                        b.IsFeatured,
                        ProductCount = _context.Products.Count(p => p.BrandId == b.Id && p.IsActive && !p.IsDeleted)
                    })
                    .ToListAsync();

                return Ok(brands);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get featured brands
        /// </summary>
        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedBrands()
        {
            try
            {
                var brands = await _context.Brands
                    .Where(b => b.IsActive && !b.IsDeleted && b.IsFeatured)
                    .OrderBy(b => b.DisplayOrder)
                    .Select(b => new
                    {
                        b.Id,
                        b.Name,
                        b.Slug,
                        b.LogoUrl,
                        b.CountryOfOrigin,
                        ProductCount = _context.Products.Count(p => p.BrandId == b.Id && p.IsActive && !p.IsDeleted)
                    })
                    .ToListAsync();

                return Ok(brands);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get brand by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBrandById(int id)
        {
            try
            {
                var brand = await _context.Brands
                    .Where(b => b.Id == id && b.IsActive && !b.IsDeleted)
                    .Select(b => new
                    {
                        b.Id,
                        b.Name,
                        b.Slug,
                        b.Description,
                        b.LogoUrl,
                        b.BannerUrl,
                        b.WebsiteUrl,
                        b.CountryOfOrigin,
                        b.YearEstablished,
                        b.IsFeatured,
                        ProductCount = _context.Products.Count(p => p.BrandId == b.Id && p.IsActive && !p.IsDeleted)
                    })
                    .FirstOrDefaultAsync();

                if (brand == null)
                    return NotFound(new { message = "Brand not found" });

                return Ok(brand);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get products by brand ID
        /// </summary>
        [HttpGet("{id}/products")]
        public async Task<IActionResult> GetProductsByBrand(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var brand = await _context.Brands
                    .FirstOrDefaultAsync(b => b.Id == id && b.IsActive && !b.IsDeleted);

                if (brand == null)
                    return NotFound(new { message = "Brand not found" });

                var query = _context.Products
                    .Where(p => p.BrandId == id && p.IsActive && !p.IsDeleted);

                var totalCount = await query.CountAsync();

                var products = await query
                    .OrderByDescending(p => p.IsFeatured)
                    .ThenByDescending(p => p.SoldCount)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        p.Id,
                        p.SKU,
                        p.Name,
                        p.Slug,
                        p.ShortDescription,
                        p.ImageUrl,
                        p.OriginalPrice,
                        p.Price,
                        p.DiscountPercent,
                        p.StockQuantity,
                        p.AverageRating,
                        p.TotalReviews,
                        p.SoldCount,
                        p.IsFeatured,
                        p.IsBestSeller,
                        p.IsNew,
                        p.IsOnSale,
                        BrandName = brand.Name
                    })
                    .ToListAsync();

                return Ok(new
                {
                    items = products,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}
