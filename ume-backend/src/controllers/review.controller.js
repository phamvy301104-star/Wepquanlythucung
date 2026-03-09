const Review = require('../models/Review');
const Product = require('../models/Product');
const Service = require('../models/Service');
const Staff = require('../models/Staff');

exports.getByProduct = async (req, res) => {
  try {
    const { page = 1, limit = 10 } = req.query;
    const query = { product: req.params.productId, isApproved: true, isDeleted: false };
    const total = await Review.countDocuments(query);
    const reviews = await Review.find(query)
      .populate('customer', 'fullName avatarUrl')
      .sort('-createdAt')
      .skip((page - 1) * limit)
      .limit(parseInt(limit));

    res.json({
      success: true,
      data: { reviews, pagination: { total, page: parseInt(page), limit: parseInt(limit), pages: Math.ceil(total / limit) } }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.getByService = async (req, res) => {
  try {
    const { page = 1, limit = 10 } = req.query;
    const query = { service: req.params.serviceId, isApproved: true, isDeleted: false };
    const total = await Review.countDocuments(query);
    const reviews = await Review.find(query)
      .populate('customer', 'fullName avatarUrl')
      .sort('-createdAt')
      .skip((page - 1) * limit)
      .limit(parseInt(limit));

    res.json({
      success: true,
      data: { reviews, pagination: { total, page: parseInt(page), limit: parseInt(limit), pages: Math.ceil(total / limit) } }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.getByStaff = async (req, res) => {
  try {
    const { page = 1, limit = 10 } = req.query;
    const query = { staff: req.params.staffId, isApproved: true, isDeleted: false };
    const total = await Review.countDocuments(query);
    const reviews = await Review.find(query)
      .populate('customer', 'fullName avatarUrl')
      .sort('-createdAt')
      .skip((page - 1) * limit)
      .limit(parseInt(limit));

    res.json({
      success: true,
      data: { reviews, pagination: { total, page: parseInt(page), limit: parseInt(limit), pages: Math.ceil(total / limit) } }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.create = async (req, res) => {
  try {
    const { productId, serviceId, staffId, appointmentId, orderId, rating, title, comment } = req.body;
    
    const review = new Review({
      customer: req.userId,
      product: productId,
      service: serviceId,
      staff: staffId,
      appointment: appointmentId,
      order: orderId,
      rating,
      title: title || '',
      comment: comment || ''
    });

    if (req.files) {
      review.images = req.files.map(f => `/uploads/reviews/${f.filename}`);
    }

    await review.save();

    // Update average rating on target
    if (productId) {
      const stats = await Review.aggregate([
        { $match: { product: review.product, isApproved: true, isDeleted: false } },
        { $group: { _id: null, avg: { $avg: '$rating' }, count: { $sum: 1 } } }
      ]);
      if (stats.length) {
        await Product.findByIdAndUpdate(productId, { averageRating: stats[0].avg, totalReviews: stats[0].count });
      }
    }
    if (serviceId) {
      const stats = await Review.aggregate([
        { $match: { service: review.service, isApproved: true, isDeleted: false } },
        { $group: { _id: null, avg: { $avg: '$rating' }, count: { $sum: 1 } } }
      ]);
      if (stats.length) {
        await Service.findByIdAndUpdate(serviceId, { averageRating: stats[0].avg, totalReviews: stats[0].count });
      }
    }
    if (staffId) {
      const stats = await Review.aggregate([
        { $match: { staff: review.staff, isApproved: true, isDeleted: false } },
        { $group: { _id: null, avg: { $avg: '$rating' }, count: { $sum: 1 } } }
      ]);
      if (stats.length) {
        await Staff.findByIdAndUpdate(staffId, { averageRating: stats[0].avg, totalReviews: stats[0].count });
      }
    }

    res.status(201).json({ success: true, message: 'Đánh giá thành công', data: review });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.reply = async (req, res) => {
  try {
    const review = await Review.findById(req.params.id);
    if (!review) return res.status(404).json({ success: false, message: 'Không tìm thấy đánh giá' });
    
    if (req.body.reply !== undefined) {
      review.reply = req.body.reply;
      review.repliedAt = new Date();
      review.repliedBy = req.userId;
    }
    if (req.body.adminReply !== undefined) {
      review.reply = req.body.adminReply;
      review.repliedAt = new Date();
      review.repliedBy = req.userId;
    }
    if (req.body.isApproved !== undefined) {
      review.isApproved = req.body.isApproved;
    }
    await review.save();
    res.json({ success: true, message: 'Cập nhật thành công', data: review });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.delete = async (req, res) => {
  try {
    const review = await Review.findById(req.params.id);
    if (!review) return res.status(404).json({ success: false, message: 'Không tìm thấy đánh giá' });

    if (review.customer.toString() !== req.userId.toString() && req.user.role !== 'Admin') {
      return res.status(403).json({ success: false, message: 'Không có quyền xóa' });
    }

    review.isDeleted = true;
    await review.save();
    res.json({ success: true, message: 'Xóa đánh giá thành công' });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

// Admin: get all reviews
exports.getAll = async (req, res) => {
  try {
    const { page = 1, limit = 20, isApproved } = req.query;
    const query = { isDeleted: false };
    if (isApproved !== undefined) query.isApproved = isApproved === 'true';

    const total = await Review.countDocuments(query);
    const reviews = await Review.find(query)
      .populate('customer', 'fullName avatarUrl')
      .populate('product', 'name')
      .populate('service', 'name')
      .populate('staff', 'fullName')
      .populate('appointment', 'appointmentCode services')
      .sort('-createdAt')
      .skip((page - 1) * limit)
      .limit(parseInt(limit));

    res.json({
      success: true,
      data: { reviews, pagination: { total, page: parseInt(page), limit: parseInt(limit), pages: Math.ceil(total / limit) } }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

// Check if user already reviewed an appointment
exports.checkByAppointment = async (req, res) => {
  try {
    const review = await Review.findOne({
      appointment: req.params.appointmentId,
      customer: req.userId,
      isDeleted: false
    });
    res.json({ success: true, data: { reviewed: !!review, review: review || null } });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

// Check which products in an order have been reviewed
exports.checkByOrder = async (req, res) => {
  try {
    const reviews = await Review.find({
      order: req.params.orderId,
      customer: req.userId,
      isDeleted: false
    }).select('product rating comment');
    const reviewedProductIds = reviews.map(r => r.product?.toString()).filter(Boolean);
    res.json({ success: true, data: { reviews, reviewedProductIds } });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};
