using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using nhom6_admin.Models;
using nhom6_admin.Models.Entities;
using nhom6_admin.Services;
using nhom6_admin.Hubs;

namespace nhom6_admin.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IAdminNotificationService _notificationService;

        public AppointmentsController(
            ApplicationDbContext context, 
            IEmailService emailService,
            IAdminNotificationService notificationService)
        {
            _context = context;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        // GET: Admin/Appointments
        public async Task<IActionResult> Index()
        {
            await LoadSelectLists();
            return View();
        }

        // GET: Admin/Appointments/GetEvents
        [HttpGet]
        public async Task<IActionResult> GetEvents(string start, string end)
        {
            if (!DateTime.TryParse(start, out var startDate))
                startDate = DateTime.Today.AddMonths(-1);
            if (!DateTime.TryParse(end, out var endDate))
                endDate = DateTime.Today.AddMonths(1);

            var appointments = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Staff)
                .Include(a => a.AppointmentServices!)
                    .ThenInclude(aps => aps.Service)
                .Where(a => !a.IsDeleted && a.AppointmentDate >= startDate.Date && a.AppointmentDate <= endDate.Date)
                .ToListAsync();

            var events = appointments.Select(a => new
            {
                id = a.Id,
                title = $"{a.GuestName ?? a.User?.FullName ?? "Khách"} - {GetServiceNames(a.AppointmentServices)}",
                start = a.AppointmentDate.Add(a.StartTime).ToString("yyyy-MM-ddTHH:mm:ss"),
                end = a.AppointmentDate.Add(a.EndTime).ToString("yyyy-MM-ddTHH:mm:ss"),
                backgroundColor = GetStatusColor(a.Status),
                borderColor = GetStatusColor(a.Status),
                extendedProps = new
                {
                    status = a.Status,
                    customerName = a.GuestName ?? a.User?.FullName ?? "Khách vãng lai",
                    customerPhone = a.GuestPhone ?? a.User?.PhoneNumber ?? "-",
                    staffName = a.Staff?.FullName,
                    services = GetServiceNames(a.AppointmentServices),
                    totalAmount = a.TotalAmount
                }
            });

            return Json(events);
        }

        // GET: Admin/Appointments/GetData (for table view)
        [HttpGet]
        public async Task<IActionResult> GetData(int draw, int start, int length, string? search, string? status, string? date)
        {
            var query = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Staff)
                .Include(a => a.AppointmentServices!)
                    .ThenInclude(aps => aps.Service)
                .Where(a => !a.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.ToLower();
                query = query.Where(a => 
                    (a.User != null && a.User.FullName != null && a.User.FullName.ToLower().Contains(s)) ||
                    (a.User != null && a.User.PhoneNumber != null && a.User.PhoneNumber.Contains(search)));
            }

            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.Status == status);

            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var filterDate))
                query = query.Where(a => a.AppointmentDate.Date == filterDate.Date);

            var totalRecords = await _context.Appointments.CountAsync(a => !a.IsDeleted);
            var filteredRecords = await query.CountAsync();

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Skip(start)
                .Take(length)
                .Select(a => new
                {
                    a.Id,
                    CustomerName = a.GuestName ?? (a.User != null ? a.User.FullName : "Khách vãng lai"),
                    CustomerPhone = a.GuestPhone ?? (a.User != null ? a.User.PhoneNumber : "") ?? "-",
                    StaffName = a.Staff != null ? a.Staff.FullName : "-",
                    Services = string.Join(", ", a.AppointmentServices!.Select(s => s.Service!.Name)),
                    Date = a.AppointmentDate.ToString("dd/MM/yyyy"),
                    Time = $"{a.StartTime:hh\\:mm} - {a.EndTime:hh\\:mm}",
                    a.TotalAmount,
                    a.Status
                })
                .ToListAsync();

            return Json(new { draw, recordsTotal = totalRecords, recordsFiltered = filteredRecords, data = appointments });
        }

        // POST: Admin/Appointments/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return Json(new { success = false, message = "Lịch hẹn không tồn tại" });

            appointment.Status = status;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã cập nhật trạng thái" });
        }

        // POST: Admin/Appointments/Cancel
        [HttpPost]
        public async Task<IActionResult> Cancel(int id, string? reason)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return Json(new { success = false, message = "Lịch hẹn không tồn tại" });

            appointment.Status = "Cancelled";
            appointment.CancellationReason = reason;
            appointment.CancelledAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã hủy lịch hẹn" });
        }

        // POST: Admin/Appointments/Confirm
        [HttpPost]
        public async Task<IActionResult> Confirm(int id, int? staffId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.AppointmentServices!)
                    .ThenInclude(aps => aps.Service)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (appointment == null)
                return Json(new { success = false, message = "Lịch hẹn không tồn tại" });

            if (appointment.Status != "Pending")
                return Json(new { success = false, message = "Chỉ có thể xác nhận lịch hẹn ở trạng thái Chờ xác nhận" });

            // Cập nhật trạng thái
            appointment.Status = "Confirmed";
            appointment.StaffId = staffId;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Gửi email xác nhận cho khách hàng
            _ = Task.Run(async () =>
            {
                await _emailService.SendAppointmentConfirmationToCustomer(appointment);
            });

            // Gửi realtime notification cho user nếu có userId
            if (!string.IsNullOrEmpty(appointment.UserId))
            {
                _ = Task.Run(async () =>
                {
                    var message = $"Lịch hẹn #{appointment.AppointmentCode} của bạn đã được xác nhận. Vui lòng đến đúng giờ vào {appointment.AppointmentDate:dd/MM/yyyy} lúc {appointment.StartTime:hh\\:mm}.";
                    await _notificationService.SendNotificationToUser(appointment.UserId, "Lịch hẹn đã được xác nhận", message);
                });
            }

            return Json(new { success = true, message = "Đã xác nhận lịch hẹn và gửi thông báo cho khách hàng" });
        }

        // GET: Admin/Appointments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Staff)
                .Include(a => a.AppointmentServices!)
                    .ThenInclude(aps => aps.Service)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (appointment == null)
                return NotFound();

            return PartialView("_Details", appointment);
        }

        #region Helpers

        private async Task LoadSelectLists()
        {
            ViewBag.Staff = await _context.Staff
                .Where(s => !s.IsDeleted && s.Status == "Active")
                .OrderBy(s => s.FullName)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.FullName })
                .ToListAsync();

            ViewBag.Services = await _context.Services
                .Where(s => !s.IsDeleted && s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = $"{s.Name} ({s.Price:N0}đ)" })
                .ToListAsync();

            ViewBag.StatusList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Pending", Text = "Chờ xác nhận" },
                new SelectListItem { Value = "Confirmed", Text = "Đã xác nhận" },
                new SelectListItem { Value = "InProgress", Text = "Đang thực hiện" },
                new SelectListItem { Value = "Completed", Text = "Hoàn thành" },
                new SelectListItem { Value = "Cancelled", Text = "Đã hủy" }
            };
        }

        private string GetServiceNames(ICollection<AppointmentService>? services)
        {
            if (services == null || !services.Any()) return "-";
            return string.Join(", ", services.Where(s => s.Service != null).Select(s => s.Service!.Name));
        }

        private string GetStatusColor(string status)
        {
            return status switch
            {
                "Pending" => "#FF9800",      // Orange
                "Confirmed" => "#2196F3",    // Blue
                "InProgress" => "#9C27B0",   // Purple
                "Completed" => "#4CAF50",    // Green
                "Cancelled" => "#F44336",    // Red
                _ => "#9E9E9E"               // Grey
            };
        }

        #endregion
    }
}
