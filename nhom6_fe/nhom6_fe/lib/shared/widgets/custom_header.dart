import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:hugeicons/hugeicons.dart';
import '../../core/constants/constants.dart';
import '../../features/cart/cart_provider.dart';
import '../../features/cart/cart_screen.dart';

/// Custom Header Widget for UME App
/// Contains logo, greeting, cart icon, and notification icon
class CustomHeader extends ConsumerWidget {
  final String userName;
  final String? avatarUrl;
  final VoidCallback? onNotificationTap;
  final VoidCallback? onAvatarTap;
  final VoidCallback? onCartTap;

  const CustomHeader({
    super.key,
    required this.userName,
    this.avatarUrl,
    this.onNotificationTap,
    this.onAvatarTap,
    this.onCartTap,
  });

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    // Watch cart item count for badge
    final cartItemCount = ref.watch(cartItemCountProvider);

    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: AppSizes.screenPaddingH,
        vertical: AppSizes.paddingM,
      ),
      color: AppColors.white,
      child: SafeArea(
        bottom: false,
        child: Row(
          children: [
            // Avatar
            GestureDetector(
              onTap: onAvatarTap,
              child: Container(
                width: AppSizes.avatarM,
                height: AppSizes.avatarM,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: AppColors.veryLightGrey,
                  border: Border.all(color: AppColors.border, width: 1),
                ),
                child: avatarUrl != null
                    ? ClipOval(
                        child: Image.network(
                          avatarUrl!,
                          fit: BoxFit.cover,
                          errorBuilder: (_, __, ___) => HugeIcon(
                            icon: AppIcons.profile,
                            color: AppColors.iconInactive,
                            size: AppSizes.iconM,
                          ),
                        ),
                      )
                    : HugeIcon(
                        icon: AppIcons.profile,
                        color: AppColors.iconInactive,
                        size: AppSizes.iconM,
                      ),
              ),
            ),
            const SizedBox(width: AppSizes.spacingM),

            // Greeting
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(_getGreeting(), style: AppTextStyles.bodySmall),
                  Text(
                    userName,
                    style: AppTextStyles.h4,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                ],
              ),
            ),

            // Cart Icon with badge
            GestureDetector(
              onTap:
                  onCartTap ??
                  () {
                    Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (context) => const CartScreen(),
                      ),
                    );
                  },
              child: Container(
                width: AppSizes.avatarM,
                height: AppSizes.avatarM,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: AppColors.veryLightGrey,
                ),
                child: Stack(
                  children: [
                    Center(
                      child: HugeIcon(
                        icon: HugeIcons.strokeRoundedShoppingCart01,
                        color: AppColors.iconActive,
                        size: AppSizes.iconM,
                      ),
                    ),
                    // Badge
                    if (cartItemCount > 0)
                      Positioned(
                        right: 0,
                        top: 0,
                        child: Container(
                          padding: const EdgeInsets.all(4),
                          decoration: BoxDecoration(
                            color: AppColors.error,
                            shape: BoxShape.circle,
                          ),
                          constraints: const BoxConstraints(
                            minWidth: 18,
                            minHeight: 18,
                          ),
                          child: Text(
                            cartItemCount > 99
                                ? '99+'
                                : cartItemCount.toString(),
                            style: const TextStyle(
                              color: Colors.white,
                              fontSize: 10,
                              fontWeight: FontWeight.bold,
                            ),
                            textAlign: TextAlign.center,
                          ),
                        ),
                      ),
                  ],
                ),
              ),
            ),

            const SizedBox(width: AppSizes.spacingS),

            // Notification Icon
            GestureDetector(
              onTap: onNotificationTap,
              child: Container(
                width: AppSizes.avatarM,
                height: AppSizes.avatarM,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: AppColors.veryLightGrey,
                ),
                child: Center(
                  child: HugeIcon(
                    icon: AppIcons.notification,
                    color: AppColors.iconActive,
                    size: AppSizes.iconM,
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  String _getGreeting() {
    final hour = DateTime.now().hour;
    if (hour < 12) {
      return 'Chào buổi sáng 👋';
    } else if (hour < 18) {
      return 'Chào buổi chiều 👋';
    } else {
      return 'Chào buổi tối 👋';
    }
  }
}
