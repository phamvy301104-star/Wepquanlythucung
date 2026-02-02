using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nhom6_admin.Models;
using nhom6_admin.Models.Entities;

namespace nhom6_admin.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Settings
        public IActionResult Index()
        {
            var settings = _context.SiteSettings.ToDictionary(s => s.Key, s => s.Value);
            return View(settings);
        }

        // POST: Admin/Settings/SaveGeneral
        [HttpPost]
        public async Task<IActionResult> SaveGeneral([FromBody] GeneralSettingsModel model)
        {
            try
            {
                await SaveSetting("SiteName", model.SiteName);
                await SaveSetting("SiteDescription", model.SiteDescription);
                await SaveSetting("SiteLogo", model.SiteLogo);
                await SaveSetting("SiteFavicon", model.SiteFavicon);
                await SaveSetting("ContactEmail", model.ContactEmail);
                await SaveSetting("ContactPhone", model.ContactPhone);
                await SaveSetting("ContactAddress", model.ContactAddress);
                await SaveSetting("WorkingHours", model.WorkingHours);
                await SaveSetting("GoogleMapsUrl", model.GoogleMapsUrl);

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã lưu cài đặt chung" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Admin/Settings/SaveSocial
        [HttpPost]
        public async Task<IActionResult> SaveSocial([FromBody] SocialSettingsModel model)
        {
            try
            {
                await SaveSetting("FacebookUrl", model.FacebookUrl);
                await SaveSetting("InstagramUrl", model.InstagramUrl);
                await SaveSetting("YoutubeUrl", model.YoutubeUrl);
                await SaveSetting("TiktokUrl", model.TiktokUrl);
                await SaveSetting("ZaloUrl", model.ZaloUrl);

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã lưu cài đặt mạng xã hội" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Admin/Settings/SaveEmail
        [HttpPost]
        public async Task<IActionResult> SaveEmail([FromBody] EmailSettingsModel model)
        {
            try
            {
                await SaveSetting("SmtpHost", model.SmtpHost);
                await SaveSetting("SmtpPort", model.SmtpPort);
                await SaveSetting("SmtpUsername", model.SmtpUsername);
                await SaveSetting("SmtpPassword", model.SmtpPassword);
                await SaveSetting("SmtpEnableSsl", model.SmtpEnableSsl.ToString());
                await SaveSetting("EmailFrom", model.EmailFrom);
                await SaveSetting("EmailFromName", model.EmailFromName);

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã lưu cài đặt email" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Admin/Settings/SavePayment
        [HttpPost]
        public async Task<IActionResult> SavePayment([FromBody] PaymentSettingsModel model)
        {
            try
            {
                await SaveSetting("EnableCOD", model.EnableCOD.ToString());
                await SaveSetting("EnableBankTransfer", model.EnableBankTransfer.ToString());
                await SaveSetting("BankName", model.BankName);
                await SaveSetting("BankAccountNumber", model.BankAccountNumber);
                await SaveSetting("BankAccountName", model.BankAccountName);
                await SaveSetting("EnableMomo", model.EnableMomo.ToString());
                await SaveSetting("MomoPhone", model.MomoPhone);
                await SaveSetting("EnableVNPay", model.EnableVNPay.ToString());
                await SaveSetting("VNPayTmnCode", model.VNPayTmnCode);
                await SaveSetting("VNPayHashSecret", model.VNPayHashSecret);

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã lưu cài đặt thanh toán" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Admin/Settings/SaveBooking
        [HttpPost]
        public async Task<IActionResult> SaveBooking([FromBody] BookingSettingsModel model)
        {
            try
            {
                await SaveSetting("BookingStartTime", model.BookingStartTime);
                await SaveSetting("BookingEndTime", model.BookingEndTime);
                await SaveSetting("BookingSlotDuration", model.BookingSlotDuration.ToString());
                await SaveSetting("MaxBookingsPerSlot", model.MaxBookingsPerSlot.ToString());
                await SaveSetting("BookingAdvanceDays", model.BookingAdvanceDays.ToString());
                await SaveSetting("RequireDeposit", model.RequireDeposit.ToString());
                await SaveSetting("DepositPercentage", model.DepositPercentage.ToString());
                await SaveSetting("CancellationHours", model.CancellationHours.ToString());

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã lưu cài đặt đặt lịch" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task SaveSetting(string key, string? value)
        {
            var setting = await _context.SiteSettings.FindAsync(key);
            if (setting != null)
            {
                setting.Value = value ?? "";
                setting.UpdatedAt = DateTime.Now;
            }
            else
            {
                _context.SiteSettings.Add(new SiteSetting
                {
                    Key = key,
                    Value = value ?? "",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
        }

        // View Models
        public class GeneralSettingsModel
        {
            public string? SiteName { get; set; }
            public string? SiteDescription { get; set; }
            public string? SiteLogo { get; set; }
            public string? SiteFavicon { get; set; }
            public string? ContactEmail { get; set; }
            public string? ContactPhone { get; set; }
            public string? ContactAddress { get; set; }
            public string? WorkingHours { get; set; }
            public string? GoogleMapsUrl { get; set; }
        }

        public class SocialSettingsModel
        {
            public string? FacebookUrl { get; set; }
            public string? InstagramUrl { get; set; }
            public string? YoutubeUrl { get; set; }
            public string? TiktokUrl { get; set; }
            public string? ZaloUrl { get; set; }
        }

        public class EmailSettingsModel
        {
            public string? SmtpHost { get; set; }
            public string? SmtpPort { get; set; }
            public string? SmtpUsername { get; set; }
            public string? SmtpPassword { get; set; }
            public bool SmtpEnableSsl { get; set; }
            public string? EmailFrom { get; set; }
            public string? EmailFromName { get; set; }
        }

        public class PaymentSettingsModel
        {
            public bool EnableCOD { get; set; }
            public bool EnableBankTransfer { get; set; }
            public string? BankName { get; set; }
            public string? BankAccountNumber { get; set; }
            public string? BankAccountName { get; set; }
            public bool EnableMomo { get; set; }
            public string? MomoPhone { get; set; }
            public bool EnableVNPay { get; set; }
            public string? VNPayTmnCode { get; set; }
            public string? VNPayHashSecret { get; set; }
        }

        public class BookingSettingsModel
        {
            public string? BookingStartTime { get; set; }
            public string? BookingEndTime { get; set; }
            public int BookingSlotDuration { get; set; }
            public int MaxBookingsPerSlot { get; set; }
            public int BookingAdvanceDays { get; set; }
            public bool RequireDeposit { get; set; }
            public int DepositPercentage { get; set; }
            public int CancellationHours { get; set; }
        }
    }
}
