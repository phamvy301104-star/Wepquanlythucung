const router = require('express').Router();
const { auth, authorize } = require('../middleware/auth');
const upload = require('../middleware/upload');

// General file upload endpoint
router.post('/', auth, (req, res, next) => {
  req.uploadSubDir = req.query.folder || req.body.folder || 'general';
  next();
}, upload.single('file'), (req, res) => {
  if (!req.file) {
    return res.status(400).json({ success: false, message: 'Vui lòng chọn file' });
  }
  const folder = req.query.folder || req.body.folder || 'general';
  const baseUrl = `${req.protocol}://${req.get('host')}`;
  res.json({
    success: true,
    data: {
      url: `/uploads/${folder}/${req.file.filename}`,
      filename: req.file.filename,
      originalName: req.file.originalname,
      size: req.file.size
    }
  });
});

// Multiple files upload
router.post('/multiple', auth, (req, res, next) => {
  req.uploadSubDir = req.body.folder || 'general';
  next();
}, upload.array('files', 10), (req, res) => {
  if (!req.files || req.files.length === 0) {
    return res.status(400).json({ success: false, message: 'Vui lòng chọn file' });
  }
  const folder = req.body.folder || 'general';
  const files = req.files.map(f => ({
    url: `/uploads/${folder}/${f.filename}`,
    filename: f.filename,
    originalName: f.originalname,
    size: f.size
  }));
  res.json({ success: true, data: files });
});

module.exports = router;
