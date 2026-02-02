using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_admin.Models.Entities
{
    /// <summary>
    /// Quan hệ nhân viên - dịch vụ (nhân viên có thể thực hiện dịch vụ nào)
    /// </summary>
    public class StaffService : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Staff
        /// </summary>
        public int StaffId { get; set; }

        [ForeignKey("StaffId")]
        public virtual Staff? Staff { get; set; }

        /// <summary>
        /// Khóa ngoại đến Service
        /// </summary>
        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }

        /// <summary>
        /// Cấp độ thực hiện dịch vụ
        /// </summary>
        [MaxLength(20)]
        public string? ProficiencyLevel { get; set; }

        /// <summary>
        /// Có hiệu lực từ ngày
        /// </summary>
        public DateTime? EffectiveFrom { get; set; }

        /// <summary>
        /// Hết hiệu lực ngày
        /// </summary>
        public DateTime? EffectiveTo { get; set; }
    }
}
