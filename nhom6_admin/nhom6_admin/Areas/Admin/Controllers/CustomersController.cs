using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Models;

namespace nhom6_admin.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        
        public CustomersController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        
        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetData(string? search, string? memberLevel, string? status, int start = 0, int length = 10)
        {
            // Lấy tất cả users từ database (không chỉ Customer)
            var usersQuery = _context.Users.AsQueryable();

            var usersList = await usersQuery.ToListAsync();
            var customersWithStats = new List<CustomerInfo>();

            foreach (var user in usersList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var roleName = roles.FirstOrDefault() ?? "Customer";
                var isLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow;

                var totalOrders = await _context.Orders.CountAsync(o => o.UserId == user.Id && !o.IsDeleted);
                var totalSpent = await _context.Orders
                    .Where(o => o.UserId == user.Id && !o.IsDeleted && o.Status == "Completed")
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
                var totalAppointments = await _context.Appointments.CountAsync(a => a.UserId == user.Id && !a.IsDeleted);

                customersWithStats.Add(new CustomerInfo
                {
                    UserId = user.Id,
                    FullName = user.FullName ?? "Chưa cập nhật",
                    Email = user.Email ?? "",
                    Phone = user.PhoneNumber ?? "",
                    Avatar = user.AvatarUrl,
                    IsActive = user.IsActive,
                    IsLocked = isLocked,
                    LockoutEnd = user.LockoutEnd?.LocalDateTime,
                    Role = roleName,
                    JoinDate = user.CreatedAt,
                    TotalOrders = totalOrders,
                    TotalSpent = totalSpent,
                    TotalAppointments = totalAppointments,
                    MemberLevel = GetMemberLevel(totalOrders),
                    Points = totalOrders * 100
                });
            }

            // Filter by search
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                customersWithStats = customersWithStats.Where(c => 
                    c.FullName.ToLower().Contains(search) || 
                    c.Email.ToLower().Contains(search) || 
                    c.Phone.Contains(search)).ToList();
            }

            // Filter by member level
            if (!string.IsNullOrEmpty(memberLevel))
            {
                customersWithStats = customersWithStats.Where(c => c.MemberLevel == memberLevel).ToList();
            }

            // Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "locked")
                    customersWithStats = customersWithStats.Where(c => c.IsLocked).ToList();
                else if (status == "active")
                    customersWithStats = customersWithStats.Where(c => c.IsActive && !c.IsLocked).ToList();
                else if (status == "inactive")
                    customersWithStats = customersWithStats.Where(c => !c.IsActive).ToList();
            }

            var total = customersWithStats.Count;
            var data = customersWithStats.OrderByDescending(c => c.JoinDate).Skip(start).Take(length).ToList();
            
            // Assign ID for display
            for (int i = 0; i < data.Count; i++)
            {
                data[i].Id = start + i + 1;
            }

            return Json(new { recordsTotal = total, recordsFiltered = total, data });
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            var totalOrders = await _context.Orders.CountAsync(o => o.UserId == id && !o.IsDeleted);
            var totalSpent = await _context.Orders
                .Where(o => o.UserId == id && !o.IsDeleted && o.Status == "Completed")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            var totalAppointments = await _context.Appointments.CountAsync(a => a.UserId == id && !a.IsDeleted);

            var customer = new CustomerInfo
            {
                UserId = user.Id,
                FullName = user.FullName ?? "Chưa cập nhật",
                Email = user.Email ?? "",
                Phone = user.PhoneNumber ?? "",
                Avatar = user.AvatarUrl,
                IsActive = user.IsActive,
                JoinDate = user.CreatedAt,
                TotalOrders = totalOrders,
                TotalSpent = totalSpent,
                TotalAppointments = totalAppointments,
                MemberLevel = GetMemberLevel(totalOrders),
                Points = totalOrders * 100
            };

            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return Json(new { success = false, message = "Không tìm thấy khách hàng" });

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
            
            return Json(new { success = true, message = user.IsActive ? "Đã kích hoạt tài khoản" : "Đã vô hiệu hóa tài khoản" });
        }

        [HttpPost]
        public IActionResult UpdateMemberLevel(string id, string level) 
        {
            // Member level is calculated automatically based on orders, not stored
            return Json(new { success = true, message = "Cấp độ thành viên được tính tự động dựa trên số đơn hàng!" });
        }

        /// <summary>
        /// Khóa tài khoản user
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LockUser(string userId, int? lockDays)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng" });

            // Prevent locking own account
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == userId)
                return Json(new { success = false, message = "Không thể khóa tài khoản của chính mình" });

            // Prevent locking Admin
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
                return Json(new { success = false, message = "Không thể khóa tài khoản Admin" });

            // Set lockout
            DateTimeOffset lockoutEnd;
            if (lockDays.HasValue && lockDays > 0)
                lockoutEnd = DateTimeOffset.UtcNow.AddDays(lockDays.Value);
            else
                lockoutEnd = DateTimeOffset.UtcNow.AddYears(100); // Permanent

            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);

            var message = lockDays.HasValue && lockDays > 0 
                ? $"Đã khóa tài khoản {user.FullName ?? user.Email} trong {lockDays} ngày"
                : $"Đã khóa vĩnh viễn tài khoản {user.FullName ?? user.Email}";

            return Json(new { success = true, message });
        }

        /// <summary>
        /// Mở khóa tài khoản user
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UnlockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng" });

            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);

            return Json(new { success = true, message = $"Đã mở khóa tài khoản {user.FullName ?? user.Email}" });
        }

        /// <summary>
        /// Thay đổi role của user
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng" });

            // Prevent changing own role
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == userId)
                return Json(new { success = false, message = "Không thể thay đổi quyền của chính mình" });

            // Validate role
            var validRoles = new[] { "Admin", "Staff", "Customer" };
            if (!validRoles.Contains(newRole))
                return Json(new { success = false, message = "Role không hợp lệ" });

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(newRole))
                await _roleManager.CreateAsync(new IdentityRole(newRole));

            // Remove all current roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Add new role
            await _userManager.AddToRoleAsync(user, newRole);

            return Json(new { success = true, message = $"Đã thay đổi quyền của {user.FullName ?? user.Email} thành {newRole}" });
        }

        /// <summary>
        /// Lấy danh sách roles
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return Json(new { success = true, data = roles });
        }

        /// <summary>
        /// Tính cấp độ thành viên dựa trên số đơn hàng
        /// 0-1 đơn: Không cấp
        /// 2-4 đơn: Iron
        /// 5-9 đơn: Silver
        /// 10-14 đơn: Gold
        /// 15+ đơn: Platinum
        /// </summary>
        private string GetMemberLevel(int totalOrders)
        {
            if (totalOrders >= 15) return "Platinum";
            if (totalOrders >= 10) return "Gold";
            if (totalOrders >= 5) return "Silver";
            if (totalOrders >= 2) return "Iron";
            return ""; // Chưa có cấp
        }

        public class CustomerInfo
        {
            public int Id { get; set; }
            public string UserId { get; set; } = "";
            public string FullName { get; set; } = "";
            public string Email { get; set; } = "";
            public string Phone { get; set; } = "";
            public string? Avatar { get; set; }
            public string MemberLevel { get; set; } = "";
            public string Role { get; set; } = "Customer";
            public int TotalOrders { get; set; }
            public decimal TotalSpent { get; set; }
            public int TotalAppointments { get; set; }
            public int Points { get; set; }
            public DateTime JoinDate { get; set; }
            public DateTime LastVisit { get; set; }
            public bool IsActive { get; set; } = true;
            public bool IsLocked { get; set; } = false;
            public DateTime? LockoutEnd { get; set; }
        }
    }
}
