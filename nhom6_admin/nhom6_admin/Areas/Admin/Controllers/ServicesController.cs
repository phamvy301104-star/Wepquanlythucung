using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Models;
using nhom6_admin.Models.Entities;

namespace nhom6_admin.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ServicesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Index() => View();

        public async Task<IActionResult> Create()
        {
            await LoadSelectLists();
            return View(new ServiceViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceViewModel model, IFormFile? imageFile, string? imageUrl)
        {
            if (ModelState.IsValid)
            {
                var service = new Service
                {
                    ServiceCode = $"SVC{DateTime.Now:yyyyMMddHHmmss}",
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    OriginalPrice = model.OriginalPrice ?? model.Price,
                    DurationMinutes = model.Duration,
                    IsActive = model.IsActive,
                    IsFeatured = model.IsFeatured,
                    CreatedAt = DateTime.UtcNow
                };

                // Upload image file hoặc sử dụng URL
                if (imageFile != null && imageFile.Length > 0)
                    service.ImageUrl = await UploadImage(imageFile);
                else if (!string.IsNullOrEmpty(imageUrl))
                    service.ImageUrl = imageUrl;

                _context.Services.Add(service);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm dịch vụ thành công!";
                return RedirectToAction(nameof(Index));
            }
            await LoadSelectLists();
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null || service.IsDeleted) return NotFound();

            var model = new ServiceViewModel
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                ImageUrl = service.ImageUrl,
                Price = service.Price,
                OriginalPrice = service.OriginalPrice,
                Duration = service.DurationMinutes,
                IsActive = service.IsActive,
                IsFeatured = service.IsFeatured
            };

            await LoadSelectLists();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceViewModel model, IFormFile? imageFile, string? imageUrl)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var service = await _context.Services.FindAsync(id);
                if (service == null || service.IsDeleted) return NotFound();

                service.Name = model.Name;
                service.Description = model.Description;
                service.Price = model.Price;
                service.OriginalPrice = model.OriginalPrice ?? model.Price;
                service.DurationMinutes = model.Duration;
                service.IsActive = model.IsActive;
                service.IsFeatured = model.IsFeatured;
                service.UpdatedAt = DateTime.UtcNow;

                // Upload image file hoặc sử dụng URL
                if (imageFile != null && imageFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(service.ImageUrl) && service.ImageUrl.StartsWith("/uploads/"))
                        DeleteImage(service.ImageUrl);
                    service.ImageUrl = await UploadImage(imageFile);
                }
                else if (!string.IsNullOrEmpty(imageUrl))
                {
                    // Chỉ update nếu URL khác với URL hiện tại trong database
                    if (imageUrl != service.ImageUrl)
                    {
                        if (!string.IsNullOrEmpty(service.ImageUrl) && service.ImageUrl.StartsWith("/uploads/"))
                            DeleteImage(service.ImageUrl);
                        service.ImageUrl = imageUrl;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật dịch vụ thành công!";
                return RedirectToAction(nameof(Index));
            }
            await LoadSelectLists();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetData(int draw, int start, int length, string? search, int? categoryId)
        {
            var query = _context.Services
                .Where(s => !s.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(s => s.Name.ToLower().Contains(search.ToLower()));

            var totalRecords = await _context.Services.CountAsync(s => !s.IsDeleted);
            var filteredRecords = await query.CountAsync();

            var services = await query
                .OrderBy(s => s.Name)
                .Skip(start).Take(length)
                .Select(s => new
                {
                    s.Id, s.Name, s.ImageUrl, s.Price, s.OriginalPrice, 
                    Duration = s.DurationMinutes,
                    s.IsActive, s.IsFeatured, s.AverageRating,
                    BookingCount = s.AppointmentServices!.Count
                })
                .ToListAsync();

            return Json(new { draw, recordsTotal = totalRecords, recordsFiltered = filteredRecords, data = services });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return Json(new { success = false, message = "Dịch vụ không tồn tại" });

            service.IsDeleted = true;
            service.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return Json(new { success = false });

            service.IsActive = !service.IsActive;
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = service.IsActive });
        }

        private async Task LoadSelectLists()
        {
            ViewBag.Categories = await _context.ServiceCategories
                .Where(c => !c.IsDeleted && c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();
        }

        private async Task<string> UploadImage(IFormFile file)
        {
            var folder = Path.Combine(_environment.WebRootPath, "uploads", "services");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var path = Path.Combine(folder, fileName);
            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/services/{fileName}";
        }

        private void DeleteImage(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            var path = Path.Combine(_environment.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }

        public class ServiceViewModel
        {
            public int Id { get; set; }

            [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập tên dịch vụ")]
            [System.ComponentModel.DataAnnotations.Display(Name = "Tên dịch vụ")]
            public string Name { get; set; } = string.Empty;

            [System.ComponentModel.DataAnnotations.Display(Name = "Mô tả")]
            public string? Description { get; set; }

            public string? ImageUrl { get; set; }
            public string? ExistingImage { get; set; }

            [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập giá")]
            [System.ComponentModel.DataAnnotations.Display(Name = "Giá dịch vụ")]
            public decimal Price { get; set; }

            [System.ComponentModel.DataAnnotations.Display(Name = "Giá gốc")]
            public decimal? OriginalPrice { get; set; }

            [System.ComponentModel.DataAnnotations.Display(Name = "Thời gian (phút)")]
            public int Duration { get; set; } = 30;

            [System.ComponentModel.DataAnnotations.Display(Name = "Danh mục")]
            public int? CategoryId { get; set; }

            public int SortOrder { get; set; }
            public bool IsActive { get; set; } = true;
            public bool IsFeatured { get; set; }
        }
    }
}
