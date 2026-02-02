using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_backend.Models.Entities
{
    /// <summary>
    /// Danh sách yêu thích của người dùng
    /// </summary>
    public class Wishlist : BaseEntity
    {
        /// <summary>
        /// Khóa ngoại đến User
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Khóa ngoại đến Product
        /// </summary>
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        /// <summary>
        /// Ghi chú cá nhân
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Thông báo khi giảm giá
        /// </summary>
        public bool NotifyOnSale { get; set; } = false;

        /// <summary>
        /// Thông báo khi có hàng
        /// </summary>
        public bool NotifyOnStock { get; set; } = false;

        /// <summary>
        /// Mức độ ưu tiên: 1-5 (1: thấp, 5: cao)
        /// </summary>
        public int Priority { get; set; } = 3;
    }
}
