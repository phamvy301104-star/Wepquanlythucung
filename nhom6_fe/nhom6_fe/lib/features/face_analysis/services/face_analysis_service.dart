import 'dart:developer' as developer;
import 'dart:io';
import 'dart:math';
import 'package:google_mlkit_face_detection/google_mlkit_face_detection.dart';

/// Enum định nghĩa các dáng mặt
enum FaceShape {
  oval, // Trái xoan
  round, // Tròn
  square, // Vuông
  oblong, // Dài/Chữ nhật
  heart, // Trái tim
  diamond, // Kim cương
}

/// Extension để lấy tên tiếng Việt và mô tả
extension FaceShapeExtension on FaceShape {
  String get displayName {
    switch (this) {
      case FaceShape.oval:
        return 'Trái Xoan';
      case FaceShape.round:
        return 'Tròn';
      case FaceShape.square:
        return 'Vuông';
      case FaceShape.oblong:
        return 'Dài (Oblong)';
      case FaceShape.heart:
        return 'Trái Tim';
      case FaceShape.diamond:
        return 'Kim Cương';
    }
  }

  String get description {
    switch (this) {
      case FaceShape.oval:
        return 'Khuôn mặt cân đối, trán và hàm hơi hẹp hơn gò má. Đây là dáng mặt lý tưởng, phù hợp với hầu hết các kiểu tóc.';
      case FaceShape.round:
        return 'Chiều dài và chiều rộng gần bằng nhau, gò má đầy đặn, cằm tròn. Nên chọn kiểu tóc tạo góc cạnh.';
      case FaceShape.square:
        return 'Trán rộng, đường hàm góc cạnh, các cạnh gần như song song. Kiểu tóc mềm mại sẽ giúp cân bằng.';
      case FaceShape.oblong:
        return 'Khuôn mặt dài hơn rộng đáng kể, trán, gò má và hàm có chiều rộng tương đương. Cần kiểu tóc tạo chiều rộng.';
      case FaceShape.heart:
        return 'Trán rộng, gò má cao, hàm và cằm nhỏ hẹp. Kiểu tóc cân bằng vùng trán sẽ rất hợp.';
      case FaceShape.diamond:
        return 'Gò má rộng nhất, trán và hàm đều hẹp. Dáng mặt độc đáo, thích hợp với kiểu tóc phồng hai bên.';
    }
  }

  String get emoji {
    switch (this) {
      case FaceShape.oval:
        return '🥚';
      case FaceShape.round:
        return '🔵';
      case FaceShape.square:
        return '🟦';
      case FaceShape.oblong:
        return '📏';
      case FaceShape.heart:
        return '💜';
      case FaceShape.diamond:
        return '💎';
    }
  }

  /// Các kiểu tóc nam được đề xuất cho từng dáng mặt
  List<String> get recommendedHairstyles {
    switch (this) {
      case FaceShape.oval:
        return [
          'Side Part',
          'Quiff',
          'Pompadour',
          'Textured Crop',
          'Undercut',
          'Man Bun',
        ];
      case FaceShape.round:
        return [
          'Side Part 7/3',
          'Pompadour cao',
          'Faux Hawk',
          'Angular Fringe',
          'Undercut + Slick Back',
        ];
      case FaceShape.square:
        return [
          'Textured Fringe',
          'Messy Quiff',
          'Side Swept',
          'Medium Length Layers',
          'Taper Fade',
        ];
      case FaceShape.oblong:
        return [
          'Fringe / Mái rủ',
          'Side Part ngắn',
          'Textured Crop',
          'Buzz Cut với Beard',
          'Crew Cut',
        ];
      case FaceShape.heart:
        return [
          'Side Part',
          'Fringe dài',
          'Medium Length',
          'Textured Waves',
          'Comb Over',
        ];
      case FaceShape.diamond:
        return [
          'Fringe dày',
          'Side Swept Bangs',
          'Textured Quiff',
          'Messy Medium',
          'Curtain Bangs',
        ];
    }
  }
}

/// Kết quả phân tích khuôn mặt
class FaceAnalysisResult {
  final FaceShape faceShape;
  final double confidence;
  final Map<String, double> measurements;
  final List<String> recommendations;

  FaceAnalysisResult({
    required this.faceShape,
    required this.confidence,
    required this.measurements,
    required this.recommendations,
  });

  Map<String, dynamic> toJson() => {
    'faceShape': faceShape.name,
    'faceShapeName': faceShape.displayName,
    'description': faceShape.description,
    'confidence': confidence,
    'measurements': measurements,
    'recommendations': recommendations,
    'emoji': faceShape.emoji,
  };
}

/// Service phân tích khuôn mặt sử dụng ML Kit
class FaceAnalysisService {
  late final FaceDetector _faceDetector;

  FaceAnalysisService() {
    _faceDetector = FaceDetector(
      options: FaceDetectorOptions(
        enableContours: true,
        enableLandmarks: true,
        performanceMode: FaceDetectorMode.accurate,
      ),
    );
  }

  /// Phân tích khuôn mặt từ file ảnh
  Future<FaceAnalysisResult?> analyzeFromFile(File imageFile) async {
    final inputImage = InputImage.fromFile(imageFile);
    return _analyze(inputImage);
  }

  /// Phân tích khuôn mặt từ path
  Future<FaceAnalysisResult?> analyzeFromPath(String imagePath) async {
    final inputImage = InputImage.fromFilePath(imagePath);
    return _analyze(inputImage);
  }

  /// Logic phân tích chính
  Future<FaceAnalysisResult?> _analyze(InputImage inputImage) async {
    try {
      final faces = await _faceDetector.processImage(inputImage);

      if (faces.isEmpty) {
        return null;
      }

      // Lấy khuôn mặt đầu tiên (lớn nhất)
      final face = faces.first;

      // Tính toán các số đo từ contours và landmarks
      final measurements = _calculateMeasurements(face);

      // Xác định dáng mặt dựa trên tỉ lệ
      final (faceShape, confidence) = _determineFaceShape(measurements);

      return FaceAnalysisResult(
        faceShape: faceShape,
        confidence: confidence,
        measurements: measurements,
        recommendations: faceShape.recommendedHairstyles,
      );
    } catch (e) {
      developer.log(
        'Error analyzing face: $e',
        name: 'FaceAnalysisService',
        error: e,
      );
      return null;
    }
  }

  /// Tính toán các số đo khuôn mặt
  Map<String, double> _calculateMeasurements(Face face) {
    final boundingBox = face.boundingBox;

    // Chiều rộng và chiều cao tổng thể từ bounding box
    double faceWidth = boundingBox.width;
    double faceHeight = boundingBox.height;

    // Lấy contour khuôn mặt nếu có
    final faceContour = face.contours[FaceContourType.face];

    double foreheadWidth = faceWidth * 0.85; // Ước tính
    double cheekboneWidth = faceWidth;
    double jawWidth = faceWidth * 0.75;
    double chinWidth = faceWidth * 0.4;

    // Nếu có face contour, tính chính xác hơn
    if (faceContour != null && faceContour.points.length >= 30) {
      final points = faceContour.points;

      // Điểm trên cùng (trán)
      final topPoints = points
          .where((p) => p.y < boundingBox.top + boundingBox.height * 0.25)
          .toList();
      if (topPoints.length >= 2) {
        foreheadWidth = _getMaxWidth(topPoints);
      }

      // Điểm giữa (gò má) - khoảng 1/3 từ trên
      final midPoints = points
          .where(
            (p) =>
                p.y >= boundingBox.top + boundingBox.height * 0.3 &&
                p.y <= boundingBox.top + boundingBox.height * 0.5,
          )
          .toList();
      if (midPoints.length >= 2) {
        cheekboneWidth = _getMaxWidth(midPoints);
      }

      // Điểm hàm - khoảng 2/3 từ trên
      final jawPoints = points
          .where(
            (p) =>
                p.y >= boundingBox.top + boundingBox.height * 0.6 &&
                p.y <= boundingBox.top + boundingBox.height * 0.8,
          )
          .toList();
      if (jawPoints.length >= 2) {
        jawWidth = _getMaxWidth(jawPoints);
      }

      // Điểm cằm - dưới cùng
      final chinPoints = points
          .where((p) => p.y >= boundingBox.top + boundingBox.height * 0.85)
          .toList();
      if (chinPoints.length >= 2) {
        chinWidth = _getMaxWidth(chinPoints);
      }
    }

    // Tính các tỉ lệ quan trọng
    double lengthToWidthRatio = faceHeight / cheekboneWidth;
    double foreheadToCheekRatio = foreheadWidth / cheekboneWidth;
    double jawToCheekRatio = jawWidth / cheekboneWidth;
    double chinToCheekRatio = chinWidth / cheekboneWidth;

    return {
      'faceHeight': faceHeight,
      'faceWidth': faceWidth,
      'foreheadWidth': foreheadWidth,
      'cheekboneWidth': cheekboneWidth,
      'jawWidth': jawWidth,
      'chinWidth': chinWidth,
      'lengthToWidthRatio': lengthToWidthRatio,
      'foreheadToCheekRatio': foreheadToCheekRatio,
      'jawToCheekRatio': jawToCheekRatio,
      'chinToCheekRatio': chinToCheekRatio,
    };
  }

  /// Tìm chiều rộng lớn nhất từ các điểm
  double _getMaxWidth(List<Point<int>> points) {
    if (points.length < 2) return 0;

    double minX = points.map((p) => p.x.toDouble()).reduce(min);
    double maxX = points.map((p) => p.x.toDouble()).reduce(max);

    return maxX - minX;
  }

  /// Xác định dáng mặt từ các tỉ lệ đo được
  (FaceShape, double) _determineFaceShape(Map<String, double> m) {
    final ratio = m['lengthToWidthRatio'] ?? 1.3;
    final foreheadRatio = m['foreheadToCheekRatio'] ?? 0.85;
    final jawRatio = m['jawToCheekRatio'] ?? 0.75;
    final chinRatio = m['chinToCheekRatio'] ?? 0.4;

    // Scoring system cho từng dáng mặt
    Map<FaceShape, double> scores = {};

    // OVAL: Tỉ lệ dài/rộng ~1.3-1.5, trán & hàm hơi hẹp hơn gò má
    scores[FaceShape.oval] = _calculateScore([
      (ratio >= 1.3 && ratio <= 1.5, 40),
      (foreheadRatio >= 0.8 && foreheadRatio <= 0.95, 30),
      (jawRatio >= 0.7 && jawRatio <= 0.85, 30),
    ]);

    // ROUND: Tỉ lệ dài/rộng ~1.0-1.2, các phần gần bằng nhau
    scores[FaceShape.round] = _calculateScore([
      (ratio >= 1.0 && ratio <= 1.25, 40),
      (foreheadRatio >= 0.85 && foreheadRatio <= 1.0, 30),
      (jawRatio >= 0.8 && jawRatio <= 0.95, 30),
    ]);

    // SQUARE: Trán rộng, hàm góc cạnh (gần bằng gò má)
    scores[FaceShape.square] = _calculateScore([
      (ratio >= 1.1 && ratio <= 1.4, 30),
      (foreheadRatio >= 0.9 && foreheadRatio <= 1.05, 35),
      (jawRatio >= 0.85 && jawRatio <= 1.0, 35),
    ]);

    // OBLONG: Dài hơn nhiều (>1.5), các phần có chiều rộng tương đương
    scores[FaceShape.oblong] = _calculateScore([
      (ratio >= 1.5, 50),
      (foreheadRatio >= 0.85 && foreheadRatio <= 1.0, 25),
      (jawRatio >= 0.8 && jawRatio <= 0.95, 25),
    ]);

    // HEART: Trán rộng, cằm nhọn hẹp
    scores[FaceShape.heart] = _calculateScore([
      (ratio >= 1.2 && ratio <= 1.5, 30),
      (foreheadRatio >= 0.9 && foreheadRatio <= 1.1, 35),
      (chinRatio <= 0.5, 35),
    ]);

    // DIAMOND: Gò má rộng nhất, trán & hàm đều hẹp
    scores[FaceShape.diamond] = _calculateScore([
      (ratio >= 1.2 && ratio <= 1.5, 30),
      (foreheadRatio <= 0.85, 35),
      (jawRatio <= 0.75, 35),
    ]);

    // Tìm dáng mặt có điểm cao nhất
    FaceShape bestShape = FaceShape.oval;
    double bestScore = 0;

    for (var entry in scores.entries) {
      if (entry.value > bestScore) {
        bestScore = entry.value;
        bestShape = entry.key;
      }
    }

    // Normalize confidence to 0-1
    double confidence = bestScore / 100.0;

    return (bestShape, confidence);
  }

  /// Tính điểm dựa trên các điều kiện
  double _calculateScore(List<(bool, int)> conditions) {
    double score = 0;
    for (var (condition, points) in conditions) {
      if (condition) {
        score += points;
      }
    }
    return score;
  }

  /// Dispose resources
  void dispose() {
    _faceDetector.close();
  }
}
