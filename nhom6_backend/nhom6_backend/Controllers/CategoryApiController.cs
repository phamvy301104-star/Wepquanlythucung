using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_backend.Models;

namespace nhom6_backend.Controllers
{
    /// <summary>
    /// Public API Controller for Categories - No authentication required
    /// Used by mobile app to display category list
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all active categories
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Where(c => c.IsActive && !c.IsDeleted)
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Slug,
                        c.Description,
                        c.ImageUrl,
                        c.Icon,
                        c.ParentCategoryId,
                        c.DisplayOrder,
                        c.ShowOnHomePage,
                        ProductCount = _context.Products.Count(p => p.CategoryId == c.Id && p.IsActive && !p.IsDeleted)
                    })
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get categories for homepage (ShowOnHomePage = true)
        /// </summary>
        [HttpGet("home")]
        public async Task<IActionResult> GetHomeCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Where(c => c.IsActive && !c.IsDeleted && c.ShowOnHomePage)
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Slug,
                        c.ImageUrl,
                        c.Icon,
                        ProductCount = _context.Products.Count(p => p.CategoryId == c.Id && p.IsActive && !p.IsDeleted)
                    })
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Where(c => c.Id == id && c.IsActive && !c.IsDeleted)
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Slug,
                        c.Description,
                        c.ImageUrl,
                        c.Icon,
                        c.ParentCategoryId,
                        c.DisplayOrder,
                        c.ShowOnHomePage,
                        ProductCount = _context.Products.Count(p => p.CategoryId == c.Id && p.IsActive && !p.IsDeleted)
                    })
                    .FirstOrDefaultAsync();

                if (category == null)
                    return NotFound(new { message = "Category not found" });

                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get products by category ID
        /// </summary>
        [HttpGet("{id}/products")]
        public async Task<IActionResult> GetProductsByCategory(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == id && c.IsActive && !c.IsDeleted);

                if (category == null)
                    return NotFound(new { message = "Category not found" });

                var query = _context.Products
                    .Where(p => p.CategoryId == id && p.IsActive && !p.IsDeleted);

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
                        CategoryName = category.Name
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
