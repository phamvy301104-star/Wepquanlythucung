using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Refresh Token cho JWT Authentication
    /// </summary>
    public class RefreshToken : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến User
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Refresh Token string
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Ngày hết hạn
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Ngày token được thu hồi (revoked)
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// IP Address khi tạo token
        /// </summary>
        [MaxLength(50)]
        public string? CreatedByIp { get; set; }

        /// <summary>
        /// IP Address khi thu hồi token
        /// </summary>
        [MaxLength(50)]
        public string? RevokedByIp { get; set; }

        /// <summary>
        /// Token thay thế (nếu có refresh)
        /// </summary>
        [MaxLength(500)]
        public string? ReplacedByToken { get; set; }

        /// <summary>
        /// Lý do thu hồi
        /// </summary>
        [MaxLength(200)]
        public string? RevokeReason { get; set; }

        /// <summary>
        /// Device/Browser info
        /// </summary>
        [MaxLength(500)]
        public string? DeviceInfo { get; set; }

        /// <summary>
        /// Check token còn active không
        /// </summary>
        public bool IsActive => RevokedAt == null && !IsExpired;

        /// <summary>
        /// Check token hết hạn chưa
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    }
}
