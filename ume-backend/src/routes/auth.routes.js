const router = require('express').Router();
const { body } = require('express-validator');
const { validate } = require('../middleware/validate');
const { auth } = require('../middleware/auth');
const upload = require('../middleware/upload');
const ctrl = require('../controllers/auth.controller');

// Register
router.post('/register', [
  body('email').isEmail().withMessage('Email không hợp lệ'),
  body('password').isLength({ min: 6 }).withMessage('Mật khẩu tối thiểu 6 ký tự'),
  body('fullName').notEmpty().withMessage('Họ tên là bắt buộc'),
  validate
], ctrl.register);

// Login
router.post('/login', [
  body('email').isEmail().withMessage('Email không hợp lệ'),
  body('password').notEmpty().withMessage('Mật khẩu là bắt buộc'),
  validate
], ctrl.login);

// Google login
router.post('/google', ctrl.googleLogin);

// Facebook login
router.post('/facebook', ctrl.facebookLogin);

// Refresh token
router.post('/refresh-token', ctrl.refreshToken);

// Logout
router.post('/logout', auth, ctrl.logout);

// Get profile
router.get('/profile', auth, ctrl.getProfile);

// Update profile
router.put('/profile', auth, ctrl.updateProfile);

// Change password
router.put('/change-password', auth, [
  body('currentPassword').notEmpty().withMessage('Mật khẩu hiện tại là bắt buộc'),
  body('newPassword').isLength({ min: 6 }).withMessage('Mật khẩu mới tối thiểu 6 ký tự'),
  validate
], ctrl.changePassword);

// Upload avatar
router.post('/avatar', auth, (req, res, next) => {
  req.uploadSubDir = 'avatars';
  next();
}, upload.single('avatar'), ctrl.uploadAvatar);

module.exports = router;
