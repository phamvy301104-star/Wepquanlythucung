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
    /// Admin Product Management Controller
    /// Quản lý sản phẩm: CRUD, stock, status
    /// </summary>
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ApplicationDbContext context, ILogger<ProductController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách sản phẩm với filter và phân trang
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<AdminApiResponse<PagedResult<ProductListDto>>>> GetProducts(
            [FromQuery] ProductFilterRequest filter)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .AsQueryable();

                // Filter by category
                if (filter.CategoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == filter.CategoryId);
                }

                // Filter by brand
                if (filter.BrandId.HasValue)
                {
                    query = query.Where(p => p.BrandId == filter.BrandId);
                }

                // Filter by status
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    var isActive = filter.Status.ToLower() == "active";
                    query = query.Where(p => p.IsActive == isActive);
                }

                // Filter low stock
                if (filter.LowStock == true)
                {
                    query = query.Where(p => p.StockQuantity <= 10);
                }

                // Search by name, SKU
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    var search = filter.SearchTerm.ToLower();
                    query = query.Where(p =>
                        p.Name.ToLower().Contains(search) ||
                        (p.SKU != null && p.SKU.ToLower().Contains(search)));
                }

                var totalCount = await query.CountAsync();

                // Sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "name" => filter.SortDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                    "price" => filter.SortDesc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                    "stock" => filter.SortDesc ? query.OrderByDescending(p => p.StockQuantity) : query.OrderBy(p => p.StockQuantity),
                    "sold" => filter.SortDesc ? query.OrderByDescending(p => p.SoldCount) : query.OrderBy(p => p.SoldCount),
                    _ => filter.SortDesc ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt)
                };

                var products = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(p => new ProductListDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        SKU = p.SKU ?? "",
                        ImageUrl = p.ImageUrl,
                        Price = p.Price,
                        OriginalPrice = p.OriginalPrice,
                        StockQuantity = p.StockQuantity,
                        SoldQuantity = p.SoldCount,
                        CategoryName = p.Category != null ? p.Category.Name : null,
                        BrandName = p.Brand != null ? p.Brand.Name : null,
                        IsActive = p.IsActive,
                        IsFeatured = p.IsFeatured,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();

                var result = new PagedResult<ProductListDto>
                {
                    Items = products,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

                return Ok(AdminApiResponse<PagedResult<ProductListDto>>.Ok(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, AdminApiResponse<PagedResult<ProductListDto>>.Fail("Lỗi server khi lấy danh sách sản phẩm"));
            }
        }

        /// <summary>
        /// Lấy chi tiết sản phẩm
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminApiResponse<object>>> GetProductDetail(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                    return NotFound(AdminApiResponse<object>.Fail("Sản phẩm không tồn tại"));

                var dto = new
                {
                    product.Id,
                    product.Name,
                    product.Slug,
                    product.Description,
                    product.ShortDescription,
                    product.SKU,
                    product.Price,
                    product.OriginalPrice,
                    DiscountPercent = product.OriginalPrice > 0 
                        ? Math.Round((1 - product.Price / product.OriginalPrice) * 100, 0) 
                        : 0,
                    product.StockQuantity,
                    SoldQuantity = product.SoldCount,
                    ViewCount = product.ViewCount,
                    product.ImageUrl,
                    Images = product.ProductImages?.Select(i => new
                    {
                        i.Id,
                        i.ImageUrl,
                        i.DisplayOrder,
                        i.AltText
                    }).ToList(),
                    product.CategoryId,
                    CategoryName = product.Category?.Name,
                    product.BrandId,
                    BrandName = product.Brand?.Name,
                    product.IsActive,
                    product.IsFeatured,
                    product.MetaTitle,
                    product.MetaDescription,
                    product.MetaKeywords,
                    product.Weight,
                    product.Unit,
                    product.CreatedAt,
                    product.UpdatedAt
                };

                return Ok(AdminApiResponse<object>.Ok(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product detail for {ProductId}", id);
                return StatusCode(500, AdminApiResponse<object>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Tạo sản phẩm mới
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<int>>> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                // Validate category
                if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId))
                    return BadRequest(AdminApiResponse<int>.Fail("Danh mục không tồn tại"));

                // Validate brand if provided
                if (request.BrandId.HasValue && !await _context.Brands.AnyAsync(b => b.Id == request.BrandId))
                    return BadRequest(AdminApiResponse<int>.Fail("Thương hiệu không tồn tại"));

                // Generate slug
                var slug = GenerateSlug(request.Name);
                var existingSlug = await _context.Products.AnyAsync(p => p.Slug == slug);
                if (existingSlug)
                    slug = $"{slug}-{DateTime.Now.Ticks}";

                var product = new Product
                {
                    Name = request.Name,
                    Slug = slug,
                    Description = request.Description,
                    ShortDescription = request.ShortDescription,
                    SKU = request.SKU ?? "",
                    Price = request.Price,
                    OriginalPrice = request.OriginalPrice ?? 0,
                    StockQuantity = request.StockQuantity,
                    SoldCount = 0,
                    ViewCount = 0,
                    ImageUrl = request.ImageUrl,
                    CategoryId = request.CategoryId,
                    BrandId = request.BrandId,
                    IsActive = request.IsActive,
                    IsFeatured = request.IsFeatured,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Add product images
                if (request.ImageUrls != null && request.ImageUrls.Count > 0)
                {
                    var displayOrder = 0;
                    foreach (var url in request.ImageUrls)
                    {
                        _context.ProductImages.Add(new ProductImage
                        {
                            ProductId = product.Id,
                            ImageUrl = url,
                            DisplayOrder = displayOrder++,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Product created: {ProductId} - {ProductName}", product.Id, product.Name);

                return Ok(AdminApiResponse<int>.Ok(product.Id, "Tạo sản phẩm thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, AdminApiResponse<int>.Fail("Lỗi server khi tạo sản phẩm"));
            }
        }

        /// <summary>
        /// Cập nhật sản phẩm
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<bool>>> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Sản phẩm không tồn tại"));

                // Validate category
                if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId))
                    return BadRequest(AdminApiResponse<bool>.Fail("Danh mục không tồn tại"));

                // Validate brand
                if (request.BrandId.HasValue && !await _context.Brands.AnyAsync(b => b.Id == request.BrandId))
                    return BadRequest(AdminApiResponse<bool>.Fail("Thương hiệu không tồn tại"));

                // Update slug if name changed
                if (product.Name != request.Name)
                {
                    var slug = GenerateSlug(request.Name);
                    var existingSlug = await _context.Products.AnyAsync(p => p.Slug == slug && p.Id != id);
                    product.Slug = existingSlug ? $"{slug}-{DateTime.Now.Ticks}" : slug;
                }

                product.Name = request.Name;
                product.Description = request.Description;
                product.ShortDescription = request.ShortDescription;
                product.SKU = request.SKU ?? "";
                product.Price = request.Price;
                product.OriginalPrice = request.OriginalPrice ?? 0;
                product.StockQuantity = request.StockQuantity;
                product.ImageUrl = request.ImageUrl;
                product.CategoryId = request.CategoryId;
                product.BrandId = request.BrandId;
                product.IsActive = request.IsActive;
                product.IsFeatured = request.IsFeatured;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Product updated: {ProductId}", id);

                return Ok(AdminApiResponse<bool>.Ok(true, "Cập nhật sản phẩm thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server khi cập nhật sản phẩm"));
            }
        }

        /// <summary>
        /// Xóa sản phẩm (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminApiResponse<bool>>> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Sản phẩm không tồn tại"));

                // Check if product has orders
                var hasOrders = await _context.OrderItems.AnyAsync(oi => oi.ProductId == id);
                if (hasOrders)
                {
                    // Soft delete - just deactivate
                    product.IsActive = false;
                    product.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return Ok(AdminApiResponse<bool>.Ok(true, "Sản phẩm đã được vô hiệu hóa (có đơn hàng liên quan)"));
                }

                // Hard delete
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Product deleted: {ProductId}", id);

                return Ok(AdminApiResponse<bool>.Ok(true, "Xóa sản phẩm thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server khi xóa sản phẩm"));
            }
        }

        /// <summary>
        /// Cập nhật số lượng tồn kho
        /// </summary>
        [HttpPatch("{id}/stock")]
        public async Task<ActionResult<AdminApiResponse<bool>>> UpdateStock(int id, [FromBody] UpdateStockRequest request)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Sản phẩm không tồn tại"));

                if (request.Quantity < 0)
                    return BadRequest(AdminApiResponse<bool>.Fail("Số lượng không được âm"));

                product.StockQuantity = request.Quantity;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Product {ProductId} stock updated to {Stock}", id, request.Quantity);

                return Ok(AdminApiResponse<bool>.Ok(true, "Cập nhật tồn kho thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for product {ProductId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server"));
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
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Sản phẩm không tồn tại"));

                product.IsActive = !product.IsActive;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var status = product.IsActive ? "kích hoạt" : "vô hiệu hóa";
                return Ok(AdminApiResponse<bool>.Ok(true, $"Đã {status} sản phẩm"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling product active state for {ProductId}", id);
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
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return NotFound(AdminApiResponse<bool>.Fail("Sản phẩm không tồn tại"));

                product.IsFeatured = !product.IsFeatured;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var status = product.IsFeatured ? "đánh dấu nổi bật" : "bỏ đánh dấu nổi bật";
                return Ok(AdminApiResponse<bool>.Ok(true, $"Đã {status} sản phẩm"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling product featured state for {ProductId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Lấy thống kê sản phẩm
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<AdminApiResponse<object>>> GetProductStats()
        {
            try
            {
                var stats = new
                {
                    TotalProducts = await _context.Products.CountAsync(),
                    ActiveProducts = await _context.Products.CountAsync(p => p.IsActive),
                    InactiveProducts = await _context.Products.CountAsync(p => !p.IsActive),
                    FeaturedProducts = await _context.Products.CountAsync(p => p.IsFeatured),
                    LowStockProducts = await _context.Products.CountAsync(p => p.StockQuantity <= 10 && p.IsActive),
                    OutOfStockProducts = await _context.Products.CountAsync(p => p.StockQuantity == 0 && p.IsActive),
                    TotalStockValue = await _context.Products.SumAsync(p => p.StockQuantity * p.Price),
                    TotalSoldValue = await _context.Products.SumAsync(p => p.SoldCount * p.Price),
                    CategoriesCount = await _context.Categories.CountAsync(),
                    BrandsCount = await _context.Brands.CountAsync()
                };

                return Ok(AdminApiResponse<object>.Ok(stats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product stats");
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
                var categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .Select(c => new { c.Id, c.Name, c.ParentCategoryId })
                    .ToListAsync();

                return Ok(AdminApiResponse<List<object>>.Ok(categories.Cast<object>().ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return StatusCode(500, AdminApiResponse<List<object>>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Lấy danh sách brands cho dropdown
        /// </summary>
        [HttpGet("brands")]
        public async Task<ActionResult<AdminApiResponse<List<object>>>> GetBrands()
        {
            try
            {
                var brands = await _context.Brands
                    .Where(b => b.IsActive)
                    .OrderBy(b => b.Name)
                    .Select(b => new { b.Id, b.Name })
                    .ToListAsync();

                return Ok(AdminApiResponse<List<object>>.Ok(brands.Cast<object>().ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brands");
                return StatusCode(500, AdminApiResponse<List<object>>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Export danh sách sản phẩm
        /// </summary>
        [HttpGet("export")]
        public async Task<IActionResult> ExportProducts()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(10000)
                    .ToListAsync();

                var csv = new StringBuilder();
                csv.AppendLine("ID,Tên,SKU,Danh mục,Thương hiệu,Giá bán,Giá gốc,Tồn kho,Đã bán,Active,Featured,Ngày tạo");

                foreach (var p in products)
                {
                    csv.AppendLine($"{p.Id}," +
                        $"\"{p.Name.Replace("\"", "\"\"")}\"," +
                        $"\"{p.SKU}\"," +
                        $"\"{p.Category?.Name}\"," +
                        $"\"{p.Brand?.Name}\"," +
                        $"{p.Price}," +
                        $"{p.OriginalPrice}," +
                        $"{p.StockQuantity}," +
                        $"{p.SoldCount}," +
                        $"{p.IsActive}," +
                        $"{p.IsFeatured}," +
                        $"\"{p.CreatedAt:yyyy-MM-dd}\"");
                }

                var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
                return File(bytes, "text/csv; charset=utf-8", $"products_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting products");
                return StatusCode(500, "Lỗi server khi export sản phẩm");
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
}
