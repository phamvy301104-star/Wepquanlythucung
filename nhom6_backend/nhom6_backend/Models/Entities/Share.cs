using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Share bài viết
    /// </summary>
    public class Share : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Post được share
        /// </summary>
        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post? Post { get; set; }

        /// <summary>
        /// Khóa ngoại đến User share
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Khóa ngoại đến Post mới (bài share)
        /// </summary>
        public int? SharedPostId { get; set; }

        [ForeignKey("SharedPostId")]
        public virtual Post? SharedPost { get; set; }

        /// <summary>
        /// Nơi share: Timeline, Group, Messenger, External
        /// </summary>
        [MaxLength(20)]
        public string ShareType { get; set; } = "Timeline";

        /// <summary>
        /// Caption khi share
        /// </summary>
        [MaxLength(1000)]
        public string? Caption { get; set; }
    }
}
