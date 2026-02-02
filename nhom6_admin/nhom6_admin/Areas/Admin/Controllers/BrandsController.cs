using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Models;
using nhom6_admin.Models.Entities;

namespace nhom6_admin.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class BrandsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public BrandsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Index() => View();

        public IActionResult Create() => View(new BrandViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandViewModel model, IFormFile? imageFile, string? imageUrl)
        {
            if (ModelState.IsValid)
            {
                var brand = new Brand
                {
                    Name = model.Name,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    DisplayOrder = model.DisplayOrder,
                    CreatedAt = DateTime.UtcNow
                };

                if (imageFile != null && imageFile.Length > 0)
                    brand.LogoUrl = await UploadImage(imageFile);
                else if (!string.IsNullOrEmpty(imageUrl))
                    brand.LogoUrl = imageUrl;

                _context.Brands.Add(brand);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm thương hiệu thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null || brand.IsDeleted) return NotFound();

            var model = new BrandViewModel
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                LogoUrl = brand.LogoUrl,
                IsActive = brand.IsActive,
                DisplayOrder = brand.DisplayOrder
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BrandViewModel model, IFormFile? imageFile, string? imageUrl)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var brand = await _context.Brands.FindAsync(id);
                if (brand == null || brand.IsDeleted) return NotFound();

                brand.Name = model.Name;
                brand.Description = model.Description;
                brand.IsActive = model.IsActive;
                brand.DisplayOrder = model.DisplayOrder;
                brand.UpdatedAt = DateTime.UtcNow;

                if (imageFile != null && imageFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(brand.LogoUrl) && brand.LogoUrl.StartsWith("/uploads/"))
                        DeleteImage(brand.LogoUrl);
                    brand.LogoUrl = await UploadImage(imageFile);
                }
                else if (!string.IsNullOrEmpty(imageUrl) && imageUrl != model.LogoUrl)
                {
                    if (!string.IsNullOrEmpty(brand.LogoUrl) && brand.LogoUrl.StartsWith("/uploads/"))
                        DeleteImage(brand.LogoUrl);
                    brand.LogoUrl = imageUrl;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thương hiệu thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetData(int draw, int start, int length, string? search)
        {
            var query = _context.Brands.Where(b => !b.IsDeleted).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(b => b.Name.ToLower().Contains(search.ToLower()));

            var totalRecords = await _context.Brands.CountAsync(b => !b.IsDeleted);
            var filteredRecords = await query.CountAsync();

            var brands = await query
                .OrderBy(b => b.DisplayOrder).ThenBy(b => b.Name)
                .Skip(start).Take(length)
                .Select(b => new
                {
                    b.Id,
                    b.Name,
                    b.LogoUrl,
                    b.Description,
                    b.IsActive,
                    b.DisplayOrder,
                    ProductCount = b.Products!.Count(p => !p.IsDeleted)
                })
                .ToListAsync();

            return Json(new { draw, recordsTotal = totalRecords, recordsFiltered = filteredRecords, data = brands });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return Json(new { success = false, message = "Không tìm thấy" });

            brand.IsDeleted = true;
            brand.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return Json(new { success = false });

            brand.IsActive = !brand.IsActive;
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = brand.IsActive });
        }

        private async Task<string> UploadImage(IFormFile file)
        {
            var folder = Path.Combine(_environment.WebRootPath, "uploads", "brands");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var path = Path.Combine(folder, fileName);
            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/brands/{fileName}";
        }

        private void DeleteImage(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            var path = Path.Combine(_environment.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }
    }

    public class BrandViewModel
    {
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập tên")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Tên thương hiệu")]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Display(Name = "Mô tả")]
        public string? Description { get; set; }

        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }
}
