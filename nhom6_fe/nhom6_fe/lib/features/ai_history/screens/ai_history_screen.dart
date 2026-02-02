import 'package:flutter/material.dart';
import '../../../core/services/ai_history_service.dart';
import '../../../core/constants/app_colors.dart';
import '../../../core/constants/app_text_styles.dart';

/// Màn hình hiển thị lịch sử sử dụng các tính năng AI
class AiHistoryScreen extends StatefulWidget {
  const AiHistoryScreen({super.key});

  @override
  State<AiHistoryScreen> createState() => _AiHistoryScreenState();
}

class _AiHistoryScreenState extends State<AiHistoryScreen>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  final AiHistoryService _historyService = AiHistoryService();

  List<FaceAnalysisHistory> _faceAnalyses = [];
  List<HairTryOnHistory> _hairTryOns = [];
  List<ChatSessionHistory> _chatSessions = [];

  bool _isLoading = true;
  String? _errorMessage;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
    _loadHistory();
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Future<void> _loadHistory() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final history = await _historyService.getAllHistory(limit: 20);

      if (history != null) {
        setState(() {
          _faceAnalyses = history.faceAnalyses;
          _hairTryOns = history.hairTryOns;
          _chatSessions = history.chatSessions;
          _isLoading = false;
        });
      } else {
        // If combined API fails, load individually
        final faces = await _historyService.getFaceAnalysisHistory();
        final hairs = await _historyService.getHairTryOnHistory();
        final chats = await _historyService.getChatSessions();

        setState(() {
          _faceAnalyses = faces;
          _hairTryOns = hairs;
          _chatSessions = chats;
          _isLoading = false;
        });
      }
    } catch (e) {
      setState(() {
        _errorMessage = 'Không thể tải lịch sử. Vui lòng thử lại.';
        _isLoading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Lịch Sử AI'),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        bottom: TabBar(
          controller: _tabController,
          indicatorColor: Colors.white,
          labelColor: Colors.white,
          unselectedLabelColor: Colors.white70,
          tabs: [
            Tab(
              icon: const Icon(Icons.face_retouching_natural, size: 20),
              text: 'Khuôn Mặt (${_faceAnalyses.length})',
            ),
            Tab(
              icon: const Icon(Icons.auto_awesome, size: 20),
              text: 'Thử Tóc (${_hairTryOns.length})',
            ),
            Tab(
              icon: const Icon(Icons.chat_bubble_outline, size: 20),
              text: 'Tư Vấn (${_chatSessions.length})',
            ),
          ],
        ),
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _errorMessage != null
          ? _buildErrorWidget()
          : TabBarView(
              controller: _tabController,
              children: [
                _buildFaceAnalysisTab(),
                _buildHairTryOnTab(),
                _buildChatSessionsTab(),
              ],
            ),
    );
  }

  Widget _buildErrorWidget() {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Icon(Icons.error_outline, size: 64, color: Colors.grey),
          const SizedBox(height: 16),
          Text(
            _errorMessage!,
            style: AppTextStyles.bodyMedium.copyWith(color: Colors.grey),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 16),
          ElevatedButton.icon(
            onPressed: _loadHistory,
            icon: const Icon(Icons.refresh),
            label: const Text('Thử lại'),
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.primary,
              foregroundColor: Colors.white,
            ),
          ),
        ],
      ),
    );
  }

  // ==========================================
  // FACE ANALYSIS TAB
  // ==========================================

  Widget _buildFaceAnalysisTab() {
    if (_faceAnalyses.isEmpty) {
      return _buildEmptyState(
        icon: Icons.face_retouching_natural,
        title: 'Chưa có kết quả phân tích',
        subtitle: 'Hãy thử tính năng Scan Khuôn Mặt để xem kết quả ở đây',
      );
    }

    return RefreshIndicator(
      onRefresh: _loadHistory,
      child: ListView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: _faceAnalyses.length,
        itemBuilder: (context, index) {
          final analysis = _faceAnalyses[index];
          return _FaceAnalysisCard(
            analysis: analysis,
            onDelete: () => _deleteFaceAnalysis(analysis.id),
          );
        },
      ),
    );
  }

  Future<void> _deleteFaceAnalysis(int id) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Xóa kết quả?'),
        content: const Text('Bạn có chắc muốn xóa kết quả phân tích này?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Hủy'),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(ctx, true),
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
            child: const Text('Xóa', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );

    if (confirmed == true) {
      final success = await _historyService.deleteFaceAnalysis(id);
      if (success && mounted) {
        setState(() {
          _faceAnalyses.removeWhere((a) => a.id == id);
        });
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(const SnackBar(content: Text('Đã xóa kết quả')));
      }
    }
  }

  // ==========================================
  // HAIR TRY-ON TAB
  // ==========================================

  Widget _buildHairTryOnTab() {
    if (_hairTryOns.isEmpty) {
      return _buildEmptyState(
        icon: Icons.auto_awesome,
        title: 'Chưa có kết quả thử tóc',
        subtitle: 'Hãy thử tính năng Thử Tóc Ảo để xem kết quả ở đây',
      );
    }

    return RefreshIndicator(
      onRefresh: _loadHistory,
      child: GridView.builder(
        padding: const EdgeInsets.all(16),
        gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
          crossAxisCount: 2,
          crossAxisSpacing: 12,
          mainAxisSpacing: 12,
          childAspectRatio: 0.75,
        ),
        itemCount: _hairTryOns.length,
        itemBuilder: (context, index) {
          final tryOn = _hairTryOns[index];
          return _HairTryOnCard(
            tryOn: tryOn,
            onSaveToggle: () => _toggleSaveHairTryOn(tryOn),
            onDelete: () => _deleteHairTryOn(tryOn.id),
          );
        },
      ),
    );
  }

  Future<void> _toggleSaveHairTryOn(HairTryOnHistory tryOn) async {
    final success = await _historyService.updateHairTryOnSave(
      tryOn.id,
      !tryOn.isSaved,
    );
    if (success) {
      _loadHistory();
    }
  }

  Future<void> _deleteHairTryOn(int id) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Xóa kết quả?'),
        content: const Text('Bạn có chắc muốn xóa kết quả thử tóc này?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Hủy'),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(ctx, true),
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
            child: const Text('Xóa', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );

    if (confirmed == true) {
      final success = await _historyService.deleteHairTryOn(id);
      if (success && mounted) {
        setState(() {
          _hairTryOns.removeWhere((h) => h.id == id);
        });
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(const SnackBar(content: Text('Đã xóa kết quả')));
      }
    }
  }

  // ==========================================
  // CHAT SESSIONS TAB
  // ==========================================

  Widget _buildChatSessionsTab() {
    if (_chatSessions.isEmpty) {
      return _buildEmptyState(
        icon: Icons.chat_bubble_outline,
        title: 'Chưa có cuộc trò chuyện',
        subtitle: 'Hãy thử tính năng Tư Vấn AI để xem lịch sử ở đây',
      );
    }

    return RefreshIndicator(
      onRefresh: _loadHistory,
      child: ListView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: _chatSessions.length,
        itemBuilder: (context, index) {
          final session = _chatSessions[index];
          return _ChatSessionCard(
            session: session,
            onTap: () => _openChatSession(session),
            onDelete: () => _deleteChatSession(session.id),
          );
        },
      ),
    );
  }

  void _openChatSession(ChatSessionHistory session) {
    // TODO: Navigate to chat screen with session loaded
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text('Mở cuộc trò chuyện: ${session.title ?? "Chat"}')),
    );
  }

  Future<void> _deleteChatSession(int id) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Xóa cuộc trò chuyện?'),
        content: const Text(
          'Toàn bộ tin nhắn trong cuộc trò chuyện này sẽ bị xóa.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Hủy'),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(ctx, true),
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
            child: const Text('Xóa', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );

    if (confirmed == true) {
      final success = await _historyService.deleteChatSession(id);
      if (success && mounted) {
        setState(() {
          _chatSessions.removeWhere((c) => c.id == id);
        });
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(const SnackBar(content: Text('Đã xóa cuộc trò chuyện')));
      }
    }
  }

  Widget _buildEmptyState({
    required IconData icon,
    required String title,
    required String subtitle,
  }) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(icon, size: 80, color: Colors.grey[300]),
            const SizedBox(height: 24),
            Text(
              title,
              style: AppTextStyles.titleMedium.copyWith(
                color: Colors.grey[600],
              ),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 8),
            Text(
              subtitle,
              style: AppTextStyles.bodySmall.copyWith(color: Colors.grey[500]),
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
    );
  }
}

// ==========================================
// CARD WIDGETS
// ==========================================

class _FaceAnalysisCard extends StatelessWidget {
  final FaceAnalysisHistory analysis;
  final VoidCallback onDelete;

  const _FaceAnalysisCard({required this.analysis, required this.onDelete});

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Row(
          children: [
            // Thumbnail
            Container(
              width: 70,
              height: 70,
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(8),
                color: AppColors.primary.withValues(alpha: 0.1),
              ),
              child: analysis.originalImageUrl.startsWith('data:image')
                  ? ClipRRect(
                      borderRadius: BorderRadius.circular(8),
                      child: Image.memory(
                        _decodeBase64Image(analysis.originalImageUrl),
                        fit: BoxFit.cover,
                      ),
                    )
                  : const Icon(
                      Icons.face_retouching_natural,
                      size: 35,
                      color: AppColors.primary,
                    ),
            ),
            const SizedBox(width: 16),
            // Info
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    analysis.faceShape ?? 'Không xác định',
                    style: AppTextStyles.titleMedium.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 4),
                  if (analysis.confidence != null)
                    Text(
                      'Độ chính xác: ${(analysis.confidence! * 100).toStringAsFixed(0)}%',
                      style: AppTextStyles.bodySmall.copyWith(
                        color: Colors.grey[600],
                      ),
                    ),
                  const SizedBox(height: 4),
                  Text(
                    _formatDate(analysis.createdAt),
                    style: AppTextStyles.bodySmall.copyWith(
                      color: Colors.grey[500],
                    ),
                  ),
                ],
              ),
            ),
            // Delete button
            IconButton(
              icon: const Icon(Icons.delete_outline, color: Colors.red),
              onPressed: onDelete,
            ),
          ],
        ),
      ),
    );
  }

  static _decodeBase64Image(String dataUrl) {
    final base64 = dataUrl.split(',').last;
    return Uri.parse('data:image/png;base64,$base64').data!.contentAsBytes();
  }

  String _formatDate(DateTime date) {
    return '${date.day}/${date.month}/${date.year} ${date.hour}:${date.minute.toString().padLeft(2, '0')}';
  }
}

class _HairTryOnCard extends StatelessWidget {
  final HairTryOnHistory tryOn;
  final VoidCallback onSaveToggle;
  final VoidCallback onDelete;

  const _HairTryOnCard({
    required this.tryOn,
    required this.onSaveToggle,
    required this.onDelete,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      clipBehavior: Clip.antiAlias,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          // Image
          Expanded(
            child: Stack(
              fit: StackFit.expand,
              children: [
                Container(
                  color: AppColors.primary.withValues(alpha: 0.1),
                  child:
                      tryOn.resultImageUrl != null &&
                          tryOn.resultImageUrl!.startsWith('data:image')
                      ? Image.memory(
                          _decodeBase64Image(tryOn.resultImageUrl!),
                          fit: BoxFit.cover,
                        )
                      : const Icon(
                          Icons.auto_awesome,
                          size: 50,
                          color: AppColors.primary,
                        ),
                ),
                // Save indicator
                Positioned(
                  top: 8,
                  right: 8,
                  child: GestureDetector(
                    onTap: onSaveToggle,
                    child: Container(
                      padding: const EdgeInsets.all(6),
                      decoration: BoxDecoration(
                        color: Colors.white.withValues(alpha: 0.9),
                        shape: BoxShape.circle,
                      ),
                      child: Icon(
                        tryOn.isSaved ? Icons.bookmark : Icons.bookmark_border,
                        color: tryOn.isSaved ? AppColors.primary : Colors.grey,
                        size: 20,
                      ),
                    ),
                  ),
                ),
              ],
            ),
          ),
          // Info
          Padding(
            padding: const EdgeInsets.all(8),
            child: Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        tryOn.hairStyleName ?? 'Kiểu tóc',
                        style: AppTextStyles.bodySmall.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                      ),
                      Text(
                        _formatDate(tryOn.createdAt),
                        style: AppTextStyles.bodySmall.copyWith(
                          color: Colors.grey[500],
                          fontSize: 10,
                        ),
                      ),
                    ],
                  ),
                ),
                IconButton(
                  icon: const Icon(Icons.delete_outline, size: 18),
                  color: Colors.red[300],
                  onPressed: onDelete,
                  padding: EdgeInsets.zero,
                  constraints: const BoxConstraints(
                    minWidth: 30,
                    minHeight: 30,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  static _decodeBase64Image(String dataUrl) {
    final base64 = dataUrl.split(',').last;
    return Uri.parse('data:image/png;base64,$base64').data!.contentAsBytes();
  }

  String _formatDate(DateTime date) {
    return '${date.day}/${date.month}';
  }
}

class _ChatSessionCard extends StatelessWidget {
  final ChatSessionHistory session;
  final VoidCallback onTap;
  final VoidCallback onDelete;

  const _ChatSessionCard({
    required this.session,
    required this.onTap,
    required this.onDelete,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(12),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Row(
            children: [
              // Icon
              Container(
                width: 50,
                height: 50,
                decoration: BoxDecoration(
                  color: _getSessionTypeColor().withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Icon(
                  _getSessionTypeIcon(),
                  color: _getSessionTypeColor(),
                  size: 24,
                ),
              ),
              const SizedBox(width: 16),
              // Info
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      session.title ?? 'Cuộc trò chuyện',
                      style: AppTextStyles.titleSmall.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const SizedBox(height: 4),
                    Row(
                      children: [
                        Icon(Icons.chat, size: 14, color: Colors.grey[500]),
                        const SizedBox(width: 4),
                        Text(
                          '${session.messageCount} tin nhắn',
                          style: AppTextStyles.bodySmall.copyWith(
                            color: Colors.grey[600],
                          ),
                        ),
                        const SizedBox(width: 12),
                        Icon(
                          Icons.access_time,
                          size: 14,
                          color: Colors.grey[500],
                        ),
                        const SizedBox(width: 4),
                        Text(
                          _formatDate(session.createdAt),
                          style: AppTextStyles.bodySmall.copyWith(
                            color: Colors.grey[500],
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
              // Delete button
              IconButton(
                icon: const Icon(Icons.delete_outline, color: Colors.red),
                onPressed: onDelete,
              ),
            ],
          ),
        ),
      ),
    );
  }

  Color _getSessionTypeColor() {
    switch (session.sessionType) {
      case 'HairStyle':
        return Colors.purple;
      case 'Product':
        return Colors.orange;
      case 'Service':
        return Colors.green;
      default:
        return AppColors.primary;
    }
  }

  IconData _getSessionTypeIcon() {
    switch (session.sessionType) {
      case 'HairStyle':
        return Icons.content_cut;
      case 'Product':
        return Icons.shopping_bag;
      case 'Service':
        return Icons.spa;
      default:
        return Icons.smart_toy;
    }
  }

  String _formatDate(DateTime date) {
    final now = DateTime.now();
    final diff = now.difference(date);

    if (diff.inDays == 0) {
      return 'Hôm nay';
    } else if (diff.inDays == 1) {
      return 'Hôm qua';
    } else if (diff.inDays < 7) {
      return '${diff.inDays} ngày trước';
    } else {
      return '${date.day}/${date.month}/${date.year}';
    }
  }
}
