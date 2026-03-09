const Staff = require('../models/Staff');

exports.getAll = async (req, res) => {
  try {
    const { search, status, position } = req.query;
    const query = { isDeleted: false };
    if (search) {
      query.$or = [
        { fullName: { $regex: search, $options: 'i' } },
        { nickName: { $regex: search, $options: 'i' } },
        { staffCode: { $regex: search, $options: 'i' } }
      ];
    }
    if (status) query.status = status;
    if (position) query.position = position;

    const staff = await Staff.find(query).populate('services', 'name price duration').sort('-createdAt');
    res.json({ success: true, data: staff });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.getById = async (req, res) => {
  try {
    const staff = await Staff.findById(req.params.id).populate('services', 'name price duration imageUrl');
    if (!staff || staff.isDeleted) return res.status(404).json({ success: false, message: 'Không tìm thấy nhân viên' });
    res.json({ success: true, data: staff });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.create = async (req, res) => {
  try {
    const staffData = { ...req.body };
    // Auto-generate staffCode if not provided
    if (!staffData.staffCode) {
      const count = await Staff.countDocuments();
      staffData.staffCode = 'NV' + String(count + 1).padStart(4, '0');
    }
    const staff = new Staff(staffData);
    if (req.file) staff.avatarUrl = `/uploads/staff/${req.file.filename}`;
    await staff.save();
    res.status(201).json({ success: true, message: 'Thêm nhân viên thành công', data: staff });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.update = async (req, res) => {
  try {
    const staff = await Staff.findById(req.params.id);
    if (!staff) return res.status(404).json({ success: false, message: 'Không tìm thấy nhân viên' });

    const body = { ...req.body };
    // Handle services separately - comes as repeated fields in FormData
    const services = body.services;
    delete body.services;

    // Only assign known simple fields
    const allowedFields = ['fullName', 'nickName', 'email', 'phoneNumber', 'bio', 'position', 'level', 'specialties', 'yearsOfExperience', 'gender', 'hireDate', 'baseSalary', 'commissionPercent', 'status', 'facebookUrl', 'instagramUrl', 'tiktokUrl', 'dateOfBirth'];
    allowedFields.forEach(f => {
      if (body[f] !== undefined) staff[f] = body[f];
    });

    if (services !== undefined) {
      staff.services = Array.isArray(services) ? services : [services];
    }

    if (req.file) staff.avatarUrl = `/uploads/staff/${req.file.filename}`;
    await staff.save();
    res.json({ success: true, message: 'Cập nhật thành công', data: staff });
  } catch (error) {
    console.error('Staff update error:', error.message);
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.delete = async (req, res) => {
  try {
    await Staff.findByIdAndDelete(req.params.id);
    res.json({ success: true, message: 'Xóa nhân viên thành công' });
  } catch (error) {
    console.error('Staff delete error:', error.message);
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.getAvailable = async (req, res) => {
  try {
    const { date, serviceId } = req.query;
    const query = { status: 'Active', isDeleted: false };
    if (serviceId) query.services = serviceId;

    const staff = await Staff.find(query).select('fullName nickName avatarUrl position level schedule averageRating');
    res.json({ success: true, data: staff });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.getSchedule = async (req, res) => {
  try {
    const staff = await Staff.findById(req.params.id).select('schedule fullName');
    if (!staff) return res.status(404).json({ success: false, message: 'Không tìm thấy nhân viên' });
    res.json({ success: true, data: staff.schedule });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.updateSchedule = async (req, res) => {
  try {
    const staff = await Staff.findById(req.params.id);
    if (!staff) return res.status(404).json({ success: false, message: 'Không tìm thấy nhân viên' });
    staff.schedule = req.body.schedule;
    await staff.save();
    res.json({ success: true, message: 'Cập nhật lịch làm việc thành công', data: staff.schedule });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};
