using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using nhom6_backend.Models;
using nhom6_backend.Models.Entities;

namespace nhom6_backend.Services
{
    /// <summary>
    /// Email Service Implementation using MailKit
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration configuration,
            ApplicationDbContext context,
            ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        private string SmtpServer => _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
        private int SmtpPort => int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        private string SenderEmail => _configuration["EmailSettings:SenderEmail"] ?? "";
        private string SenderName => _configuration["EmailSettings:SenderName"] ?? "UME Barbershop";
        private string AdminEmail => _configuration["EmailSettings:AdminEmail"] ?? "bbaohan2212@gmail.com";
        private string Username => _configuration["EmailSettings:Username"] ?? "";
        private string Password => _configuration["EmailSettings:Password"] ?? "";
        private bool EnableSsl => bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

        public async Task<bool> SendAppointmentNotificationToAdmin(Appointment appointment)
        {
            try
            {
                _logger.LogInformation("📧 SendAppointmentNotificationToAdmin called for appointment {AppointmentId}", appointment.Id);
                
                // Kiểm tra nếu AppointmentServices chưa được load
                if (appointment.AppointmentServices == null || !appointment.AppointmentServices.Any())
                {
                    try
                    {
                        await _context.Entry(appointment)
                            .Collection(a => a.AppointmentServices!)
                            .Query()
                            .Include(s => s.Service)
                            .LoadAsync();
                    }
                    catch (Exception loadEx)
                    {
                        _logger.LogWarning(loadEx, "Could not load AppointmentServices, using default");
                    }
                }

                var services = appointment.AppointmentServices?
                    .Select(s => s.Service?.Name ?? s.ServiceName ?? "Unknown")
                    .ToList() ?? new List<string> { "Dịch vụ" };

                var customerName = !string.IsNullOrEmpty(appointment.GuestName)
                    ? appointment.GuestName
                    : appointment.User?.FullName ?? "Khách vãng lai";

                var subject = $"🔔 Lịch hẹn mới #{appointment.AppointmentCode}";
                var body = GenerateAdminNotificationHtml(appointment, customerName, services);

                _logger.LogInformation("📧 Sending email to admin: {AdminEmail}", AdminEmail);
                return await SendEmailAsync(AdminEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending admin notification for appointment {AppointmentId}", appointment.Id);
                return false;
            }
        }
        
        /// <summary>
        /// Send admin notification without requiring DbContext tracking
        /// Use this method when calling from background tasks
        /// </summary>
        public async Task<bool> SendAppointmentNotificationToAdminSimple(
            int appointmentId,
            string appointmentCode, 
            string customerName,
            string customerPhone,
            string customerEmail,
            DateTime appointmentDate,
            TimeSpan startTime,
            decimal totalAmount,
            List<string> services)
        {
            try
            {
                _logger.LogInformation("📧 SendAppointmentNotificationToAdminSimple called for appointment {AppointmentCode}", appointmentCode);
                
                var subject = $"🔔 Lịch hẹn mới #{appointmentCode}";
                var servicesHtml = string.Join("<br/>", services.Select(s => $"• {s}"));
                
                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .header {{ background-color: #2c3e50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: white; padding: 30px; border-radius: 0 0 5px 5px; }}
        .info-row {{ margin: 15px 0; padding: 10px; background-color: #ecf0f1; border-radius: 3px; }}
        .label {{ font-weight: bold; color: #2c3e50; }}
        .footer {{ text-align: center; margin-top: 20px; color: #7f8c8d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔔 LỊCH HẸN MỚI</h1>
        </div>
        <div class='content'>
            <p>Xin chào Admin,</p>
            <p>Có một lịch hẹn mới từ khách hàng!</p>
            
            <div class='info-row'>
                <span class='label'>Mã lịch hẹn:</span> #{appointmentCode}
            </div>
            <div class='info-row'>
                <span class='label'>Khách hàng:</span> {customerName}
            </div>
            <div class='info-row'>
                <span class='label'>Số điện thoại:</span> {customerPhone}
            </div>
            <div class='info-row'>
                <span class='label'>Email:</span> {customerEmail}
            </div>
            <div class='info-row'>
                <span class='label'>Ngày hẹn:</span> {appointmentDate:dd/MM/yyyy}
            </div>
            <div class='info-row'>
                <span class='label'>Giờ hẹn:</span> {startTime:hh\\:mm}
            </div>
            <div class='info-row'>
                <span class='label'>Dịch vụ:</span><br/>{servicesHtml}
            </div>
            <div class='info-row'>
                <span class='label'>Tổng tiền:</span> {totalAmount:N0}đ
            </div>
            
            <p>Vui lòng đăng nhập Admin Dashboard để xác nhận lịch hẹn!</p>
            
            <div class='footer'>
                <p>Email này được gửi tự động từ hệ thống UME Barbershop</p>
            </div>
        </div>
    </div>
</body>
</html>";

                _logger.LogInformation("📧 Sending email to admin: {AdminEmail}", AdminEmail);
                return await SendEmailAsync(AdminEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in SendAppointmentNotificationToAdminSimple");
                return false;
            }
        }

        public async Task<bool> SendAppointmentConfirmationToCustomer(Appointment appointment)
        {
            try
            {
                await _context.Entry(appointment)
                    .Collection(a => a.AppointmentServices!)
                    .Query()
                    .Include(s => s.Service)
                    .LoadAsync();

                var services = appointment.AppointmentServices?
                    .Select(s => s.Service?.Name ?? s.ServiceName ?? "Unknown")
                    .ToList() ?? new List<string>();

                var customerEmail = appointment.GuestEmail ?? appointment.User?.Email;
                if (string.IsNullOrEmpty(customerEmail))
                {
                    _logger.LogWarning("No email found for appointment {AppointmentId}", appointment.Id);
                    return false;
                }

                var customerName = !string.IsNullOrEmpty(appointment.GuestName)
                    ? appointment.GuestName
                    : appointment.User?.FullName ?? "Quý khách";

                var subject = $"✅ Xác nhận lịch hẹn #{appointment.AppointmentCode} - UME Barbershop";
                var body = GenerateCustomerConfirmationHtml(appointment, customerName, services);

                return await SendEmailAsync(customerEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending confirmation to customer for appointment {AppointmentId}", appointment.Id);
                return false;
            }
        }

        public async Task<bool> SendAppointmentCancellationEmail(Appointment appointment, string recipientEmail, bool isCustomer)
        {
            try
            {
                var customerName = !string.IsNullOrEmpty(appointment.GuestName)
                    ? appointment.GuestName
                    : appointment.User?.FullName ?? (isCustomer ? "Quý khách" : "Admin");

                var subject = $"❌ Hủy lịch hẹn #{appointment.AppointmentCode}";
                var body = GenerateCancellationHtml(appointment, customerName, isCustomer);

                return await SendEmailAsync(recipientEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending cancellation email for appointment {AppointmentId}", appointment.Id);
                return false;
            }
        }

        /// <summary>
        /// Send cancellation email without requiring DbContext - for background tasks
        /// </summary>
        public async Task<bool> SendAppointmentCancellationEmailSimple(
            string appointmentCode,
            string customerName,
            string customerPhone,
            DateTime appointmentDate,
            TimeSpan startTime,
            decimal totalAmount,
            string cancellationReason,
            string recipientEmail,
            bool isCustomer)
        {
            try
            {
                _logger.LogInformation("📧 SendAppointmentCancellationEmailSimple called for appointment {AppointmentCode}", appointmentCode);
                
                var subject = $"❌ Hủy lịch hẹn #{appointmentCode}";
                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .header {{ background-color: #e74c3c; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: white; padding: 30px; border-radius: 0 0 5px 5px; }}
        .info-row {{ margin: 15px 0; padding: 10px; background-color: #ecf0f1; border-radius: 3px; }}
        .label {{ font-weight: bold; color: #2c3e50; }}
        .reason {{ background-color: #fdf2f2; border-left: 4px solid #e74c3c; padding: 15px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #7f8c8d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>❌ HỦY LỊCH HẸN</h1>
        </div>
        <div class='content'>
            <p>Xin chào {(isCustomer ? customerName : "Admin")},</p>
            <p>{(isCustomer ? "Lịch hẹn của bạn đã bị hủy." : "Khách hàng đã hủy lịch hẹn.")}</p>
            
            <div class='info-row'>
                <span class='label'>Mã lịch hẹn:</span> #{appointmentCode}
            </div>
            <div class='info-row'>
                <span class='label'>Khách hàng:</span> {customerName}
            </div>
            <div class='info-row'>
                <span class='label'>Số điện thoại:</span> {customerPhone}
            </div>
            <div class='info-row'>
                <span class='label'>Ngày hẹn:</span> {appointmentDate:dd/MM/yyyy}
            </div>
            <div class='info-row'>
                <span class='label'>Giờ hẹn:</span> {startTime:hh\\:mm}
            </div>
            <div class='info-row'>
                <span class='label'>Tổng tiền:</span> {totalAmount:N0}đ
            </div>
            
            <div class='reason'>
                <span class='label'>Lý do hủy:</span><br/>
                {cancellationReason}
            </div>
            
            <div class='footer'>
                <p>Email này được gửi tự động từ hệ thống UME Barbershop</p>
            </div>
        </div>
    </div>
</body>
</html>";

                _logger.LogInformation("📧 Sending cancellation email to: {Email}", recipientEmail);
                return await SendEmailAsync(recipientEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in SendAppointmentCancellationEmailSimple");
                return false;
            }
        }

        public async Task<bool> SendAppointmentReminder(Appointment appointment)
        {
            try
            {
                var customerEmail = appointment.GuestEmail ?? appointment.User?.Email;
                if (string.IsNullOrEmpty(customerEmail)) return false;

                var customerName = !string.IsNullOrEmpty(appointment.GuestName)
                    ? appointment.GuestName
                    : appointment.User?.FullName ?? "Quý khách";

                var subject = $"⏰ Nhắc nhở lịch hẹn #{appointment.AppointmentCode} - UME Barbershop";
                var body = GenerateReminderHtml(appointment, customerName);

                return await SendEmailAsync(customerEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reminder for appointment {AppointmentId}", appointment.Id);
                return false;
            }
        }

        public async Task<bool> TestEmailConnection()
        {
            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(SmtpServer, SmtpPort, EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                await client.AuthenticateAsync(Username, Password);
                await client.DisconnectAsync(true);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email connection test failed");
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                _logger.LogInformation("📧 Preparing to send email to {ToEmail}", toEmail);
                _logger.LogInformation("📧 SMTP Config: Server={Server}, Port={Port}, SSL={SSL}", SmtpServer, SmtpPort, EnableSsl);
                _logger.LogInformation("📧 From: {SenderEmail} ({SenderName})", SenderEmail, SenderName);
                
                // Validate email settings
                if (string.IsNullOrEmpty(SenderEmail))
                {
                    _logger.LogError("❌ SenderEmail is not configured");
                    throw new InvalidOperationException("Email sender is not configured");
                }
                if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
                {
                    _logger.LogError("❌ Email credentials are not configured");
                    throw new InvalidOperationException("Email credentials are not configured");
                }
                
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(SenderName, SenderEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                // Set timeout to 30 seconds
                client.Timeout = 30000;
                
                _logger.LogInformation("📧 Connecting to SMTP server...");
                await client.ConnectAsync(SmtpServer, SmtpPort, EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                
                _logger.LogInformation("📧 Authenticating...");
                await client.AuthenticateAsync(Username, Password);
                
                _logger.LogInformation("📧 Sending message...");
                await client.SendAsync(message);
                
                await client.DisconnectAsync(true);

                _logger.LogInformation("✅ Email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (MailKit.Security.AuthenticationException authEx)
            {
                _logger.LogError(authEx, "❌ Email authentication failed. Check username/password. Username: {Username}", Username);
                throw; // Re-throw to let caller handle it
            }
            catch (MailKit.Net.Smtp.SmtpCommandException smtpEx)
            {
                _logger.LogError(smtpEx, "❌ SMTP command error: {StatusCode} - {Message}", smtpEx.StatusCode, smtpEx.Message);
                throw; // Re-throw to let caller handle it
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send email to {Email}. Error: {ErrorType} - {ErrorMessage}", toEmail, ex.GetType().Name, ex.Message);
                throw; // Re-throw to let caller handle it;
            }
        }

        private string GenerateAdminNotificationHtml(Appointment appointment, string customerName, List<string> services)
        {
            var appointmentDateTime = appointment.AppointmentDate.Add(appointment.StartTime);
            var servicesHtml = string.Join("<br/>", services.Select(s => $"• {s}"));

            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .header {{ background-color: #2c3e50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: white; padding: 30px; border-radius: 0 0 5px 5px; }}
        .info-row {{ margin: 15px 0; padding: 10px; background-color: #ecf0f1; border-radius: 3px; }}
        .label {{ font-weight: bold; color: #2c3e50; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #27ae60; color: white; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #7f8c8d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔔 LỊCH HẸN MỚI</h1>
        </div>
        <div class='content'>
            <p>Xin chào Admin,</p>
            <p>Có một lịch hẹn mới từ khách hàng. Vui lòng vào hệ thống để xác nhận.</p>
            
            <div class='info-row'>
                <span class='label'>Mã lịch:</span> #{appointment.AppointmentCode}
            </div>
            <div class='info-row'>
                <span class='label'>Khách hàng:</span> {customerName}
            </div>
            <div class='info-row'>
                <span class='label'>Số điện thoại:</span> {appointment.GuestPhone ?? "Không có"}
            </div>
            <div class='info-row'>
                <span class='label'>Email:</span> {appointment.GuestEmail ?? "Không có"}
            </div>
            <div class='info-row'>
                <span class='label'>Ngày giờ:</span> {appointmentDateTime:dd/MM/yyyy HH:mm}
            </div>
            <div class='info-row'>
                <span class='label'>Dịch vụ:</span><br/>{servicesHtml}
            </div>
            <div class='info-row'>
                <span class='label'>Tổng tiền:</span> {appointment.TotalAmount:N0}đ
            </div>
            {(string.IsNullOrEmpty(appointment.CustomerNotes) ? "" : $@"
            <div class='info-row'>
                <span class='label'>Ghi chú:</span> {appointment.CustomerNotes}
            </div>
            ")}
            
            <center>
                <a href='http://localhost:5000/Admin/Appointments/Details/{appointment.Id}' class='button'>
                    Xem chi tiết & Xác nhận
                </a>
            </center>
        </div>
        <div class='footer'>
            <p>Email tự động từ hệ thống UME Barbershop</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateCustomerConfirmationHtml(Appointment appointment, string customerName, List<string> services)
        {
            var appointmentDateTime = appointment.AppointmentDate.Add(appointment.StartTime);
            var servicesHtml = string.Join("<br/>", services.Select(s => $"• {s}"));

            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .header {{ background-color: #27ae60; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: white; padding: 30px; border-radius: 0 0 5px 5px; }}
        .success-icon {{ font-size: 60px; text-align: center; margin: 20px 0; }}
        .info-row {{ margin: 15px 0; padding: 10px; background-color: #ecf0f1; border-radius: 3px; }}
        .label {{ font-weight: bold; color: #2c3e50; }}
        .highlight {{ background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #7f8c8d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ XÁC NHẬN LỊCH HẸN</h1>
        </div>
        <div class='content'>
            <div class='success-icon'>✨</div>
            <p>Xin chào {customerName},</p>
            <p>Shop UME Barbershop đã xác nhận lịch hẹn của bạn. Cảm ơn bạn đã tin tưởng dịch vụ của chúng tôi!</p>
            
            <div class='info-row'>
                <span class='label'>Mã lịch hẹn:</span> <strong>#{appointment.AppointmentCode}</strong>
            </div>
            <div class='info-row'>
                <span class='label'>Ngày giờ:</span> <strong>{appointmentDateTime:dd/MM/yyyy HH:mm}</strong>
            </div>
            <div class='info-row'>
                <span class='label'>Dịch vụ:</span><br/>{servicesHtml}
            </div>
            <div class='info-row'>
                <span class='label'>Thời gian ước tính:</span> {appointment.TotalDurationMinutes} phút
            </div>
            <div class='info-row'>
                <span class='label'>Tổng tiền:</span> <strong>{appointment.TotalAmount:N0}đ</strong>
            </div>
            
            <div class='highlight'>
                <strong>📍 Địa chỉ:</strong> [Địa chỉ shop của bạn]<br/>
                <strong>📞 Hotline:</strong> [Số điện thoại shop]
            </div>
            
            <p style='color: #e74c3c; font-size: 14px;'>
                <strong>Lưu ý:</strong> Vui lòng đến đúng giờ. Nếu có thay đổi, hãy liên hệ shop trước 24h.
            </p>
        </div>
        <div class='footer'>
            <p>Trân trọng,<br/>UME Barbershop Team</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateCancellationHtml(Appointment appointment, string customerName, bool isCustomer)
        {
            var appointmentDateTime = appointment.AppointmentDate.Add(appointment.StartTime);

            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .header {{ background-color: #e74c3c; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: white; padding: 30px; border-radius: 0 0 5px 5px; }}
        .info-row {{ margin: 15px 0; padding: 10px; background-color: #ecf0f1; border-radius: 3px; }}
        .label {{ font-weight: bold; color: #2c3e50; }}
        .footer {{ text-align: center; margin-top: 20px; color: #7f8c8d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>❌ HỦY LỊCH HẸN</h1>
        </div>
        <div class='content'>
            <p>Xin chào {customerName},</p>
            <p>{(isCustomer ? "Lịch hẹn của bạn đã bị hủy." : $"Lịch hẹn #{appointment.AppointmentCode} đã bị hủy bởi khách hàng.")}</p>
            
            <div class='info-row'>
                <span class='label'>Mã lịch:</span> #{appointment.AppointmentCode}
            </div>
            <div class='info-row'>
                <span class='label'>Ngày giờ:</span> {appointmentDateTime:dd/MM/yyyy HH:mm}
            </div>
            {(string.IsNullOrEmpty(appointment.CancellationReason) ? "" : $@"
            <div class='info-row'>
                <span class='label'>Lý do:</span> {appointment.CancellationReason}
            </div>
            ")}
            
            {(isCustomer ? "<p>Nếu bạn muốn đặt lịch lại, vui lòng liên hệ shop hoặc đặt lịch mới qua app.</p>" : "")}
        </div>
        <div class='footer'>
            <p>UME Barbershop</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateReminderHtml(Appointment appointment, string customerName)
        {
            var appointmentDateTime = appointment.AppointmentDate.Add(appointment.StartTime);

            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .header {{ background-color: #3498db; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: white; padding: 30px; border-radius: 0 0 5px 5px; }}
        .info-row {{ margin: 15px 0; padding: 10px; background-color: #ecf0f1; border-radius: 3px; }}
        .label {{ font-weight: bold; color: #2c3e50; }}
        .footer {{ text-align: center; margin-top: 20px; color: #7f8c8d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>⏰ NHẮC NHỞ LỊCH HẸN</h1>
        </div>
        <div class='content'>
            <p>Xin chào {customerName},</p>
            <p>Đây là email nhắc nhở về lịch hẹn sắp tới của bạn tại UME Barbershop.</p>
            
            <div class='info-row'>
                <span class='label'>Mã lịch:</span> #{appointment.AppointmentCode}
            </div>
            <div class='info-row'>
                <span class='label'>Ngày giờ:</span> <strong>{appointmentDateTime:dd/MM/yyyy HH:mm}</strong>
            </div>
            <div class='info-row'>
                <span class='label'>Tổng tiền:</span> {appointment.TotalAmount:N0}đ
            </div>
            
            <p style='color: #e74c3c;'>
                <strong>Lưu ý:</strong> Vui lòng đến đúng giờ hoặc báo trước nếu có thay đổi.
            </p>
        </div>
        <div class='footer'>
            <p>Hẹn gặp bạn!<br/>UME Barbershop</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
