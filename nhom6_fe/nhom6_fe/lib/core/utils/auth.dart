import '../services/auth_service.dart';

/// Auth utility class - Static methods for easy access
class Auth {
  static final AuthService _authService = AuthService();

  /// Đăng nhập
  static Future<Map<String, dynamic>> login(
    String username,
    String password,
  ) async {
    return await _authService.login(username, password);
  }

  /// Đăng ký
  static Future<Map<String, dynamic>> register({
    required String username,
    required String email,
    required String password,
    String? fullName,
    String? initials,
    String? phone,
    String? role,
  }) async {
    return await _authService.register(
      username: username,
      email: email,
      password: password,
      fullName: fullName,
      initials: initials,
      phone: phone,
      role: role,
    );
  }

  /// Lấy profile
  static Future<Map<String, dynamic>> getProfile() async {
    return await _authService.getProfile();
  }

  /// Đăng xuất
  static Future<void> logout() async {
    await _authService.logout();
  }

  /// Kiểm tra đã đăng nhập chưa
  static Future<bool> isLoggedIn() async {
    return await _authService.isLoggedIn();
  }

  /// Lấy token hiện tại
  static Future<String?> getToken() async {
    return await _authService.getToken();
  }

  /// Lấy role hiện tại
  static Future<String?> getRole() async {
    return await _authService.getRole();
  }

  /// Lấy thông tin user đã lưu
  static Future<Map<String, dynamic>?> getUserInfo() async {
    return await _authService.getUserInfo();
  }

  /// Lấy tên user để hiển thị
  static Future<String> getUserDisplayName() async {
    return await _authService.getUserDisplayName();
  }
}
