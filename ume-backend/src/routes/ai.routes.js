const router = require('express').Router();
const upload = require('../middleware/upload');
const ctrl = require('../controllers/ai.controller');

// Set upload subdirectory for AI detection images
router.post('/detect-pets', (req, res, next) => {
  req.uploadSubDir = 'general';
  next();
}, upload.single('file'), ctrl.detectPets);

router.get('/health', ctrl.healthCheck);

module.exports = router;
