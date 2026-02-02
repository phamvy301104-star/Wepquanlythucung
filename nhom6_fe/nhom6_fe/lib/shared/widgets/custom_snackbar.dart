import 'package:flutter/material.dart';

/// Custom SnackBar Widget for beautiful notifications
class CustomSnackBar {
  /// Show success notification
  static void showSuccess(BuildContext context, String message) {
    _show(context, message, _SnackBarType.success);
  }

  /// Show error notification
  static void showError(BuildContext context, String message) {
    _show(context, message, _SnackBarType.error);
  }

  /// Show warning notification
  static void showWarning(BuildContext context, String message) {
    _show(context, message, _SnackBarType.warning);
  }

  /// Show info notification
  static void showInfo(BuildContext context, String message) {
    _show(context, message, _SnackBarType.info);
  }

  static void _show(BuildContext context, String message, _SnackBarType type) {
    ScaffoldMessenger.of(context).hideCurrentSnackBar();

    final config = _getConfig(type);

    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Container(
          padding: const EdgeInsets.symmetric(vertical: 4),
          child: Row(
            children: [
              Container(
                padding: const EdgeInsets.all(8),
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.2),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Icon(config.icon, color: Colors.white, size: 20),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Text(
                      config.title,
                      style: const TextStyle(
                        color: Colors.white,
                        fontWeight: FontWeight.bold,
                        fontSize: 14,
                      ),
                    ),
                    const SizedBox(height: 2),
                    Text(
                      message,
                      style: TextStyle(
                        color: Colors.white.withValues(alpha: 0.9),
                        fontSize: 13,
                      ),
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
        backgroundColor: config.backgroundColor,
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        margin: const EdgeInsets.all(16),
        duration: const Duration(seconds: 3),
        elevation: 8,
        dismissDirection: DismissDirection.horizontal,
      ),
    );
  }

  static _SnackBarConfig _getConfig(_SnackBarType type) {
    switch (type) {
      case _SnackBarType.success:
        return _SnackBarConfig(
          icon: Icons.check_circle_outline,
          title: 'Thành công',
          backgroundColor: const Color(0xFF4CAF50), // Xanh lá sáng
        );
      case _SnackBarType.error:
        return _SnackBarConfig(
          icon: Icons.error_outline,
          title: 'Lỗi',
          backgroundColor: const Color(0xFFE57373), // Đỏ nhạt
        );
      case _SnackBarType.warning:
        return _SnackBarConfig(
          icon: Icons.warning_amber_outlined,
          title: 'Cảnh báo',
          backgroundColor: const Color(0xFFFFB74D), // Cam nhạt
        );
      case _SnackBarType.info:
        return _SnackBarConfig(
          icon: Icons.info_outline,
          title: 'Thông báo',
          backgroundColor: const Color(0xFF64B5F6), // Xanh dương nhạt
        );
    }
  }
}

enum _SnackBarType { success, error, warning, info }

class _SnackBarConfig {
  final IconData icon;
  final String title;
  final Color backgroundColor;

  _SnackBarConfig({
    required this.icon,
    required this.title,
    required this.backgroundColor,
  });
}
