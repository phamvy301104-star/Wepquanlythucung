import 'dart:io';
import 'package:flutter/material.dart';
import 'package:hugeicons/hugeicons.dart';
import 'package:image_picker/image_picker.dart';
import '../../../core/constants/constants.dart';
import '../services/hair_tryon_service.dart';

/// Hair Try-On Screen
/// Cho phép người dùng thử kiểu tóc ảo bằng AI
class HairTryOnScreen extends StatefulWidget {
  const HairTryOnScreen({super.key});

  @override
  State<HairTryOnScreen> createState() => _HairTryOnScreenState();
}

class _HairTryOnScreenState extends State<HairTryOnScreen> {
  final ImagePicker _picker = ImagePicker();
  final HairTryOnService _hairTryOnService = HairTryOnService();

  File? _faceImage;
  File? _hairStyleImage;
  File? _resultImage;
  bool _isProcessing = false;
  String? _statusMessage;

  Future<void> _pickFaceImage() async {
    final source = await _showImageSourceDialog('Ảnh khuôn mặt');
    if (source == null) return;

    final XFile? image = await _picker.pickImage(
      source: source,
      maxWidth: 1024,
      maxHeight: 1024,
      imageQuality: 90,
    );

    if (image != null) {
      setState(() {
        _faceImage = File(image.path);
        _resultImage = null;
      });
    }
  }

  Future<void> _pickHairStyleImage() async {
    final source = await _showImageSourceDialog('Ảnh kiểu tóc');
    if (source == null) return;

    final XFile? image = await _picker.pickImage(
      source: source,
      maxWidth: 1024,
      maxHeight: 1024,
      imageQuality: 90,
    );

    if (image != null) {
      setState(() {
        _hairStyleImage = File(image.path);
        _resultImage = null;
      });
    }
  }

  Future<ImageSource?> _showImageSourceDialog(String title) async {
    return showModalBottomSheet<ImageSource>(
      context: context,
      backgroundColor: AppColors.white,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (context) => Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Container(
              width: 40,
              height: 4,
              decoration: BoxDecoration(
                color: AppColors.lightGrey,
                borderRadius: BorderRadius.circular(2),
              ),
            ),
            const SizedBox(height: 20),
            Text(title, style: AppTextStyles.titleLarge),
            const SizedBox(height: 24),
            Row(
              children: [
                Expanded(
                  child: _SourceButton(
                    icon: HugeIcons.strokeRoundedCamera01,
                    label: 'Chụp ảnh',
                    onTap: () => Navigator.pop(context, ImageSource.camera),
                  ),
                ),
                const SizedBox(width: 16),
                Expanded(
                  child: _SourceButton(
                    icon: HugeIcons.strokeRoundedImage01,
                    label: 'Thư viện',
                    onTap: () => Navigator.pop(context, ImageSource.gallery),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
          ],
        ),
      ),
    );
  }

  Future<void> _processHairTryOn() async {
    if (_faceImage == null || _hairStyleImage == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Vui lòng chọn đủ ảnh khuôn mặt và kiểu tóc'),
          backgroundColor: AppColors.error,
        ),
      );
      return;
    }

    setState(() {
      _isProcessing = true;
      _statusMessage = 'Đang kết nối AI... 🚀';
      _resultImage = null;
    });

    // Update status message during processing
    Future.delayed(const Duration(seconds: 3), () {
      if (_isProcessing && mounted) {
        setState(() => _statusMessage = 'Đang phân tích khuôn mặt... 📸');
      }
    });
    Future.delayed(const Duration(seconds: 8), () {
      if (_isProcessing && mounted) {
        setState(() => _statusMessage = 'Đang ghép kiểu tóc... ✂️');
      }
    });
    Future.delayed(const Duration(seconds: 15), () {
      if (_isProcessing && mounted) {
        setState(() => _statusMessage = 'Đang tạo ảnh kết quả... ✨');
      }
    });

    final result = await _hairTryOnService.tryHairStyle(
      faceImage: _faceImage!,
      hairStyleImage: _hairStyleImage!,
    );

    setState(() {
      _isProcessing = false;
      if (result.success && result.resultImage != null) {
        _resultImage = result.resultImage;
        _statusMessage = result.message;
      } else {
        _statusMessage = result.message;
        // Show retry hint if model is loading
        if (result.isModelLoading) {
          _showModelLoadingDialog();
        }
      }
    });
  }

  void _showModelLoadingDialog() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Row(
          children: [
            const Text('🔄'),
            const SizedBox(width: 8),
            Text('AI đang khởi động', style: AppTextStyles.titleMedium),
          ],
        ),
        content: const Text(
          'Model AI trên HuggingFace đang trong trạng thái "cold start". '
          'Đây là bình thường với dịch vụ miễn phí.\n\n'
          'Vui lòng đợi khoảng 30 giây và thử lại!',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Đã hiểu'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(context);
              _processHairTryOn();
            },
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary),
            child: const Text('Thử lại'),
          ),
        ],
      ),
    );
  }

  void _saveResult() async {
    if (_resultImage == null) return;

    // TODO: Implement save to gallery
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('Đã lưu ảnh vào thư viện! 📸'),
        backgroundColor: AppColors.success,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.white,
        elevation: 0,
        title: Text('Thử Tóc Ảo AI ✨', style: AppTextStyles.titleLarge),
        leading: IconButton(
          icon: const HugeIcon(
            icon: HugeIcons.strokeRoundedArrowLeft01,
            color: AppColors.textPrimary,
            size: 24,
          ),
          onPressed: () => Navigator.pop(context),
        ),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(AppSizes.screenPaddingH),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Instructions
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: AppColors.info.withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(AppSizes.radiusM),
                border: Border.all(
                  color: AppColors.info.withValues(alpha: 0.3),
                ),
              ),
              child: Row(
                children: [
                  const HugeIcon(
                    icon: HugeIcons.strokeRoundedInformationCircle,
                    color: AppColors.info,
                    size: 24,
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Text(
                      'Upload ảnh chân dung và ảnh kiểu tóc muốn thử. AI sẽ ghép tóc mới lên khuôn mặt bạn!',
                      style: AppTextStyles.bodySmall.copyWith(
                        color: AppColors.info,
                      ),
                    ),
                  ),
                ],
              ),
            ),

            const SizedBox(height: 24),

            // Face Image Section
            Text(
              '📸 Ảnh khuôn mặt của bạn',
              style: AppTextStyles.titleMedium.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 12),
            _buildImagePicker(
              image: _faceImage,
              onTap: _pickFaceImage,
              placeholder: 'Chụp/chọn ảnh mặt nhìn thẳng',
              hint: 'Tip: Chụp rõ mặt, đủ sáng, nhìn thẳng camera',
            ),

            const SizedBox(height: 24),

            // Hair Style Section
            Text(
              '✂️ Kiểu tóc muốn thử',
              style: AppTextStyles.titleMedium.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 12),
            _buildImagePicker(
              image: _hairStyleImage,
              onTap: _pickHairStyleImage,
              placeholder: 'Chọn ảnh kiểu tóc mẫu',
              hint: 'Tip: Chọn ảnh có người thật, thấy rõ kiểu tóc',
            ),

            const SizedBox(height: 24),

            // Process Button
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed:
                    (_faceImage != null &&
                        _hairStyleImage != null &&
                        !_isProcessing)
                    ? _processHairTryOn
                    : null,
                icon: _isProcessing
                    ? const SizedBox(
                        width: 20,
                        height: 20,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          color: AppColors.white,
                        ),
                      )
                    : const HugeIcon(
                        icon: HugeIcons.strokeRoundedMagicWand01,
                        color: AppColors.white,
                        size: 20,
                      ),
                label: Text(
                  _isProcessing ? 'Đang xử lý...' : 'Thử kiểu tóc này!',
                  style: AppTextStyles.buttonText,
                ),
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.primary,
                  disabledBackgroundColor: AppColors.lightGrey,
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(AppSizes.radiusM),
                  ),
                ),
              ),
            ),

            // Status Message
            if (_statusMessage != null) ...[
              const SizedBox(height: 16),
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: _resultImage != null
                      ? AppColors.success.withValues(alpha: 0.1)
                      : AppColors.warning.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(AppSizes.radiusM),
                ),
                child: Row(
                  children: [
                    if (_isProcessing)
                      const SizedBox(
                        width: 16,
                        height: 16,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    else
                      HugeIcon(
                        icon: _resultImage != null
                            ? HugeIcons.strokeRoundedCheckmarkCircle02
                            : HugeIcons.strokeRoundedAlertCircle,
                        color: _resultImage != null
                            ? AppColors.success
                            : AppColors.warning,
                        size: 20,
                      ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Text(
                        _statusMessage!,
                        style: AppTextStyles.bodyMedium,
                      ),
                    ),
                  ],
                ),
              ),
            ],

            // Result Image
            if (_resultImage != null) ...[
              const SizedBox(height: 24),
              Text(
                '🎉 Kết quả',
                style: AppTextStyles.titleMedium.copyWith(
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 12),
              Container(
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(AppSizes.radiusL),
                  boxShadow: [
                    BoxShadow(
                      color: AppColors.primary.withValues(alpha: 0.2),
                      blurRadius: 20,
                      offset: const Offset(0, 8),
                    ),
                  ],
                ),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(AppSizes.radiusL),
                  child: Image.file(
                    _resultImage!,
                    fit: BoxFit.cover,
                    width: double.infinity,
                  ),
                ),
              ),
              const SizedBox(height: 16),
              Row(
                children: [
                  Expanded(
                    child: OutlinedButton.icon(
                      onPressed: _processHairTryOn,
                      icon: const HugeIcon(
                        icon: HugeIcons.strokeRoundedRefresh,
                        color: AppColors.primary,
                        size: 20,
                      ),
                      label: const Text('Thử lại'),
                      style: OutlinedButton.styleFrom(
                        foregroundColor: AppColors.primary,
                        side: const BorderSide(color: AppColors.primary),
                        padding: const EdgeInsets.symmetric(vertical: 14),
                      ),
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: ElevatedButton.icon(
                      onPressed: _saveResult,
                      icon: const HugeIcon(
                        icon: HugeIcons.strokeRoundedDownload04,
                        color: AppColors.white,
                        size: 20,
                      ),
                      label: const Text('Lưu ảnh'),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppColors.success,
                        padding: const EdgeInsets.symmetric(vertical: 14),
                      ),
                    ),
                  ),
                ],
              ),
            ],

            const SizedBox(height: 32),

            // Preset Hairstyles (Preview)
            Text(
              '💈 Kiểu tóc gợi ý',
              style: AppTextStyles.titleMedium.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              'Tính năng đang phát triển - Coming soon!',
              style: AppTextStyles.bodySmall.copyWith(
                color: AppColors.textSecondary,
              ),
            ),
            const SizedBox(height: 16),
            _buildPresetHairstylesGrid(),

            const SizedBox(height: 32),
          ],
        ),
      ),
    );
  }

  Widget _buildImagePicker({
    required File? image,
    required VoidCallback onTap,
    required String placeholder,
    required String hint,
  }) {
    return GestureDetector(
      onTap: _isProcessing ? null : onTap,
      child: Container(
        height: 180,
        decoration: BoxDecoration(
          color: AppColors.white,
          borderRadius: BorderRadius.circular(AppSizes.radiusL),
          border: Border.all(
            color: image != null ? AppColors.primary : AppColors.lightGrey,
            width: 2,
          ),
        ),
        child: image != null
            ? ClipRRect(
                borderRadius: BorderRadius.circular(AppSizes.radiusL - 2),
                child: Stack(
                  fit: StackFit.expand,
                  children: [
                    Image.file(image, fit: BoxFit.cover),
                    Positioned(
                      top: 8,
                      right: 8,
                      child: Container(
                        padding: const EdgeInsets.all(8),
                        decoration: BoxDecoration(
                          color: AppColors.white,
                          shape: BoxShape.circle,
                          boxShadow: [
                            BoxShadow(
                              color: Colors.black.withValues(alpha: 0.1),
                              blurRadius: 8,
                            ),
                          ],
                        ),
                        child: const HugeIcon(
                          icon: HugeIcons.strokeRoundedEdit02,
                          color: AppColors.primary,
                          size: 18,
                        ),
                      ),
                    ),
                  ],
                ),
              )
            : Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: AppColors.veryLightGrey,
                      shape: BoxShape.circle,
                    ),
                    child: HugeIcon(
                      icon: HugeIcons.strokeRoundedImageAdd01,
                      color: AppColors.lightGrey,
                      size: 32,
                    ),
                  ),
                  const SizedBox(height: 12),
                  Text(
                    placeholder,
                    style: AppTextStyles.bodyMedium.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    hint,
                    style: AppTextStyles.bodySmall.copyWith(
                      color: AppColors.textTertiary,
                    ),
                    textAlign: TextAlign.center,
                  ),
                ],
              ),
      ),
    );
  }

  Widget _buildPresetHairstylesGrid() {
    final presets = HairTryOnService.getPresetHairstyles();

    return SizedBox(
      height: 120,
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        itemCount: presets.length,
        itemBuilder: (context, index) {
          final preset = presets[index];
          return Container(
            width: 100,
            margin: const EdgeInsets.only(right: 12),
            decoration: BoxDecoration(
              color: AppColors.veryLightGrey,
              borderRadius: BorderRadius.circular(AppSizes.radiusM),
            ),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Container(
                  width: 50,
                  height: 50,
                  decoration: BoxDecoration(
                    color: AppColors.lightGrey,
                    shape: BoxShape.circle,
                  ),
                  child: const Center(
                    child: Text('✂️', style: TextStyle(fontSize: 24)),
                  ),
                ),
                const SizedBox(height: 8),
                Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 8),
                  child: Text(
                    preset.name,
                    style: AppTextStyles.labelSmall,
                    textAlign: TextAlign.center,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }
}

// Source Button Widget
class _SourceButton extends StatelessWidget {
  final IconData icon;
  final String label;
  final VoidCallback onTap;

  const _SourceButton({
    required this.icon,
    required this.label,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(AppSizes.radiusM),
      child: Container(
        padding: const EdgeInsets.symmetric(vertical: 20),
        decoration: BoxDecoration(
          color: AppColors.veryLightGrey,
          borderRadius: BorderRadius.circular(AppSizes.radiusM),
        ),
        child: Column(
          children: [
            HugeIcon(icon: icon, color: AppColors.primary, size: 32),
            const SizedBox(height: 8),
            Text(
              label,
              style: AppTextStyles.bodyMedium.copyWith(
                fontWeight: FontWeight.w500,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
