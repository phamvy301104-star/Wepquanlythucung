using System.ComponentModel.DataAnnotations;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Log hoạt động hệ thống (Audit Log)
    /// </summary>
    public class AuditLog : BaseEntity
    {
        /// <summary>
        /// User thực hiện action (null nếu là system)
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Username (snapshot)
        /// </summary>
        [MaxLength(100)]
        public string? UserName { get; set; }

        /// <summary>
        /// Loại action: Create, Read, Update, Delete, Login, Logout, etc.
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Loại entity bị tác động
        /// </summary>
        [MaxLength(50)]
        public string? EntityType { get; set; }

        /// <summary>
        /// ID của entity
        /// </summary>
        [MaxLength(50)]
        public string? EntityId { get; set; }

        /// <summary>
        /// Giá trị cũ (JSON)
        /// </summary>
        public string? OldValues { get; set; }

        /// <summary>
        /// Giá trị mới (JSON)
        /// </summary>
        public string? NewValues { get; set; }

        /// <summary>
        /// Các trường đã thay đổi
        /// </summary>
        [MaxLength(500)]
        public string? AffectedColumns { get; set; }

        /// <summary>
        /// Mô tả hành động
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// IP Address
        /// </summary>
        [MaxLength(50)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User Agent
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Request URL
        /// </summary>
        [MaxLength(500)]
        public string? RequestUrl { get; set; }

        /// <summary>
        /// HTTP Method
        /// </summary>
        [MaxLength(10)]
        public string? HttpMethod { get; set; }

        /// <summary>
        /// Response Status Code
        /// </summary>
        public int? ResponseStatusCode { get; set; }

        /// <summary>
        /// Thời gian thực hiện (ms)
        /// </summary>
        public int? ExecutionTimeMs { get; set; }

        /// <summary>
        /// Module/Feature
        /// </summary>
        [MaxLength(50)]
        public string? Module { get; set; }

        /// <summary>
        /// Có lỗi không
        /// </summary>
        public bool IsError { get; set; } = false;

        /// <summary>
        /// Thông tin lỗi
        /// </summary>
        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Stack trace (nếu lỗi)
        /// </summary>
        public string? StackTrace { get; set; }
    }
}
