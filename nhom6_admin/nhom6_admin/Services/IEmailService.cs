using nhom6_admin.Models.Entities;

namespace nhom6_admin.Services
{
    /// <summary>
    /// Email Service Interface
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Gửi email thông báo lịch hẹn mới đến Admin
        /// </summary>
        Task<bool> SendAppointmentNotificationToAdmin(Appointment appointment);

        /// <summary>
        /// Gửi email xác nhận lịch hẹn đến khách hàng
        /// </summary>
        Task<bool> SendAppointmentConfirmationToCustomer(Appointment appointment);

        /// <summary>
        /// Gửi email hủy lịch hẹn
        /// </summary>
        Task<bool> SendAppointmentCancellationEmail(Appointment appointment, string recipientEmail, bool isCustomer);

        /// <summary>
        /// Gửi email nhắc nhở lịch hẹn
        /// </summary>
        Task<bool> SendAppointmentReminder(Appointment appointment);

        /// <summary>
        /// Test email connection
        /// </summary>
        Task<bool> TestEmailConnection();
    }
}
