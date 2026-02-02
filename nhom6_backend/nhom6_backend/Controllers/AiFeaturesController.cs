using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_backend.Models;
using nhom6_backend.Models.DTOs;
using nhom6_backend.Models.Entities;
using System.Security.Claims;
using System.Text.Json;

namespace nhom6_backend.Controllers
{
    /// <summary>
    /// API Controller cho các tính năng AI: Face Analysis, Hair Try-On, Chat History
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AiFeaturesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AiFeaturesController> _logger;

        public AiFeaturesController(
            ApplicationDbContext context,
            ILogger<AiFeaturesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ==========================================
        // FACE ANALYSIS ENDPOINTS
        // ==========================================

        /// <summary>
        /// Lưu kết quả phân tích khuôn mặt
        /// </summary>
        [HttpPost("face-analysis")]
        [Authorize]
        public async Task<ActionResult<FaceAnalysisResponseDto>> SaveFaceAnalysis(
            [FromBody] FaceAnalysisRequestDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var faceAnalysis = new FaceAnalysis
                {
                    UserId = userId,
                    OriginalImageUrl = request.OriginalImageUrl,
                    FaceShape = request.FaceShape,
                    FaceShapeConfidence = request.Confidence,
                    SuggestedHairStyles = JsonSerializer.Serialize(request.RecommendedHairstyles),
                    FaceProportions = request.MeasurementsJson,
                    AiModel = "Google ML Kit",
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow
                };

                _context.FaceAnalyses.Add(faceAnalysis);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Face analysis saved for user {UserId}, result: {FaceShape}", 
                    userId, request.FaceShape);

                return Ok(new FaceAnalysisResponseDto
                {
                    Id = faceAnalysis.Id,
                    OriginalImageUrl = faceAnalysis.OriginalImageUrl,
                    FaceShape = faceAnalysis.FaceShape,
                    Confidence = faceAnalysis.FaceShapeConfidence,
                    Description = request.Description,
                    RecommendedHairstyles = request.RecommendedHairstyles,
                    CreatedAt = faceAnalysis.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving face analysis");
                return StatusCode(500, new { message = "Lỗi khi lưu kết quả phân tích" });
            }
        }

        /// <summary>
        /// Lấy lịch sử phân tích khuôn mặt của user
        /// </summary>
        [HttpGet("face-analysis/history")]
        [Authorize]
        public async Task<ActionResult<List<FaceAnalysisResponseDto>>> GetFaceAnalysisHistory(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var analyses = await _context.FaceAnalyses
                .Where(f => f.UserId == userId && f.Status == "Completed")
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FaceAnalysisResponseDto
                {
                    Id = f.Id,
                    OriginalImageUrl = f.OriginalImageUrl,
                    FaceShape = f.FaceShape,
                    Confidence = f.FaceShapeConfidence,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();

            // Parse JSON outside of LINQ expression tree
            foreach (var analysis in analyses)
            {
                var entity = await _context.FaceAnalyses.FindAsync(analysis.Id);
                if (entity != null && !string.IsNullOrEmpty(entity.SuggestedHairStyles))
                {
                    analysis.RecommendedHairstyles = JsonSerializer.Deserialize<List<string>>(entity.SuggestedHairStyles) ?? new List<string>();
                }
            }

            return Ok(analyses);
        }

        /// <summary>
        /// Xóa một kết quả phân tích
        /// </summary>
        [HttpDelete("face-analysis/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteFaceAnalysis(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var analysis = await _context.FaceAnalyses
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (analysis == null)
                return NotFound(new { message = "Không tìm thấy kết quả phân tích" });

            _context.FaceAnalyses.Remove(analysis);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa kết quả phân tích" });
        }

        // ==========================================
        // HAIR TRY-ON ENDPOINTS
        // ==========================================

        /// <summary>
        /// Lưu kết quả thử tóc ảo
        /// </summary>
        [HttpPost("hair-tryon")]
        [Authorize]
        public async Task<ActionResult<HairTryOnResponseDto>> SaveHairTryOn(
            [FromBody] HairTryOnRequestDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var hairTryOn = new HairStyleTryOn
                {
                    UserId = userId,
                    FaceImageUrl = request.FaceImageUrl,
                    HairStyleImageUrl = request.HairStyleImageUrl,
                    ResultImageUrl = request.ResultImageUrl,
                    HairStyleName = request.HairStyleName,
                    AiModel = request.AiModel,
                    IsSaved = request.IsSaved ?? false,
                    Status = string.IsNullOrEmpty(request.ResultImageUrl) ? "Processing" : "Completed",
                    CreatedAt = DateTime.UtcNow
                };

                _context.HairStyleTryOns.Add(hairTryOn);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Hair try-on saved for user {UserId}", userId);

                return Ok(new HairTryOnResponseDto
                {
                    Id = hairTryOn.Id,
                    FaceImageUrl = hairTryOn.FaceImageUrl,
                    HairStyleImageUrl = hairTryOn.HairStyleImageUrl,
                    ResultImageUrl = hairTryOn.ResultImageUrl,
                    HairStyleName = hairTryOn.HairStyleName,
                    AiModel = hairTryOn.AiModel,
                    IsSaved = hairTryOn.IsSaved,
                    CreatedAt = hairTryOn.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving hair try-on");
                return StatusCode(500, new { message = "Lỗi khi lưu kết quả" });
            }
        }

        /// <summary>
        /// Lấy lịch sử thử tóc ảo của user
        /// </summary>
        [HttpGet("hair-tryon/history")]
        [Authorize]
        public async Task<ActionResult<List<HairTryOnResponseDto>>> GetHairTryOnHistory(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var tryOns = await _context.HairStyleTryOns
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new HairTryOnResponseDto
                {
                    Id = h.Id,
                    FaceImageUrl = h.FaceImageUrl,
                    HairStyleImageUrl = h.HairStyleImageUrl,
                    ResultImageUrl = h.ResultImageUrl,
                    HairStyleName = h.HairStyleName,
                    AiModel = h.AiModel,
                    IsSaved = h.IsSaved,
                    CreatedAt = h.CreatedAt
                })
                .ToListAsync();

            return Ok(tryOns);
        }

        /// <summary>
        /// Cập nhật trạng thái lưu/bỏ lưu cho hair try-on
        /// </summary>
        [HttpPatch("hair-tryon/{id}/save")]
        [Authorize]
        public async Task<IActionResult> UpdateHairTryOnSave(int id, [FromBody] bool isSaved)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tryOn = await _context.HairStyleTryOns
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (tryOn == null)
                return NotFound(new { message = "Không tìm thấy kết quả" });

            tryOn.IsSaved = isSaved;
            tryOn.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = isSaved ? "Đã lưu" : "Đã bỏ lưu" });
        }

        /// <summary>
        /// Xóa một kết quả thử tóc
        /// </summary>
        [HttpDelete("hair-tryon/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteHairTryOn(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tryOn = await _context.HairStyleTryOns
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (tryOn == null)
                return NotFound(new { message = "Không tìm thấy kết quả" });

            _context.HairStyleTryOns.Remove(tryOn);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa kết quả" });
        }

        // ==========================================
        // CHAT HISTORY ENDPOINTS
        // ==========================================

        /// <summary>
        /// Tạo phiên chat mới
        /// </summary>
        [HttpPost("chat/sessions")]
        [Authorize]
        public async Task<ActionResult<ChatSessionResponseDto>> CreateChatSession(
            [FromBody] CreateChatSessionRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var session = new ChatSession
            {
                UserId = userId,
                Title = request.Title ?? "Cuộc trò chuyện mới",
                SessionType = request.SessionType,
                AiModel = "Gemini 1.5 Flash",
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok(new ChatSessionResponseDto
            {
                Id = session.Id,
                Title = session.Title,
                SessionType = session.SessionType,
                MessageCount = 0,
                CreatedAt = session.CreatedAt
            });
        }

        /// <summary>
        /// Lưu tin nhắn vào session
        /// </summary>
        [HttpPost("chat/sessions/{sessionId}/messages")]
        [Authorize]
        public async Task<ActionResult<ChatMessageResponseDto>> SaveChatMessage(
            int sessionId,
            [FromBody] SendMessageRequestDto request,
            [FromQuery] string role = "user")
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

            if (session == null)
                return NotFound(new { message = "Không tìm thấy phiên chat" });

            var message = new ChatMessage
            {
                ChatSessionId = sessionId,
                Content = request.Content,
                Role = role,
                Images = request.ImageUrl, // Stored as JSON or single URL
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(message);
            
            // Update session
            session.MessageCount++;
            session.LastMessageAt = DateTime.UtcNow;
            session.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            return Ok(new ChatMessageResponseDto
            {
                Id = message.Id,
                Content = message.Content,
                Role = message.Role,
                ImageUrl = message.Images,
                CreatedAt = message.CreatedAt
            });
        }

        /// <summary>
        /// Lấy danh sách phiên chat của user
        /// </summary>
        [HttpGet("chat/sessions")]
        [Authorize]
        public async Task<ActionResult<List<ChatSessionResponseDto>>> GetChatSessions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var sessions = await _context.ChatSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ChatSessionResponseDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    SessionType = s.SessionType,
                    MessageCount = s.MessageCount,
                    CreatedAt = s.CreatedAt,
                    LastMessageAt = s.UpdatedAt
                })
                .ToListAsync();

            return Ok(sessions);
        }

        /// <summary>
        /// Lấy chi tiết phiên chat với tin nhắn
        /// </summary>
        [HttpGet("chat/sessions/{sessionId}")]
        [Authorize]
        public async Task<ActionResult<ChatSessionResponseDto>> GetChatSession(int sessionId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var session = await _context.ChatSessions
                .Where(s => s.Id == sessionId && s.UserId == userId)
                .Select(s => new ChatSessionResponseDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    SessionType = s.SessionType,
                    MessageCount = s.MessageCount,
                    CreatedAt = s.CreatedAt,
                    LastMessageAt = s.UpdatedAt,
                    Messages = _context.ChatMessages
                        .Where(m => m.ChatSessionId == sessionId)
                        .OrderBy(m => m.CreatedAt)
                        .Select(m => new ChatMessageResponseDto
                        {
                            Id = m.Id,
                            Content = m.Content ?? string.Empty,
                            Role = m.Role,
                            ImageUrl = m.Images ?? string.Empty,
                            CreatedAt = m.CreatedAt
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (session == null)
                return NotFound(new { message = "Không tìm thấy phiên chat" });

            return Ok(session);
        }

        /// <summary>
        /// Xóa phiên chat
        /// </summary>
        [HttpDelete("chat/sessions/{sessionId}")]
        [Authorize]
        public async Task<IActionResult> DeleteChatSession(int sessionId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var session = await _context.ChatSessions
                .Include(s => s.ChatMessages)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

            if (session == null)
                return NotFound(new { message = "Không tìm thấy phiên chat" });

            // Delete all messages first
            if (session.ChatMessages != null)
            {
                _context.ChatMessages.RemoveRange(session.ChatMessages);
            }
            _context.ChatSessions.Remove(session);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa phiên chat" });
        }

        // ==========================================
        // COMBINED HISTORY ENDPOINT
        // ==========================================

        /// <summary>
        /// Lấy toàn bộ lịch sử AI của user (face analysis, hair try-on, chat)
        /// </summary>
        [HttpGet("history")]
        [Authorize]
        public async Task<ActionResult<AiHistoryResponseDto>> GetAllAiHistory(
            [FromQuery] int limit = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var faceAnalyses = await _context.FaceAnalyses
                .Where(f => f.UserId == userId && f.Status == "Completed")
                .OrderByDescending(f => f.CreatedAt)
                .Take(limit)
                .Select(f => new FaceAnalysisResponseDto
                {
                    Id = f.Id,
                    OriginalImageUrl = f.OriginalImageUrl,
                    FaceShape = f.FaceShape,
                    Confidence = f.FaceShapeConfidence,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();

            var hairTryOns = await _context.HairStyleTryOns
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.CreatedAt)
                .Take(limit)
                .Select(h => new HairTryOnResponseDto
                {
                    Id = h.Id,
                    FaceImageUrl = h.FaceImageUrl,
                    HairStyleImageUrl = h.HairStyleImageUrl,
                    ResultImageUrl = h.ResultImageUrl,
                    HairStyleName = h.HairStyleName,
                    IsSaved = h.IsSaved,
                    CreatedAt = h.CreatedAt
                })
                .ToListAsync();

            var chatSessions = await _context.ChatSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt)
                .Take(limit)
                .Select(s => new ChatSessionResponseDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    SessionType = s.SessionType,
                    MessageCount = s.MessageCount,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return Ok(new AiHistoryResponseDto
            {
                FaceAnalyses = faceAnalyses,
                HairTryOns = hairTryOns,
                ChatSessions = chatSessions
            });
        }

        // ==========================================
        // STATISTICS ENDPOINT (Optional - for admin)
        // ==========================================

        /// <summary>
        /// Thống kê AI features (chỉ admin)
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStatistics()
        {
            var totalFaceAnalyses = await _context.FaceAnalyses.CountAsync();
            var totalHairTryOns = await _context.HairStyleTryOns.CountAsync();
            var totalChatSessions = await _context.ChatSessions.CountAsync();
            var totalChatMessages = await _context.ChatMessages.CountAsync();

            var faceShapeDistribution = await _context.FaceAnalyses
                .Where(f => f.FaceShape != null)
                .GroupBy(f => f.FaceShape)
                .Select(g => new { FaceShape = g.Key, Count = g.Count() })
                .ToListAsync();

            return Ok(new
            {
                TotalFaceAnalyses = totalFaceAnalyses,
                TotalHairTryOns = totalHairTryOns,
                TotalChatSessions = totalChatSessions,
                TotalChatMessages = totalChatMessages,
                FaceShapeDistribution = faceShapeDistribution
            });
        }
    }
}
