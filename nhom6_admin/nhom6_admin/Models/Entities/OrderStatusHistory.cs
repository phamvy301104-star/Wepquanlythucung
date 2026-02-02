using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_admin.Models.Entities
{
    /// <summary>
    /// Lịch sử trạng thái đơn hàng
    /// </summary>
    public class OrderStatusHistory : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Order
        /// </summary>
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        /// <summary>
        /// Trạng thái trước
        /// </summary>
        [MaxLength(20)]
        public string? FromStatus { get; set; }

        /// <summary>
        /// Trạng thái sau
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string ToStatus { get; set; } = string.Empty;

        /// <summary>
        /// Ghi chú
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Người thay đổi (User ID hoặc System)
        /// </summary>
        [MaxLength(50)]
        public string? ChangedBy { get; set; }

        /// <summary>
        /// Loại người thay đổi: Admin, Staff, Customer, System
        /// </summary>
        [MaxLength(20)]
        public string? ChangedByType { get; set; }
    }
}
