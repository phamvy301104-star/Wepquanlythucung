using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Bài viết trên Blog/Diễn đàn
    /// </summary>
    public class Post : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến User (tác giả)
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Tiêu đề bài viết
        /// </summary>
        [MaxLength(300)]
        public string? Title { get; set; }

        /// <summary>
        /// Slug URL-friendly
        /// </summary>
        [MaxLength(350)]
        public string? Slug { get; set; }

        /// <summary>
        /// Nội dung bài viết (HTML/Markdown)
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Nội dung thuần text (để tìm kiếm)
        /// </summary>
        public string? PlainTextContent { get; set; }

        /// <summary>
        /// Loại bài viết: Status (ngắn), Article (dài), Question, Poll
        /// </summary>
        [MaxLength(20)]
        public string PostType { get; set; } = "Status";

        /// <summary>
        /// Quyền xem: Public, Followers, Private
        /// </summary>
        [MaxLength(20)]
        public string Visibility { get; set; } = "Public";

        /// <summary>
        /// Số lượt xem
        /// </summary>
        public int ViewCount { get; set; } = 0;

        /// <summary>
        /// Số lượt thích
        /// </summary>
        public int LikeCount { get; set; } = 0;

        /// <summary>
        /// Số lượt comment
        /// </summary>
        public int CommentCount { get; set; } = 0;

        /// <summary>
        /// Số lượt share
        /// </summary>
        public int ShareCount { get; set; } = 0;

        /// <summary>
        /// Số lượt bookmark (lưu)
        /// </summary>
        public int BookmarkCount { get; set; } = 0;

        /// <summary>
        /// Cho phép comment
        /// </summary>
        public bool AllowComments { get; set; } = true;

        /// <summary>
        /// Bài viết đã được pin
        /// </summary>
        public bool IsPinned { get; set; } = false;

        /// <summary>
        /// Bài viết nổi bật (do admin chọn)
        /// </summary>
        public bool IsFeatured { get; set; } = false;

        /// <summary>
        /// Trạng thái: Draft, Published, Archived, Hidden
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Published";

        /// <summary>
        /// Ngày đăng
        /// </summary>
        public DateTime? PublishedAt { get; set; }

        /// <summary>
        /// Ngày chỉnh sửa cuối
        /// </summary>
        public DateTime? EditedAt { get; set; }

        /// <summary>
        /// Tags (JSON array)
        /// </summary>
        [MaxLength(500)]
        public string? Tags { get; set; }

        /// <summary>
        /// Hashtags (JSON array)
        /// </summary>
        [MaxLength(500)]
        public string? Hashtags { get; set; }

        /// <summary>
        /// Location (nếu có check-in)
        /// </summary>
        [MaxLength(200)]
        public string? Location { get; set; }

        /// <summary>
        /// Feeling/Activity (đang cảm thấy...)
        /// </summary>
        [MaxLength(100)]
        public string? Feeling { get; set; }

        /// <summary>
        /// ID bài viết được share (nếu là share post)
        /// </summary>
        public int? OriginalPostId { get; set; }

        [ForeignKey("OriginalPostId")]
        public virtual Post? OriginalPost { get; set; }

        /// <summary>
        /// Bị report
        /// </summary>
        public bool IsReported { get; set; } = false;

        /// <summary>
        /// Số lần bị report
        /// </summary>
        public int ReportCount { get; set; } = 0;

        /// <summary>
        /// Bị ẩn do vi phạm
        /// </summary>
        public bool IsHiddenByAdmin { get; set; } = false;

        /// <summary>
        /// Lý do ẩn
        /// </summary>
        [MaxLength(500)]
        public string? HiddenReason { get; set; }

        // Navigation Properties
        public virtual ICollection<PostImage>? PostImages { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ICollection<Like>? Likes { get; set; }
        public virtual ICollection<Share>? Shares { get; set; }
        public virtual ICollection<Bookmark>? Bookmarks { get; set; }
        public virtual ICollection<Post>? SharedPosts { get; set; }
    }
}
