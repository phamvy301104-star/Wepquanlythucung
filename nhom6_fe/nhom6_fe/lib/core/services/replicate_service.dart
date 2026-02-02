import 'dart:convert';
import 'dart:io';
import 'dart:developer' as developer;
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:http/http.dart' as http;
import 'package:path_provider/path_provider.dart';

/// Replicate API Service for Hair Try-On
/// Model: flux-kontext-apps/multi-image-kontext-pro
/// Cost: $0.04 per image (~50 uses with $2 credit)
class ReplicateService {
  static const String _logName = 'ReplicateService';
  static const String _baseUrl = 'https://api.replicate.com/v1';

  /// Model version for multi-image-kontext-pro
  /// This model combines multiple input images based on text prompt
  static const String _modelVersion =
      'flux-kontext-apps/multi-image-kontext-pro';

  late final String _apiKey;
  bool _isInitialized = false;

  /// Initialize service with API key from .env
  Future<void> initialize() async {
    try {
      _apiKey = dotenv.env['REPLICATE_API_KEY'] ?? '';

      if (_apiKey.isEmpty) {
        throw Exception(
          'REPLICATE_API_KEY not found in .env file.\n'
          'Add this line to your .env:\n'
          'REPLICATE_API_KEY=your_api_key_here',
        );
      }

      _isInitialized = true;
      developer.log('✅ Replicate service initialized', name: _logName);
    } catch (e) {
      developer.log('❌ Failed to initialize: $e', name: _logName);
      rethrow;
    }
  }

  /// Combine face image with hairstyle image using AI
  /// [faceImage] - User's face photo (Ảnh mặt khách)
  /// [hairStyleImage] - Reference hairstyle photo (Ảnh tóc mẫu)
  /// [prompt] - Optional custom prompt (default uses optimized hair transfer prompt)
  /// Returns: Result with generated image or error
  Future<HairTransferResult> transferHairStyle({
    required File faceImage,
    required File hairStyleImage,
    String? prompt,
  }) async {
    if (!_isInitialized) {
      await initialize();
    }

    developer.log('🚀 Starting hair transfer...', name: _logName);

    try {
      // Step 1: Upload images to get URLs
      developer.log('📤 Uploading images...', name: _logName);
      final faceUrl = await _uploadImage(faceImage, 'face');
      final hairUrl = await _uploadImage(hairStyleImage, 'hair');

      developer.log(
        '✅ Images uploaded: face=$faceUrl, hair=$hairUrl',
        name: _logName,
      );

      // Step 2: Create prediction with multi-image-kontext-pro
      // TỐI ƯU PROMPT CHO HAIR TRANSPLANT:
      // 1. Định nghĩa rõ Role: Identity (ảnh 1) vs Style (ảnh 2)
      // 2. Yêu cầu "Masking" bằng lời: Ignore face/background in image 2
      // 3. Giữ nguyên background từ ảnh 1
      // 4. Blend hairline tự nhiên
      final defaultPrompt =
          prompt ??
          '''
MISSION: Professional Hair Transplant / Hairstyle Swap.

SOURCE IMAGES:
- input_image_1 (BASE): This is the CLIENT'S PHOTO. Use the EXACT facial features, skin tone, head shape, facial expression, body, clothing, and BACKGROUND from this image. DO NOT CHANGE THE IDENTITY OR BACKGROUND.
- input_image_2 (REFERENCE): This contains the TARGET HAIRSTYLE only. Extract ONLY the hairstyle, hair texture, hair volume, and hair color. COMPLETELY IGNORE the face, skin, background, and body in this image.

DETAILED INSTRUCTIONS:
1. IDENTITY PRESERVATION: Keep 100% of the person's face from input_image_1 - eyes, nose, mouth, skin tone, facial structure, expression.
2. BACKGROUND PRESERVATION: Keep the EXACT same background, lighting, and environment from input_image_1.
3. HAIR EXTRACTION: From input_image_2, extract ONLY the hair style, shape, texture, and color. Ignore everything else.
4. HAIR TRANSPLANT: Replace the hair on the person in input_image_1 with the extracted hairstyle from input_image_2.
5. NATURAL BLENDING: Blend the hairline naturally with the forehead. Match the lighting and shadows.
6. OUTPUT QUALITY: Photorealistic, high resolution, natural looking, no artifacts.

STRICT CONSTRAINTS (DO NOT VIOLATE):
- DO NOT use the face from input_image_2.
- DO NOT use the background from input_image_2.
- DO NOT change the person's identity, gender, age, or ethnicity.
- DO NOT produce cartoon, illustration, or artistic style.
- The output MUST look like a real photo of the person from input_image_1 with a new haircut.
''';

      developer.log(
        '🤖 Creating prediction with Optimized Prompt...',
        name: _logName,
      );
      final prediction = await _createPrediction(
        faceImageUrl: faceUrl,
        hairImageUrl: hairUrl,
        prompt: defaultPrompt,
      );

      developer.log(
        '✅ Prediction created: ${prediction['id']}',
        name: _logName,
      );

      // Step 3: Poll for result
      developer.log('⏳ Waiting for result...', name: _logName);
      final result = await _pollForResult(prediction['id']);

      if (result['status'] == 'succeeded') {
        // Get output URL
        final outputUrl = result['output'];
        developer.log('✅ Generation succeeded: $outputUrl', name: _logName);

        // Download and save result
        final resultFile = await _downloadImage(outputUrl);

        return HairTransferResult(
          success: true,
          resultImage: resultFile,
          message:
              '✨ Tút tát thành công! Check ngay diện mạo mới nào bro! 💇‍♂️',
          cost: 0.04, // $0.04 per image
        );
      } else {
        final error = result['error'] ?? 'Unknown error';
        developer.log('❌ Generation failed: $error', name: _logName);
        return HairTransferResult(
          success: false,
          message: 'AI đang bận xíu, thử lại sau nha! 🔄 ($error)',
        );
      }
    } catch (e) {
      developer.log('❌ Hair transfer failed: $e', name: _logName);
      return HairTransferResult(
        success: false,
        message: 'Lỗi hệ thống rồi bro! Thử lại nhé 🙏 ($e)',
      );
    }
  }

  /// Upload image to Replicate's file upload endpoint
  Future<String> _uploadImage(File imageFile, String name) async {
    try {
      // Read file bytes
      final bytes = await imageFile.readAsBytes();
      final base64Image = base64Encode(bytes);

      // Get file extension
      final extension = imageFile.path.split('.').last.toLowerCase();
      final mimeType = extension == 'png' ? 'image/png' : 'image/jpeg';

      // Use Replicate's file upload API
      final response = await http.post(
        Uri.parse('$_baseUrl/files'),
        headers: {
          'Authorization': 'Bearer $_apiKey',
          'Content-Type': 'application/json',
        },
        body: jsonEncode({
          'content': base64Image,
          'content_type': mimeType,
          'name': '${name}_${DateTime.now().millisecondsSinceEpoch}.$extension',
        }),
      );

      if (response.statusCode == 201 || response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return data['urls']['get'];
      } else {
        // Fallback: Use base64 data URI directly
        developer.log(
          'File upload failed (${response.statusCode}), using base64 fallback',
          name: _logName,
        );
        return 'data:$mimeType;base64,$base64Image';
      }
    } catch (e) {
      developer.log('Upload error, using base64 fallback: $e', name: _logName);
      final bytes = await imageFile.readAsBytes();
      final base64Image = base64Encode(bytes);
      final extension = imageFile.path.split('.').last.toLowerCase();
      final mimeType = extension == 'png' ? 'image/png' : 'image/jpeg';
      return 'data:$mimeType;base64,$base64Image';
    }
  }

  /// Create a prediction using multi-image-kontext-pro
  Future<Map<String, dynamic>> _createPrediction({
    required String faceImageUrl,
    required String hairImageUrl,
    required String prompt,
  }) async {
    final response = await http.post(
      Uri.parse('$_baseUrl/models/$_modelVersion/predictions'),
      headers: {
        'Authorization': 'Bearer $_apiKey',
        'Content-Type': 'application/json',
        'Prefer': 'wait', // Wait for result instead of polling
      },
      body: jsonEncode({
        'input': {
          'prompt': prompt,
          'input_image_1': faceImageUrl,
          'input_image_2': hairImageUrl,
          'aspect_ratio': '1:1',
          'output_format': 'jpg',
          'output_quality': 90,
          'safety_tolerance': 2,
        },
      }),
    );

    developer.log(
      'Create prediction response: ${response.statusCode}',
      name: _logName,
    );

    if (response.statusCode == 201 || response.statusCode == 200) {
      return jsonDecode(response.body);
    } else {
      final error = jsonDecode(response.body);
      throw Exception(
        'Failed to create prediction: ${error['detail'] ?? response.body}',
      );
    }
  }

  /// Poll for prediction result
  Future<Map<String, dynamic>> _pollForResult(String predictionId) async {
    const maxAttempts = 60; // 2 minutes max
    const pollInterval = Duration(seconds: 2);

    for (var i = 0; i < maxAttempts; i++) {
      final response = await http.get(
        Uri.parse('$_baseUrl/predictions/$predictionId'),
        headers: {'Authorization': 'Bearer $_apiKey'},
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        final status = data['status'];

        developer.log('Poll #$i: status=$status', name: _logName);

        if (status == 'succeeded' ||
            status == 'failed' ||
            status == 'canceled') {
          return data;
        }
      }

      await Future.delayed(pollInterval);
    }

    throw Exception('Timeout waiting for result');
  }

  /// Download image from URL and save to local file
  Future<File> _downloadImage(dynamic outputUrl) async {
    // Handle both string URL and list of URLs
    String url;
    if (outputUrl is List && outputUrl.isNotEmpty) {
      url = outputUrl[0];
    } else if (outputUrl is String) {
      url = outputUrl;
    } else {
      throw Exception('Invalid output URL format');
    }

    final response = await http.get(Uri.parse(url));

    if (response.statusCode == 200) {
      final tempDir = await getTemporaryDirectory();
      final fileName =
          'hair_tryon_${DateTime.now().millisecondsSinceEpoch}.jpg';
      final file = File('${tempDir.path}/$fileName');
      await file.writeAsBytes(response.bodyBytes);
      return file;
    } else {
      throw Exception('Failed to download result image');
    }
  }

  /// Check remaining credit (estimated based on $0.04 per image)
  Future<CreditInfo?> getAccountInfo() async {
    if (!_isInitialized) {
      await initialize();
    }

    try {
      final response = await http.get(
        Uri.parse('$_baseUrl/account'),
        headers: {'Authorization': 'Bearer $_apiKey'},
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return CreditInfo(
          username: data['username'] ?? 'Unknown',
          type: data['type'] ?? 'user',
        );
      }
    } catch (e) {
      developer.log('Failed to get account info: $e', name: _logName);
    }
    return null;
  }

  bool get isInitialized => _isInitialized;
}

/// Hair Transfer Result
class HairTransferResult {
  final bool success;
  final File? resultImage;
  final String message;
  final double? cost;

  HairTransferResult({
    required this.success,
    this.resultImage,
    required this.message,
    this.cost,
  });
}

/// Credit Info from Replicate account
class CreditInfo {
  final String username;
  final String type;

  CreditInfo({required this.username, required this.type});
}
