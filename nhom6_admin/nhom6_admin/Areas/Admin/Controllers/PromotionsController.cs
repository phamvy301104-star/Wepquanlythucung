using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nhom6_admin.Models;
using nhom6_admin.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace nhom6_admin.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class PromotionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PromotionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Promotions
        public IActionResult Index()
        {
            return View();
        }

        // GET: Admin/Promotions/GetData
        [HttpGet]
        public IActionResult GetData(string? search, string? status, int page = 1, int pageSize = 10)
        {
            var query = _context.Promotions.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Code.Contains(search));
            }

            if (!string.IsNullOrEmpty(status))
            {
                var now = DateTime.Now;
                if (status == "active")
                    query = query.Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now);
                else if (status == "expired")
                    query = query.Where(p => p.EndDate < now);
                else if (status == "upcoming")
                    query = query.Where(p => p.StartDate > now);
                else if (status == "inactive")
                    query = query.Where(p => !p.IsActive);
            }

            var total = query.Count();
            var data = query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Code,
                    p.Name,
                    p.Description,
                    p.DiscountType,
                    p.DiscountValue,
                    p.MinOrderValue,
                    p.MaxDiscountAmount,
                    StartDate = p.StartDate.ToString("dd/MM/yyyy"),
                    EndDate = p.EndDate.ToString("dd/MM/yyyy"),
                    p.UsageLimit,
                    p.UsedCount,
                    p.IsActive,
                    Status = GetPromotionStatus(p)
                })
                .ToList();

            return Json(new { data, total, page, pageSize, totalPages = (int)Math.Ceiling(total / (double)pageSize) });
        }

        private static string GetPromotionStatus(Promotion p)
        {
            var now = DateTime.Now;
            if (!p.IsActive) return "inactive";
            if (p.EndDate < now) return "expired";
            if (p.StartDate > now) return "upcoming";
            if (p.UsageLimit > 0 && p.UsedCount >= p.UsageLimit) return "exhausted";
            return "active";
        }

        // GET: Admin/Promotions/Create
        public IActionResult Create()
        {
            return View(new PromotionViewModel());
        }

        // POST: Admin/Promotions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PromotionViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if code exists
                if (_context.Promotions.Any(p => p.Code == model.Code))
                {
                    ModelState.AddModelError("Code", "Mã khuyến mãi đã tồn tại");
                    return View(model);
                }

                var promotion = new Promotion
                {
                    Code = model.Code.ToUpper(),
                    Name = model.Name,
                    Description = model.Description,
                    DiscountType = model.DiscountType,
                    DiscountValue = model.DiscountValue,
                    MinOrderValue = model.MinOrderValue,
                    MaxDiscountAmount = model.MaxDiscountAmount,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    UsageLimit = model.UsageLimit,
                    UsedCount = 0,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now
                };

                _context.Promotions.Add(promotion);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tạo khuyến mãi thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Admin/Promotions/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return NotFound();

            var model = new PromotionViewModel
            {
                Id = promotion.Id,
                Code = promotion.Code,
                Name = promotion.Name,
                Description = promotion.Description,
                DiscountType = promotion.DiscountType,
                DiscountValue = promotion.DiscountValue,
                MinOrderValue = promotion.MinOrderValue,
                MaxDiscountAmount = promotion.MaxDiscountAmount,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                UsageLimit = promotion.UsageLimit,
                IsActive = promotion.IsActive
            };

            return View(model);
        }

        // POST: Admin/Promotions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PromotionViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var promotion = await _context.Promotions.FindAsync(id);
                if (promotion == null) return NotFound();

                // Check if code exists (except current)
                if (_context.Promotions.Any(p => p.Code == model.Code && p.Id != id))
                {
                    ModelState.AddModelError("Code", "Mã khuyến mãi đã tồn tại");
                    return View(model);
                }

                promotion.Code = model.Code.ToUpper();
                promotion.Name = model.Name;
                promotion.Description = model.Description;
                promotion.DiscountType = model.DiscountType;
                promotion.DiscountValue = model.DiscountValue;
                promotion.MinOrderValue = model.MinOrderValue;
                promotion.MaxDiscountAmount = model.MaxDiscountAmount;
                promotion.StartDate = model.StartDate;
                promotion.EndDate = model.EndDate;
                promotion.UsageLimit = model.UsageLimit;
                promotion.IsActive = model.IsActive;
                promotion.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật khuyến mãi thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: Admin/Promotions/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
                return Json(new { success = false, message = "Không tìm thấy khuyến mãi" });

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Xóa khuyến mãi thành công" });
        }

        // POST: Admin/Promotions/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
                return Json(new { success = false, message = "Không tìm thấy khuyến mãi" });

            promotion.IsActive = !promotion.IsActive;
            promotion.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isActive = promotion.IsActive, message = promotion.IsActive ? "Đã kích hoạt" : "Đã tắt" });
        }

        // View Model
        public class PromotionViewModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập mã khuyến mãi")]
            [StringLength(50)]
            [Display(Name = "Mã khuyến mãi")]
            public string Code { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập tên khuyến mãi")]
            [StringLength(200)]
            [Display(Name = "Tên khuyến mãi")]
            public string Name { get; set; } = string.Empty;

            [Display(Name = "Mô tả")]
            public string? Description { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn loại giảm giá")]
            [Display(Name = "Loại giảm giá")]
            public string DiscountType { get; set; } = "Percentage"; // Percentage, FixedAmount

            [Required(ErrorMessage = "Vui lòng nhập giá trị giảm")]
            [Range(0, double.MaxValue)]
            [Display(Name = "Giá trị giảm")]
            public decimal DiscountValue { get; set; }

            [Display(Name = "Đơn hàng tối thiểu")]
            public decimal MinOrderValue { get; set; }

            [Display(Name = "Giảm tối đa")]
            public decimal? MaxDiscountAmount { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
            [Display(Name = "Ngày bắt đầu")]
            [DataType(DataType.Date)]
            public DateTime StartDate { get; set; } = DateTime.Today;

            [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
            [Display(Name = "Ngày kết thúc")]
            [DataType(DataType.Date)]
            public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(1);

            [Display(Name = "Giới hạn sử dụng")]
            public int UsageLimit { get; set; }

            [Display(Name = "Kích hoạt")]
            public bool IsActive { get; set; } = true;
        }
    }
}
