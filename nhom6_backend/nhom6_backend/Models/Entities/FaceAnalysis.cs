using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Kết quả phân tích khuôn mặt từ AI
    /// </summary>
    public class FaceAnalysis : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến User
        /// </summary>
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Khóa ngoại đến ChatSession (nếu phân tích qua chatbot)
        /// </summary>
        public int? ChatSessionId { get; set; }

        [ForeignKey("ChatSessionId")]
        public virtual ChatSession? ChatSession { get; set; }

        /// <summary>
        /// URL ảnh gốc
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string OriginalImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// URL ảnh đã xử lý (có đánh dấu face landmarks)
        /// </summary>
        [MaxLength(500)]
        public string? ProcessedImageUrl { get; set; }

        /// <summary>
        /// Hình dáng khuôn mặt: Oval, Round, Square, Heart, Oblong, Diamond, Rectangle
        /// </summary>
        [MaxLength(30)]
        public string? FaceShape { get; set; }

        /// <summary>
        /// Confidence score cho FaceShape
        /// </summary>
        public decimal? FaceShapeConfidence { get; set; }

        /// <summary>
        /// Kiểu trán: High, Low, Wide, Narrow
        /// </summary>
        [MaxLength(30)]
        public string? ForeheadType { get; set; }

        /// <summary>
        /// Kiểu mắt: Round, Almond, Hooded, Monolid, etc.
        /// </summary>
        [MaxLength(30)]
        public string? EyeShape { get; set; }

        /// <summary>
        /// Kiểu mũi: Straight, Aquiline, Button, etc.
        /// </summary>
        [MaxLength(30)]
        public string? NoseShape { get; set; }

        /// <summary>
        /// Kiểu môi: Thin, Full, Wide, etc.
        /// </summary>
        [MaxLength(30)]
        public string? LipShape { get; set; }

        /// <summary>
        /// Kiểu cằm: Pointed, Square, Round, etc.
        /// </summary>
        [MaxLength(30)]
        public string? ChinShape { get; set; }

        /// <summary>
        /// Màu da dự đoán: Light, Medium, Olive, Dark
        /// </summary>
        [MaxLength(30)]
        public string? SkinTone { get; set; }

        /// <summary>
        /// Giới tính dự đoán
        /// </summary>
        [MaxLength(10)]
        public string? PredictedGender { get; set; }

        /// <summary>
        /// Độ tuổi dự đoán
        /// </summary>
        public int? PredictedAge { get; set; }

        /// <summary>
        /// Face landmarks (JSON) - các điểm đánh dấu trên mặt
        /// </summary>
        public string? FaceLandmarks { get; set; }

        /// <summary>
        /// Tỷ lệ khuôn mặt (JSON) - chiều dài/chiều rộng, etc.
        /// </summary>
        public string? FaceProportions { get; set; }

        /// <summary>
        /// Kiểu tóc được gợi ý (JSON array)
        /// </summary>
        public string? SuggestedHairStyles { get; set; }

        /// <summary>
        /// Lý do gợi ý (JSON) - giải thích tại sao gợi ý kiểu tóc đó
        /// </summary>
        public string? SuggestionReasons { get; set; }

        /// <summary>
        /// Dịch vụ được gợi ý (JSON array of service IDs)
        /// </summary>
        public string? SuggestedServiceIds { get; set; }

        /// <summary>
        /// Raw API response (JSON) - để debug
        /// </summary>
        public string? RawApiResponse { get; set; }

        /// <summary>
        /// Model AI đã sử dụng
        /// </summary>
        [MaxLength(50)]
        public string? AiModel { get; set; }

        /// <summary>
        /// Thời gian xử lý (ms)
        /// </summary>
        public int? ProcessingTimeMs { get; set; }

        /// <summary>
        /// Trạng thái: Processing, Completed, Failed
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Processing";

        /// <summary>
        /// Lỗi (nếu có)
        /// </summary>
        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Feedback từ user
        /// </summary>
        public bool? IsAccurate { get; set; }

        /// <summary>
        /// Ghi chú feedback
        /// </summary>
        [MaxLength(500)]
        public string? FeedbackNotes { get; set; }
    }
}
