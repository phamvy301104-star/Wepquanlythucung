const router = require('express').Router();
const { auth, authorize } = require('../middleware/auth');
const ctrl = require('../controllers/settings.controller');

// Public - lấy thông tin liên hệ
router.get('/', ctrl.getSettings);

// Admin - cập nhật thông tin liên hệ
router.put('/', auth, authorize('Admin'), ctrl.updateSettings);

module.exports = router;
