using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace nhom6_backend.Hubs
{
    /// <summary>
    /// SignalR Hub cho Chat nội bộ nhân viên
    /// </summary>
    [Authorize]
    public class StaffChatHub : Hub
    {
        private readonly ILogger<StaffChatHub> _logger;
        
        // Track online staff
        private static readonly ConcurrentDictionary<int, HashSet<string>> _staffConnections = new();
        private static readonly ConcurrentDictionary<string, int> _connectionStaffMap = new();

        public StaffChatHub(ILogger<StaffChatHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var staffIdClaim = Context.User?.FindFirst("StaffId")?.Value;
            
            if (int.TryParse(staffIdClaim, out int staffId))
            {
                // Track connection
                _connectionStaffMap[Context.ConnectionId] = staffId;
                
                if (!_staffConnections.ContainsKey(staffId))
                {
                    _staffConnections[staffId] = new HashSet<string>();
                }
                _staffConnections[staffId].Add(Context.ConnectionId);
                
                // Add to personal group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Staff_{staffId}");
                
                // Notify others that this staff is online
                await Clients.Others.SendAsync("StaffOnline", staffId);
                
                _logger.LogInformation("Staff {StaffId} connected. ConnectionId: {ConnectionId}", 
                    staffId, Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connectionStaffMap.TryRemove(Context.ConnectionId, out int staffId))
            {
                if (_staffConnections.TryGetValue(staffId, out var connections))
                {
                    connections.Remove(Context.ConnectionId);
                    
                    // If no more connections, staff is offline
                    if (connections.Count == 0)
                    {
                        _staffConnections.TryRemove(staffId, out _);
                        await Clients.Others.SendAsync("StaffOffline", staffId);
                    }
                }
                
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Staff_{staffId}");
                
                _logger.LogInformation("Staff {StaffId} disconnected. ConnectionId: {ConnectionId}", 
                    staffId, Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join a private chat room
        /// </summary>
        public async Task JoinChatRoom(int chatRoomId)
        {
            var groupName = $"ChatRoom_{chatRoomId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            var staffId = _connectionStaffMap.GetValueOrDefault(Context.ConnectionId, 0);
            _logger.LogInformation("Staff {StaffId} joined chat room {ChatRoomId}", staffId, chatRoomId);
            
            // Notify others in room
            await Clients.OthersInGroup(groupName).SendAsync("UserJoinedChat", staffId);
        }

        /// <summary>
        /// Leave a private chat room
        /// </summary>
        public async Task LeaveChatRoom(int chatRoomId)
        {
            var groupName = $"ChatRoom_{chatRoomId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            
            var staffId = _connectionStaffMap.GetValueOrDefault(Context.ConnectionId, 0);
            _logger.LogInformation("Staff {StaffId} left chat room {ChatRoomId}", staffId, chatRoomId);
            
            // Notify others in room
            await Clients.OthersInGroup(groupName).SendAsync("UserLeftChat", staffId);
        }

        /// <summary>
        /// Notify typing status
        /// </summary>
        public async Task UserTyping(int chatRoomId)
        {
            var staffId = _connectionStaffMap.GetValueOrDefault(Context.ConnectionId, 0);
            await Clients.OthersInGroup($"ChatRoom_{chatRoomId}")
                .SendAsync("UserTyping", staffId);
        }

        /// <summary>
        /// Notify stopped typing
        /// </summary>
        public async Task UserStoppedTyping(int chatRoomId)
        {
            var staffId = _connectionStaffMap.GetValueOrDefault(Context.ConnectionId, 0);
            await Clients.OthersInGroup($"ChatRoom_{chatRoomId}")
                .SendAsync("UserStoppedTyping", staffId);
        }

        /// <summary>
        /// Get list of online staff IDs
        /// </summary>
        public Task<List<int>> GetOnlineStaff()
        {
            return Task.FromResult(_staffConnections.Keys.ToList());
        }

        /// <summary>
        /// Check if a specific staff is online
        /// </summary>
        public Task<bool> IsStaffOnline(int staffId)
        {
            return Task.FromResult(_staffConnections.ContainsKey(staffId));
        }
    }
}
