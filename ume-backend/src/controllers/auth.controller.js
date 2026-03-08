const User = require('../models/User');
const jwt = require('jsonwebtoken');
const bcrypt = require('bcryptjs');
const axios = require('axios');
const { generateAccessToken, generateRefreshToken } = require('../middleware/auth');

// Register
exports.register = async (req, res) => {
  try {
    const { email, password, fullName, phoneNumber } = req.body;

    const existingUser = await User.findOne({ email: email.toLowerCase() });
    if (existingUser) {
      return res.status(400).json({ success: false, message: 'Email đã được sử dụng' });
    }

    const user = new User({
      email: email.toLowerCase(),
      password,
      fullName,
      phoneNumber: phoneNumber || ''
    });

    await user.save();

    const accessToken = generateAccessToken(user);
    const refreshToken = generateRefreshToken(user);
    
    user.refreshTokens.push({ token: refreshToken, expiresAt: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000) });
    await user.save();

    res.status(201).json({
      success: true,
      message: 'Đăng ký thành công',
      data: {
        user: user.toSafeObject(),
        accessToken,
        refreshToken
      }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

// Login
exports.login = async (req, res) => {
  try {
    const { email, password } = req.body;

    const user = await User.findOne({ email: email.toLowerCase() });
    if (!user) {
      return res.status(401).json({ success: false, message: 'Email hoặc mật khẩu không đúng' });
    }

    if (!user.isActive) {
      return res.status(403).json({ success: false, message: 'Tài khoản đã bị vô hiệu hóa' });
    }

    const isMatch = await user.comparePassword(password);
    if (!isMatch) {
      return res.status(401).json({ success: false, message: 'Email hoặc mật khẩu không đúng' });
    }

    const accessToken = generateAccessToken(user);
    const refreshToken = generateRefreshToken(user);
    
    user.refreshTokens.push({ token: refreshToken, expiresAt: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000) });
    user.lastLoginAt = new Date();
    await user.save();

    res.json({
      success: true,
      message: 'Đăng nhập thành công',
      data: {
        user: user.toSafeObject(),
        accessToken,
        refreshToken
      }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

// Google Login (Google Identity Services - credential/idToken)
exports.googleLogin = async (req, res) => {
  try {
    const { idToken } = req.body;
    if (!idToken) {
      return res.status(400).json({ success: false, message: 'Thiếu thông tin đăng nhập Google' });
    }

    // Verify token with Google
    const verifyRes = await axios.get(`https://oauth2.googleapis.com/tokeninfo?id_token=${encodeURIComponent(idToken)}`);
    const payload = verifyRes.data;

    if (payload.aud !== process.env.GOOGLE_CLIENT_ID) {
      return res.status(401).json({ success: false, message: 'Token không hợp lệ' });
    }

    const userData = {
      googleId: payload.sub,
      email: payload.email,
      fullName: payload.name,
      avatarUrl: payload.picture
    };

    let user = await User.findOne({ $or: [{ googleId: userData.googleId }, { email: userData.email }] });
    
    if (user) {
      user.googleId = userData.googleId;
      user.avatarUrl = user.avatarUrl || userData.avatarUrl;
      user.lastLoginAt = new Date();
      await user.save();
    } else {
      user = new User({
        email: userData.email,
        fullName: userData.fullName,
        googleId: userData.googleId,
        avatarUrl: userData.avatarUrl,
        password: Math.random().toString(36).slice(-12),
        isEmailVerified: true
      });
      await user.save();
    }

    const accessToken = generateAccessToken(user);
    const refreshToken = generateRefreshToken(user);
    
    user.refreshTokens.push({ token: refreshToken, expiresAt: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000) });
    await user.save();

    res.json({
      success: true,
      message: 'Đăng nhập Google thành công',
      data: { user: user.toSafeObject(), accessToken, refreshToken }
    });
  } catch (error) {
    console.error('Google login error:', error.response?.data || error.message);
    res.status(500).json({ success: false, message: 'Lỗi đăng nhập Google', error: error.message });
  }
};



// Facebook Login
exports.facebookLogin = async (req, res) => {
  try {
    const { accessToken: fbAccessToken } = req.body;
    
    const response = await axios.get(`https://graph.facebook.com/me?fields=id,name,email,picture.type(large)&access_token=${fbAccessToken}`);
    const { id, name, email, picture } = response.data;

    let user = await User.findOne({ $or: [{ facebookId: id }, ...(email ? [{ email }] : [])] });

    if (user) {
      user.facebookId = id;
      user.avatarUrl = user.avatarUrl || picture?.data?.url;
      user.lastLoginAt = new Date();
      await user.save();
    } else {
      user = new User({
        email: email || `fb_${id}@facebook.com`,
        fullName: name,
        facebookId: id,
        avatarUrl: picture?.data?.url,
        password: Math.random().toString(36).slice(-12),
        isEmailVerified: !!email
      });
      await user.save();
    }

    const accessToken = generateAccessToken(user);
    const refreshToken = generateRefreshToken(user);
    
    user.refreshTokens.push({ token: refreshToken, expiresAt: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000) });
    await user.save();

    res.json({
      success: true,
      message: 'Đăng nhập Facebook thành công',
      data: { user: user.toSafeObject(), accessToken, refreshToken }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi đăng nhập Facebook', error: error.message });
  }
};

// Refresh Token
exports.refreshToken = async (req, res) => {
  try {
    const { refreshToken } = req.body;
    if (!refreshToken) {
      return res.status(400).json({ success: false, message: 'Refresh token là bắt buộc' });
    }

    const decoded = jwt.verify(refreshToken, process.env.JWT_REFRESH_SECRET);
    const user = await User.findById(decoded.userId);
    
    if (!user) {
      return res.status(401).json({ success: false, message: 'Người dùng không tồn tại' });
    }

    const tokenExists = user.refreshTokens.some(t => t.token === refreshToken && t.expiresAt > new Date());
    if (!tokenExists) {
      return res.status(401).json({ success: false, message: 'Refresh token không hợp lệ hoặc đã hết hạn' });
    }

    // Remove old token and add new one
    user.refreshTokens = user.refreshTokens.filter(t => t.token !== refreshToken);
    const newAccessToken = generateAccessToken(user);
    const newRefreshToken = generateRefreshToken(user);
    user.refreshTokens.push({ token: newRefreshToken, expiresAt: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000) });
    await user.save();

    res.json({
      success: true,
      data: { accessToken: newAccessToken, refreshToken: newRefreshToken }
    });
  } catch (error) {
    res.status(401).json({ success: false, message: 'Refresh token không hợp lệ' });
  }
};

// Logout
exports.logout = async (req, res) => {
  try {
    const { refreshToken } = req.body;
    if (refreshToken && req.user) {
      req.user.refreshTokens = req.user.refreshTokens.filter(t => t.token !== refreshToken);
      await req.user.save();
    }
    res.json({ success: true, message: 'Đăng xuất thành công' });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

// Get current user profile
exports.getProfile = async (req, res) => {
  res.json({ success: true, data: req.user.toSafeObject() });
};

// Update profile
exports.updateProfile = async (req, res) => {
  try {
    const { fullName, phoneNumber, dateOfBirth, gender, address, avatarUrl } = req.body;
    const user = await User.findById(req.userId);

    if (fullName) user.fullName = fullName;
    if (phoneNumber) user.phoneNumber = phoneNumber;
    if (dateOfBirth) user.dateOfBirth = dateOfBirth;
    if (gender) user.gender = gender;
    if (avatarUrl) user.avatarUrl = avatarUrl;
    if (address !== undefined && address !== null) {
      if (typeof address === 'object') {
        // Frontend sends {street, ward, district, city} — join into string
        const parts = [address.street, address.ward, address.district, address.city].filter(Boolean);
        user.address = parts.join(', ');
      } else {
        user.address = address;
      }
    }

    await user.save();
    res.json({ success: true, message: 'Cập nhật thành công', data: user.toSafeObject() });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

// Change password
exports.changePassword = async (req, res) => {
  try {
    const { currentPassword, newPassword } = req.body;
    const user = await User.findById(req.userId);

    const isMatch = await user.comparePassword(currentPassword);
    if (!isMatch) {
      return res.status(400).json({ success: false, message: 'Mật khẩu hiện tại không đúng' });
    }

    user.password = newPassword;
    await user.save();
    res.json({ success: true, message: 'Đổi mật khẩu thành công' });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

// Upload avatar
exports.uploadAvatar = async (req, res) => {
  try {
    if (!req.file) {
      return res.status(400).json({ success: false, message: 'Vui lòng chọn ảnh' });
    }

    const user = await User.findById(req.userId);
    user.avatarUrl = `/uploads/avatars/${req.file.filename}`;
    await user.save();

    res.json({ success: true, message: 'Cập nhật ảnh đại diện thành công', data: { avatarUrl: user.avatarUrl } });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};
