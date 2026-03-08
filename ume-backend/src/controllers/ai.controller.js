const fs = require('fs');
const PetDetector = require('../services/pet-detector');

/**
 * POST /api/ai/detect-pets
 * Detect animals in image using YOLO26n + EfficientNet (Node.js ONNX Runtime)
 * Supports: dog, cat, bird, horse, sheep, cow, elephant, bear, zebra, giraffe
 */
exports.detectPets = async (req, res) => {
  try {
    if (!req.file) {
      return res.status(400).json({
        success: false,
        message: 'Vui lòng tải lên hình ảnh',
      });
    }

    const detector = await PetDetector.getInstance();
    const result = await detector.detect(req.file.path);

    // Clean up temp file
    if (fs.existsSync(req.file.path)) fs.unlinkSync(req.file.path);

    res.json(result);
  } catch (error) {
    // Clean up temp file
    if (req.file && fs.existsSync(req.file.path)) {
      fs.unlinkSync(req.file.path);
    }

    console.error('AI detect error:', error.message);
    res.status(500).json({
      success: false,
      message: 'Lỗi xử lý ảnh',
      error: error.message,
    });
  }
};

/**
 * GET /api/ai/health
 * Check AI service status
 */
exports.healthCheck = async (req, res) => {
  try {
    const detector = await PetDetector.getInstance();
    res.json({
      success: true,
      data: {
        status: 'ok',
        service: 'ume-pet-detection',
        runtime: 'onnxruntime-node',
        models: ['YOLO26n', 'EfficientNet-B0'],
        animals: ['dog', 'cat', 'bird', 'horse', 'sheep', 'cow', 'elephant', 'bear', 'zebra', 'giraffe'],
      },
    });
  } catch (error) {
    res.json({
      success: false,
      message: 'AI models not loaded: ' + error.message,
    });
  }
};
