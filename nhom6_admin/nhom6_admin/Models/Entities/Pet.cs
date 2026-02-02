using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom6_admin.Models.Entities
{
    /// <summary>
    /// Thú cưng đang bán hoặc được quản lý
    /// </summary>
    public class Pet : BaseEntity
    {
        /// <summary>
        /// Mã thú cưng (unique)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PetCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên thú cưng
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Loại thú cưng (Chó, Mèo, Hamster, etc.)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Species { get; set; } = string.Empty;

        /// <summary>
        /// Giống (Corgi, Poodle, British Shorthair, etc.)
        /// </summary>
        [MaxLength(100)]
        public string? Breed { get; set; }

        /// <summary>
        /// Giới tính (Male/Female/Unknown)
        /// </summary>
        [MaxLength(20)]
        public string? Gender { get; set; }

        /// <summary>
        /// Ngày sinh (ước tính)
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Tuổi (tính theo tháng)
        /// </summary>
        public int? AgeInMonths { get; set; }

        /// <summary>
        /// Màu lông
        /// </summary>
        [MaxLength(100)]
        public string? Color { get; set; }

        /// <summary>
        /// Cân nặng (kg)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Weight { get; set; }

        /// <summary>
        /// Mô tả chi tiết
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Tình trạng sức khỏe
        /// </summary>
        [MaxLength(500)]
        public string? HealthStatus { get; set; }

        /// <summary>
        /// Đã tiêm vaccine chưa
        /// </summary>
        public bool IsVaccinated { get; set; } = false;

        /// <summary>
        /// Chi tiết vaccine đã tiêm
        /// </summary>
        [MaxLength(500)]
        public string? VaccinationDetails { get; set; }

        /// <summary>
        /// Đã triệt sản chưa
        /// </summary>
        public bool IsNeutered { get; set; } = false;

        /// <summary>
        /// Đã có microchip chưa
        /// </summary>
        public bool HasMicrochip { get; set; } = false;

        /// <summary>
        /// Số microchip
        /// </summary>
        [MaxLength(50)]
        public string? MicrochipNumber { get; set; }

        /// <summary>
        /// Hình ảnh chính
        /// </summary>
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Danh sách hình ảnh phụ (JSON array)
        /// </summary>
        public string? AdditionalImages { get; set; }

        /// <summary>
        /// Video URL
        /// </summary>
        [MaxLength(500)]
        public string? VideoUrl { get; set; }

        /// <summary>
        /// Giá bán
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// Giá gốc (nếu có giảm giá)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? OriginalPrice { get; set; }

        /// <summary>
        /// Trạng thái: Available, Sold, Reserved, NotForSale
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Available";

        /// <summary>
        /// Đang bán hay chỉ lưu trữ
        /// </summary>
        public bool IsForSale { get; set; } = true;

        /// <summary>
        /// Hiển thị trên trang chủ
        /// </summary>
        public bool IsFeatured { get; set; } = false;

        /// <summary>
        /// Nguồn gốc (nhập từ đâu, trang trại nào)
        /// </summary>
        [MaxLength(200)]
        public string? Origin { get; set; }

        /// <summary>
        /// Ngày nhập về
        /// </summary>
        public DateTime? ArrivalDate { get; set; }

        /// <summary>
        /// Ngày bán
        /// </summary>
        public DateTime? SoldDate { get; set; }

        /// <summary>
        /// ID người mua (nếu đã bán)
        /// </summary>
        [MaxLength(450)]
        public string? BuyerId { get; set; }

        /// <summary>
        /// Tên người mua
        /// </summary>
        [MaxLength(200)]
        public string? BuyerName { get; set; }

        /// <summary>
        /// SĐT người mua
        /// </summary>
        [MaxLength(20)]
        public string? BuyerPhone { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Số lượt xem
        /// </summary>
        public int ViewCount { get; set; } = 0;

        /// <summary>
        /// Loại đăng: Sale (Bán) hoặc Adoption (Nhận nuôi miễn phí)
        /// </summary>
        [MaxLength(20)]
        public string ListingType { get; set; } = "Sale";

        /// <summary>
        /// Phí nhận nuôi (nếu có, cho trường hợp Adoption)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? AdoptionFee { get; set; }

        /// <summary>
        /// QR Code data (JSON chứa thông tin thú cưng)
        /// </summary>
        public string? QRCodeData { get; set; }
    }
}
