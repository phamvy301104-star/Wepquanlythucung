using System.ComponentModel.DataAnnotations;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Danh sách từ ngữ bị cấm
    /// </summary>
    public class BannedWord : BaseEntity
    {
        /// <summary>
        /// Từ bị cấm
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Word { get; set; } = string.Empty;

        /// <summary>
        /// Loại: Profanity, Spam, Advertising, Violence, etc.
        /// </summary>
        [MaxLength(30)]
        public string Category { get; set; } = "Profanity";

        /// <summary>
        /// Mức độ nghiêm trọng: 1-5
        /// </summary>
        public int SeverityLevel { get; set; } = 3;

        /// <summary>
        /// Hành động: Warn, Block, Hide, Ban
        /// </summary>
        [MaxLength(20)]
        public string Action { get; set; } = "Warn";

        /// <summary>
        /// Từ thay thế (nếu cần censor)
        /// </summary>
        [MaxLength(100)]
        public string? Replacement { get; set; }

        /// <summary>
        /// Sử dụng regex
        /// </summary>
        public bool IsRegex { get; set; } = false;

        /// <summary>
        /// Case sensitive
        /// </summary>
        public bool IsCaseSensitive { get; set; } = false;

        /// <summary>
        /// Trạng thái active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Số lần bị match
        /// </summary>
        public int MatchCount { get; set; } = 0;

        /// <summary>
        /// Ngôn ngữ: vi, en, etc.
        /// </summary>
        [MaxLength(10)]
        public string? Language { get; set; }
    }
}
