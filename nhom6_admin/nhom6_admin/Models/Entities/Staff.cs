using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_admin.Models.Entities
{
    /// <summary>
    /// Nhân viên salon (Thợ cắt tóc, Thợ gội, etc.)
    /// </summary>
    public class Staff : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến User account (nếu có)
        /// </summary>
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Mã nhân viên
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string StaffCode { get; set; } = string.Empty;

        /// <summary>
        /// Họ và tên
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Biệt danh/Tên nghệ danh
        /// </summary>
        [MaxLength(50)]
        public string? NickName { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        [MaxLength(100)]
        public string? Email { get; set; }

        /// <summary>
        /// Số điện thoại
        /// </summary>
        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Ảnh đại diện
        /// </summary>
        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Ảnh bìa/Banner
        /// </summary>
        [MaxLength(500)]
        public string? CoverImageUrl { get; set; }

        /// <summary>
        /// Giới thiệu bản thân
        /// </summary>
        [MaxLength(1000)]
        public string? Bio { get; set; }

        /// <summary>
        /// Chức vụ: Barber, Stylist, Manager, Trainee
        /// </summary>
        [MaxLength(50)]
        public string Position { get; set; } = "Barber";

        /// <summary>
        /// Cấp độ: Junior, Senior, Master, Expert
        /// </summary>
        [MaxLength(20)]
        public string Level { get; set; } = "Junior";

        /// <summary>
        /// Chuyên môn/Thế mạnh
        /// </summary>
        [MaxLength(500)]
        public string? Specialties { get; set; }

        /// <summary>
        /// Số năm kinh nghiệm
        /// </summary>
        public int YearsOfExperience { get; set; } = 0;

        /// <summary>
        /// Ngày sinh
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Giới tính
        /// </summary>
        [MaxLength(10)]
        public string? Gender { get; set; }

        /// <summary>
        /// Ngày bắt đầu làm việc
        /// </summary>
        public DateTime HireDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Lương cơ bản
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? BaseSalary { get; set; }

        /// <summary>
        /// Phần trăm hoa hồng
        /// </summary>
        public int CommissionPercent { get; set; } = 0;

        /// <summary>
        /// Đánh giá trung bình
        /// </summary>
        public decimal AverageRating { get; set; } = 0;

        /// <summary>
        /// Tổng số đánh giá
        /// </summary>
        public int TotalReviews { get; set; } = 0;

        /// <summary>
        /// Tổng số khách đã phục vụ
        /// </summary>
        public int TotalCustomersServed { get; set; } = 0;

        /// <summary>
        /// Tổng doanh thu tạo ra
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalRevenue { get; set; } = 0;

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
        public string? TiktokUrl { get; set; }

        /// <summary>
        /// Trạng thái: Active, OnLeave, Resigned
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Danh sách dịch vụ có thể thực hiện
        /// </summary>
        public virtual ICollection<StaffService>? StaffServices { get; set; }

        /// <summary>
        /// Danh sách lịch hẹn
        /// </summary>
        public virtual ICollection<Appointment>? Appointments { get; set; }

        /// <summary>
        /// Lịch làm việc
        /// </summary>
        public virtual ICollection<StaffSchedule>? Schedules { get; set; }
    }
}
