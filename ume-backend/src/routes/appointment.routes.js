const router = require('express').Router();
const { auth, authorize } = require('../middleware/auth');
const ctrl = require('../controllers/appointment.controller');

router.get('/', auth, ctrl.getAll);
router.get('/my', auth, ctrl.getMyAppointments);
router.get('/stats', auth, authorize('Admin', 'Staff'), ctrl.getStats);
router.get('/:id', auth, ctrl.getById);
router.post('/', auth, ctrl.create);
router.put('/:id/status', auth, authorize('Admin', 'Staff'), ctrl.updateStatus);
router.put('/:id/cancel', auth, ctrl.cancel);
router.delete('/:id', auth, authorize('Admin'), ctrl.delete);

module.exports = router;
