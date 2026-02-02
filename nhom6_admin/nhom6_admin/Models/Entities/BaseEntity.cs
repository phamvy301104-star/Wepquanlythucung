using System.ComponentModel.DataAnnotations;

namespace nhom6_admin.Models.Entities
{
    /// <summary>
    /// Base Entity class với các thuộc tính chung
    /// </summary>
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Ngày tạo record
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ngày cập nhật cuối cùng
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Trạng thái xóa mềm
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }
}
