using System.ComponentModel.DataAnnotations;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Thông tin cửa hàng/salon
    /// </summary>
    public class StoreInfo : BaseEntity
    {
        /// <summary>
        /// Mã cửa hàng
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string StoreCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên cửa hàng
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Slogan
        /// </summary>
        [MaxLength(300)]
        public string? Slogan { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Logo URL
        /// </summary>
        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Banner URL
        /// </summary>
        [MaxLength(500)]
        public string? BannerUrl { get; set; }

        /// <summary>
        /// Favicon URL
        /// </summary>
        [MaxLength(500)]
        public string? FaviconUrl { get; set; }

        /// <summary>
        /// Địa chỉ chi tiết
        /// </summary>
        [MaxLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// Mã tỉnh/thành
        /// </summary>
        [MaxLength(10)]
        public string? ProvinceCode { get; set; }

        /// <summary>
        /// Mã quận/huyện
        /// </summary>
        [MaxLength(10)]
        public string? DistrictCode { get; set; }

        /// <summary>
        /// Mã phường/xã
        /// </summary>
        [MaxLength(10)]
        public string? WardCode { get; set; }

        /// <summary>
        /// Tọa độ Latitude
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Tọa độ Longitude
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Số điện thoại chính
        /// </summary>
        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Hotline
        /// </summary>
        [MaxLength(15)]
        public string? Hotline { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        [MaxLength(100)]
        public string? Email { get; set; }

        /// <summary>
        /// Website
        /// </summary>
        [MaxLength(200)]
        public string? Website { get; set; }

        /// <summary>
        /// Link Facebook
        /// </summary>
        [MaxLength(200)]
        public string? FacebookUrl { get; set; }

        /// <summary>
        /// Link Instagram
        /// </summary>
        [MaxLength(200)]
        public string? InstagramUrl { get; set; }

        /// <summary>
        /// Link TikTok
        /// </summary>
        [MaxLength(200)]
        public string? TikTokUrl { get; set; }

        /// <summary>
        /// Link YouTube
        /// </summary>
        [MaxLength(200)]
        public string? YouTubeUrl { get; set; }

        /// <summary>
        /// Zalo
        /// </summary>
        [MaxLength(50)]
        public string? ZaloNumber { get; set; }

        /// <summary>
        /// Giờ mở cửa (JSON) - từng ngày trong tuần
        /// </summary>
        public string? BusinessHours { get; set; }

        /// <summary>
        /// Mã số thuế
        /// </summary>
        [MaxLength(20)]
        public string? TaxCode { get; set; }

        /// <summary>
        /// Tên ngân hàng
        /// </summary>
        [MaxLength(100)]
        public string? BankName { get; set; }

        /// <summary>
        /// Số tài khoản ngân hàng
        /// </summary>
        [MaxLength(30)]
        public string? BankAccountNumber { get; set; }

        /// <summary>
        /// Tên chủ tài khoản
        /// </summary>
        [MaxLength(100)]
        public string? BankAccountName { get; set; }

        /// <summary>
        /// VietQR URL/Data
        /// </summary>
        [MaxLength(500)]
        public string? VietQRData { get; set; }

        /// <summary>
        /// Tiền tệ mặc định
        /// </summary>
        [MaxLength(3)]
        public string Currency { get; set; } = "VND";

        /// <summary>
        /// Múi giờ
        /// </summary>
        [MaxLength(50)]
        public string TimeZone { get; set; } = "SE Asia Standard Time";

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
        /// Google Analytics ID
        /// </summary>
        [MaxLength(50)]
        public string? GoogleAnalyticsId { get; set; }

        /// <summary>
        /// Facebook Pixel ID
        /// </summary>
        [MaxLength(50)]
        public string? FacebookPixelId { get; set; }

        /// <summary>
        /// Trạng thái: Open, Closed, Maintenance
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Open";

        /// <summary>
        /// Thông báo đặc biệt (hiển thị trên app/web)
        /// </summary>
        [MaxLength(500)]
        public string? Announcement { get; set; }
    }
}
