using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Models;
using nhom6_admin.Models.Entities;

namespace nhom6_admin.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CategoriesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Admin/Categories
        public IActionResult Index()
        {
            return View();
        }

        // GET: Admin/Categories/Create
        public IActionResult Create()
        {
            return View(new CategoryViewModel());
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                var category = new Category
                {
                    Name = model.Name,
                    Description = model.Description,
                    DisplayOrder = model.DisplayOrder,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                if (imageFile != null && imageFile.Length > 0)
                {
                    category.ImageUrl = await UploadImage(imageFile);
                }

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null || category.IsDeleted)
            {
                return NotFound();
            }

            var model = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive
            };

            return View(model);
        }

        // POST: Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryViewModel model, IFormFile? imageFile)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null || category.IsDeleted)
                {
                    return NotFound();
                }

                category.Name = model.Name;
                category.Description = model.Description;
                category.DisplayOrder = model.DisplayOrder;
                category.IsActive = model.IsActive;
                category.UpdatedAt = DateTime.UtcNow;

                if (imageFile != null && imageFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(category.ImageUrl))
                    {
                        DeleteImage(category.ImageUrl);
                    }
                    category.ImageUrl = await UploadImage(imageFile);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Admin/Categories/GetData
        [HttpGet]
        public async Task<IActionResult> GetData(int draw, int start, int length, string? search)
        {
            var query = _context.Categories.Where(c => !c.IsDeleted).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()));
            }

            var totalRecords = await _context.Categories.CountAsync(c => !c.IsDeleted);
            var filteredRecords = await query.CountAsync();

            var categories = await query
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Skip(start)
                .Take(length)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.ImageUrl,
                    c.Description,
                    c.DisplayOrder,
                    c.IsActive,
                    ProductCount = c.Products!.Count(p => !p.IsDeleted),
                    CreatedAt = c.CreatedAt.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            return Json(new
            {
                draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = categories
            });
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id, bool forceDelete = false)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return Json(new { success = false, message = "Danh mục không tồn tại" });
            }

            // Đếm số sản phẩm active trong danh mục
            var productCount = category.Products?.Count(p => !p.IsDeleted) ?? 0;

            // Nếu có sản phẩm và chưa confirm force delete
            if (productCount > 0 && !forceDelete)
            {
                return Json(new { 
                    success = false, 
                    requireConfirm = true,
                    productCount = productCount,
                    message = $"Danh mục này có {productCount} sản phẩm. Bạn có chắc chắn muốn xóa? Tất cả sản phẩm trong danh mục cũng sẽ bị xóa." 
                });
            }

            // Xóa tất cả sản phẩm trong danh mục (soft delete)
            if (category.Products != null && category.Products.Any())
            {
                foreach (var product in category.Products.Where(p => !p.IsDeleted))
                {
                    product.IsDeleted = true;
                    product.UpdatedAt = DateTime.UtcNow;
                    
                    // Xóa ảnh sản phẩm nếu là file local
                    if (!string.IsNullOrEmpty(product.ImageUrl) && (product.ImageUrl.StartsWith("/uploads/") || product.ImageUrl.StartsWith("/images/")))
                    {
                        DeleteImage(product.ImageUrl);
                    }
                }
            }

            // Xóa ảnh danh mục nếu có
            if (!string.IsNullOrEmpty(category.ImageUrl))
            {
                DeleteImage(category.ImageUrl);
            }

            category.IsDeleted = true;
            category.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var message = productCount > 0 
                ? $"Đã xóa danh mục và {productCount} sản phẩm liên quan!" 
                : "Xóa danh mục thành công!";

            return Json(new { success = true, message = message });
        }

        // POST: Admin/Categories/ToggleActive/5
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return Json(new { success = false, message = "Danh mục không tồn tại" });
            }

            category.IsActive = !category.IsActive;
            category.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isActive = category.IsActive });
        }

        #region Helper Methods

        private async Task<string> UploadImage(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "categories");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/categories/{uniqueFileName}";
        }

        private void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        #endregion
    }

    public class CategoryViewModel
    {
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Tên danh mục")]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.MaxLength(500)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Hình ảnh")]
        public string? ImageUrl { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Thứ tự")]
        public int DisplayOrder { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Hoạt động")]
        public bool IsActive { get; set; } = true;
    }
}
