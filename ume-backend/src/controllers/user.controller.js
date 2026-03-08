const User = require('../models/User');
const bcrypt = require('bcryptjs');

// Get all users (admin) - only Admin & Staff
exports.getAll = async (req, res) => {
  try {
    const { page = 1, limit = 20, search, role, isActive } = req.query;
    const query = {};

    // Only show Admin and Staff accounts
    if (role) {
      query.role = role;
    } else {
      query.role = { $in: ['Admin', 'Staff'] };
    }

    if (search) {
      query.$or = [
        { fullName: { $regex: search, $options: 'i' } },
        { email: { $regex: search, $options: 'i' } },
        { phoneNumber: { $regex: search, $options: 'i' } }
      ];
    }
    if (isActive !== undefined) query.isActive = isActive === 'true';

    const total = await User.countDocuments(query);
    const users = await User.find(query)
      .select('-password -refreshTokens')
      .sort({ createdAt: -1 })
      .skip((page - 1) * limit)
      .limit(parseInt(limit));

    res.json({
      success: true,
      data: { users, pagination: { total, page: parseInt(page), limit: parseInt(limit), pages: Math.ceil(total / limit) } }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

// Get user by ID
exports.getById = async (req, res) => {
  try {
    const user = await User.findById(req.params.id).select('-password -refreshTokens');
    if (!user) return res.status(404).json({ success: false, message: 'Không tìm thấy người dùng' });
    res.json({ success: true, data: user });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

// Create user (admin only) - for creating Staff accounts
exports.create = async (req, res) => {
  try {
    const { email, password, fullName, phoneNumber, role, address, dateOfBirth, gender, idNumber, startDate, position, notes } = req.body;

    if (!email || !password) {
      return res.status(400).json({ success: false, message: 'Email và mật khẩu là bắt buộc' });
    }

    // Only allow creating Staff accounts
    if (role && role !== 'Staff') {
      return res.status(403).json({ success: false, message: 'Chỉ có thể tạo tài khoản nhân viên' });
    }

    const existingUser = await User.findOne({ email: email.toLowerCase() });
    if (existingUser) {
      return res.status(400).json({ success: false, message: 'Email đã được sử dụng' });
    }

    const user = new User({
      email: email.toLowerCase(),
      password,
      fullName: fullName || '',
      phoneNumber: phoneNumber || '',
      role: 'Staff',
      address: address || '',
      dateOfBirth: dateOfBirth || undefined,
      gender: gender || '',
      idNumber: idNumber || '',
      startDate: startDate || undefined,
      position: position || '',
      notes: notes || '',
      isActive: true,
      emailConfirmed: true
    });

    await user.save();

    res.status(201).json({ success: true, message: 'Tạo tài khoản thành công', data: user.toSafeObject() });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

// Update user (admin)
exports.update = async (req, res) => {
  try {
    const { fullName, phoneNumber, role, isActive, address, password, dateOfBirth, gender, idNumber, startDate, position, notes } = req.body;
    const user = await User.findById(req.params.id);
    if (!user) return res.status(404).json({ success: false, message: 'Không tìm thấy người dùng' });

    // Protect Admin accounts - cannot be edited by other users
    if (user.role === 'Admin' && req.user._id.toString() !== user._id.toString()) {
      return res.status(403).json({ success: false, message: 'Không thể chỉnh sửa tài khoản Admin' });
    }

    // Cannot change role to Admin
    if (role === 'Admin' && user.role !== 'Admin') {
      return res.status(403).json({ success: false, message: 'Không thể đổi vai trò thành Admin' });
    }

    if (fullName !== undefined) user.fullName = fullName;
    if (phoneNumber !== undefined) user.phoneNumber = phoneNumber;
    if (address !== undefined) user.address = address;
    if (dateOfBirth !== undefined) user.dateOfBirth = dateOfBirth || null;
    if (gender !== undefined) user.gender = gender;
    if (idNumber !== undefined) user.idNumber = idNumber;
    if (startDate !== undefined) user.startDate = startDate || null;
    if (position !== undefined) user.position = position;
    if (notes !== undefined) user.notes = notes;
    if (role && user.role !== 'Admin') user.role = role;
    if (isActive !== undefined && user.role !== 'Admin') user.isActive = isActive;
    if (password) user.password = password;
    await user.save();

    res.json({ success: true, message: 'Cập nhật thành công', data: user.toSafeObject() });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

// Delete user (admin)
exports.delete = async (req, res) => {
  try {
    const user = await User.findById(req.params.id);
    if (!user) return res.status(404).json({ success: false, message: 'Không tìm thấy người dùng' });

    // Protect Admin accounts from deletion
    if (user.role === 'Admin') {
      return res.status(403).json({ success: false, message: 'Không thể xóa tài khoản Admin' });
    }

    await User.findByIdAndDelete(req.params.id);
    res.json({ success: true, message: 'Đã xóa tài khoản thành công' });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

// Get user stats (admin)
exports.getStats = async (req, res) => {
  try {
    const staffQuery = { role: { $in: ['Admin', 'Staff'] } };
    const totalUsers = await User.countDocuments(staffQuery);
    const activeUsers = await User.countDocuments({ ...staffQuery, isActive: true });
    const adminUsers = await User.countDocuments({ role: 'Admin' });
    const staffUsers = await User.countDocuments({ role: 'Staff' });
    const newUsersToday = await User.countDocuments({
      createdAt: { $gte: new Date(new Date().setHours(0, 0, 0, 0)) }
    });

    res.json({
      success: true,
      data: { totalUsers, activeUsers, adminUsers, staffUsers, newUsersToday }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};
