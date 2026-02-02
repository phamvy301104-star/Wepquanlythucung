using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_admin.Models.Entities
{
    public class Review : BaseEntity
    {
        public int? ProductId { get; set; }
        public int? ServiceId { get; set; }
        
        [StringLength(450)]
        public string? CustomerId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public string Comment { get; set; } = string.Empty;

        public string? AdminReply { get; set; }

        public DateTime? RepliedAt { get; set; }

        public bool IsApproved { get; set; }

        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }

        [ForeignKey("CustomerId")]
        public virtual User? Customer { get; set; }
    }
}
