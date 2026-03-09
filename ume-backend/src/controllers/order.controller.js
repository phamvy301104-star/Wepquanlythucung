const Order = require('../models/Order');
const Product = require('../models/Product');
const Pet = require('../models/Pet');
const Notification = require('../models/Notification');
const Promotion = require('../models/Promotion');

exports.getAll = async (req, res) => {
  try {
    const { page = 1, limit = 20, status, search } = req.query;
    const query = { isDeleted: false };

    if (req.user.role !== 'Admin') {
      query.customer = req.userId;
    }
    if (status) query.status = status;
    if (search) query.orderCode = { $regex: search, $options: 'i' };

    const total = await Order.countDocuments(query);
    const orders = await Order.find(query)
      .populate('customer', 'fullName email phoneNumber')
      .sort('-createdAt')
      .skip((page - 1) * limit)
      .limit(parseInt(limit));

    res.json({
      success: true,
      data: { orders, pagination: { total, page: parseInt(page), limit: parseInt(limit), pages: Math.ceil(total / limit) } }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.getById = async (req, res) => {
  try {
    const order = await Order.findById(req.params.id)
      .populate('customer', 'fullName email phoneNumber')
      .populate('items.product', 'name mainImage slug');

    if (!order || order.isDeleted) return res.status(404).json({ success: false, message: 'Không tìm thấy đơn hàng' });

    if (req.user.role !== 'Admin' && order.customer._id.toString() !== req.userId.toString()) {
      return res.status(403).json({ success: false, message: 'Không có quyền truy cập' });
    }

    res.json({ success: true, data: order });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.create = async (req, res) => {
  try {
    const { items, shippingAddress, paymentMethod, notes, promotionCode } = req.body;

    // Validate and populate items
    const orderItems = [];
    let subtotal = 0;

    for (const item of items) {
      // Handle pet items
      if (item.petId) {
        const pet = await Pet.findById(item.petId);
        if (!pet) return res.status(400).json({ success: false, message: `Thú cưng ${item.petId} không tồn tại` });
        if (pet.listingStatus !== 'Active') return res.status(400).json({ success: false, message: `${pet.name} không còn được bán/nhận nuôi` });

        const petPrice = pet.listingPrice || 0;
        orderItems.push({
          pet: pet._id,
          productName: `[Thú cưng] ${pet.name}`,
          productImage: pet.imageUrl || '',
          sku: '',
          quantity: 1,
          unitPrice: petPrice,
          totalPrice: petPrice,
          notes: item.notes || `${pet.type} - ${pet.breed || ''} - ${pet.listingType}`
        });
        subtotal += petPrice;

        // Mark pet as sold/adopted
        pet.listingStatus = pet.listingType === 'Adoption' ? 'Adopted' : 'Sold';
        await pet.save();
        continue;
      }

      // Handle product items
      const product = await Product.findById(item.productId);
      if (!product) return res.status(400).json({ success: false, message: `Sản phẩm ${item.productId} không tồn tại` });
      if (product.stockQuantity < item.quantity) return res.status(400).json({ success: false, message: `Sản phẩm ${product.name} không đủ số lượng` });

      const totalPrice = product.salePrice || product.price;
      orderItems.push({
        product: product._id,
        productName: product.name,
        productImage: product.mainImage,
        sku: product.sku,
        quantity: item.quantity,
        unitPrice: totalPrice,
        totalPrice: totalPrice * item.quantity,
        notes: item.notes || ''
      });
      subtotal += totalPrice * item.quantity;

      // Decrease stock
      product.stockQuantity -= item.quantity;
      product.soldCount += item.quantity;
      await product.save();
    }

    // Apply promotion code if provided
    let discountAmount = 0;
    let appliedPromoCode = '';
    const shippingFee = req.body.shippingFee || 0;

    if (promotionCode) {
      const now = new Date();
      const promotion = await Promotion.findOne({
        code: promotionCode.toUpperCase(),
        isActive: true,
        isDeleted: false,
        startDate: { $lte: now },
        endDate: { $gte: now }
      });

      if (promotion) {
        const canUse = promotion.usageLimit === 0 || promotion.usedCount < promotion.usageLimit;
        const meetsMinimum = promotion.minOrderAmount === 0 || subtotal >= promotion.minOrderAmount;

        if (canUse && meetsMinimum) {
          if (promotion.type === 'Percentage') {
            discountAmount = (subtotal * promotion.value) / 100;
            if (promotion.maxDiscountAmount > 0) {
              discountAmount = Math.min(discountAmount, promotion.maxDiscountAmount);
            }
          } else if (promotion.type === 'FixedAmount') {
            discountAmount = Math.min(promotion.value, subtotal);
          } else if (promotion.type === 'FreeShipping') {
            discountAmount = shippingFee;
          }

          discountAmount = Math.round(discountAmount);
          appliedPromoCode = promotion.code;

          // Increment usage count
          promotion.usedCount += 1;
          await promotion.save();
        }
      }
    }

    const totalAmount = Math.max(0, subtotal + shippingFee - discountAmount);

    const order = new Order({
      customer: req.userId,
      items: orderItems,
      subtotal,
      shippingFee,
      discountAmount,
      totalAmount,
      shippingAddress,
      paymentMethod: paymentMethod || 'COD',
      notes: notes || '',
      promotionCode: appliedPromoCode,
      statusHistory: [{ status: 'Pending', note: 'Đơn hàng được tạo', changedBy: req.userId }]
    });

    await order.save();

    // Notification
    if (req.app.get('io')) {
      req.app.get('io').emit('newOrder', { order });
    }

    res.status(201).json({ success: true, message: 'Đặt hàng thành công', data: order });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.updateStatus = async (req, res) => {
  try {
    const { status: rawStatus, note } = req.body;
    // Normalize status name (frontend may use 'Shipped' for 'Shipping')
    const statusMap = { 'Shipped': 'Shipping' };
    const status = statusMap[rawStatus] || rawStatus;
    const order = await Order.findById(req.params.id);
    if (!order) return res.status(404).json({ success: false, message: 'Không tìm thấy đơn hàng' });

    order.status = status;
    order.statusHistory.push({ status, note: note || '', changedBy: req.userId, changedAt: new Date() });

    if (status === 'Confirmed') order.confirmedAt = new Date();
    if (status === 'Shipping') order.shippedAt = new Date();
    if (status === 'Delivered') order.deliveredAt = new Date();
    if (status === 'Completed') { order.completedAt = new Date(); order.paymentStatus = 'Paid'; }
    if (status === 'Cancelled') {
      order.cancelledAt = new Date();
      order.cancelReason = req.body.cancelReason || '';
      // Restore stock
      for (const item of order.items) {
        if (item.product) {
          await Product.findByIdAndUpdate(item.product, { $inc: { stockQuantity: item.quantity, soldCount: -item.quantity } });
        }
      }
    }

    await order.save();

    // Notify customer
    await new Notification({
      recipient: order.customer,
      title: 'Cập nhật đơn hàng',
      message: `Đơn hàng ${order.orderCode} đã được cập nhật: ${status}`,
      type: 'Order',
      referenceId: order._id.toString()
    }).save();

    res.json({ success: true, message: 'Cập nhật thành công', data: order });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.cancelOrder = async (req, res) => {
  try {
    const order = await Order.findById(req.params.id);
    if (!order) return res.status(404).json({ success: false, message: 'Không tìm thấy đơn hàng' });

    if (order.customer.toString() !== req.userId.toString() && req.user.role !== 'Admin') {
      return res.status(403).json({ success: false, message: 'Không có quyền hủy' });
    }

    if (!['Pending', 'Confirmed'].includes(order.status)) {
      return res.status(400).json({ success: false, message: 'Không thể hủy đơn hàng này' });
    }

    order.status = 'Cancelled';
    order.cancelReason = req.body.cancelReason || 'Khách hàng hủy';
    order.cancelledAt = new Date();
    order.statusHistory.push({ status: 'Cancelled', note: order.cancelReason, changedBy: req.userId });

    // Restore stock
    for (const item of order.items) {
      if (item.product) {
        await Product.findByIdAndUpdate(item.product, { $inc: { stockQuantity: item.quantity, soldCount: -item.quantity } });
      }
    }

    await order.save();
    res.json({ success: true, message: 'Hủy đơn hàng thành công', data: order });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.getMyOrders = async (req, res) => {
  try {
    const { status, page = 1, limit = 10 } = req.query;
    const query = { customer: req.userId, isDeleted: false };
    if (status) query.status = status;

    const total = await Order.countDocuments(query);
    const orders = await Order.find(query).sort('-createdAt').skip((page - 1) * limit).limit(parseInt(limit));

    res.json({
      success: true,
      data: { orders, pagination: { total, page: parseInt(page), limit: parseInt(limit), pages: Math.ceil(total / limit) } }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.deleteOrder = async (req, res) => {
  try {
    const order = await Order.findById(req.params.id);
    if (!order) return res.status(404).json({ success: false, message: 'Không tìm thấy đơn hàng' });
    // Soft delete
    order.isDeleted = true;
    await order.save();
    res.json({ success: true, message: 'Đã xóa đơn hàng thành công' });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.getStats = async (req, res) => {
  try {
    const today = new Date(new Date().setHours(0, 0, 0, 0));
    const stats = {
      total: await Order.countDocuments({ isDeleted: false }),
      pending: await Order.countDocuments({ status: 'Pending', isDeleted: false }),
      todayOrders: await Order.countDocuments({ createdAt: { $gte: today }, isDeleted: false }),
      completed: await Order.countDocuments({ status: 'Completed', isDeleted: false }),
      totalRevenue: (await Order.aggregate([
        { $match: { status: 'Completed', isDeleted: false } },
        { $group: { _id: null, total: { $sum: '$totalAmount' } } }
      ]))[0]?.total || 0
    };
    res.json({ success: true, data: stats });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};
