const Settings = require('../models/Settings');

// GET /api/settings - Public: lấy thông tin liên hệ
exports.getSettings = async (req, res) => {
  try {
    const settings = await Settings.getSettings();
    res.json({ success: true, data: settings });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

// PUT /api/settings - Admin: cập nhật thông tin liên hệ
exports.updateSettings = async (req, res) => {
  try {
    const fields = [
      'address', 'phone', 'email', 'workingHours',
      'facebook', 'instagram', 'tiktok', 'youtube', 'zalo',
      'mapEmbedUrl', 'storeName', 'storeDescription',
      'shippingStandardPrice', 'shippingExpressPrice',
      'freeShipStandardThreshold', 'freeShipExpressThreshold',
      'shippingPolicy', 'returnPolicy',
      'codEnabled', 'codDescription',
      'bankTransferEnabled', 'bankName', 'bankAccountNumber',
      'bankAccountHolder', 'bankBranch', 'bankDescription', 'bankQrImage'
    ];

    let settings = await Settings.getSettings();

    for (const field of fields) {
      if (req.body[field] !== undefined) {
        settings[field] = req.body[field];
      }
    }

    await settings.save();

    res.json({ success: true, message: 'Cập nhật cài đặt thành công', data: settings });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};
