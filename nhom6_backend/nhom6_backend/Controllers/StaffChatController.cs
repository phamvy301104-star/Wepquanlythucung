using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using nhom6_backend.Hubs;
using nhom6_backend.Models;
using nhom6_backend.Models.DTOs.Staff;
using nhom6_backend.Models.Entities;
using System.Security.Claims;

namespace nhom6_backend.Controllers
{
    /// <summary>
    /// API Chat nội bộ nhân viên với SignalR real-time
    /// </summary>
    [Route("api/staff-chat")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    public class StaffChatController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IHubContext<StaffChatHub> _hubContext;
        private readonly ILogger<StaffChatController> _logger;
        private readonly UserManager<User> _userManager;

        public StaffChatController(
            ApplicationDbContext db, 
            IHubContext<StaffChatHub> hubContext,
            ILogger<StaffChatController> logger,
            UserManager<User> userManager)
        {
            _db = db;
            _hubContext = hubContext;
            _logger = logger;
            _userManager = userManager;
        }

        /// <summary>
        /// Lấy danh sách đồng nghiệp
        /// </summary>
        [HttpGet("colleagues")]
        public async Task<IActionResult> GetColleagues()
        {
            try
            {
                var currentStaff = await GetCurrentStaff();
                if (currentStaff == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                var colleagues = await _db.Staff
                    .AsNoTracking()
                    .Where(s => s.Id != currentStaff.Id && s.Status == "Active" && !s.IsDeleted)
                    .OrderBy(s => s.FullName)
                    .Select(s => new StaffColleagueDto
                    {
                        Id = s.Id,
                        FullName = s.FullName,
                        NickName = s.NickName,
                        AvatarUrl = s.AvatarUrl,
                        Position = s.Position,
                        IsOnline = false // Will be updated by SignalR
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = colleagues });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách đồng nghiệp");
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách phòng chat
        /// </summary>
        [HttpGet("rooms")]
        public async Task<IActionResult> GetChatRooms()
        {
            try
            {
                var currentStaff = await GetCurrentStaff();
                if (currentStaff == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                var rooms = await _db.StaffChatRooms
                    .AsNoTracking()
                    .Include(r => r.Staff1)
                    .Include(r => r.Staff2)
                    .Where(r => (r.Staff1Id == currentStaff.Id || r.Staff2Id == currentStaff.Id) && !r.IsDeleted)
                    .OrderByDescending(r => r.LastMessageAt ?? r.CreatedAt)
                    .ToListAsync();

                var roomDtos = rooms.Select(r =>
                {
                    var otherStaff = r.Staff1Id == currentStaff.Id ? r.Staff2 : r.Staff1;
                    var unreadCount = r.Staff1Id == currentStaff.Id ? r.Staff1UnreadCount : r.Staff2UnreadCount;

                    return new StaffChatRoomDto
                    {
                        Id = r.Id,
                        OtherStaff = new StaffColleagueDto
                        {
                            Id = otherStaff?.Id ?? 0,
                            FullName = otherStaff?.FullName ?? "",
                            NickName = otherStaff?.NickName,
                            AvatarUrl = otherStaff?.AvatarUrl,
                            Position = otherStaff?.Position ?? "",
                            IsOnline = false
                        },
                        LastMessageText = r.LastMessageText,
                        LastMessageAt = r.LastMessageAt,
                        LastMessageTimeAgo = r.LastMessageAt.HasValue ? GetTimeAgo(r.LastMessageAt.Value) : null,
                        UnreadCount = unreadCount,
                        IsMuted = r.Staff1Id == currentStaff.Id ? r.Staff1Muted : r.Staff2Muted
                    };
                }).ToList();

                return Ok(new { success = true, data = roomDtos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phòng chat");
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy hoặc tạo phòng chat với staff khác
        /// </summary>
        [HttpGet("rooms/{targetStaffId}")]
        public async Task<IActionResult> GetOrCreateRoom(int targetStaffId)
        {
            try
            {
                var currentStaff = await GetCurrentStaff();
                if (currentStaff == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                if (currentStaff.Id == targetStaffId)
                {
                    return BadRequest(new { message = "Không thể chat với chính mình" });
                }

                var targetStaff = await _db.Staff
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == targetStaffId && !s.IsDeleted);

                if (targetStaff == null)
                {
                    return NotFound(new { message = "Không tìm thấy nhân viên" });
                }

                // Tìm room đã tồn tại
                var room = await _db.StaffChatRooms
                    .Include(r => r.Staff1)
                    .Include(r => r.Staff2)
                    .FirstOrDefaultAsync(r =>
                        ((r.Staff1Id == currentStaff.Id && r.Staff2Id == targetStaffId) ||
                         (r.Staff1Id == targetStaffId && r.Staff2Id == currentStaff.Id)) &&
                        !r.IsDeleted);

                if (room == null)
                {
                    // Tạo room mới
                    room = new StaffChatRoom
                    {
                        Staff1Id = currentStaff.Id,
                        Staff2Id = targetStaffId
                    };
                    _db.StaffChatRooms.Add(room);
                    await _db.SaveChangesAsync();

                    // Reload với navigation properties
                    room = await _db.StaffChatRooms
                        .Include(r => r.Staff1)
                        .Include(r => r.Staff2)
                        .FirstAsync(r => r.Id == room.Id);
                }

                var otherStaff = room.Staff1Id == currentStaff.Id ? room.Staff2 : room.Staff1;
                var unreadCount = room.Staff1Id == currentStaff.Id ? room.Staff1UnreadCount : room.Staff2UnreadCount;

                var roomDto = new StaffChatRoomDto
                {
                    Id = room.Id,
                    OtherStaff = new StaffColleagueDto
                    {
                        Id = otherStaff?.Id ?? 0,
                        FullName = otherStaff?.FullName ?? "",
                        NickName = otherStaff?.NickName,
                        AvatarUrl = otherStaff?.AvatarUrl,
                        Position = otherStaff?.Position ?? "",
                        IsOnline = false
                    },
                    LastMessageText = room.LastMessageText,
                    LastMessageAt = room.LastMessageAt,
                    UnreadCount = unreadCount
                };

                return Ok(new { success = true, data = roomDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi get/create phòng chat");
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy tin nhắn trong phòng chat
        /// </summary>
        [HttpGet("messages/{roomId}")]
        public async Task<IActionResult> GetMessages(int roomId, [FromQuery] int page = 1, [FromQuery] int limit = 50)
        {
            try
            {
                var currentStaff = await GetCurrentStaff();
                if (currentStaff == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                var room = await _db.StaffChatRooms
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == roomId && 
                        (r.Staff1Id == currentStaff.Id || r.Staff2Id == currentStaff.Id) &&
                        !r.IsDeleted);

                if (room == null)
                {
                    return NotFound(new { message = "Không tìm thấy phòng chat" });
                }

                var messages = await _db.StaffChatMessages
                    .AsNoTracking()
                    .Include(m => m.Sender)
                    .Where(m => m.ChatRoomId == roomId && !m.IsDeleted)
                    .OrderByDescending(m => m.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var messageDtos = messages.Select(m => new StaffChatMessageDto
                {
                    Id = m.Id,
                    ChatRoomId = m.ChatRoomId,
                    SenderId = m.SenderId,
                    SenderName = m.Sender?.FullName ?? "",
                    SenderAvatar = m.Sender?.AvatarUrl,
                    Content = m.Content,
                    MessageType = m.MessageType,
                    AttachmentUrl = m.AttachmentUrl,
                    FileName = m.FileName,
                    FileSize = m.FileSize,
                    CreatedAt = m.CreatedAt,
                    TimeAgo = GetTimeAgo(m.CreatedAt),
                    IsRead = m.IsRead,
                    ReadAt = m.ReadAt,
                    IsDeleted = m.IsDeleted,
                    IsMe = m.SenderId == currentStaff.Id
                }).Reverse().ToList(); // Reverse để oldest first

                return Ok(new { success = true, data = messageDtos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tin nhắn");
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        /// <summary>
        /// Gửi tin nhắn
        /// </summary>
        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage([FromBody] SendStaffMessageRequest request)
        {
            try
            {
                var currentStaff = await GetCurrentStaff();
                if (currentStaff == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                var room = await _db.StaffChatRooms
                    .FirstOrDefaultAsync(r => r.Id == request.ChatRoomId && 
                        (r.Staff1Id == currentStaff.Id || r.Staff2Id == currentStaff.Id) &&
                        !r.IsDeleted);

                if (room == null)
                {
                    return NotFound(new { message = "Không tìm thấy phòng chat" });
                }

                // Tạo tin nhắn
                var message = new StaffChatMessage
                {
                    ChatRoomId = request.ChatRoomId,
                    SenderId = currentStaff.Id,
                    Content = request.Content,
                    MessageType = request.MessageType ?? "text",
                    AttachmentUrl = request.AttachmentUrl,
                    FileName = request.FileName,
                    FileSize = request.FileSize
                };
                _db.StaffChatMessages.Add(message);

                // Cập nhật room
                room.LastMessageText = request.Content.Length > 200 
                    ? request.Content.Substring(0, 200) + "..." 
                    : request.Content;
                room.LastMessageSenderId = currentStaff.Id;
                room.LastMessageAt = DateTime.UtcNow;
                room.UpdatedAt = DateTime.UtcNow;

                // Tăng unread count cho người kia
                if (room.Staff1Id == currentStaff.Id)
                {
                    room.Staff2UnreadCount++;
                }
                else
                {
                    room.Staff1UnreadCount++;
                }

                await _db.SaveChangesAsync();

                // Tạo DTO để trả về và broadcast
                var messageDto = new StaffChatMessageDto
                {
                    Id = message.Id,
                    ChatRoomId = message.ChatRoomId,
                    SenderId = message.SenderId,
                    SenderName = currentStaff.FullName,
                    SenderAvatar = currentStaff.AvatarUrl,
                    Content = message.Content,
                    MessageType = message.MessageType,
                    AttachmentUrl = message.AttachmentUrl,
                    FileName = message.FileName,
                    FileSize = message.FileSize,
                    CreatedAt = message.CreatedAt,
                    TimeAgo = "Vừa xong",
                    IsRead = false,
                    IsDeleted = false,
                    IsMe = true
                };

                // Broadcast qua SignalR
                await _hubContext.Clients.Group($"ChatRoom_{request.ChatRoomId}")
                    .SendAsync("ReceiveStaffMessage", messageDto);

                // Notify người nhận (nếu không trong room)
                var otherStaffId = room.Staff1Id == currentStaff.Id ? room.Staff2Id : room.Staff1Id;
                await _hubContext.Clients.Group($"Staff_{otherStaffId}")
                    .SendAsync("NewChatMessage", new
                    {
                        RoomId = room.Id,
                        SenderId = currentStaff.Id,
                        SenderName = currentStaff.FullName,
                        Preview = messageDto.Content.Length > 50 
                            ? messageDto.Content.Substring(0, 50) + "..." 
                            : messageDto.Content
                    });

                return Ok(new { success = true, data = messageDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi tin nhắn");
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        /// <summary>
        /// Đánh dấu đã đọc
        /// </summary>
        [HttpPut("messages/{roomId}/read")]
        public async Task<IActionResult> MarkAsRead(int roomId)
        {
            try
            {
                var currentStaff = await GetCurrentStaff();
                if (currentStaff == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                var room = await _db.StaffChatRooms
                    .FirstOrDefaultAsync(r => r.Id == roomId && 
                        (r.Staff1Id == currentStaff.Id || r.Staff2Id == currentStaff.Id));

                if (room == null)
                {
                    return NotFound(new { message = "Không tìm thấy phòng chat" });
                }

                // Reset unread count
                if (room.Staff1Id == currentStaff.Id)
                {
                    room.Staff1UnreadCount = 0;
                }
                else
                {
                    room.Staff2UnreadCount = 0;
                }

                // Đánh dấu tin nhắn đã đọc
                var unreadMessages = await _db.StaffChatMessages
                    .Where(m => m.ChatRoomId == roomId && 
                        m.SenderId != currentStaff.Id && 
                        !m.IsRead)
                    .ToListAsync();

                var now = DateTime.UtcNow;
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                    msg.ReadAt = now;
                }

                await _db.SaveChangesAsync();

                // Broadcast qua SignalR
                var messageIds = unreadMessages.Select(m => m.Id).ToList();
                await _hubContext.Clients.Group($"ChatRoom_{roomId}")
                    .SendAsync("MessagesMarkedAsRead", new { RoomId = roomId, MessageIds = messageIds });

                return Ok(new { success = true, message = "Đã đánh dấu đã đọc" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu đã đọc");
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        #region Private Methods

        private async Task<Staff?> GetCurrentStaff()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return null;

            // Tìm Staff record hiện có
            var staff = await _db.Staff
                .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);

            if (staff != null) return staff;

            // Auto-create Staff record nếu user có role Staff
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Staff") && !roles.Contains("Admin")) return null;

            _logger.LogInformation("🔄 [StaffChat] Auto-creating Staff record for user {UserId}", userId);

            staff = new Staff
            {
                UserId = userId,
                StaffCode = $"STF{DateTime.UtcNow:yyyyMMddHHmmss}",
                FullName = user.FullName ?? user.UserName ?? "Staff",
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Position = "Barber",
                Level = "Junior",
                Status = "Active",
                IsAvailable = true,
                AcceptOnlineBooking = true,
                HireDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _db.Staff.Add(staff);
            await _db.SaveChangesAsync();

            _logger.LogInformation("✅ [StaffChat] Created Staff record Id={StaffId}", staff.Id);

            return staff;
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var span = DateTime.UtcNow - dateTime;

            if (span.TotalMinutes < 1) return "Vừa xong";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} phút";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} giờ";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays} ngày";
            if (span.TotalDays < 30) return $"{(int)(span.TotalDays / 7)} tuần";
            if (span.TotalDays < 365) return $"{(int)(span.TotalDays / 30)} tháng";
            return $"{(int)(span.TotalDays / 365)} năm";
        }

        #endregion
    }
}
