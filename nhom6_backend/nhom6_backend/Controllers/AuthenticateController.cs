using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using nhom6_backend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace nhom6_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthenticateController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if user already exists
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status400BadRequest, 
                    new { Status = false, Message = "User already exists" });

            // Check if email already exists
            var emailExists = await _userManager.FindByEmailAsync(model.Email);
            if (emailExists != null)
                return StatusCode(StatusCodes.Status400BadRequest, 
                    new { Status = false, Message = "Email already exists" });

            // Create new user
            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Initials = model.Initials ?? GenerateInitials(model.FullName ?? model.Username),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { Status = false, Message = $"Đăng ký thất bại: {errors}" });
            }

            // Luôn gán role Customer, không cho phép client chọn role khác
            const string role = "Customer";
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
            await _userManager.AddToRoleAsync(user, role);

            return Ok(new { Status = true, Message = "Đăng ký thành công" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized(new { Status = false, Message = "Invalid username or password" });

            if (!user.IsActive)
                return Unauthorized(new { Status = false, Message = "Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ admin." });

            // Check if account is locked
            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                var lockoutEndLocal = user.LockoutEnd.Value.LocalDateTime;
                return Unauthorized(new { 
                    Status = false, 
                    Message = $"Tài khoản đã bị khóa đến ngày {lockoutEndLocal:dd/MM/yyyy HH:mm}. Vui lòng liên hệ admin." 
                });
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GenerateToken(authClaims);

            return Ok(new
            {
                Status = true,
                Message = "Logged in successfully",
                Token = token,
                User = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FullName,
                    user.Initials,
                    user.AvatarUrl,
                    Roles = userRoles
                }
            });
        }

        [HttpGet("profile")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return Unauthorized(new { Status = false, Message = "User not found" });

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return NotFound(new { Status = false, Message = "User not found" });

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                Status = true,
                User = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FullName,
                    user.Initials,
                    user.AvatarUrl,
                    user.Address,
                    user.PhoneNumber,
                    user.CreatedAt,
                    Roles = roles
                }
            });
        }

        /// <summary>
        /// External login (Google Sign-In)
        /// </summary>
        [HttpPost("external-login")]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginModel model)
        {
            if (string.IsNullOrEmpty(model.Provider) || string.IsNullOrEmpty(model.AccessToken))
            {
                return BadRequest(new { Status = false, Message = "Provider và AccessToken là bắt buộc" });
            }

            try
            {
                // Verify Google access token by calling Google userinfo endpoint
                string? email = null;
                string? name = null;
                string? photoUrl = null;
                string? googleId = null;

                if (model.Provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
                {
                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", model.AccessToken);

                    var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        return Unauthorized(new { Status = false, Message = "Token Google không hợp lệ" });
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    var userInfo = JsonSerializer.Deserialize<JsonElement>(json);

                    email = userInfo.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
                    name = userInfo.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
                    photoUrl = userInfo.TryGetProperty("picture", out var pictureProp) ? pictureProp.GetString() : null;
                    googleId = userInfo.TryGetProperty("sub", out var subProp) ? subProp.GetString() : null;

                    // Use email from model if not in token
                    if (string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(model.Email))
                    {
                        email = model.Email;
                    }
                    if (string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(model.DisplayName))
                    {
                        name = model.DisplayName;
                    }
                    if (string.IsNullOrEmpty(photoUrl) && !string.IsNullOrEmpty(model.PhotoUrl))
                    {
                        photoUrl = model.PhotoUrl;
                    }
                }
                else
                {
                    return BadRequest(new { Status = false, Message = $"Provider '{model.Provider}' không được hỗ trợ" });
                }

                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { Status = false, Message = "Không thể lấy email từ tài khoản Google" });
                }

                // Find or create user
                var user = await _userManager.FindByEmailAsync(email);
                
                if (user == null)
                {
                    // Create new user from Google account
                    user = new User
                    {
                        UserName = email,
                        Email = email,
                        FullName = name ?? email.Split('@')[0],
                        Initials = GenerateInitials(name ?? email),
                        AvatarUrl = photoUrl,
                        EmailConfirmed = true, // Google email is verified
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                        return StatusCode(500, new { Status = false, Message = $"Không thể tạo tài khoản: {errors}" });
                    }

                    // Assign Customer role
                    if (!await _roleManager.RoleExistsAsync("Customer"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Customer"));
                    }
                    await _userManager.AddToRoleAsync(user, "Customer");
                }
                else
                {
                    // Update existing user info from Google if needed
                    bool needUpdate = false;
                    
                    if (!string.IsNullOrEmpty(photoUrl) && user.AvatarUrl != photoUrl)
                    {
                        user.AvatarUrl = photoUrl;
                        needUpdate = true;
                    }
                    if (!string.IsNullOrEmpty(name) && string.IsNullOrEmpty(user.FullName))
                    {
                        user.FullName = name;
                        user.Initials = GenerateInitials(name);
                        needUpdate = true;
                    }

                    if (needUpdate)
                    {
                        await _userManager.UpdateAsync(user);
                    }
                }

                if (!user.IsActive)
                {
                    return Unauthorized(new { Status = false, Message = "Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ admin." });
                }

                // Check if account is locked
                if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
                {
                    var lockoutEndLocal = user.LockoutEnd.Value.LocalDateTime;
                    return Unauthorized(new { 
                        Status = false, 
                        Message = $"Tài khoản đã bị khóa đến ngày {lockoutEndLocal:dd/MM/yyyy HH:mm}. Vui lòng liên hệ admin." 
                    });
                }

                // Generate JWT token
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = GenerateToken(authClaims);

                return Ok(new
                {
                    Status = true,
                    Message = "Đăng nhập thành công",
                    Token = token,
                    User = new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        user.FullName,
                        user.Initials,
                        user.AvatarUrl,
                        Roles = userRoles
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Status = false, Message = $"Lỗi server: {ex.Message}" });
            }
        }

        private string GenerateToken(IEnumerable<Claim> claims)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "UmeAppSecretKey2024VeryLongSecretKeyForJWT!@#$%";
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var expirationMinutes = Convert.ToDouble(jwtSettings["ExpirationInMinutes"] ?? "60");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Issuer = jwtSettings["Issuer"] ?? "UmeAPI",
                Audience = jwtSettings["Audience"] ?? "UmeApp",
                SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static string GenerateInitials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "U";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            return name.Length >= 2 ? name[..2].ToUpper() : name.ToUpper();
        }
    }
}
