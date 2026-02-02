import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import '../config/config_url.dart';
import '../models/category_model.dart';

/// Category Service - Fetch categories from API
class CategoryService {
  static final CategoryService _instance = CategoryService._internal();
  factory CategoryService() => _instance;
  CategoryService._internal();

  /// Get all categories
  Future<List<CategoryModel>> getCategories() async {
    try {
      final url = '${ConfigUrl.baseUrl}CategoryApi';
      debugPrint('[CategoryService] Fetching: $url');

      final response = await http.get(
        Uri.parse(url),
        headers: {
          'Accept': 'application/json',
          'ngrok-skip-browser-warning': 'true',
        },
      );

      debugPrint('[CategoryService] Status: ${response.statusCode}');

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

        return items.map((item) => CategoryModel.fromJson(item)).toList();
      }

      debugPrint('[CategoryService] Error: ${response.body}');
      return [];
    } catch (e) {
      debugPrint('[CategoryService] Exception: $e');
      return [];
    }
  }

  /// Get categories for homepage (showOnHomePage = true)
  Future<List<CategoryModel>> getHomeCategories() async {
    try {
      final url = '${ConfigUrl.baseUrl}CategoryApi/home';
      debugPrint('[CategoryService] Fetching home categories: $url');

      final response = await http.get(
        Uri.parse(url),
        headers: {
          'Accept': 'application/json',
          'ngrok-skip-browser-warning': 'true',
        },
      );

      debugPrint('[CategoryService] Status: ${response.statusCode}');

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

        return items.map((item) => CategoryModel.fromJson(item)).toList();
      }

      debugPrint('[CategoryService] Error: ${response.body}');
      return [];
    } catch (e) {
      debugPrint('[CategoryService] Exception: $e');
      return [];
    }
  }

  /// Get category by ID
  Future<CategoryModel?> getCategoryById(int id) async {
    try {
      final url = '${ConfigUrl.baseUrl}CategoryApi/$id';
      debugPrint('[CategoryService] Fetching category $id: $url');

      final response = await http.get(
        Uri.parse(url),
        headers: {
          'Accept': 'application/json',
          'ngrok-skip-browser-warning': 'true',
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return CategoryModel.fromJson(data);
      }

      return null;
    } catch (e) {
      debugPrint('[CategoryService] Exception: $e');
      return null;
    }
  }

  /// Get products by category ID
  Future<Map<String, dynamic>> getProductsByCategory(
    int categoryId, {
    int page = 1,
    int pageSize = 10,
  }) async {
    try {
      final url =
          '${ConfigUrl.baseUrl}CategoryApi/$categoryId/products?page=$page&pageSize=$pageSize';
      debugPrint(
        '[CategoryService] Fetching products for category $categoryId: $url',
      );

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
      debugPrint('[CategoryService] Exception: $e');
      return {'items': [], 'totalCount': 0, 'page': page, 'pageSize': pageSize};
    }
  }
}
