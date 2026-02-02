import 'package:flutter/material.dart';
import 'package:hugeicons/hugeicons.dart';
import '../../core/constants/constants.dart';

/// Custom Bottom Navigation Bar for UME App
/// Floating center button design with smooth animations
class BottomNavBar extends StatelessWidget {
  final int currentIndex;
  final Function(int) onTap;

  const BottomNavBar({
    super.key,
    required this.currentIndex,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return Stack(
      clipBehavior: Clip.none,
      alignment: Alignment.bottomCenter,
      children: [
        // Bottom Navigation Bar
        Container(
          height: AppSizes.bottomNavHeight,
          decoration: BoxDecoration(
            color: AppColors.white,
            boxShadow: [
              BoxShadow(
                color: AppColors.shadow,
                blurRadius: 10,
                offset: const Offset(0, -2),
              ),
            ],
          ),
          child: SafeArea(
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceAround,
              children: [
                _NavItem(
                  icon: AppIcons.home,
                  label: 'Trang chủ',
                  isSelected: currentIndex == 0,
                  onTap: () => onTap(0),
                ),
                _NavItem(
                  icon: AppIcons.services,
                  label: 'Dịch vụ',
                  isSelected: currentIndex == 1,
                  onTap: () => onTap(1),
                ),
                // Spacer for floating button
                const SizedBox(width: 64),
                _NavItem(
                  icon: AppIcons.orders,
                  label: 'Đơn hàng',
                  isSelected: currentIndex == 3,
                  onTap: () => onTap(3),
                ),
                _NavItem(
                  icon: AppIcons.profile,
                  label: 'Cá nhân',
                  isSelected: currentIndex == 4,
                  onTap: () => onTap(4),
                ),
              ],
            ),
          ),
        ),

        // Floating AI Chatbot Button
        Positioned(
          top: -25,
          child: GestureDetector(
            onTap: () => onTap(2),
            child: Container(
              width: 64,
              height: 64,
              decoration: BoxDecoration(
                gradient: currentIndex == 2
                    ? LinearGradient(
                        colors: [AppColors.primary, AppColors.primaryDark],
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                      )
                    : LinearGradient(
                        colors: [AppColors.black, AppColors.darkGrey],
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                      ),
                shape: BoxShape.circle,
                boxShadow: [
                  BoxShadow(
                    color: currentIndex == 2
                        ? AppColors.primary.withValues(alpha: 0.4)
                        : AppColors.shadow,
                    blurRadius: 15,
                    offset: const Offset(0, 4),
                  ),
                ],
              ),
              child: AnimatedScale(
                scale: currentIndex == 2 ? 1.1 : 1.0,
                duration: const Duration(milliseconds: 200),
                curve: Curves.easeInOut,
                child: Center(
                  child: HugeIcon(
                    icon: AppIcons.chat,
                    color: AppColors.white,
                    size: AppSizes.iconL,
                  ),
                ),
              ),
            ),
          ),
        ),
      ],
    );
  }
}

class _NavItem extends StatelessWidget {
  final IconData icon;
  final String label;
  final bool isSelected;
  final VoidCallback onTap;

  const _NavItem({
    required this.icon,
    required this.label,
    required this.isSelected,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      behavior: HitTestBehavior.opaque,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 200),
        curve: Curves.easeInOut,
        width: 64,
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            AnimatedScale(
              scale: isSelected ? 1.1 : 1.0,
              duration: const Duration(milliseconds: 200),
              curve: Curves.easeInOut,
              child: HugeIcon(
                icon: icon,
                color: isSelected
                    ? AppColors.iconActive
                    : AppColors.iconInactive,
                size: AppSizes.iconM,
              ),
            ),
            const SizedBox(height: AppSizes.spacingXS),
            AnimatedDefaultTextStyle(
              duration: const Duration(milliseconds: 200),
              style: isSelected
                  ? AppTextStyles.navLabelActive
                  : AppTextStyles.navLabel,
              child: Text(label, maxLines: 1, overflow: TextOverflow.ellipsis),
            ),
          ],
        ),
      ),
    );
  }
}
