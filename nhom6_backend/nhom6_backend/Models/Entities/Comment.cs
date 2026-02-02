using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Bình luận bài viết
    /// </summary>
    public class Comment : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Post
        /// </summary>
        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post? Post { get; set; }

        /// <summary>
        /// Khóa ngoại đến User (người comment)
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Comment cha (nếu là reply)
        /// </summary>
        public int? ParentCommentId { get; set; }

        [ForeignKey("ParentCommentId")]
        public virtual Comment? ParentComment { get; set; }

        /// <summary>
        /// Nội dung comment
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Hình ảnh đính kèm (JSON array)
        /// </summary>
        public string? Images { get; set; }

        /// <summary>
        /// GIF URL
        /// </summary>
        [MaxLength(500)]
        public string? GifUrl { get; set; }

        /// <summary>
        /// Số lượt thích
        /// </summary>
        public int LikeCount { get; set; } = 0;

        /// <summary>
        /// Số lượt trả lời
        /// </summary>
        public int ReplyCount { get; set; } = 0;

        /// <summary>
        /// Đã chỉnh sửa
        /// </summary>
        public bool IsEdited { get; set; } = false;

        /// <summary>
        /// Ngày chỉnh sửa
        /// </summary>
        public DateTime? EditedAt { get; set; }

        /// <summary>
        /// Bị ẩn
        /// </summary>
        public bool IsHidden { get; set; } = false;

        /// <summary>
        /// Bị report
        /// </summary>
        public bool IsReported { get; set; } = false;

        /// <summary>
        /// Số lần bị report
        /// </summary>
        public int ReportCount { get; set; } = 0;

        /// <summary>
        /// Được pin (bởi tác giả bài viết)
        /// </summary>
        public bool IsPinned { get; set; } = false;

        /// <summary>
        /// Lý do ẩn
        /// </summary>
        [MaxLength(500)]
        public string? HiddenReason { get; set; }

        // Navigation Properties
        public virtual ICollection<Comment>? Replies { get; set; }
        public virtual ICollection<CommentLike>? CommentLikes { get; set; }
    }
}
