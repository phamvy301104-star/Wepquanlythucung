using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Địa chỉ của người dùng (nhiều địa chỉ)
    /// </summary>
    public class UserAddress : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến User
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Nhãn địa chỉ: Nhà, Công ty, Khác
        /// </summary>
        [MaxLength(50)]
        public string Label { get; set; } = "Nhà";

        /// <summary>
        /// Tên người nhận
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ReceiverName { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại người nhận
        /// </summary>
        [Required]
        [MaxLength(15)]
        public string ReceiverPhone { get; set; } = string.Empty;

        /// <summary>
        /// Mã tỉnh/thành phố (từ API provinces.open-api.vn)
        /// </summary>
        [MaxLength(10)]
        public string? ProvinceCode { get; set; }

        /// <summary>
        /// Tên tỉnh/thành phố
        /// </summary>
        [MaxLength(100)]
        public string? ProvinceName { get; set; }

        /// <summary>
        /// Mã quận/huyện
        /// </summary>
        [MaxLength(10)]
        public string? DistrictCode { get; set; }

        /// <summary>
        /// Tên quận/huyện
        /// </summary>
        [MaxLength(100)]
        public string? DistrictName { get; set; }

        /// <summary>
        /// Mã phường/xã
        /// </summary>
        [MaxLength(10)]
        public string? WardCode { get; set; }

        /// <summary>
        /// Tên phường/xã
        /// </summary>
        [MaxLength(100)]
        public string? WardName { get; set; }

        /// <summary>
        /// Địa chỉ chi tiết (số nhà, tên đường)
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string AddressDetail { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ đầy đủ (computed)
        /// </summary>
        [MaxLength(500)]
        public string? FullAddress { get; set; }

        /// <summary>
        /// Tọa độ Latitude (cho bản đồ)
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Tọa độ Longitude (cho bản đồ)
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Địa chỉ mặc định
        /// </summary>
        public bool IsDefault { get; set; } = false;
    }
}
