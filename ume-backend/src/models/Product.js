const mongoose = require('mongoose');

const productSchema = new mongoose.Schema({
  sku: { type: String, unique: true, sparse: true, trim: true },
  barcode: { type: String, default: '' },
  name: { type: String, required: true, trim: true },
  slug: { type: String, unique: true, sparse: true },
  shortDescription: { type: String, default: '' },
  description: { type: String, default: '' },
  imageUrl: { type: String, default: '' },
  additionalImages: [{ type: String }],
  videoUrl: { type: String, default: '' },
  originalPrice: { type: Number, default: 0 },
  price: { type: Number, required: true, default: 0 },
  discountPercent: { type: Number, default: 0 },
  costPrice: { type: Number, default: 0 },
  stockQuantity: { type: Number, default: 0 },
  lowStockThreshold: { type: Number, default: 10 },
  category: { type: mongoose.Schema.Types.ObjectId, ref: 'Category' },
  brand: { type: mongoose.Schema.Types.ObjectId, ref: 'Brand' },
  weight: { type: Number },
  volume: { type: Number },
  unit: { type: String, default: '' },
  ingredients: { type: String, default: '' },
  usage: { type: String, default: '' },
  warnings: { type: String, default: '' },
  manufactureDate: { type: Date },
  expiryDate: { type: Date },
  origin: { type: String, default: '' },
  averageRating: { type: Number, default: 0 },
  totalReviews: { type: Number, default: 0 },
  viewCount: { type: Number, default: 0 },
  soldCount: { type: Number, default: 0 },
  wishlistCount: { type: Number, default: 0 },
  isFeatured: { type: Boolean, default: false },
  isBestSeller: { type: Boolean, default: false },
  isNewArrival: { type: Boolean, default: false },
  isOnSale: { type: Boolean, default: false },
  isActive: { type: Boolean, default: true },
  allowBackorder: { type: Boolean, default: false },
  isDeleted: { type: Boolean, default: false },
  tags: [{ type: String }],
  metaTitle: { type: String, default: '' },
  metaDescription: { type: String, default: '' },
  metaKeywords: { type: String, default: '' }
}, {
  timestamps: true,
  toJSON: { virtuals: true },
  toObject: { virtuals: true }
});

// Auto-generate slug and SKU
productSchema.pre('save', async function(next) {
  if (this.isModified('name') && !this.slug) {
    this.slug = this.name.toLowerCase()
      .normalize('NFD').replace(/[\u0300-\u036f]/g, '')
      .replace(/đ/g, 'd').replace(/Đ/g, 'D')
      .replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');
  }
  if (!this.sku) {
    const count = await mongoose.model('Product').countDocuments();
    this.sku = 'SP' + String(count + 1).padStart(5, '0');
  }
  next();
});

// Default query: exclude deleted
productSchema.pre(/^find/, function(next) {
  if (this.getQuery().isDeleted === undefined) {
    this.where({ isDeleted: false });
  }
  next();
});

module.exports = mongoose.model('Product', productSchema);
