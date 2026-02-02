namespace nhom6_backend.Models.DTOs.Admin
{
    // ============================================
    // DASHBOARD DTOs
    // ============================================

    /// <summary>
    /// DTO cho Dashboard thống kê tổng quan
    /// </summary>
    public class DashboardStatsDto
    {
        // Thống kê tổng quan
        public decimal TotalRevenue { get; set; }
        public decimal RevenueToday { get; set; }
        public decimal RevenueThisWeek { get; set; }
        public decimal RevenueThisMonth { get; set; }

        // Thống kê đơn hàng
        public int TotalOrders { get; set; }
        public int OrdersToday { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }

        // Thống kê khách hàng
        public int TotalCustomers { get; set; }
        public int NewCustomersToday { get; set; }
        public int NewCustomersThisWeek { get; set; }
        public int NewCustomersThisMonth { get; set; }

        // Thống kê lịch hẹn
        public int TotalAppointments { get; set; }
        public int AppointmentsToday { get; set; }
        public int PendingAppointments { get; set; }

        // Thống kê sản phẩm
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int OutOfStockProducts { get; set; }

        // Thống kê dịch vụ
        public int TotalServices { get; set; }
        public int ActiveServices { get; set; }

        // Top lists
        public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();
        public List<TopServiceDto> TopServices { get; set; } = new();
        public List<LoyalCustomerDto> LoyalCustomers { get; set; } = new();
        public List<RecentOrderDto> RecentOrders { get; set; } = new();

        // Chart data
        public List<RevenueChartDataDto> RevenueChart { get; set; } = new();
        public List<OrderChartDataDto> OrderChart { get; set; } = new();
    }

    public class TopSellingProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopServiceDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class LoyalCustomerDto
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastOrderDate { get; set; }
    }

    public class RecentOrderDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class RevenueChartDataDto
    {
        public string Label { get; set; } = string.Empty; // Ngày/Tuần/Tháng
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class OrderChartDataDto
    {
        public string Label { get; set; } = string.Empty;
        public int Pending { get; set; }
        public int Processing { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }

    // ============================================
    // USER MANAGEMENT DTOs
    // ============================================

    public class UserListDto
    {
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class UserDetailDto : UserListDto
    {
        public string? Address { get; set; }
        public List<UserOrderSummaryDto> RecentOrders { get; set; } = new();
        public List<UserAddressDto> Addresses { get; set; } = new();
    }

    public class UserOrderSummaryDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UserAddressDto
    {
        public int Id { get; set; }
        public string FullAddress { get; set; } = string.Empty;
        public string? ReceiverName { get; set; }
        public string? ReceiverPhone { get; set; }
        public bool IsDefault { get; set; }
    }

    public class LockUserRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public int? LockDays { get; set; } // null = vĩnh viễn
    }

    public class UpdateUserRoleRequest
    {
        public string UserId { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }

    // ============================================
    // ORDER MANAGEMENT DTOs
    // ============================================

    public class OrderListDto
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string? PaymentMethodName { get; set; }
        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OrderDetailDto : OrderListDto
    {
        public string? UserId { get; set; }
        public string? ShippingAddressText { get; set; }
        public string? ReceiverName { get; set; }
        public string? ReceiverPhone { get; set; }
        public string? TrackingNumber { get; set; }
        public string? ShippingMethodName { get; set; }
        public string? CouponCode { get; set; }
        public string? CustomerNotes { get; set; }
        public string? InternalNotes { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public string? SKU { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class OrderStatusHistoryDto
    {
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        public int OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? TrackingNumber { get; set; }
    }

    public class OrderFilterRequest
    {
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; } // Order code, customer name, phone
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDesc { get; set; } = true;
    }

    // ============================================
    // PRODUCT MANAGEMENT DTOs
    // ============================================

    public class ProductListDto
    {
        public int Id { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? CategoryName { get; set; }
        public string? BrandName { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int StockQuantity { get; set; }
        public int SoldQuantity { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public decimal? AverageRating { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateProductRequest
    {
        public string? SKU { get; set; }
        public string? Barcode { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<string>? ImageUrls { get; set; }
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? CostPrice { get; set; }
        public int StockQuantity { get; set; }
        public int? LowStockThreshold { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public decimal? Weight { get; set; }
        public string? Unit { get; set; }
    }

    public class UpdateProductRequest : CreateProductRequest
    {
        public int Id { get; set; }
        public new List<string>? ImageUrls { get; set; }
        public new string? MetaTitle { get; set; }
        public new string? MetaDescription { get; set; }
        public new string? MetaKeywords { get; set; }
        public new decimal? Weight { get; set; }
        public new string? Unit { get; set; }
    }

    public class ProductFilterRequest
    {
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public string? Status { get; set; }
        public bool? LowStock { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDesc { get; set; } = true;
    }

    public class UpdateStockRequest
    {
        public int Quantity { get; set; }
    }

    // ============================================
    // SERVICE MANAGEMENT DTOs
    // ============================================

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
        public int TotalBookings { get; set; }
        public decimal? AverageRating { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateServiceRequest
    {
        public string ServiceCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int? CategoryId { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public List<int>? StaffIds { get; set; }
    }

    public class UpdateServiceRequest : CreateServiceRequest
    {
        public int Id { get; set; }
    }

    public class ServiceFilterRequest
    {
        public int? CategoryId { get; set; }
        public string? Status { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDesc { get; set; } = true;
    }

    // ============================================
    // APPOINTMENT MANAGEMENT DTOs
    // ============================================

    public class AppointmentListDto
    {
        public int Id { get; set; }
        public string AppointmentCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string? StaffName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int TotalDurationMinutes { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> ServiceNames { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateAppointmentStatusRequest
    {
        public int AppointmentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class AppointmentFilterRequest
    {
        public string? Status { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? StaffId { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "AppointmentDate";
        public bool SortDesc { get; set; } = true;
    }

    // ============================================
    // REPORT DTOs
    // ============================================

    public class RevenueReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal ProductRevenue { get; set; }
        public decimal ServiceRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalAppointments { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<RevenueByDayDto> DailyRevenue { get; set; } = new();
        public List<RevenueByCategoryDto> RevenueByCategory { get; set; } = new();
    }

    public class RevenueByDayDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public int AppointmentCount { get; set; }
    }

    public class RevenueByCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Percentage { get; set; }
    }

    public class ReportFilterRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? ReportType { get; set; } // daily, weekly, monthly, yearly
    }

    // ============================================
    // PAGINATION RESPONSE
    // ============================================

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    // ============================================
    // COMMON RESPONSE
    // ============================================

    public class AdminApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static AdminApiResponse<T> Ok(T data, string? message = null)
            => new() { Success = true, Data = data, Message = message };

        public static AdminApiResponse<T> Fail(string message, List<string>? errors = null)
            => new() { Success = false, Message = message, Errors = errors };
    }

    // ============================================
    // CATEGORY MANAGEMENT DTOs
    // ============================================

    public class CategoryListDto
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
        public bool ShowOnHomePage { get; set; }
        public int ProductCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CategoryDetailDto : CategoryListDto
    {
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public List<CategoryListDto>? ChildCategories { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Icon { get; set; }
        public int? ParentCategoryId { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public bool ShowOnHomePage { get; set; } = false;
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
    }

    public class UpdateCategoryRequest : CreateCategoryRequest
    {
        public int Id { get; set; }
    }

    public class CategoryFilterRequest
    {
        public int? ParentCategoryId { get; set; }
        public string? Status { get; set; } // active, inactive
        public bool? ShowOnHomePage { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "DisplayOrder";
        public bool SortDesc { get; set; } = false;
    }

    // ============================================
    // BRAND MANAGEMENT DTOs
    // ============================================

    public class BrandListDto
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
        public DateTime CreatedAt { get; set; }
    }

    public class BrandDetailDto : BrandListDto
    {
        public string? BannerUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public int? YearEstablished { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateBrandRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? CountryOfOrigin { get; set; }
        public int? YearEstablished { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
    }

    public class UpdateBrandRequest : CreateBrandRequest
    {
        public int Id { get; set; }
    }

    public class BrandFilterRequest
    {
        public string? Status { get; set; } // active, inactive
        public bool? IsFeatured { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "DisplayOrder";
        public bool SortDesc { get; set; } = false;
    }

    // ============================================
    // STAFF MANAGEMENT DTOs
    // ============================================

    public class StaffListDto
    {
        public int Id { get; set; }
        public string StaffCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? NickName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string Position { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public bool AcceptOnlineBooking { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalCustomersServed { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class StaffDetailDto : StaffListDto
    {
        public string? UserId { get; set; }
        public string? Bio { get; set; }
        public string? Specialties { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public DateTime HireDate { get; set; }
        public decimal? BaseSalary { get; set; }
        public int CommissionPercent { get; set; }
        public decimal TotalRevenue { get; set; }
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TikTokUrl { get; set; }
        public List<int>? ServiceIds { get; set; }
        public List<string>? ServiceNames { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateStaffRequest
    {
        public string? UserId { get; set; }
        public string StaffCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? NickName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Bio { get; set; }
        public string Position { get; set; } = "Barber";
        public string Level { get; set; } = "Junior";
        public string? Specialties { get; set; }
        public int YearsOfExperience { get; set; } = 0;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public DateTime? HireDate { get; set; }
        public decimal? BaseSalary { get; set; }
        public int CommissionPercent { get; set; } = 0;
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TikTokUrl { get; set; }
        public string Status { get; set; } = "Active";
        public bool IsAvailable { get; set; } = true;
        public bool AcceptOnlineBooking { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
        public List<int>? ServiceIds { get; set; }
    }

    public class UpdateStaffRequest : CreateStaffRequest
    {
        public int Id { get; set; }
    }

    public class StaffFilterRequest
    {
        public string? Status { get; set; } // Active, OnLeave, Resigned
        public string? Position { get; set; }
        public string? Level { get; set; }
        public bool? IsAvailable { get; set; }
        public bool? AcceptOnlineBooking { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "DisplayOrder";
        public bool SortDesc { get; set; } = false;
    }
}
