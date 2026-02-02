using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Đánh giá sản phẩm từ khách hàng
    /// </summary>
    public class ProductReview : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Product
        /// </summary>
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        /// <summary>
        /// Khóa ngoại đến User
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Khóa ngoại đến OrderItem (chứng minh đã mua)
        /// </summary>
        public int? OrderItemId { get; set; }

        [ForeignKey("OrderItemId")]
        public virtual OrderItem? OrderItem { get; set; }

        /// <summary>
        /// Điểm đánh giá (1-5 sao)
        /// </summary>
        [Range(1, 5)]
        public int Rating { get; set; }

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
        /// Ưu điểm
        /// </summary>
        [MaxLength(500)]
        public string? Pros { get; set; }

        /// <summary>
        /// Nhược điểm
        /// </summary>
        [MaxLength(500)]
        public string? Cons { get; set; }

        /// <summary>
        /// Hình ảnh đánh giá (JSON array)
        /// </summary>
        public string? ReviewImages { get; set; }

        /// <summary>
        /// Video đánh giá URL
        /// </summary>
        [MaxLength(500)]
        public string? VideoUrl { get; set; }

        /// <summary>
        /// Đã mua sản phẩm (verified purchase)
        /// </summary>
        public bool IsVerifiedPurchase { get; set; } = false;

        /// <summary>
        /// Số lượt thấy hữu ích
        /// </summary>
        public int HelpfulCount { get; set; } = 0;

        /// <summary>
        /// Số lượt không hữu ích
        /// </summary>
        public int NotHelpfulCount { get; set; } = 0;

        /// <summary>
        /// Trạng thái: Pending, Approved, Rejected
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Nguyên nhân từ chối
        /// </summary>
        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        /// <summary>
        /// Admin phê duyệt
        /// </summary>
        public string? ApprovedByUserId { get; set; }

        /// <summary>
        /// Ngày phê duyệt
        /// </summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// Phản hồi từ admin/store
        /// </summary>
        [MaxLength(1000)]
        public string? AdminReply { get; set; }

        /// <summary>
        /// Ngày phản hồi
        /// </summary>
        public DateTime? AdminReplyAt { get; set; }

        /// <summary>
        /// Đánh giá ẩn danh
        /// </summary>
        public bool IsAnonymous { get; set; } = false;
    }
}
