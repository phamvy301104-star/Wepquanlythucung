using System.ComponentModel.DataAnnotations;

namespace nhom6_admin.Models.Entities
{
    public class SiteSetting
    {
        [Key]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
