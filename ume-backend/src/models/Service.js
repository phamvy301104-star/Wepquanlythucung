const mongoose = require('mongoose');

const serviceSchema = new mongoose.Schema({
  serviceCode: { type: String, unique: true, sparse: true },
  name: { type: String, required: true, trim: true },
  slug: { type: String, unique: true, sparse: true },
  shortDescription: { type: String, default: '' },
  description: { type: String, default: '' },
  imageUrl: { type: String, default: '' },
  galleryImages: [{ type: String }],
  videoUrl: { type: String, default: '' },
  price: { type: Number, required: true, default: 0 },
  originalPrice: { type: Number },
  minPrice: { type: Number },
  maxPrice: { type: Number },
  durationMinutes: { type: Number, default: 30 },
  bufferMinutes: { type: Number, default: 10 },
  requiredStaff: { type: Number, default: 1 },
  gender: { type: String, enum: ['Male', 'Female', 'All'], default: 'All' },
  category: { type: mongoose.Schema.Types.ObjectId, ref: 'ServiceCategory' },
  isFeatured: { type: Boolean, default: false },
  isPopular: { type: Boolean, default: false },
  isNewArrival: { type: Boolean, default: false },
  isActive: { type: Boolean, default: true },
  displayOrder: { type: Number, default: 0 },
  averageRating: { type: Number, default: 0 },
  totalReviews: { type: Number, default: 0 },
  totalBookings: { type: Number, default: 0 },
  isDeleted: { type: Boolean, default: false }
}, { timestamps: true });

serviceSchema.pre('save', async function(next) {
  if (this.isModified('name') && !this.slug) {
    this.slug = this.name.toLowerCase()
      .normalize('NFD').replace(/[\u0300-\u036f]/g, '')
      .replace(/đ/g, 'd').replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');
  }
  if (!this.serviceCode) {
    const count = await mongoose.model('Service').countDocuments();
    this.serviceCode = 'DV' + String(count + 1).padStart(4, '0');
  }
  next();
});

module.exports = mongoose.model('Service', serviceSchema);
