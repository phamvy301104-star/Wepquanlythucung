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
    const staff = new Staff(req.body);
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
    Object.assign(staff, req.body);
    if (req.file) staff.avatarUrl = `/uploads/staff/${req.file.filename}`;
    await staff.save();
    res.json({ success: true, message: 'Cập nhật thành công', data: staff });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.delete = async (req, res) => {
  try {
    const staff = await Staff.findById(req.params.id);
    if (!staff) return res.status(404).json({ success: false, message: 'Không tìm thấy nhân viên' });
    staff.isDeleted = true;
    await staff.save();
    res.json({ success: true, message: 'Xóa nhân viên thành công' });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
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
