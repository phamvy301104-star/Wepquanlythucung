import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../config/config_url.dart';

/// Service xử lý đặt lịch hẹn
class BookingService {
  /// Lấy danh sách dịch vụ
  Future<Map<String, dynamic>> getServices() async {
    try {
      final response = await http.get(
        Uri.parse('${ConfigUrl.baseUrl}ServiceApi'),
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json",
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return {"success": true, "data": data};
      } else {
        return {"success": false, "message": "Không thể tải danh sách dịch vụ"};
      }
    } catch (e) {
      return {"success": false, "message": "Lỗi kết nối: $e"};
    }
  }

  /// Lấy danh sách nhân viên rảnh theo ngày và giờ
  Future<Map<String, dynamic>> getAvailableStaff({
    required DateTime date,
    required String startTime,
    required int durationMinutes,
  }) async {
    try {
      final dateStr =
          '${date.year}-${date.month.toString().padLeft(2, '0')}-${date.day.toString().padLeft(2, '0')}';
      final uri =
          Uri.parse(
            '${ConfigUrl.baseUrl}admin/appointment/available-staff',
          ).replace(
            queryParameters: {
              'date': dateStr,
              'startTime': startTime,
              'durationMinutes': durationMinutes.toString(),
            },
          );

      final response = await http.get(
        uri,
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json",
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return {"success": true, "data": data};
      } else {
        return {
          "success": false,
          "message": "Không thể tải danh sách nhân viên",
        };
      }
    } catch (e) {
      return {"success": false, "message": "Lỗi kết nối: $e"};
    }
  }

  /// Lấy tất cả nhân viên
  Future<Map<String, dynamic>> getAllStaff() async {
    try {
      final response = await http.get(
        Uri.parse('${ConfigUrl.baseUrl}admin/staff'),
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json",
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return {"success": true, "data": data};
      } else {
        return {
          "success": false,
          "message": "Không thể tải danh sách nhân viên",
        };
      }
    } catch (e) {
      return {"success": false, "message": "Lỗi kết nối: $e"};
    }
  }

  /// Kiểm tra nhân viên có rảnh không
  Future<Map<String, dynamic>> checkStaffAvailability({
    required int staffId,
    required DateTime date,
    required String startTime,
    required int durationMinutes,
  }) async {
    try {
      final dateStr =
          '${date.year}-${date.month.toString().padLeft(2, '0')}-${date.day.toString().padLeft(2, '0')}';
      final uri =
          Uri.parse(
            '${ConfigUrl.baseUrl}admin/appointment/check-staff-availability',
          ).replace(
            queryParameters: {
              'staffId': staffId.toString(),
              'date': dateStr,
              'startTime': startTime,
              'durationMinutes': durationMinutes.toString(),
            },
          );

      final response = await http.get(
        uri,
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json",
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return {
          "success": true,
          "available": data['available'] ?? false,
          "message": data['message'],
        };
      } else {
        final data = jsonDecode(response.body);
        return {
          "success": false,
          "available": false,
          "message": data['message'] ?? "Nhân viên đang bận",
        };
      }
    } catch (e) {
      return {
        "success": false,
        "available": false,
        "message": "Lỗi kết nối: $e",
      };
    }
  }

  /// Tạo lịch hẹn mới
  Future<Map<String, dynamic>> createAppointment({
    required int? staffId,
    required DateTime date,
    required String startTime,
    required List<int> serviceIds,
    String? notes,
    String? guestName,
    String? guestPhone,
  }) async {
    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      // Lấy thông tin user từ SharedPreferences nếu không truyền vào
      String? userJson = prefs.getString('user_info');
      String finalGuestName = guestName ?? 'Khách hàng';
      String finalGuestPhone = guestPhone ?? '';
      String guestEmail = '';

      if (userJson != null) {
        try {
          final userInfo = jsonDecode(userJson);
          // Ưu tiên giá trị truyền vào, nếu không có thì lấy từ user info
          if (guestName == null || guestName.isEmpty) {
            finalGuestName =
                userInfo['fullName'] ?? userInfo['userName'] ?? 'Khách hàng';
          }
          if (guestPhone == null || guestPhone.isEmpty) {
            finalGuestPhone = userInfo['phoneNumber'] ?? '';
          }
          guestEmail = userInfo['email'] ?? '';
        } catch (e) {
          print('[BookingService] Error parsing user info: $e');
        }
      }

      final dateStr =
          '${date.year}-${date.month.toString().padLeft(2, '0')}-${date.day.toString().padLeft(2, '0')}';

      // Format startTime to TimeSpan format "HH:mm:ss" for ASP.NET
      final formattedStartTime =
          startTime.contains(':') && startTime.split(':').length == 2
          ? '$startTime:00' // "09:00" -> "09:00:00"
          : startTime;

      print(
        '[BookingService] Creating appointment: date=$dateStr, startTime=$formattedStartTime, services=$serviceIds',
      );

      // Sử dụng endpoint AppointmentApi thay vì admin (không cần role)
      final body = {
        "guestName": finalGuestName,
        "guestPhone": finalGuestPhone,
        "guestEmail": guestEmail,
        "appointmentDate": dateStr,
        "startTime": formattedStartTime,
        "serviceIds": serviceIds,
        "customerNotes": notes ?? "",
      };

      print('[BookingService] Request body: ${jsonEncode(body)}');

      // Headers - có token thì gửi để link với user account
      final headers = {
        "Content-Type": "application/json",
        "Accept": "application/json",
      };
      if (token != null) {
        headers["Authorization"] = "Bearer $token";
      }

      final response = await http.post(
        Uri.parse('${ConfigUrl.baseUrl}AppointmentApi/create'),
        headers: headers,
        body: jsonEncode(body),
      );

      // Debug log
      print(
        '[BookingService] createAppointment response: ${response.statusCode}',
      );
      print('[BookingService] response body: ${response.body}');

      if (response.statusCode == 200 || response.statusCode == 201) {
        try {
          final data = response.body.isNotEmpty
              ? jsonDecode(response.body)
              : {};
          return {
            "success": true,
            "data": data,
            "message": data['message'] ?? "Đặt lịch thành công",
          };
        } catch (e) {
          // Response không phải JSON nhưng vẫn thành công
          return {
            "success": true,
            "data": {},
            "message": "Đặt lịch thành công",
          };
        }
      } else if (response.statusCode == 401) {
        return {
          "success": false,
          "message": "Phiên đăng nhập hết hạn, vui lòng đăng nhập lại",
        };
      } else if (response.statusCode == 409) {
        // Conflict - staff busy
        try {
          final data = response.body.isNotEmpty
              ? jsonDecode(response.body)
              : {};
          return {
            "success": false,
            "message":
                data['message'] ??
                "Nhân viên đang bận, vui lòng chọn nhân viên khác",
          };
        } catch (e) {
          return {
            "success": false,
            "message": "Nhân viên đang bận, vui lòng chọn nhân viên khác",
          };
        }
      } else {
        try {
          final data = response.body.isNotEmpty
              ? jsonDecode(response.body)
              : {};
          return {
            "success": false,
            "message":
                data['message'] ?? "Đặt lịch thất bại (${response.statusCode})",
          };
        } catch (e) {
          return {
            "success": false,
            "message": "Đặt lịch thất bại (${response.statusCode})",
          };
        }
      }
    } catch (e) {
      print('[BookingService] createAppointment error: $e');
      return {"success": false, "message": "Lỗi: $e"};
    }
  }

  /// Lấy danh sách lịch hẹn của user
  Future<Map<String, dynamic>> getMyAppointments({String? status}) async {
    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      if (token == null) {
        return {"success": false, "message": "Vui lòng đăng nhập"};
      }

      // Build URL with query parameters
      var uri = Uri.parse('${ConfigUrl.baseUrl}AppointmentApi/my-appointments');
      if (status != null && status.isNotEmpty && status != 'All') {
        uri = uri.replace(queryParameters: {'status': status});
      }

      print('[BookingService] Getting appointments from: $uri');

      final response = await http.get(
        uri,
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json",
          "Authorization": "Bearer $token",
        },
      );

      print('[BookingService] Response status: ${response.statusCode}');

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return {"success": data['success'] ?? true, "data": data['data']};
      } else if (response.statusCode == 401) {
        return {"success": false, "message": "Phiên đăng nhập hết hạn"};
      } else {
        return {"success": false, "message": "Không thể tải lịch hẹn"};
      }
    } catch (e) {
      print('[BookingService] getMyAppointments error: $e');
      return {"success": false, "message": "Lỗi kết nối: $e"};
    }
  }

  /// Hủy lịch hẹn
  Future<Map<String, dynamic>> cancelAppointment(int appointmentId) async {
    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      if (token == null) {
        return {"success": false, "message": "Vui lòng đăng nhập"};
      }

      final response = await http.patch(
        Uri.parse('${ConfigUrl.baseUrl}AppointmentApi/$appointmentId/cancel'),
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json",
          "Authorization": "Bearer $token",
        },
        body: jsonEncode({"reason": "Khách hàng hủy qua ứng dụng"}),
      );

      print('[BookingService] Cancel response: ${response.statusCode}');

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return {
          "success": data['success'] ?? true,
          "message": data['message'] ?? "Đã hủy lịch hẹn",
        };
      } else if (response.statusCode == 401) {
        return {"success": false, "message": "Phiên đăng nhập hết hạn"};
      } else {
        try {
          final data = jsonDecode(response.body);
          return {
            "success": false,
            "message": data['message'] ?? "Hủy lịch thất bại",
          };
        } catch (e) {
          return {"success": false, "message": "Hủy lịch thất bại"};
        }
      }
    } catch (e) {
      print('[BookingService] cancelAppointment error: $e');
      return {"success": false, "message": "Lỗi kết nối: $e"};
    }
  }
}
