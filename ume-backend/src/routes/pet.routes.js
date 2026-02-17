const router = require('express').Router();
const multer = require('multer');
const { auth, authorize, optionalAuth } = require('../middleware/auth');
const upload = require('../middleware/upload');
const ctrl = require('../controllers/pet.controller');

// Wrapper to handle multer errors gracefully
const handleUpload = (req, res, next) => {
  req.uploadSubDir = 'pets';
  upload.single('image')(req, res, (err) => {
    if (err instanceof multer.MulterError) {
      return res.status(400).json({ success: false, message: 'Lỗi upload: ' + err.message });
    } else if (err) {
      return res.status(400).json({ success: false, message: err.message });
    }
    next();
  });
};

router.get('/', optionalAuth, ctrl.getAll);
router.get('/listings', ctrl.getListings);
router.get('/my', auth, ctrl.getMyPets);
router.get('/:id', optionalAuth, ctrl.getById);

router.post('/', auth, handleUpload, ctrl.create);
router.put('/:id', auth, handleUpload, ctrl.update);
router.delete('/:id', auth, ctrl.delete);

module.exports = router;
