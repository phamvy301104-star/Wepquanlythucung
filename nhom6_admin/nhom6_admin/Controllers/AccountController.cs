using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using nhom6_admin.Models;
using nhom6_admin.Models.ViewModels;
using System.Security.Claims;

namespace nhom6_admin.Controllers
{
    /// <summary>
    /// Controller xử lý đăng nhập, đăng ký cho Public Website
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        /// <summary>
        /// Trang đăng nhập
        /// </summary>
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            
            ViewData["Title"] = "Đăng nhập - UME Salon";
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Xử lý đăng nhập
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                    return View(model);
                }

                // Use null-forgiving operator since we know UserName should exist for authenticated users
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName!, 
                    model.Password, 
                    model.RememberMe, 
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Người dùng {Email} đã đăng nhập.", model.Email);
                    
                    // Nếu có returnUrl, redirect về đó
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    
                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Tài khoản {Email} bị khóa.", model.Email);
                    return View("Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                    return View(model);
                }
            }

            return View(model);
        }

        /// <summary>
        /// Trang đăng ký
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            
            ViewData["Title"] = "Đăng ký - UME Salon";
            return View();
        }

        /// <summary>
        /// Xử lý đăng ký
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Đã tạo tài khoản mới cho {Email}.", model.Email);

                    // Đảm bảo role Customer tồn tại
                    if (!await _roleManager.RoleExistsAsync("Customer"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Customer"));
                    }
                    
                    // Gán role Customer cho người dùng mới
                    await _userManager.AddToRoleAsync(user, "Customer");

                    // Tự động đăng nhập sau khi đăng ký
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Người dùng đã đăng xuất.");
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Đăng xuất (GET cho link)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> LogoutGet()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Tài khoản bị khóa
        /// </summary>
        public IActionResult Lockout()
        {
            ViewData["Title"] = "Tài khoản bị khóa";
            return View();
        }

        /// <summary>
        /// Truy cập bị từ chối
        /// </summary>
        public IActionResult AccessDenied()
        {
            ViewData["Title"] = "Truy cập bị từ chối";
            return View();
        }

        /// <summary>
        /// Trang hồ sơ cá nhân
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Hồ sơ cá nhân - UME Salon";
            return View(user);
        }

        /// <summary>
        /// Kiểm tra xem user có phải admin không (API endpoint)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> IsAdmin()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Json(new { isAdmin = false });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { isAdmin = false });
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            return Json(new { isAdmin = isAdmin });
        }
    }
}
