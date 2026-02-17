const mongoose = require('mongoose');

const appointmentServiceSchema = new mongoose.Schema({
  service: { type: mongoose.Schema.Types.ObjectId, ref: 'Service', required: true },
  serviceName: { type: String },
  price: { type: Number, default: 0 },
  duration: { type: Number, default: 0 },
  notes: { type: String, default: '' }
});

const appointmentSchema = new mongoose.Schema({
  appointmentCode: { type: String, unique: true },
  customer: { type: mongoose.Schema.Types.ObjectId, ref: 'User', required: true },
  staff: { type: mongoose.Schema.Types.ObjectId, ref: 'Staff' },
  pet: { type: mongoose.Schema.Types.ObjectId, ref: 'Pet' },
  petDescription: {
    name: { type: String, default: '' },
    type: { type: String, default: '' },
    gender: { type: String, enum: ['', 'Male', 'Female', 'Neutered'], default: '' },
    age: { type: Number, default: 0 },
    weight: { type: Number, default: 0 },
    notes: { type: String, default: '' }
  },
  appointmentDate: { type: Date, required: true },
  startTime: { type: String, required: true },
  endTime: { type: String },
  services: [appointmentServiceSchema],
  totalAmount: { type: Number, default: 0 },
  discountAmount: { type: Number, default: 0 },
  finalAmount: { type: Number, default: 0 },
  status: {
    type: String,
    enum: ['Pending', 'Confirmed', 'InProgress', 'Completed', 'Cancelled', 'NoShow'],
    default: 'Pending'
  },
  paymentStatus: {
    type: String,
    enum: ['Unpaid', 'PartiallyPaid', 'Paid', 'Refunded'],
    default: 'Unpaid'
  },
  paymentMethod: { type: String, default: '' },
  notes: { type: String, default: '' },
  cancelReason: { type: String, default: '' },
  cancelledAt: { type: Date },
  confirmedAt: { type: Date },
  completedAt: { type: Date },
  checkinAt: { type: Date },
  rating: { type: Number, min: 1, max: 5 },
  reviewComment: { type: String, default: '' },
  isDeleted: { type: Boolean, default: false }
}, { timestamps: true });

// Auto-generate appointment code
appointmentSchema.pre('save', async function(next) {
  if (!this.appointmentCode) {
    const year = new Date().getFullYear();
    const count = await mongoose.model('Appointment').countDocuments();
    this.appointmentCode = `APT-${year}0${String(count + 1).padStart(3, '0')}`;
  }
  next();
});

module.exports = mongoose.model('Appointment', appointmentSchema);
