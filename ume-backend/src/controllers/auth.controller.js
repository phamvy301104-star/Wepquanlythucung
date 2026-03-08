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

// Google Login
exports.googleLogin = async (req, res) => {
  try {
    const { idToken, accessToken: googleAccessToken } = req.body;
    let userData;

    if (idToken) {
      // Decode Google ID token
      const parts = idToken.split('.');
      if (parts.length !== 3) {
        return res.status(400).json({ success: false, message: 'Token không hợp lệ' });
      }
      const payload = JSON.parse(Buffer.from(parts[1], 'base64').toString());
      userData = {
        googleId: payload.sub,
        email: payload.email,
        fullName: payload.name,
        avatarUrl: payload.picture
      };
    } else if (googleAccessToken) {
      const response = await axios.get('https://www.googleapis.com/oauth2/v3/userinfo', {
        headers: { Authorization: `Bearer ${googleAccessToken}` }
      });
      userData = {
        googleId: response.data.sub,
        email: response.data.email,
        fullName: response.data.name,
        avatarUrl: response.data.picture
      };
    } else {
      return res.status(400).json({ success: false, message: 'Thiếu thông tin đăng nhập Google' });
    }

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
    res.status(500).json({ success: false, message: 'Lỗi đăng nhập Google', error: error.message });
  }
};

// Google OAuth - Step 1: Redirect to Google consent screen
exports.googleRedirect = (req, res) => {
  const clientId = process.env.GOOGLE_CLIENT_ID;
  const proto = req.headers['x-forwarded-proto'] || req.protocol;
  const redirectUri = `${proto}://${req.get('host')}/api/auth/google/callback`;
  const scope = encodeURIComponent('openid email profile');
  const url = `https://accounts.google.com/o/oauth2/v2/auth?client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUri)}&response_type=code&scope=${scope}&access_type=offline&prompt=consent`;
  res.redirect(url);
};

// Google OAuth - Step 2: Handle callback, exchange code for tokens
exports.googleCallback = async (req, res) => {
  try {
    const { code, error: oauthError } = req.query;
    
    if (oauthError || !code) {
      return res.send(`<html><body><script>
        window.opener.postMessage({ type: 'google-auth-error', error: '${oauthError || 'no_code'}' }, '*');
        window.close();
      </script></body></html>`);
    }

    const clientId = process.env.GOOGLE_CLIENT_ID;
    const clientSecret = process.env.GOOGLE_CLIENT_SECRET;
    const proto = req.headers['x-forwarded-proto'] || req.protocol;
    const redirectUri = `${proto}://${req.get('host')}/api/auth/google/callback`;

    // Exchange authorization code for tokens
    const tokenResponse = await axios.post('https://oauth2.googleapis.com/token', new URLSearchParams({
      code,
      client_id: clientId,
      client_secret: clientSecret,
      redirect_uri: redirectUri,
      grant_type: 'authorization_code'
    }).toString(), {
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
    });

    const { access_token } = tokenResponse.data;

    // Get user info from Google
    const userInfoResponse = await axios.get('https://www.googleapis.com/oauth2/v3/userinfo', {
      headers: { Authorization: `Bearer ${access_token}` }
    });

    const googleUser = userInfoResponse.data;
    const userData = {
      googleId: googleUser.sub,
      email: googleUser.email,
      fullName: googleUser.name,
      avatarUrl: googleUser.picture
    };

    // Find or create user
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

    const authData = JSON.stringify({
      success: true,
      data: { user: user.toSafeObject(), accessToken, refreshToken }
    });

    // Send result back to opener window via postMessage
    res.send(`<html><body><script>
      if (window.opener) {
        window.opener.postMessage({ type: 'google-auth-success', data: ${authData} }, '*');
        setTimeout(function() { window.close(); }, 300);
      } else {
        localStorage.setItem('google-auth-result', JSON.stringify(${authData}));
        window.close();
      }
    </script><p>Đăng nhập thành công! Đang chuyển hướng...</p></body></html>`);
  } catch (error) {
    console.error('Google callback error:', error.message);
    if (error.response) console.error('Google error data:', JSON.stringify(error.response.data));
    const errMsg = encodeURIComponent(error.message || 'server_error');
    res.send(`<html><body><script>
      if (window.opener) {
        window.opener.postMessage({ type: 'google-auth-error', error: '${errMsg}' }, '*');
      }
      setTimeout(function() { window.close(); }, 500);
    </script><p>Đang xử lý... cửa sổ sẽ tự đóng.</p></body></html>`);
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
