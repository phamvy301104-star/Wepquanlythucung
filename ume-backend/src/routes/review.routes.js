const router = require('express').Router();
const { auth, authorize } = require('../middleware/auth');
const upload = require('../middleware/upload');
const ctrl = require('../controllers/review.controller');

// Public
router.get('/product/:productId', ctrl.getByProduct);
router.get('/service/:serviceId', ctrl.getByService);
router.get('/staff/:staffId', ctrl.getByStaff);

// Auth required
router.get('/check/:appointmentId', auth, ctrl.checkByAppointment);
router.get('/check-order/:orderId', auth, ctrl.checkByOrder);
router.post('/', auth, (req, res, next) => {
  req.uploadSubDir = 'reviews';
  next();
}, upload.array('images', 5), ctrl.create);

router.delete('/:id', auth, ctrl.delete);

// Admin
router.get('/', auth, authorize('Admin'), ctrl.getAll);
router.put('/:id/reply', auth, authorize('Admin', 'Staff'), ctrl.reply);

module.exports = router;
