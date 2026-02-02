/* Admin Notification System với SignalR - Enhanced for Ngrok */
$(document).ready(function () {
    let connection = null;
    let notifications = [];
    let retryCount = 0;
    const maxRetries = 10; // Tăng số lần retry cho ngrok
    let heartbeatInterval = null;
    let lastPingTime = null;
    
    // ============================================
    // DYNAMIC URL CONFIGURATION
    // URL của Backend API - ưu tiên từ window.BACKEND_URL hoặc appsettings
    // ============================================
    function getBackendSignalRUrl() {
        // Ưu tiên 1: Từ window variable (được set từ layout)
        if (window.BACKEND_SIGNALR_URL) {
            return window.BACKEND_SIGNALR_URL;
        }
        
        // Ưu tiên 2: Từ window.BACKEND_URL (base URL)
        if (window.BACKEND_URL) {
            const baseUrl = window.BACKEND_URL.replace(/\/+$/, ''); // Remove trailing slash
            return `${baseUrl}/hubs/notification`;
        }
        
        // Ưu tiên 3: Detect từ current page nếu là ngrok
        if (window.location.hostname.includes('ngrok')) {
            // Nếu admin cũng chạy trên ngrok, có thể cùng backend
            return `${window.location.origin}/hubs/notification`;
        }
        
        // Fallback: localhost
        return "http://localhost:5256/hubs/notification";
    }
    
    const BACKEND_SIGNALR_URL = getBackendSignalRUrl();
    console.log("🔗 SignalR URL:", BACKEND_SIGNALR_URL);

    // ============================================
    // SIGNALR CONNECTION WITH NGROK OPTIMIZATION
    // ============================================
    function initSignalR() {
        try {
            // Build connection với options tối ưu cho ngrok
            connection = new signalR.HubConnectionBuilder()
                .withUrl(BACKEND_SIGNALR_URL, {
                    // Thêm headers cho ngrok
                    headers: {
                        "ngrok-skip-browser-warning": "true"
                    },
                    // Skip negotiation có thể giúp với ngrok
                    skipNegotiation: false,
                    transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
                })
                // Tăng retry delays cho ngrok unstable connection
                .withAutomaticReconnect([0, 3000, 10000, 30000, 60000, 120000])
                .configureLogging(signalR.LogLevel.Information)
                // Tăng timeout cho ngrok
                .withServerTimeout(60000)      // 60s server timeout
                .withKeepAliveInterval(15000)  // 15s keep alive (ngrok timeout là 30s)
                .build();

            // Xử lý khi nhận notification: NewAppointment
            connection.on("NewAppointment", function (notification) {
                console.log("📅 Lịch hẹn mới:", notification);
                
                // Hiển thị toast notification
                toastr.info(notification.message, notification.title || "Lịch hẹn mới", {
                    closeButton: true,
                    progressBar: true,
                    positionClass: "toast-top-right",
                    timeOut: 8000,
                    onclick: function() {
                        if (notification.data && notification.data.Id) {
                            window.location.href = `/Admin/Appointments/Details/${notification.data.Id}`;
                        }
                    }
                });

                // Play notification sound
                playNotificationSound();

                // Load lại danh sách notifications
                loadNotifications();
            });

            // Xử lý khi nhận notification: AppointmentStatusChanged
            connection.on("AppointmentStatusChanged", function (notification) {
                console.log("📅 Trạng thái lịch hẹn thay đổi:", notification);
                
                toastr.success(notification.message, "Cập nhật lịch hẹn", {
                    closeButton: true,
                    progressBar: true,
                    positionClass: "toast-top-right",
                    timeOut: 5000
                });

                // Refresh page nếu đang ở trang Appointments
                if (window.location.pathname.includes('/Admin/Appointments')) {
                    setTimeout(() => location.reload(), 1000);
                }
            });

            // Xử lý khi nhận notification: NewOrder
            connection.on("NewOrder", function (notification) {
                console.log("🛒 Đơn hàng mới:", notification);
                
                toastr.warning(notification.message, notification.title || "Đơn hàng mới", {
                    closeButton: true,
                    progressBar: true,
                    positionClass: "toast-top-right",
                    timeOut: 8000,
                    onclick: function() {
                        if (notification.data && notification.data.Id) {
                            window.location.href = `/Admin/Orders/Details/${notification.data.Id}`;
                        }
                    }
                });

                playNotificationSound();
                loadNotifications();
            });

            // Xử lý khi nhận notification chung
            connection.on("ReceiveNotification", function (title, message) {
                console.log("🔔 Notification:", title, message);
                
                toastr.info(message, title, {
                    closeButton: true,
                    progressBar: true,
                    positionClass: "toast-top-right",
                    timeOut: 5000
                });

                loadNotifications();
            });

            // Xử lý khi reconnecting
            connection.onreconnecting((error) => {
                console.log("🔄 Đang kết nối lại...", error);
                updateConnectionStatus('connecting');
            });

            connection.onreconnected((connectionId) => {
                console.log("✅ Đã kết nối lại. ConnectionId:", connectionId);
                updateConnectionStatus('connected');
                retryCount = 0; // Reset retry count
                // Join lại Admin group sau khi reconnect
                connection.invoke("JoinAdminGroup")
                    .then(() => console.log("✅ Rejoined Admin group"))
                    .catch(err => console.warn("⚠️ Could not rejoin Admin group:", err));
                loadNotifications();
                
                // Restart heartbeat sau khi reconnect
                startHeartbeat();
            });

            connection.onclose((error) => {
                console.log("❌ Mất kết nối SignalR", error);
                updateConnectionStatus('disconnected');
                
                // Dừng heartbeat khi mất kết nối
                stopHeartbeat();
                
                // Manual retry nếu auto reconnect thất bại
                if (retryCount < maxRetries) {
                    retryCount++;
                    // Exponential backoff với jitter cho ngrok
                    const baseDelay = Math.min(retryCount * 5000, 60000);
                    const jitter = Math.random() * 3000; // Random 0-3s
                    const delay = baseDelay + jitter;
                    console.log(`🔄 Thử kết nối lại sau ${Math.round(delay/1000)}s (lần ${retryCount}/${maxRetries})`);
                    setTimeout(initSignalR, delay);
                } else {
                    console.log("⚠️ Đã hết số lần retry. Chuyển sang polling mode.");
                    // Fallback to polling when SignalR fails
                    startPollingFallback();
                }
            });

            // Bắt đầu kết nối
            connection.start()
                .then(() => {
                    console.log("✅ SignalR connected to Backend!");
                    updateConnectionStatus('connected');
                    retryCount = 0;
                    
                    // Join Admin group để nhận notifications dành cho Admin
                    connection.invoke("JoinAdminGroup")
                        .then(() => console.log("✅ Joined Admin group"))
                        .catch(err => console.warn("⚠️ Could not join Admin group:", err));
                    
                    loadNotifications();
                    
                    // Start heartbeat để giữ connection với ngrok
                    startHeartbeat();
                })
                .catch(err => {
                    console.error("❌ SignalR connection error:", err);
                    updateConnectionStatus('disconnected');
                    
                    // Retry với exponential backoff và jitter
                    if (retryCount < maxRetries) {
                        retryCount++;
                        const baseDelay = Math.min(retryCount * 5000, 60000);
                        const jitter = Math.random() * 3000;
                        const delay = baseDelay + jitter;
                        console.log(`🔄 Thử kết nối lại sau ${Math.round(delay/1000)}s (lần ${retryCount}/${maxRetries})`);
                        setTimeout(initSignalR, delay);
                    } else {
                        // Fallback to polling
                        startPollingFallback();
                    }
                });
        } catch (err) {
            console.error("❌ Error initializing SignalR:", err);
            updateConnectionStatus('disconnected');
        }
    }
    
    // ============================================
    // HEARTBEAT - Giữ connection sống với ngrok
    // Ngrok free tier có 30s inactivity timeout
    // ============================================
    function startHeartbeat() {
        stopHeartbeat(); // Clear existing interval
        
        heartbeatInterval = setInterval(() => {
            if (connection && connection.state === signalR.HubConnectionState.Connected) {
                // Ping server để giữ connection
                connection.invoke("Ping")
                    .then(() => {
                        lastPingTime = new Date();
                        console.log("💓 Heartbeat OK");
                    })
                    .catch(err => {
                        console.warn("💔 Heartbeat failed:", err);
                        // Nếu ping fail, connection có thể đã mất
                        if (connection.state !== signalR.HubConnectionState.Connected) {
                            stopHeartbeat();
                        }
                    });
            }
        }, 20000); // Ping mỗi 20s (ngrok timeout là 30s)
        
        console.log("💓 Heartbeat started (20s interval)");
    }
    
    function stopHeartbeat() {
        if (heartbeatInterval) {
            clearInterval(heartbeatInterval);
            heartbeatInterval = null;
            console.log("💔 Heartbeat stopped");
        }
    }
    
    // ============================================
    // POLLING FALLBACK - Khi SignalR fail
    // ============================================
    let pollingInterval = null;
    
    function startPollingFallback() {
        if (pollingInterval) return; // Already polling
        
        console.log("📡 Starting polling fallback mode (every 15s)");
        updateConnectionStatus('polling');
        
        // Poll notifications API trực tiếp
        pollingInterval = setInterval(() => {
            loadNotifications();
        }, 15000); // Poll mỗi 15s
        
        // Load immediately
        loadNotifications();
    }
    
    function stopPollingFallback() {
        if (pollingInterval) {
            clearInterval(pollingInterval);
            pollingInterval = null;
            console.log("📡 Polling fallback stopped");
        }
    }

    // Load danh sách notifications từ API
    function loadNotifications() {
        $.ajax({
            url: '/api/admin/AdminNotification?pageSize=10&unreadOnly=false',
            method: 'GET',
            timeout: 10000, // 10s timeout
            success: function(response) {
                if (response.success && response.data) {
                    notifications = response.data.items || [];
                    updateNotificationBadge(response.data.unreadCount || 0);
                    renderNotificationDropdown(notifications);
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading notifications:', status, error);
                // Không hiển thị toast lỗi liên tục, chỉ log
                if (status === 'timeout') {
                    console.log('API timeout - sẽ thử lại sau');
                }
            }
        });
    }

    // Cập nhật số lượng notification chưa đọc
    function updateNotificationBadge(count) {
        const $badge = $('#notification-badge, .notification-badge');
        if (count > 0) {
            $badge.text(count > 99 ? '99+' : count).show();
        } else {
            $badge.hide();
        }
    }

    // Render dropdown notification list
    function renderNotificationDropdown(items) {
        const $dropdown = $('#notification-dropdown, .notification-dropdown-menu');
        if ($dropdown.length === 0) return;

        let html = '';
        
        if (items.length === 0) {
            html = '<div class="dropdown-item text-center text-muted py-3">Không có thông báo</div>';
        } else {
            items.forEach(item => {
                const icon = getNotificationIcon(item.type);
                const timeAgo = formatTimeAgo(item.createdAt);
                const readClass = item.isRead ? 'read' : 'unread';
                
                html += `
                    <a href="${item.actionUrl || '#'}" class="dropdown-item notification-item ${readClass}" 
                       data-id="${item.id}" onclick="markAsRead(${item.id})">
                        <div class="d-flex align-items-start">
                            <div class="notification-icon me-3">
                                <i class="${icon}"></i>
                            </div>
                            <div class="notification-content flex-grow-1">
                                <div class="notification-title fw-bold">${item.title}</div>
                                <div class="notification-text text-muted small">${item.content || ''}</div>
                                <div class="notification-time text-muted small mt-1">${timeAgo}</div>
                            </div>
                            ${!item.isRead ? '<span class="notification-dot bg-primary"></span>' : ''}
                        </div>
                    </a>
                `;
            });

            // Thêm footer
            html += `
                <div class="dropdown-divider"></div>
                <a href="/Admin/Notifications" class="dropdown-item text-center text-primary">
                    Xem tất cả thông báo
                </a>
            `;
        }

        $dropdown.html(html);
    }

    // Lấy icon theo loại notification
    function getNotificationIcon(type) {
        const icons = {
            'NewAppointment': 'fas fa-calendar-plus text-info',
            'AppointmentStatusChanged': 'fas fa-calendar-check text-success',
            'NewOrder': 'fas fa-shopping-cart text-warning',
            'OrderStatusChanged': 'fas fa-truck text-primary',
            'LowStock': 'fas fa-exclamation-triangle text-danger',
            'NewReview': 'fas fa-star text-warning',
            'System': 'fas fa-cog text-secondary'
        };
        return icons[type] || 'fas fa-bell text-primary';
    }

    // Format thời gian
    function formatTimeAgo(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 1) return 'Vừa xong';
        if (diffMins < 60) return `${diffMins} phút trước`;
        if (diffHours < 24) return `${diffHours} giờ trước`;
        if (diffDays < 7) return `${diffDays} ngày trước`;
        return date.toLocaleDateString('vi-VN');
    }

    // Update connection status indicator
    function updateConnectionStatus(status) {
        const $indicator = $('#connection-status');
        const $statusText = $('#connection-status-text');
        
        switch(status) {
            case 'connected':
                if ($indicator.length) {
                    $indicator.removeClass('bg-warning bg-danger bg-info').addClass('bg-success').attr('title', 'Đã kết nối real-time');
                }
                // Ẩn thông báo lỗi nếu có
                $('.connection-error-alert').fadeOut();
                // Dừng polling nếu đã kết nối SignalR
                stopPollingFallback();
                break;
            case 'connecting':
                if ($indicator.length) {
                    $indicator.removeClass('bg-success bg-danger bg-info').addClass('bg-warning').attr('title', 'Đang kết nối...');
                }
                break;
            case 'disconnected':
                if ($indicator.length) {
                    $indicator.removeClass('bg-success bg-warning bg-info').addClass('bg-danger').attr('title', 'Mất kết nối');
                }
                // Hiển thị thông báo khi mất kết nối lâu (chỉ sau nhiều lần retry)
                if (retryCount >= 3) {
                    showConnectionError();
                }
                break;
            case 'polling':
                if ($indicator.length) {
                    $indicator.removeClass('bg-success bg-warning bg-danger').addClass('bg-info').attr('title', 'Chế độ polling (15s)');
                }
                break;
        }
    }
    
    // Hiển thị thông báo lỗi kết nối (chỉ khi thực sự cần)
    function showConnectionError() {
        // Kiểm tra nếu đã có thông báo rồi thì không hiển thị nữa
        if ($('.connection-error-alert').length === 0) {
            const alertHtml = `
                <div class="connection-error-alert alert alert-warning alert-dismissible fade show position-fixed" 
                     style="top: 70px; right: 20px; z-index: 9999; min-width: 300px;">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    <strong>Lỗi kết nối</strong><br>
                    <small>Không thể kết nối đến Backend API. Thông báo real-time có thể không hoạt động.</small>
                    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                </div>
            `;
            $('body').append(alertHtml);
            
            // Tự động ẩn sau 10s
            setTimeout(() => {
                $('.connection-error-alert').fadeOut(function() {
                    $(this).remove();
                });
            }, 10000);
        }
    }

    // Play notification sound
    function playNotificationSound() {
        try {
            const audio = new Audio('/admin/sounds/notification.mp3');
            audio.volume = 0.5;
            audio.play().catch(e => console.log('Cannot play sound:', e));
        } catch (e) {
            console.log('Audio not supported');
        }
    }

    // Khởi tạo
    initSignalR();

    // Tự động refresh notifications mỗi 30 giây
    setInterval(loadNotifications, 30000);
});

// Đánh dấu notification đã đọc
function markAsRead(id) {
    $.ajax({
        url: `/api/admin/AdminNotification/${id}/read`,
        method: 'PUT',
        success: function(response) {
            console.log('Marked as read:', id);
        },
        error: function(err) {
            console.error('Error marking as read:', err);
        }
    });
}

// Đánh dấu tất cả đã đọc
function markAllAsRead() {
    $.ajax({
        url: '/api/admin/AdminNotification/mark-all-read',
        method: 'PUT',
        success: function(response) {
            console.log('All marked as read');
            location.reload();
        },
        error: function(err) {
            console.error('Error marking all as read:', err);
        }
    });
}

// Hàm hiển thị toast notification (có thể gọi từ nơi khác)
function showNotification(title, message, type = 'info') {
    const toastTypes = {
        'success': toastr.success,
        'info': toastr.info,
        'warning': toastr.warning,
        'error': toastr.error
    };
    
    const toastFn = toastTypes[type] || toastr.info;
    toastFn(message, title, {
        closeButton: true,
        progressBar: true,
        positionClass: "toast-top-right",
        timeOut: 5000
    });
}
