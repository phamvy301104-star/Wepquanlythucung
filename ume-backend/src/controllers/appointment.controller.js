const Appointment = require('../models/Appointment');
const Notification = require('../models/Notification');
const Service = require('../models/Service');
const User = require('../models/User');

exports.getAll = async (req, res) => {
  try {
    const { page = 1, limit = 20, status, staffId, date, search } = req.query;
    const query = { isDeleted: false };

    // Non-admin users can only see their own appointments
    if (req.user.role !== 'Admin' && req.user.role !== 'Staff') {
      query.customer = req.userId;
    }

    if (status) query.status = status;
    if (staffId) query.staff = staffId;
    if (date) {
      const d = new Date(date);
      query.appointmentDate = { $gte: d, $lt: new Date(d.getTime() + 86400000) };
    }
    if (search) query.appointmentCode = { $regex: search, $options: 'i' };

    const total = await Appointment.countDocuments(query);
    const appointments = await Appointment.find(query)
      .populate('customer', 'fullName email phoneNumber avatarUrl')
      .populate('staff', 'fullName nickName avatarUrl position')
      .populate('pet', 'name type breed imageUrl')
      .populate('services.service', 'name price')
      .sort('-appointmentDate')
      .skip((page - 1) * limit)
      .limit(parseInt(limit));

    res.json({
      success: true,
      data: { appointments, pagination: { total, page: parseInt(page), limit: parseInt(limit), pages: Math.ceil(total / limit) } }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.getById = async (req, res) => {
  try {
    const appointment = await Appointment.findById(req.params.id)
      .populate('customer', 'fullName email phoneNumber avatarUrl')
      .populate('staff', 'fullName nickName avatarUrl position')
      .populate('pet', 'name type breed imageUrl')
      .populate('services.service', 'name price duration');

    if (!appointment || appointment.isDeleted) {
      return res.status(404).json({ success: false, message: 'Không tìm thấy lịch hẹn' });
    }

    // Only owner, admin or assigned staff can view
    if (req.user.role !== 'Admin' && req.user.role !== 'Staff' && 
        appointment.customer._id.toString() !== req.userId.toString()) {
      return res.status(403).json({ success: false, message: 'Không có quyền truy cập' });
    }

    res.json({ success: true, data: appointment });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.create = async (req, res) => {
  try {
    const { services, staffId, staff, petId, petDescription, appointmentDate, startTime, notes, customerName, phone, email, petName } = req.body;

    // Map service data - accept both array of IDs and array of objects
    let serviceObjects = [];
    let totalAmount = 0;
    const svcInput = services || [];
    
    if (svcInput.length > 0) {
      // If services are objects with service/price, extract IDs
      const svcIds = svcInput.map((s) => (typeof s === 'object' && s.service) ? s.service : s);
      const serviceDocs = await Service.find({ _id: { $in: svcIds } });
      serviceObjects = serviceDocs.map(s => {
        const inputSvc = svcInput.find(is => (typeof is === 'object' ? is.service : is) === s._id.toString());
        return {
          service: s._id,
          serviceName: s.name,
          price: (inputSvc && typeof inputSvc === 'object' && inputSvc.price) ? inputSvc.price : (s.price || 0),
          duration: s.durationMinutes || 0
        };
      });
      totalAmount = serviceObjects.reduce((sum, s) => sum + s.price, 0);
    }

    const appointmentData = {
      customer: req.userId,
      services: serviceObjects,
      appointmentDate,
      startTime: startTime || (appointmentDate ? new Date(appointmentDate).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : '08:00'),
      notes: notes || '',
      totalAmount,
      finalAmount: totalAmount - (req.body.discountAmount || 0)
    };

    // Admin can specify staff by either staffId or staff field
    const staffValue = staffId || staff;
    if (staffValue) appointmentData.staff = staffValue;
    if (petId) appointmentData.pet = petId;
    if (petDescription && petDescription.name) {
      appointmentData.petDescription = petDescription;
    }
    // Store extra info for admin-created appointments
    if (customerName || petName) {
      appointmentData.petDescription = appointmentData.petDescription || {};
      if (petName) appointmentData.petDescription.name = petName;
    }
    if (customerName) appointmentData.notes = (appointmentData.notes ? appointmentData.notes + '\n' : '') + `KH: ${customerName}${phone ? ', SĐT: ' + phone : ''}${email ? ', Email: ' + email : ''}`;

    const appointment = new Appointment(appointmentData);
    await appointment.save();

    // Create notification for admin
    const admins = await User.find({ role: 'Admin', isActive: true }).select('_id');
    for (const admin of admins) {
      await new Notification({
        recipient: admin._id,
        title: 'Lịch hẹn mới',
        message: `Khách hàng ${req.user.fullName} đã đặt lịch hẹn ${appointment.appointmentCode}`,
        type: 'Appointment',
        referenceId: appointment._id.toString()
      }).save();
    }

    // Emit socket event
    if (req.app.get('io')) {
      req.app.get('io').emit('newAppointment', { appointment });
    }

    res.status(201).json({ success: true, message: 'Đặt lịch hẹn thành công', data: appointment });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.updateStatus = async (req, res) => {
  try {
    const { status, cancelReason } = req.body;
    const appointment = await Appointment.findById(req.params.id);
    if (!appointment) return res.status(404).json({ success: false, message: 'Không tìm thấy lịch hẹn' });

    appointment.status = status;
    if (status === 'Cancelled') {
      appointment.cancelReason = cancelReason || '';
      appointment.cancelledAt = new Date();
    }
    if (status === 'Confirmed') appointment.confirmedAt = new Date();
    if (status === 'Completed') appointment.completedAt = new Date();
    if (status === 'InProgress') appointment.checkinAt = new Date();

    await appointment.save();

    // Notify customer
    await new Notification({
      recipient: appointment.customer,
      title: 'Cập nhật lịch hẹn',
      message: `Lịch hẹn ${appointment.appointmentCode} đã được cập nhật: ${status}`,
      type: 'Appointment',
      referenceId: appointment._id.toString()
    }).save();

    if (req.app.get('io')) {
      req.app.get('io').to(appointment.customer.toString()).emit('appointmentUpdate', { appointment });
    }

    res.json({ success: true, message: 'Cập nhật trạng thái thành công', data: appointment });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.cancel = async (req, res) => {
  try {
    const appointment = await Appointment.findById(req.params.id);
    if (!appointment) return res.status(404).json({ success: false, message: 'Không tìm thấy lịch hẹn' });

    if (appointment.customer.toString() !== req.userId.toString() && req.user.role !== 'Admin') {
      return res.status(403).json({ success: false, message: 'Không có quyền hủy' });
    }

    if (['Completed', 'Cancelled'].includes(appointment.status)) {
      return res.status(400).json({ success: false, message: 'Không thể hủy lịch hẹn này' });
    }

    appointment.status = 'Cancelled';
    appointment.cancelReason = req.body.cancelReason || 'Khách hàng hủy';
    appointment.cancelledAt = new Date();
    await appointment.save();

    res.json({ success: true, message: 'Hủy lịch hẹn thành công', data: appointment });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.delete = async (req, res) => {
  try {
    const appointment = await Appointment.findById(req.params.id);
    if (!appointment) return res.status(404).json({ success: false, message: 'Không tìm thấy lịch hẹn' });
    appointment.isDeleted = true;
    await appointment.save();
    res.json({ success: true, message: 'Đã xóa lịch hẹn thành công' });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.getMyAppointments = async (req, res) => {
  try {
    const { status, page = 1, limit = 10 } = req.query;
    const query = { customer: req.userId, isDeleted: false };
    if (status) query.status = status;

    const total = await Appointment.countDocuments(query);
    const appointments = await Appointment.find(query)
      .populate('staff', 'fullName nickName avatarUrl')
      .populate('pet', 'name type breed imageUrl')
      .populate('services.service', 'name price')
      .sort('-appointmentDate')
      .skip((page - 1) * limit)
      .limit(parseInt(limit));

    res.json({
      success: true,
      data: { appointments, pagination: { total, page: parseInt(page), limit: parseInt(limit), pages: Math.ceil(total / limit) } }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.getStats = async (req, res) => {
  try {
    const today = new Date(new Date().setHours(0, 0, 0, 0));
    const stats = {
      total: await Appointment.countDocuments({ isDeleted: false }),
      pending: await Appointment.countDocuments({ status: 'Pending', isDeleted: false }),
      confirmed: await Appointment.countDocuments({ status: 'Confirmed', isDeleted: false }),
      today: await Appointment.countDocuments({ appointmentDate: { $gte: today, $lt: new Date(today.getTime() + 86400000) }, isDeleted: false }),
      completed: await Appointment.countDocuments({ status: 'Completed', isDeleted: false }),
      cancelled: await Appointment.countDocuments({ status: 'Cancelled', isDeleted: false })
    };
    res.json({ success: true, data: stats });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};
