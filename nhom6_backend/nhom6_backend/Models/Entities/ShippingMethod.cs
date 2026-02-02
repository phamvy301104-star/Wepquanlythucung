using System.ComponentModel.DataAnnotations;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Phương thức vận chuyển
    /// </summary>
    public class ShippingMethod : BaseEntity
    {
        /// <summary>
        /// Mã phương thức vận chuyển
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên phương thức
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Icon/Logo
        /// </summary>
        [MaxLength(500)]
        public string? IconUrl { get; set; }

        /// <summary>
        /// Loại: Standard, Express, SameDay, Pickup
        /// </summary>
        [MaxLength(30)]
        public string Type { get; set; } = "Standard";

        /// <summary>
        /// Phí vận chuyển cơ bản
        /// </summary>
        public decimal BaseFee { get; set; } = 0;

        /// <summary>
        /// Phí mỗi kg thêm
        /// </summary>
        public decimal FeePerKg { get; set; } = 0;

        /// <summary>
        /// Miễn phí ship từ (số tiền đơn hàng)
        /// </summary>
        public decimal? FreeShippingMinAmount { get; set; }

        /// <summary>
        /// Thời gian giao hàng dự kiến (ngày)
        /// </summary>
        public int EstimatedDays { get; set; } = 3;

        /// <summary>
        /// Thời gian giao tối thiểu (ngày)
        /// </summary>
        public int MinDays { get; set; } = 1;

        /// <summary>
        /// Thời gian giao tối đa (ngày)
        /// </summary>
        public int MaxDays { get; set; } = 5;

        /// <summary>
        /// Trọng lượng tối đa (kg)
        /// </summary>
        public decimal? MaxWeight { get; set; }

        /// <summary>
        /// Danh sách tỉnh thành được hỗ trợ (JSON array)
        /// </summary>
        public string? SupportedProvinces { get; set; }

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Trạng thái active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Tích hợp API (đơn vị vận chuyển)
        /// </summary>
        [MaxLength(50)]
        public string? CarrierCode { get; set; }

        /// <summary>
        /// Cấu hình API (JSON)
        /// </summary>
        public string? ApiConfiguration { get; set; }

        // Navigation Properties
        public virtual ICollection<Order>? Orders { get; set; }
    }
}
