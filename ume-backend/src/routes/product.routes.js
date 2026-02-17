const router = require('express').Router();
const { auth, authorize, optionalAuth } = require('../middleware/auth');
const upload = require('../middleware/upload');
const ctrl = require('../controllers/product.controller');

// Public routes
router.get('/', optionalAuth, ctrl.getAll);
router.get('/featured', ctrl.getFeatured);
router.get('/category/:categoryId', ctrl.getByCategory);
router.get('/:id', optionalAuth, ctrl.getById);

// Admin & Staff routes
router.post('/', auth, authorize('Admin', 'Staff'), (req, res, next) => {
  req.uploadSubDir = 'products';
  next();
}, upload.fields([{ name: 'image', maxCount: 1 }, { name: 'mainImage', maxCount: 1 }, { name: 'additionalImages', maxCount: 5 }]), ctrl.create);

router.put('/:id', auth, authorize('Admin', 'Staff'), (req, res, next) => {
  req.uploadSubDir = 'products';
  next();
}, upload.fields([{ name: 'image', maxCount: 1 }, { name: 'mainImage', maxCount: 1 }, { name: 'additionalImages', maxCount: 5 }]), ctrl.update);

router.delete('/:id', auth, authorize('Admin', 'Staff'), ctrl.delete);

module.exports = router;
