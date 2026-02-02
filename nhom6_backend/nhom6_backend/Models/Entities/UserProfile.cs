using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Thông tin profile mở rộng của người dùng
    /// </summary>
    public class UserProfile : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến User (IdentityUser)
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Ngày sinh
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Giới tính: Male, Female, Other
        /// </summary>
        [MaxLength(10)]
        public string? Gender { get; set; }

        /// <summary>
        /// Số CMND/CCCD
        /// </summary>
        [MaxLength(20)]
        public string? IdentityNumber { get; set; }

        /// <summary>
        /// Nghề nghiệp
        /// </summary>
        [MaxLength(100)]
        public string? Occupation { get; set; }

        /// <summary>
        /// Mô tả bản thân (Bio)
        /// </summary>
        [MaxLength(500)]
        public string? Bio { get; set; }

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
        /// Kiểu khuôn mặt (AI phân tích): Oval, Round, Square, Heart, Oblong
        /// </summary>
        [MaxLength(50)]
        public string? FaceShape { get; set; }

        /// <summary>
        /// Kiểu tóc yêu thích
        /// </summary>
        [MaxLength(100)]
        public string? PreferredHairStyle { get; set; }

        /// <summary>
        /// Ghi chú về tình trạng tóc (dầu, khô, gàu, etc.)
        /// </summary>
        [MaxLength(500)]
        public string? HairConditionNotes { get; set; }

        /// <summary>
        /// Điểm tích lũy (Loyalty Points)
        /// </summary>
        public int LoyaltyPoints { get; set; } = 0;

        /// <summary>
        /// Hạng thành viên: Bronze, Silver, Gold, Platinum, Diamond
        /// </summary>
        [MaxLength(20)]
        public string MembershipTier { get; set; } = "Bronze";
    }
}
