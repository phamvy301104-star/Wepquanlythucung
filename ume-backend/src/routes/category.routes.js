const router = require('express').Router();
const { auth, authorize } = require('../middleware/auth');
const upload = require('../middleware/upload');
const ctrl = require('../controllers/category.controller');

router.get('/', ctrl.getAll);
router.get('/tree', ctrl.getTree);
router.get('/:id', ctrl.getById);

router.post('/', auth, authorize('Admin', 'Staff'), (req, res, next) => {
  req.uploadSubDir = 'categories';
  next();
}, upload.single('image'), ctrl.create);

router.put('/:id', auth, authorize('Admin', 'Staff'), (req, res, next) => {
  req.uploadSubDir = 'categories';
  next();
}, upload.single('image'), ctrl.update);

router.delete('/:id', auth, authorize('Admin', 'Staff'), ctrl.delete);

module.exports = router;
