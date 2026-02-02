using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Lịch sử đăng nhập của người dùng
    /// </summary>
    public class UserLoginHistory : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến User
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Thời gian đăng nhập
        /// </summary>
        public DateTime LoginTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời gian đăng xuất
        /// </summary>
        public DateTime? LogoutTime { get; set; }

        /// <summary>
        /// IP Address
        /// </summary>
        [MaxLength(50)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User Agent (Browser/App info)
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Loại thiết bị: Mobile, Desktop, Tablet
        /// </summary>
        [MaxLength(20)]
        public string? DeviceType { get; set; }

        /// <summary>
        /// Tên thiết bị
        /// </summary>
        [MaxLength(100)]
        public string? DeviceName { get; set; }

        /// <summary>
        /// Hệ điều hành
        /// </summary>
        [MaxLength(50)]
        public string? OperatingSystem { get; set; }

        /// <summary>
        /// Tên trình duyệt
        /// </summary>
        [MaxLength(50)]
        public string? Browser { get; set; }

        /// <summary>
        /// Vị trí địa lý (từ IP)
        /// </summary>
        [MaxLength(100)]
        public string? Location { get; set; }

        /// <summary>
        /// Quốc gia
        /// </summary>
        [MaxLength(50)]
        public string? Country { get; set; }

        /// <summary>
        /// Đăng nhập thành công hay thất bại
        /// </summary>
        public bool IsSuccessful { get; set; } = true;

        /// <summary>
        /// Lý do thất bại (nếu có)
        /// </summary>
        [MaxLength(200)]
        public string? FailureReason { get; set; }

        /// <summary>
        /// Phương thức đăng nhập: Password, Google, Facebook
        /// </summary>
        [MaxLength(20)]
        public string LoginMethod { get; set; } = "Password";
    }
}
