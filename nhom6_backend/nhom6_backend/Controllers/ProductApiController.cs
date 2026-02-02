using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_backend.Models;
using nhom6_backend.Repositories;

namespace nhom6_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductApiController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;

        public ProductApiController(IProductRepository productRepository, ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDesc = false,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Where(p => !p.IsDeleted && p.IsActive)
                    .AsQueryable();

                // Filter by category
                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == categoryId);
                }

                // Search
                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(p => 
                        p.Name.ToLower().Contains(searchLower) ||
                        (p.Description != null && p.Description.ToLower().Contains(searchLower)));
                }

                // Sorting
                query = sortBy?.ToLower() switch
                {
                    "price" => sortDesc 
                        ? query.OrderByDescending(p => p.Price) 
                        : query.OrderBy(p => p.Price),
                    "name" => sortDesc 
                        ? query.OrderByDescending(p => p.Name) 
                        : query.OrderBy(p => p.Name),
                    "soldcount" => sortDesc 
                        ? query.OrderByDescending(p => p.SoldCount) 
                        : query.OrderBy(p => p.SoldCount),
                    "createdat" => sortDesc 
                        ? query.OrderByDescending(p => p.CreatedAt) 
                        : query.OrderBy(p => p.CreatedAt),
                    _ => query.OrderByDescending(p => p.CreatedAt)
                };

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var products = await query
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
                        Price = p.OriginalPrice,
                        SalePrice = p.Price < p.OriginalPrice ? p.Price : (decimal?)null,
                        p.OriginalPrice,
                        DiscountPercent = p.DiscountPercent,
                        p.StockQuantity,
                        p.SoldCount,
                        p.AverageRating,
                        ReviewCount = p.TotalReviews,
                        p.Unit,
                        p.Weight,
                        p.Volume,
                        p.Ingredients,
                        p.Usage,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category != null ? p.Category.Name : null,
                        BrandId = p.BrandId,
                        BrandName = p.Brand != null ? p.Brand.Name : null,
                        p.IsActive,
                        p.IsFeatured,
                        p.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new 
                {
                    items = products,
                    page,
                    pageSize,
                    totalItems,
                    totalPages
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Where(p => p.Id == id && !p.IsDeleted)
                    .Select(p => new 
                    {
                        p.Id,
                        p.Name,
                        p.SKU,
                        p.Barcode,
                        p.ShortDescription,
                        p.Description,
                        p.ImageUrl,
                        p.AdditionalImages,
                        Price = p.OriginalPrice,
                        SalePrice = p.Price < p.OriginalPrice ? p.Price : (decimal?)null,
                        p.OriginalPrice,
                        DiscountPercent = p.DiscountPercent,
                        p.StockQuantity,
                        p.SoldCount,
                        p.AverageRating,
                        ReviewCount = p.TotalReviews,
                        p.Unit,
                        p.Weight,
                        p.Volume,
                        p.Ingredients,
                        p.Usage,
                        p.Warnings,
                        p.Origin,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category != null ? p.Category.Name : null,
                        BrandId = p.BrandId,
                        BrandName = p.Brand != null ? p.Brand.Name : null,
                        p.IsActive,
                        p.IsFeatured,
                        p.CreatedAt,
                        p.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (product == null)
                    return NotFound(new { message = "Product not found" });
                    
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            try
            {
                await _productRepository.AddProductAsync(product);
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            try
            {
                if (id != product.Id)
                    return BadRequest();
                await _productRepository.UpdateProductAsync(product);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productRepository.DeleteProductAsync(id);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
