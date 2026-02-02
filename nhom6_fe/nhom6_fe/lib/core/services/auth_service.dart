import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:jwt_decoder/jwt_decoder.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../config/config_url.dart';

class AuthService {
  /// Đăng nhập
  Future<Map<String, dynamic>> login(String username, String password) async {
    try {
      final response = await http.post(
        Uri.parse(ConfigUrl.loginUrl),
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json",
        },
        body: jsonEncode({"username": username, "password": password}),
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        bool status = data['status'] ?? false;

        if (!status) {
          return {
            "success": false,
            "message": data['message'] ?? 'Login failed',
          };
        }

        // Lấy token từ response
        String token = data['token'];

        // Decode token để lấy thông tin user
        Map<String, dynamic> decodedToken = JwtDecoder.decode(token);

        // Lấy role từ decoded token
        String? role = _extractRole(decodedToken);

        // Lưu token vào SharedPreferences
        SharedPreferences prefs = await SharedPreferences.getInstance();
        await prefs.setString('jwt_token', token);
        await prefs.setString('user_role', role ?? 'Customer');

        // Lưu thêm user info nếu có
        if (data['user'] != null) {
          await prefs.setString('user_info', jsonEncode(data['user']));
        }

        return {
          "success": true,
          "token": token,
          "role": role,
          "decodedToken": decodedToken,
          "user": data['user'],
        };
      } else if (response.statusCode == 401) {
        final data = jsonDecode(response.body);
        return {
          "success": false,
          "message": data['message'] ?? 'Invalid credentials',
        };
      } else {
        return {
          "success": false,
          "message": "Failed to login: ${response.statusCode}",
        };
      }
    } catch (e) {
      return {"success": false, "message": "Network error: $e"};
    }
  }

  /// Đăng ký
  Future<Map<String, dynamic>> register({
    required String username,
    required String email,
    required String password,
    String? fullName,
    String? initials,
    String? phone,
    String? role,
  }) async {
    try {
      final body = {
        "username": username,
        "email": email,
        "password": password,
        "fullName": fullName,
        "initials": initials,
        "role": role ?? "Customer",
      };

      // Thêm phone nếu có
      if (phone != null && phone.isNotEmpty) {
        body["phoneNumber"] = phone;
      }

      final response = await http.post(
        Uri.parse(ConfigUrl.registerUrl),
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json",
        },
        body: jsonEncode(body),
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return {
          "success": data['status'] ?? true,
          "message": data['message'] ?? 'Đăng ký thành công',
        };
      } else {
        final data = jsonDecode(response.body);
        return {
          "success": false,
          "message":
              data['message'] ?? 'Đăng ký thất bại: ${response.statusCode}',
        };
      }
    } catch (e) {
      return {"success": false, "message": "Lỗi kết nối: $e"};
    }
  }

  /// Lấy profile user
  Future<Map<String, dynamic>> getProfile() async {
    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      if (token == null) {
        return {"success": false, "message": "Not authenticated"};
      }

      final response = await http.get(
        Uri.parse(ConfigUrl.profileUrl),
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json",
          "Authorization": "Bearer $token",
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return {"success": true, "user": data['user']};
      } else if (response.statusCode == 401) {
        // Token expired or invalid
        await logout();
        return {"success": false, "message": "Session expired"};
      } else {
        return {"success": false, "message": "Failed to get profile"};
      }
    } catch (e) {
      return {"success": false, "message": "Network error: $e"};
    }
  }

  /// Đăng xuất
  Future<void> logout() async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    await prefs.remove('jwt_token');
    await prefs.remove('user_role');
    await prefs.remove('user_info');
  }

  /// Kiểm tra đã đăng nhập chưa
  Future<bool> isLoggedIn() async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    String? token = prefs.getString('jwt_token');

    if (token == null) return false;

    // Kiểm tra token có hết hạn không
    if (JwtDecoder.isExpired(token)) {
      await logout();
      return false;
    }

    return true;
  }

  /// Lấy token hiện tại
  Future<String?> getToken() async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    return prefs.getString('jwt_token');
  }

  /// Lấy role hiện tại
  Future<String?> getRole() async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    return prefs.getString('user_role');
  }

  /// Lấy thông tin user đã lưu trong SharedPreferences
  Future<Map<String, dynamic>?> getUserInfo() async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    String? userInfoStr = prefs.getString('user_info');
    if (userInfoStr != null) {
      return jsonDecode(userInfoStr);
    }
    return null;
  }

  /// Lấy tên user để hiển thị (username - tên đăng nhập)
  Future<String> getUserDisplayName() async {
    final userInfo = await getUserInfo();
    if (userInfo != null) {
      return userInfo['userName'] ?? userInfo['fullName'] ?? 'Khách hàng';
    }
    return 'Khách hàng';
  }

  /// Extract role từ decoded token
  String? _extractRole(Map<String, dynamic> decodedToken) {
    // JWT claims có thể chứa role ở nhiều key khác nhau
    // Thử các key phổ biến
    var role =
        decodedToken['role'] ??
        decodedToken['roles'] ??
        decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

    if (role is List && role.isNotEmpty) {
      return role.first.toString();
    }
    return role?.toString();
  }
}
