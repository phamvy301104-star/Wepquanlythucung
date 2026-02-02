import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:hugeicons/hugeicons.dart';
import '../../../core/constants/constants.dart';
import '../../../core/services/product_service.dart';
import '../../cart/cart_provider.dart';
import '../../cart/cart_screen.dart';

/// Product Detail Screen
/// Displays full product information with image gallery
class ProductDetailScreen extends ConsumerStatefulWidget {
  final Product product;

  const ProductDetailScreen({super.key, required this.product});

  @override
  ConsumerState<ProductDetailScreen> createState() =>
      _ProductDetailScreenState();
}

class _ProductDetailScreenState extends ConsumerState<ProductDetailScreen> {
  int _quantity = 1;
  int _selectedImageIndex = 0;
  final PageController _pageController = PageController();

  @override
  void dispose() {
    _pageController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final product = widget.product;
    final images = product.allImages;

    return Scaffold(
      backgroundColor: AppColors.background,
      body: Column(
        children: [
          // Main Content
          Expanded(
            child: CustomScrollView(
              slivers: [
                // App Bar with Image Gallery
                SliverAppBar(
                  expandedHeight: 350,
                  pinned: true,
                  backgroundColor: AppColors.white,
                  leading: _buildBackButton(context),
                  actions: [
                    _buildActionButton(
                      icon: HugeIcons.strokeRoundedFavourite,
                      onTap: () {
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(
                            content: Text('Đã thêm vào yêu thích'),
                            duration: Duration(seconds: 1),
                          ),
                        );
                      },
                    ),
                    _buildActionButton(
                      icon: HugeIcons.strokeRoundedShare08,
                      onTap: () {
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(
                            content: Text('Chức năng chia sẻ đang phát triển'),
                            duration: Duration(seconds: 1),
                          ),
                        );
                      },
                    ),
                    const SizedBox(width: 8),
                  ],
                  flexibleSpace: FlexibleSpaceBar(
                    background: _ImageGallery(
                      images: images,
                      selectedIndex: _selectedImageIndex,
                      pageController: _pageController,
                      onPageChanged: (index) {
                        setState(() => _selectedImageIndex = index);
                      },
                    ),
                  ),
                ),

                // Product Info
                SliverToBoxAdapter(
                  child: Container(
                    color: AppColors.white,
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        // Image Thumbnails
                        if (images.length > 1)
                          _ImageThumbnails(
                            images: images,
                            selectedIndex: _selectedImageIndex,
                            onThumbnailTap: (index) {
                              _pageController.animateToPage(
                                index,
                                duration: const Duration(milliseconds: 300),
                                curve: Curves.easeInOut,
                              );
                              setState(() => _selectedImageIndex = index);
                            },
                          ),

                        // Category & Brand
                        Row(
                          children: [
                            if (product.categoryName != null) ...[
                              Container(
                                padding: const EdgeInsets.symmetric(
                                  horizontal: 8,
                                  vertical: 4,
                                ),
                                decoration: BoxDecoration(
                                  color: AppColors.primary.withOpacity(0.1),
                                  borderRadius: BorderRadius.circular(4),
                                ),
                                child: Text(
                                  product.categoryName!,
                                  style: AppTextStyles.bodySmall.copyWith(
                                    color: AppColors.primary,
                                    fontWeight: FontWeight.w500,
                                  ),
                                ),
                              ),
                              const SizedBox(width: 8),
                            ],
                            if (product.brandName != null) ...[
                              Text(
                                product.brandName!,
                                style: AppTextStyles.bodySmall.copyWith(
                                  color: AppColors.textSecondary,
                                ),
                              ),
                            ],
                          ],
                        ),

                        const SizedBox(height: 12),

                        // Product Name
                        Text(
                          product.name,
                          style: AppTextStyles.h3.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                        ),

                        const SizedBox(height: 8),

                        // Rating & Sold Count
                        Row(
                          children: [
                            if (product.rating > 0) ...[
                              const Icon(
                                Icons.star,
                                size: 18,
                                color: Colors.amber,
                              ),
                              const SizedBox(width: 4),
                              Text(
                                product.rating.toStringAsFixed(1),
                                style: AppTextStyles.bodyMedium.copyWith(
                                  fontWeight: FontWeight.w600,
                                ),
                              ),
                              const SizedBox(width: 16),
                            ],
                            Text(
                              'Đã bán ${product.soldCount}',
                              style: AppTextStyles.bodySmall.copyWith(
                                color: AppColors.textSecondary,
                              ),
                            ),
                          ],
                        ),

                        const SizedBox(height: 16),

                        // Price Section
                        _PriceSection(product: product),

                        const SizedBox(height: 16),

                        // Stock Status
                        _StockStatus(product: product),
                      ],
                    ),
                  ),
                ),

                // Divider
                const SliverToBoxAdapter(child: SizedBox(height: 8)),

                // Product Details Section
                SliverToBoxAdapter(
                  child: Container(
                    color: AppColors.white,
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Thông tin sản phẩm',
                          style: AppTextStyles.h4.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(height: 16),

                        // Product Specs
                        if (product.sku != null)
                          _InfoRow(label: 'Mã sản phẩm', value: product.sku!),
                        if (product.weight != null)
                          _InfoRow(
                            label: 'Trọng lượng',
                            value: '${product.weight}g',
                          ),
                        if (product.volume != null)
                          _InfoRow(
                            label: 'Dung tích',
                            value: '${product.volume}ml',
                          ),
                        if (product.unit != null)
                          _InfoRow(label: 'Đơn vị', value: product.unit!),

                        if (product.ingredients != null) ...[
                          const SizedBox(height: 16),
                          Text(
                            'Thành phần',
                            style: AppTextStyles.labelLarge.copyWith(
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                          const SizedBox(height: 8),
                          Text(
                            product.ingredients!,
                            style: AppTextStyles.bodyMedium.copyWith(
                              color: AppColors.textSecondary,
                              height: 1.5,
                            ),
                          ),
                        ],

                        if (product.usage != null) ...[
                          const SizedBox(height: 16),
                          Text(
                            'Hướng dẫn sử dụng',
                            style: AppTextStyles.labelLarge.copyWith(
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                          const SizedBox(height: 8),
                          Text(
                            product.usage!,
                            style: AppTextStyles.bodyMedium.copyWith(
                              color: AppColors.textSecondary,
                              height: 1.5,
                            ),
                          ),
                        ],
                      ],
                    ),
                  ),
                ),

                // Description Section
                if (product.description != null) ...[
                  const SliverToBoxAdapter(child: SizedBox(height: 8)),
                  SliverToBoxAdapter(
                    child: Container(
                      color: AppColors.white,
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            'Mô tả sản phẩm',
                            style: AppTextStyles.h4.copyWith(
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                          const SizedBox(height: 12),
                          Text(
                            product.description!,
                            style: AppTextStyles.bodyMedium.copyWith(
                              color: AppColors.textSecondary,
                              height: 1.6,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                ],

                // Bottom Spacing
                const SliverToBoxAdapter(child: SizedBox(height: 100)),
              ],
            ),
          ),

          // Bottom Bar - Add to Cart
          _BottomBar(
            product: product,
            quantity: _quantity,
            onQuantityChanged: (newQuantity) {
              setState(() => _quantity = newQuantity);
            },
            onAddToCart: () async {
              try {
                await ref
                    .read(cartProvider.notifier)
                    .addToCart(
                      product.id,
                      _quantity,
                      price: product.displayPrice,
                      productName: product.name,
                      imageUrl: product.imageUrl,
                      stockQuantity: product.stockQuantity > 0
                          ? product.stockQuantity
                          : 999,
                    );
                if (mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(
                      content: Text(
                        'Đã thêm $_quantity ${product.name} vào giỏ hàng',
                      ),
                      duration: const Duration(seconds: 2),
                      action: SnackBarAction(
                        label: 'Xem giỏ',
                        onPressed: () {
                          Navigator.push(
                            context,
                            MaterialPageRoute(
                              builder: (context) => const CartScreen(),
                            ),
                          );
                        },
                      ),
                    ),
                  );
                }
              } catch (e) {
                if (mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(
                      content: Text('Lỗi: $e'),
                      backgroundColor: AppColors.error,
                    ),
                  );
                }
              }
            },
          ),
        ],
      ),
    );
  }

  Widget _buildBackButton(BuildContext context) {
    return Container(
      margin: const EdgeInsets.all(8),
      decoration: BoxDecoration(
        color: AppColors.white,
        shape: BoxShape.circle,
        boxShadow: [
          BoxShadow(color: AppColors.black.withOpacity(0.1), blurRadius: 8),
        ],
      ),
      child: IconButton(
        icon: const Icon(Icons.arrow_back, color: AppColors.black),
        onPressed: () => Navigator.pop(context),
      ),
    );
  }

  Widget _buildActionButton({
    required IconData icon,
    required VoidCallback onTap,
  }) {
    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 4, vertical: 8),
      decoration: BoxDecoration(
        color: AppColors.white,
        shape: BoxShape.circle,
        boxShadow: [
          BoxShadow(color: AppColors.black.withOpacity(0.1), blurRadius: 8),
        ],
      ),
      child: IconButton(
        icon: HugeIcon(icon: icon, color: AppColors.black, size: 20),
        onPressed: onTap,
      ),
    );
  }
}

/// Image Gallery Widget
class _ImageGallery extends StatelessWidget {
  final List<String> images;
  final int selectedIndex;
  final PageController pageController;
  final Function(int) onPageChanged;

  const _ImageGallery({
    required this.images,
    required this.selectedIndex,
    required this.pageController,
    required this.onPageChanged,
  });

  @override
  Widget build(BuildContext context) {
    if (images.isEmpty) {
      return Container(
        color: AppColors.veryLightGrey,
        child: Center(
          child: HugeIcon(
            icon: HugeIcons.strokeRoundedShoppingBag01,
            color: AppColors.iconInactive,
            size: 80,
          ),
        ),
      );
    }

    return Stack(
      children: [
        PageView.builder(
          controller: pageController,
          onPageChanged: onPageChanged,
          itemCount: images.length,
          itemBuilder: (context, index) {
            return Container(
              color: AppColors.white,
              child: Image.network(
                images[index],
                fit: BoxFit.contain,
                errorBuilder: (_, __, ___) => Container(
                  color: AppColors.veryLightGrey,
                  child: Center(
                    child: HugeIcon(
                      icon: HugeIcons.strokeRoundedShoppingBag01,
                      color: AppColors.iconInactive,
                      size: 80,
                    ),
                  ),
                ),
                loadingBuilder: (context, child, loadingProgress) {
                  if (loadingProgress == null) return child;
                  return Center(
                    child: CircularProgressIndicator(
                      value: loadingProgress.expectedTotalBytes != null
                          ? loadingProgress.cumulativeBytesLoaded /
                                loadingProgress.expectedTotalBytes!
                          : null,
                    ),
                  );
                },
              ),
            );
          },
        ),

        // Page Indicator
        if (images.length > 1)
          Positioned(
            bottom: 16,
            left: 0,
            right: 0,
            child: Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: List.generate(
                images.length,
                (index) => Container(
                  width: selectedIndex == index ? 24 : 8,
                  height: 8,
                  margin: const EdgeInsets.symmetric(horizontal: 4),
                  decoration: BoxDecoration(
                    color: selectedIndex == index
                        ? AppColors.primary
                        : AppColors.lightGrey,
                    borderRadius: BorderRadius.circular(4),
                  ),
                ),
              ),
            ),
          ),
      ],
    );
  }
}

/// Image Thumbnails Widget
class _ImageThumbnails extends StatelessWidget {
  final List<String> images;
  final int selectedIndex;
  final Function(int) onThumbnailTap;

  const _ImageThumbnails({
    required this.images,
    required this.selectedIndex,
    required this.onThumbnailTap,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 70,
      margin: const EdgeInsets.only(bottom: 16),
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        itemCount: images.length,
        itemBuilder: (context, index) {
          final isSelected = index == selectedIndex;
          return GestureDetector(
            onTap: () => onThumbnailTap(index),
            child: Container(
              width: 60,
              height: 60,
              margin: const EdgeInsets.only(right: 8),
              decoration: BoxDecoration(
                border: Border.all(
                  color: isSelected ? AppColors.primary : AppColors.border,
                  width: isSelected ? 2 : 1,
                ),
                borderRadius: BorderRadius.circular(8),
              ),
              child: ClipRRect(
                borderRadius: BorderRadius.circular(6),
                child: Image.network(
                  images[index],
                  fit: BoxFit.cover,
                  errorBuilder: (_, __, ___) => Container(
                    color: AppColors.veryLightGrey,
                    child: const Icon(
                      Icons.image,
                      color: AppColors.iconInactive,
                    ),
                  ),
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}

/// Price Section Widget
class _PriceSection extends StatelessWidget {
  final Product product;

  const _PriceSection({required this.product});

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.end,
      children: [
        Text(
          product.formattedDisplayPrice,
          style: const TextStyle(
            fontSize: 28,
            fontWeight: FontWeight.bold,
            color: AppColors.primary,
          ),
        ),
        if (product.hasDiscount) ...[
          const SizedBox(width: 12),
          Text(
            product.formattedPrice,
            style: AppTextStyles.bodyLarge.copyWith(
              color: AppColors.textSecondary,
              decoration: TextDecoration.lineThrough,
            ),
          ),
          const SizedBox(width: 8),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
            decoration: BoxDecoration(
              color: AppColors.error,
              borderRadius: BorderRadius.circular(4),
            ),
            child: Text(
              '-${product.discountPercent}%',
              style: AppTextStyles.bodySmall.copyWith(
                color: AppColors.white,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        ],
      ],
    );
  }
}

/// Stock Status Widget
class _StockStatus extends StatelessWidget {
  final Product product;

  const _StockStatus({required this.product});

  @override
  Widget build(BuildContext context) {
    final isInStock = product.isInStock;
    return Row(
      children: [
        Icon(
          isInStock ? Icons.check_circle : Icons.cancel,
          size: 20,
          color: isInStock ? AppColors.success : AppColors.error,
        ),
        const SizedBox(width: 8),
        Text(
          isInStock ? 'Còn hàng (${product.stockQuantity})' : 'Hết hàng',
          style: AppTextStyles.bodyMedium.copyWith(
            color: isInStock ? AppColors.success : AppColors.error,
            fontWeight: FontWeight.w500,
          ),
        ),
      ],
    );
  }
}

/// Info Row Widget
class _InfoRow extends StatelessWidget {
  final String label;
  final String value;

  const _InfoRow({required this.label, required this.value});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Row(
        children: [
          SizedBox(
            width: 120,
            child: Text(
              label,
              style: AppTextStyles.bodyMedium.copyWith(
                color: AppColors.textSecondary,
              ),
            ),
          ),
          Expanded(
            child: Text(
              value,
              style: AppTextStyles.bodyMedium.copyWith(
                fontWeight: FontWeight.w500,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

/// Bottom Bar Widget
class _BottomBar extends StatelessWidget {
  final Product product;
  final int quantity;
  final Function(int) onQuantityChanged;
  final VoidCallback onAddToCart;

  const _BottomBar({
    required this.product,
    required this.quantity,
    required this.onQuantityChanged,
    required this.onAddToCart,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.white,
        boxShadow: [
          BoxShadow(
            color: AppColors.black.withOpacity(0.1),
            blurRadius: 8,
            offset: const Offset(0, -2),
          ),
        ],
      ),
      child: SafeArea(
        child: Row(
          children: [
            // Quantity Selector
            Container(
              decoration: BoxDecoration(
                border: Border.all(color: AppColors.border),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Row(
                children: [
                  IconButton(
                    onPressed: quantity > 1
                        ? () => onQuantityChanged(quantity - 1)
                        : null,
                    icon: const Icon(Icons.remove),
                    iconSize: 20,
                    constraints: const BoxConstraints(
                      minWidth: 40,
                      minHeight: 40,
                    ),
                  ),
                  SizedBox(
                    width: 32,
                    child: Text(
                      quantity.toString(),
                      textAlign: TextAlign.center,
                      style: AppTextStyles.bodyLarge.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                  IconButton(
                    onPressed: quantity < product.stockQuantity
                        ? () => onQuantityChanged(quantity + 1)
                        : null,
                    icon: const Icon(Icons.add),
                    iconSize: 20,
                    constraints: const BoxConstraints(
                      minWidth: 40,
                      minHeight: 40,
                    ),
                  ),
                ],
              ),
            ),

            const SizedBox(width: 16),

            // Add to Cart Button
            Expanded(
              child: ElevatedButton.icon(
                onPressed: product.isInStock ? onAddToCart : null,
                icon: const Icon(Icons.add_shopping_cart),
                label: const Text('Thêm vào giỏ hàng'),
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.primary,
                  foregroundColor: AppColors.white,
                  padding: const EdgeInsets.symmetric(vertical: 14),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
