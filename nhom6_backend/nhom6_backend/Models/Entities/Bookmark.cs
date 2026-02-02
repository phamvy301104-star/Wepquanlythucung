using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Bookmark (lưu bài viết)
    /// </summary>
    public class Bookmark : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Post
        /// </summary>
        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post? Post { get; set; }

        /// <summary>
        /// Khóa ngoại đến User
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Folder/Collection name
        /// </summary>
        [MaxLength(100)]
        public string? CollectionName { get; set; }

        /// <summary>
        /// Ghi chú cá nhân
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
