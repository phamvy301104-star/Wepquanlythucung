using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_backend.Models;
using nhom6_backend.Models.DTOs.Admin;

namespace nhom6_backend.Controllers.Admin
{
    /// <summary>
    /// Admin User Management Controller
    /// Quản lý tài khoản người dùng: list, lock/unlock, update roles
    /// </summary>
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserController> _logger;

        public UserController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả users với phân trang và filter
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<AdminApiResponse<PagedResult<UserListDto>>>> GetUsers(
            [FromQuery] string? search,
            [FromQuery] string? role,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isLocked,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _userManager.Users.AsQueryable();

                // Filter by search term
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    query = query.Where(u =>
                        (u.UserName != null && u.UserName.ToLower().Contains(search)) ||
                        (u.Email != null && u.Email.ToLower().Contains(search)) ||
                        (u.FullName != null && u.FullName.ToLower().Contains(search)) ||
                        (u.PhoneNumber != null && u.PhoneNumber.Contains(search)));
                }

                // Filter by active status
                if (isActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == isActive.Value);
                }

                // Filter by locked status
                if (isLocked.HasValue)
                {
                    if (isLocked.Value)
                        query = query.Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow);
                    else
                        query = query.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow);
                }

                var totalCount = await query.CountAsync();

                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var userDtos = new List<UserListDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    // Filter by role if specified
                    if (!string.IsNullOrEmpty(role) && !roles.Contains(role))
                        continue;

                    var orderStats = await _context.Orders
                        .Where(o => o.UserId == user.Id && 
                               (o.Status == "Completed" || o.Status == "Delivered"))
                        .GroupBy(o => o.UserId)
                        .Select(g => new { OrderCount = g.Count(), TotalSpent = g.Sum(o => o.TotalAmount) })
                        .FirstOrDefaultAsync();

                    userDtos.Add(new UserListDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        AvatarUrl = user.AvatarUrl,
                        Roles = roles.ToList(),
                        IsActive = user.IsActive,
                        IsLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow,
                        LockoutEnd = user.LockoutEnd?.DateTime,
                        CreatedAt = user.CreatedAt,
                        OrderCount = orderStats?.OrderCount ?? 0,
                        TotalSpent = orderStats?.TotalSpent ?? 0
                    });
                }

                var result = new PagedResult<UserListDto>
                {
                    Items = userDtos,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };

                return Ok(AdminApiResponse<PagedResult<UserListDto>>.Ok(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, AdminApiResponse<PagedResult<UserListDto>>.Fail("Lỗi server khi lấy danh sách users"));
            }
        }

        /// <summary>
        /// Lấy chi tiết một user
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminApiResponse<UserDetailDto>>> GetUserDetail(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound(AdminApiResponse<UserDetailDto>.Fail("User không tồn tại"));

                var roles = await _userManager.GetRolesAsync(user);

                var orderStats = await _context.Orders
                    .Where(o => o.UserId == user.Id && 
                           (o.Status == "Completed" || o.Status == "Delivered"))
                    .GroupBy(o => o.UserId)
                    .Select(g => new { OrderCount = g.Count(), TotalSpent = g.Sum(o => o.TotalAmount) })
                    .FirstOrDefaultAsync();

                var recentOrders = await _context.Orders
                    .Where(o => o.UserId == user.Id)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(10)
                    .Select(o => new UserOrderSummaryDto
                    {
                        OrderId = o.Id,
                        OrderCode = o.OrderCode,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        CreatedAt = o.CreatedAt
                    })
                    .ToListAsync();

                var addresses = await _context.UserAddresses
                    .Where(a => a.UserId == user.Id)
                    .Select(a => new UserAddressDto
                    {
                        Id = a.Id,
                        FullAddress = a.FullAddress ?? "",
                        ReceiverName = a.ReceiverName,
                        ReceiverPhone = a.ReceiverPhone,
                        IsDefault = a.IsDefault
                    })
                    .ToListAsync();

                var dto = new UserDetailDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    AvatarUrl = user.AvatarUrl,
                    Address = user.Address,
                    Roles = roles.ToList(),
                    IsActive = user.IsActive,
                    IsLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow,
                    LockoutEnd = user.LockoutEnd?.DateTime,
                    CreatedAt = user.CreatedAt,
                    OrderCount = orderStats?.OrderCount ?? 0,
                    TotalSpent = orderStats?.TotalSpent ?? 0,
                    RecentOrders = recentOrders,
                    Addresses = addresses
                };

                return Ok(AdminApiResponse<UserDetailDto>.Ok(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user detail for {UserId}", id);
                return StatusCode(500, AdminApiResponse<UserDetailDto>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Khóa tài khoản user
        /// </summary>
        [HttpPost("lock")]
        public async Task<ActionResult<AdminApiResponse<bool>>> LockUser([FromBody] LockUserRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                    return NotFound(AdminApiResponse<bool>.Fail("User không tồn tại"));

                // Không cho phép khóa Admin
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin"))
                    return BadRequest(AdminApiResponse<bool>.Fail("Không thể khóa tài khoản Admin"));

                // Set lockout
                DateTimeOffset lockoutEnd;
                if (request.LockDays.HasValue && request.LockDays > 0)
                {
                    lockoutEnd = DateTimeOffset.UtcNow.AddDays(request.LockDays.Value);
                }
                else
                {
                    // Khóa vĩnh viễn (100 năm)
                    lockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
                }

                await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
                await _userManager.SetLockoutEnabledAsync(user, true);

                _logger.LogInformation("User {UserId} locked by admin until {LockoutEnd}", user.Id, lockoutEnd);

                return Ok(AdminApiResponse<bool>.Ok(true, $"Đã khóa tài khoản {user.UserName}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error locking user {UserId}", request.UserId);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server khi khóa tài khoản"));
            }
        }

        /// <summary>
        /// Mở khóa tài khoản user
        /// </summary>
        [HttpPost("unlock/{id}")]
        public async Task<ActionResult<AdminApiResponse<bool>>> UnlockUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound(AdminApiResponse<bool>.Fail("User không tồn tại"));

                await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.ResetAccessFailedCountAsync(user);

                _logger.LogInformation("User {UserId} unlocked by admin", user.Id);

                return Ok(AdminApiResponse<bool>.Ok(true, $"Đã mở khóa tài khoản {user.UserName}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user {UserId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server khi mở khóa tài khoản"));
            }
        }

        /// <summary>
        /// Vô hiệu hóa tài khoản (soft delete)
        /// </summary>
        [HttpPost("deactivate/{id}")]
        public async Task<ActionResult<AdminApiResponse<bool>>> DeactivateUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound(AdminApiResponse<bool>.Fail("User không tồn tại"));

                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin"))
                    return BadRequest(AdminApiResponse<bool>.Fail("Không thể vô hiệu hóa tài khoản Admin"));

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                return Ok(AdminApiResponse<bool>.Ok(true, $"Đã vô hiệu hóa tài khoản {user.UserName}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Kích hoạt lại tài khoản
        /// </summary>
        [HttpPost("activate/{id}")]
        public async Task<ActionResult<AdminApiResponse<bool>>> ActivateUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound(AdminApiResponse<bool>.Fail("User không tồn tại"));

                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                return Ok(AdminApiResponse<bool>.Ok(true, $"Đã kích hoạt tài khoản {user.UserName}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user {UserId}", id);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Cập nhật roles của user
        /// </summary>
        [HttpPut("roles")]
        public async Task<ActionResult<AdminApiResponse<bool>>> UpdateUserRoles([FromBody] UpdateUserRoleRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                    return NotFound(AdminApiResponse<bool>.Fail("User không tồn tại"));

                // Validate roles
                foreach (var role in request.Roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Remove current roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                // Add new roles
                await _userManager.AddToRolesAsync(user, request.Roles);

                _logger.LogInformation("User {UserId} roles updated to: {Roles}", user.Id, string.Join(", ", request.Roles));

                return Ok(AdminApiResponse<bool>.Ok(true, "Đã cập nhật quyền thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating roles for user {UserId}", request.UserId);
                return StatusCode(500, AdminApiResponse<bool>.Fail("Lỗi server khi cập nhật quyền"));
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả roles
        /// </summary>
        [HttpGet("roles")]
        public async Task<ActionResult<AdminApiResponse<List<string>>>> GetAllRoles()
        {
            try
            {
                var roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                return Ok(AdminApiResponse<List<string>>.Ok(roles));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles");
                return StatusCode(500, AdminApiResponse<List<string>>.Fail("Lỗi server"));
            }
        }

        /// <summary>
        /// Thống kê users theo role
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<AdminApiResponse<object>>> GetUserStats()
        {
            try
            {
                var totalUsers = await _userManager.Users.CountAsync();
                var activeUsers = await _userManager.Users.CountAsync(u => u.IsActive);
                var lockedUsers = await _userManager.Users
                    .CountAsync(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow);

                var adminCount = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
                var staffCount = (await _userManager.GetUsersInRoleAsync("Staff")).Count;
                var customerCount = (await _userManager.GetUsersInRoleAsync("Customer")).Count;

                var stats = new
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    InactiveUsers = totalUsers - activeUsers,
                    LockedUsers = lockedUsers,
                    ByRole = new
                    {
                        Admin = adminCount,
                        Staff = staffCount,
                        Customer = customerCount
                    }
                };

                return Ok(AdminApiResponse<object>.Ok(stats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user stats");
                return StatusCode(500, AdminApiResponse<object>.Fail("Lỗi server"));
            }
        }
    }
}
