using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Quan hệ theo dõi giữa users
    /// </summary>
    public class Follow : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến User (người theo dõi)
        /// </summary>
        [Required]
        public string FollowerId { get; set; } = string.Empty;

        [ForeignKey("FollowerId")]
        public virtual User? Follower { get; set; }

        /// <summary>
        /// Khóa ngoại đến User (người được theo dõi)
        /// </summary>
        [Required]
        public string FollowingId { get; set; } = string.Empty;

        [ForeignKey("FollowingId")]
        public virtual User? Following { get; set; }

        /// <summary>
        /// Trạng thái: Pending (chờ duyệt), Accepted, Rejected
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Accepted";

        /// <summary>
        /// Nhận thông báo từ người này
        /// </summary>
        public bool NotificationsEnabled { get; set; } = true;

        /// <summary>
        /// Thêm vào danh sách bạn thân (close friends)
        /// </summary>
        public bool IsCloseFriend { get; set; } = false;

        /// <summary>
        /// Đã tắt thông báo (mute)
        /// </summary>
        public bool IsMuted { get; set; } = false;
    }
}
