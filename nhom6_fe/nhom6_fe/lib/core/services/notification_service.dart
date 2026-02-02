import 'dart:async';
import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import 'package:nhom6_fe/core/config/config_url.dart';

/// Notification types received from SignalR
enum NotificationType {
  newOrder,
  orderStatusChanged,
  newAppointment,
  appointmentStatusChanged,
  staffAssigned,
  newReview,
  lowStock,
}

/// Notification data model
class AppNotification {
  final NotificationType type;
  final String message;
  final dynamic data;
  final DateTime timestamp;

  AppNotification({
    required this.type,
    required this.message,
    this.data,
    DateTime? timestamp,
  }) : timestamp = timestamp ?? DateTime.now();

  factory AppNotification.fromJson(Map<String, dynamic> json) {
    return AppNotification(
      type: _parseType(json['type'] as String?),
      message: json['message'] as String? ?? '',
      data: json['data'],
      timestamp: json['timestamp'] != null
          ? DateTime.tryParse(json['timestamp'].toString()) ?? DateTime.now()
          : DateTime.now(),
    );
  }

  static NotificationType _parseType(String? type) {
    switch (type) {
      case 'NewOrder':
        return NotificationType.newOrder;
      case 'OrderStatusChanged':
        return NotificationType.orderStatusChanged;
      case 'NewAppointment':
        return NotificationType.newAppointment;
      case 'AppointmentStatusChanged':
        return NotificationType.appointmentStatusChanged;
      case 'StaffAssigned':
        return NotificationType.staffAssigned;
      case 'NewReview':
        return NotificationType.newReview;
      case 'LowStock':
        return NotificationType.lowStock;
      default:
        return NotificationType.newOrder;
    }
  }
}

/// SignalR-like notification service using long polling
/// (Flutter web doesn't support WebSocket well on all platforms)
class NotificationService {
  static final NotificationService _instance = NotificationService._internal();
  factory NotificationService() => _instance;
  NotificationService._internal();

  final _notificationController = StreamController<AppNotification>.broadcast();
  Stream<AppNotification> get notifications => _notificationController.stream;

  Timer? _pollingTimer;
  bool _isConnected = false;
  String? _token;
  String? _userId;

  bool get isConnected => _isConnected;
  String? get userId => _userId;

  /// Initialize and connect to notification service
  Future<void> connect() async {
    if (_isConnected) return;

    try {
      final prefs = await SharedPreferences.getInstance();
      _token = prefs.getString('token');
      _userId = prefs.getString('userId');

      if (_token == null) {
        debugPrint('NotificationService: No token found, skipping connection');
        return;
      }

      _isConnected = true;

      // Start polling for notifications (fallback for SignalR)
      // In production, use signalr_netcore package for true WebSocket connection
      _startPolling();

      debugPrint('NotificationService: Connected successfully');
    } catch (e) {
      debugPrint('NotificationService: Connection error - $e');
    }
  }

  /// Disconnect from notification service
  void disconnect() {
    _pollingTimer?.cancel();
    _pollingTimer = null;
    _isConnected = false;
    debugPrint('NotificationService: Disconnected');
  }

  /// Start polling for notifications
  void _startPolling() {
    _pollingTimer?.cancel();

    // Poll every 30 seconds (in production, use WebSocket via signalr_netcore)
    _pollingTimer = Timer.periodic(const Duration(seconds: 30), (_) {
      _checkForNotifications();
    });

    // Initial check
    _checkForNotifications();
  }

  /// Check for new notifications
  Future<void> _checkForNotifications() async {
    if (!_isConnected || _token == null) return;

    try {
      final response = await http.get(
        Uri.parse('${ConfigUrl.baseUrl}notifications/unread'),
        headers: {
          'Authorization': 'Bearer $_token',
          'Content-Type': 'application/json',
        },
      );

      if (response.statusCode == 200) {
        final List<dynamic> data = json.decode(response.body);
        for (var item in data) {
          final notification = AppNotification.fromJson(item);
          _notificationController.add(notification);
        }
      }
    } catch (e) {
      debugPrint('NotificationService: Error checking notifications - $e');
    }
  }

  /// Manually trigger a local notification (for testing)
  void addLocalNotification(AppNotification notification) {
    _notificationController.add(notification);
  }

  /// Subscribe to specific notification type
  Stream<AppNotification> onNotificationType(NotificationType type) {
    return notifications.where((n) => n.type == type);
  }

  /// Dispose the service
  void dispose() {
    disconnect();
    _notificationController.close();
  }
}

/// Mixin to easily add notification listening to StatefulWidgets
mixin NotificationListenerMixin<T extends StatefulWidget> on State<T> {
  StreamSubscription<AppNotification>? _notificationSubscription;

  @override
  void initState() {
    super.initState();
    _notificationSubscription = NotificationService().notifications.listen(
      onNotificationReceived,
    );
  }

  @override
  void dispose() {
    _notificationSubscription?.cancel();
    super.dispose();
  }

  /// Override this to handle notifications
  void onNotificationReceived(AppNotification notification) {
    // Default implementation - override in subclass
    debugPrint('Notification received: ${notification.message}');
  }
}
