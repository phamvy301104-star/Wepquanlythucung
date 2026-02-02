import 'package:flutter/material.dart';
import 'package:hugeicons/hugeicons.dart';
import '../../../core/constants/constants.dart';
import '../../../core/utils/auth.dart';
import '../../../shared/widgets/custom_snackbar.dart';
import '../../../features/auth/login_screen.dart';
import '../../../features/orders/screens/product_order_history_screen.dart';

/// Profile Screen for UME App
/// User account and settings
class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  String _userName = 'Khách hàng';
  String _userPhone = '';
  String _userEmail = '';
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadUserInfo();
  }

  Future<void> _loadUserInfo() async {
    final userInfo = await Auth.getUserInfo();
    if (mounted) {
      setState(() {
        if (userInfo != null) {
          _userName =
              userInfo['userName'] ?? userInfo['fullName'] ?? 'Khách hàng';
          _userPhone = userInfo['phoneNumber'] ?? '';
          _userEmail = userInfo['email'] ?? '';
        }
        _isLoading = false;
      });
    }
  }

  Future<void> _handleLogout() async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: Row(
          children: [
            Container(
              padding: const EdgeInsets.all(8),
              decoration: BoxDecoration(
                color: AppColors.error.withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(8),
              ),
              child: const Icon(Icons.logout, color: AppColors.error, size: 24),
            ),
            const SizedBox(width: 12),
            const Text('Đăng xuất'),
          ],
        ),
        content: const Text('Bạn có chắc chắn muốn đăng xuất khỏi tài khoản?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: Text(
              'Hủy',
              style: TextStyle(color: AppColors.textSecondary),
            ),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(context, true),
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.error,
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(8),
              ),
            ),
            child: const Text('Đăng xuất'),
          ),
        ],
      ),
    );

    if (confirmed == true && mounted) {
      await Auth.logout();
      if (mounted) {
        CustomSnackBar.showSuccess(context, 'Đã đăng xuất thành công!');
        Navigator.pushAndRemoveUntil(
          context,
          MaterialPageRoute(builder: (context) => const LoginScreen()),
          (route) => false,
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: SingleChildScrollView(
          child: Column(
            children: [
              // Header
              Container(
                padding: const EdgeInsets.all(AppSizes.screenPaddingH),
                color: AppColors.white,
                child: Row(
                  children: [
                    Text('Cá nhân', style: AppTextStyles.h2),
                    const Spacer(),
                    GestureDetector(
                      child: HugeIcon(
                        icon: AppIcons.settings,
                        color: AppColors.iconActive,
                        size: AppSizes.iconM,
                      ),
                    ),
                  ],
                ),
              ),

              // Profile Card
              Container(
                margin: const EdgeInsets.all(AppSizes.screenPaddingH),
                padding: const EdgeInsets.all(AppSizes.paddingXL),
                decoration: BoxDecoration(
                  color: AppColors.white,
                  borderRadius: BorderRadius.circular(AppSizes.radiusL),
                  border: Border.all(color: AppColors.borderLight),
                ),
                child: Row(
                  children: [
                    Container(
                      width: AppSizes.avatarXL,
                      height: AppSizes.avatarXL,
                      decoration: BoxDecoration(
                        shape: BoxShape.circle,
                        color: AppColors.primary.withValues(alpha: 0.1),
                        border: Border.all(color: AppColors.primary, width: 2),
                      ),
                      child: Center(
                        child: _isLoading
                            ? const CircularProgressIndicator(strokeWidth: 2)
                            : Text(
                                _getInitials(_userName),
                                style: AppTextStyles.h2.copyWith(
                                  color: AppColors.primary,
                                ),
                              ),
                      ),
                    ),
                    const SizedBox(width: AppSizes.spacingL),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            _isLoading ? 'Đang tải...' : _userName,
                            style: AppTextStyles.h3,
                          ),
                          const SizedBox(height: AppSizes.spacingXS),
                          if (_userEmail.isNotEmpty)
                            Text(
                              _userEmail,
                              style: AppTextStyles.bodyMedium.copyWith(
                                color: AppColors.textSecondary,
                              ),
                            ),
                          if (_userPhone.isNotEmpty)
                            Text(
                              _userPhone,
                              style: AppTextStyles.bodyMedium.copyWith(
                                color: AppColors.textSecondary,
                              ),
                            ),
                          const SizedBox(height: AppSizes.spacingS),
                          Container(
                            padding: const EdgeInsets.symmetric(
                              horizontal: AppSizes.paddingS,
                              vertical: AppSizes.paddingXS,
                            ),
                            decoration: BoxDecoration(
                              color: AppColors.primary.withValues(alpha: 0.1),
                              borderRadius: BorderRadius.circular(
                                AppSizes.radiusXS,
                              ),
                            ),
                            child: Text(
                              'Thành viên Vàng',
                              style: AppTextStyles.labelSmall.copyWith(
                                color: AppColors.primary,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                    HugeIcon(
                      icon: AppIcons.edit,
                      color: AppColors.iconActive,
                      size: AppSizes.iconM,
                    ),
                  ],
                ),
              ),

              // Stats
              Padding(
                padding: const EdgeInsets.symmetric(
                  horizontal: AppSizes.screenPaddingH,
                ),
                child: Row(
                  children: [
                    _StatCard(
                      icon: AppIcons.orders,
                      value: '12',
                      label: 'Đơn hàng',
                    ),
                    const SizedBox(width: AppSizes.spacingM),
                    _StatCard(
                      icon: AppIcons.wallet,
                      value: '150K',
                      label: 'Điểm thưởng',
                    ),
                    const SizedBox(width: AppSizes.spacingM),
                    _StatCard(
                      icon: AppIcons.favorite,
                      value: '5',
                      label: 'Yêu thích',
                    ),
                  ],
                ),
              ),

              const SizedBox(height: AppSizes.spacingXL),

              // Menu Items
              Container(
                margin: const EdgeInsets.symmetric(
                  horizontal: AppSizes.screenPaddingH,
                ),
                decoration: BoxDecoration(
                  color: AppColors.white,
                  borderRadius: BorderRadius.circular(AppSizes.radiusL),
                  border: Border.all(color: AppColors.borderLight),
                ),
                child: Column(
                  children: [
                    _MenuItem(
                      icon: AppIcons.profile,
                      title: 'Thông tin cá nhân',
                      onTap: () {},
                    ),
                    _MenuDivider(),
                    _MenuItem(
                      icon: AppIcons.location,
                      title: 'Địa chỉ',
                      onTap: () {},
                    ),
                    _MenuDivider(),
                    _MenuItem(
                      icon: AppIcons.orders,
                      title: 'Đơn hàng của tôi',
                      onTap: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (context) =>
                                const ProductOrderHistoryScreen(),
                          ),
                        );
                      },
                    ),
                    _MenuDivider(),
                    _MenuItem(
                      icon: AppIcons.payment,
                      title: 'Phương thức thanh toán',
                      onTap: () {},
                    ),
                    _MenuDivider(),
                    _MenuItem(
                      icon: AppIcons.notification,
                      title: 'Thông báo',
                      onTap: () {},
                    ),
                  ],
                ),
              ),

              const SizedBox(height: AppSizes.spacingL),

              Container(
                margin: const EdgeInsets.symmetric(
                  horizontal: AppSizes.screenPaddingH,
                ),
                decoration: BoxDecoration(
                  color: AppColors.white,
                  borderRadius: BorderRadius.circular(AppSizes.radiusL),
                  border: Border.all(color: AppColors.borderLight),
                ),
                child: Column(
                  children: [
                    _MenuItem(
                      icon: AppIcons.help,
                      title: 'Trợ giúp & Hỗ trợ',
                      onTap: () {},
                    ),
                    _MenuDivider(),
                    _MenuItem(
                      icon: AppIcons.info,
                      title: 'Về ứng dụng',
                      onTap: () {},
                    ),
                  ],
                ),
              ),

              const SizedBox(height: AppSizes.spacingL),

              // Logout
              Container(
                margin: const EdgeInsets.symmetric(
                  horizontal: AppSizes.screenPaddingH,
                ),
                decoration: BoxDecoration(
                  color: AppColors.white,
                  borderRadius: BorderRadius.circular(AppSizes.radiusL),
                  border: Border.all(color: AppColors.borderLight),
                ),
                child: _MenuItem(
                  icon: AppIcons.logout,
                  title: 'Đăng xuất',
                  onTap: _handleLogout,
                  isDestructive: true,
                ),
              ),

              const SizedBox(height: AppSizes.spacingXXL),
            ],
          ),
        ),
      ),
    );
  }

  String _getInitials(String name) {
    if (name.isEmpty) return 'U';
    final parts = name.split(' ').where((s) => s.isNotEmpty).toList();
    if (parts.length >= 2) {
      return '${parts.first[0]}${parts.last[0]}'.toUpperCase();
    }
    return name.length >= 2
        ? name.substring(0, 2).toUpperCase()
        : name.toUpperCase();
  }
}

class _StatCard extends StatelessWidget {
  final IconData icon;
  final String value;
  final String label;

  const _StatCard({
    required this.icon,
    required this.value,
    required this.label,
  });

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: Container(
        padding: const EdgeInsets.all(AppSizes.paddingL),
        decoration: BoxDecoration(
          color: AppColors.white,
          borderRadius: BorderRadius.circular(AppSizes.radiusM),
          border: Border.all(color: AppColors.borderLight),
        ),
        child: Column(
          children: [
            HugeIcon(
              icon: icon,
              color: AppColors.iconActive,
              size: AppSizes.iconM,
            ),
            const SizedBox(height: AppSizes.spacingS),
            Text(value, style: AppTextStyles.h3),
            Text(label, style: AppTextStyles.labelSmall),
          ],
        ),
      ),
    );
  }
}

class _MenuItem extends StatelessWidget {
  final IconData icon;
  final String title;
  final VoidCallback onTap;
  final bool isDestructive;

  const _MenuItem({
    required this.icon,
    required this.title,
    required this.onTap,
    this.isDestructive = false,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      behavior: HitTestBehavior.opaque,
      child: Padding(
        padding: const EdgeInsets.all(AppSizes.paddingL),
        child: Row(
          children: [
            HugeIcon(
              icon: icon,
              color: isDestructive ? AppColors.error : AppColors.iconActive,
              size: AppSizes.iconM,
            ),
            const SizedBox(width: AppSizes.spacingL),
            Expanded(
              child: Text(
                title,
                style: AppTextStyles.bodyLarge.copyWith(
                  color: isDestructive
                      ? AppColors.error
                      : AppColors.textPrimary,
                ),
              ),
            ),
            HugeIcon(
              icon: AppIcons.arrowRight,
              color: AppColors.iconInactive,
              size: AppSizes.iconS,
            ),
          ],
        ),
      ),
    );
  }
}

class _MenuDivider extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return const Divider(
      height: 1,
      indent: AppSizes.paddingL + AppSizes.iconM + AppSizes.spacingL,
      endIndent: AppSizes.paddingL,
      color: AppColors.borderLight,
    );
  }
}
