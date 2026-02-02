using Microsoft.AspNetCore.SignalR;

namespace nhom6_backend.Hubs
{
    public class NotificationHub : Hub
    {
        // Được gọi khi client kết nối
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("UserId")?.Value;
            var role = Context.User?.FindFirst("Role")?.Value;
            
            // Thêm user vào group theo role
            if (!string.IsNullOrEmpty(role))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, role);
            }
            
            // Thêm user vào group cá nhân
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            
            await base.OnConnectedAsync();
        }
        
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst("UserId")?.Value;
            var role = Context.User?.FindFirst("Role")?.Value;
            
            if (!string.IsNullOrEmpty(role))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, role);
            }
            
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            
            await base.OnDisconnectedAsync(exception);
        }
        
        // Client đăng ký nhận thông báo cho user cụ thể
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }
        
        // Client đăng ký nhận thông báo admin
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admin");
        }
        
        // Client đăng ký nhận thông báo staff
        public async Task JoinStaffGroup(string staffId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Staff_{staffId}");
        }
        
        // Heartbeat/Ping method để giữ connection sống (cho ngrok)
        public Task Ping()
        {
            // Trả về Task.CompletedTask để giữ connection sống
            // Client sẽ gọi method này định kỳ để tránh timeout
            return Task.CompletedTask;
        }
    }
    
    // Interface để các Controller sử dụng để gửi notification
    public interface INotificationService
    {
        Task NotifyNewOrder(dynamic orderData);
        Task NotifyOrderStatusChanged(string userId, dynamic orderData);
        Task NotifyNewAppointment(dynamic appointmentData);
        Task NotifyAppointmentStatusChanged(string userId, string? staffId, dynamic appointmentData);
        Task NotifyStaffAssigned(string staffId, dynamic appointmentData);
        Task NotifyNewReview(string staffId, dynamic reviewData);
        Task NotifyLowStock(dynamic productData);
    }
    
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;
        
        public NotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }
        
        // Thông báo đơn hàng mới cho Admin
        public async Task NotifyNewOrder(dynamic orderData)
        {
            _logger.LogInformation("📦 Sending NewOrder notification to Admin group");
            await _hubContext.Clients.Group("Admin").SendAsync("NewOrder", new
            {
                type = "NewOrder",
                message = $"Đơn hàng mới #{orderData.Id}",
                data = orderData,
                timestamp = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy")
            });
        }
        
        // Thông báo trạng thái đơn hàng thay đổi cho Customer
        public async Task NotifyOrderStatusChanged(string userId, dynamic orderData)
        {
            await _hubContext.Clients.Group($"User_{userId}").SendAsync("OrderStatusChanged", new
            {
                type = "OrderStatusChanged",
                message = $"Đơn hàng #{orderData.Id} - {orderData.Status}",
                data = orderData,
                timestamp = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy")
            });
        }
        
        // Thông báo lịch hẹn mới cho Admin
        public async Task NotifyNewAppointment(dynamic appointmentData)
        {
            _logger.LogInformation("📅 Sending NewAppointment notification to Admin group");
            _logger.LogInformation($"📅 Appointment Data: Id={appointmentData.Id}, Customer={appointmentData.CustomerName}");
            
            var notification = new
            {
                type = "NewAppointment",
                title = $"Lịch hẹn mới #{appointmentData.AppointmentCode}",
                message = $"{appointmentData.CustomerName} đặt lịch lúc {appointmentData.StartTime} ngày {appointmentData.AppointmentDate}",
                data = appointmentData,
                timestamp = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy")
            };
            
            await _hubContext.Clients.Group("Admin").SendAsync("NewAppointment", notification);
            _logger.LogInformation("✅ NewAppointment notification sent successfully");
        }
        
        // Thông báo trạng thái lịch hẹn thay đổi
        public async Task NotifyAppointmentStatusChanged(string userId, string? staffId, dynamic appointmentData)
        {
            // Gửi cho customer
            await _hubContext.Clients.Group($"User_{userId}").SendAsync("AppointmentStatusChanged", new
            {
                type = "AppointmentStatusChanged",
                message = $"Lịch hẹn #{appointmentData.Id} - {appointmentData.Status}",
                data = appointmentData,
                timestamp = DateTime.Now
            });
            
            // Gửi cho staff nếu có
            if (!string.IsNullOrEmpty(staffId))
            {
                await _hubContext.Clients.Group($"Staff_{staffId}").SendAsync("AppointmentStatusChanged", new
                {
                    type = "AppointmentStatusChanged",
                    message = $"Lịch hẹn #{appointmentData.Id} - {appointmentData.Status}",
                    data = appointmentData,
                    timestamp = DateTime.Now
                });
            }
            
            // Gửi cho Admin
            await _hubContext.Clients.Group("Admin").SendAsync("AppointmentStatusChanged", new
            {
                type = "AppointmentStatusChanged",
                message = $"Lịch hẹn #{appointmentData.Id} - {appointmentData.Status}",
                data = appointmentData,
                timestamp = DateTime.Now
            });
        }
        
        // Thông báo nhân viên được assign lịch hẹn
        public async Task NotifyStaffAssigned(string staffId, dynamic appointmentData)
        {
            await _hubContext.Clients.Group($"Staff_{staffId}").SendAsync("StaffAssigned", new
            {
                type = "StaffAssigned",
                message = $"Bạn được phân công lịch hẹn #{appointmentData.Id}",
                data = appointmentData,
                timestamp = DateTime.Now
            });
        }
        
        // Thông báo có đánh giá mới cho staff
        public async Task NotifyNewReview(string staffId, dynamic reviewData)
        {
            await _hubContext.Clients.Group($"Staff_{staffId}").SendAsync("NewReview", new
            {
                type = "NewReview",
                message = $"Bạn có đánh giá mới: {reviewData.Rating} sao",
                data = reviewData,
                timestamp = DateTime.Now
            });
            
            await _hubContext.Clients.Group("Admin").SendAsync("NewReview", new
            {
                type = "NewReview",
                message = $"Đánh giá mới cho nhân viên",
                data = reviewData,
                timestamp = DateTime.Now
            });
        }
        
        // Thông báo sản phẩm sắp hết hàng cho Admin
        public async Task NotifyLowStock(dynamic productData)
        {
            await _hubContext.Clients.Group("Admin").SendAsync("LowStock", new
            {
                type = "LowStock",
                message = $"Sản phẩm {productData.Name} sắp hết hàng",
                data = productData,
                timestamp = DateTime.Now
            });
        }
    }
}
