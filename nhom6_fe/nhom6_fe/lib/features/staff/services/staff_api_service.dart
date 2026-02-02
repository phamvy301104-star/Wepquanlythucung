// Staff API Service
// Service xử lý tất cả API calls cho Staff Module

import 'dart:convert';
import 'dart:developer' as developer;
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:http/io_client.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../../core/config/config_url.dart';
import '../models/staff_models.dart';

/// Custom exception for Staff API errors
class StaffApiException implements Exception {
  final String message;
  final int statusCode;
  final bool isNotFound;

  StaffApiException({
    required this.message,
    required this.statusCode,
    this.isNotFound = false,
  });

  @override
  String toString() => 'StaffApiException: $message (status: $statusCode)';
}

class StaffApiService {
  final String baseUrl;
  late final http.Client _httpClient;

  // Error tracking để ngăn API spam
  DateTime? _lastProfileErrorTime;
  int _profileErrorCount = 0;
  static const _errorCooldownSeconds = 30; // Chờ 30s sau mỗi lỗi
  static const _maxErrorRetries = 3;

  StaffApiService({String? baseUrl}) : baseUrl = baseUrl ?? ConfigUrl.baseUrl {
    // Create custom HttpClient that bypasses SSL certificate verification (Dev only)
    final httpClient = HttpClient()
      ..badCertificateCallback =
          (X509Certificate cert, String host, int port) => true;
    _httpClient = IOClient(httpClient);
  }

  // ============================================
  // HELPER METHODS
  // ============================================
  Future<String?> _getToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('jwt_token');
  }

  Map<String, String> _headers(String? token) {
    return {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'ngrok-skip-browser-warning': 'true', // Required for ngrok free tier
      if (token != null) 'Authorization': 'Bearer $token',
    };
  }

  void _log(String message) {
    developer.log(message, name: 'StaffApiService');
  }

  // Log full URL for debugging
  void _logUrl(String method, String url) {
    developer.log('$method: $url', name: 'StaffApiService');
  }

  /// Kiểm tra xem có nên retry API call không (tránh spam)
  bool _shouldRetryProfileApi() {
    if (_lastProfileErrorTime == null) return true;

    final secondsSinceError = DateTime.now()
        .difference(_lastProfileErrorTime!)
        .inSeconds;

    // Nếu đã quá cooldown period, reset error count
    if (secondsSinceError > _errorCooldownSeconds * 2) {
      _profileErrorCount = 0;
      return true;
    }

    // Nếu quá nhiều lỗi, chờ cooldown
    if (_profileErrorCount >= _maxErrorRetries) {
      if (secondsSinceError < _errorCooldownSeconds) {
        _log(
          '⏳ API rate limited. Waiting ${_errorCooldownSeconds - secondsSinceError}s before retry',
        );
        return false;
      }
      // Reset sau cooldown
      _profileErrorCount = 0;
    }

    return true;
  }

  /// Ghi nhận lỗi API
  void _recordApiError() {
    _lastProfileErrorTime = DateTime.now();
    _profileErrorCount++;
  }

  /// Reset error tracking (gọi sau khi success)
  void _resetApiErrors() {
    _lastProfileErrorTime = null;
    _profileErrorCount = 0;
  }

  // ============================================
  // PROFILE APIs
  // ============================================
  Future<StaffProfile?> getProfile() async {
    // Kiểm tra rate limiting
    if (!_shouldRetryProfileApi()) {
      _log('⏳ Skipping profile API call (rate limited)');
      return null;
    }

    try {
      final token = await _getToken();
      final url = '${baseUrl}staff/profile';
      _logUrl('GET', url);
      _log(
        'Token exists: ${token != null}, Token length: ${token?.length ?? 0}',
      );

      final response = await _httpClient
          .get(Uri.parse(url), headers: _headers(token))
          .timeout(const Duration(seconds: 15));

      _log('GET Profile: ${response.statusCode}');

      // Log response body for debugging errors
      if (response.statusCode != 200) {
        _log('Error Response [${response.statusCode}]: ${response.body}');
        _recordApiError();

        // Nếu 404, không throw - chỉ return null để UI hiển thị thông báo phù hợp
        if (response.statusCode == 404) {
          return null;
        }
      }

      if (response.statusCode == 200) {
        _resetApiErrors(); // Reset error tracking on success
        final data = jsonDecode(response.body);
        if (data['success'] == true && data['data'] != null) {
          return StaffProfile.fromJson(data['data']);
        }
        return StaffProfile.fromJson(data);
      }
      return null;
    } catch (e) {
      _log('Error getProfile: $e');
      _recordApiError();
      return null;
    }
  }

  Future<StaffStats?> getStats() async {
    try {
      final token = await _getToken();
      final url = '${baseUrl}staff/stats';
      _logUrl('GET', url);

      final response = await _httpClient.get(
        Uri.parse(url),
        headers: _headers(token),
      );

      _log('GET Stats: ${response.statusCode}');

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        if (data['success'] == true && data['data'] != null) {
          return StaffStats.fromJson(data['data']);
        }
        return StaffStats.fromJson(data);
      }
      return null;
    } catch (e) {
      _log('Error getStats: $e');
      return null;
    }
  }

  Future<List<StaffSchedule>> getSchedule() async {
    try {
      final token = await _getToken();
      final response = await _httpClient.get(
        Uri.parse('${baseUrl}staff/schedule'),
        headers: _headers(token),
      );

      _log('GET Schedule: ${response.statusCode}');

      if (response.statusCode == 200) {
        final body = jsonDecode(response.body);
        // Handle wrapped response { success: true, data: [...] }
        final List<dynamic> data = body is List ? body : (body['data'] ?? []);
        return data.map((e) => StaffSchedule.fromJson(e)).toList();
      }
      return [];
    } catch (e) {
      _log('Error getSchedule: $e');
      return [];
    }
  }

  Future<List<StaffAppointment>> getAppointments({DateTime? date}) async {
    try {
      final token = await _getToken();
      String url = '${baseUrl}staff/appointments';
      if (date != null) {
        url += '?date=${date.toIso8601String().split('T')[0]}';
      }

      final response = await _httpClient.get(
        Uri.parse(url),
        headers: _headers(token),
      );

      _log('GET Appointments: ${response.statusCode}');

      if (response.statusCode == 200) {
        final body = jsonDecode(response.body);
        // Handle wrapped response { success: true, data: [...] }
        final List<dynamic> data = body is List ? body : (body['data'] ?? []);
        return data.map((e) => StaffAppointment.fromJson(e)).toList();
      }
      return [];
    } catch (e) {
      _log('Error getAppointments: $e');
      return [];
    }
  }

  Future<Map<String, List<StaffAppointment>>> getAppointmentsCalendar(
    int month,
    int year,
  ) async {
    try {
      final token = await _getToken();
      final response = await _httpClient.get(
        Uri.parse(
          '${baseUrl}staff/appointments/calendar?month=$month&year=$year',
        ),
        headers: _headers(token),
      );

      _log('GET Calendar: ${response.statusCode}');

      if (response.statusCode == 200) {
        final Map<String, dynamic> data = jsonDecode(response.body);
        final Map<String, List<StaffAppointment>> result = {};

        data.forEach((key, value) {
          result[key] = (value as List)
              .map((e) => StaffAppointment.fromJson(e))
              .toList();
        });

        return result;
      }
      return {};
    } catch (e) {
      _log('Error getAppointmentsCalendar: $e');
      return {};
    }
  }

  // ============================================
  // ATTENDANCE APIs
  // ============================================
  Future<AttendanceToday?> getTodayAttendance() async {
    try {
      final token = await _getToken();
      final url = '${baseUrl}attendance/today';
      _logUrl('GET', url);

      final response = await _httpClient.get(
        Uri.parse(url),
        headers: _headers(token),
      );

      _log('GET Today Attendance: ${response.statusCode}');

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        // Backend returns {success: true, data: {...}}
        if (data['success'] == true && data['data'] != null) {
          return AttendanceToday.fromJson(data['data']);
        }
        return AttendanceToday.fromJson(data);
      }
      return null;
    } catch (e) {
      _log('Error getTodayAttendance: $e');
      return null;
    }
  }

  Future<Map<String, dynamic>> checkIn({
    required int checkType,
    required String photoBase64,
    String? notes,
  }) async {
    try {
      final token = await _getToken();
      final response = await _httpClient.post(
        Uri.parse('${baseUrl}attendance/check'),
        headers: _headers(token),
        body: jsonEncode({
          'checkType': checkType,
          'photoBase64': photoBase64,
          'checkTime': DateTime.now().toIso8601String(),
          'notes': notes,
        }),
      );

      _log('POST Check-in: ${response.statusCode}');

      final data = jsonDecode(response.body);

      if (response.statusCode == 200) {
        return {
          'success': true,
          'message': data['message'] ?? 'Chấm công thành công',
          'attendance': data['attendance'] != null
              ? Attendance.fromJson(data['attendance'])
              : null,
        };
      }

      return {
        'success': false,
        'message': data['message'] ?? 'Chấm công thất bại',
      };
    } catch (e) {
      _log('Error checkIn: $e');
      return {'success': false, 'message': 'Lỗi kết nối: $e'};
    }
  }

  Future<List<Attendance>> getAttendanceHistory({
    DateTime? startDate,
    DateTime? endDate,
    int page = 1,
    int pageSize = 20,
  }) async {
    try {
      final token = await _getToken();
      String url = '${baseUrl}attendance/history?page=$page&pageSize=$pageSize';

      if (startDate != null) {
        url += '&startDate=${startDate.toIso8601String().split('T')[0]}';
      }
      if (endDate != null) {
        url += '&endDate=${endDate.toIso8601String().split('T')[0]}';
      }

      final response = await _httpClient.get(
        Uri.parse(url),
        headers: _headers(token),
      );

      _log('GET Attendance History: ${response.statusCode}');

      if (response.statusCode == 200) {
        final body = jsonDecode(response.body);
        // Handle wrapped response { success: true, data: [...] }
        final List<dynamic> data = body is List ? body : (body['data'] ?? []);
        return data.map((e) => Attendance.fromJson(e)).toList();
      }
      return [];
    } catch (e) {
      _log('Error getAttendanceHistory: $e');
      return [];
    }
  }

  Future<Map<String, dynamic>> getAttendanceStats(int month, int year) async {
    try {
      final token = await _getToken();
      final response = await _httpClient.get(
        Uri.parse('${baseUrl}attendance/stats?month=$month&year=$year'),
        headers: _headers(token),
      );

      _log('GET Attendance Stats: ${response.statusCode}');

      if (response.statusCode == 200) {
        return jsonDecode(response.body);
      }
      return {};
    } catch (e) {
      _log('Error getAttendanceStats: $e');
      return {};
    }
  }

  // ============================================
  // SALARY APIs
  // ============================================
  Future<SalarySlip?> getCurrentSalary() async {
    try {
      final token = await _getToken();
      final response = await _httpClient.get(
        Uri.parse('${baseUrl}salary/current'),
        headers: _headers(token),
      );

      _log('GET Current Salary: ${response.statusCode}');

      if (response.statusCode == 200) {
        final body = jsonDecode(response.body);
        // Handle wrapped response { success: true, data: {...} }
        final data = body is Map && body['data'] != null ? body['data'] : body;
        return SalarySlip.fromJson(data);
      }
      return null;
    } catch (e) {
      _log('Error getCurrentSalary: $e');
      return null;
    }
  }

  Future<SalarySlip?> getSalaryByMonth(int month, int year) async {
    try {
      final token = await _getToken();
      final response = await _httpClient.get(
        Uri.parse('${baseUrl}salary/$month/$year'),
        headers: _headers(token),
      );

      _log('GET Salary $month/$year: ${response.statusCode}');

      if (response.statusCode == 200) {
        final body = jsonDecode(response.body);
        // Handle wrapped response { success: true, data: {...} }
        final data = body is Map && body['data'] != null ? body['data'] : body;
        return SalarySlip.fromJson(data);
      }
      return null;
    } catch (e) {
      _log('Error getSalaryByMonth: $e');
      return null;
    }
  }

  Future<List<SalarySlip>> getSalaryHistory({
    int page = 1,
    int pageSize = 12,
  }) async {
    try {
      final token = await _getToken();
      final response = await _httpClient.get(
        Uri.parse('${baseUrl}salary/history?page=$page&pageSize=$pageSize'),
        headers: _headers(token),
      );

      _log('GET Salary History: ${response.statusCode}');

      if (response.statusCode == 200) {
        final body = jsonDecode(response.body);
        // Handle wrapped response { success: true, data: [...] }
        final List<dynamic> data = body is List ? body : (body['data'] ?? []);
        return data.map((e) => SalarySlip.fromJson(e)).toList();
      }
      return [];
    } catch (e) {
      _log('Error getSalaryHistory: $e');
      return [];
    }
  }

  // ============================================
  // CHAT APIs
  // ============================================
  Future<List<StaffColleague>> getColleagues() async {
    try {
      final token = await _getToken();
      final response = await _httpClient.get(
        Uri.parse('${baseUrl}staff-chat/colleagues'),
        headers: _headers(token),
      );

      _log('GET Colleagues: ${response.statusCode}');

      if (response.statusCode == 200) {
        final body = jsonDecode(response.body);
        // Handle wrapped response { success: true, data: [...] }
        final List<dynamic> data = body is List ? body : (body['data'] ?? []);
        return data.map((e) => StaffColleague.fromJson(e)).toList();
      }
      return [];
    } catch (e) {
      _log('Error getColleagues: $e');
      return [];
    }
  }

  Future<List<StaffChatRoom>> getChatRooms() async {
    try {
      final token = await _getToken();
      final response = await _httpClient.get(
        Uri.parse('${baseUrl}staff-chat/rooms'),
        headers: _headers(token),
      );

      _log('GET Chat Rooms: ${response.statusCode}');

      if (response.statusCode == 200) {
        final body = jsonDecode(response.body);
        // Handle wrapped response { success: true, data: [...] }
        final List<dynamic> data = body is List ? body : (body['data'] ?? []);
        return data.map((e) => StaffChatRoom.fromJson(e)).toList();
      }
      return [];
    } catch (e) {
      _log('Error getChatRooms: $e');
      return [];
    }
  }

  Future<StaffChatRoom?> getOrCreateRoom(int otherStaffId) async {
    try {
      final token = await _getToken();
      final response = await _httpClient.get(
        Uri.parse('${baseUrl}staff-chat/rooms/$otherStaffId'),
        headers: _headers(token),
      );

      _log('GET/Create Room: ${response.statusCode}');

      if (response.statusCode == 200) {
        final body = jsonDecode(response.body);
        // Handle wrapped response { success: true, data: {...} }
        final data = body is Map && body['data'] != null ? body['data'] : body;
        return StaffChatRoom.fromJson(data);
      }
      return null;
    } catch (e) {
      _log('Error getOrCreateRoom: $e');
      return null;
    }
  }

  Future<List<StaffChatMessage>> getMessages(
    int roomId, {
    int page = 1,
    int pageSize = 50,
  }) async {
    try {
      final token = await _getToken();
      final response = await _httpClient.get(
        Uri.parse(
          '${baseUrl}staff-chat/rooms/$roomId/messages?page=$page&pageSize=$pageSize',
        ),
        headers: _headers(token),
      );

      _log('GET Messages: ${response.statusCode}');

      if (response.statusCode == 200) {
        final body = jsonDecode(response.body);
        // Handle wrapped response { success: true, data: [...] }
        final List<dynamic> data = body is List ? body : (body['data'] ?? []);
        return data.map((e) => StaffChatMessage.fromJson(e)).toList();
      }
      return [];
    } catch (e) {
      _log('Error getMessages: $e');
      return [];
    }
  }

  Future<StaffChatMessage?> sendMessage({
    required int roomId,
    required String content,
    String messageType = 'Text',
  }) async {
    try {
      final token = await _getToken();
      final response = await _httpClient.post(
        Uri.parse('${baseUrl}staff-chat/rooms/$roomId/messages'),
        headers: _headers(token),
        body: jsonEncode({'content': content, 'messageType': messageType}),
      );

      _log('POST Send Message: ${response.statusCode}');

      if (response.statusCode == 200) {
        final body = jsonDecode(response.body);
        // Handle wrapped response { success: true, data: {...} }
        final data = body is Map && body['data'] != null ? body['data'] : body;
        return StaffChatMessage.fromJson(data);
      }
      return null;
    } catch (e) {
      _log('Error sendMessage: $e');
      return null;
    }
  }

  Future<bool> markMessagesAsRead(int roomId) async {
    try {
      final token = await _getToken();
      final response = await _httpClient.post(
        Uri.parse('${baseUrl}staff-chat/rooms/$roomId/read'),
        headers: _headers(token),
      );

      _log('POST Mark Read: ${response.statusCode}');

      return response.statusCode == 200;
    } catch (e) {
      _log('Error markMessagesAsRead: $e');
      return false;
    }
  }
}
