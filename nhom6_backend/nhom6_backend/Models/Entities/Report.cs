using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Báo cáo vi phạm từ người dùng
    /// </summary>
    public class Report : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến User (người báo cáo)
        /// </summary>
        [Required]
        public string ReporterUserId { get; set; } = string.Empty;

        [ForeignKey("ReporterUserId")]
        public virtual User? ReporterUser { get; set; }

        /// <summary>
        /// Loại nội dung bị báo cáo: Post, Comment, User, Product, Review
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// ID của nội dung bị báo cáo
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ContentId { get; set; } = string.Empty;

        /// <summary>
        /// ID của User bị báo cáo (nếu ContentType là User hoặc owner của content)
        /// </summary>
        public string? ReportedUserId { get; set; }

        [ForeignKey("ReportedUserId")]
        public virtual User? ReportedUser { get; set; }

        /// <summary>
        /// Lý do báo cáo: Spam, Harassment, FakeInfo, InappropriateContent, Violence, etc.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Chi tiết lý do
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Bằng chứng (URL hình ảnh, JSON array)
        /// </summary>
        public string? Evidence { get; set; }

        /// <summary>
        /// Trạng thái: Pending, Reviewing, Resolved, Dismissed
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Admin xử lý
        /// </summary>
        public string? ReviewedByUserId { get; set; }

        [ForeignKey("ReviewedByUserId")]
        public virtual User? ReviewedByUser { get; set; }

        /// <summary>
        /// Ngày xử lý
        /// </summary>
        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// Hành động đã thực hiện
        /// </summary>
        [MaxLength(100)]
        public string? ActionTaken { get; set; }

        /// <summary>
        /// Ghi chú của admin
        /// </summary>
        [MaxLength(500)]
        public string? AdminNotes { get; set; }

        /// <summary>
        /// Mức độ nghiêm trọng được đánh giá: 1-5
        /// </summary>
        public int? AssessedSeverity { get; set; }
    }
}
