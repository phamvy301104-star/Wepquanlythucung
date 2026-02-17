const router = require('express').Router();
const { auth, authorize } = require('../middleware/auth');
const upload = require('../middleware/upload');
const ctrl = require('../controllers/staff.controller');

router.get('/', ctrl.getAll);
router.get('/available', ctrl.getAvailable);
router.get('/:id', ctrl.getById);
router.get('/:id/schedule', ctrl.getSchedule);

router.post('/', auth, authorize('Admin'), (req, res, next) => {
  req.uploadSubDir = 'staff';
  next();
}, upload.single('avatar'), ctrl.create);

router.put('/:id', auth, authorize('Admin'), (req, res, next) => {
  req.uploadSubDir = 'staff';
  next();
}, upload.single('avatar'), ctrl.update);

router.put('/:id/schedule', auth, authorize('Admin'), ctrl.updateSchedule);
router.delete('/:id', auth, authorize('Admin'), ctrl.delete);

module.exports = router;
