using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Dịch vụ mà nhân viên có thể thực hiện (Many-to-Many)
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
        /// Mức độ thành thạo: 1-5
        /// </summary>
        public int ProficiencyLevel { get; set; } = 3;

        /// <summary>
        /// Giá riêng của nhân viên (nếu khác giá chuẩn)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? CustomPrice { get; set; }

        /// <summary>
        /// Thời gian thực hiện riêng (phút)
        /// </summary>
        public int? CustomDurationMinutes { get; set; }

        /// <summary>
        /// Số lượt đã thực hiện dịch vụ này
        /// </summary>
        public int TotalPerformed { get; set; } = 0;

        /// <summary>
        /// Trạng thái active
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
