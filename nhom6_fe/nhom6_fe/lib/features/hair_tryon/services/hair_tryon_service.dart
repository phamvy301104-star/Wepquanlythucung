import 'dart:convert';
import 'dart:io';
import 'dart:typed_data';
import 'package:http/http.dart' as http;
import 'package:path_provider/path_provider.dart';

/// Hair Try-On Service
/// Sử dụng HuggingFace Spaces API để ghép tóc ảo
/// HairFastGAN: https://huggingface.co/spaces/AIRI-Institute/HairFastGAN
class HairTryOnService {
  // HuggingFace Gradio API endpoint
  static const String _hairFastGanApi =
      'https://airi-institute-hairfastgan.hf.space/api/predict';

  // Alternative: Replicate API (nếu HuggingFace chậm)
  // static const String _replicateApi = 'https://api.replicate.com/v1/predictions';

  /// Thử kiểu tóc mới bằng HairFastGAN
  /// [faceImage] - Ảnh khuôn mặt người dùng
  /// [hairStyleImage] - Ảnh kiểu tóc muốn thử
  /// Returns: File ảnh kết quả hoặc null nếu lỗi
  Future<HairTryOnResult> tryHairStyle({
    required File faceImage,
    required File hairStyleImage,
  }) async {
    try {
      // Convert images to base64
      final faceBytes = await faceImage.readAsBytes();
      final hairBytes = await hairStyleImage.readAsBytes();

      final faceBase64 = base64Encode(faceBytes);
      final hairBase64 = base64Encode(hairBytes);

      // Call HuggingFace Gradio API
      final response = await http
          .post(
            Uri.parse(_hairFastGanApi),
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode({
              'data': [
                'data:image/jpeg;base64,$faceBase64', // Face image
                'data:image/jpeg;base64,$hairBase64', // Hair reference
                'data:image/jpeg;base64,$hairBase64', // Shape reference (same as hair)
                'data:image/jpeg;base64,$hairBase64', // Color reference (same as hair)
              ],
            }),
          )
          .timeout(
            const Duration(seconds: 120), // Timeout 2 phút
            onTimeout: () {
              throw TimeoutException('Request timed out. Vui lòng thử lại.');
            },
          );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);

        // Extract result image from response
        if (data['data'] != null && data['data'].isNotEmpty) {
          final resultData = data['data'][0];

          // Parse base64 from data URL
          String base64Image;
          if (resultData is String && resultData.startsWith('data:image')) {
            base64Image = resultData.split(',')[1];
          } else if (resultData is Map && resultData['data'] != null) {
            base64Image = resultData['data'];
          } else {
            throw Exception('Unexpected response format');
          }

          // Decode và save to file
          final bytes = base64Decode(base64Image);
          final resultFile = await _saveResultImage(bytes);

          return HairTryOnResult(
            success: true,
            resultImage: resultFile,
            message: 'Ghép tóc thành công! 🎉',
          );
        } else {
          throw Exception('No result data in response');
        }
      } else if (response.statusCode == 503) {
        // Model đang loading
        return HairTryOnResult(
          success: false,
          message:
              'AI model đang khởi động (cold start). Vui lòng đợi 30s và thử lại! 🔄',
          isModelLoading: true,
        );
      } else {
        throw Exception('API Error: ${response.statusCode} - ${response.body}');
      }
    } on TimeoutException {
      return HairTryOnResult(
        success: false,
        message:
            'Quá thời gian chờ. HuggingFace có thể đang bận. Thử lại sau nhé! ⏰',
      );
    } catch (e) {
      return HairTryOnResult(success: false, message: 'Lỗi: $e');
    }
  }

  /// Alternative method: Gọi Gradio API theo cách khác
  Future<HairTryOnResult> tryHairStyleGradio({
    required File faceImage,
    required File hairStyleImage,
  }) async {
    try {
      // Step 1: Upload files
      final faceBytes = await faceImage.readAsBytes();
      final hairBytes = await hairStyleImage.readAsBytes();

      // Create multipart request
      final request = http.MultipartRequest(
        'POST',
        Uri.parse('https://airi-institute-hairfastgan.hf.space/run/predict'),
      );

      request.files.add(
        http.MultipartFile.fromBytes('files', faceBytes, filename: 'face.jpg'),
      );
      request.files.add(
        http.MultipartFile.fromBytes('files', hairBytes, filename: 'hair.jpg'),
      );

      final response = await request.send().timeout(
        const Duration(seconds: 120),
      );

      if (response.statusCode == 200) {
        final responseBody = await response.stream.bytesToString();
        final data = jsonDecode(responseBody);

        // Process response similar to above
        if (data['data'] != null && data['data'].isNotEmpty) {
          final resultData = data['data'][0];
          final base64Image = resultData.toString().split(',')[1];
          final bytes = base64Decode(base64Image);
          final resultFile = await _saveResultImage(bytes);

          return HairTryOnResult(
            success: true,
            resultImage: resultFile,
            message: 'Ghép tóc thành công! 🎉',
          );
        }
      }

      return HairTryOnResult(
        success: false,
        message: 'Không thể xử lý ảnh. Thử lại sau!',
      );
    } catch (e) {
      return HairTryOnResult(success: false, message: 'Lỗi: $e');
    }
  }

  /// Save result image to cache directory
  Future<File> _saveResultImage(Uint8List bytes) async {
    final tempDir = await getTemporaryDirectory();
    final timestamp = DateTime.now().millisecondsSinceEpoch;
    final resultFile = File('${tempDir.path}/hair_result_$timestamp.jpg');
    await resultFile.writeAsBytes(bytes);
    return resultFile;
  }

  /// Get list of preset hairstyle images
  /// Trả về URL các kiểu tóc mẫu để user chọn
  static List<HairStylePreset> getPresetHairstyles() {
    return [
      HairStylePreset(
        id: 'side_part',
        name: 'Side Part 7/3',
        category: 'Classic',
        imageUrl: 'assets/hairstyles/side_part.jpg',
        description: 'Kiểu rẽ ngôi sang một bên, phù hợp mặt tròn',
      ),
      HairStylePreset(
        id: 'undercut',
        name: 'Undercut',
        category: 'Trendy',
        imageUrl: 'assets/hairstyles/undercut.jpg',
        description: 'Cạo 2 bên, để dài phần trên',
      ),
      HairStylePreset(
        id: 'pompadour',
        name: 'Pompadour',
        category: 'Classic',
        imageUrl: 'assets/hairstyles/pompadour.jpg',
        description: 'Vuốt ngược ra sau, tạo độ phồng',
      ),
      HairStylePreset(
        id: 'textured_crop',
        name: 'Textured Crop',
        category: 'Modern',
        imageUrl: 'assets/hairstyles/textured_crop.jpg',
        description: 'Tóc ngắn, texture tự nhiên',
      ),
      HairStylePreset(
        id: 'layer',
        name: 'Layer Hàn Quốc',
        category: 'Korean',
        imageUrl: 'assets/hairstyles/layer_korean.jpg',
        description: 'Tóc layer, mái bay, phong cách Hàn',
      ),
      HairStylePreset(
        id: 'mohican',
        name: 'Mohican',
        category: 'Edgy',
        imageUrl: 'assets/hairstyles/mohican.jpg',
        description: 'Để đỉnh, fade 2 bên',
      ),
      HairStylePreset(
        id: 'quiff',
        name: 'Quiff',
        category: 'Classic',
        imageUrl: 'assets/hairstyles/quiff.jpg',
        description: 'Vuốt ngược, phồng phần trước',
      ),
      HairStylePreset(
        id: 'buzz_cut',
        name: 'Buzz Cut',
        category: 'Minimal',
        imageUrl: 'assets/hairstyles/buzz_cut.jpg',
        description: 'Tóc siêu ngắn, dễ chăm sóc',
      ),
    ];
  }
}

/// Kết quả ghép tóc
class HairTryOnResult {
  final bool success;
  final File? resultImage;
  final String message;
  final bool isModelLoading;

  HairTryOnResult({
    required this.success,
    this.resultImage,
    required this.message,
    this.isModelLoading = false,
  });
}

/// Preset hairstyle model
class HairStylePreset {
  final String id;
  final String name;
  final String category;
  final String imageUrl;
  final String description;

  HairStylePreset({
    required this.id,
    required this.name,
    required this.category,
    required this.imageUrl,
    required this.description,
  });
}

/// Timeout exception
class TimeoutException implements Exception {
  final String message;
  TimeoutException(this.message);

  @override
  String toString() => message;
}
