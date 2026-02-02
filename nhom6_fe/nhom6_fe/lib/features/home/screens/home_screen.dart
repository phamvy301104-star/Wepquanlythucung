import 'package:flutter/material.dart';
import 'package:hugeicons/hugeicons.dart';
import '../../../core/constants/constants.dart';
import '../../../core/utils/auth.dart';
import '../../../core/models/category_model.dart';
import '../../../core/services/product_service.dart';
import '../../../core/services/service_service.dart';
import '../../../core/services/category_service.dart';
import '../../../shared/widgets/widgets.dart';
import '../../face_analysis/face_analysis.dart';
import '../../hair_tryon/hair_tryon.dart';
import '../../ai_history/screens/ai_history_screen.dart';
import '../../product/product.dart';

/// Home Screen for UME App - Redesigned
/// Layout: Header → Search → Categories (horizontal) → Products Grid
class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  String _userName = 'Khách hàng';

  // Categories
  List<CategoryModel> _categories = [];
  bool _isCategoriesLoading = true;
  int? _selectedCategoryId;

  // Products
  List<Product> _products = [];
  bool _isProductsLoading = true;

  @override
  void initState() {
    super.initState();
    _loadUserName();
    _loadCategories();
    _loadProducts();
  }

  Future<void> _loadUserName() async {
    final name = await Auth.getUserDisplayName();
    if (mounted) {
      setState(() {
        _userName = name;
      });
    }
  }

  Future<void> _loadCategories() async {
    try {
      // Load all active categories from database (not hardcoded icons)
      final categories = await CategoryService().getCategories();
      if (mounted) {
        setState(() {
          _categories = categories;
          _isCategoriesLoading = false;
        });
        debugPrint(
          '[HomeScreen] Loaded ${categories.length} categories from database',
        );
      }
    } catch (e) {
      debugPrint('[HomeScreen] Error loading categories: $e');
      if (mounted) {
        setState(() => _isCategoriesLoading = false);
      }
    }
  }

  Future<void> _loadProducts({int? categoryId}) async {
    setState(() => _isProductsLoading = true);

    try {
      List<Product> products;

      if (categoryId != null) {
        // Load products by category
        final result = await CategoryService().getProductsByCategory(
          categoryId,
          pageSize: 20,
        );
        final items = result['items'] as List? ?? [];
        products = items.map((item) => Product.fromJson(item)).toList();
      } else {
        // Load all featured products
        products = await ProductService().getFeaturedProducts(limit: 20);
      }

      if (mounted) {
        setState(() {
          _products = products;
          _isProductsLoading = false;
        });
      }
    } catch (e) {
      debugPrint('[HomeScreen] Error loading products: $e');
      if (mounted) {
        setState(() => _isProductsLoading = false);
      }
    }
  }

  void _onCategoryTap(CategoryModel? category) {
    setState(() {
      _selectedCategoryId = category?.id;
    });
    _loadProducts(categoryId: category?.id);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: RefreshIndicator(
        onRefresh: () async {
          await Future.wait([
            _loadCategories(),
            _loadProducts(categoryId: _selectedCategoryId),
          ]);
        },
        child: CustomScrollView(
          slivers: [
            // Header
            SliverToBoxAdapter(child: CustomHeader(userName: _userName)),

            // Search Bar
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.symmetric(
                  horizontal: AppSizes.screenPaddingH,
                  vertical: AppSizes.paddingS,
                ),
                child: GestureDetector(
                  onTap: () {
                    // TODO: Navigate to search screen
                  },
                  child: Container(
                    height: 48,
                    decoration: BoxDecoration(
                      color: AppColors.white,
                      borderRadius: BorderRadius.circular(AppSizes.radiusL),
                      border: Border.all(color: AppColors.border),
                    ),
                    child: Row(
                      children: [
                        const SizedBox(width: AppSizes.paddingL),
                        HugeIcon(
                          icon: HugeIcons.strokeRoundedSearch01,
                          color: AppColors.iconInactive,
                          size: AppSizes.iconM,
                        ),
                        const SizedBox(width: AppSizes.paddingM),
                        Expanded(
                          child: Text(
                            'Tìm kiếm sản phẩm, dịch vụ...',
                            style: AppTextStyles.bodyMedium.copyWith(
                              color: AppColors.textHint,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ),

            // Banner Carousel
            const SliverToBoxAdapter(child: _BannerCarousel()),

            // AI Features Section
            SliverToBoxAdapter(
              child: SectionHeader(
                title: 'Tính năng AI',
                actionText: 'Lịch sử',
                onActionTap: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (context) => const AiHistoryScreen(),
                    ),
                  );
                },
              ),
            ),
            SliverToBoxAdapter(child: _AiFeaturesSection()),

            // Categories Section
            const SliverToBoxAdapter(
              child: SectionHeader(title: 'Danh mục sản phẩm'),
            ),
            SliverToBoxAdapter(
              child: CategorySlider(
                categories: _categories,
                selectedCategoryId: _selectedCategoryId,
                onCategoryTap: _onCategoryTap,
                isLoading: _isCategoriesLoading,
              ),
            ),

            // Products Section Header
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.only(top: AppSizes.spacingL),
                child: SectionHeader(
                  title: _selectedCategoryId == null
                      ? 'Sản phẩm nổi bật'
                      : 'Sản phẩm',
                  actionText: 'Xem tất cả',
                  onActionTap: () {
                    // TODO: Navigate to all products
                  },
                ),
              ),
            ),

            // Products Grid
            SliverToBoxAdapter(
              child: ProductGrid(
                products: _products,
                isLoading: _isProductsLoading,
                onProductTap: (product) {
                  // Navigate to product detail screen
                  Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (context) =>
                          ProductDetailScreen(product: product),
                    ),
                  );
                },
                onAddToCart: (product) {
                  // TODO: Add to cart
                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(
                      content: Text('Đã thêm ${product.name} vào giỏ hàng'),
                      duration: const Duration(seconds: 2),
                    ),
                  );
                },
              ),
            ),

            // Services Section
            const SliverToBoxAdapter(
              child: Padding(
                padding: EdgeInsets.only(top: AppSizes.spacingL),
                child: SectionHeader(
                  title: 'Dịch vụ',
                  actionText: 'Xem tất cả',
                ),
              ),
            ),
            const SliverToBoxAdapter(child: _ServicesGrid()),

            // Promotions Section
            const SliverToBoxAdapter(
              child: Padding(
                padding: EdgeInsets.only(top: AppSizes.spacingL),
                child: SectionHeader(title: 'Ưu đãi hôm nay'),
              ),
            ),
            const SliverToBoxAdapter(child: _PromotionBanner()),

            // Bottom spacing
            const SliverToBoxAdapter(
              child: SizedBox(height: AppSizes.spacingXXL),
            ),
          ],
        ),
      ),
    );
  }
}

/// Banner Carousel Widget
class _BannerCarousel extends StatefulWidget {
  const _BannerCarousel();

  @override
  State<_BannerCarousel> createState() => _BannerCarouselState();
}

class _BannerCarouselState extends State<_BannerCarousel> {
  int _currentIndex = 0;

  final List<Map<String, String>> banners = [
    {'title': 'Giảm 20% dịch vụ cắt tóc', 'subtitle': 'Áp dụng đến hết tháng'},
    {'title': 'Combo chăm sóc tóc', 'subtitle': 'Chỉ từ 199K'},
    {'title': 'Thành viên mới', 'subtitle': 'Giảm ngay 50K'},
  ];

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        SizedBox(
          height: AppSizes.bannerHeight,
          child: PageView.builder(
            itemCount: banners.length,
            onPageChanged: (index) {
              setState(() {
                _currentIndex = index;
              });
            },
            itemBuilder: (context, index) {
              return Container(
                margin: const EdgeInsets.symmetric(
                  horizontal: AppSizes.screenPaddingH,
                  vertical: AppSizes.paddingS,
                ),
                decoration: BoxDecoration(
                  gradient: const LinearGradient(
                    colors: [AppColors.black, AppColors.darkGrey],
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                  ),
                  borderRadius: BorderRadius.circular(AppSizes.radiusL),
                ),
                child: Stack(
                  children: [
                    Positioned(
                      left: AppSizes.paddingXL,
                      top: AppSizes.paddingXL,
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            banners[index]['title']!,
                            style: AppTextStyles.h3.copyWith(
                              color: AppColors.white,
                            ),
                          ),
                          const SizedBox(height: AppSizes.spacingS),
                          Text(
                            banners[index]['subtitle']!,
                            style: AppTextStyles.bodyMedium.copyWith(
                              color: AppColors.white.withOpacity(0.8),
                            ),
                          ),
                          const SizedBox(height: AppSizes.spacingL),
                          Container(
                            padding: const EdgeInsets.symmetric(
                              horizontal: AppSizes.paddingL,
                              vertical: AppSizes.paddingS,
                            ),
                            decoration: BoxDecoration(
                              color: AppColors.primary,
                              borderRadius: BorderRadius.circular(
                                AppSizes.radiusS,
                              ),
                            ),
                            child: Text(
                              'Đặt ngay',
                              style: AppTextStyles.buttonMedium,
                            ),
                          ),
                        ],
                      ),
                    ),
                    Positioned(
                      right: AppSizes.paddingL,
                      bottom: AppSizes.paddingL,
                      child: Icon(
                        Icons.content_cut,
                        color: AppColors.white.withOpacity(0.2),
                        size: 80,
                      ),
                    ),
                  ],
                ),
              );
            },
          ),
        ),
        // Indicators
        Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: List.generate(
            banners.length,
            (index) => Container(
              width: _currentIndex == index ? 24 : 8,
              height: 8,
              margin: const EdgeInsets.symmetric(horizontal: 4),
              decoration: BoxDecoration(
                color: _currentIndex == index
                    ? AppColors.black
                    : AppColors.lightGrey,
                borderRadius: BorderRadius.circular(4),
              ),
            ),
          ),
        ),
      ],
    );
  }
}

/// Services Grid Widget - Fetch from API
class _ServicesGrid extends StatefulWidget {
  const _ServicesGrid();

  @override
  State<_ServicesGrid> createState() => _ServicesGridState();
}

class _ServicesGridState extends State<_ServicesGrid> {
  List<ServiceModel> _services = [];
  bool _isLoading = true;

  // Default icons for services by name keyword
  IconData _getServiceIcon(String name) {
    final lowerName = name.toLowerCase();
    if (lowerName.contains('cắt') || lowerName.contains('cat')) {
      return Icons.content_cut;
    } else if (lowerName.contains('uốn') || lowerName.contains('uon')) {
      return Icons.water_drop;
    } else if (lowerName.contains('nhuộm') ||
        lowerName.contains('nhuom') ||
        lowerName.contains('màu')) {
      return Icons.brush;
    } else if (lowerName.contains('gội') ||
        lowerName.contains('goi') ||
        lowerName.contains('rửa')) {
      return Icons.water;
    } else if (lowerName.contains('cạo') ||
        lowerName.contains('cao') ||
        lowerName.contains('râu')) {
      return Icons.face;
    } else if (lowerName.contains('combo') || lowerName.contains('full')) {
      return Icons.stars;
    }
    return Icons.content_cut;
  }

  @override
  void initState() {
    super.initState();
    _loadServices();
  }

  Future<void> _loadServices() async {
    try {
      final services = await ServiceService().getFeaturedServices(limit: 6);
      if (mounted) {
        setState(() {
          _services = services;
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return Padding(
        padding: const EdgeInsets.symmetric(
          horizontal: AppSizes.screenPaddingH,
        ),
        child: GridView.builder(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
            crossAxisCount: 3,
            crossAxisSpacing: AppSizes.spacingM,
            mainAxisSpacing: AppSizes.spacingM,
            childAspectRatio: 1,
          ),
          itemCount: 6,
          itemBuilder: (context, index) {
            return Container(
              decoration: BoxDecoration(
                color: AppColors.veryLightGrey,
                borderRadius: BorderRadius.circular(AppSizes.radiusM),
              ),
            );
          },
        ),
      );
    }

    if (_services.isEmpty) {
      return Padding(
        padding: const EdgeInsets.symmetric(
          horizontal: AppSizes.screenPaddingH,
        ),
        child: Center(
          child: Text(
            'Chưa có dịch vụ nào',
            style: AppTextStyles.bodyMedium.copyWith(color: AppColors.textHint),
          ),
        ),
      );
    }

    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: AppSizes.screenPaddingH),
      child: GridView.builder(
        shrinkWrap: true,
        physics: const NeverScrollableScrollPhysics(),
        gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
          crossAxisCount: 3,
          crossAxisSpacing: AppSizes.spacingM,
          mainAxisSpacing: AppSizes.spacingM,
          childAspectRatio: 1,
        ),
        itemCount: _services.length,
        itemBuilder: (context, index) {
          final service = _services[index];
          return ServiceCard(
            icon: _getServiceIcon(service.name),
            title: service.name,
            onTap: () {
              // Navigate to service detail or booking
            },
          );
        },
      ),
    );
  }
}

/// Promotion Banner Widget
class _PromotionBanner extends StatelessWidget {
  const _PromotionBanner();

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.symmetric(horizontal: AppSizes.screenPaddingH),
      padding: const EdgeInsets.all(AppSizes.paddingL),
      decoration: BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadius.circular(AppSizes.radiusL),
        border: Border.all(color: AppColors.border),
      ),
      child: Row(
        children: [
          Container(
            width: 56,
            height: 56,
            decoration: BoxDecoration(
              color: AppColors.veryLightGrey,
              borderRadius: BorderRadius.circular(AppSizes.radiusM),
            ),
            child: Center(
              child: HugeIcon(
                icon: HugeIcons.strokeRoundedCalendar01,
                color: AppColors.iconActive,
                size: AppSizes.iconL,
              ),
            ),
          ),
          const SizedBox(width: AppSizes.spacingL),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Đặt lịch ngay hôm nay', style: AppTextStyles.labelLarge),
                const SizedBox(height: AppSizes.spacingXS),
                Text(
                  'Nhận ưu đãi giảm 10% cho lần đặt đầu tiên',
                  style: AppTextStyles.bodySmall,
                ),
              ],
            ),
          ),
          HugeIcon(
            icon: HugeIcons.strokeRoundedArrowRight01,
            color: AppColors.iconActive,
            size: AppSizes.iconM,
          ),
        ],
      ),
    );
  }
}

/// AI Features Section Widget - Modern Redesign
class _AiFeaturesSection extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: AppSizes.screenPaddingH),
      child: Row(
        children: [
          // Face Analysis Card
          Expanded(
            child: _AiFeatureCard(
              icon: HugeIcons.strokeRoundedFaceId,
              title: 'Scan Khuôn Mặt',
              subtitle: 'Phân tích khuôn mặt',
              gradient: const [Color(0xFF4A90E2), Color(0xFF357ABD)],
              onTap: () {
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => const FaceAnalysisScreen(),
                  ),
                );
              },
            ),
          ),
          const SizedBox(width: 12),
          // Hair Try-On Card
          Expanded(
            child: _AiFeatureCard(
              icon: HugeIcons.strokeRoundedMagicWand02,
              title: 'Thử Tóc Ảo',
              subtitle: 'Thử nghiệm kiểu tóc',
              gradient: const [Color(0xFFE94057), Color(0xFFF27121)],
              onTap: () {
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => const HairTryOnScreen(),
                  ),
                );
              },
            ),
          ),
        ],
      ),
    );
  }
}

/// AI Feature Card Widget - Simplified Modern Design
class _AiFeatureCard extends StatelessWidget {
  final IconData icon;
  final String title;
  final String subtitle;
  final List<Color> gradient;
  final VoidCallback onTap;

  const _AiFeatureCard({
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.gradient,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          gradient: LinearGradient(
            colors: gradient,
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
          ),
          borderRadius: BorderRadius.circular(AppSizes.radiusL),
          boxShadow: [
            BoxShadow(
              color: gradient.first.withOpacity(0.25),
              blurRadius: 8,
              offset: const Offset(0, 4),
            ),
          ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Simple icon without background
            HugeIcon(icon: icon, color: Colors.white, size: 32),
            const SizedBox(height: 12),
            Text(
              title,
              style: AppTextStyles.labelLarge.copyWith(
                color: Colors.white,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              subtitle,
              style: AppTextStyles.bodySmall.copyWith(
                color: Colors.white.withOpacity(0.9),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
