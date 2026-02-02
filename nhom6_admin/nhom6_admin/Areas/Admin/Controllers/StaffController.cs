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
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StaffController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetData(string? search, string? status, int start = 0, int length = 10)
        {
            var query = _context.Staff.Where(s => !s.IsDeleted).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(s => 
                    s.FullName.ToLower().Contains(search) || 
                    (s.Email != null && s.Email.ToLower().Contains(search)) ||
                    s.StaffCode.ToLower().Contains(search));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(s => s.Status == status);
            }

            var total = await query.CountAsync();
            
            var data = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip(start)
                .Take(length)
                .Select(s => new
                {
                    s.Id,
                    s.StaffCode,
                    s.FullName,
                    s.Email,
                    Phone = s.PhoneNumber,
                    Avatar = s.AvatarUrl,
                    Department = s.Position,
                    s.Position,
                    s.Status,
                    JoinDate = s.HireDate,
                    IsActive = s.Status == "Active",
                    appointmentCount = s.Appointments != null ? s.Appointments.Count(a => !a.IsDeleted) : 0,
                    rating = s.AverageRating
                })
                .ToListAsync();

            return Json(new { recordsTotal = total, recordsFiltered = total, data });
        }

        public IActionResult Create()
        {
            return View(new StaffViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StaffViewModel model, IFormFile? AvatarFile)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Generate StaffCode
            var lastStaff = await _context.Staff.OrderByDescending(s => s.Id).FirstOrDefaultAsync();
            var staffCode = $"NV{(lastStaff?.Id ?? 0) + 1:D4}";

            var staff = new Staff
            {
                StaffCode = staffCode,
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.Phone,
                Position = model.Position ?? "Barber",
                Level = model.Level ?? "Junior",
                DateOfBirth = model.DateOfBirth,
                HireDate = model.JoinDate ?? DateTime.UtcNow,
                BaseSalary = model.Salary,
                Status = model.IsActive ? "Active" : "OnLeave",
                CreatedAt = DateTime.UtcNow
            };

            // Upload avatar
            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "staff");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(AvatarFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await AvatarFile.CopyToAsync(stream);
                staff.AvatarUrl = $"/uploads/staff/{fileName}";
            }

            _context.Staff.Add(staff);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            if (staff == null) return NotFound();

            var model = new StaffViewModel
            {
                Id = staff.Id,
                FullName = staff.FullName,
                Email = staff.Email,
                Phone = staff.PhoneNumber,
                Position = staff.Position,
                Level = staff.Level,
                DateOfBirth = staff.DateOfBirth,
                JoinDate = staff.HireDate,
                Salary = staff.BaseSalary ?? 0,
                IsActive = staff.Status == "Active",
                ExistingAvatar = staff.AvatarUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StaffViewModel model, IFormFile? AvatarFile)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var staff = await _context.Staff.FindAsync(model.Id);
            if (staff == null || staff.IsDeleted) return NotFound();

            staff.FullName = model.FullName;
            staff.Email = model.Email;
            staff.PhoneNumber = model.Phone;
            staff.Position = model.Position ?? "Barber";
            staff.Level = model.Level ?? "Junior";
            staff.DateOfBirth = model.DateOfBirth;
            staff.HireDate = model.JoinDate ?? staff.HireDate;
            staff.BaseSalary = model.Salary;
            staff.Status = model.IsActive ? "Active" : "OnLeave";
            staff.UpdatedAt = DateTime.UtcNow;

            // Upload new avatar
            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "staff");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(AvatarFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await AvatarFile.CopyToAsync(stream);
                staff.AvatarUrl = $"/uploads/staff/{fileName}";
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật nhân viên thành công!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var staff = await _context.Staff
                .Include(s => s.Appointments)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            if (staff == null) return NotFound();
            return View(staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var staff = await _context.Staff.FindAsync(id);
            if (staff == null)
            {
                return Json(new { success = false, message = "Nhân viên không tồn tại!" });
            }

            staff.IsDeleted = true;
            staff.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa nhân viên!" });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var staff = await _context.Staff.FindAsync(id);
            if (staff == null)
            {
                return Json(new { success = false, message = "Nhân viên không tồn tại!" });
            }

            var isActive = staff.Status == "Active";
            staff.Status = isActive ? "OnLeave" : "Active";
            staff.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isActive = staff.Status == "Active" });
        }
    }

    public class StaffViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Position { get; set; }
        public string? Level { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? JoinDate { get; set; }
        public decimal Salary { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ExistingAvatar { get; set; }
    }
}
