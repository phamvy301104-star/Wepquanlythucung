import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:http/io_client.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../config/config_url.dart';

class ApiClient {
  final String baseUrl;
  late final http.Client _httpClient;

  ApiClient({String? baseUrl}) : baseUrl = baseUrl ?? ConfigUrl.baseUrl {
    // Create custom HttpClient that bypasses SSL certificate verification (Dev only)
    final httpClient = HttpClient()
      ..badCertificateCallback =
          (X509Certificate cert, String host, int port) => true;
    _httpClient = IOClient(httpClient);
  }

  /// GET request
  Future<http.Response> get(String endpoint, {Map<String, String>? headers}) {
    return _httpClient.get(
      Uri.parse('$baseUrl$endpoint'),
      headers: _buildHeaders(headers),
    );
  }

  /// GET request with authentication
  Future<http.Response> getWithAuth(
    String endpoint, {
    Map<String, String>? headers,
  }) async {
    final token = await _getToken();
    return _httpClient.get(
      Uri.parse('$baseUrl$endpoint'),
      headers: _buildHeadersWithAuth(headers, token),
    );
  }

  /// POST request
  Future<http.Response> post(
    String endpoint, {
    Map<String, String>? headers,
    dynamic body,
  }) {
    return _httpClient.post(
      Uri.parse('$baseUrl$endpoint'),
      headers: _buildHeaders(headers),
      body: jsonEncode(body),
    );
  }

  /// POST request with authentication
  Future<http.Response> postWithAuth(
    String endpoint, {
    Map<String, String>? headers,
    dynamic body,
  }) async {
    final token = await _getToken();
    return _httpClient.post(
      Uri.parse('$baseUrl$endpoint'),
      headers: _buildHeadersWithAuth(headers, token),
      body: jsonEncode(body),
    );
  }

  /// PUT request
  Future<http.Response> put(
    String endpoint, {
    Map<String, String>? headers,
    dynamic body,
  }) {
    return _httpClient.put(
      Uri.parse('$baseUrl$endpoint'),
      headers: _buildHeaders(headers),
      body: jsonEncode(body),
    );
  }

  /// PUT request with authentication
  Future<http.Response> putWithAuth(
    String endpoint, {
    Map<String, String>? headers,
    dynamic body,
  }) async {
    final token = await _getToken();
    return _httpClient.put(
      Uri.parse('$baseUrl$endpoint'),
      headers: _buildHeadersWithAuth(headers, token),
      body: jsonEncode(body),
    );
  }

  /// DELETE request
  Future<http.Response> delete(
    String endpoint, {
    Map<String, String>? headers,
  }) {
    return _httpClient.delete(
      Uri.parse('$baseUrl$endpoint'),
      headers: _buildHeaders(headers),
    );
  }

  /// DELETE request with authentication
  Future<http.Response> deleteWithAuth(
    String endpoint, {
    Map<String, String>? headers,
  }) async {
    final token = await _getToken();
    return _httpClient.delete(
      Uri.parse('$baseUrl$endpoint'),
      headers: _buildHeadersWithAuth(headers, token),
    );
  }

  /// PATCH request
  Future<http.Response> patch(
    String endpoint, {
    Map<String, String>? headers,
    dynamic body,
  }) {
    return _httpClient.patch(
      Uri.parse('$baseUrl$endpoint'),
      headers: _buildHeaders(headers),
      body: jsonEncode(body),
    );
  }

  /// PATCH request with authentication
  Future<http.Response> patchWithAuth(
    String endpoint, {
    Map<String, String>? headers,
    dynamic body,
  }) async {
    final token = await _getToken();
    return _httpClient.patch(
      Uri.parse('$baseUrl$endpoint'),
      headers: _buildHeadersWithAuth(headers, token),
      body: jsonEncode(body),
    );
  }

  /// Build headers without auth
  Map<String, String> _buildHeaders(Map<String, String>? headers) {
    return {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'ngrok-skip-browser-warning': 'true', // Required for ngrok free tier
      if (headers != null) ...headers,
    };
  }

  /// Build headers with auth token
  Map<String, String> _buildHeadersWithAuth(
    Map<String, String>? headers,
    String? token,
  ) {
    return {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'ngrok-skip-browser-warning': 'true', // Required for ngrok free tier
      if (token != null) 'Authorization': 'Bearer $token',
      if (headers != null) ...headers,
    };
  }

  /// Get token from SharedPreferences
  Future<String?> _getToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('jwt_token');
  }
}
