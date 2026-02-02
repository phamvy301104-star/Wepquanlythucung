import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../config/config_url.dart';

/// Service for user notifications
class UserNotificationService {
  /// Get all notifications for current user
  Future<Map<String, dynamic>> getMyNotifications({
    bool? isRead,
    int page = 1,
    int pageSize = 20,
  }) async {
    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      if (token == null) {
        return {"success": false, "message": "Vui lòng đăng nhập"};
      }

      // Build URL with query parameters
      var queryParams = <String, String>{
        'page': page.toString(),
        'pageSize': pageSize.toString(),
      };
      if (isRead != null) {
        queryParams['isRead'] = isRead.toString();
      }

      final uri = Uri.parse(
        '${ConfigUrl.baseUrl}NotificationApi/my-notifications',
      ).replace(queryParameters: queryParams);

      final response = await http.get(
        uri,
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json",
          "Authorization": "Bearer $token",
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return {"success": data['success'] ?? true, "data": data['data']};
      } else if (response.statusCode == 401) {
        return {"success": false, "message": "Phiên đăng nhập hết hạn"};
      } else {
        return {"success": false, "message": "Không thể tải thông báo"};
      }
    } catch (e) {
      return {"success": false, "message": "Lỗi kết nối: $e"};
    }
  }

  /// Get unread notification count
  Future<int> getUnreadCount() async {
    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      if (token == null) return 0;

      final response = await http.get(
        Uri.parse('${ConfigUrl.baseUrl}NotificationApi/unread-count'),
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json",
          "Authorization": "Bearer $token",
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return data['count'] ?? 0;
      }
      return 0;
    } catch (e) {
      return 0;
    }
  }

  /// Mark notification as read
  Future<bool> markAsRead(int notificationId) async {
    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      if (token == null) return false;

      final response = await http.patch(
        Uri.parse(
          '${ConfigUrl.baseUrl}NotificationApi/$notificationId/mark-read',
        ),
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json",
          "Authorization": "Bearer $token",
        },
      );

      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  /// Mark all notifications as read
  Future<bool> markAllAsRead() async {
    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      if (token == null) return false;

      final response = await http.patch(
        Uri.parse('${ConfigUrl.baseUrl}NotificationApi/mark-all-read'),
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json",
          "Authorization": "Bearer $token",
        },
      );

      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }
}
