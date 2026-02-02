using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Models;
using nhom6_admin.Models.DTOs;

namespace nhom6_admin.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Get all users with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<UserListDto>>>> GetUsers(
            [FromQuery] UserFilterRequest filter)
        {
            try
            {
                var query = _context.Users.AsQueryable();

                // Search
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    var search = filter.Search.ToLower();
                    query = query.Where(u =>
                        (u.Email != null && u.Email.ToLower().Contains(search)) ||
                        (u.FullName != null && u.FullName.ToLower().Contains(search)) ||
                        (u.PhoneNumber != null && u.PhoneNumber.Contains(search)));
                }

                // Filter by active status
                if (filter.IsActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == filter.IsActive.Value);
                }

                // Date filter
                if (filter.FromDate.HasValue)
                {
                    query = query.Where(u => u.CreatedAt >= filter.FromDate.Value);
                }
                if (filter.ToDate.HasValue)
                {
                    query = query.Where(u => u.CreatedAt <= filter.ToDate.Value);
                }

                var totalCount = await query.CountAsync();

                // Sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "email" => filter.SortDesc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                    "fullname" => filter.SortDesc ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName),
                    "createdat" => filter.SortDesc ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
                    _ => query.OrderByDescending(u => u.CreatedAt)
                };

                // Pagination
                var users = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                // Get roles and orders for each user
                var userDtos = new List<UserListDto>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    // Filter by role if specified
                    if (!string.IsNullOrEmpty(filter.Role) && !roles.Contains(filter.Role))
                    {
                        continue;
                    }

                    var orderStats = await _context.Orders
                        .Where(o => o.UserId == user.Id && !o.IsDeleted)
                        .GroupBy(o => o.UserId)
                        .Select(g => new
                        {
                            TotalOrders = g.Count(),
                            TotalSpent = g.Sum(o => o.TotalAmount)
                        })
                        .FirstOrDefaultAsync();

                    userDtos.Add(new UserListDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        AvatarUrl = user.AvatarUrl,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        Roles = roles.ToList(),
                        TotalOrders = orderStats?.TotalOrders ?? 0,
                        TotalSpent = orderStats?.TotalSpent ?? 0
                    });
                }

                var response = new PaginatedResponse<UserListDto>
                {
                    Items = userDtos,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

                return Ok(ApiResponse<PaginatedResponse<UserListDto>>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<UserListDto>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserDto>.ErrorResponse("User not found"));
                }

                var roles = await _userManager.GetRolesAsync(user);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    AvatarUrl = user.AvatarUrl,
                    Address = user.Address,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList()
                };

                return Ok(ApiResponse<UserDto>.SuccessResponse(userDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create new user
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                // Check if email exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return BadRequest(ApiResponse<UserDto>.ErrorResponse("Email already exists"));
                }

                var user = new User
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    AvatarUrl = request.AvatarUrl,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return BadRequest(ApiResponse<UserDto>.ErrorResponse(
                        "Failed to create user",
                        result.Errors.Select(e => e.Description).ToList()));
                }

                // Add roles
                if (request.Roles != null && request.Roles.Any())
                {
                    foreach (var role in request.Roles)
                    {
                        if (await _roleManager.RoleExistsAsync(role))
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                    }
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "Customer");
                }

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(ApiResponse<UserDto>.SuccessResponse(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    AvatarUrl = user.AvatarUrl,
                    Address = user.Address,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList()
                }, "User created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update user
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(string id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserDto>.ErrorResponse("User not found"));
                }

                // Update properties
                if (!string.IsNullOrEmpty(request.FullName))
                    user.FullName = request.FullName;
                if (!string.IsNullOrEmpty(request.PhoneNumber))
                    user.PhoneNumber = request.PhoneNumber;
                if (!string.IsNullOrEmpty(request.Address))
                    user.Address = request.Address;
                if (!string.IsNullOrEmpty(request.AvatarUrl))
                    user.AvatarUrl = request.AvatarUrl;
                if (request.IsActive.HasValue)
                    user.IsActive = request.IsActive.Value;

                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(ApiResponse<UserDto>.ErrorResponse(
                        "Failed to update user",
                        result.Errors.Select(e => e.Description).ToList()));
                }

                // Update roles
                if (request.Roles != null)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    foreach (var role in request.Roles)
                    {
                        if (await _roleManager.RoleExistsAsync(role))
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                    }
                }

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(ApiResponse<UserDto>.SuccessResponse(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    AvatarUrl = user.AvatarUrl,
                    Address = user.Address,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList()
                }, "User updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserDto>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete user (soft delete by deactivating)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse("User not found"));
                }

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);

                return Ok(ApiResponse<bool>.SuccessResponse(true, "User deactivated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Toggle user active status
        /// </summary>
        [HttpPut("{id}/toggle-active")]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleUserActive(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse("User not found"));
                }

                user.IsActive = !user.IsActive;
                user.UpdatedAt = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);

                return Ok(ApiResponse<bool>.SuccessResponse(user.IsActive,
                    user.IsActive ? "User activated" : "User deactivated"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Reset user password
        /// </summary>
        [HttpPost("{id}/reset-password")]
        public async Task<ActionResult<ApiResponse<string>>> ResetPassword(string id, [FromBody] string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<string>.ErrorResponse("User not found"));
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (!result.Succeeded)
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse(
                        "Failed to reset password",
                        result.Errors.Select(e => e.Description).ToList()));
                }

                return Ok(ApiResponse<string>.SuccessResponse("Password reset successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all available roles
        /// </summary>
        [HttpGet("roles")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetRoles()
        {
            try
            {
                var roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                return Ok(ApiResponse<List<string>>.SuccessResponse(roles));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<string>>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }
    }
}
