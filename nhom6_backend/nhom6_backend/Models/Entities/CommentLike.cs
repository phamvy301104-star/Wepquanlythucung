using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Like comment
    /// </summary>
    public class CommentLike : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Comment
        /// </summary>
        public int CommentId { get; set; }

        [ForeignKey("CommentId")]
        public virtual Comment? Comment { get; set; }

        /// <summary>
        /// Khóa ngoại đến User
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Loại reaction
        /// </summary>
        [MaxLength(20)]
        public string ReactionType { get; set; } = "Like";
    }
}
