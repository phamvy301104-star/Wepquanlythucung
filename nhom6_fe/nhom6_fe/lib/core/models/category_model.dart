/// Category model for product categories
class CategoryModel {
  final int id;
  final String name;
  final String? slug;
  final String? description;
  final String? imageUrl;
  final String? icon;
  final int? parentCategoryId;
  final int displayOrder;
  final bool showOnHomePage;
  final int productCount;

  CategoryModel({
    required this.id,
    required this.name,
    this.slug,
    this.description,
    this.imageUrl,
    this.icon,
    this.parentCategoryId,
    this.displayOrder = 0,
    this.showOnHomePage = false,
    this.productCount = 0,
  });

  /// Process image URL to ensure it's a full URL (similar to Product)
  static String? _processImageUrl(String? rawUrl) {
    if (rawUrl == null || rawUrl.isEmpty) return null;

    final imageStr = rawUrl.toString().trim();

    // If already a full URL (starts with http), use as is
    if (imageStr.startsWith('http://') || imageStr.startsWith('https://')) {
      return imageStr;
    }

    // For relative paths, we need to construct full URL
    // Using a base URL pattern similar to ConfigUrl
    // This should match your backend server URL
    const baseUrlWithoutApi = 'http://localhost:5078/';

    // If starts with /, prepend base URL
    if (imageStr.startsWith('/')) {
      return '$baseUrlWithoutApi${imageStr.substring(1)}';
    }

    // Otherwise, assume it's relative path
    return '$baseUrlWithoutApi$imageStr';
  }

  factory CategoryModel.fromJson(Map<String, dynamic> json) {
    return CategoryModel(
      id: json['id'] ?? 0,
      name: json['name'] ?? '',
      slug: json['slug'],
      description: json['description'],
      imageUrl: _processImageUrl(json['imageUrl']),
      icon: json['icon'],
      parentCategoryId: json['parentCategoryId'],
      displayOrder: json['displayOrder'] ?? 0,
      showOnHomePage: json['showOnHomePage'] ?? false,
      productCount: json['productCount'] ?? 0,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'slug': slug,
      'description': description,
      'imageUrl': imageUrl,
      'icon': icon,
      'parentCategoryId': parentCategoryId,
      'displayOrder': displayOrder,
      'showOnHomePage': showOnHomePage,
      'productCount': productCount,
    };
  }

  /// Get display icon (emoji or default)
  String get displayIcon => icon ?? '📦';

  /// Check if category has image
  bool get hasImage => imageUrl != null && imageUrl!.isNotEmpty;

  @override
  String toString() => 'CategoryModel(id: $id, name: $name)';

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is CategoryModel && other.id == id;
  }

  @override
  int get hashCode => id.hashCode;
}
