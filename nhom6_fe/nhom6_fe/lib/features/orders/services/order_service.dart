// Order Service for UME App
// Service handling order API calls for users

import 'dart:convert';
import 'dart:developer' as developer;
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:http/io_client.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../../core/config/config_url.dart';
import '../models/order_model.dart';

class OrderService {
  static final OrderService _instance = OrderService._internal();
  factory OrderService() => _instance;

  late final http.Client _httpClient;

  OrderService._internal() {
    // Create custom HttpClient that bypasses SSL certificate verification (Dev only)
    final httpClient = HttpClient()
      ..badCertificateCallback =
          (X509Certificate cert, String host, int port) => true;
    _httpClient = IOClient(httpClient);
  }

  String get baseUrl => ConfigUrl.baseUrl;

  void _log(String message) {
    developer.log(message, name: 'OrderService');
  }

  /// Get authorization headers
  Future<Map<String, String>> _getHeaders() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('jwt_token') ?? prefs.getString('token');

    return {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'ngrok-skip-browser-warning': 'true',
      if (token != null) 'Authorization': 'Bearer $token',
    };
  }

  /// Get user's orders with filtering
  /// GET /api/OrderApi/my-orders
  Future<OrderListResponse> getMyOrders({
    String? status,
    int page = 1,
    int pageSize = 10,
  }) async {
    try {
      final headers = await _getHeaders();
      String url = '${baseUrl}OrderApi/my-orders?page=$page&pageSize=$pageSize';
      if (status != null && status.isNotEmpty && status != 'All') {
        url += '&status=$status';
      }

      _log('GET Orders: $url');

      final response = await _httpClient.get(Uri.parse(url), headers: headers);

      _log('GET Orders Response: ${response.statusCode}');

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);

        // Handle wrapped response { success: true, data: {...} }
        if (data['success'] == true && data['data'] != null) {
          return OrderListResponse.fromJson(data['data']);
        }

        // Handle direct response
        return OrderListResponse.fromJson(data);
      } else if (response.statusCode == 401) {
        throw OrderException('Phiên đăng nhập đã hết hạn', 401);
      } else {
        final error = jsonDecode(response.body);
        throw OrderException(
          error['message'] ?? 'Không thể tải danh sách đơn hàng',
          response.statusCode,
        );
      }
    } catch (e) {
      _log('Error getMyOrders: $e');
      if (e is OrderException) rethrow;
      throw OrderException('Lỗi kết nối: $e', 0);
    }
  }

  /// Get order detail
  /// GET /api/OrderApi/{id}
  Future<Order?> getOrderDetail(int orderId) async {
    try {
      final headers = await _getHeaders();
      final url = '${baseUrl}OrderApi/$orderId';

      _log('GET Order Detail: $url');

      final response = await _httpClient.get(Uri.parse(url), headers: headers);

      _log('GET Order Detail Response: ${response.statusCode}');

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);

        // Handle wrapped response
        if (data['success'] == true && data['data'] != null) {
          return Order.fromJson(data['data']);
        }

        return Order.fromJson(data);
      } else if (response.statusCode == 404) {
        return null;
      } else {
        final error = jsonDecode(response.body);
        throw OrderException(
          error['message'] ?? 'Không thể tải chi tiết đơn hàng',
          response.statusCode,
        );
      }
    } catch (e) {
      _log('Error getOrderDetail: $e');
      if (e is OrderException) rethrow;
      throw OrderException('Lỗi kết nối: $e', 0);
    }
  }

  /// Cancel order
  /// POST /api/OrderApi/{id}/cancel
  Future<bool> cancelOrder(int orderId, String reason) async {
    try {
      final headers = await _getHeaders();
      final url = '${baseUrl}OrderApi/$orderId/cancel';

      _log('POST Cancel Order: $url');

      final response = await _httpClient.post(
        Uri.parse(url),
        headers: headers,
        body: jsonEncode({'reason': reason}),
      );

      _log('POST Cancel Order Response: ${response.statusCode}');

      if (response.statusCode == 200) {
        return true;
      } else {
        final error = jsonDecode(response.body);
        throw OrderException(
          error['message'] ?? 'Không thể hủy đơn hàng',
          response.statusCode,
        );
      }
    } catch (e) {
      _log('Error cancelOrder: $e');
      if (e is OrderException) rethrow;
      throw OrderException('Lỗi kết nối: $e', 0);
    }
  }
}

/// Order list response with pagination
class OrderListResponse {
  final List<Order> orders;
  final int totalCount;
  final int page;
  final int pageSize;
  final int totalPages;

  OrderListResponse({
    required this.orders,
    required this.totalCount,
    required this.page,
    required this.pageSize,
    required this.totalPages,
  });

  factory OrderListResponse.fromJson(Map<String, dynamic> json) {
    return OrderListResponse(
      orders:
          (json['items'] as List<dynamic>?)
              ?.map((e) => Order.fromJson(e))
              .toList() ??
          (json['orders'] as List<dynamic>?)
              ?.map((e) => Order.fromJson(e))
              .toList() ??
          [],
      totalCount: json['totalCount'] ?? json['total'] ?? 0,
      page: json['page'] ?? json['currentPage'] ?? 1,
      pageSize: json['pageSize'] ?? 10,
      totalPages: json['totalPages'] ?? 1,
    );
  }
}

/// Custom exception for Order errors
class OrderException implements Exception {
  final String message;
  final int statusCode;

  OrderException(this.message, this.statusCode);

  @override
  String toString() => 'OrderException: $message (status: $statusCode)';
}
