const router = require('express').Router();
const { auth, authorize } = require('../middleware/auth');
const upload = require('../middleware/upload');
const ctrl = require('../controllers/service.controller');

// Services
router.get('/', ctrl.getAll);
router.get('/categories', ctrl.getAllCategories);
router.get('/:id', ctrl.getById);

router.post('/', auth, authorize('Admin', 'Staff'), (req, res, next) => {
  req.uploadSubDir = 'services';
  next();
}, upload.single('image'), ctrl.create);

router.put('/:id', auth, authorize('Admin', 'Staff'), (req, res, next) => {
  req.uploadSubDir = 'services';
  next();
}, upload.single('image'), ctrl.update);

router.delete('/:id', auth, authorize('Admin', 'Staff'), ctrl.delete);

// Service Categories
router.post('/categories', auth, authorize('Admin', 'Staff'), (req, res, next) => {
  req.uploadSubDir = 'service-categories';
  next();
}, upload.single('image'), ctrl.createCategory);

router.put('/categories/:id', auth, authorize('Admin', 'Staff'), (req, res, next) => {
  req.uploadSubDir = 'service-categories';
  next();
}, upload.single('image'), ctrl.updateCategory);

router.delete('/categories/:id', auth, authorize('Admin', 'Staff'), ctrl.deleteCategory);

module.exports = router;
