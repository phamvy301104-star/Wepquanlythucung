using Microsoft.AspNetCore.Identity;

namespace nhom6_backend.Models
{
    /// <summary>
    /// Custom User model extending IdentityUser
    /// Thêm các trường bổ sung cho ứng dụng UME Salon
    /// </summary>
    public class User : IdentityUser
    {
        /// <summary>
        /// Họ và tên đầy đủ của người dùng
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Chữ cái viết tắt tên (VD: NVA)
        /// </summary>
        public string? Initials { get; set; }

        /// <summary>
        /// Đường dẫn ảnh đại diện
        /// </summary>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Địa chỉ của người dùng
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Ngày tạo tài khoản
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ngày cập nhật cuối cùng
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Trạng thái kích hoạt tài khoản
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
