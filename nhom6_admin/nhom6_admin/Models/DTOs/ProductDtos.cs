using System.ComponentModel.DataAnnotations;

namespace nhom6_admin.Models.DTOs
{
    // ==================== PRODUCT DTOs ====================
    public class ProductListDto
    {
        public int Id { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public int StockQuantity { get; set; }
        public int SoldCount { get; set; }
        public string? CategoryName { get; set; }
        public string? BrandName { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public decimal AverageRating { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProductDetailDto
    {
        public int Id { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<ProductImageDto>? Images { get; set; }
        public string? VideoUrl { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public int DiscountPercent { get; set; }
        public decimal? CostPrice { get; set; }
        public int StockQuantity { get; set; }
        public int LowStockThreshold { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public int? Weight { get; set; }
        public int? Volume { get; set; }
        public string? Unit { get; set; }
        public string? Ingredients { get; set; }
        public string? Usage { get; set; }
        public string? Origin { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int ViewCount { get; set; }
        public int SoldCount { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsBestSeller { get; set; }
        public bool IsNew { get; set; }
        public bool IsOnSale { get; set; }
        public bool IsActive { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public string? Tags { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ProductImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class CreateProductRequest
    {
        [Required]
        [MaxLength(50)]
        public string SKU { get; set; } = string.Empty;

        public string? Barcode { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Slug { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<string>? ImageUrls { get; set; }
        public string? VideoUrl { get; set; }

        [Required]
        public decimal Price { get; set; }

        public decimal OriginalPrice { get; set; }
        public decimal? CostPrice { get; set; }
        public int StockQuantity { get; set; } = 0;
        public int LowStockThreshold { get; set; } = 10;
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? Weight { get; set; }
        public int? Volume { get; set; }
        public string? Unit { get; set; }
        public string? Ingredients { get; set; }
        public string? Usage { get; set; }
        public string? Origin { get; set; }
        public bool IsFeatured { get; set; } = false;
        public bool IsNew { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public string? Tags { get; set; }
    }

    public class UpdateProductRequest
    {
        public string? SKU { get; set; }
        public string? Barcode { get; set; }
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<string>? ImageUrls { get; set; }
        public string? VideoUrl { get; set; }
        public decimal? Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? CostPrice { get; set; }
        public int? StockQuantity { get; set; }
        public int? LowStockThreshold { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? Weight { get; set; }
        public int? Volume { get; set; }
        public string? Unit { get; set; }
        public string? Ingredients { get; set; }
        public string? Usage { get; set; }
        public string? Origin { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsBestSeller { get; set; }
        public bool? IsNew { get; set; }
        public bool? IsOnSale { get; set; }
        public bool? IsActive { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public string? Tags { get; set; }
    }

    public class ProductFilterRequest
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? LowStock { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }

    public class UpdateStockRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Type { get; set; } = "add"; // add, subtract, set
        public string? Reason { get; set; }
    }

    // ==================== CATEGORY DTOs ====================
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Icon { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public int ProductCount { get; set; }
        public List<CategoryDto>? ChildCategories { get; set; }
    }

    public class CreateCategoryRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Slug { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Icon { get; set; }
        public int? ParentCategoryId { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public bool ShowOnHomePage { get; set; } = false;
    }

    public class UpdateCategoryRequest
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Icon { get; set; }
        public int? ParentCategoryId { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsActive { get; set; }
        public bool? ShowOnHomePage { get; set; }
    }

    // ==================== BRAND DTOs ====================
    public class BrandDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? CountryOfOrigin { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public int ProductCount { get; set; }
    }

    public class CreateBrandRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Slug { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? CountryOfOrigin { get; set; }
        public int? YearEstablished { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
    }

    public class UpdateBrandRequest
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? CountryOfOrigin { get; set; }
        public int? YearEstablished { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsFeatured { get; set; }
    }
}
