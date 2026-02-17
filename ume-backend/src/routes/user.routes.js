const router = require('express').Router();
const { auth, authorize } = require('../middleware/auth');
const ctrl = require('../controllers/user.controller');

router.get('/', auth, authorize('Admin'), ctrl.getAll);
router.get('/stats', auth, authorize('Admin'), ctrl.getStats);
router.get('/:id', auth, authorize('Admin'), ctrl.getById);
router.post('/', auth, authorize('Admin'), ctrl.create);
router.put('/:id', auth, authorize('Admin'), ctrl.update);
router.delete('/:id', auth, authorize('Admin'), ctrl.delete);

module.exports = router;
