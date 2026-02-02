using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Models;
using nhom6_admin.Models.Entities;
using System.Text.Json;

namespace nhom6_admin.Controllers.Admin
{
    [Area("Admin")]
    [Authorize]
    public class PetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PetsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Admin/Pets
        public async Task<IActionResult> Index(string? search, string? species, string? status, int page = 1)
        {
            var query = _context.Pets.Where(p => !p.IsDeleted).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.PetCode.Contains(search) || (p.Breed != null && p.Breed.Contains(search)));
            }

            if (!string.IsNullOrEmpty(species))
            {
                query = query.Where(p => p.Species == species);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            var pageSize = 12;
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var pets = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Species = species;
            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            // Statistics
            ViewBag.TotalPets = await _context.Pets.CountAsync(p => !p.IsDeleted);
            ViewBag.AvailablePets = await _context.Pets.CountAsync(p => !p.IsDeleted && p.Status == "Available");
            ViewBag.SoldPets = await _context.Pets.CountAsync(p => !p.IsDeleted && p.Status == "Sold");
            ViewBag.ReservedPets = await _context.Pets.CountAsync(p => !p.IsDeleted && p.Status == "Reserved");

            return View(pets);
        }

        // GET: Admin/Pets/Create
        public IActionResult Create()
        {
            var pet = new Pet
            {
                PetCode = GeneratePetCode()
            };
            return View(pet);
        }

        // POST: Admin/Pets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pet pet, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    pet.ImageUrl = await SaveImage(imageFile);
                }

                // Generate QR Code data
                pet.QRCodeData = GenerateQRCodeData(pet);
                pet.CreatedAt = DateTime.UtcNow;

                _context.Pets.Add(pet);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đã thêm thú cưng {pet.Name} thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(pet);
        }

        // GET: Admin/Pets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pet = await _context.Pets.FindAsync(id);
            if (pet == null || pet.IsDeleted) return NotFound();

            return View(pet);
        }

        // POST: Admin/Pets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pet pet, IFormFile? imageFile)
        {
            if (id != pet.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPet = await _context.Pets.FindAsync(id);
                    if (existingPet == null) return NotFound();

                    // Update properties
                    existingPet.Name = pet.Name;
                    existingPet.Species = pet.Species;
                    existingPet.Breed = pet.Breed;
                    existingPet.Gender = pet.Gender;
                    existingPet.DateOfBirth = pet.DateOfBirth;
                    existingPet.AgeInMonths = pet.AgeInMonths;
                    existingPet.Color = pet.Color;
                    existingPet.Weight = pet.Weight;
                    existingPet.Description = pet.Description;
                    existingPet.HealthStatus = pet.HealthStatus;
                    existingPet.IsVaccinated = pet.IsVaccinated;
                    existingPet.VaccinationDetails = pet.VaccinationDetails;
                    existingPet.IsNeutered = pet.IsNeutered;
                    existingPet.HasMicrochip = pet.HasMicrochip;
                    existingPet.MicrochipNumber = pet.MicrochipNumber;
                    existingPet.Price = pet.Price;
                    existingPet.OriginalPrice = pet.OriginalPrice;
                    existingPet.Status = pet.Status;
                    existingPet.IsForSale = pet.IsForSale;
                    existingPet.IsFeatured = pet.IsFeatured;
                    existingPet.Origin = pet.Origin;
                    existingPet.Notes = pet.Notes;
                    existingPet.UpdatedAt = DateTime.UtcNow;

                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        existingPet.ImageUrl = await SaveImage(imageFile);
                    }

                    // Update QR Code data
                    existingPet.QRCodeData = GenerateQRCodeData(existingPet);

                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Đã cập nhật thú cưng {pet.Name} thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PetExists(pet.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(pet);
        }

        // GET: Admin/Pets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (pet == null) return NotFound();

            // Increment view count
            pet.ViewCount++;
            await _context.SaveChangesAsync();

            return View(pet);
        }

        // GET: Admin/Pets/QRCode/5
        public async Task<IActionResult> QRCode(int? id)
        {
            if (id == null) return NotFound();

            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (pet == null) return NotFound();

            return View(pet);
        }

        // POST: Admin/Pets/Sell/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sell(int id, string buyerName, string buyerPhone)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null || pet.IsDeleted)
            {
                return Json(new { success = false, message = "Không tìm thấy thú cưng" });
            }

            pet.Status = "Sold";
            pet.SoldDate = DateTime.UtcNow;
            pet.BuyerName = buyerName;
            pet.BuyerPhone = buyerPhone;
            pet.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Đã bán {pet.Name} cho {buyerName} thành công!" });
        }

        // POST: Admin/Pets/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thú cưng" });
            }

            pet.IsDeleted = true;
            pet.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa thú cưng thành công!" });
        }

        // API: Get QR Code data
        [HttpGet]
        public async Task<IActionResult> GetQRData(int id)
        {
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (pet == null) return NotFound();

            var qrData = new
            {
                id = pet.Id,
                code = pet.PetCode,
                name = pet.Name,
                species = pet.Species,
                breed = pet.Breed ?? "Không rõ",
                gender = pet.Gender ?? "Không rõ",
                age = pet.AgeInMonths.HasValue ? $"{pet.AgeInMonths} tháng" : "Không rõ",
                color = pet.Color ?? "Không rõ",
                weight = pet.Weight.HasValue ? $"{pet.Weight} kg" : "Không rõ",
                vaccinated = pet.IsVaccinated ? "Đã tiêm" : "Chưa tiêm",
                neutered = pet.IsNeutered ? "Đã triệt sản" : "Chưa",
                microchip = pet.HasMicrochip ? pet.MicrochipNumber : "Không có",
                price = pet.Price.ToString("N0") + "đ",
                status = pet.Status,
                origin = pet.Origin ?? "Không rõ",
                healthStatus = pet.HealthStatus ?? "Khỏe mạnh",
                createdAt = pet.CreatedAt.ToString("dd/MM/yyyy"),
                url = Url.Action("Details", "Pets", new { area = "Admin", id = pet.Id }, Request.Scheme)
            };

            return Json(qrData);
        }

        #region Helper Methods

        private string GeneratePetCode()
        {
            var random = new Random();
            var code = $"PET{DateTime.Now:yyMMdd}{random.Next(1000, 9999)}";
            return code;
        }

        private string GenerateQRCodeData(Pet pet)
        {
            var data = new
            {
                id = pet.Id,
                code = pet.PetCode,
                name = pet.Name,
                species = pet.Species,
                breed = pet.Breed,
                price = pet.Price,
                vaccinated = pet.IsVaccinated,
                createdAt = pet.CreatedAt
            };
            return JsonSerializer.Serialize(data);
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "pets");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return $"/uploads/pets/{uniqueFileName}";
        }

        private bool PetExists(int id)
        {
            return _context.Pets.Any(e => e.Id == id && !e.IsDeleted);
        }

        #endregion
    }
}
