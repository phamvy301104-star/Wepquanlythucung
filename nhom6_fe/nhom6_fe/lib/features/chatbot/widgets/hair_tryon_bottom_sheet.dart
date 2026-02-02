import 'dart:io';
import 'package:flutter/material.dart';
import 'package:hugeicons/hugeicons.dart';
import 'package:image_picker/image_picker.dart';
import '../../../core/constants/constants.dart';
import '../../../core/services/replicate_service.dart';

/// Hair Try-On Bottom Sheet
/// Cho phép người dùng chọn ảnh mặt + ảnh kiểu tóc để AI ghép
class HairTryonBottomSheet extends StatefulWidget {
  final Function(File resultImage, String message)? onSuccess;
  final Function(String error)? onError;

  const HairTryonBottomSheet({super.key, this.onSuccess, this.onError});

  /// Show bottom sheet và trả về kết quả
  static Future<HairTransferResult?> show(BuildContext context) async {
    HairTransferResult? result;

    await showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => HairTryonBottomSheet(
        onSuccess: (file, message) {
          result = HairTransferResult(
            success: true,
            resultImage: file,
            message: message,
          );
          Navigator.pop(context);
        },
        onError: (error) {
          result = HairTransferResult(success: false, message: error);
          Navigator.pop(context);
        },
      ),
    );

    return result;
  }

  @override
  State<HairTryonBottomSheet> createState() => _HairTryonBottomSheetState();
}

class _HairTryonBottomSheetState extends State<HairTryonBottomSheet> {
  final ImagePicker _picker = ImagePicker();
  final ReplicateService _replicateService = ReplicateService();

  File? _faceImage;
  File? _hairStyleImage;
  bool _isProcessing = false;
  String _statusMessage = '';
  double _progress = 0;

  @override
  void initState() {
    super.initState();
    _initializeService();
  }

  Future<void> _initializeService() async {
    try {
      await _replicateService.initialize();
    } catch (e) {
      if (mounted) {
        setState(() {
          _statusMessage = 'Lỗi: $e';
        });
      }
    }
  }

  Future<void> _pickImage(bool isFace) async {
    try {
      // Show option dialog
      final source = await showDialog<ImageSource>(
        context: context,
        builder: (context) => AlertDialog(
          title: Text(isFace ? 'Chọn ảnh khuôn mặt' : 'Chọn ảnh kiểu tóc'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              ListTile(
                leading: const Icon(Icons.camera_alt, color: AppColors.primary),
                title: const Text('Chụp ảnh mới'),
                onTap: () => Navigator.pop(context, ImageSource.camera),
              ),
              ListTile(
                leading: const Icon(Icons.photo_library, color: AppColors.info),
                title: const Text('Chọn từ thư viện'),
                onTap: () => Navigator.pop(context, ImageSource.gallery),
              ),
            ],
          ),
        ),
      );

      if (source == null) return;

      final XFile? picked = await _picker.pickImage(
        source: source,
        maxWidth: 1024,
        maxHeight: 1024,
        imageQuality: 85,
      );

      if (picked != null && mounted) {
        setState(() {
          if (isFace) {
            _faceImage = File(picked.path);
          } else {
            _hairStyleImage = File(picked.path);
          }
        });
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('Lỗi chọn ảnh: $e')));
      }
    }
  }

  Future<void> _processHairTryon() async {
    if (_faceImage == null || _hairStyleImage == null) {
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(const SnackBar(content: Text('Vui lòng chọn cả 2 ảnh!')));
      return;
    }

    setState(() {
      _isProcessing = true;
      _progress = 0;
      _statusMessage = '🚀 Đang upload ảnh...';
    });

    // Simulate progress
    _updateProgress();

    try {
      final result = await _replicateService.transferHairStyle(
        faceImage: _faceImage!,
        hairStyleImage: _hairStyleImage!,
      );

      if (mounted) {
        if (result.success && result.resultImage != null) {
          widget.onSuccess?.call(result.resultImage!, result.message);
        } else {
          widget.onError?.call(result.message);
        }
      }
    } catch (e) {
      if (mounted) {
        widget.onError?.call('Có lỗi xảy ra: $e');
      }
    }

    if (mounted) {
      setState(() {
        _isProcessing = false;
      });
    }
  }

  void _updateProgress() async {
    final messages = [
      '📤 Đang upload ảnh...',
      '🤖 AI đang phân tích khuôn mặt...',
      '✂️ Đang xử lý kiểu tóc...',
      '🎨 Đang ghép tóc vào ảnh...',
      '✨ Đang hoàn thiện...',
    ];

    for (var i = 0; i < messages.length; i++) {
      if (!mounted || !_isProcessing) break;

      await Future.delayed(const Duration(seconds: 3));

      if (mounted && _isProcessing) {
        setState(() {
          _progress = (i + 1) / messages.length;
          _statusMessage = messages[i];
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      constraints: BoxConstraints(
        maxHeight: MediaQuery.of(context).size.height * 0.85,
      ),
      decoration: const BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      child: SingleChildScrollView(
        child: Padding(
          padding: EdgeInsets.only(
            left: 24,
            right: 24,
            top: 16,
            bottom: MediaQuery.of(context).viewInsets.bottom + 24,
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              // Handle bar
              Container(
                width: 40,
                height: 4,
                decoration: BoxDecoration(
                  color: AppColors.lightGrey,
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
              const SizedBox(height: 20),

              // Title
              Row(
                children: [
                  Container(
                    padding: const EdgeInsets.all(10),
                    decoration: BoxDecoration(
                      gradient: const LinearGradient(
                        colors: [Color(0xFF667eea), Color(0xFF764ba2)],
                      ),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: const Text('✨', style: TextStyle(fontSize: 24)),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Thử Kiểu Tóc AI',
                          style: AppTextStyles.h4.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        Text(
                          'Ghép kiểu tóc yêu thích lên ảnh của bạn',
                          style: AppTextStyles.bodySmall.copyWith(
                            color: AppColors.textSecondary,
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 24),

              // Image Selection Row
              Row(
                children: [
                  // Face Image
                  Expanded(
                    child: _buildImageSelector(
                      title: 'Ảnh khuôn mặt',
                      subtitle: 'Chụp ảnh thẳng mặt',
                      image: _faceImage,
                      icon: HugeIcons.strokeRoundedUser,
                      color: const Color(0xFF667eea),
                      onTap: () => _pickImage(true),
                    ),
                  ),
                  const SizedBox(width: 16),
                  // Hair Style Image
                  Expanded(
                    child: _buildImageSelector(
                      title: 'Ảnh kiểu tóc',
                      subtitle: 'Tóc bạn muốn thử',
                      image: _hairStyleImage,
                      icon: HugeIcons.strokeRoundedHairDryer,
                      color: const Color(0xFF764ba2),
                      onTap: () => _pickImage(false),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 20),

              // Tips
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: AppColors.info.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(
                    color: AppColors.info.withValues(alpha: 0.3),
                  ),
                ),
                child: Row(
                  children: [
                    const Icon(
                      Icons.lightbulb_outline,
                      color: AppColors.info,
                      size: 20,
                    ),
                    const SizedBox(width: 10),
                    Expanded(
                      child: Text(
                        'Tip: Dùng ảnh rõ mặt, ánh sáng tốt để có kết quả đẹp nhất!',
                        style: AppTextStyles.bodySmall.copyWith(
                          color: AppColors.info,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 20),

              // Progress indicator (when processing)
              if (_isProcessing) ...[
                Container(
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: AppColors.veryLightGrey,
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Column(
                    children: [
                      LinearProgressIndicator(
                        value: _progress,
                        backgroundColor: AppColors.lightGrey,
                        valueColor: const AlwaysStoppedAnimation<Color>(
                          Color(0xFF667eea),
                        ),
                        borderRadius: BorderRadius.circular(4),
                      ),
                      const SizedBox(height: 12),
                      Text(
                        _statusMessage,
                        style: AppTextStyles.bodyMedium.copyWith(
                          color: AppColors.textSecondary,
                        ),
                        textAlign: TextAlign.center,
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 20),
              ],

              // Action Button
              SizedBox(
                width: double.infinity,
                height: 52,
                child: ElevatedButton(
                  onPressed:
                      (_faceImage != null &&
                          _hairStyleImage != null &&
                          !_isProcessing)
                      ? _processHairTryon
                      : null,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: const Color(0xFF667eea),
                    foregroundColor: Colors.white,
                    disabledBackgroundColor: AppColors.lightGrey,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(16),
                    ),
                    elevation: 0,
                  ),
                  child: _isProcessing
                      ? Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            const SizedBox(
                              width: 20,
                              height: 20,
                              child: CircularProgressIndicator(
                                strokeWidth: 2,
                                valueColor: AlwaysStoppedAnimation<Color>(
                                  Colors.white,
                                ),
                              ),
                            ),
                            const SizedBox(width: 12),
                            Text(
                              'Đang xử lý...',
                              style: AppTextStyles.labelLarge.copyWith(
                                color: Colors.white,
                              ),
                            ),
                          ],
                        )
                      : Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            const Text('✨', style: TextStyle(fontSize: 20)),
                            const SizedBox(width: 8),
                            Text(
                              'Tạo ảnh thử tóc',
                              style: AppTextStyles.labelLarge.copyWith(
                                color: Colors.white,
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                          ],
                        ),
                ),
              ),
              const SizedBox(height: 12),

              // Cost info
              Text(
                '💰 Chi phí: ~\$0.04/ảnh (~1,000đ)',
                style: AppTextStyles.labelSmall.copyWith(
                  color: AppColors.textHint,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildImageSelector({
    required String title,
    required String subtitle,
    required File? image,
    required IconData icon,
    required Color color,
    required VoidCallback onTap,
  }) {
    return GestureDetector(
      onTap: _isProcessing ? null : onTap,
      child: Container(
        height: 160,
        decoration: BoxDecoration(
          color: image != null ? null : color.withValues(alpha: 0.05),
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: image != null ? color : color.withValues(alpha: 0.3),
            width: image != null ? 2 : 1,
          ),
          image: image != null
              ? DecorationImage(image: FileImage(image), fit: BoxFit.cover)
              : null,
        ),
        child: image == null
            ? Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: color.withValues(alpha: 0.1),
                      shape: BoxShape.circle,
                    ),
                    child: HugeIcon(icon: icon, color: color, size: 28),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    title,
                    style: AppTextStyles.labelMedium.copyWith(
                      fontWeight: FontWeight.w600,
                      color: color,
                    ),
                  ),
                  Text(
                    subtitle,
                    style: AppTextStyles.labelSmall.copyWith(
                      color: AppColors.textHint,
                    ),
                    textAlign: TextAlign.center,
                  ),
                ],
              )
            : Align(
                alignment: Alignment.topRight,
                child: Container(
                  margin: const EdgeInsets.all(8),
                  padding: const EdgeInsets.all(6),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    shape: BoxShape.circle,
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withValues(alpha: 0.1),
                        blurRadius: 4,
                      ),
                    ],
                  ),
                  child: Icon(Icons.edit, size: 16, color: color),
                ),
              ),
      ),
    );
  }
}
