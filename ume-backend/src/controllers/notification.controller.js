const Notification = require('../models/Notification');

exports.getAll = async (req, res) => {
  try {
    const { page = 1, limit = 20, isRead } = req.query;
    const query = { recipient: req.userId };
    if (isRead !== undefined) query.isRead = isRead === 'true';

    const total = await Notification.countDocuments(query);
    const notifications = await Notification.find(query)
      .sort('-createdAt')
      .skip((page - 1) * limit)
      .limit(parseInt(limit));

    const unreadCount = await Notification.countDocuments({ recipient: req.userId, isRead: false });

    res.json({
      success: true,
      data: { notifications, unreadCount, pagination: { total, page: parseInt(page), limit: parseInt(limit), pages: Math.ceil(total / limit) } }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.markAsRead = async (req, res) => {
  try {
    const notification = await Notification.findById(req.params.id);
    if (!notification) return res.status(404).json({ success: false, message: 'Không tìm thấy thông báo' });

    if (notification.recipient.toString() !== req.userId.toString()) {
      return res.status(403).json({ success: false, message: 'Không có quyền' });
    }

    notification.isRead = true;
    notification.readAt = new Date();
    await notification.save();
    res.json({ success: true, data: notification });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.markAllAsRead = async (req, res) => {
  try {
    await Notification.updateMany(
      { recipient: req.userId, isRead: false },
      { isRead: true, readAt: new Date() }
    );
    res.json({ success: true, message: 'Đã đánh dấu tất cả đã đọc' });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.delete = async (req, res) => {
  try {
    const notification = await Notification.findById(req.params.id);
    if (!notification) return res.status(404).json({ success: false, message: 'Không tìm thấy thông báo' });

    if (notification.recipient.toString() !== req.userId.toString()) {
      return res.status(403).json({ success: false, message: 'Không có quyền' });
    }

    await notification.deleteOne();
    res.json({ success: true, message: 'Xóa thông báo thành công' });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.getUnreadCount = async (req, res) => {
  try {
    const count = await Notification.countDocuments({ recipient: req.userId, isRead: false });
    res.json({ success: true, data: { count } });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

// Admin: send notification to user
exports.sendToUser = async (req, res) => {
  try {
    const { recipientId, title, message, type } = req.body;
    const notification = new Notification({
      recipient: recipientId,
      title,
      message,
      type: type || 'System'
    });
    await notification.save();

    if (req.app.get('io')) {
      req.app.get('io').to(recipientId).emit('notification', notification);
    }

    res.status(201).json({ success: true, message: 'Gửi thông báo thành công', data: notification });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};
