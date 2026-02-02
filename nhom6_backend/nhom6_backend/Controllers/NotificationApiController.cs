using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_backend.Models;
using System.Security.Claims;

namespace nhom6_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationApiController> _logger;

        public NotificationApiController(
            ApplicationDbContext context,
            ILogger<NotificationApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách thông báo của user
        /// GET /api/NotificationApi/my-notifications
        /// </summary>
        [HttpGet("my-notifications")]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] bool? isRead = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });

                var query = _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsDeleted)
                    .AsQueryable();

                // Filter by read status
                if (isRead.HasValue)
                {
                    query = query.Where(n => n.IsRead == isRead.Value);
                }

                // Order by created date desc
                query = query.OrderByDescending(n => n.CreatedAt);

                var totalItems = await query.CountAsync();
                var unreadCount = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                    .CountAsync();

                var notifications = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(n => new
                    {
                        n.Id,
                        n.Type,
                        n.Title,
                        n.Content,
                        n.ImageUrl,
                        n.ActionUrl,
                        n.ReferenceType,
                        n.ReferenceId,
                        n.IsRead,
                        n.ReadAt,
                        n.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        items = notifications,
                        unreadCount,
                        page,
                        pageSize,
                        totalItems
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }

        /// <summary>
        /// Đếm số thông báo chưa đọc
        /// GET /api/NotificationApi/unread-count
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });

                var count = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                    .CountAsync();

                return Ok(new { success = true, count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }

        /// <summary>
        /// Đánh dấu đã đọc một thông báo
        /// PATCH /api/NotificationApi/{id}/mark-read
        /// </summary>
        [HttpPatch("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var notification = await _context.Notifications
                    .Where(n => n.Id == id && n.UserId == userId && !n.IsDeleted)
                    .FirstOrDefaultAsync();

                if (notification == null)
                    return NotFound(new { success = false, message = "Không tìm thấy thông báo" });

                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.Now;
                    notification.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                return Ok(new { success = true, message = "Đã đánh dấu đã đọc" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }

        /// <summary>
        /// Đánh dấu tất cả thông báo là đã đọc
        /// PATCH /api/NotificationApi/mark-all-read
        /// </summary>
        [HttpPatch("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var unreadNotifications = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                    .ToListAsync();

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.Now;
                    notification.UpdatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Đã đánh dấu tất cả thông báo là đã đọc",
                    count = unreadNotifications.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }

        /// <summary>
        /// Xóa thông báo
        /// DELETE /api/NotificationApi/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var notification = await _context.Notifications
                    .Where(n => n.Id == id && n.UserId == userId && !n.IsDeleted)
                    .FirstOrDefaultAsync();

                if (notification == null)
                    return NotFound(new { success = false, message = "Không tìm thấy thông báo" });

                notification.IsDeleted = true;
                notification.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Đã xóa thông báo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }

        /// <summary>
        /// Xóa tất cả thông báo đã đọc
        /// DELETE /api/NotificationApi/delete-read
        /// </summary>
        [HttpDelete("delete-read")]
        public async Task<IActionResult> DeleteReadNotifications()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var readNotifications = await _context.Notifications
                    .Where(n => n.UserId == userId && n.IsRead && !n.IsDeleted)
                    .ToListAsync();

                foreach (var notification in readNotifications)
                {
                    notification.IsDeleted = true;
                    notification.UpdatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Đã xóa tất cả thông báo đã đọc",
                    count = readNotifications.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting read notifications");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }
    }
}
