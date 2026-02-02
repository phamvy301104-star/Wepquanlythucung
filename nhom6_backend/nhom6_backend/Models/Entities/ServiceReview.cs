using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Đánh giá dịch vụ từ khách hàng
    /// </summary>
    public class ServiceReview : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Service
        /// </summary>
        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }

        /// <summary>
        /// Khóa ngoại đến User
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Khóa ngoại đến Appointment (chứng minh đã sử dụng dịch vụ)
        /// </summary>
        public int? AppointmentId { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual Appointment? Appointment { get; set; }

        /// <summary>
        /// Khóa ngoại đến Staff được đánh giá
        /// </summary>
        public int? StaffId { get; set; }

        [ForeignKey("StaffId")]
        public virtual Staff? Staff { get; set; }

        /// <summary>
        /// Điểm đánh giá dịch vụ (1-5)
        /// </summary>
        [Range(1, 5)]
        public int ServiceRating { get; set; }

        /// <summary>
        /// Điểm đánh giá nhân viên (1-5)
        /// </summary>
        [Range(1, 5)]
        public int? StaffRating { get; set; }

        /// <summary>
        /// Điểm đánh giá cơ sở vật chất (1-5)
        /// </summary>
        [Range(1, 5)]
        public int? FacilityRating { get; set; }

        /// <summary>
        /// Tiêu đề đánh giá
        /// </summary>
        [MaxLength(200)]
        public string? Title { get; set; }

        /// <summary>
        /// Nội dung đánh giá
        /// </summary>
        [MaxLength(2000)]
        public string? Content { get; set; }

        /// <summary>
        /// Hình ảnh trước dịch vụ (JSON array)
        /// </summary>
        public string? BeforeImages { get; set; }

        /// <summary>
        /// Hình ảnh sau dịch vụ (JSON array)
        /// </summary>
        public string? AfterImages { get; set; }

        /// <summary>
        /// Có sử dụng dịch vụ (verified)
        /// </summary>
        public bool IsVerified { get; set; } = false;

        /// <summary>
        /// Số lượt thấy hữu ích
        /// </summary>
        public int HelpfulCount { get; set; } = 0;

        /// <summary>
        /// Trạng thái: Pending, Approved, Rejected
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Phản hồi từ salon
        /// </summary>
        [MaxLength(1000)]
        public string? SalonReply { get; set; }

        /// <summary>
        /// Ngày phản hồi
        /// </summary>
        public DateTime? SalonReplyAt { get; set; }

        /// <summary>
        /// Đánh giá ẩn danh
        /// </summary>
        public bool IsAnonymous { get; set; } = false;

        /// <summary>
        /// Có giới thiệu cho người khác không
        /// </summary>
        public bool WouldRecommend { get; set; } = true;
    }
}
