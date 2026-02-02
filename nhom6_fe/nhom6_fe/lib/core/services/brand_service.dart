import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import '../config/config_url.dart';
import '../models/brand_model.dart';

/// Brand Service - Fetch brands from API
class BrandService {
  static final BrandService _instance = BrandService._internal();
  factory BrandService() => _instance;
  BrandService._internal();

  /// Get all brands
  Future<List<BrandModel>> getBrands() async {
    try {
      final url = '${ConfigUrl.baseUrl}BrandApi';
      debugPrint('[BrandService] Fetching: $url');

      final response = await http.get(
        Uri.parse(url),
        headers: {
          'Accept': 'application/json',
          'ngrok-skip-browser-warning': 'true',
        },
      );

      debugPrint('[BrandService] Status: ${response.statusCode}');

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);

        List<dynamic> items;
        if (data is List) {
          items = data;
        } else if (data['data'] != null) {
          items = data['data'] is List ? data['data'] : [data['data']];
        } else if (data['items'] != null) {
          items = data['items'];
        } else {
          items = [];
        }

        return items.map((item) => BrandModel.fromJson(item)).toList();
      }

      debugPrint('[BrandService] Error: ${response.body}');
      return [];
    } catch (e) {
      debugPrint('[BrandService] Exception: $e');
      return [];
    }
  }

  /// Get featured brands
  Future<List<BrandModel>> getFeaturedBrands() async {
    try {
      final url = '${ConfigUrl.baseUrl}BrandApi/featured';
      debugPrint('[BrandService] Fetching featured brands: $url');

      final response = await http.get(
        Uri.parse(url),
        headers: {
          'Accept': 'application/json',
          'ngrok-skip-browser-warning': 'true',
        },
      );

      debugPrint('[BrandService] Status: ${response.statusCode}');

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);

        List<dynamic> items;
        if (data is List) {
          items = data;
        } else if (data['data'] != null) {
          items = data['data'] is List ? data['data'] : [data['data']];
        } else {
          items = [];
        }

        return items.map((item) => BrandModel.fromJson(item)).toList();
      }

      debugPrint('[BrandService] Error: ${response.body}');
      return [];
    } catch (e) {
      debugPrint('[BrandService] Exception: $e');
      return [];
    }
  }

  /// Get brand by ID
  Future<BrandModel?> getBrandById(int id) async {
    try {
      final url = '${ConfigUrl.baseUrl}BrandApi/$id';
      debugPrint('[BrandService] Fetching brand $id: $url');

      final response = await http.get(
        Uri.parse(url),
        headers: {
          'Accept': 'application/json',
          'ngrok-skip-browser-warning': 'true',
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return BrandModel.fromJson(data);
      }

      return null;
    } catch (e) {
      debugPrint('[BrandService] Exception: $e');
      return null;
    }
  }

  /// Get products by brand ID
  Future<Map<String, dynamic>> getProductsByBrand(
    int brandId, {
    int page = 1,
    int pageSize = 10,
  }) async {
    try {
      final url =
          '${ConfigUrl.baseUrl}BrandApi/$brandId/products?page=$page&pageSize=$pageSize';
      debugPrint('[BrandService] Fetching products for brand $brandId: $url');

      final response = await http.get(
        Uri.parse(url),
        headers: {
          'Accept': 'application/json',
          'ngrok-skip-browser-warning': 'true',
        },
      );

      if (response.statusCode == 200) {
        return jsonDecode(response.body);
      }

      return {'items': [], 'totalCount': 0, 'page': page, 'pageSize': pageSize};
    } catch (e) {
      debugPrint('[BrandService] Exception: $e');
      return {'items': [], 'totalCount': 0, 'page': page, 'pageSize': pageSize};
    }
  }
}
