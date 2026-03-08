const Contact = require('../models/Contact');

// POST /api/contacts - Khách gửi liên hệ
exports.createContact = async (req, res) => {
  try {
    const { fullName, phone, email, subject, message } = req.body;

    if (!fullName || !phone || !email || !message) {
      return res.status(400).json({ success: false, message: 'Vui lòng điền đầy đủ thông tin bắt buộc' });
    }

    const contact = await Contact.create({ fullName, phone, email, subject: subject || 'Khác', message });

    // Gửi notification cho admin qua socket
    const io = req.app.get('io');
    if (io) {
      io.emit('new-contact', {
        message: `📩 Liên hệ mới từ ${fullName}: ${subject || 'Khác'}`,
        contact
      });
    }

    res.status(201).json({ success: true, message: 'Gửi liên hệ thành công', data: contact });
  } catch (error) {
    res.status(500).json({ success: false, message: error.message });
  }
};

// GET /api/contacts - Admin lấy danh sách liên hệ
exports.getContacts = async (req, res) => {
  try {
    const { page = 1, limit = 20, status, search } = req.query;
    const query = {};

    if (status && status !== 'all') query.status = status;
    if (search) {
      query.$or = [
        { fullName: { $regex: search, $options: 'i' } },
        { email: { $regex: search, $options: 'i' } },
        { phone: { $regex: search, $options: 'i' } },
        { message: { $regex: search, $options: 'i' } }
      ];
    }

    const total = await Contact.countDocuments(query);
    const contacts = await Contact.find(query)
      .sort({ createdAt: -1 })
      .skip((page - 1) * limit)
      .limit(parseInt(limit));

    // Đếm theo trạng thái
    const counts = {
      all: await Contact.countDocuments(),
      new: await Contact.countDocuments({ status: 'new' }),
      read: await Contact.countDocuments({ status: 'read' }),
      replied: await Contact.countDocuments({ status: 'replied' }),
      archived: await Contact.countDocuments({ status: 'archived' })
    };

    res.json({
      success: true,
      data: {
        contacts,
        counts,
        pagination: { total, page: parseInt(page), limit: parseInt(limit), pages: Math.ceil(total / limit) }
      }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: error.message });
  }
};

// GET /api/contacts/:id - Xem chi tiết & đánh dấu đã đọc
exports.getContact = async (req, res) => {
  try {
    const contact = await Contact.findById(req.params.id);
    if (!contact) return res.status(404).json({ success: false, message: 'Không tìm thấy' });

    // Tự động đánh dấu đã đọc
    if (contact.status === 'new') {
      contact.status = 'read';
      await contact.save();
    }

    res.json({ success: true, data: contact });
  } catch (error) {
    res.status(500).json({ success: false, message: error.message });
  }
};

// PUT /api/contacts/:id - Cập nhật trạng thái / ghi chú
exports.updateContact = async (req, res) => {
  try {
    const { status, adminNote } = req.body;
    const update = {};
    if (status) update.status = status;
    if (adminNote !== undefined) update.adminNote = adminNote;
    if (status === 'replied') {
      update.repliedAt = new Date();
      update.repliedBy = req.user?._id;
    }

    const contact = await Contact.findByIdAndUpdate(req.params.id, update, { new: true });
    if (!contact) return res.status(404).json({ success: false, message: 'Không tìm thấy' });

    res.json({ success: true, data: contact });
  } catch (error) {
    res.status(500).json({ success: false, message: error.message });
  }
};

// DELETE /api/contacts/:id - Xóa liên hệ
exports.deleteContact = async (req, res) => {
  try {
    const contact = await Contact.findByIdAndDelete(req.params.id);
    if (!contact) return res.status(404).json({ success: false, message: 'Không tìm thấy' });

    res.json({ success: true, message: 'Đã xóa liên hệ' });
  } catch (error) {
    res.status(500).json({ success: false, message: error.message });
  }
};
