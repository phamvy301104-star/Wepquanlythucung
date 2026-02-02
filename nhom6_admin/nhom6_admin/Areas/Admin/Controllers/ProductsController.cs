using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Models;
using System.ComponentModel.DataAnnotations;

namespace nhom6_admin.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index(string? search, int? categoryId, int? brandId, 
            bool? isActive, bool? isFeatured, bool? lowStock, int page = 1)
        {
            ViewBag.Categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            ViewBag.Brands = await _context.Brands
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.Name)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name })
                .ToListAsync();

            ViewBag.CurrentFilter = new {
                Search = search,
                CategoryId = categoryId,
                BrandId = brandId,
                IsActive = isActive,
                IsFeatured = isFeatured,
                LowStock = lowStock
            };

            return View();
        }

        // GET: Admin/Products/Create
        public async Task<IActionResult> Create()
        {
            await LoadSelectLists();
            return View(new ProductViewModel());
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model, IFormFile? imageFile, string? imageUrl,
            List<IFormFile>? galleryFiles, string? galleryUrls, List<string>? newGalleryUrls)
        {
            if (ModelState.IsValid)
            {
                // Check SKU unique
                if (await _context.Products.AnyAsync(p => p.SKU == model.SKU && !p.IsDeleted))
                {
                    ModelState.AddModelError("SKU", "Mã SKU đã tồn tại");
                    await LoadSelectLists();
                    return View(model);
                }

                var product = new Product
                {
                    SKU = model.SKU,
                    Barcode = model.Barcode,
                    Name = model.Name,
                    Slug = GenerateSlug(model.Name),
                    ShortDescription = model.ShortDescription,
                    Description = model.Description,
                    OriginalPrice = model.OriginalPrice,
                    Price = model.Price,
                    DiscountPercent = model.DiscountPercent,
                    CostPrice = model.CostPrice,
                    StockQuantity = model.StockQuantity,
                    LowStockThreshold = model.LowStockThreshold,
                    CategoryId = model.CategoryId,
                    BrandId = model.BrandId,
                    Weight = model.Weight,
                    Volume = model.Volume,
                    Unit = model.Unit,
                    Ingredients = model.Ingredients,
                    Usage = model.Usage,
                    Warnings = model.Warnings,
                    Origin = model.Origin,
                    IsFeatured = model.IsFeatured,
                    IsBestSeller = model.IsBestSeller,
                    IsNew = model.IsNewArrival,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                // Upload image file hoặc sử dụng URL
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Validate file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ImageFile", "Chỉ chấp nhận file ảnh (JPG, PNG, GIF, WEBP)");
                        await LoadSelectLists();
                        return View(model);
                    }
                    if (imageFile.Length > 5 * 1024 * 1024) // 5MB
                    {
                        ModelState.AddModelError("ImageFile", "Kích thước file tối đa 5MB");
                        await LoadSelectLists();
                        return View(model);
                    }
                    product.ImageUrl = await UploadImage(imageFile);
                }
                else if (!string.IsNullOrEmpty(imageUrl))
                {
                    product.ImageUrl = imageUrl;
                }

                // Process gallery images
                var galleryImagesList = new List<string>();
                
                // Upload new gallery files
                if (galleryFiles != null && galleryFiles.Any())
                {
                    foreach (var file in galleryFiles)
                    {
                        if (file.Length > 0)
                        {
                            var uploadedUrl = await UploadImage(file);
                            galleryImagesList.Add(uploadedUrl);
                        }
                    }
                }
                
                // Add new gallery URLs
                if (newGalleryUrls != null && newGalleryUrls.Any())
                {
                    galleryImagesList.AddRange(newGalleryUrls.Where(u => !string.IsNullOrWhiteSpace(u)));
                }
                
                // Save as JSON
                if (galleryImagesList.Any())
                {
                    product.AdditionalImages = System.Text.Json.JsonSerializer.Serialize(galleryImagesList);
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            await LoadSelectLists();
            return View(model);
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null)
            {
                return NotFound();
            }

            var model = new ProductViewModel
            {
                Id = product.Id,
                SKU = product.SKU,
                Barcode = product.Barcode,
                Name = product.Name,
                ShortDescription = product.ShortDescription,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                AdditionalImages = product.AdditionalImages,
                OriginalPrice = product.OriginalPrice,
                Price = product.Price,
                DiscountPercent = product.DiscountPercent,
                CostPrice = product.CostPrice,
                StockQuantity = product.StockQuantity,
                LowStockThreshold = product.LowStockThreshold,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                Weight = product.Weight,
                Volume = product.Volume,
                Unit = product.Unit,
                Ingredients = product.Ingredients,
                Usage = product.Usage,
                Warnings = product.Warnings,
                Origin = product.Origin,
                IsFeatured = product.IsFeatured,
                IsBestSeller = product.IsBestSeller,
                IsNewArrival = product.IsNew,
                IsActive = product.IsActive
            };

            await LoadSelectLists();
            return View(model);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel model, IFormFile? imageFile, string? imageUrl, 
            List<IFormFile>? galleryFiles, string? galleryUrls, List<string>? existingGalleryImages, List<string>? newGalleryUrls)
        {
            // Debug: Log received values
            Console.WriteLine($"\n=== EDIT PRODUCT DEBUG ===");
            Console.WriteLine($"imageFile: {(imageFile != null ? imageFile.FileName : "NULL")}");
            Console.WriteLine($"imageUrl: '{imageUrl}'");
            Console.WriteLine($"model.ImageUrl: '{model.ImageUrl}'");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState Errors:");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"  {state.Key}: {error.ErrorMessage}");
                    }
                }
            }
            Console.WriteLine("========================\n");
            
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null || product.IsDeleted)
                {
                    return NotFound();
                }

                // Check SKU unique (exclude current)
                if (await _context.Products.AnyAsync(p => p.SKU == model.SKU && p.Id != id && !p.IsDeleted))
                {
                    ModelState.AddModelError("SKU", "Mã SKU đã tồn tại");
                    await LoadSelectLists();
                    return View(model);
                }

                product.SKU = model.SKU;
                product.Barcode = model.Barcode;
                product.Name = model.Name;
                product.Slug = GenerateSlug(model.Name);
                product.ShortDescription = model.ShortDescription;
                product.Description = model.Description;
                product.OriginalPrice = model.OriginalPrice;
                product.Price = model.Price;
                product.DiscountPercent = model.DiscountPercent;
                product.CostPrice = model.CostPrice;
                product.StockQuantity = model.StockQuantity;
                product.LowStockThreshold = model.LowStockThreshold;
                product.CategoryId = model.CategoryId;
                product.BrandId = model.BrandId;
                product.Weight = model.Weight;
                product.Volume = model.Volume;
                product.Unit = model.Unit;
                product.Ingredients = model.Ingredients;
                product.Usage = model.Usage;
                product.Warnings = model.Warnings;
                product.Origin = model.Origin;
                product.IsFeatured = model.IsFeatured;
                product.IsBestSeller = model.IsBestSeller;
                product.IsNew = model.IsNewArrival;
                product.IsActive = model.IsActive;
                product.UpdatedAt = DateTime.UtcNow;

                // Upload new image file hoặc sử dụng URL
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Validate file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ImageFile", "Chỉ chấp nhận file ảnh (JPG, PNG, GIF, WEBP)");
                        await LoadSelectLists();
                        return View(model);
                    }
                    if (imageFile.Length > 5 * 1024 * 1024) // 5MB
                    {
                        ModelState.AddModelError("ImageFile", "Kích thước file tối đa 5MB");
                        await LoadSelectLists();
                        return View(model);
                    }
                    
                    Console.WriteLine(">>> Uploading new image file...");
                    // Delete old image (nếu là file local)
                    if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl.StartsWith("/uploads/"))
                    {
                        DeleteImage(product.ImageUrl);
                    }
                    product.ImageUrl = await UploadImage(imageFile);
                    Console.WriteLine($">>> New ImageUrl: {product.ImageUrl}");
                }
                else if (!string.IsNullOrEmpty(imageUrl))
                {
                    Console.WriteLine($">>> imageUrl parameter: '{imageUrl}'");
                    Console.WriteLine($">>> Current product.ImageUrl: '{product.ImageUrl}'");
                    Console.WriteLine($">>> Are they equal? {imageUrl == product.ImageUrl}");
                    
                    // Sử dụng URL mới (chỉ khi URL khác với hiện tại)
                    if (imageUrl != product.ImageUrl)
                    {
                        Console.WriteLine(">>> URLs are different, updating...");
                        if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl.StartsWith("/uploads/"))
                        {
                            DeleteImage(product.ImageUrl);
                        }
                        product.ImageUrl = imageUrl;
                        Console.WriteLine($">>> Updated product.ImageUrl to: {product.ImageUrl}");
                    }
                    else
                    {
                        Console.WriteLine(">>> URLs are the same, skipping update");
                    }
                }
                else
                {
                    Console.WriteLine(">>> No image file or URL provided");
                }

                // Process gallery images
                var galleryImagesList = new List<string>();
                
                // Keep existing gallery images
                if (existingGalleryImages != null && existingGalleryImages.Any())
                {
                    galleryImagesList.AddRange(existingGalleryImages);
                }
                
                // Upload new gallery files
                if (galleryFiles != null && galleryFiles.Any())
                {
                    foreach (var file in galleryFiles)
                    {
                        if (file.Length > 0)
                        {
                            var uploadedUrl = await UploadImage(file);
                            galleryImagesList.Add(uploadedUrl);
                        }
                    }
                }
                
                // Add new gallery URLs
                if (newGalleryUrls != null && newGalleryUrls.Any())
                {
                    galleryImagesList.AddRange(newGalleryUrls.Where(u => !string.IsNullOrWhiteSpace(u)));
                }
                
                // Save as JSON
                if (galleryImagesList.Any())
                {
                    product.AdditionalImages = System.Text.Json.JsonSerializer.Serialize(galleryImagesList);
                }
                else
                {
                    product.AdditionalImages = null;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            await LoadSelectLists();
            return View(model);
        }

        // GET: Admin/Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa sản phẩm thành công!" });
        }

        // POST: Admin/Products/ToggleActive/5
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            product.IsActive = !product.IsActive;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isActive = product.IsActive });
        }

        // POST: Admin/Products/ToggleFeatured/5
        [HttpPost]
        public async Task<IActionResult> ToggleFeatured(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            product.IsFeatured = !product.IsFeatured;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isFeatured = product.IsFeatured });
        }

        // GET: Admin/Products/GetData (for DataTables)
        [HttpGet]
        public async Task<IActionResult> GetData(
            int draw, int start, int length,
            string? search, int? categoryId, int? brandId,
            bool? isActive, bool? isFeatured, bool? lowStock,
            string? orderColumn, string? orderDir)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => !p.IsDeleted)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchLower) ||
                    p.SKU.ToLower().Contains(searchLower) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchLower)));
            }

            // Filters
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId.Value);

            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            if (isFeatured.HasValue)
                query = query.Where(p => p.IsFeatured == isFeatured.Value);

            if (lowStock == true)
                query = query.Where(p => p.StockQuantity <= p.LowStockThreshold);

            var totalRecords = await _context.Products.CountAsync(p => !p.IsDeleted);
            var filteredRecords = await query.CountAsync();

            // Sorting
            query = orderColumn?.ToLower() switch
            {
                "name" => orderDir == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "price" => orderDir == "asc" ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
                "stock" => orderDir == "asc" ? query.OrderBy(p => p.StockQuantity) : query.OrderByDescending(p => p.StockQuantity),
                "sold" => orderDir == "asc" ? query.OrderBy(p => p.SoldCount) : query.OrderByDescending(p => p.SoldCount),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var products = await query
                .Skip(start)
                .Take(length)
                .Select(p => new
                {
                    p.Id,
                    p.SKU,
                    p.Name,
                    p.ImageUrl,
                    p.Price,
                    p.OriginalPrice,
                    p.StockQuantity,
                    p.LowStockThreshold,
                    p.SoldCount,
                    CategoryName = p.Category != null ? p.Category.Name : "-",
                    BrandName = p.Brand != null ? p.Brand.Name : "-",
                    p.IsActive,
                    p.IsFeatured,
                    p.AverageRating,
                    CreatedAt = p.CreatedAt.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            return Json(new
            {
                draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = products
            });
        }

        // API: Cập nhật stock nhanh
        [HttpPost]
        public async Task<IActionResult> UpdateStock(int id, int quantity)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            product.StockQuantity = quantity;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, newStock = product.StockQuantity });
        }

        #region Helper Methods

        private async Task LoadSelectLists()
        {
            ViewBag.Categories = await _context.Categories
                .Where(c => !c.IsDeleted && c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            ViewBag.Brands = await _context.Brands
                .Where(b => !b.IsDeleted && b.IsActive)
                .OrderBy(b => b.Name)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name })
                .ToListAsync();
        }

        private string GenerateSlug(string name)
        {
            var slug = name.ToLower()
                .Replace("đ", "d").Replace("Đ", "d")
                .Replace("à", "a").Replace("á", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
                .Replace("ă", "a").Replace("ằ", "a").Replace("ắ", "a").Replace("ẳ", "a").Replace("ẵ", "a").Replace("ặ", "a")
                .Replace("â", "a").Replace("ầ", "a").Replace("ấ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a")
                .Replace("è", "e").Replace("é", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
                .Replace("ê", "e").Replace("ề", "e").Replace("ế", "e").Replace("ể", "e").Replace("ễ", "e").Replace("ệ", "e")
                .Replace("ì", "i").Replace("í", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i")
                .Replace("ò", "o").Replace("ó", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
                .Replace("ô", "o").Replace("ồ", "o").Replace("ố", "o").Replace("ổ", "o").Replace("ỗ", "o").Replace("ộ", "o")
                .Replace("ơ", "o").Replace("ờ", "o").Replace("ớ", "o").Replace("ở", "o").Replace("ỡ", "o").Replace("ợ", "o")
                .Replace("ù", "u").Replace("ú", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u")
                .Replace("ư", "u").Replace("ừ", "u").Replace("ứ", "u").Replace("ử", "u").Replace("ữ", "u").Replace("ự", "u")
                .Replace("ỳ", "y").Replace("ý", "y").Replace("ỷ", "y").Replace("ỹ", "y").Replace("ỵ", "y");

            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');

            return slug;
        }

        private async Task<string> UploadImage(IFormFile file)
        {
            // Upload ảnh vào folder uploads/products/
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Tạo tên file unique
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Lưu file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/products/{uniqueFileName}";
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

    // ViewModel for Product form
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã SKU")]
        [MaxLength(50)]
        [Display(Name = "Mã SKU")]
        public string SKU { get; set; } = string.Empty;

        [MaxLength(50)]
        [Display(Name = "Mã vạch")]
        public string? Barcode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [MaxLength(200)]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "Mô tả ngắn")]
        public string? ShortDescription { get; set; }

        [Display(Name = "Mô tả chi tiết")]
        public string? Description { get; set; }

        [Display(Name = "Hình ảnh")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Thư viện ảnh")]
        public string? AdditionalImages { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá gốc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải >= 0")]
        [Display(Name = "Giá gốc")]
        public decimal OriginalPrice { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá bán")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải >= 0")]
        [Display(Name = "Giá bán")]
        public decimal Price { get; set; }

        [Range(0, 100)]
        [Display(Name = "Giảm giá (%)")]
        public int DiscountPercent { get; set; }

        [Display(Name = "Giá nhập")]
        public decimal? CostPrice { get; set; }

        [Range(0, int.MaxValue)]
        [Display(Name = "Số lượng tồn")]
        public int StockQuantity { get; set; }

        [Display(Name = "Ngưỡng cảnh báo")]
        public int LowStockThreshold { get; set; } = 10;

        [Display(Name = "Danh mục")]
        public int? CategoryId { get; set; }

        [Display(Name = "Thương hiệu")]
        public int? BrandId { get; set; }

        [Display(Name = "Trọng lượng (g)")]
        public int? Weight { get; set; }

        [Display(Name = "Dung tích (ml)")]
        public int? Volume { get; set; }

        [MaxLength(20)]
        [Display(Name = "Đơn vị")]
        public string? Unit { get; set; }

        [Display(Name = "Thành phần")]
        public string? Ingredients { get; set; }

        [Display(Name = "Hướng dẫn sử dụng")]
        public string? Usage { get; set; }

        [Display(Name = "Cảnh báo")]
        public string? Warnings { get; set; }

        [MaxLength(100)]
        [Display(Name = "Xuất xứ")]
        public string? Origin { get; set; }

        [Display(Name = "Nổi bật")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Bán chạy")]
        public bool IsBestSeller { get; set; }

        [Display(Name = "Mới về")]
        public bool IsNewArrival { get; set; }

        [Display(Name = "Hoạt động")]
        public bool IsActive { get; set; } = true;
    }
}
