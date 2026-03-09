const Appointment = require('../models/Appointment');
const Order = require('../models/Order');
const User = require('../models/User');
const Product = require('../models/Product');
const Staff = require('../models/Staff');
const Pet = require('../models/Pet');
const Service = require('../models/Service');
const Review = require('../models/Review');
const Promotion = require('../models/Promotion');

exports.getDashboard = async (req, res) => {
  try {
    const today = new Date(new Date().setHours(0, 0, 0, 0));
    const tomorrow = new Date(today.getTime() + 86400000);
    const thisMonth = new Date(new Date().getFullYear(), new Date().getMonth(), 1);
    const lastMonth = new Date(new Date().getFullYear(), new Date().getMonth() - 1, 1);
    const lastMonthEnd = new Date(new Date().getFullYear(), new Date().getMonth(), 0, 23, 59, 59);

    const [
      totalUsers,
      totalCustomers,
      newCustomersMonth,
      totalProducts,
      lowStockProducts,
      totalServices,
      totalStaff,
      activeStaff,
      totalPets,
      totalOrders,
      ordersToday,
      totalAppointments,
      todayAppointmentsCount,
      pendingOrders,
      pendingAppointments,
      appointmentsMonth,
      todayRevenue,
      monthlyRevenue,
      lastMonthRevenue,
      recentOrders,
      todayAppointments,
      orderStatusAgg,
      revenueChart
    ] = await Promise.all([
      User.countDocuments({ isActive: true }),
      User.countDocuments({ isActive: true, role: 'Customer' }),
      User.countDocuments({ role: 'Customer', createdAt: { $gte: thisMonth } }),
      Product.countDocuments({ isDeleted: { $ne: true } }),
      Product.countDocuments({ isDeleted: { $ne: true }, stockQuantity: { $lte: 5, $gt: 0 } }),
      Service.countDocuments({ isActive: true }),
      Staff.countDocuments({ isDeleted: { $ne: true } }),
      Staff.countDocuments({ isDeleted: { $ne: true }, status: 'Active' }),
      Pet.countDocuments({ isDeleted: { $ne: true } }),
      Order.countDocuments({ isDeleted: { $ne: true } }),
      Order.countDocuments({ isDeleted: { $ne: true }, createdAt: { $gte: today, $lt: tomorrow } }),
      Appointment.countDocuments({ isDeleted: { $ne: true } }),
      Appointment.countDocuments({ appointmentDate: { $gte: today, $lt: tomorrow }, isDeleted: { $ne: true } }),
      Order.countDocuments({ status: 'Pending', isDeleted: { $ne: true } }),
      Appointment.countDocuments({ status: 'Pending', isDeleted: { $ne: true } }),
      Appointment.countDocuments({ appointmentDate: { $gte: thisMonth }, isDeleted: { $ne: true } }),
      Order.aggregate([
        { $match: { status: 'Completed', createdAt: { $gte: today, $lt: tomorrow }, isDeleted: { $ne: true } } },
        { $group: { _id: null, total: { $sum: '$totalAmount' } } }
      ]),
      Order.aggregate([
        { $match: { status: 'Completed', createdAt: { $gte: thisMonth }, isDeleted: { $ne: true } } },
        { $group: { _id: null, total: { $sum: '$totalAmount' } } }
      ]),
      Order.aggregate([
        { $match: { status: 'Completed', createdAt: { $gte: lastMonth, $lte: lastMonthEnd }, isDeleted: { $ne: true } } },
        { $group: { _id: null, total: { $sum: '$totalAmount' } } }
      ]),
      Order.find({ isDeleted: { $ne: true } }).sort('-createdAt').limit(5).populate('customer', 'fullName'),
      Appointment.find({ appointmentDate: { $gte: today, $lt: tomorrow }, isDeleted: { $ne: true } })
        .sort('startTime').limit(10)
        .populate('customer', 'fullName phoneNumber')
        .populate('staff', 'fullName')
        .populate('services.service', 'name'),
      Order.aggregate([
        { $match: { isDeleted: { $ne: true } } },
        { $group: { _id: '$status', count: { $sum: 1 } } }
      ]),
      // Revenue last 7 days
      Order.aggregate([
        { $match: { status: 'Completed', isDeleted: { $ne: true }, createdAt: { $gte: new Date(today.getTime() - 6 * 86400000) } } },
        { $group: { _id: { $dateToString: { format: '%Y-%m-%d', date: '$createdAt' } }, totalRevenue: { $sum: '$totalAmount' }, count: { $sum: 1 } } },
        { $sort: { _id: 1 } }
      ])
    ]);

    // Build order status chart data
    const orderStatus = {};
    const statusMap = { Pending: 'pending', Confirmed: 'confirmed', Processing: 'processing', Shipping: 'shipping', Completed: 'completed', Cancelled: 'cancelled' };
    orderStatusAgg.forEach(s => {
      const key = statusMap[s._id] || s._id?.toLowerCase();
      if (key) orderStatus[key] = s.count;
    });

    // Revenue growth
    const currentMonthRev = monthlyRevenue[0]?.total || 0;
    const lastMonthRev = lastMonthRevenue[0]?.total || 0;
    const revenueGrowth = lastMonthRev > 0 ? Math.round(((currentMonthRev - lastMonthRev) / lastMonthRev) * 100) : 0;

    // Build 7-day chart filling missing days
    const chartData = [];
    for (let i = 6; i >= 0; i--) {
      const d = new Date(today.getTime() - i * 86400000);
      const label = d.toISOString().split('T')[0];
      const dayNames = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'];
      const found = revenueChart.find(r => r._id === label);
      chartData.push({ date: label, label: dayNames[d.getDay()], totalRevenue: found?.totalRevenue || 0, count: found?.count || 0 });
    }

    res.json({
      success: true,
      data: {
        revenueToday: todayRevenue[0]?.total || 0,
        revenueMonth: currentMonthRev,
        revenueGrowth,
        ordersToday,
        pendingOrders,
        appointmentsToday: todayAppointmentsCount,
        pendingAppointments,
        appointmentsMonth,
        totalCustomers,
        newCustomersMonth,
        totalProducts,
        lowStock: lowStockProducts,
        totalServices,
        activeStaff,
        totalStaff,
        totalPets,
        totalUsers,
        totalOrders,
        totalAppointments,
        orderStatusChart: orderStatus,
        revenueChart: chartData,
        recentOrders,
        todayAppointments
      }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.getRevenueChart = async (req, res) => {
  try {
    const { period = 'monthly', year = new Date().getFullYear() } = req.query;
    
    let groupBy;
    if (period === 'daily') {
      groupBy = { $dateToString: { format: '%Y-%m-%d', date: '$createdAt' } };
    } else if (period === 'monthly') {
      groupBy = { $dateToString: { format: '%Y-%m', date: '$createdAt' } };
    } else {
      groupBy = { $dateToString: { format: '%Y', date: '$createdAt' } };
    }

    const data = await Order.aggregate([
      { $match: { status: 'Completed', isDeleted: false, createdAt: { $gte: new Date(`${year}-01-01`), $lt: new Date(`${parseInt(year) + 1}-01-01`) } } },
      { $group: { _id: groupBy, revenue: { $sum: '$totalAmount' }, count: { $sum: 1 } } },
      { $sort: { _id: 1 } }
    ]);

    res.json({ success: true, data });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

// Reports endpoint - aggregated data by period
exports.getReports = async (req, res) => {
  try {
    const { period = 'month' } = req.query;
    const now = new Date();
    let startDate;

    if (period === 'week') {
      startDate = new Date(now.getTime() - 7 * 86400000);
    } else if (period === 'month') {
      startDate = new Date(now.getFullYear(), now.getMonth(), 1);
    } else if (period === 'quarter') {
      const qMonth = Math.floor(now.getMonth() / 3) * 3;
      startDate = new Date(now.getFullYear(), qMonth, 1);
    } else {
      startDate = new Date(now.getFullYear(), 0, 1);
    }

    // Revenue chart - group by day for week/month, by month for quarter/year
    let dateFormat = '%Y-%m-%d';
    if (period === 'quarter' || period === 'year') dateFormat = '%Y-%m';

    const [revenueAgg, orderStatusAgg, topProducts, topServices, totalOrdersCount, totalApptsCount, newCustomersCount, inventoryData, lowStockProducts] = await Promise.all([
      Order.aggregate([
        { $match: { status: 'Completed', isDeleted: { $ne: true }, createdAt: { $gte: startDate } } },
        { $group: { _id: { $dateToString: { format: dateFormat, date: '$createdAt' } }, revenue: { $sum: '$totalAmount' }, count: { $sum: 1 } } },
        { $sort: { _id: 1 } }
      ]),
      Order.aggregate([
        { $match: { isDeleted: { $ne: true }, createdAt: { $gte: startDate } } },
        { $group: { _id: '$status', count: { $sum: 1 } } }
      ]),
      Order.aggregate([
        { $match: { status: 'Completed', isDeleted: { $ne: true }, createdAt: { $gte: startDate } } },
        { $unwind: '$items' },
        { $group: { _id: '$items.productName', sold: { $sum: '$items.quantity' }, revenue: { $sum: { $multiply: ['$items.unitPrice', '$items.quantity'] } } } },
        { $sort: { sold: -1 } },
        { $limit: 10 },
        { $project: { _id: 0, name: '$_id', sold: 1, revenue: 1 } }
      ]),
      Appointment.aggregate([
        { $match: { status: 'Completed', isDeleted: { $ne: true }, appointmentDate: { $gte: startDate } } },
        { $unwind: '$services' },
        { $group: { _id: '$services.serviceName', count: { $sum: 1 }, revenue: { $sum: '$services.price' } } },
        { $sort: { count: -1 } },
        { $limit: 10 },
        { $project: { _id: 0, name: '$_id', count: 1, revenue: 1 } }
      ]),
      Order.countDocuments({ isDeleted: { $ne: true }, createdAt: { $gte: startDate } }),
      Appointment.countDocuments({ isDeleted: { $ne: true }, appointmentDate: { $gte: startDate } }),
      User.countDocuments({ role: 'Customer', createdAt: { $gte: startDate } }),
      // Inventory summary
      Product.aggregate([
        { $match: { isDeleted: { $ne: true } } },
        { $group: { _id: null, totalProducts: { $sum: 1 }, totalStock: { $sum: '$stockQuantity' }, totalValue: { $sum: { $multiply: ['$price', '$stockQuantity'] } }, outOfStock: { $sum: { $cond: [{ $lte: ['$stockQuantity', 0] }, 1, 0] } }, lowStock: { $sum: { $cond: [{ $and: [{ $gt: ['$stockQuantity', 0] }, { $lte: ['$stockQuantity', 5] }] }, 1, 0] } } } }
      ]),
      // Low stock products list
      Product.find({ isDeleted: { $ne: true }, stockQuantity: { $lte: 10, $gte: 0 } }).select('name sku stockQuantity price soldCount imageUrl').sort('stockQuantity').limit(15)
    ]);

    const totalRevenue = revenueAgg.reduce((sum, r) => sum + r.revenue, 0);
    const orderStats = {};
    orderStatusAgg.forEach(s => { if (s._id) orderStats[s._id] = s.count; });

    res.json({
      success: true,
      data: {
        totalRevenue,
        totalOrders: totalOrdersCount,
        totalAppointments: totalApptsCount,
        newCustomers: newCustomersCount,
        revenueChart: {
          labels: revenueAgg.map(r => r._id),
          data: revenueAgg.map(r => r.revenue)
        },
        orderStats,
        topProducts,
        topServices,
        inventory: inventoryData[0] || { totalProducts: 0, totalStock: 0, totalValue: 0, outOfStock: 0, lowStock: 0 },
        lowStockProducts
      }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};
