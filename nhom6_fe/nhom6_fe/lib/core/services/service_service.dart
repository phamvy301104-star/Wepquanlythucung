import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import '../config/config_url.dart';

/// Service model
class ServiceModel {
  final int id;
  final String name;
  final String? description;
  final double price;
  final int duration; // in minutes
  final String? imageUrl;
  final String? categoryName;
  final bool isActive;

  ServiceModel({
    required this.id,
    required this.name,
    this.description,
    required this.price,
    this.duration = 30,
    this.imageUrl,
    this.categoryName,
    this.isActive = true,
  });

  factory ServiceModel.fromJson(Map<String, dynamic> json) {
    return ServiceModel(
      id: json['id'] ?? 0,
      name: json['name'] ?? '',
      description: json['description'],
      price: (json['price'] ?? 0).toDouble(),
      duration: json['duration'] ?? json['estimatedDuration'] ?? 30,
      imageUrl: json['imageUrl'],
      categoryName: json['categoryName'] ?? json['category']?['name'],
      isActive: json['isActive'] ?? true,
    );
  }

  /// Format price to Vietnamese currency
  String get formattedPrice {
    return '${price.toStringAsFixed(0).replaceAllMapped(RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'), (Match m) => '${m[1]}.')}đ';
  }

  /// Format duration to readable string
  String get formattedDuration {
    if (duration < 60) {
      return '$duration phút';
    } else {
      int hours = duration ~/ 60;
      int mins = duration % 60;
      if (mins == 0) {
        return '$hours giờ';
      }
      return '$hours giờ $mins phút';
    }
  }
}

/// Service Service - Fetch services from API
class ServiceService {
  static final ServiceService _instance = ServiceService._internal();
  factory ServiceService() => _instance;
  ServiceService._internal();

  /// Get all active services
  Future<List<ServiceModel>> getServices({int limit = 20}) async {
    try {
      final url = '${ConfigUrl.baseUrl}ServiceApi?pageSize=$limit';
      debugPrint('[ServiceService] Fetching: $url');

      final response = await http.get(
        Uri.parse(url),
        headers: {'Accept': 'application/json'},
      );

      debugPrint('[ServiceService] Status: ${response.statusCode}');

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);

        // Handle different response structures
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

        return items
            .map((item) => ServiceModel.fromJson(item))
            .where((s) => s.isActive)
            .toList();
      }
      return [];
    } catch (e) {
      debugPrint('[ServiceService] Error: $e');
      return [];
    }
  }

  /// Get featured services (for home screen)
  Future<List<ServiceModel>> getFeaturedServices({int limit = 6}) async {
    try {
      // First try the featured endpoint if available
      var url = '${ConfigUrl.baseUrl}ServiceApi/featured?limit=$limit';
      debugPrint('[ServiceService] Fetching featured: $url');

      var response = await http.get(
        Uri.parse(url),
        headers: {'Accept': 'application/json'},
      );

      // If featured endpoint doesn't exist, get all services
      if (response.statusCode == 404) {
        url = '${ConfigUrl.baseUrl}ServiceApi?pageSize=$limit';
        response = await http.get(
          Uri.parse(url),
          headers: {'Accept': 'application/json'},
        );
      }

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

        return items
            .map((item) => ServiceModel.fromJson(item))
            .where((s) => s.isActive)
            .take(limit)
            .toList();
      }
      return [];
    } catch (e) {
      debugPrint('[ServiceService] Error: $e');
      return [];
    }
  }

  /// Get service by ID
  Future<ServiceModel?> getServiceById(int id) async {
    try {
      final url = '${ConfigUrl.baseUrl}ServiceApi/$id';

      final response = await http.get(
        Uri.parse(url),
        headers: {'Accept': 'application/json'},
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return ServiceModel.fromJson(data['data'] ?? data);
      }
      return null;
    } catch (e) {
      debugPrint('[ServiceService] Error: $e');
      return null;
    }
  }

  /// Fetch services as raw JSON for chatbot
  Future<List<Map<String, dynamic>>> fetchServices() async {
    try {
      final services = await getServices();
      return services.map((service) {
        return {
          'name': service.name,
          'description': service.description,
          'price': service.price,
          'originalPrice': null, // Services don't have original price
          'duration': service.duration,
        };
      }).toList();
    } catch (e) {
      debugPrint('[ServiceService] Error fetching services: $e');
      return [];
    }
  }
}
