import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import '../config/config_url.dart';

/// Product model
class Product {
  final int id;
  final String name;
  final String? description;
  final String? shortDescription;
  final double price;
  final double? salePrice;
  final double? originalPrice;
  final double? discountPercentage;
  final String? imageUrl;
  final List<String> additionalImages;
  final double rating;
  final int soldCount;
  final int stockQuantity;
  final String? categoryName;
  final String? brandName;
  final String? sku;
  final String? unit;
  final String? ingredients;
  final String? usage;
  final int? weight;
  final int? volume;
  final bool isActive;

  Product({
    required this.id,
    required this.name,
    this.description,
    this.shortDescription,
    required this.price,
    this.salePrice,
    this.originalPrice,
    this.discountPercentage,
    this.imageUrl,
    this.additionalImages = const [],
    this.rating = 0,
    this.soldCount = 0,
    this.stockQuantity = 0,
    this.categoryName,
    this.brandName,
    this.sku,
    this.unit,
    this.ingredients,
    this.usage,
    this.weight,
    this.volume,
    this.isActive = true,
  });

  /// Process image URL to ensure it's a full URL
  static String? _processImageUrl(String? rawUrl) {
    if (rawUrl == null || rawUrl.isEmpty) return null;

    final imageStr = rawUrl.toString().trim();

    // If already a full URL (starts with http), use as is
    if (imageStr.startsWith('http://') || imageStr.startsWith('https://')) {
      return imageStr;
    }

    // Get base URL without /api/
    final baseUrlWithoutApi = ConfigUrl.baseUrl.replaceAll('/api/', '/');

    // If starts with /, prepend base URL
    if (imageStr.startsWith('/')) {
      return '$baseUrlWithoutApi${imageStr.substring(1)}';
    }

    // Otherwise, assume it's relative path
    return '$baseUrlWithoutApi$imageStr';
  }

  factory Product.fromJson(Map<String, dynamic> json) {
    // Process main image URL
    final processedImageUrl = _processImageUrl(json['imageUrl']);

    // Process additional images
    List<String> additionalImagesList = [];
    final additionalImagesRaw = json['additionalImages'];
    if (additionalImagesRaw != null) {
      if (additionalImagesRaw is String && additionalImagesRaw.isNotEmpty) {
        try {
          // Parse JSON array string
          final parsed = jsonDecode(additionalImagesRaw);
          if (parsed is List) {
            additionalImagesList = parsed
                .map((e) => _processImageUrl(e.toString()))
                .where((e) => e != null)
                .cast<String>()
                .toList();
          }
        } catch (_) {
          // If not valid JSON, treat as single URL
          final processed = _processImageUrl(additionalImagesRaw);
          if (processed != null) additionalImagesList = [processed];
        }
      } else if (additionalImagesRaw is List) {
        additionalImagesList = additionalImagesRaw
            .map((e) => _processImageUrl(e.toString()))
            .where((e) => e != null)
            .cast<String>()
            .toList();
      }
    }

    // Debug log for image URLs
    debugPrint(
      '[Product] ${json['name']} - imageUrl: ${json['imageUrl']} -> $processedImageUrl',
    );
    debugPrint(
      '[Product] ${json['name']} - additionalImages raw: $additionalImagesRaw',
    );
    debugPrint(
      '[Product] ${json['name']} - additionalImages parsed: $additionalImagesList',
    );

    return Product(
      id: json['id'] ?? 0,
      name: json['name'] ?? '',
      description: json['description'],
      shortDescription: json['shortDescription'],
      price: (json['price'] ?? 0).toDouble(),
      salePrice: json['salePrice']?.toDouble(),
      originalPrice: json['originalPrice']?.toDouble(),
      discountPercentage: json['discountPercent']?.toDouble(),
      imageUrl: processedImageUrl,
      additionalImages: additionalImagesList,
      rating: (json['averageRating'] ?? json['rating'] ?? 0).toDouble(),
      soldCount: json['soldCount'] ?? 0,
      stockQuantity: json['stockQuantity'] ?? 0,
      categoryName: json['categoryName'] ?? json['category']?['name'],
      brandName: json['brandName'] ?? json['brand']?['name'],
      sku: json['sku'] ?? json['SKU'],
      unit: json['unit'],
      ingredients: json['ingredients'],
      usage: json['usage'],
      weight: json['weight'],
      volume: json['volume'],
      isActive: json['isActive'] ?? true,
    );
  }

  /// Get all images (main + additional)
  List<String> get allImages {
    final images = <String>[];
    if (imageUrl != null) images.add(imageUrl!);
    images.addAll(additionalImages);
    return images;
  }

  /// Check if product has discount
  bool get hasDiscount =>
      salePrice != null && salePrice! > 0 && salePrice! < price;

  /// Get discount percentage
  int get discountPercent =>
      hasDiscount ? ((price - salePrice!) / price * 100).round() : 0;

  /// Check if product is in stock
  bool get isInStock => stockQuantity > 0;

  /// Format price to Vietnamese currency
  String get formattedPrice {
    return '${price.toStringAsFixed(0).replaceAllMapped(RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'), (Match m) => '${m[1]}.')}đ';
  }

  String? get formattedSalePrice {
    if (salePrice == null) return null;
    return '${salePrice!.toStringAsFixed(0).replaceAllMapped(RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'), (Match m) => '${m[1]}.')}đ';
  }

  /// Get display price (sale price if available, otherwise regular price)
  double get displayPrice => salePrice ?? price;

  String get formattedDisplayPrice {
    return '${displayPrice.toStringAsFixed(0).replaceAllMapped(RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'), (Match m) => '${m[1]}.')}đ';
  }
}

/// Product Service - Fetch products from API
class ProductService {
  static final ProductService _instance = ProductService._internal();
  factory ProductService() => _instance;
  ProductService._internal();

  /// Get featured products (best sellers or newest)
  Future<List<Product>> getFeaturedProducts({int limit = 10}) async {
    try {
      final url =
          '${ConfigUrl.baseUrl}ProductApi?pageSize=$limit&sortBy=soldCount&sortDesc=true';
      debugPrint('[ProductService] Fetching: $url');

      final response = await http.get(
        Uri.parse(url),
        headers: {'Accept': 'application/json'},
      );

      debugPrint('[ProductService] Status: ${response.statusCode}');

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
            .map((item) => Product.fromJson(item))
            .where((p) => p.isActive)
            .toList();
      }
      return [];
    } catch (e) {
      debugPrint('[ProductService] Error: $e');
      return [];
    }
  }

  /// Get all products with pagination
  Future<List<Product>> getProducts({
    int page = 1,
    int pageSize = 20,
    String? categoryId,
    String? search,
  }) async {
    try {
      var url = '${ConfigUrl.baseUrl}ProductApi?page=$page&pageSize=$pageSize';

      if (categoryId != null) {
        url += '&categoryId=$categoryId';
      }
      if (search != null && search.isNotEmpty) {
        url += '&search=$search';
      }

      debugPrint('[ProductService] Fetching: $url');

      final response = await http.get(
        Uri.parse(url),
        headers: {'Accept': 'application/json'},
      );

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
            .map((item) => Product.fromJson(item))
            .where((p) => p.isActive)
            .toList();
      }
      return [];
    } catch (e) {
      debugPrint('[ProductService] Error: $e');
      return [];
    }
  }

  /// Get product by ID
  Future<Product?> getProductById(int id) async {
    try {
      final url = '${ConfigUrl.baseUrl}ProductApi/$id';

      final response = await http.get(
        Uri.parse(url),
        headers: {'Accept': 'application/json'},
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return Product.fromJson(data['data'] ?? data);
      }
      return null;
    } catch (e) {
      debugPrint('[ProductService] Error: $e');
      return null;
    }
  }
}
