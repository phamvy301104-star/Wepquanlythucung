using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Lưu thông tin tài khoản bị cấm/khóa
    /// </summary>
    public class UserBan : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến User bị cấm
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Admin/Staff thực hiện ban
        /// </summary>
        public string? BannedByUserId { get; set; }

        [ForeignKey("BannedByUserId")]
        public virtual User? BannedByUser { get; set; }

        /// <summary>
        /// Loại ban: Temporary, Permanent
        /// </summary>
        [MaxLength(20)]
        public string BanType { get; set; } = "Temporary";

        /// <summary>
        /// Lý do ban
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chi tiết
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Ngày bắt đầu ban
        /// </summary>
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ngày kết thúc ban (null nếu vĩnh viễn)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Ban còn hiệu lực không
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Ngày được gỡ ban
        /// </summary>
        public DateTime? UnbannedAt { get; set; }

        /// <summary>
        /// Admin gỡ ban
        /// </summary>
        public string? UnbannedByUserId { get; set; }

        /// <summary>
        /// Lý do gỡ ban
        /// </summary>
        [MaxLength(500)]
        public string? UnbanReason { get; set; }
    }
}
