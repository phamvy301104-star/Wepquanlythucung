using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_admin.Models.Entities
{
    /// <summary>
    /// Dịch vụ salon (Cắt tóc, Uốn, Nhuộm, Gội đầu, Massage, etc.)
    /// </summary>
    public class Service : BaseEntity
    {
        /// <summary>
        /// Mã dịch vụ
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string ServiceCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên dịch vụ
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
        /// Danh sách hình ảnh (JSON array)
        /// </summary>
        public string? GalleryImages { get; set; }

        /// <summary>
        /// Video demo (YouTube URL)
        /// </summary>
        [MaxLength(500)]
        public string? VideoUrl { get; set; }

        /// <summary>
        /// Giá dịch vụ
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// Giá gốc (để hiển thị giảm giá)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? OriginalPrice { get; set; }

        /// <summary>
        /// Giá tối thiểu
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// Giá tối đa
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Thời gian thực hiện dịch vụ (phút)
        /// </summary>
        public int DurationMinutes { get; set; } = 30;

        /// <summary>
        /// Thời gian buffer sau dịch vụ (phút)
        /// </summary>
        public int BufferMinutes { get; set; } = 10;

        /// <summary>
        /// Số nhân viên tối thiểu cần thiết
        /// </summary>
        public int RequiredStaff { get; set; } = 1;

        /// <summary>
        /// Giới tính phù hợp: Male, Female, All
        /// </summary>
        [MaxLength(10)]
        public string Gender { get; set; } = "Male";

        /// <summary>
        /// Yêu cầu đặt trước (giờ)
        /// </summary>
        public int? RequiredAdvanceBookingHours { get; set; }

        /// <summary>
        /// Cho phép hủy trước (giờ)
        /// </summary>
        public int? CancellationHours { get; set; }

        /// <summary>
        /// Dịch vụ nổi bật
        /// </summary>
        public bool IsFeatured { get; set; } = false;

        /// <summary>
        /// Dịch vụ phổ biến
        /// </summary>
        public bool IsPopular { get; set; } = false;

        /// <summary>
        /// Dịch vụ mới
        /// </summary>
        public bool IsNew { get; set; } = false;

        /// <summary>
        /// Trạng thái hiển thị
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Đánh giá trung bình
        /// </summary>
        public decimal AverageRating { get; set; } = 0;

        /// <summary>
        /// Tổng số đánh giá
        /// </summary>
        public int TotalReviews { get; set; } = 0;

        /// <summary>
        /// Tổng số lượt đặt
        /// </summary>
        public int TotalBookings { get; set; } = 0;

        // Navigation Properties
        public virtual ICollection<AppointmentService>? AppointmentServices { get; set; }
        public virtual ICollection<StaffService>? StaffServices { get; set; }
    }
}
