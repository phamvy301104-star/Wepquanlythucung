const mongoose = require('mongoose');

const orderItemSchema = new mongoose.Schema({
  product: { type: mongoose.Schema.Types.ObjectId, ref: 'Product' },
  pet: { type: mongoose.Schema.Types.ObjectId, ref: 'Pet' },
  productName: { type: String, required: true },
  productImage: { type: String, default: '' },
  sku: { type: String, default: '' },
  quantity: { type: Number, required: true, min: 1 },
  unitPrice: { type: Number, required: true },
  discount: { type: Number, default: 0 },
  totalPrice: { type: Number, required: true },
  notes: { type: String, default: '' }
});

const statusHistorySchema = new mongoose.Schema({
  status: { type: String, required: true },
  note: { type: String, default: '' },
  changedBy: { type: mongoose.Schema.Types.ObjectId, ref: 'User' },
  changedAt: { type: Date, default: Date.now }
});

const orderSchema = new mongoose.Schema({
  orderCode: { type: String, unique: true },
  customer: { type: mongoose.Schema.Types.ObjectId, ref: 'User', required: true },
  items: [orderItemSchema],
  subtotal: { type: Number, default: 0 },
  shippingFee: { type: Number, default: 0 },
  discountAmount: { type: Number, default: 0 },
  taxAmount: { type: Number, default: 0 },
  totalAmount: { type: Number, default: 0 },
  status: {
    type: String,
    enum: ['Pending', 'Confirmed', 'Processing', 'Shipping', 'Delivered', 'Completed', 'Cancelled', 'Returned'],
    default: 'Pending'
  },
  paymentMethod: { type: String, default: 'COD' },
  paymentStatus: {
    type: String,
    enum: ['Unpaid', 'Paid', 'Refunded'],
    default: 'Unpaid'
  },
  shippingAddress: {
    fullName: { type: String },
    phone: { type: String },
    address: { type: String },
    ward: { type: String },
    district: { type: String },
    city: { type: String }
  },
  notes: { type: String, default: '' },
  cancelReason: { type: String, default: '' },
  statusHistory: [statusHistorySchema],
  confirmedAt: { type: Date },
  shippedAt: { type: Date },
  deliveredAt: { type: Date },
  completedAt: { type: Date },
  cancelledAt: { type: Date },
  promotionCode: { type: String, default: '' },
  isDeleted: { type: Boolean, default: false }
}, { timestamps: true });

// Auto-generate order code
orderSchema.pre('save', async function(next) {
  if (!this.orderCode) {
    const count = await mongoose.model('Order').countDocuments();
    this.orderCode = `ORD${String(count + 1).padStart(6, '0')}`;
  }
  next();
});

module.exports = mongoose.model('Order', orderSchema);
