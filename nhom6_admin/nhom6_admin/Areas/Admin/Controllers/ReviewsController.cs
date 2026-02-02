using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Models;

namespace nhom6_admin.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Reviews
        public IActionResult Index()
        {
            return View();
        }

        // GET: Admin/Reviews/GetData
        [HttpGet]
        public IActionResult GetData(string? search, int? rating, string? status, string? type, int page = 1, int pageSize = 10)
        {
            var query = _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Product)
                .Include(r => r.Service)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => 
                    r.Comment.Contains(search) || 
                    (r.Customer != null && r.Customer.FullName != null && r.Customer.FullName.Contains(search)) ||
                    (r.Customer != null && r.Customer.Email != null && r.Customer.Email.Contains(search)));
            }

            if (rating.HasValue)
            {
                query = query.Where(r => r.Rating == rating.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "approved")
                    query = query.Where(r => r.IsApproved);
                else if (status == "pending")
                    query = query.Where(r => !r.IsApproved);
            }

            if (!string.IsNullOrEmpty(type))
            {
                if (type == "product")
                    query = query.Where(r => r.ProductId != null);
                else if (type == "service")
                    query = query.Where(r => r.ServiceId != null);
            }

            var total = query.Count();
            var data = query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.Id,
                    CustomerName = r.Customer != null ? r.Customer.FullName : "Ẩn danh",
                    CustomerAvatar = r.Customer != null ? r.Customer.AvatarUrl : null,
                    CustomerEmail = r.Customer != null ? r.Customer.Email : null,
                    r.Rating,
                    r.Comment,
                    r.AdminReply,
                    r.IsApproved,
                    Type = r.ProductId != null ? "product" : "service",
                    ItemName = r.ProductId != null ? (r.Product != null ? r.Product.Name : "N/A") : (r.Service != null ? r.Service.Name : "N/A"),
                    ItemImage = r.ProductId != null ? (r.Product != null ? r.Product.ImageUrl : null) : (r.Service != null ? r.Service.ImageUrl : null),
                    CreatedAt = r.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    RepliedAt = r.RepliedAt.HasValue ? r.RepliedAt.Value.ToString("dd/MM/yyyy HH:mm") : null
                })
                .ToList();

            return Json(new { data, total, page, pageSize, totalPages = (int)Math.Ceiling(total / (double)pageSize) });
        }

        // GET: Admin/Reviews/GetStats
        [HttpGet]
        public IActionResult GetStats()
        {
            var reviews = _context.Reviews.ToList();
            
            return Json(new
            {
                total = reviews.Count,
                approved = reviews.Count(r => r.IsApproved),
                pending = reviews.Count(r => !r.IsApproved),
                avgRating = reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 1) : 0,
                fiveStars = reviews.Count(r => r.Rating == 5),
                fourStars = reviews.Count(r => r.Rating == 4),
                threeStars = reviews.Count(r => r.Rating == 3),
                twoStars = reviews.Count(r => r.Rating == 2),
                oneStars = reviews.Count(r => r.Rating == 1)
            });
        }

        // POST: Admin/Reviews/Approve/5
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return Json(new { success = false, message = "Không tìm thấy đánh giá" });

            review.IsApproved = true;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã duyệt đánh giá" });
        }

        // POST: Admin/Reviews/Reject/5
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return Json(new { success = false, message = "Không tìm thấy đánh giá" });

            review.IsApproved = false;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã từ chối đánh giá" });
        }

        // POST: Admin/Reviews/Reply
        [HttpPost]
        public async Task<IActionResult> Reply([FromBody] ReplyModel model)
        {
            var review = await _context.Reviews.FindAsync(model.Id);
            if (review == null)
                return Json(new { success = false, message = "Không tìm thấy đánh giá" });

            review.AdminReply = model.Reply;
            review.RepliedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã phản hồi đánh giá" });
        }

        // POST: Admin/Reviews/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return Json(new { success = false, message = "Không tìm thấy đánh giá" });

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa đánh giá" });
        }

        // POST: Admin/Reviews/BulkApprove
        [HttpPost]
        public async Task<IActionResult> BulkApprove([FromBody] int[] ids)
        {
            var reviews = await _context.Reviews.Where(r => ids.Contains(r.Id)).ToListAsync();
            foreach (var review in reviews)
            {
                review.IsApproved = true;
            }
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Đã duyệt {reviews.Count} đánh giá" });
        }

        public class ReplyModel
        {
            public int Id { get; set; }
            public string Reply { get; set; } = string.Empty;
        }
    }
}
