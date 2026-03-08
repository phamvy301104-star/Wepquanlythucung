const router = require('express').Router();
const { auth, authorize } = require('../middleware/auth');
const ctrl = require('../controllers/admin.controller');

router.get('/dashboard', auth, authorize('Admin', 'Staff'), ctrl.getDashboard);
router.get('/revenue-chart', auth, authorize('Admin', 'Staff'), ctrl.getRevenueChart);

module.exports = router;
