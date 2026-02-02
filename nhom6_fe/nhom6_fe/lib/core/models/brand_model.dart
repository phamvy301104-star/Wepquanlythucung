/// Brand model for product brands
class BrandModel {
  final int id;
  final String name;
  final String? slug;
  final String? description;
  final String? logoUrl;
  final String? bannerUrl;
  final String? websiteUrl;
  final String? countryOfOrigin;
  final int? yearEstablished;
  final bool isFeatured;
  final int productCount;

  BrandModel({
    required this.id,
    required this.name,
    this.slug,
    this.description,
    this.logoUrl,
    this.bannerUrl,
    this.websiteUrl,
    this.countryOfOrigin,
    this.yearEstablished,
    this.isFeatured = false,
    this.productCount = 0,
  });

  factory BrandModel.fromJson(Map<String, dynamic> json) {
    return BrandModel(
      id: json['id'] ?? 0,
      name: json['name'] ?? '',
      slug: json['slug'],
      description: json['description'],
      logoUrl: json['logoUrl'],
      bannerUrl: json['bannerUrl'],
      websiteUrl: json['websiteUrl'],
      countryOfOrigin: json['countryOfOrigin'],
      yearEstablished: json['yearEstablished'],
      isFeatured: json['isFeatured'] ?? false,
      productCount: json['productCount'] ?? 0,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'slug': slug,
      'description': description,
      'logoUrl': logoUrl,
      'bannerUrl': bannerUrl,
      'websiteUrl': websiteUrl,
      'countryOfOrigin': countryOfOrigin,
      'yearEstablished': yearEstablished,
      'isFeatured': isFeatured,
      'productCount': productCount,
    };
  }

  /// Check if brand has logo
  bool get hasLogo => logoUrl != null && logoUrl!.isNotEmpty;

  /// Get country flag emoji based on country
  String get countryFlag {
    switch (countryOfOrigin?.toLowerCase()) {
      case 'việt nam':
      case 'vietnam':
        return '🇻🇳';
      case 'nhật bản':
      case 'japan':
        return '🇯🇵';
      case 'hàn quốc':
      case 'korea':
        return '🇰🇷';
      case 'mỹ':
      case 'usa':
      case 'united states':
        return '🇺🇸';
      case 'đức':
      case 'germany':
        return '🇩🇪';
      case 'pháp':
      case 'france':
        return '🇫🇷';
      case 'hà lan':
      case 'netherlands':
        return '🇳🇱';
      case 'đan mạch':
      case 'denmark':
        return '🇩🇰';
      case 'australia':
        return '🇦🇺';
      case 'trung quốc':
      case 'china':
        return '🇨🇳';
      default:
        return '🌍';
    }
  }

  @override
  String toString() => 'BrandModel(id: $id, name: $name)';

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is BrandModel && other.id == id;
  }

  @override
  int get hashCode => id.hashCode;
}
