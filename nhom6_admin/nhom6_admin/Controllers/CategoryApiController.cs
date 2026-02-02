using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Models;
using nhom6_admin.Models.Entities;

namespace nhom6_admin.Controllers
{
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
                    .Where(c => !c.IsDeleted && c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
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
                        c.ProductCount
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
        /// Get categories for homepage (showOnHomePage = true)
        /// </summary>
        [HttpGet("home")]
        public async Task<IActionResult> GetHomeCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Where(c => !c.IsDeleted && c.IsActive && c.ShowOnHomePage)
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Slug,
                        c.Description,
                        c.ImageUrl,
                        c.Icon,
                        c.DisplayOrder,
                        c.ProductCount
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
                    .Where(c => c.Id == id && !c.IsDeleted && c.IsActive)
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
                        c.ProductCount,
                        c.MetaTitle,
                        c.MetaDescription,
                        c.MetaKeywords
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
        public async Task<IActionResult> GetProductsByCategory(
            int id, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                // Verify category exists
                var categoryExists = await _context.Categories
                    .AnyAsync(c => c.Id == id && !c.IsDeleted && c.IsActive);

                if (!categoryExists)
                    return NotFound(new { message = "Category not found" });

                // Get products
                var query = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Where(p => p.CategoryId == id && !p.IsDeleted && p.IsActive);

                var totalCount = await query.CountAsync();

                var products = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.SKU,
                        p.ShortDescription,
                        p.Description,
                        p.ImageUrl,
                        p.AdditionalImages,
                        p.Price,
                        p.OriginalPrice,
                        p.DiscountPercent,
                        p.StockQuantity,
                        CategoryName = p.Category != null ? p.Category.Name : null,
                        BrandName = p.Brand != null ? p.Brand.Name : null,
                        p.AverageRating,
                        p.SoldCount,
                        p.Unit,
                        p.Weight,
                        p.Volume,
                        p.Ingredients,
                        p.Usage
                    })
                    .ToListAsync();

                return Ok(new
                {
                    items = products,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}
