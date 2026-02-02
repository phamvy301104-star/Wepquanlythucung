using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using nhom6_admin.Models.Entities;

namespace nhom6_admin.Models
{
    /// <summary>
    /// Sản phẩm (Dầu gội, dầu xả, keo vuốt tóc, etc.)
    /// </summary>
    public class Product
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Mã sản phẩm (SKU)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string SKU { get; set; } = string.Empty;

        /// <summary>
        /// Mã vạch sản phẩm
        /// </summary>
        [MaxLength(50)]
        public string? Barcode { get; set; }

        /// <summary>
        /// Tên sản phẩm
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Slug URL-friendly
        /// </summary>
        [MaxLength(250)]
        public string? Slug { get; set; }

        /// <summary>
        /// Mô tả ngắn
        /// </summary>
        [MaxLength(500)]
        public string? ShortDescription { get; set; }

        /// <summary>
        /// Mô tả chi tiết (HTML)
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Hình ảnh chính
        /// </summary>
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Danh sách hình ảnh phụ (JSON array)
        /// </summary>
        public string? AdditionalImages { get; set; }

        /// <summary>
        /// Video sản phẩm (YouTube URL)
        /// </summary>
        [MaxLength(500)]
        public string? VideoUrl { get; set; }

        /// <summary>
        /// Giá gốc
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal OriginalPrice { get; set; }

        /// <summary>
        /// Giá bán (sau giảm giá)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// Phần trăm giảm giá
        /// </summary>
        public int DiscountPercent { get; set; } = 0;

        /// <summary>
        /// Giá nhập
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? CostPrice { get; set; }

        /// <summary>
        /// Số lượng tồn kho
        /// </summary>
        public int StockQuantity { get; set; } = 0;

        /// <summary>
        /// Ngưỡng cảnh báo hết hàng
        /// </summary>
        public int LowStockThreshold { get; set; } = 10;

        /// <summary>
        /// Khóa ngoại đến Category
        /// </summary>
        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        /// <summary>
        /// Khóa ngoại đến Brand
        /// </summary>
        public int? BrandId { get; set; }

        [ForeignKey("BrandId")]
        public virtual Brand? Brand { get; set; }

        /// <summary>
        /// Trọng lượng (gram)
        /// </summary>
        public int? Weight { get; set; }

        /// <summary>
        /// Dung tích (ml)
        /// </summary>
        public int? Volume { get; set; }

        /// <summary>
        /// Đơn vị tính
        /// </summary>
        [MaxLength(20)]
        public string? Unit { get; set; }

        /// <summary>
        /// Thành phần
        /// </summary>
        [MaxLength(1000)]
        public string? Ingredients { get; set; }

        /// <summary>
        /// Hướng dẫn sử dụng
        /// </summary>
        [MaxLength(1000)]
        public string? Usage { get; set; }

        /// <summary>
        /// Cảnh báo/Lưu ý
        /// </summary>
        [MaxLength(500)]
        public string? Warnings { get; set; }

        /// <summary>
        /// Ngày sản xuất
        /// </summary>
        public DateTime? ManufactureDate { get; set; }

        /// <summary>
        /// Ngày hết hạn
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Xuất xứ
        /// </summary>
        [MaxLength(100)]
        public string? Origin { get; set; }

        /// <summary>
        /// Đánh giá trung bình (1-5)
        /// </summary>
        public decimal AverageRating { get; set; } = 0;

        /// <summary>
        /// Tổng số đánh giá
        /// </summary>
        public int TotalReviews { get; set; } = 0;

        /// <summary>
        /// Số lượt xem
        /// </summary>
        public int ViewCount { get; set; } = 0;

        /// <summary>
        /// Số lượng đã bán
        /// </summary>
        public int SoldCount { get; set; } = 0;

        /// <summary>
        /// Số lượt yêu thích
        /// </summary>
        public int WishlistCount { get; set; } = 0;

        /// <summary>
        /// Sản phẩm nổi bật
        /// </summary>
        public bool IsFeatured { get; set; } = false;

        /// <summary>
        /// Sản phẩm bán chạy
        /// </summary>
        public bool IsBestSeller { get; set; } = false;

        /// <summary>
        /// Sản phẩm mới
        /// </summary>
        public bool IsNew { get; set; } = false;

        /// <summary>
        /// Đang khuyến mãi
        /// </summary>
        public bool IsOnSale { get; set; } = false;

        /// <summary>
        /// Trạng thái hiển thị
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Cho phép đặt hàng khi hết hàng
        /// </summary>
        public bool AllowBackorder { get; set; } = false;

        /// <summary>
        /// Ngày tạo
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ngày cập nhật
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Đã xóa (soft delete)
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Meta Title cho SEO
        /// </summary>
        [MaxLength(200)]
        public string? MetaTitle { get; set; }

        /// <summary>
        /// Meta Description cho SEO
        /// </summary>
        [MaxLength(500)]
        public string? MetaDescription { get; set; }

        /// <summary>
        /// Meta Keywords cho SEO
        /// </summary>
        [MaxLength(300)]
        public string? MetaKeywords { get; set; }

        /// <summary>
        /// Tags (JSON array)
        /// </summary>
        [MaxLength(500)]
        public string? Tags { get; set; }

        // Navigation Properties
        public virtual ICollection<ProductImage>? ProductImages { get; set; }
        public virtual ICollection<OrderItem>? OrderItems { get; set; }
    }
}

