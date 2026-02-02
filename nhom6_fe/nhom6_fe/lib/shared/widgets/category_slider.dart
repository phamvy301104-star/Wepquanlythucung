import 'package:flutter/material.dart';
import '../../../core/constants/constants.dart';
import '../../../core/models/category_model.dart';

/// Horizontal scrollable category slider widget
/// Similar to PawVerse app's category section
class CategorySlider extends StatelessWidget {
  final List<CategoryModel> categories;
  final int? selectedCategoryId;
  final Function(CategoryModel?) onCategoryTap;
  final bool isLoading;

  const CategorySlider({
    super.key,
    required this.categories,
    this.selectedCategoryId,
    required this.onCategoryTap,
    this.isLoading = false,
  });

  @override
  Widget build(BuildContext context) {
    if (isLoading) {
      return SizedBox(
        height: 110,
        child: ListView.builder(
          scrollDirection: Axis.horizontal,
          padding: const EdgeInsets.symmetric(
            horizontal: AppSizes.screenPaddingH,
          ),
          itemCount: 5,
          itemBuilder: (context, index) {
            return Padding(
              padding: const EdgeInsets.only(right: 12),
              child: _buildShimmerItem(),
            );
          },
        ),
      );
    }

    if (categories.isEmpty) {
      return const SizedBox(
        height: 110,
        child: Center(child: Text('Chưa có danh mục nào')),
      );
    }

    return SizedBox(
      height: 110,
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.symmetric(
          horizontal: AppSizes.screenPaddingH,
        ),
        itemCount: categories.length + 1, // +1 for "All" option
        itemBuilder: (context, index) {
          if (index == 0) {
            // "All" category option
            final isSelected = selectedCategoryId == null;
            return Padding(
              padding: const EdgeInsets.only(right: 12),
              child: _CategoryItem(
                icon: '🏠',
                name: 'Tất cả',
                isSelected: isSelected,
                onTap: () => onCategoryTap(null),
              ),
            );
          }

          final category = categories[index - 1];
          final isSelected = selectedCategoryId == category.id;

          return Padding(
            padding: const EdgeInsets.only(right: 12),
            child: _CategoryItem(
              icon: category.displayIcon,
              name: category.name,
              imageUrl: category.imageUrl,
              isSelected: isSelected,
              onTap: () => onCategoryTap(category),
            ),
          );
        },
      ),
    );
  }

  Widget _buildShimmerItem() {
    return Container(
      width: 80,
      decoration: BoxDecoration(
        color: AppColors.veryLightGrey,
        borderRadius: BorderRadius.circular(AppSizes.radiusM),
      ),
    );
  }
}

/// Individual category item
class _CategoryItem extends StatelessWidget {
  final String icon;
  final String name;
  final String? imageUrl;
  final bool isSelected;
  final VoidCallback onTap;

  const _CategoryItem({
    required this.icon,
    required this.name,
    this.imageUrl,
    required this.isSelected,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 200),
        width: 80,
        padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 8),
        decoration: BoxDecoration(
          color: isSelected ? AppColors.primary : AppColors.white,
          borderRadius: BorderRadius.circular(AppSizes.radiusM),
          border: Border.all(
            color: isSelected ? AppColors.primary : AppColors.border,
            width: isSelected ? 2 : 1,
          ),
          boxShadow: isSelected
              ? [
                  BoxShadow(
                    color: AppColors.primary.withOpacity(0.3),
                    blurRadius: 8,
                    offset: const Offset(0, 4),
                  ),
                ]
              : null,
        ),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            // Prioritize Image over Icon
            _buildImageWidget(),
            const SizedBox(height: 8),
            // Name
            Text(
              name,
              style: AppTextStyles.bodySmall.copyWith(
                color: isSelected ? AppColors.white : AppColors.textPrimary,
                fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
              ),
              maxLines: 2,
              overflow: TextOverflow.ellipsis,
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildImageWidget() {
    // If imageUrl exists and not empty, show image
    if (imageUrl != null && imageUrl!.isNotEmpty) {
      return ClipRRect(
        borderRadius: BorderRadius.circular(AppSizes.radiusS),
        child: Image.network(
          imageUrl!,
          width: 40,
          height: 40,
          fit: BoxFit.cover,
          loadingBuilder: (context, child, loadingProgress) {
            if (loadingProgress == null) return child;
            return Container(
              width: 40,
              height: 40,
              decoration: BoxDecoration(
                color: AppColors.veryLightGrey,
                borderRadius: BorderRadius.circular(AppSizes.radiusS),
              ),
              child: Center(
                child: SizedBox(
                  width: 20,
                  height: 20,
                  child: CircularProgressIndicator(
                    strokeWidth: 2,
                    valueColor: AlwaysStoppedAnimation<Color>(
                      isSelected ? AppColors.white : AppColors.primary,
                    ),
                  ),
                ),
              ),
            );
          },
          errorBuilder: (context, error, stackTrace) {
            // If image fails to load, fallback to icon
            return _buildIconWidget();
          },
        ),
      );
    }
    // No imageUrl, show icon
    return _buildIconWidget();
  }

  Widget _buildIconWidget() {
    return Container(
      width: 40,
      height: 40,
      decoration: BoxDecoration(
        color: isSelected
            ? AppColors.white.withOpacity(0.2)
            : AppColors.veryLightGrey,
        borderRadius: BorderRadius.circular(AppSizes.radiusS),
      ),
      child: Center(child: Text(icon, style: const TextStyle(fontSize: 24))),
    );
  }
}
