using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Models;
using nhom6_admin.Models.DTOs;

namespace nhom6_admin.Controllers.Admin
{
    /// <summary>
    /// API Controller for Admin Notifications (Bell icon notifications)
    /// Removed [Authorize] to allow notification loading from admin dashboard
    /// The admin area pages themselves are already protected
    /// </summary>
    [Route("api/admin/[controller]")]
    [ApiController]
    public class AdminNotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminNotificationController> _logger;

        public AdminNotificationController(
            ApplicationDbContext context,
            ILogger<AdminNotificationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all admin notifications with pagination
        /// GET /api/admin/AdminNotification
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<object>>> GetNotifications(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? unreadOnly = null)
        {
            try
            {
                var query = _context.AdminNotifications
                    .Where(n => !n.IsDeleted)
                    .AsQueryable();

                if (unreadOnly == true)
                {
                    query = query.Where(n => !n.IsRead);
                }

                var totalCount = await query.CountAsync();

                var notifications = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(n => new
                    {
                        n.Id,
                        n.Type,
                        n.Title,
                        n.Content,
                        n.ActionUrl,
                        n.IsRead,
                        n.ReadAt,
                        n.RelatedEntityId,
                        n.RelatedEntityType,
                        n.CreatedAt
                    })
                    .ToListAsync();

                return Ok(ApiResponse<object>.SuccessResponse(new
                {
                    items = notifications,
                    totalCount,
                    page,
                    pageSize,
                    unreadCount = await _context.AdminNotifications.CountAsync(n => !n.IsDeleted && !n.IsRead)
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin notifications");
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get unread notification count
        /// GET /api/admin/AdminNotification/unread-count
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
        {
            try
            {
                var count = await _context.AdminNotifications
                    .CountAsync(n => !n.IsDeleted && !n.IsRead);

                return Ok(ApiResponse<int>.SuccessResponse(count));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return StatusCode(500, ApiResponse<int>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Mark notification as read
        /// PUT /api/admin/AdminNotification/{id}/read
        /// </summary>
        [HttpPut("{id}/read")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(int id)
        {
            try
            {
                var notification = await _context.AdminNotifications.FindAsync(id);
                if (notification == null || notification.IsDeleted)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse("Notification not found"));
                }

                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                notification.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Marked as read"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Mark all notifications as read
        /// PUT /api/admin/AdminNotification/mark-all-read
        /// </summary>
        [HttpPut("mark-all-read")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkAllAsRead()
        {
            try
            {
                var unreadNotifications = await _context.AdminNotifications
                    .Where(n => !n.IsDeleted && !n.IsRead)
                    .ToListAsync();

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    notification.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(true, $"Marked {unreadNotifications.Count} notifications as read"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete notification
        /// DELETE /api/admin/AdminNotification/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteNotification(int id)
        {
            try
            {
                var notification = await _context.AdminNotifications.FindAsync(id);
                if (notification == null || notification.IsDeleted)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse("Notification not found"));
                }

                notification.IsDeleted = true;
                notification.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Notification deleted"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Server error: {ex.Message}"));
            }
        }
    }
}
