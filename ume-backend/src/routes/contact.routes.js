const express = require('express');
const router = express.Router();
const contactController = require('../controllers/contact.controller');
const { auth, authorize } = require('../middleware/auth');

// Public - Khách gửi liên hệ
router.post('/', contactController.createContact);

// Admin - Quản lý liên hệ
router.get('/', auth, authorize('Admin', 'Staff'), contactController.getContacts);
router.get('/:id', auth, authorize('Admin'), contactController.getContact);
router.put('/:id', auth, authorize('Admin'), contactController.updateContact);
router.delete('/:id', auth, authorize('Admin'), contactController.deleteContact);

module.exports = router;
