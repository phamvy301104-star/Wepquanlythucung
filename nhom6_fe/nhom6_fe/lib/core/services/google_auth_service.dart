import 'package:google_sign_in/google_sign_in.dart';
import 'package:flutter/foundation.dart';
import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import 'package:jwt_decoder/jwt_decoder.dart';
import '../config/config_url.dart';

/// Google Sign-In Service
/// Handles Google authentication and backend token exchange
class GoogleAuthService {
  static final GoogleAuthService _instance = GoogleAuthService._internal();
  factory GoogleAuthService() => _instance;
  GoogleAuthService._internal();

  final GoogleSignIn _googleSignIn = GoogleSignIn(scopes: ['email', 'profile']);

  /// Sign in with Google
  /// Returns user data and JWT token on success
  Future<Map<String, dynamic>> signInWithGoogle() async {
    try {
      // 1. Trigger Google Sign In
      final GoogleSignInAccount? account = await _googleSignIn.signIn();

      if (account == null) {
        // User cancelled the sign-in
        return {'success': false, 'message': 'Đăng nhập Google bị hủy'};
      }

      debugPrint('[GoogleAuth] Account: ${account.email}');

      // 2. Get Google authentication
      final GoogleSignInAuthentication auth = await account.authentication;

      // 3. Get Access Token (REQUIRED for backend userinfo verification)
      final String? accessToken = auth.accessToken;
      final String? idToken = auth.idToken;

      debugPrint('[GoogleAuth] accessToken available: ${accessToken != null}');
      debugPrint('[GoogleAuth] idToken available: ${idToken != null}');

      if (accessToken == null) {
        await _googleSignIn.signOut();
        return {
          'success': false,
          'message': 'Không thể lấy Google Access Token',
        };
      }

      // 4. Send token to backend for verification and JWT generation
      final response = await _sendToBackend(
        accessToken: accessToken,
        idToken: idToken,
        email: account.email,
        displayName: account.displayName,
        photoUrl: account.photoUrl,
      );

      return response;
    } catch (e) {
      debugPrint('[GoogleAuth] Error: $e');
      await _googleSignIn.signOut();
      return {'success': false, 'message': 'Lỗi đăng nhập Google: $e'};
    }
  }

  /// Send Google credentials to backend
  Future<Map<String, dynamic>> _sendToBackend({
    required String accessToken,
    String? idToken,
    required String email,
    String? displayName,
    String? photoUrl,
  }) async {
    try {
      final url = '${ConfigUrl.baseUrl}Authenticate/external-login';
      debugPrint('[GoogleAuth] Sending to backend: $url');

      final response = await http.post(
        Uri.parse(url),
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
        },
        body: jsonEncode({
          'provider': 'Google',
          'accessToken': accessToken,
          'idToken': idToken,
          'email': email,
          'displayName': displayName,
          'photoUrl': photoUrl,
        }),
      );

      debugPrint('[GoogleAuth] Response status: ${response.statusCode}');
      debugPrint('[GoogleAuth] Response body: ${response.body}');

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);

        // Check if login was successful
        bool status = data['status'] ?? false;
        if (!status) {
          return {
            'success': false,
            'message': data['message'] ?? 'Đăng nhập thất bại',
          };
        }

        // Get token from response
        String token = data['token'];

        // Decode token to get user info
        Map<String, dynamic> decodedToken = JwtDecoder.decode(token);

        // Extract role from decoded token
        String? role = _extractRole(decodedToken);

        // Save to SharedPreferences
        SharedPreferences prefs = await SharedPreferences.getInstance();
        await prefs.setString('jwt_token', token);
        await prefs.setString(
          'token',
          token,
        ); // Also save as 'token' for compatibility
        await prefs.setString('user_role', role ?? 'Customer');
        await prefs.setBool('isLoggedIn', true);

        // Save user info if available
        if (data['user'] != null) {
          await prefs.setString('user_info', jsonEncode(data['user']));
          await prefs.setString(
            'user_name',
            data['user']['fullName'] ?? displayName ?? '',
          );
          await prefs.setString('user_email', data['user']['email'] ?? email);
          if (data['user']['avatarUrl'] != null) {
            await prefs.setString('user_avatar', data['user']['avatarUrl']);
          } else if (photoUrl != null) {
            await prefs.setString('user_avatar', photoUrl);
          }
        }

        return {
          'success': true,
          'token': token,
          'role': role,
          'decodedToken': decodedToken,
          'user': data['user'],
        };
      } else {
        final data = jsonDecode(response.body);
        return {
          'success': false,
          'message':
              data['message'] ?? 'Đăng nhập thất bại: ${response.statusCode}',
        };
      }
    } catch (e) {
      debugPrint('[GoogleAuth] Backend error: $e');
      return {'success': false, 'message': 'Lỗi kết nối server: $e'};
    }
  }

  /// Extract role from JWT token claims
  String? _extractRole(Map<String, dynamic> decodedToken) {
    // Try different claim formats
    if (decodedToken.containsKey('role')) {
      var role = decodedToken['role'];
      if (role is List && role.isNotEmpty) {
        return role.first.toString();
      }
      return role?.toString();
    }

    // Microsoft identity claim
    if (decodedToken.containsKey(
      'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
    )) {
      var role =
          decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      if (role is List && role.isNotEmpty) {
        return role.first.toString();
      }
      return role?.toString();
    }

    return null;
  }

  /// Sign out from Google
  Future<void> signOut() async {
    try {
      await _googleSignIn.signOut();

      // Clear local storage
      SharedPreferences prefs = await SharedPreferences.getInstance();
      await prefs.remove('jwt_token');
      await prefs.remove('token');
      await prefs.remove('user_role');
      await prefs.remove('user_info');
      await prefs.remove('user_name');
      await prefs.remove('user_email');
      await prefs.remove('user_avatar');
      await prefs.setBool('isLoggedIn', false);

      debugPrint('[GoogleAuth] Signed out successfully');
    } catch (e) {
      debugPrint('[GoogleAuth] Sign out error: $e');
    }
  }

  /// Check if user is currently signed in with Google
  Future<bool> isSignedIn() async {
    return await _googleSignIn.isSignedIn();
  }

  /// Get current Google account (if signed in)
  GoogleSignInAccount? get currentAccount => _googleSignIn.currentUser;

  /// Silent sign in (try to restore previous session)
  Future<Map<String, dynamic>?> trySilentSignIn() async {
    try {
      final account = await _googleSignIn.signInSilently();
      if (account != null) {
        final auth = await account.authentication;
        if (auth.accessToken != null) {
          return await _sendToBackend(
            accessToken: auth.accessToken!,
            idToken: auth.idToken,
            email: account.email,
            displayName: account.displayName,
            photoUrl: account.photoUrl,
          );
        }
      }
    } catch (e) {
      debugPrint('[GoogleAuth] Silent sign in error: $e');
    }
    return null;
  }
}
