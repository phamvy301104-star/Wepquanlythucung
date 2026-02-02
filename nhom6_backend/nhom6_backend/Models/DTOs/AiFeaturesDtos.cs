using System.ComponentModel.DataAnnotations;

namespace nhom6_backend.Models.DTOs
{
    // ==========================================
    // FACE ANALYSIS DTOs
    // ==========================================
    
    /// <summary>
    /// Request để lưu kết quả phân tích khuôn mặt
    /// </summary>
    public class FaceAnalysisRequestDto
    {
        /// <summary>
        /// URL ảnh gốc (base64 hoặc URL)
        /// </summary>
        [Required]
        public string OriginalImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Hình dáng khuôn mặt: Oval, Round, Square, Heart, Oblong, Diamond
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string FaceShape { get; set; } = string.Empty;

        /// <summary>
        /// Confidence score (0-1)
        /// </summary>
        [Range(0, 1)]
        public decimal Confidence { get; set; }

        /// <summary>
        /// Mô tả chi tiết
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Danh sách kiểu tóc được đề xuất
        /// </summary>
        public List<string> RecommendedHairstyles { get; set; } = new();

        /// <summary>
        /// JSON chứa các measurements (optional)
        /// </summary>
        public string? MeasurementsJson { get; set; }
    }

    /// <summary>
    /// Response cho Face Analysis
    /// </summary>
    public class FaceAnalysisResponseDto
    {
        public int Id { get; set; }
        public string OriginalImageUrl { get; set; } = string.Empty;
        public string? FaceShape { get; set; }
        public decimal? Confidence { get; set; }
        public string? Description { get; set; }
        public List<string> RecommendedHairstyles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    // ==========================================
    // HAIR TRY-ON DTOs
    // ==========================================

    /// <summary>
    /// Request để lưu kết quả thử tóc ảo
    /// </summary>
    public class HairTryOnRequestDto
    {
        /// <summary>
        /// URL ảnh khuôn mặt
        /// </summary>
        [Required]
        public string FaceImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// URL ảnh kiểu tóc
        /// </summary>
        [Required]
        public string HairStyleImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// URL ảnh kết quả
        /// </summary>
        public string? ResultImageUrl { get; set; }

        /// <summary>
        /// Tên kiểu tóc
        /// </summary>
        public string? HairStyleName { get; set; }

        /// <summary>
        /// Model AI đã sử dụng
        /// </summary>
        public string AiModel { get; set; } = "HairFastGAN";

        /// <summary>
        /// Người dùng có muốn lưu không
        /// </summary>
        public bool? IsSaved { get; set; }
    }

    /// <summary>
    /// Response cho Hair Try-On
    /// </summary>
    public class HairTryOnResponseDto
    {
        public int Id { get; set; }
        public string FaceImageUrl { get; set; } = string.Empty;
        public string HairStyleImageUrl { get; set; } = string.Empty;
        public string? ResultImageUrl { get; set; }
        public string? HairStyleName { get; set; }
        public string? AiModel { get; set; }
        public bool? IsSaved { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ==========================================
    // CHAT SESSION DTOs
    // ==========================================

    /// <summary>
    /// Request để tạo chat session mới
    /// </summary>
    public class CreateChatSessionRequestDto
    {
        public string? Title { get; set; }
        public string SessionType { get; set; } = "General";
    }

    /// <summary>
    /// Request để gửi tin nhắn
    /// </summary>
    public class SendMessageRequestDto
    {
        [Required]
        public string Content { get; set; } = string.Empty;
        
        public string? ImageUrl { get; set; }
    }

    /// <summary>
    /// Response cho Chat Session
    /// </summary>
    public class ChatSessionResponseDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string SessionType { get; set; } = "General";
        public int MessageCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public List<ChatMessageResponseDto> Messages { get; set; } = new();
    }

    /// <summary>
    /// Response cho Chat Message
    /// </summary>
    public class ChatMessageResponseDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Role { get; set; } = "user"; // user, assistant
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ==========================================
    // AI HISTORY DTOs
    // ==========================================

    /// <summary>
    /// Response tổng hợp lịch sử AI của user
    /// </summary>
    public class AiHistoryResponseDto
    {
        public List<FaceAnalysisResponseDto> FaceAnalyses { get; set; } = new();
        public List<HairTryOnResponseDto> HairTryOns { get; set; } = new();
        public List<ChatSessionResponseDto> ChatSessions { get; set; } = new();
    }
}
