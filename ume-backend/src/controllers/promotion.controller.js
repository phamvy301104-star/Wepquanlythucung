const Promotion = require('../models/Promotion');

// Get all promotions (Admin)
exports.getAll = async (req, res) => {
  try {
    const { page = 1, limit = 20, search, isActive, type } = req.query;
    const query = { isDeleted: false };
    
    if (search) {
      query.$or = [
        { code: { $regex: search, $options: 'i' } },
        { name: { $regex: search, $options: 'i' } }
      ];
    }
    if (isActive !== undefined) query.isActive = isActive === 'true';
    if (type) query.type = type;

    const total = await Promotion.countDocuments(query);
    const promotions = await Promotion.find(query)
      .sort('-createdAt')
      .skip((page - 1) * limit)
      .limit(parseInt(limit));

    res.json({
      success: true,
      data: { promotions, pagination: { total, page: parseInt(page), limit: parseInt(limit), pages: Math.ceil(total / limit) } }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

// Get single promotion
exports.getById = async (req, res) => {
  try {
    const promotion = await Promotion.findById(req.params.id);
    if (!promotion || promotion.isDeleted) {
      return res.status(404).json({ success: false, message: 'Không tìm thấy khuyến mãi' });
    }
    res.json({ success: true, data: promotion });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

// Create promotion
exports.create = async (req, res) => {
  try {
    const { code, name, description, type, value, minOrderAmount, maxDiscountAmount, usageLimit, perUserLimit, startDate, endDate, applicableProducts, applicableCategories, applicableServices, isActive } = req.body;

    const existing = await Promotion.findOne({ code: code?.toUpperCase(), isDeleted: false });
    if (existing) {
      return res.status(400).json({ success: false, message: 'Mã khuyến mãi đã tồn tại' });
    }

    const promotion = await Promotion.create({
      code, name, description, type, value,
      minOrderAmount: minOrderAmount || 0,
      maxDiscountAmount: maxDiscountAmount || 0,
      usageLimit: usageLimit || 0,
      perUserLimit: perUserLimit || 1,
      startDate, endDate,
      applicableProducts: applicableProducts || [],
      applicableCategories: applicableCategories || [],
      applicableServices: applicableServices || [],
      isActive: isActive !== false
    });

    res.status(201).json({ success: true, message: 'Tạo khuyến mãi thành công', data: promotion });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

// Update promotion
exports.update = async (req, res) => {
  try {
    const promotion = await Promotion.findById(req.params.id);
    if (!promotion || promotion.isDeleted) {
      return res.status(404).json({ success: false, message: 'Không tìm thấy khuyến mãi' });
    }

    const fields = ['name', 'description', 'type', 'value', 'minOrderAmount', 'maxDiscountAmount', 'usageLimit', 'perUserLimit', 'startDate', 'endDate', 'applicableProducts', 'applicableCategories', 'applicableServices', 'isActive'];
    fields.forEach(f => { if (req.body[f] !== undefined) promotion[f] = req.body[f]; });

    if (req.body.code) {
      const existing = await Promotion.findOne({ code: req.body.code.toUpperCase(), _id: { $ne: promotion._id }, isDeleted: false });
      if (existing) return res.status(400).json({ success: false, message: 'Mã khuyến mãi đã tồn tại' });
      promotion.code = req.body.code;
    }

    await promotion.save();
    res.json({ success: true, message: 'Cập nhật thành công', data: promotion });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

// Delete promotion (soft delete)
exports.delete = async (req, res) => {
  try {
    const promotion = await Promotion.findById(req.params.id);
    if (!promotion || promotion.isDeleted) {
      return res.status(404).json({ success: false, message: 'Không tìm thấy khuyến mãi' });
    }
    promotion.isDeleted = true;
    await promotion.save();
    res.json({ success: true, message: 'Xóa khuyến mãi thành công' });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

// Validate promotion code (public)
exports.validate = async (req, res) => {
  try {
    const { code, orderAmount } = req.body;
    const now = new Date();
    const promotion = await Promotion.findOne({
      code: code?.toUpperCase(),
      isActive: true,
      isDeleted: false,
      startDate: { $lte: now },
      endDate: { $gte: now }
    });

    if (!promotion) {
      return res.status(404).json({ success: false, message: 'Mã khuyến mãi không hợp lệ hoặc đã hết hạn' });
    }

    if (promotion.usageLimit > 0 && promotion.usedCount >= promotion.usageLimit) {
      return res.status(400).json({ success: false, message: 'Mã khuyến mãi đã hết lượt sử dụng' });
    }

    if (promotion.minOrderAmount > 0 && orderAmount < promotion.minOrderAmount) {
      return res.status(400).json({ success: false, message: `Đơn hàng tối thiểu ${promotion.minOrderAmount.toLocaleString()}đ` });
    }

    let discount = 0;
    if (promotion.type === 'Percentage') {
      discount = (orderAmount * promotion.value) / 100;
      if (promotion.maxDiscountAmount > 0) discount = Math.min(discount, promotion.maxDiscountAmount);
    } else if (promotion.type === 'FixedAmount') {
      discount = promotion.value;
    } else if (promotion.type === 'FreeShipping') {
      discount = 0; // handled separately
    }

    res.json({ success: true, data: { promotion, discount } });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};
