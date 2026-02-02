import 'dart:convert';
import 'dart:developer' as developer;
import '../services/api_client.dart';

/// Service để lưu và lấy lịch sử AI từ backend
class AiHistoryService {
  final ApiClient _apiClient;

  AiHistoryService({ApiClient? apiClient})
    : _apiClient = apiClient ?? ApiClient();

  // ==========================================
  // FACE ANALYSIS
  // ==========================================

  /// Lưu kết quả phân tích khuôn mặt lên server
  Future<FaceAnalysisHistory?> saveFaceAnalysis({
    required String imageBase64,
    required String faceShape,
    required double confidence,
    required String description,
    required List<String> recommendedHairstyles,
    String? measurementsJson,
  }) async {
    try {
      final response = await _apiClient.postWithAuth(
        '/api/AiFeatures/face-analysis',
        body: {
          'originalImageUrl': 'data:image/jpeg;base64,$imageBase64',
          'faceShape': faceShape,
          'confidence': confidence,
          'description': description,
          'recommendedHairstyles': recommendedHairstyles,
          'measurementsJson': measurementsJson,
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return FaceAnalysisHistory.fromJson(data);
      }

      developer.log(
        'Error saving face analysis: ${response.statusCode}',
        name: 'AiHistoryService',
      );
      return null;
    } catch (e) {
      developer.log(
        'Exception saving face analysis: $e',
        name: 'AiHistoryService',
        error: e,
      );
      return null;
    }
  }

  /// Lấy lịch sử phân tích khuôn mặt
  Future<List<FaceAnalysisHistory>> getFaceAnalysisHistory({
    int page = 1,
    int pageSize = 10,
  }) async {
    try {
      final response = await _apiClient.getWithAuth(
        '/api/AiFeatures/face-analysis/history?page=$page&pageSize=$pageSize',
      );

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return data.map((json) => FaceAnalysisHistory.fromJson(json)).toList();
      }

      return [];
    } catch (e) {
      developer.log(
        'Exception getting face analysis history: $e',
        name: 'AiHistoryService',
        error: e,
      );
      return [];
    }
  }

  /// Xóa kết quả phân tích
  Future<bool> deleteFaceAnalysis(int id) async {
    try {
      final response = await _apiClient.deleteWithAuth(
        '/api/AiFeatures/face-analysis/$id',
      );
      return response.statusCode == 200;
    } catch (e) {
      developer.log(
        'Exception deleting face analysis: $e',
        name: 'AiHistoryService',
        error: e,
      );
      return false;
    }
  }

  // ==========================================
  // HAIR TRY-ON
  // ==========================================

  /// Lưu kết quả thử tóc ảo lên server
  Future<HairTryOnHistory?> saveHairTryOn({
    required String faceImageBase64,
    required String hairStyleImageBase64,
    String? resultImageBase64,
    String? hairStyleName,
    String aiModel = 'HairFastGAN',
    bool isSaved = false,
  }) async {
    try {
      final response = await _apiClient.postWithAuth(
        '/api/AiFeatures/hair-tryon',
        body: {
          'faceImageUrl': 'data:image/jpeg;base64,$faceImageBase64',
          'hairStyleImageUrl': 'data:image/jpeg;base64,$hairStyleImageBase64',
          'resultImageUrl': resultImageBase64 != null
              ? 'data:image/jpeg;base64,$resultImageBase64'
              : null,
          'hairStyleName': hairStyleName,
          'aiModel': aiModel,
          'isSaved': isSaved,
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return HairTryOnHistory.fromJson(data);
      }

      developer.log(
        'Error saving hair try-on: ${response.statusCode}',
        name: 'AiHistoryService',
      );
      return null;
    } catch (e) {
      developer.log(
        'Exception saving hair try-on: $e',
        name: 'AiHistoryService',
        error: e,
      );
      return null;
    }
  }

  /// Lấy lịch sử thử tóc ảo
  Future<List<HairTryOnHistory>> getHairTryOnHistory({
    int page = 1,
    int pageSize = 10,
  }) async {
    try {
      final response = await _apiClient.getWithAuth(
        '/api/AiFeatures/hair-tryon/history?page=$page&pageSize=$pageSize',
      );

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return data.map((json) => HairTryOnHistory.fromJson(json)).toList();
      }

      return [];
    } catch (e) {
      developer.log(
        'Exception getting hair try-on history: $e',
        name: 'AiHistoryService',
        error: e,
      );
      return [];
    }
  }

  /// Cập nhật trạng thái lưu
  Future<bool> updateHairTryOnSave(int id, bool isSaved) async {
    try {
      final response = await _apiClient.patchWithAuth(
        '/api/AiFeatures/hair-tryon/$id/save',
        body: isSaved,
      );
      return response.statusCode == 200;
    } catch (e) {
      developer.log(
        'Exception updating hair try-on: $e',
        name: 'AiHistoryService',
        error: e,
      );
      return false;
    }
  }

  /// Xóa kết quả thử tóc
  Future<bool> deleteHairTryOn(int id) async {
    try {
      final response = await _apiClient.deleteWithAuth(
        '/api/AiFeatures/hair-tryon/$id',
      );
      return response.statusCode == 200;
    } catch (e) {
      developer.log(
        'Exception deleting hair try-on: $e',
        name: 'AiHistoryService',
        error: e,
      );
      return false;
    }
  }

  // ==========================================
  // CHAT SESSIONS
  // ==========================================

  /// Tạo phiên chat mới
  Future<ChatSessionHistory?> createChatSession({
    String? title,
    String sessionType = 'General',
  }) async {
    try {
      final response = await _apiClient.postWithAuth(
        '/api/AiFeatures/chat/sessions',
        body: {'title': title, 'sessionType': sessionType},
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return ChatSessionHistory.fromJson(data);
      }

      return null;
    } catch (e) {
      developer.log(
        'Exception creating chat session: $e',
        name: 'AiHistoryService',
        error: e,
      );
      return null;
    }
  }

  /// Lưu tin nhắn vào session
  Future<ChatMessageHistory?> saveChatMessage({
    required int sessionId,
    required String content,
    String role = 'user',
    String? imageUrl,
  }) async {
    try {
      final response = await _apiClient.postWithAuth(
        '/api/AiFeatures/chat/sessions/$sessionId/messages?role=$role',
        body: {'content': content, 'imageUrl': imageUrl},
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return ChatMessageHistory.fromJson(data);
      }

      return null;
    } catch (e) {
      developer.log(
        'Exception saving chat message: $e',
        name: 'AiHistoryService',
        error: e,
      );
      return null;
    }
  }

  /// Lấy danh sách phiên chat
  Future<List<ChatSessionHistory>> getChatSessions({
    int page = 1,
    int pageSize = 20,
  }) async {
    try {
      final response = await _apiClient.getWithAuth(
        '/api/AiFeatures/chat/sessions?page=$page&pageSize=$pageSize',
      );

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return data.map((json) => ChatSessionHistory.fromJson(json)).toList();
      }

      return [];
    } catch (e) {
      developer.log(
        'Exception getting chat sessions: $e',
        name: 'AiHistoryService',
        error: e,
      );
      return [];
    }
  }

  /// Lấy chi tiết phiên chat với tin nhắn
  Future<ChatSessionHistory?> getChatSessionDetail(int sessionId) async {
    try {
      final response = await _apiClient.getWithAuth(
        '/api/AiFeatures/chat/sessions/$sessionId',
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return ChatSessionHistory.fromJson(data);
      }

      return null;
    } catch (e) {
      developer.log(
        'Exception getting chat session detail: $e',
        name: 'AiHistoryService',
        error: e,
      );
      return null;
    }
  }

  /// Xóa phiên chat
  Future<bool> deleteChatSession(int sessionId) async {
    try {
      final response = await _apiClient.deleteWithAuth(
        '/api/AiFeatures/chat/sessions/$sessionId',
      );
      return response.statusCode == 200;
    } catch (e) {
      developer.log(
        'Exception deleting chat session: $e',
        name: 'AiHistoryService',
        error: e,
      );
      return false;
    }
  }

  // ==========================================
  // COMBINED HISTORY
  // ==========================================

  /// Lấy toàn bộ lịch sử AI
  Future<AiHistoryResponse?> getAllHistory({int limit = 10}) async {
    try {
      final response = await _apiClient.getWithAuth(
        '/api/AiFeatures/history?limit=$limit',
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return AiHistoryResponse.fromJson(data);
      }

      return null;
    } catch (e) {
      developer.log(
        'Exception getting all history: $e',
        name: 'AiHistoryService',
        error: e,
      );
      return null;
    }
  }
}

// ==========================================
// DATA MODELS
// ==========================================

class FaceAnalysisHistory {
  final int id;
  final String originalImageUrl;
  final String? faceShape;
  final double? confidence;
  final String? description;
  final List<String> recommendedHairstyles;
  final DateTime createdAt;

  FaceAnalysisHistory({
    required this.id,
    required this.originalImageUrl,
    this.faceShape,
    this.confidence,
    this.description,
    this.recommendedHairstyles = const [],
    required this.createdAt,
  });

  factory FaceAnalysisHistory.fromJson(Map<String, dynamic> json) {
    return FaceAnalysisHistory(
      id: json['id'] ?? 0,
      originalImageUrl: json['originalImageUrl'] ?? '',
      faceShape: json['faceShape'],
      confidence: json['confidence']?.toDouble(),
      description: json['description'],
      recommendedHairstyles: json['recommendedHairstyles'] != null
          ? List<String>.from(json['recommendedHairstyles'])
          : [],
      createdAt: json['createdAt'] != null
          ? DateTime.parse(json['createdAt'])
          : DateTime.now(),
    );
  }
}

class HairTryOnHistory {
  final int id;
  final String faceImageUrl;
  final String hairStyleImageUrl;
  final String? resultImageUrl;
  final String? hairStyleName;
  final String? aiModel;
  final bool isSaved;
  final DateTime createdAt;

  HairTryOnHistory({
    required this.id,
    required this.faceImageUrl,
    required this.hairStyleImageUrl,
    this.resultImageUrl,
    this.hairStyleName,
    this.aiModel,
    this.isSaved = false,
    required this.createdAt,
  });

  factory HairTryOnHistory.fromJson(Map<String, dynamic> json) {
    return HairTryOnHistory(
      id: json['id'] ?? 0,
      faceImageUrl: json['faceImageUrl'] ?? '',
      hairStyleImageUrl: json['hairStyleImageUrl'] ?? '',
      resultImageUrl: json['resultImageUrl'],
      hairStyleName: json['hairStyleName'],
      aiModel: json['aiModel'],
      isSaved: json['isSaved'] ?? false,
      createdAt: json['createdAt'] != null
          ? DateTime.parse(json['createdAt'])
          : DateTime.now(),
    );
  }
}

class ChatSessionHistory {
  final int id;
  final String? title;
  final String sessionType;
  final int messageCount;
  final DateTime createdAt;
  final DateTime? lastMessageAt;
  final List<ChatMessageHistory> messages;

  ChatSessionHistory({
    required this.id,
    this.title,
    this.sessionType = 'General',
    this.messageCount = 0,
    required this.createdAt,
    this.lastMessageAt,
    this.messages = const [],
  });

  factory ChatSessionHistory.fromJson(Map<String, dynamic> json) {
    return ChatSessionHistory(
      id: json['id'] ?? 0,
      title: json['title'],
      sessionType: json['sessionType'] ?? 'General',
      messageCount: json['messageCount'] ?? 0,
      createdAt: json['createdAt'] != null
          ? DateTime.parse(json['createdAt'])
          : DateTime.now(),
      lastMessageAt: json['lastMessageAt'] != null
          ? DateTime.parse(json['lastMessageAt'])
          : null,
      messages: json['messages'] != null
          ? (json['messages'] as List)
                .map((m) => ChatMessageHistory.fromJson(m))
                .toList()
          : [],
    );
  }
}

class ChatMessageHistory {
  final int id;
  final String content;
  final String role;
  final String? imageUrl;
  final DateTime createdAt;

  ChatMessageHistory({
    required this.id,
    required this.content,
    this.role = 'user',
    this.imageUrl,
    required this.createdAt,
  });

  factory ChatMessageHistory.fromJson(Map<String, dynamic> json) {
    return ChatMessageHistory(
      id: json['id'] ?? 0,
      content: json['content'] ?? '',
      role: json['role'] ?? 'user',
      imageUrl: json['imageUrl'],
      createdAt: json['createdAt'] != null
          ? DateTime.parse(json['createdAt'])
          : DateTime.now(),
    );
  }
}

class AiHistoryResponse {
  final List<FaceAnalysisHistory> faceAnalyses;
  final List<HairTryOnHistory> hairTryOns;
  final List<ChatSessionHistory> chatSessions;

  AiHistoryResponse({
    this.faceAnalyses = const [],
    this.hairTryOns = const [],
    this.chatSessions = const [],
  });

  factory AiHistoryResponse.fromJson(Map<String, dynamic> json) {
    return AiHistoryResponse(
      faceAnalyses: json['faceAnalyses'] != null
          ? (json['faceAnalyses'] as List)
                .map((f) => FaceAnalysisHistory.fromJson(f))
                .toList()
          : [],
      hairTryOns: json['hairTryOns'] != null
          ? (json['hairTryOns'] as List)
                .map((h) => HairTryOnHistory.fromJson(h))
                .toList()
          : [],
      chatSessions: json['chatSessions'] != null
          ? (json['chatSessions'] as List)
                .map((c) => ChatSessionHistory.fromJson(c))
                .toList()
          : [],
    );
  }
}
