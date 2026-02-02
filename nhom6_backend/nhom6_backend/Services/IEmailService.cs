using nhom6_backend.Models.Entities;

namespace nhom6_backend.Services
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
        /// Gửi email thông báo lịch hẹn mới đến Admin (không cần DbContext)
        /// Use this for background tasks
        /// </summary>
        Task<bool> SendAppointmentNotificationToAdminSimple(
            int appointmentId,
            string appointmentCode, 
            string customerName,
            string customerPhone,
            string customerEmail,
            DateTime appointmentDate,
            TimeSpan startTime,
            decimal totalAmount,
            List<string> services);

        /// <summary>
        /// Gửi email xác nhận lịch hẹn đến khách hàng
        /// </summary>
        Task<bool> SendAppointmentConfirmationToCustomer(Appointment appointment);

        /// <summary>
        /// Gửi email hủy lịch hẹn
        /// </summary>
        Task<bool> SendAppointmentCancellationEmail(Appointment appointment, string recipientEmail, bool isCustomer);

        /// <summary>
        /// Gửi email hủy lịch hẹn (không cần DbContext - dùng cho background tasks)
        /// </summary>
        Task<bool> SendAppointmentCancellationEmailSimple(
            string appointmentCode,
            string customerName,
            string customerPhone,
            DateTime appointmentDate,
            TimeSpan startTime,
            decimal totalAmount,
            string cancellationReason,
            string recipientEmail,
            bool isCustomer);

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
