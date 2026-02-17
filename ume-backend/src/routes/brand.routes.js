const router = require('express').Router();
const { auth, authorize } = require('../middleware/auth');
const upload = require('../middleware/upload');
const ctrl = require('../controllers/brand.controller');

router.get('/', ctrl.getAll);
router.get('/:id', ctrl.getById);

router.post('/', auth, authorize('Admin', 'Staff'), (req, res, next) => {
  req.uploadSubDir = 'brands';
  next();
}, upload.single('logo'), ctrl.create);

router.put('/:id', auth, authorize('Admin', 'Staff'), (req, res, next) => {
  req.uploadSubDir = 'brands';
  next();
}, upload.single('logo'), ctrl.update);

router.delete('/:id', auth, authorize('Admin', 'Staff'), ctrl.delete);

module.exports = router;
