const router = require('express').Router();
const { auth, authorize } = require('../middleware/auth');
const ctrl = require('../controllers/order.controller');

router.get('/', auth, ctrl.getAll);
router.get('/my', auth, ctrl.getMyOrders);
router.get('/stats', auth, authorize('Admin'), ctrl.getStats);
router.get('/:id', auth, ctrl.getById);
router.post('/', auth, ctrl.create);
router.put('/:id/status', auth, authorize('Admin'), ctrl.updateStatus);
router.put('/:id/cancel', auth, ctrl.cancelOrder);
router.delete('/:id', auth, authorize('Admin'), ctrl.deleteOrder);

module.exports = router;
