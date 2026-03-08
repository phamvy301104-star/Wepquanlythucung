const mongoose = require('mongoose');
const bcrypt = require('bcryptjs');

const userSchema = new mongoose.Schema({
  email: { type: String, required: true, unique: true, lowercase: true, trim: true },
  password: { type: String },
  fullName: { type: String, default: '' },
  initials: { type: String, default: '' },
  phoneNumber: { type: String, default: '' },
  avatarUrl: { type: String, default: '' },
  address: { type: String, default: '' },
  dateOfBirth: { type: Date },
  gender: { type: String, enum: ['Nam', 'Nữ', 'Khác', ''], default: '' },
  idNumber: { type: String, default: '' },
  startDate: { type: Date },
  position: { type: String, default: '' },
  notes: { type: String, default: '' },
  role: { type: String, enum: ['Admin', 'Staff', 'Customer'], default: 'Customer' },
  isActive: { type: Boolean, default: true },
  emailConfirmed: { type: Boolean, default: false },
  // OAuth
  googleId: { type: String },
  facebookId: { type: String },
  // Refresh token
  refreshTokens: [{
    token: String,
    expiresAt: Date,
    createdAt: { type: Date, default: Date.now }
  }]
}, {
  timestamps: true,
  toJSON: { virtuals: true },
  toObject: { virtuals: true }
});

// Hash password
userSchema.pre('save', async function(next) {
  if (!this.isModified('password') || !this.password) return next();
  this.password = await bcrypt.hash(this.password, 12);
  next();
});

// Compare password
userSchema.methods.comparePassword = async function(candidatePassword) {
  if (!this.password) return false;
  return bcrypt.compare(candidatePassword, this.password);
};

// Remove sensitive fields
userSchema.methods.toSafeObject = function() {
  const obj = this.toObject();
  delete obj.password;
  delete obj.refreshTokens;
  return obj;
};

module.exports = mongoose.model('User', userSchema);
