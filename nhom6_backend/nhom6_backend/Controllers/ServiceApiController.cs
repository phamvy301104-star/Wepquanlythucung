using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_backend.Models;

namespace nhom6_backend.Controllers
{
    /// <summary>
    /// Public API Controller for Services - No authentication required
    /// Used by mobile app to display service list for booking
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all active services
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetServices()
        {
            try
            {
                var services = await _context.Services
                    .Where(s => s.IsActive && !s.IsDeleted)
                    .OrderBy(s => s.DisplayOrder)
                    .Select(s => new
                    {
                        s.Id,
                        s.ServiceCode,
                        s.Name,
                        s.Slug,
                        s.ShortDescription,
                        s.Description,
                        s.ImageUrl,
                        s.Price,
                        s.OriginalPrice,
                        s.DurationMinutes,
                        s.Gender,
                        s.IsFeatured,
                        s.IsPopular,
                        s.IsNew,
                        s.AverageRating,
                        s.TotalReviews,
                        s.TotalBookings
                    })
                    .ToListAsync();

                return Ok(services);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get featured services for homepage
        /// </summary>
        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedServices([FromQuery] int limit = 6)
        {
            try
            {
                var services = await _context.Services
                    .Where(s => s.IsActive && !s.IsDeleted && s.IsFeatured)
                    .OrderBy(s => s.DisplayOrder)
                    .Take(limit)
                    .Select(s => new
                    {
                        s.Id,
                        s.ServiceCode,
                        s.Name,
                        s.Slug,
                        s.ShortDescription,
                        s.ImageUrl,
                        s.Price,
                        s.OriginalPrice,
                        s.DurationMinutes,
                        s.Gender,
                        s.IsFeatured,
                        s.IsPopular,
                        s.AverageRating,
                        s.TotalReviews
                    })
                    .ToListAsync();

                return Ok(services);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get popular services
        /// </summary>
        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularServices([FromQuery] int limit = 6)
        {
            try
            {
                var services = await _context.Services
                    .Where(s => s.IsActive && !s.IsDeleted && s.IsPopular)
                    .OrderByDescending(s => s.TotalBookings)
                    .Take(limit)
                    .Select(s => new
                    {
                        s.Id,
                        s.ServiceCode,
                        s.Name,
                        s.Slug,
                        s.ShortDescription,
                        s.ImageUrl,
                        s.Price,
                        s.OriginalPrice,
                        s.DurationMinutes,
                        s.Gender,
                        s.AverageRating,
                        s.TotalBookings
                    })
                    .ToListAsync();

                return Ok(services);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get service by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            try
            {
                var service = await _context.Services
                    .Where(s => s.Id == id && s.IsActive && !s.IsDeleted)
                    .Select(s => new
                    {
                        s.Id,
                        s.ServiceCode,
                        s.Name,
                        s.Slug,
                        s.ShortDescription,
                        s.Description,
                        s.ImageUrl,
                        s.GalleryImages,
                        s.VideoUrl,
                        s.Price,
                        s.OriginalPrice,
                        s.MinPrice,
                        s.MaxPrice,
                        s.DurationMinutes,
                        s.BufferMinutes,
                        s.RequiredStaff,
                        s.Gender,
                        s.RequiredAdvanceBookingHours,
                        s.CancellationHours,
                        s.IsFeatured,
                        s.IsPopular,
                        s.IsNew,
                        s.AverageRating,
                        s.TotalReviews,
                        s.TotalBookings,
                        s.Notes,
                        s.Warnings
                    })
                    .FirstOrDefaultAsync();

                if (service == null)
                    return NotFound(new { message = "Service not found" });

                return Ok(service);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get services by category
        /// </summary>
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetServicesByCategory(int categoryId)
        {
            try
            {
                var services = await _context.Services
                    .Where(s => s.IsActive && !s.IsDeleted)
                    .OrderBy(s => s.DisplayOrder)
                    .Select(s => new
                    {
                        s.Id,
                        s.ServiceCode,
                        s.Name,
                        s.Slug,
                        s.ShortDescription,
                        s.ImageUrl,
                        s.Price,
                        s.OriginalPrice,
                        s.DurationMinutes,
                        s.Gender,
                        s.IsFeatured,
                        s.IsPopular,
                        s.AverageRating,
                        s.TotalReviews
                    })
                    .ToListAsync();

                return Ok(services);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get service categories
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetServiceCategories()
        {
            try
            {
                var categories = await _context.ServiceCategories
                    .Where(c => c.IsActive && !c.IsDeleted)
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Slug,
                        c.Description,
                        c.ImageUrl,
                        c.Icon
                    })
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get available staff for a service
        /// </summary>
        [HttpGet("{id}/staff")]
        public async Task<IActionResult> GetStaffForService(int id)
        {
            try
            {
                var service = await _context.Services
                    .FirstOrDefaultAsync(s => s.Id == id && s.IsActive && !s.IsDeleted);

                if (service == null)
                    return NotFound(new { message = "Service not found" });

                // Get staff who can perform this service (available and accept online booking)
                var staff = await _context.Staff
                    .Where(s => s.IsAvailable && s.AcceptOnlineBooking && s.Status == "Active" && !s.IsDeleted)
                    .OrderBy(s => s.DisplayOrder)
                    .Select(s => new
                    {
                        s.Id,
                        s.StaffCode,
                        s.FullName,
                        s.NickName,
                        s.AvatarUrl,
                        s.Position,
                        s.Level,
                        s.Specialties,
                        s.YearsOfExperience,
                        s.AverageRating,
                        s.TotalReviews
                    })
                    .ToListAsync();

                return Ok(staff);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}
