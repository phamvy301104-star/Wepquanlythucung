using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Item trong giỏ hàng
    /// </summary>
    public class CartItem : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến Cart
        /// </summary>
        public int CartId { get; set; }

        [ForeignKey("CartId")]
        public virtual Cart? Cart { get; set; }

        /// <summary>
        /// Khóa ngoại đến Product
        /// </summary>
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        /// <summary>
        /// Khóa ngoại đến ProductVariant (nếu có)
        /// </summary>
        public int? ProductVariantId { get; set; }

        [ForeignKey("ProductVariantId")]
        public virtual ProductVariant? ProductVariant { get; set; }

        /// <summary>
        /// Số lượng
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Giá tại thời điểm thêm vào giỏ
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Tổng tiền (Quantity * UnitPrice)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Đã chọn để thanh toán
        /// </summary>
        public bool IsSelected { get; set; } = true;

        /// <summary>
        /// Ghi chú
        /// </summary>
        [MaxLength(300)]
        public string? Notes { get; set; }

        /// <summary>
        /// Đã lưu để mua sau
        /// </summary>
        public bool SavedForLater { get; set; } = false;
    }
}
