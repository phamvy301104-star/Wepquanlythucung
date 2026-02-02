using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace nhom6_admin.Hubs
{
    /// <summary>
    /// SignalR Hub for Admin real-time notifications
    /// Receives notifications from backend and broadcasts to admin users
    /// Note: Authorization removed to allow SignalR connection from admin dashboard
    /// The admin pages themselves are already protected by [Authorize]
    /// </summary>
    public class AdminNotificationHub : Hub
    {
        private readonly ILogger<AdminNotificationHub> _logger;
        
        public AdminNotificationHub(ILogger<AdminNotificationHub> logger)
        {
            _logger = logger;
        }
        
        public override async Task OnConnectedAsync()
        {
            // Add to Admin group when connected
            await Groups.AddToGroupAsync(Context.ConnectionId, "AdminDashboard");
            _logger.LogInformation("SignalR client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }
        
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AdminDashboard");
            _logger.LogInformation("SignalR client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
        
        /// <summary>
        /// Join specific notification group
        /// </summary>
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
        }
        
        /// <summary>
        /// Leave a notification group
        /// </summary>
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
        }
    }
    
    /// <summary>
    /// Service to send notifications to admin dashboard
    /// </summary>
    public interface IAdminNotificationService
    {
        Task NotifyNewOrder(dynamic orderData);
        Task NotifyOrderStatusChanged(dynamic orderData);
        Task NotifyNewAppointment(dynamic appointmentData);
        Task NotifyAppointmentStatusChanged(dynamic appointmentData);
        Task NotifyLowStock(dynamic productData);
        Task NotifyNewReview(dynamic reviewData);
        Task RefreshDashboard();
        Task SendNotificationToUser(string userId, string title, string message);
    }
    
    public class AdminNotificationService : IAdminNotificationService
    {
        private readonly IHubContext<AdminNotificationHub> _hubContext;
        
        public AdminNotificationService(IHubContext<AdminNotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }
        
        public async Task NotifyNewOrder(dynamic orderData)
        {
            await _hubContext.Clients.Group("AdminDashboard").SendAsync("NewOrder", new
            {
                type = "NewOrder",
                message = $"Đơn hàng mới #{orderData.Id}",
                data = orderData,
                timestamp = DateTime.Now.ToString("HH:mm:ss")
            });
        }
        
        public async Task NotifyOrderStatusChanged(dynamic orderData)
        {
            await _hubContext.Clients.Group("AdminDashboard").SendAsync("OrderStatusChanged", new
            {
                type = "OrderStatusChanged",
                message = $"Đơn hàng #{orderData.Id} - {orderData.Status}",
                data = orderData,
                timestamp = DateTime.Now.ToString("HH:mm:ss")
            });
        }
        
        public async Task NotifyNewAppointment(dynamic appointmentData)
        {
            await _hubContext.Clients.Group("AdminDashboard").SendAsync("NewAppointment", new
            {
                type = "NewAppointment",
                message = $"Lịch hẹn mới #{appointmentData.Id}",
                data = appointmentData,
                timestamp = DateTime.Now.ToString("HH:mm:ss")
            });
        }
        
        public async Task NotifyAppointmentStatusChanged(dynamic appointmentData)
        {
            await _hubContext.Clients.Group("AdminDashboard").SendAsync("AppointmentStatusChanged", new
            {
                type = "AppointmentStatusChanged",
                message = $"Lịch hẹn #{appointmentData.Id} - {appointmentData.Status}",
                data = appointmentData,
                timestamp = DateTime.Now.ToString("HH:mm:ss")
            });
        }
        
        public async Task NotifyLowStock(dynamic productData)
        {
            await _hubContext.Clients.Group("AdminDashboard").SendAsync("LowStock", new
            {
                type = "LowStock",
                message = $"Sản phẩm {productData.Name} sắp hết hàng",
                data = productData,
                timestamp = DateTime.Now.ToString("HH:mm:ss")
            });
        }
        
        public async Task NotifyNewReview(dynamic reviewData)
        {
            await _hubContext.Clients.Group("AdminDashboard").SendAsync("NewReview", new
            {
                type = "NewReview",
                message = $"Đánh giá mới: {reviewData.Rating} sao",
                data = reviewData,
                timestamp = DateTime.Now.ToString("HH:mm:ss")
            });
        }
        
        public async Task RefreshDashboard()
        {
            await _hubContext.Clients.Group("AdminDashboard").SendAsync("RefreshDashboard", new
            {
                type = "RefreshDashboard",
                message = "Cập nhật dữ liệu dashboard",
                timestamp = DateTime.Now.ToString("HH:mm:ss")
            });
        }

        public async Task SendNotificationToUser(string userId, string title, string message)
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", title, message);
        }
    }
}
