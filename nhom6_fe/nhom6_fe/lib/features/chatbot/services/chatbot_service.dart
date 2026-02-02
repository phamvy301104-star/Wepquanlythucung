import 'dart:developer' as developer;
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:google_generative_ai/google_generative_ai.dart';
import '../../face_analysis/services/face_analysis_service.dart';
import '../../../core/services/product_service.dart';
import '../../../core/services/service_service.dart';

/// Ume-Stylist AI Chatbot Service
/// Sử dụng Google Gemini API (Free tier: 15 requests/min)
class ChatbotService {
  late final GenerativeModel _model;
  late final ChatSession _chatSession;

  String? _currentFaceShape;
  bool _isInitialized = false;

  // Dynamic data from database
  String _servicesData = '';
  String _productsData = '';

  /// Logger name để dễ filter trong console
  static const String _logName = 'ChatbotService';

  /// Build dynamic system prompt with real data
  String _buildSystemPrompt() {
    return '''
# Vai trò (Role)
Bạn là **Ume-Stylist** - Trợ lý ảo AI chuyên biệt của UME Salon. Bạn CHỈ tư vấn về sản phẩm, dịch vụ và thông tin liên quan đến UME Salon.

# Quy tắc QUAN TRỌNG - Phải tuân thủ NGHIÊM NGẶT:
1. **CHỈ trả lời về:** Sản phẩm/Dịch vụ/Giá cả/Đặt lịch của UME Salon
2. **KHÔNG trả lời về:** Chính trị, Y tế, Pháp luật, Tin tức, Bóng đá, Giải trí, hoặc BẤT KỲ chủ đề nào NGOÀI UME Salon
3. **Khi khách hỏi chủ đề ngoài salon:** Trả lời lịch sự: "Xin lỗi bro, mình là trợ lý chuyên biệt của UME Salon nên chỉ có thể giúp bro về các vấn đề liên quan đến tóc, sản phẩm và dịch vụ của salon thôi nhé! 💈 Bro cần tư vấn gì về UME không? 😊"

# Phong cách
- **Giọng văn:** Gen Z, trẻ trung, thân thiện nhưng chuyên nghiệp
- **Emoji:** Sử dụng phù hợp (💈, 💇‍♂️, ✨, 🔥, ✂️, 😎)
- **Thái độ:** Nhiệt tình, lắng nghe, không thảo mai

# Dữ liệu THỰC TẾ từ Database

## DỊCH VỤ CỦA UME SALON:
$_servicesData

## SẢN PHẨM CỦA UME SALON:
$_productsData

# Kỹ năng tư vấn

## 1. Tư vấn Kiểu tóc
- Hỏi về khuôn mặt khách hàng hoặc yêu cầu Scan
- Đề xuất 2-3 kiểu tóc phù hợp với giải thích
- Gợi ý tính năng "Thử Tóc Ảo" để preview

## 2. Tư vấn Sản phẩm
- Hỏi về chất tóc (Cứng/Mềm/Dầu/Khô)
- Giới thiệu sản phẩm phù hợp từ danh sách thực tế ở trên
- Không đề xuất sản phẩm không có trong database

## 3. Đặt lịch (QUAN TRỌNG)
**Khi khách hàng muốn đặt lịch, BẮT BUỘC thu thập ĐỦ 7 thông tin sau:**
1. **Họ và tên:** (VD: Nguyễn Văn A)
2. **Email:** (VD: example@gmail.com)
3. **Số điện thoại:** (VD: 0912345678)
4. **Đi một mình hay nhóm:** (VD: 1 người / 3 người)
5. **Dịch vụ cần đặt:** (Chọn từ danh sách dịch vụ thực tế ở trên)
6. **Ngày và giờ mong muốn:** (VD: 14h ngày 15/01/2025)
7. **Chọn thợ cắt:** (Nếu khách không chọn, để "Thợ bất kỳ")

**Format xác nhận đặt lịch:**
```
═══════════════════════════
✂️ **XÁC NHẬN LỊCH HẸN** ✂️
═══════════════════════════
👤 **Họ tên:** [Tên]
📧 **Email:** [Email]
📱 **SĐT:** [SĐT]
👥 **Số người:** [1 / Nhóm X người]
💈 **Dịch vụ:** [Tên dịch vụ]
📅 **Thời gian:** [Giờ, Ngày]
✂️ **Thợ cắt:** [Tên thợ / Bất kỳ]
💰 **Tạm tính:** [Giá]
═══════════════════════════
Đã ghi nhận! Tới giờ bro ghé salon nhé! 👋✨
```

## 4. Xử lý câu hỏi ngoài phạm vi
- Từ chối lịch sự với mẫu câu: "Xin lỗi bro, mình là trợ lý chuyên biệt của UME Salon nên chỉ có thể giúp bro về các vấn đề liên quan đến tóc, sản phẩm và dịch vụ của salon thôi nhé! 💈"
    - Nếu khách đặt lịch uốn tóc -> Gợi ý thêm dầu gội giữ màu/phục hồi.
    - *"Bro uốn tóc xong nhớ dùng thêm tinh dầu dưỡng để tóc luôn mướt, không bị khô xơ nhé, đang có deal hời lắm á! 💦"*

## 5. Xử lý sự cố & Từ chối (Handling Objections)
- **Nếu khách phàn nàn (Cắt xấu, thái độ nhân viên...):**
    - Chuyển tông giọng sang nghiêm túc, chân thành, xin lỗi ngay lập tức.
    - *"Ume chân thành xin lỗi vì trải nghiệm chưa vui này của bro 🙏. Mình đã ghi nhận và báo ngay cho Quản lý. Bro cho mình xin SĐT để Store Manager liên hệ giải quyết trực tiếp trong 30 phút nữa nhé!"*
- **Nếu khách hỏi chuyện ngoài lề (Tình cảm, bài tập...):**
    - *"Ui, cái này thì Ume chịu rồi 😅. Nhưng nếu hỏi làm sao để đẹp trai hơn người yêu cũ thì Ume cân được nhé! Quay lại chuyện tóc tai đi nào 😎✂️"*

# Định dạng đầu ra (Output Format)
- Ưu tiên ngắn gọn, xuống dòng thoáng mắt.
- Các thông tin quan trọng (**Giờ hẹn, Giá tiền, Tên kiểu tóc**) phải in đậm.
- Luôn kết thúc bằng một câu hỏi mở (Call to Action) để duy trì hội thoại. Ví dụ: *"Bro thấy kiểu này sao?", "Chốt giờ này nhé?", "Còn thắc mắc gì nữa không?"*

- Không bịa đặt thông tin về sản phẩm/dịch vụ không có trong database.
- Không tranh cãi với khách hàng.
- Luôn duy trì phong cách thân thiện, chuyên nghiệp.
''';
  }

  /// Fetch services data from API
  Future<void> _fetchServicesData() async {
    try {
      developer.log('📡 Fetching services data...', name: _logName);

      final response = await ServiceService().fetchServices();

      if (response.isEmpty) {
        _servicesData = 'Hiện chưa có dịch vụ nào trong database.';
        return;
      }

      final buffer = StringBuffer();
      for (var service in response) {
        buffer.writeln('- **${service['name']}**');
        buffer.writeln('  • Giá: ${_formatPrice(service['price'])}');
        if (service['originalPrice'] != null &&
            service['originalPrice'] > service['price']) {
          buffer.writeln(
            '  • Giá gốc: ${_formatPrice(service['originalPrice'])} (Giảm giá!)',
          );
        }
        if (service['shortDescription'] != null) {
          buffer.writeln('  • ${service['shortDescription']}');
        }
        buffer.writeln('  • Thời gian: ${service['durationMinutes']} phút');
        buffer.writeln();
      }

      _servicesData = buffer.toString();
      developer.log(
        '✅ Services data loaded: ${response.length} items',
        name: _logName,
      );
    } catch (e) {
      developer.log('⚠️ Error fetching services: $e', name: _logName);
      _servicesData = 'Không thể tải dữ liệu dịch vụ. Vui lòng thử lại sau.';
    }
  }

  /// Fetch products data from API
  Future<void> _fetchProductsData() async {
    try {
      developer.log('📡 Fetching products data...', name: _logName);

      final products = await ProductService().getProducts();

      if (products.isEmpty) {
        _productsData = 'Hiện chưa có sản phẩm nào trong database.';
        return;
      }

      final buffer = StringBuffer();
      for (var product in products.take(20)) {
        // Limit to 20 products to avoid token limit
        buffer.writeln('- **${product.name}**');
        buffer.writeln('  • Giá: ${_formatPrice(product.price)}');
        if (product.originalPrice != null &&
            product.originalPrice! > product.price) {
          buffer.writeln(
            '  • Giá gốc: ${_formatPrice(product.originalPrice!)} (Giảm ${product.discountPercentage?.toStringAsFixed(0)}%)',
          );
        }
        if (product.shortDescription != null) {
          buffer.writeln('  • ${product.shortDescription}');
        }
        buffer.writeln();
      }

      _productsData = buffer.toString();
      developer.log(
        '✅ Products data loaded: ${products.length} items',
        name: _logName,
      );
    } catch (e) {
      developer.log('⚠️ Error fetching products: $e', name: _logName);
      _productsData = 'Không thể tải dữ liệu sản phẩm. Vui lòng thử lại sau.';
    }
  }

  /// Format price to VND currency
  String _formatPrice(dynamic price) {
    if (price == null) return '0đ';
    final priceNum = price is num
        ? price
        : double.tryParse(price.toString()) ?? 0;
    return '${priceNum.toStringAsFixed(0).replaceAllMapped(RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'), (Match m) => '${m[1]},')}đ';
  }

  /// Khởi tạo service với API key
  Future<void> initialize() async {
    if (_isInitialized) {
      developer.log(
        '🔄 ChatbotService đã khởi tạo rồi, bỏ qua initialize',
        name: _logName,
      );
      return;
    }

    developer.log('🚀 Bắt đầu khởi tạo ChatbotService...', name: _logName);

    // Fetch dynamic data first
    await Future.wait([_fetchServicesData(), _fetchProductsData()]);

    final apiKey = dotenv.env['GEMINI_API_KEY'];

    // Detailed logging cho API key
    if (apiKey == null) {
      developer.log(
        '❌ GEMINI_API_KEY không tồn tại trong .env file!',
        name: _logName,
        error: 'API key is null',
      );
      throw Exception('GEMINI_API_KEY không tồn tại trong file .env');
    }

    if (apiKey.isEmpty) {
      developer.log(
        '❌ GEMINI_API_KEY rỗng trong .env file!',
        name: _logName,
        error: 'API key is empty',
      );
      throw Exception('GEMINI_API_KEY rỗng trong file .env');
    }

    if (apiKey == 'YOUR_GEMINI_API_KEY_HERE') {
      developer.log(
        '❌ GEMINI_API_KEY chưa được thay thế (còn giá trị mặc định)!',
        name: _logName,
        error: 'API key is placeholder',
      );
      throw Exception('GEMINI_API_KEY chưa được cấu hình trong file .env');
    }

    // Mask API key để log an toàn (chỉ hiện 8 ký tự đầu và 4 ký tự cuối)
    final maskedKey = apiKey.length > 12
        ? '${apiKey.substring(0, 8)}...${apiKey.substring(apiKey.length - 4)}'
        : '${apiKey.substring(0, 4)}...';

    developer.log(
      '🔑 API Key loaded: $maskedKey (length: ${apiKey.length})',
      name: _logName,
    );

    try {
      _model = GenerativeModel(
        model: 'gemini-2.5-flash', // Model mới nhất, MIỄN PHÍ, nhanh hơn
        apiKey: apiKey,
        systemInstruction: Content.text(_buildSystemPrompt()),
        generationConfig: GenerationConfig(
          temperature: 0.8,
          topK: 40,
          topP: 0.95,
          maxOutputTokens: 1024,
        ),
      );

      _chatSession = _model.startChat();
      _isInitialized = true;

      developer.log(
        '✅ ChatbotService khởi tạo thành công với dynamic data!',
        name: _logName,
      );
    } catch (e, stackTrace) {
      developer.log(
        '❌ Lỗi khi khởi tạo GenerativeModel',
        name: _logName,
        error: e,
        stackTrace: stackTrace,
      );
      rethrow;
    }
  }

  /// Set thông tin dáng mặt từ Face Analysis
  void setFaceShapeContext(FaceAnalysisResult result) {
    _currentFaceShape =
        '''
[THÔNG TIN KHUÔN MẶT KHÁCH HÀNG - ĐÃ SCAN]
- Dáng mặt: ${result.faceShape.displayName}
- Mô tả: ${result.faceShape.description}
- Độ chính xác AI: ${(result.confidence * 100).toInt()}%
- Kiểu tóc phù hợp: ${result.recommendations.join(', ')}

Hãy sử dụng thông tin này để tư vấn cá nhân hóa cho khách!
''';
  }

  /// Gửi tin nhắn và nhận phản hồi
  Future<String> sendMessage(String message) async {
    if (!_isInitialized) {
      developer.log(
        '⚠️ Service chưa initialize, đang khởi tạo...',
        name: _logName,
      );
      await initialize();
    }

    developer.log(
      '📤 Gửi message: ${message.substring(0, message.length > 50 ? 50 : message.length)}...',
      name: _logName,
    );

    try {
      // Nếu có context về face shape, thêm vào tin nhắn đầu
      String fullMessage = message;
      if (_currentFaceShape != null) {
        fullMessage = '$_currentFaceShape\n\nTin nhắn khách: $message';
        _currentFaceShape = null; // Clear sau khi dùng
        developer.log(
          '🎭 Đã thêm face shape context vào message',
          name: _logName,
        );
      }

      final response = await _chatSession.sendMessage(
        Content.text(fullMessage),
      );

      final responseText =
          response.text ?? 'Ume không hiểu, bro nói rõ hơn được không? 🤔';

      developer.log(
        '📥 Nhận response: ${responseText.substring(0, responseText.length > 100 ? 100 : responseText.length)}...',
        name: _logName,
      );

      return responseText;
    } catch (e, stackTrace) {
      // Detailed error logging với error type
      developer.log(
        '❌ LỖI khi gửi message',
        name: _logName,
        error: e,
        stackTrace: stackTrace,
      );

      // Phân loại lỗi và trả về message phù hợp
      final errorMessage = e.toString().toLowerCase();

      if (errorMessage.contains('api key not valid') ||
          errorMessage.contains('invalid api key') ||
          errorMessage.contains('api_key_invalid')) {
        developer.log(
          '🔑 LỖI: API Key không hợp lệ!',
          name: _logName,
          error: 'Chi tiết: $e',
        );
        return '''
❌ **API Key không hợp lệ!**

**Cách fix:**
1. Truy cập: https://aistudio.google.com/app/apikey
2. Tạo API key mới
3. Copy key và paste vào file `.env`:
   `GEMINI_API_KEY=your_new_key_here`
4. Restart app

**Chi tiết lỗi:** API key not valid. Please pass a valid API key.
''';
      } else if (errorMessage.contains('not found') ||
          errorMessage.contains('model') ||
          errorMessage.contains('404')) {
        developer.log(
          '🤖 LỖI: Model không tồn tại hoặc đã deprecated',
          name: _logName,
        );
        return '''
❌ **Model không khả dụng!**

Lỗi: Model "gemini-1.5-flash" đã deprecated.

**Đang tự động chuyển sang model mới...**
Hãy restart app để sử dụng model "gemini-2.5-flash" (MIỄN PHÍ, nhanh hơn).

Hoặc xem FIX_CHATBOT_API_KEY.md để biết thêm.
''';
      } else if (errorMessage.contains('quota') ||
          errorMessage.contains('rate') ||
          errorMessage.contains('429')) {
        developer.log('⏱️ LỖI: Vượt quá rate limit', name: _logName);
        return 'Ume đang bận xíu (hết quota tạm thời), bro đợi 1 phút rồi thử lại nhé! 😅⏱️';
      } else if (errorMessage.contains('network') ||
          errorMessage.contains('connection') ||
          errorMessage.contains('timeout')) {
        developer.log('🌐 LỖI: Vấn đề kết nối mạng', name: _logName);
        return 'Có vấn đề về kết nối mạng. Bro check lại WiFi/4G rồi thử lại nhé! 📶';
      } else {
        // Generic error với chi tiết để debug
        return 'Có lỗi xảy ra: $e\n\n💡 Bro hãy check console để xem chi tiết lỗi nhé!';
      }
    }
  }

  /// Reset conversation
  void resetConversation() {
    developer.log('🔄 Reset conversation', name: _logName);
    if (_isInitialized) {
      _chatSession = _model.startChat();
    }
    _currentFaceShape = null;
  }

  /// Lấy tin nhắn chào đón
  String getWelcomeMessage() {
    return '''
Yo! Ume đây! 💈✨

Mình là **Ume-Stylist** - Trợ lý AI chuyên biệt của UME Salon!

Mình có thể giúp bro:
• 💇‍♂️ **Tư vấn kiểu tóc** phù hợp với khuôn mặt
• 📅 **Đặt lịch** cắt tóc (thu thập đủ 7 thông tin)
• 🧴 **Tư vấn sản phẩm** sáp/gôm/dầu gội
• 💰 **Xem bảng giá** dịch vụ

**Lưu ý:** Mình CHỈ tư vấn về UME Salon thôi nhé! Các chủ đề khác mình không rành lắm 😅

**Pro tip:** Dùng tính năng **"Scan Khuôn Mặt"** để Ume tư vấn chuẩn xác hơn! 📸

Bro cần gì nè? 😎
''';
  }

  /// Kiểm tra đã khởi tạo chưa
  bool get isInitialized => _isInitialized;
}
