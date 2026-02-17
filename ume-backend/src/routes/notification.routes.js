const router = require('express').Router();
const { auth, authorize } = require('../middleware/auth');
const ctrl = require('../controllers/notification.controller');

router.get('/', auth, ctrl.getAll);
router.get('/unread-count', auth, ctrl.getUnreadCount);
router.put('/:id/read', auth, ctrl.markAsRead);
router.put('/read-all', auth, ctrl.markAllAsRead);
router.delete('/:id', auth, ctrl.delete);

// Admin
router.post('/send', auth, authorize('Admin'), ctrl.sendToUser);

module.exports = router;
