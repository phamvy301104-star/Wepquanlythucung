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
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all products with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<ProductListDto>>>> GetProducts(
            [FromQuery] ProductFilterRequest filter)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Where(p => !p.IsDeleted)
                    .AsQueryable();

                // Search
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    var search = filter.Search.ToLower();
                    query = query.Where(p =>
                        p.Name.ToLower().Contains(search) ||
                        p.SKU.ToLower().Contains(search) ||
                        (p.Description != null && p.Description.ToLower().Contains(search)));
                }

                // Filter by category
                if (filter.CategoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
                }

                // Filter by brand
                if (filter.BrandId.HasValue)
                {
                    query = query.Where(p => p.BrandId == filter.BrandId.Value);
                }

                // Filter by price
                if (filter.MinPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= filter.MinPrice.Value);
                }
                if (filter.MaxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= filter.MaxPrice.Value);
                }

                // Filter by active status
                if (filter.IsActive.HasValue)
                {
                    query = query.Where(p => p.IsActive == filter.IsActive.Value);
                }

                // Filter by featured
                if (filter.IsFeatured.HasValue)
                {
                    query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);
                }

                // Filter low stock
                if (filter.LowStock == true)
                {
                    query = query.Where(p => p.StockQuantity <= p.LowStockThreshold);
                }

                var totalCount = await query.CountAsync();

                // Sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "name" => filter.SortDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                    "price" => filter.SortDesc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                    "stock" => filter.SortDesc ? query.OrderByDescending(p => p.StockQuantity) : query.OrderBy(p => p.StockQuantity),
                    "sold" => filter.SortDesc ? query.OrderByDescending(p => p.SoldCount) : query.OrderBy(p => p.SoldCount),
                    "createdat" => filter.SortDesc ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                    _ => query.OrderByDescending(p => p.CreatedAt)
                };

                // Pagination
                var products = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(p => new ProductListDto
                    {
                        Id = p.Id,
                        SKU = p.SKU,
                        Name = p.Name,
                        ImageUrl = p.ImageUrl,
                        Price = p.Price,
                        OriginalPrice = p.OriginalPrice,
                        StockQuantity = p.StockQuantity,
                        SoldCount = p.SoldCount,
                        CategoryName = p.Category != null ? p.Category.Name : null,
                        BrandName = p.Brand != null ? p.Brand.Name : null,
                        IsActive = p.IsActive,
                        IsFeatured = p.IsFeatured,
                        AverageRating = p.AverageRating,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();

                var response = new PaginatedResponse<ProductListDto>
                {
                    Items = products,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

                return Ok(ApiResponse<PaginatedResponse<ProductListDto>>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<ProductListDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDetailDto>>> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.ProductImages)
                    .Where(p => p.Id == id && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    return NotFound(ApiResponse<ProductDetailDto>.ErrorResponse("Product not found"));
                }

                var productDto = new ProductDetailDto
                {
                    Id = product.Id,
                    SKU = product.SKU,
                    Barcode = product.Barcode,
                    Name = product.Name,
                    Slug = product.Slug,
                    ShortDescription = product.ShortDescription,
                    Description = product.Description,
                    ImageUrl = product.ImageUrl,
                    Images = product.ProductImages?.Select(i => new ProductImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        AltText = i.AltText,
                        DisplayOrder = i.DisplayOrder,
                        IsPrimary = i.IsPrimary
                    }).ToList(),
                    VideoUrl = product.VideoUrl,
                    Price = product.Price,
                    OriginalPrice = product.OriginalPrice,
                    DiscountPercent = product.DiscountPercent,
                    CostPrice = product.CostPrice,
                    StockQuantity = product.StockQuantity,
                    LowStockThreshold = product.LowStockThreshold,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category?.Name,
                    BrandId = product.BrandId,
                    BrandName = product.Brand?.Name,
                    Weight = product.Weight,
                    Volume = product.Volume,
                    Unit = product.Unit,
                    Ingredients = product.Ingredients,
                    Usage = product.Usage,
                    Origin = product.Origin,
                    AverageRating = product.AverageRating,
                    TotalReviews = product.TotalReviews,
                    ViewCount = product.ViewCount,
                    SoldCount = product.SoldCount,
                    IsFeatured = product.IsFeatured,
                    IsBestSeller = product.IsBestSeller,
                    IsNew = product.IsNew,
                    IsOnSale = product.IsOnSale,
                    IsActive = product.IsActive,
                    MetaTitle = product.MetaTitle,
                    MetaDescription = product.MetaDescription,
                    MetaKeywords = product.MetaKeywords,
                    Tags = product.Tags,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                };

                return Ok(ApiResponse<ProductDetailDto>.SuccessResponse(productDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create new product
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ProductDetailDto>>> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                // Check SKU exists
                if (await _context.Products.AnyAsync(p => p.SKU == request.SKU && !p.IsDeleted))
                {
                    return BadRequest(ApiResponse<ProductDetailDto>.ErrorResponse("SKU already exists"));
                }

                var product = new Product
                {
                    SKU = request.SKU,
                    Barcode = request.Barcode,
                    Name = request.Name,
                    Slug = request.Slug ?? GenerateSlug(request.Name),
                    ShortDescription = request.ShortDescription,
                    Description = request.Description,
                    ImageUrl = request.ImageUrl,
                    VideoUrl = request.VideoUrl,
                    Price = request.Price,
                    OriginalPrice = request.OriginalPrice > 0 ? request.OriginalPrice : request.Price,
                    CostPrice = request.CostPrice,
                    StockQuantity = request.StockQuantity,
                    LowStockThreshold = request.LowStockThreshold,
                    CategoryId = request.CategoryId,
                    BrandId = request.BrandId,
                    Weight = request.Weight,
                    Volume = request.Volume,
                    Unit = request.Unit,
                    Ingredients = request.Ingredients,
                    Usage = request.Usage,
                    Origin = request.Origin,
                    IsFeatured = request.IsFeatured,
                    IsNew = request.IsNew,
                    IsActive = request.IsActive,
                    MetaTitle = request.MetaTitle,
                    MetaDescription = request.MetaDescription,
                    MetaKeywords = request.MetaKeywords,
                    Tags = request.Tags,
                    CreatedAt = DateTime.UtcNow
                };

                // Calculate discount percent
                if (product.OriginalPrice > 0 && product.Price < product.OriginalPrice)
                {
                    product.DiscountPercent = (int)Math.Round((1 - product.Price / product.OriginalPrice) * 100);
                    product.IsOnSale = true;
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Add product images
                if (request.ImageUrls != null && request.ImageUrls.Any())
                {
                    var displayOrder = 0;
                    foreach (var imageUrl in request.ImageUrls)
                    {
                        _context.ProductImages.Add(new ProductImage
                        {
                            ProductId = product.Id,
                            ImageUrl = imageUrl,
                            DisplayOrder = displayOrder++,
                            IsPrimary = displayOrder == 1,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                return Ok(ApiResponse<ProductDetailDto>.SuccessResponse(
                    new ProductDetailDto { Id = product.Id, Name = product.Name },
                    "Product created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update product
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ProductDetailDto>>> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null || product.IsDeleted)
                {
                    return NotFound(ApiResponse<ProductDetailDto>.ErrorResponse("Product not found"));
                }

                // Check SKU if changed
                if (!string.IsNullOrEmpty(request.SKU) && request.SKU != product.SKU)
                {
                    if (await _context.Products.AnyAsync(p => p.SKU == request.SKU && p.Id != id && !p.IsDeleted))
                    {
                        return BadRequest(ApiResponse<ProductDetailDto>.ErrorResponse("SKU already exists"));
                    }
                    product.SKU = request.SKU;
                }

                // Update properties
                if (!string.IsNullOrEmpty(request.Barcode)) product.Barcode = request.Barcode;
                if (!string.IsNullOrEmpty(request.Name))
                {
                    product.Name = request.Name;
                    if (string.IsNullOrEmpty(request.Slug))
                        product.Slug = GenerateSlug(request.Name);
                }
                if (!string.IsNullOrEmpty(request.Slug)) product.Slug = request.Slug;
                if (request.ShortDescription != null) product.ShortDescription = request.ShortDescription;
                if (request.Description != null) product.Description = request.Description;
                if (request.ImageUrl != null) product.ImageUrl = request.ImageUrl;
                if (request.VideoUrl != null) product.VideoUrl = request.VideoUrl;
                if (request.Price.HasValue) product.Price = request.Price.Value;
                if (request.OriginalPrice.HasValue) product.OriginalPrice = request.OriginalPrice.Value;
                if (request.CostPrice.HasValue) product.CostPrice = request.CostPrice.Value;
                if (request.StockQuantity.HasValue) product.StockQuantity = request.StockQuantity.Value;
                if (request.LowStockThreshold.HasValue) product.LowStockThreshold = request.LowStockThreshold.Value;
                if (request.CategoryId.HasValue) product.CategoryId = request.CategoryId;
                if (request.BrandId.HasValue) product.BrandId = request.BrandId;
                if (request.Weight.HasValue) product.Weight = request.Weight;
                if (request.Volume.HasValue) product.Volume = request.Volume;
                if (request.Unit != null) product.Unit = request.Unit;
                if (request.Ingredients != null) product.Ingredients = request.Ingredients;
                if (request.Usage != null) product.Usage = request.Usage;
                if (request.Origin != null) product.Origin = request.Origin;
                if (request.IsFeatured.HasValue) product.IsFeatured = request.IsFeatured.Value;
                if (request.IsBestSeller.HasValue) product.IsBestSeller = request.IsBestSeller.Value;
                if (request.IsNew.HasValue) product.IsNew = request.IsNew.Value;
                if (request.IsOnSale.HasValue) product.IsOnSale = request.IsOnSale.Value;
                if (request.IsActive.HasValue) product.IsActive = request.IsActive.Value;
                if (request.MetaTitle != null) product.MetaTitle = request.MetaTitle;
                if (request.MetaDescription != null) product.MetaDescription = request.MetaDescription;
                if (request.MetaKeywords != null) product.MetaKeywords = request.MetaKeywords;
                if (request.Tags != null) product.Tags = request.Tags;

                // Recalculate discount
                if (product.OriginalPrice > 0 && product.Price < product.OriginalPrice)
                {
                    product.DiscountPercent = (int)Math.Round((1 - product.Price / product.OriginalPrice) * 100);
                }
                else
                {
                    product.DiscountPercent = 0;
                }

                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<ProductDetailDto>.SuccessResponse(
                    new ProductDetailDto { Id = product.Id, Name = product.Name },
                    "Product updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductDetailDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete product (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null || product.IsDeleted)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse("Product not found"));
                }

                product.IsDeleted = true;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Product deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Toggle product active status
        /// </summary>
        [HttpPut("{id}/toggle-active")]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleProductActive(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null || product.IsDeleted)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse("Product not found"));
                }

                product.IsActive = !product.IsActive;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(product.IsActive,
                    product.IsActive ? "Product activated" : "Product deactivated"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Toggle product featured status
        /// </summary>
        [HttpPut("{id}/toggle-featured")]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleProductFeatured(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null || product.IsDeleted)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse("Product not found"));
                }

                product.IsFeatured = !product.IsFeatured;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(product.IsFeatured,
                    product.IsFeatured ? "Product featured" : "Product unfeatured"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update product stock
        /// </summary>
        [HttpPut("{id}/stock")]
        public async Task<ActionResult<ApiResponse<int>>> UpdateStock(int id, [FromBody] UpdateStockRequest request)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null || product.IsDeleted)
                {
                    return NotFound(ApiResponse<int>.ErrorResponse("Product not found"));
                }

                switch (request.Type.ToLower())
                {
                    case "add":
                        product.StockQuantity += request.Quantity;
                        break;
                    case "subtract":
                        product.StockQuantity = Math.Max(0, product.StockQuantity - request.Quantity);
                        break;
                    case "set":
                        product.StockQuantity = Math.Max(0, request.Quantity);
                        break;
                    default:
                        return BadRequest(ApiResponse<int>.ErrorResponse("Invalid stock update type"));
                }

                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<int>.SuccessResponse(product.StockQuantity, "Stock updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<int>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get low stock products
        /// </summary>
        [HttpGet("low-stock")]
        public async Task<ActionResult<ApiResponse<List<ProductListDto>>>> GetLowStockProducts([FromQuery] int threshold = 0)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Where(p => !p.IsDeleted && p.IsActive);

                if (threshold > 0)
                {
                    query = query.Where(p => p.StockQuantity <= threshold);
                }
                else
                {
                    query = query.Where(p => p.StockQuantity <= p.LowStockThreshold);
                }

                var products = await query
                    .OrderBy(p => p.StockQuantity)
                    .Select(p => new ProductListDto
                    {
                        Id = p.Id,
                        SKU = p.SKU,
                        Name = p.Name,
                        ImageUrl = p.ImageUrl,
                        Price = p.Price,
                        OriginalPrice = p.OriginalPrice,
                        StockQuantity = p.StockQuantity,
                        SoldCount = p.SoldCount,
                        CategoryName = p.Category != null ? p.Category.Name : null,
                        BrandName = p.Brand != null ? p.Brand.Name : null,
                        IsActive = p.IsActive,
                        IsFeatured = p.IsFeatured,
                        AverageRating = p.AverageRating,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<ProductListDto>>.SuccessResponse(products));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ProductListDto>>.ErrorResponse($"Server error: {ex.Message}"));
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
