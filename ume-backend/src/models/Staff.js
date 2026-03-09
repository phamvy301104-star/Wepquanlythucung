const mongoose = require('mongoose');

const staffSchema = new mongoose.Schema({
  user: { type: mongoose.Schema.Types.ObjectId, ref: 'User' },
  staffCode: { type: String, required: true, unique: true },
  fullName: { type: String, required: true },
  nickName: { type: String, default: '' },
  email: { type: String, default: '' },
  phoneNumber: { type: String, default: '' },
  avatarUrl: { type: String, default: '' },
  coverImageUrl: { type: String, default: '' },
  bio: { type: String, default: '' },
  position: { type: String, enum: ['Barber', 'Stylist', 'Manager', 'Trainee', 'PetGroomer', 'Veterinarian'], default: 'PetGroomer' },
  level: { type: String, enum: ['Junior', 'Senior', 'Master', 'Expert'], default: 'Junior' },
  specialties: { type: String, default: '' },
  yearsOfExperience: { type: Number, default: 0 },
  dateOfBirth: { type: Date },
  gender: { type: String, default: '' },
  hireDate: { type: Date, default: Date.now },
  baseSalary: { type: Number, default: 0 },
  commissionPercent: { type: Number, default: 0 },
  averageRating: { type: Number, default: 0 },
  totalReviews: { type: Number, default: 0 },
  totalCustomersServed: { type: Number, default: 0 },
  totalRevenue: { type: Number, default: 0 },
  facebookUrl: { type: String, default: '' },
  instagramUrl: { type: String, default: '' },
  tiktokUrl: { type: String, default: '' },
  status: { type: String, enum: ['Active', 'OnLeave', 'Resigned'], default: 'Active' },
  services: [{ type: mongoose.Schema.Types.ObjectId, ref: 'Service' }],
  schedule: [{
    dayOfWeek: { type: Number, min: 0, max: 6 },
    startTime: { type: String },
    endTime: { type: String },
    breakStartTime: { type: String },
    breakEndTime: { type: String },
    isWorkingDay: { type: Boolean, default: true }
  }],
  isDeleted: { type: Boolean, default: false }
}, { timestamps: true });

module.exports = mongoose.model('Staff', staffSchema);
