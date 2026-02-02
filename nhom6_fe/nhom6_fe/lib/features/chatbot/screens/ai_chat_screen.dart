import 'dart:io';
import 'package:flutter/material.dart';
import 'package:hugeicons/hugeicons.dart';
import 'package:uuid/uuid.dart';
import '../../../core/constants/constants.dart';
import '../services/chatbot_service.dart';
import '../widgets/hair_tryon_bottom_sheet.dart';
import '../../face_analysis/face_analysis.dart';

/// Ume-Stylist AI Chatbot Screen
/// Chat interface với Gemini AI cho tư vấn kiểu tóc
class AiChatScreen extends StatefulWidget {
  final FaceAnalysisResult? initialFaceResult;

  const AiChatScreen({super.key, this.initialFaceResult});

  @override
  State<AiChatScreen> createState() => _AiChatScreenState();
}

class _AiChatScreenState extends State<AiChatScreen> {
  final TextEditingController _messageController = TextEditingController();
  final ScrollController _scrollController = ScrollController();
  final ChatbotService _chatbotService = ChatbotService();
  final Uuid _uuid = const Uuid();

  final List<ChatMessage> _messages = [];
  bool _isLoading = false;
  bool _isInitialized = false;

  @override
  void initState() {
    super.initState();
    _initializeChatbot();
  }

  Future<void> _initializeChatbot() async {
    try {
      await _chatbotService.initialize();

      if (!mounted) return; // Check mounted before setState

      setState(() {
        _isInitialized = true;
        // Add welcome message
        _messages.add(
          ChatMessage(
            id: _uuid.v4(),
            text: _chatbotService.getWelcomeMessage(),
            isBot: true,
            timestamp: DateTime.now(),
          ),
        );
      });

      // Nếu có face result từ scan, set context và gửi message
      if (widget.initialFaceResult != null) {
        _chatbotService.setFaceShapeContext(widget.initialFaceResult!);
        _sendAutoMessage(
          'Mình vừa scan khuôn mặt xong, kết quả là mặt ${widget.initialFaceResult!.faceShape.displayName}. Tư vấn giúp mình kiểu tóc phù hợp nhé!',
        );
      }
    } catch (e) {
      if (!mounted) return; // Check mounted before setState

      setState(() {
        _messages.add(
          ChatMessage(
            id: _uuid.v4(),
            text:
                '⚠️ **Lỗi khởi tạo AI**\n\n$e\n\nVui lòng kiểm tra GEMINI_API_KEY trong file .env',
            isBot: true,
            timestamp: DateTime.now(),
          ),
        );
      });
    }
  }

  void _scrollToBottom() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (_scrollController.hasClients) {
        _scrollController.animateTo(
          _scrollController.position.maxScrollExtent,
          duration: const Duration(milliseconds: 300),
          curve: Curves.easeOut,
        );
      }
    });
  }

  Future<void> _sendMessage() async {
    final text = _messageController.text.trim();
    if (text.isEmpty || _isLoading) return;

    _messageController.clear();
    await _sendAutoMessage(text);
  }

  Future<void> _sendAutoMessage(String text) async {
    // Add user message
    setState(() {
      _messages.add(
        ChatMessage(
          id: _uuid.v4(),
          text: text,
          isBot: false,
          timestamp: DateTime.now(),
        ),
      );
      _isLoading = true;
    });
    _scrollToBottom();

    try {
      // Get AI response
      final response = await _chatbotService.sendMessage(text);

      setState(() {
        _messages.add(
          ChatMessage(
            id: _uuid.v4(),
            text: response,
            isBot: true,
            timestamp: DateTime.now(),
          ),
        );
        _isLoading = false;
      });
      _scrollToBottom();
    } catch (e) {
      setState(() {
        _messages.add(
          ChatMessage(
            id: _uuid.v4(),
            text: 'Có lỗi xảy ra: $e',
            isBot: true,
            timestamp: DateTime.now(),
          ),
        );
        _isLoading = false;
      });
      _scrollToBottom();
    }
  }

  Future<void> _openFaceAnalysis() async {
    final result = await Navigator.push<FaceAnalysisResult>(
      context,
      MaterialPageRoute(builder: (context) => const FaceAnalysisScreen()),
    );

    if (result != null && mounted) {
      _chatbotService.setFaceShapeContext(result);
      _sendAutoMessage(
        'Mình vừa scan khuôn mặt, kết quả là mặt ${result.faceShape.displayName} (độ chính xác ${(result.confidence * 100).toInt()}%). Tư vấn giúp mình kiểu tóc phù hợp với!',
      );
    }
  }

  /// Mở Hair Try-on Bottom Sheet
  Future<void> _openHairTryon() async {
    final result = await HairTryonBottomSheet.show(context);

    if (result != null && mounted) {
      if (result.success && result.resultImage != null) {
        // Add success message with image
        setState(() {
          _messages.add(
            ChatMessage(
              id: _uuid.v4(),
              text: '✨ Đây là kết quả thử kiểu tóc của bạn!',
              isBot: true,
              timestamp: DateTime.now(),
              imageFile: result.resultImage,
            ),
          );
        });
        _scrollToBottom();

        // Ask Gemini for feedback
        _sendAutoMessage(
          'Mình vừa thử ghép một kiểu tóc mới, bạn có thể tư vấn thêm về kiểu tóc phù hợp với mình không?',
        );
      } else {
        // Show error
        setState(() {
          _messages.add(
            ChatMessage(
              id: _uuid.v4(),
              text: '⚠️ ${result.message}',
              isBot: true,
              timestamp: DateTime.now(),
            ),
          );
        });
        _scrollToBottom();
      }
    }
  }

  void _handleQuickAction(String action) {
    switch (action) {
      case 'face_scan':
        _openFaceAnalysis();
        break;
      case 'hair_tryon':
        _openHairTryon();
        break;
      case 'booking':
        _sendAutoMessage('Mình muốn đặt lịch cắt tóc');
        break;
      case 'price':
        _sendAutoMessage('Cho mình xem bảng giá dịch vụ với');
        break;
      case 'products':
        _sendAutoMessage('Tư vấn cho mình sáp vuốt tóc phù hợp với');
        break;
    }
  }

  void _resetConversation() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Xóa hội thoại?'),
        content: const Text('Bạn có chắc muốn bắt đầu cuộc trò chuyện mới?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Hủy'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(context);
              _chatbotService.resetConversation();
              setState(() {
                _messages.clear();
                _messages.add(
                  ChatMessage(
                    id: _uuid.v4(),
                    text: _chatbotService.getWelcomeMessage(),
                    isBot: true,
                    timestamp: DateTime.now(),
                  ),
                );
              });
            },
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary),
            child: const Text('Xóa'),
          ),
        ],
      ),
    );
  }

  /// Hiển thị thông tin về AI Chatbot
  void _showChatInfo() {
    showModalBottomSheet(
      context: context,
      backgroundColor: Colors.transparent,
      builder: (context) => Container(
        padding: const EdgeInsets.all(24),
        decoration: const BoxDecoration(
          color: AppColors.white,
          borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
        ),
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
            Container(
              width: 64,
              height: 64,
              decoration: BoxDecoration(
                gradient: const LinearGradient(
                  colors: [Color(0xFF667eea), Color(0xFF764ba2)],
                ),
                borderRadius: BorderRadius.circular(16),
              ),
              child: const Center(
                child: Text('✂️', style: TextStyle(fontSize: 32)),
              ),
            ),
            const SizedBox(height: 16),
            Text(
              'Ume-Stylist AI',
              style: AppTextStyles.h3.copyWith(fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 8),
            Text(
              'Powered by Google Gemini 2.5 Flash',
              style: AppTextStyles.bodyMedium.copyWith(
                color: AppColors.textSecondary,
              ),
            ),
            const SizedBox(height: 16),
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: AppColors.veryLightGrey,
                borderRadius: BorderRadius.circular(12),
              ),
              child: Column(
                children: [
                  _buildInfoRow('💇', 'Tư vấn kiểu tóc theo khuôn mặt'),
                  _buildInfoRow('📸', 'Phân tích khuôn mặt với AI'),
                  _buildInfoRow('💰', 'Tư vấn bảng giá dịch vụ'),
                  _buildInfoRow('📅', 'Hỗ trợ đặt lịch'),
                  _buildInfoRow('✨', 'Thử kiểu tóc với AI'),
                ],
              ),
            ),
            const SizedBox(height: 20),
          ],
        ),
      ),
    );
  }

  Widget _buildInfoRow(String emoji, String text) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6),
      child: Row(
        children: [
          Text(emoji, style: const TextStyle(fontSize: 18)),
          const SizedBox(width: 12),
          Expanded(child: Text(text, style: AppTextStyles.bodyMedium)),
        ],
      ),
    );
  }

  @override
  void dispose() {
    _messageController.dispose();
    _scrollController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(
          children: [
            // Header
            _buildHeader(),

            // Messages
            Expanded(
              child: ListView.builder(
                controller: _scrollController,
                padding: const EdgeInsets.all(AppSizes.screenPaddingH),
                itemCount: _messages.length + (_isLoading ? 1 : 0),
                itemBuilder: (context, index) {
                  if (index == _messages.length && _isLoading) {
                    return _buildTypingIndicator();
                  }
                  return _ChatBubble(message: _messages[index]);
                },
              ),
            ),

            // Quick Actions
            _buildQuickActions(),

            // Input
            _buildInputArea(),
          ],
        ),
      ),
    );
  }

  Widget _buildHeader() {
    return Container(
      padding: const EdgeInsets.all(AppSizes.screenPaddingH),
      color: AppColors.white,
      child: Row(
        children: [
          // Info button (thay vì back button vì đây là tab trong IndexedStack)
          GestureDetector(
            onTap: () => _showChatInfo(),
            child: const HugeIcon(
              icon: HugeIcons.strokeRoundedInformationCircle,
              color: AppColors.textPrimary,
              size: 24,
            ),
          ),
          const SizedBox(width: 12),
          // Avatar
          Container(
            width: 44,
            height: 44,
            decoration: BoxDecoration(
              gradient: const LinearGradient(
                colors: [Color(0xFF667eea), Color(0xFF764ba2)],
              ),
              borderRadius: BorderRadius.circular(AppSizes.radiusM),
            ),
            child: const Center(
              child: Text('✂️', style: TextStyle(fontSize: 24)),
            ),
          ),
          const SizedBox(width: AppSizes.spacingM),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Ume-Stylist',
                  style: AppTextStyles.labelLarge.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
                Row(
                  children: [
                    Container(
                      width: 8,
                      height: 8,
                      decoration: BoxDecoration(
                        color: _isInitialized
                            ? AppColors.success
                            : AppColors.warning,
                        shape: BoxShape.circle,
                      ),
                    ),
                    const SizedBox(width: 6),
                    Text(
                      _isInitialized
                          ? 'Online • Powered by Gemini AI'
                          : 'Đang kết nối...',
                      style: AppTextStyles.bodySmall.copyWith(
                        color: AppColors.textSecondary,
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
          // Reset button
          GestureDetector(
            onTap: _resetConversation,
            child: Container(
              padding: const EdgeInsets.all(8),
              decoration: BoxDecoration(
                color: AppColors.veryLightGrey,
                borderRadius: BorderRadius.circular(8),
              ),
              child: const HugeIcon(
                icon: HugeIcons.strokeRoundedRefresh,
                color: AppColors.textSecondary,
                size: 20,
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildTypingIndicator() {
    return Padding(
      padding: const EdgeInsets.only(bottom: AppSizes.spacingM),
      child: Row(
        children: [
          Container(
            width: 36,
            height: 36,
            decoration: BoxDecoration(
              gradient: const LinearGradient(
                colors: [Color(0xFF667eea), Color(0xFF764ba2)],
              ),
              borderRadius: BorderRadius.circular(AppSizes.radiusS),
            ),
            child: const Center(
              child: Text('✂️', style: TextStyle(fontSize: 18)),
            ),
          ),
          const SizedBox(width: AppSizes.spacingS),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
            decoration: BoxDecoration(
              color: AppColors.white,
              borderRadius: BorderRadius.circular(AppSizes.radiusL),
              border: Border.all(color: AppColors.border),
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                _buildDot(0),
                const SizedBox(width: 4),
                _buildDot(1),
                const SizedBox(width: 4),
                _buildDot(2),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildDot(int index) {
    return TweenAnimationBuilder<double>(
      tween: Tween(begin: 0, end: 1),
      duration: Duration(milliseconds: 600 + index * 200),
      builder: (context, value, child) {
        return Container(
          width: 8,
          height: 8,
          decoration: BoxDecoration(
            color: AppColors.primary.withValues(alpha: 0.3 + value * 0.7),
            shape: BoxShape.circle,
          ),
        );
      },
    );
  }

  Widget _buildQuickActions() {
    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: AppSizes.screenPaddingH,
        vertical: AppSizes.paddingS,
      ),
      child: SingleChildScrollView(
        scrollDirection: Axis.horizontal,
        child: Row(
          children: [
            _QuickActionChip(
              icon: HugeIcons.strokeRoundedCamera01,
              label: 'Scan khuôn mặt',
              color: const Color(0xFF667eea),
              onTap: () => _handleQuickAction('face_scan'),
            ),
            _QuickActionChip(
              icon: HugeIcons.strokeRoundedMagicWand01,
              label: 'Thử kiểu tóc AI',
              color: const Color(0xFF764ba2),
              onTap: () => _handleQuickAction('hair_tryon'),
            ),
            _QuickActionChip(
              icon: HugeIcons.strokeRoundedCalendar03,
              label: 'Đặt lịch',
              color: AppColors.success,
              onTap: () => _handleQuickAction('booking'),
            ),
            _QuickActionChip(
              icon: HugeIcons.strokeRoundedMoney03,
              label: 'Bảng giá',
              color: AppColors.warning,
              onTap: () => _handleQuickAction('price'),
            ),
            _QuickActionChip(
              icon: HugeIcons.strokeRoundedShoppingBag01,
              label: 'Sản phẩm',
              color: AppColors.info,
              onTap: () => _handleQuickAction('products'),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildInputArea() {
    return Container(
      padding: const EdgeInsets.all(AppSizes.screenPaddingH),
      color: AppColors.white,
      child: Row(
        children: [
          // Attachment button (Face scan shortcut)
          GestureDetector(
            onTap: _openFaceAnalysis,
            child: Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                color: AppColors.veryLightGrey,
                borderRadius: BorderRadius.circular(AppSizes.radiusM),
              ),
              child: const Center(
                child: HugeIcon(
                  icon: HugeIcons.strokeRoundedCamera01,
                  color: AppColors.primary,
                  size: 22,
                ),
              ),
            ),
          ),
          const SizedBox(width: 8),
          // Hair Try-on button
          GestureDetector(
            onTap: _openHairTryon,
            child: Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                color: const Color(0xFF764ba2).withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(AppSizes.radiusM),
              ),
              child: const Center(
                child: HugeIcon(
                  icon: HugeIcons.strokeRoundedMagicWand01,
                  color: Color(0xFF764ba2),
                  size: 22,
                ),
              ),
            ),
          ),
          const SizedBox(width: 12),
          // Text input
          Expanded(
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 16),
              decoration: BoxDecoration(
                color: AppColors.veryLightGrey,
                borderRadius: BorderRadius.circular(24),
              ),
              child: TextField(
                controller: _messageController,
                decoration: InputDecoration(
                  hintText: 'Nhắn tin cho Ume...',
                  hintStyle: AppTextStyles.bodyMedium.copyWith(
                    color: AppColors.textHint,
                  ),
                  border: InputBorder.none,
                  contentPadding: const EdgeInsets.symmetric(vertical: 12),
                ),
                textInputAction: TextInputAction.send,
                onSubmitted: (_) => _sendMessage(),
                enabled: _isInitialized && !_isLoading,
              ),
            ),
          ),
          const SizedBox(width: 12),
          // Send button
          GestureDetector(
            onTap: _isLoading ? null : _sendMessage,
            child: Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                color: _isLoading ? AppColors.lightGrey : AppColors.primary,
                borderRadius: BorderRadius.circular(AppSizes.radiusM),
              ),
              child: Center(
                child: HugeIcon(
                  icon: HugeIcons.strokeRoundedSent,
                  color: AppColors.white,
                  size: 22,
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

/// Chat Message Model
class ChatMessage {
  final String id;
  final String text;
  final bool isBot;
  final DateTime timestamp;
  final String? imageUrl;
  final File? imageFile;

  ChatMessage({
    required this.id,
    required this.text,
    required this.isBot,
    required this.timestamp,
    this.imageUrl,
    this.imageFile,
  });
}

/// Chat Bubble Widget
class _ChatBubble extends StatelessWidget {
  final ChatMessage message;

  const _ChatBubble({required this.message});

  @override
  Widget build(BuildContext context) {
    final isBot = message.isBot;

    return Padding(
      padding: const EdgeInsets.only(bottom: AppSizes.spacingM),
      child: Row(
        mainAxisAlignment: isBot
            ? MainAxisAlignment.start
            : MainAxisAlignment.end,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          if (isBot) ...[
            Container(
              width: 36,
              height: 36,
              decoration: BoxDecoration(
                gradient: const LinearGradient(
                  colors: [Color(0xFF667eea), Color(0xFF764ba2)],
                ),
                borderRadius: BorderRadius.circular(AppSizes.radiusS),
              ),
              child: const Center(
                child: Text('✂️', style: TextStyle(fontSize: 18)),
              ),
            ),
            const SizedBox(width: AppSizes.spacingS),
          ],
          Flexible(
            child: Container(
              padding: const EdgeInsets.all(14),
              decoration: BoxDecoration(
                color: isBot ? AppColors.white : AppColors.primary,
                borderRadius: BorderRadius.only(
                  topLeft: Radius.circular(isBot ? 4 : 16),
                  topRight: Radius.circular(isBot ? 16 : 4),
                  bottomLeft: const Radius.circular(16),
                  bottomRight: const Radius.circular(16),
                ),
                border: isBot ? Border.all(color: AppColors.border) : null,
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withValues(alpha: 0.05),
                    blurRadius: 8,
                    offset: const Offset(0, 2),
                  ),
                ],
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Hiển thị ảnh nếu có
                  if (message.imageFile != null) ...[
                    ClipRRect(
                      borderRadius: BorderRadius.circular(12),
                      child: Image.file(
                        message.imageFile!,
                        width: double.infinity,
                        fit: BoxFit.cover,
                      ),
                    ),
                    const SizedBox(height: 10),
                  ],
                  // Parse markdown-like text
                  _buildFormattedText(
                    message.text,
                    isBot ? AppColors.textPrimary : AppColors.white,
                  ),
                  const SizedBox(height: 6),
                  Text(
                    _formatTime(message.timestamp),
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

  Widget _buildFormattedText(String text, Color color) {
    // Simple markdown parsing for bold text
    final spans = <TextSpan>[];
    final pattern = RegExp(r'\*\*(.*?)\*\*');
    int lastEnd = 0;

    for (final match in pattern.allMatches(text)) {
      if (match.start > lastEnd) {
        spans.add(
          TextSpan(
            text: text.substring(lastEnd, match.start),
            style: AppTextStyles.bodyMedium.copyWith(color: color),
          ),
        );
      }
      spans.add(
        TextSpan(
          text: match.group(1),
          style: AppTextStyles.bodyMedium.copyWith(
            color: color,
            fontWeight: FontWeight.bold,
          ),
        ),
      );
      lastEnd = match.end;
    }

    if (lastEnd < text.length) {
      spans.add(
        TextSpan(
          text: text.substring(lastEnd),
          style: AppTextStyles.bodyMedium.copyWith(color: color),
        ),
      );
    }

    if (spans.isEmpty) {
      spans.add(
        TextSpan(
          text: text,
          style: AppTextStyles.bodyMedium.copyWith(color: color),
        ),
      );
    }

    return RichText(text: TextSpan(children: spans));
  }

  String _formatTime(DateTime time) {
    return '${time.hour.toString().padLeft(2, '0')}:${time.minute.toString().padLeft(2, '0')}';
  }
}

/// Quick Action Chip Widget
class _QuickActionChip extends StatelessWidget {
  final IconData icon;
  final String label;
  final Color color;
  final VoidCallback onTap;

  const _QuickActionChip({
    required this.icon,
    required this.label,
    required this.color,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(right: 8),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(20),
        child: Container(
          padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
          decoration: BoxDecoration(
            color: color.withValues(alpha: 0.1),
            borderRadius: BorderRadius.circular(20),
            border: Border.all(color: color.withValues(alpha: 0.3)),
          ),
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              HugeIcon(icon: icon, color: color, size: 18),
              const SizedBox(width: 6),
              Text(
                label,
                style: AppTextStyles.labelMedium.copyWith(
                  color: color,
                  fontWeight: FontWeight.w500,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
