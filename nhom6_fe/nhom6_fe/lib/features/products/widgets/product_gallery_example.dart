import 'package:flutter/material.dart';
import 'package:nhom6_fe/features/products/widgets/product_gallery_carousel.dart';

/// Example: Cách sử dụng ProductGalleryCarousel
///
/// Trường hợp 1: Chỉ có 1 ảnh chính (không có gallery)
/// Trường hợp 2: Có ảnh chính + nhiều ảnh phụ

class ProductGalleryExample extends StatelessWidget {
  const ProductGalleryExample({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Product Gallery Examples')),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          const Text(
            'Case 1: Single Image',
            style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 8),
          ProductGalleryCarousel(
            mainImageUrl: 'https://via.placeholder.com/500x500?text=Main+Image',
          ),

          const SizedBox(height: 32),

          const Text(
            'Case 2: Multiple Images (Main + 3 Additional)',
            style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 8),
          ProductGalleryCarousel(
            mainImageUrl: 'https://via.placeholder.com/500x500?text=Image+1',
            additionalImages: [
              'https://via.placeholder.com/500x500?text=Image+2',
              'https://via.placeholder.com/500x500?text=Image+3',
              'https://via.placeholder.com/500x500?text=Image+4',
            ],
          ),

          const SizedBox(height: 32),

          const Text(
            'Case 3: Custom Height (200)',
            style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 8),
          ProductGalleryCarousel(
            mainImageUrl: 'https://via.placeholder.com/500x500?text=Compact',
            height: 200,
            additionalImages: [
              'https://via.placeholder.com/500x500?text=Image+2',
            ],
          ),
        ],
      ),
    );
  }
}

/// Cách tích hợp vào Product Detail Screen:
/// 
/// ```dart
/// class ProductDetailScreen extends StatelessWidget {
///   final Product product;
/// 
///   @override
///   Widget build(BuildContext context) {
///     // Parse additionalImages từ JSON string
///     List<String>? additionalImages;
///     if (product.additionalImages != null && product.additionalImages!.isNotEmpty) {
///       try {
///         final decoded = json.decode(product.additionalImages!);
///         additionalImages = List<String>.from(decoded);
///       } catch (e) {
///         print('Error parsing additionalImages: $e');
///       }
///     }
/// 
///     return Scaffold(
///       body: CustomScrollView(
///         slivers: [
///           SliverAppBar(
///             expandedHeight: 300,
///             flexibleSpace: FlexibleSpaceBar(
///               background: ProductGalleryCarousel(
///                 mainImageUrl: product.imageUrl ?? 'https://via.placeholder.com/300',
///                 additionalImages: additionalImages,
///               ),
///             ),
///           ),
///           SliverToBoxAdapter(
///             child: Padding(
///               padding: const EdgeInsets.all(16),
///               child: Column(
///                 crossAxisAlignment: CrossAxisAlignment.start,
///                 children: [
///                   Text(product.name, style: Theme.of(context).textTheme.headlineSmall),
///                   const SizedBox(height: 8),
///                   Text('\$${product.price}', style: Theme.of(context).textTheme.titleLarge),
///                   // ... other product info
///                 ],
///               ),
///             ),
///           ),
///         ],
///       ),
///     );
///   }
/// }
/// ```
