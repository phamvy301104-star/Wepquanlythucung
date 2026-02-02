import 'package:flutter/material.dart';
import 'package:hugeicons/hugeicons.dart';
import '../../../core/constants/constants.dart';
import '../../../core/services/service_service.dart';
import '../../../core/utils/html_utils.dart';
import '../../booking/booking_screen.dart';

/// Services Screen for UME App
/// Lists all available services with details
class ServicesScreen extends StatefulWidget {
  const ServicesScreen({super.key});

  @override
  State<ServicesScreen> createState() => _ServicesScreenState();
}

class _ServicesScreenState extends State<ServicesScreen> {
  List<ServiceModel> _services = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadServices();
  }

  Future<void> _loadServices() async {
    setState(() => _isLoading = true);
    try {
      final services = await ServiceService().getServices(limit: 50);
      if (mounted) {
        setState(() {
          _services = services;
          _isLoading = false;
        });
      }
    } catch (e) {
      debugPrint('[ServicesScreen] Error: $e');
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  IconData _getServiceIcon(String serviceName) {
    final name = serviceName.toLowerCase();
    if (name.contains('cắt')) return AppIcons.haircut;
    if (name.contains('nhuộm')) return AppIcons.hairColor;
    if (name.contains('uốn') || name.contains('duỗi')) return AppIcons.hairPerm;
    if (name.contains('gội')) return AppIcons.hairWash;
    if (name.contains('cạo')) return AppIcons.shave;
    if (name.contains('massage')) return AppIcons.hairWash; // spa not available
    if (name.contains('da')) return AppIcons.hairWash; // skinCare not available
    return AppIcons.styling;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(
          children: [
            // Header
            Container(
              padding: const EdgeInsets.all(AppSizes.screenPaddingH),
              color: AppColors.white,
              child: Row(
                children: [
                  Text('Dịch vụ', style: AppTextStyles.h2),
                  const Spacer(),
                  Text(
                    '${_services.length} dịch vụ',
                    style: AppTextStyles.bodyMedium.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),
                ],
              ),
            ),

            // Services List
            Expanded(
              child: _isLoading
                  ? const Center(child: CircularProgressIndicator())
                  : _services.isEmpty
                  ? Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          const Icon(
                            Icons.spa_outlined,
                            size: 64,
                            color: Colors.grey,
                          ),
                          const SizedBox(height: 16),
                          Text(
                            'Chưa có dịch vụ nào',
                            style: AppTextStyles.bodyLarge,
                          ),
                        ],
                      ),
                    )
                  : RefreshIndicator(
                      onRefresh: _loadServices,
                      child: ListView.builder(
                        padding: const EdgeInsets.all(AppSizes.screenPaddingH),
                        itemCount: _services.length,
                        itemBuilder: (context, index) {
                          final service = _services[index];
                          return _ServiceListItem(
                            icon: _getServiceIcon(service.name),
                            title: service.name,
                            description: HtmlUtils.stripHtmlPreview(
                              service.description ?? 'Dịch vụ chuyên nghiệp',
                              maxLength: 80,
                            ),
                            price: service.formattedPrice,
                            duration: '${service.duration} phút',
                            imageUrl: service.imageUrl,
                            onTap: () {
                              // Navigate to booking screen
                              Navigator.push(
                                context,
                                MaterialPageRoute(
                                  builder: (context) => const BookingScreen(),
                                ),
                              );
                            },
                          );
                        },
                      ),
                    ),
            ),
          ],
        ),
      ),
    );
  }
}

class _ServiceListItem extends StatelessWidget {
  final IconData icon;
  final String title;
  final String description;
  final String price;
  final String duration;
  final String? imageUrl;
  final VoidCallback? onTap;

  const _ServiceListItem({
    required this.icon,
    required this.title,
    required this.description,
    required this.price,
    required this.duration,
    this.imageUrl,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        margin: const EdgeInsets.only(bottom: AppSizes.spacingM),
        padding: const EdgeInsets.all(AppSizes.paddingL),
        decoration: BoxDecoration(
          color: AppColors.white,
          borderRadius: BorderRadius.circular(AppSizes.radiusM),
          border: Border.all(color: AppColors.borderLight),
        ),
        child: Row(
          children: [
            // Icon or Image
            Container(
              width: 56,
              height: 56,
              decoration: BoxDecoration(
                color: AppColors.veryLightGrey,
                borderRadius: BorderRadius.circular(AppSizes.radiusM),
              ),
              child: imageUrl != null && imageUrl!.isNotEmpty
                  ? ClipRRect(
                      borderRadius: BorderRadius.circular(AppSizes.radiusM),
                      child: Image.network(
                        imageUrl!,
                        fit: BoxFit.cover,
                        errorBuilder: (_, __, ___) => Center(
                          child: HugeIcon(
                            icon: icon,
                            color: AppColors.iconActive,
                            size: AppSizes.iconL,
                          ),
                        ),
                      ),
                    )
                  : Center(
                      child: HugeIcon(
                        icon: icon,
                        color: AppColors.iconActive,
                        size: AppSizes.iconL,
                      ),
                    ),
            ),
            const SizedBox(width: AppSizes.spacingL),

            // Content
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(title, style: AppTextStyles.labelLarge),
                  const SizedBox(height: AppSizes.spacingXS),
                  Text(
                    description,
                    style: AppTextStyles.bodySmall,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: AppSizes.spacingS),
                  Row(
                    children: [
                      HugeIcon(
                        icon: AppIcons.clock,
                        color: AppColors.iconInactive,
                        size: AppSizes.iconXS,
                      ),
                      const SizedBox(width: 4),
                      Text(duration, style: AppTextStyles.labelSmall),
                    ],
                  ),
                ],
              ),
            ),

            // Price
            Column(
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                Text(price, style: AppTextStyles.priceMedium),
                const SizedBox(height: AppSizes.spacingS),
                Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: AppSizes.paddingM,
                    vertical: AppSizes.paddingXS,
                  ),
                  decoration: BoxDecoration(
                    color: AppColors.black,
                    borderRadius: BorderRadius.circular(AppSizes.radiusS),
                  ),
                  child: Text(
                    'Đặt',
                    style: AppTextStyles.labelSmall.copyWith(
                      color: AppColors.white,
                    ),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
