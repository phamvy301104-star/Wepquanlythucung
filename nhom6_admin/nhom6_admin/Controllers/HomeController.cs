using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Models;

namespace nhom6_admin.Controllers
{
    /// <summary>
    /// Controller cho trang chủ công khai (Public Website)
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Trang chủ
        /// </summary>
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "UME Salon - Chào mừng bạn đến với UME";
            
            // Lấy sản phẩm nổi bật
            var featuredProducts = await _context.Products
                .Where(p => !p.IsDeleted && p.IsActive && p.IsFeatured)
                .Take(8)
                .ToListAsync();
            ViewBag.FeaturedProducts = featuredProducts;

            // Lấy dịch vụ nổi bật
            var featuredServices = await _context.Services
                .Where(s => !s.IsDeleted && s.IsActive && s.IsFeatured)
                .Take(6)
                .ToListAsync();
            ViewBag.FeaturedServices = featuredServices;

            // Lấy danh mục sản phẩm
            var categories = await _context.Categories
                .Where(c => !c.IsDeleted && c.IsActive && c.ShowOnHomePage)
                .Take(6)
                .ToListAsync();
            ViewBag.Categories = categories;

            return View();
        }

        /// <summary>
        /// Trang giới thiệu
        /// </summary>
        public IActionResult About()
        {
            ViewData["Title"] = "Giới thiệu - UME Salon";
            return View();
        }

        /// <summary>
        /// Trang liên hệ
        /// </summary>
        public IActionResult Contact()
        {
            ViewData["Title"] = "Liên hệ - UME Salon";
            return View();
        }

        /// <summary>
        /// Trang sản phẩm
        /// </summary>
        public async Task<IActionResult> Products(int? categoryId, string? search, int page = 1)
        {
            ViewData["Title"] = "Sản phẩm - UME Salon";
            
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted && p.IsActive);

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));
            }

            var pageSize = 12;
            var totalItems = await query.CountAsync();
            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Products = products;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.CategoryId = categoryId;
            ViewBag.Search = search;
            ViewBag.Categories = await _context.Categories.Where(c => !c.IsDeleted && c.IsActive).ToListAsync();

            return View();
        }

        /// <summary>
        /// Chi tiết sản phẩm
        /// </summary>
        public async Task<IActionResult> ProductDetail(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null)
            {
                return NotFound();
            }

            ViewData["Title"] = product.Name + " - UME Salon";
            
            // Sản phẩm liên quan
            var relatedProducts = await _context.Products
                .Where(p => !p.IsDeleted && p.IsActive && p.CategoryId == product.CategoryId && p.Id != id)
                .Take(4)
                .ToListAsync();
            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }

        /// <summary>
        /// Trang dịch vụ
        /// </summary>
        public async Task<IActionResult> Services()
        {
            ViewData["Title"] = "Dịch vụ - UME Salon";
            
            var services = await _context.Services
                .Where(s => !s.IsDeleted && s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            ViewBag.Services = services;
            return View();
        }

        /// <summary>
        /// Chi tiết dịch vụ
        /// </summary>
        public async Task<IActionResult> ServiceDetail(int id)
        {
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (service == null)
            {
                return NotFound();
            }

            ViewData["Title"] = service.Name + " - UME Salon";
            return View(service);
        }

        /// <summary>
        /// Đặt lịch hẹn
        /// </summary>
        public async Task<IActionResult> Booking()
        {
            ViewData["Title"] = "Đặt lịch hẹn - UME Salon";
            
            ViewBag.Services = await _context.Services
                .Where(s => !s.IsDeleted && s.IsActive)
                .ToListAsync();
            ViewBag.Staff = await _context.Staff
                .Where(s => !s.IsDeleted && s.Status == "Active")
                .ToListAsync();
            
            return View();
        }

        /// <summary>
        /// Trang lỗi
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        /// <summary>
        /// Trang không tìm thấy
        /// </summary>
        public IActionResult NotFoundPage()
        {
            Response.StatusCode = 404;
            return View();
        }
    }
}
