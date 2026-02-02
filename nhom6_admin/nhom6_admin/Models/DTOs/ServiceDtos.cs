using System.ComponentModel.DataAnnotations;

namespace nhom6_admin.Models.DTOs
{
    // ==================== SERVICE DTOs ====================
    public class ServiceListDto
    {
        public int Id { get; set; }
        public string ServiceCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? CategoryName { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalBookings { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ServiceDetailDto
    {
        public int Id { get; set; }
        public string ServiceCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? GalleryImages { get; set; }
        public string? VideoUrl { get; set; }
        public int? ServiceCategoryId { get; set; }
        public string? ServiceCategoryName { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int DurationMinutes { get; set; }
        public int BufferMinutes { get; set; }
        public int RequiredStaff { get; set; }
        public string Gender { get; set; } = string.Empty;
        public int? RequiredAdvanceBookingHours { get; set; }
        public int? CancellationHours { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPopular { get; set; }
        public bool IsNew { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalBookings { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<StaffSimpleDto>? AssignedStaff { get; set; }
    }

    public class CreateServiceRequest
    {
        [Required]
        [MaxLength(20)]
        public string ServiceCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Slug { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? GalleryImages { get; set; }
        public string? VideoUrl { get; set; }
        public int? ServiceCategoryId { get; set; }

        [Required]
        public decimal Price { get; set; }

        public decimal? OriginalPrice { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int DurationMinutes { get; set; } = 30;
        public int BufferMinutes { get; set; } = 10;
        public int RequiredStaff { get; set; } = 1;
        public string Gender { get; set; } = "All";
        public int? RequiredAdvanceBookingHours { get; set; }
        public int? CancellationHours { get; set; }
        public bool IsFeatured { get; set; } = false;
        public bool IsNew { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
        public List<int>? StaffIds { get; set; }
    }

    public class UpdateServiceRequest
    {
        public string? ServiceCode { get; set; }
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? GalleryImages { get; set; }
        public string? VideoUrl { get; set; }
        public int? ServiceCategoryId { get; set; }
        public decimal? Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? DurationMinutes { get; set; }
        public int? BufferMinutes { get; set; }
        public int? RequiredStaff { get; set; }
        public string? Gender { get; set; }
        public int? RequiredAdvanceBookingHours { get; set; }
        public int? CancellationHours { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsPopular { get; set; }
        public bool? IsNew { get; set; }
        public bool? IsActive { get; set; }
        public int? DisplayOrder { get; set; }
        public List<int>? StaffIds { get; set; }
    }

    public class ServiceFilterRequest
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsFeatured { get; set; }
        public string? Gender { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }

    // ==================== SERVICE CATEGORY DTOs ====================
    public class ServiceCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? ImageUrl { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public int ServiceCount { get; set; }
        public List<ServiceCategoryDto>? ChildCategories { get; set; }
    }

    public class CreateServiceCategoryRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Slug { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? ImageUrl { get; set; }
        public int? ParentCategoryId { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

    public class UpdateServiceCategoryRequest
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? ImageUrl { get; set; }
        public int? ParentCategoryId { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsActive { get; set; }
    }
}
