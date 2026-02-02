import 'package:flutter/material.dart';
import 'package:hugeicons/hugeicons.dart';
import '../../../core/constants/constants.dart';

/// AI Chatbot Screen for UME App
/// Chat interface for hair consultation
class ChatbotScreen extends StatefulWidget {
  const ChatbotScreen({super.key});

  @override
  State<ChatbotScreen> createState() => _ChatbotScreenState();
}

class _ChatbotScreenState extends State<ChatbotScreen> {
  final TextEditingController _messageController = TextEditingController();
  final List<Map<String, dynamic>> _messages = [
    {
      'isBot': true,
      'message':
          'Xin chào! Tôi là trợ lý AI của UME. Tôi có thể giúp bạn:\n\n• Tư vấn kiểu tóc phù hợp\n• Đề xuất dịch vụ\n• Trả lời câu hỏi về chăm sóc tóc\n\nBạn cần hỗ trợ gì?',
      'time': '09:00',
    },
  ];

  @override
  void dispose() {
    _messageController.dispose();
    super.dispose();
  }

  void _sendMessage() {
    if (_messageController.text.trim().isEmpty) return;

    setState(() {
      _messages.add({
        'isBot': false,
        'message': _messageController.text,
        'time':
            '${DateTime.now().hour}:${DateTime.now().minute.toString().padLeft(2, '0')}',
      });
    });

    _messageController.clear();

    // Simulate bot response
    Future.delayed(const Duration(seconds: 1), () {
      setState(() {
        _messages.add({
          'isBot': true,
          'message': 'Cảm ơn bạn đã liên hệ! Tôi đang xử lý yêu cầu của bạn...',
          'time':
              '${DateTime.now().hour}:${DateTime.now().minute.toString().padLeft(2, '0')}',
        });
      });
    });
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
                  Container(
                    width: 40,
                    height: 40,
                    decoration: BoxDecoration(
                      color: AppColors.veryLightGrey,
                      borderRadius: BorderRadius.circular(AppSizes.radiusM),
                    ),
                    child: Center(
                      child: HugeIcon(
                        icon: AppIcons.chat,
                        color: AppColors.iconActive,
                        size: AppSizes.iconM,
                      ),
                    ),
                  ),
                  const SizedBox(width: AppSizes.spacingM),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'UME AI Assistant',
                          style: AppTextStyles.labelLarge,
                        ),
                        Text(
                          'Online',
                          style: AppTextStyles.bodySmall.copyWith(
                            color: AppColors.success,
                          ),
                        ),
                      ],
                    ),
                  ),
                  GestureDetector(
                    child: HugeIcon(
                      icon: AppIcons.menu,
                      color: AppColors.iconActive,
                      size: AppSizes.iconM,
                    ),
                  ),
                ],
              ),
            ),

            // Messages
            Expanded(
              child: ListView.builder(
                padding: const EdgeInsets.all(AppSizes.screenPaddingH),
                itemCount: _messages.length,
                itemBuilder: (context, index) {
                  final message = _messages[index];
                  return _ChatBubble(
                    isBot: message['isBot'] as bool,
                    message: message['message'] as String,
                    time: message['time'] as String,
                  );
                },
              ),
            ),

            // Quick Actions
            Container(
              padding: const EdgeInsets.symmetric(
                horizontal: AppSizes.screenPaddingH,
                vertical: AppSizes.paddingS,
              ),
              child: SingleChildScrollView(
                scrollDirection: Axis.horizontal,
                child: Row(
                  children: [
                    _QuickAction(label: 'Tư vấn kiểu tóc', onTap: () {}),
                    _QuickAction(label: 'Đặt lịch', onTap: () {}),
                    _QuickAction(label: 'Xem dịch vụ', onTap: () {}),
                    _QuickAction(label: 'Chăm sóc tóc', onTap: () {}),
                  ],
                ),
              ),
            ),

            // Input
            Container(
              padding: const EdgeInsets.all(AppSizes.screenPaddingH),
              color: AppColors.white,
              child: Row(
                children: [
                  Expanded(
                    child: Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: AppSizes.paddingL,
                      ),
                      decoration: BoxDecoration(
                        color: AppColors.veryLightGrey,
                        borderRadius: BorderRadius.circular(AppSizes.radiusXL),
                      ),
                      child: TextField(
                        controller: _messageController,
                        decoration: InputDecoration(
                          hintText: 'Nhập tin nhắn...',
                          hintStyle: AppTextStyles.bodyMedium.copyWith(
                            color: AppColors.textHint,
                          ),
                          border: InputBorder.none,
                          contentPadding: const EdgeInsets.symmetric(
                            vertical: AppSizes.paddingM,
                          ),
                        ),
                        onSubmitted: (_) => _sendMessage(),
                      ),
                    ),
                  ),
                  const SizedBox(width: AppSizes.spacingM),
                  GestureDetector(
                    onTap: _sendMessage,
                    child: Container(
                      width: 44,
                      height: 44,
                      decoration: BoxDecoration(
                        color: AppColors.black,
                        borderRadius: BorderRadius.circular(AppSizes.radiusL),
                      ),
                      child: Center(
                        child: HugeIcon(
                          icon: AppIcons.arrowRight,
                          color: AppColors.iconLight,
                          size: AppSizes.iconM,
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _ChatBubble extends StatelessWidget {
  final bool isBot;
  final String message;
  final String time;

  const _ChatBubble({
    required this.isBot,
    required this.message,
    required this.time,
  });

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: AppSizes.spacingM),
      child: Row(
        mainAxisAlignment: isBot
            ? MainAxisAlignment.start
            : MainAxisAlignment.end,
        crossAxisAlignment: CrossAxisAlignment.end,
        children: [
          if (isBot) ...[
            Container(
              width: 32,
              height: 32,
              decoration: BoxDecoration(
                color: AppColors.veryLightGrey,
                borderRadius: BorderRadius.circular(AppSizes.radiusS),
              ),
              child: Center(
                child: HugeIcon(
                  icon: AppIcons.chat,
                  color: AppColors.iconActive,
                  size: AppSizes.iconS,
                ),
              ),
            ),
            const SizedBox(width: AppSizes.spacingS),
          ],
          Flexible(
            child: Container(
              padding: const EdgeInsets.all(AppSizes.paddingM),
              decoration: BoxDecoration(
                color: isBot ? AppColors.white : AppColors.black,
                borderRadius: BorderRadius.circular(AppSizes.radiusL),
                border: isBot ? Border.all(color: AppColors.border) : null,
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    message,
                    style: AppTextStyles.bodyMedium.copyWith(
                      color: isBot ? AppColors.textPrimary : AppColors.white,
                    ),
                  ),
                  const SizedBox(height: AppSizes.spacingXS),
                  Text(
                    time,
                    style: AppTextStyles.labelSmall.copyWith(
                      color: isBot
                          ? AppColors.textHint
                          : AppColors.white.withValues(alpha: 0.7),
                    ),
                  ),
                ],
              ),
            ),
          ),
          if (!isBot) const SizedBox(width: AppSizes.spacingS),
        ],
      ),
    );
  }
}

class _QuickAction extends StatelessWidget {
  final String label;
  final VoidCallback onTap;

  const _QuickAction({required this.label, required this.onTap});

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        margin: const EdgeInsets.only(right: AppSizes.spacingS),
        padding: const EdgeInsets.symmetric(
          horizontal: AppSizes.paddingM,
          vertical: AppSizes.paddingS,
        ),
        decoration: BoxDecoration(
          color: AppColors.white,
          borderRadius: BorderRadius.circular(AppSizes.radiusFull),
          border: Border.all(color: AppColors.border),
        ),
        child: Text(label, style: AppTextStyles.labelMedium),
      ),
    );
  }
}
