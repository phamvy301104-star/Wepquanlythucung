const mongoose = require('mongoose');

const reviewSchema = new mongoose.Schema({
  customer: { type: mongoose.Schema.Types.ObjectId, ref: 'User', required: true },
  // Review can be for a product, service, or staff
  product: { type: mongoose.Schema.Types.ObjectId, ref: 'Product' },
  service: { type: mongoose.Schema.Types.ObjectId, ref: 'Service' },
  staff: { type: mongoose.Schema.Types.ObjectId, ref: 'Staff' },
  appointment: { type: mongoose.Schema.Types.ObjectId, ref: 'Appointment' },
  order: { type: mongoose.Schema.Types.ObjectId, ref: 'Order' },
  rating: { type: Number, required: true, min: 1, max: 5 },
  title: { type: String, default: '' },
  comment: { type: String, default: '' },
  images: [{ type: String }],
  reply: { type: String, default: '' },
  repliedAt: { type: Date },
  repliedBy: { type: mongoose.Schema.Types.ObjectId, ref: 'User' },
  isVerifiedPurchase: { type: Boolean, default: false },
  isApproved: { type: Boolean, default: true },
  helpfulCount: { type: Number, default: 0 },
  isDeleted: { type: Boolean, default: false }
}, { timestamps: true });

reviewSchema.index({ product: 1, createdAt: -1 });
reviewSchema.index({ service: 1, createdAt: -1 });
reviewSchema.index({ staff: 1, createdAt: -1 });
reviewSchema.index({ customer: 1 });

module.exports = mongoose.model('Review', reviewSchema);
