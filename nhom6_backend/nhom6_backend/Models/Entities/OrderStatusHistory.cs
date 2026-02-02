using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
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
        /// Trạng thái cũ
        /// </summary>
        [MaxLength(20)]
        public string? FromStatus { get; set; }

        /// <summary>
        /// Trạng thái mới
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
        /// Người thay đổi
        /// </summary>
        public string? ChangedByUserId { get; set; }

        [ForeignKey("ChangedByUserId")]
        public virtual User? ChangedByUser { get; set; }

        /// <summary>
        /// Vị trí (cho theo dõi vận chuyển)
        /// </summary>
        [MaxLength(200)]
        public string? Location { get; set; }

        /// <summary>
        /// Tự động thay đổi (bởi hệ thống)
        /// </summary>
        public bool IsAutomatic { get; set; } = false;
    }
}
