/**
 * PetDetector - Node.js AI Detection using ONNX Runtime
 * ======================================================
 * YOLO26n  : Object detection — end-to-end, NMS-free (10 loài vật COCO)
 * EfficientNet-B0 : Breed classification (120+ breeds, chó & mèo)
 *
 * Runs 100% in Node.js — no Python required
 */

const ort = require('onnxruntime-node');
const sharp = require('sharp');
const path = require('path');
const fs = require('fs');
const {
  DOG_CLASS_INDICES,
  CAT_CLASS_INDICES,
  getBreedName,
} = require('./breed-labels');

const MODELS_DIR = path.join(__dirname, '../../models');
const YOLO_SIZE = 640;
const CLASSIFY_SIZE = 224;
const CONF_THRESHOLD = 0.25;

/* ============ COCO Animal Classes ============ */
const COCO_ANIMALS = {
  14: { en: 'bird', vi: 'Chim', emoji: '🐦' },
  15: { en: 'cat', vi: 'Mèo', emoji: '🐈' },
  16: { en: 'dog', vi: 'Chó', emoji: '🐕' },
  17: { en: 'horse', vi: 'Ngựa', emoji: '🐴' },
  18: { en: 'sheep', vi: 'Cừu', emoji: '🐑' },
  19: { en: 'cow', vi: 'Bò', emoji: '🐄' },
  20: { en: 'elephant', vi: 'Voi', emoji: '🐘' },
  21: { en: 'bear', vi: 'Gấu', emoji: '🐻' },
  22: { en: 'zebra', vi: 'Ngựa vằn', emoji: '🦓' },
  23: { en: 'giraffe', vi: 'Hươu cao cổ', emoji: '🦒' },
};
const ANIMAL_CLASS_IDS = new Set(Object.keys(COCO_ANIMALS).map(Number));

class PetDetector {
  static _instance = null;

  /** Singleton accessor (async) */
  static async getInstance() {
    if (!this._instance) {
      const detector = new PetDetector();
      await detector._loadModels();
      this._instance = detector;
    }
    return this._instance;
  }

  /* ========================== Load Models ========================== */
  async _loadModels() {
    console.log('🔄 Loading AI models (ONNX Runtime)...');

    const yoloPath = path.join(MODELS_DIR, 'yolo26n.onnx');
    const enetPath = path.join(MODELS_DIR, 'efficientnet_b0.onnx');

    if (!fs.existsSync(yoloPath)) throw new Error(`Model not found: ${yoloPath}`);
    if (!fs.existsSync(enetPath)) throw new Error(`Model not found: ${enetPath}`);

    const opts = { executionProviders: ['cpu'] };
    this.yolo = await ort.InferenceSession.create(yoloPath, opts);
    this.classifier = await ort.InferenceSession.create(enetPath, opts);

    console.log('✅ AI models loaded! (YOLO26n + EfficientNet-B0)');
  }

  /* ========================== Main Pipeline ========================== */
  async detect(imagePath) {
    // 1. Preprocess for YOLO
    const prep = await this._preprocessYolo(imagePath);

    // 2. Run YOLO inference
    const feeds = { [this.yolo.inputNames[0]]: prep.tensor };
    const yoloResult = await this.yolo.run(feeds);
    const output = yoloResult[this.yolo.outputNames[0]];

    // 3. Post-process: extract detections (YOLO26 end-to-end — no NMS needed)
    const detections = this._postprocessYolo26(
      output, prep.origW, prep.origH, prep.scale, prep.padX, prep.padY
    );

    // 4. Classify breed for dog/cat detections only
    for (const det of detections) {
      if (det.type === 'dog' || det.type === 'cat') {
        const breed = await this._classifyBreed(imagePath, det.bbox, det.type);
        det.breed = breed.en;
        det.breed_vi = breed.vi;
        det.breed_confidence = breed.confidence;
      } else {
        // Other animals — no breed classification
        const info = COCO_ANIMALS[det.classId] || { en: 'Unknown', vi: 'Không xác định' };
        det.breed = info.en;
        det.breed_vi = info.vi;
        det.breed_confidence = det.confidence;
      }
    }

    // Count by type
    const typeCounts = {};
    for (const det of detections) {
      typeCounts[det.type] = (typeCounts[det.type] || 0) + 1;
    }

    return {
      success: true,
      data: {
        total_animals: detections.length,
        dogs: typeCounts['dog'] || 0,
        cats: typeCounts['cat'] || 0,
        other_animals: detections.length - (typeCounts['dog'] || 0) - (typeCounts['cat'] || 0),
        detections,
        image_width: prep.origW,
        image_height: prep.origH,
      },
    };
  }

  /* ========================== YOLO Preprocessing ========================== */
  async _preprocessYolo(imagePath) {
    const meta = await sharp(imagePath).metadata();
    const origW = meta.width;
    const origH = meta.height;

    // Letterbox resize
    const scale = Math.min(YOLO_SIZE / origW, YOLO_SIZE / origH);
    const newW = Math.round(origW * scale);
    const newH = Math.round(origH * scale);
    const padX = Math.round((YOLO_SIZE - newW) / 2);
    const padY = Math.round((YOLO_SIZE - newH) / 2);

    const pixels = await sharp(imagePath)
      .resize(newW, newH)
      .extend({
        top: padY,
        bottom: YOLO_SIZE - newH - padY,
        left: padX,
        right: YOLO_SIZE - newW - padX,
        background: { r: 114, g: 114, b: 114 },
      })
      .removeAlpha()
      .raw()
      .toBuffer();

    // HWC → CHW, normalize [0,1]
    const totalPixels = YOLO_SIZE * YOLO_SIZE;
    const float32 = new Float32Array(3 * totalPixels);
    for (let i = 0; i < totalPixels; i++) {
      float32[i] = pixels[i * 3] / 255.0;                        // R
      float32[totalPixels + i] = pixels[i * 3 + 1] / 255.0;     // G
      float32[2 * totalPixels + i] = pixels[i * 3 + 2] / 255.0; // B
    }

    const tensor = new ort.Tensor('float32', float32, [1, 3, YOLO_SIZE, YOLO_SIZE]);
    return { tensor, origW, origH, scale, padX, padY };
  }

  /* ========================== YOLO26 Post-Processing (End-to-End, NMS-free) ========================== */
  _postprocessYolo26(output, origW, origH, scale, padX, padY) {
    const data = output.data;
    const numDetections = output.dims[1]; // [1, 300, 6]

    const detections = [];

    for (let i = 0; i < numDetections; i++) {
      const offset = i * 6;
      const x1_raw = data[offset + 0]; // Already in 640x640 coords
      const y1_raw = data[offset + 1];
      const x2_raw = data[offset + 2];
      const y2_raw = data[offset + 3];
      const score = data[offset + 4];
      const classId = Math.round(data[offset + 5]);

      // Filter: only animals above threshold
      if (score < CONF_THRESHOLD) continue;
      if (!ANIMAL_CLASS_IDS.has(classId)) continue;

      // Convert letterbox coords → original image coords
      let x1 = (x1_raw - padX) / scale;
      let y1 = (y1_raw - padY) / scale;
      let x2 = (x2_raw - padX) / scale;
      let y2 = (y2_raw - padY) / scale;

      x1 = Math.max(0, Math.round(x1));
      y1 = Math.max(0, Math.round(y1));
      x2 = Math.min(origW, Math.round(x2));
      y2 = Math.min(origH, Math.round(y2));

      if (x2 - x1 < 5 || y2 - y1 < 5) continue; // Skip tiny boxes

      const animalInfo = COCO_ANIMALS[classId];
      detections.push({
        id: detections.length + 1,
        type: animalInfo.en,
        type_vi: animalInfo.vi,
        emoji: animalInfo.emoji,
        classId,
        confidence: Math.round(score * 1000) / 1000,
        bbox: { x1, y1, x2, y2 },
      });
    }

    return detections;
  }

  /* ========================== Breed Classification ========================== */
  async _classifyBreed(imagePath, bbox, animalType) {
    const pad = 10;
    const meta = await sharp(imagePath).metadata();
    const left = Math.max(0, bbox.x1 - pad);
    const top = Math.max(0, bbox.y1 - pad);
    const width = Math.min(meta.width - left, bbox.x2 - bbox.x1 + 2 * pad);
    const height = Math.min(meta.height - top, bbox.y2 - bbox.y1 + 2 * pad);

    if (width <= 0 || height <= 0) {
      return { en: 'Unknown', vi: 'Không xác định', confidence: 0 };
    }

    const pixels = await sharp(imagePath)
      .extract({ left: Math.round(left), top: Math.round(top), width: Math.round(width), height: Math.round(height) })
      .resize(CLASSIFY_SIZE, CLASSIFY_SIZE)
      .removeAlpha()
      .raw()
      .toBuffer();

    // Normalize with ImageNet stats
    const mean = [0.485, 0.456, 0.406];
    const std = [0.229, 0.224, 0.225];
    const totalPixels = CLASSIFY_SIZE * CLASSIFY_SIZE;
    const float32 = new Float32Array(3 * totalPixels);

    for (let i = 0; i < totalPixels; i++) {
      float32[i] = (pixels[i * 3] / 255.0 - mean[0]) / std[0];
      float32[totalPixels + i] = (pixels[i * 3 + 1] / 255.0 - mean[1]) / std[1];
      float32[2 * totalPixels + i] = (pixels[i * 3 + 2] / 255.0 - mean[2]) / std[2];
    }

    const tensor = new ort.Tensor('float32', float32, [1, 3, CLASSIFY_SIZE, CLASSIFY_SIZE]);
    const feeds = { [this.classifier.inputNames[0]]: tensor };
    const result = await this.classifier.run(feeds);
    const logits = result[this.classifier.outputNames[0]].data;

    // Stable softmax
    let maxLogit = -Infinity;
    for (let i = 0; i < logits.length; i++) {
      if (logits[i] > maxLogit) maxLogit = logits[i];
    }
    let expSum = 0;
    for (let i = 0; i < logits.length; i++) {
      expSum += Math.exp(logits[i] - maxLogit);
    }

    // Find best breed for this animal type
    const indices = animalType === 'dog' ? DOG_CLASS_INDICES : CAT_CLASS_INDICES;
    let bestIdx = indices[0];
    let bestProb = 0;

    for (const idx of indices) {
      const prob = Math.exp(logits[idx] - maxLogit) / expSum;
      if (prob > bestProb) {
        bestIdx = idx;
        bestProb = prob;
      }
    }

    const breed = getBreedName(bestIdx);
    return {
      en: breed.en,
      vi: breed.vi,
      confidence: Math.round(bestProb * 1000) / 1000,
    };
  }
}

module.exports = PetDetector;
