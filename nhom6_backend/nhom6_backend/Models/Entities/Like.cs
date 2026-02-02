using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Like bài viết
    /// </summary>
    public class Like : BaseEntity
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
        /// Loại reaction: Like, Love, Haha, Wow, Sad, Angry
        /// </summary>
        [MaxLength(20)]
        public string ReactionType { get; set; } = "Like";
    }
}
