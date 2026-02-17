const mongoose = require('mongoose');

const settingsSchema = new mongoose.Schema({
  // Contact Info
  address: { type: String, default: '123 Đường ABC, Quận 1, TP. Hồ Chí Minh' },
  phone: { type: String, default: '0384 731 104' },
  email: { type: String, default: 'bezubts@gmail.com' },
  workingHours: { type: String, default: '8:00 - 20:00 (Thứ 2 - Chủ nhật)' },

  // Social Media
  facebook: { type: String, default: '' },
  instagram: { type: String, default: '' },
  tiktok: { type: String, default: '' },
  youtube: { type: String, default: '' },
  zalo: { type: String, default: '' },

  // Map
  mapEmbedUrl: { type: String, default: 'https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3919.5!2d106.7!3d10.78!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x0%3A0x0!2zMTDCsDQ2JzQ4LjAiTiAxMDbCsDQyJzAwLjAiRQ!5e0!3m2!1svi!2s!4v1' },

  // Store Info
  storeName: { type: String, default: 'UME Pet Salon' },
  storeDescription: { type: String, default: '' },

  // Shipping Settings
  shippingStandardPrice: { type: Number, default: 30000 },
  shippingExpressPrice: { type: Number, default: 50000 },
  freeShipStandardThreshold: { type: Number, default: 500000 },
  freeShipExpressThreshold: { type: Number, default: 1000000 },

  // Shipping Policy
  shippingPolicy: { type: String, default: '' },

  // Return Policy
  returnPolicy: { type: String, default: 'Hỗ trợ đổi trả trong 7 ngày nếu sản phẩm lỗi từ nhà sản xuất.' },

  // COD Payment
  codEnabled: { type: Boolean, default: true },
  codDescription: { type: String, default: 'Thanh toán bằng tiền mặt khi nhận hàng' },

  // Bank Transfer Payment
  bankTransferEnabled: { type: Boolean, default: true },
  bankName: { type: String, default: '' },
  bankAccountNumber: { type: String, default: '' },
  bankAccountHolder: { type: String, default: '' },
  bankBranch: { type: String, default: '' },
  bankDescription: { type: String, default: 'Chuyển khoản trước khi giao hàng' },
  bankQrImage: { type: String, default: '' },
}, {
  timestamps: true
});

// Ensure only one settings document exists
settingsSchema.statics.getSettings = async function () {
  let settings = await this.findOne();
  if (!settings) {
    settings = await this.create({});
  }
  return settings;
};

module.exports = mongoose.model('Settings', settingsSchema);
