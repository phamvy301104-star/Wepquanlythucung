const mongoose = require('mongoose');

const petSchema = new mongoose.Schema({
  owner: { type: mongoose.Schema.Types.ObjectId, ref: 'User', required: true },
  name: { type: String, required: true },
  type: {
    type: String,
    enum: ['Dog', 'Cat', 'Bird', 'Fish', 'Hamster', 'Rabbit', 'Other'],
    required: true
  },
  breed: { type: String, default: '' },
  age: { type: Number, default: 0 },
  ageUnit: { type: String, enum: ['months', 'years'], default: 'years' },
  weight: { type: Number, default: 0 },
  weightUnit: { type: String, enum: ['kg', 'lbs'], default: 'kg' },
  gender: { type: String, enum: ['Male', 'Female', 'Unknown'], default: 'Unknown' },
  color: { type: String, default: '' },
  imageUrl: { type: String, default: '' },
  additionalImages: [{ type: String }],
  description: { type: String, default: '' },
  healthNotes: { type: String, default: '' },
  allergies: { type: String, default: '' },
  vaccinated: { type: Boolean, default: false },
  neutered: { type: Boolean, default: false },
  microchipId: { type: String, default: '' },
  dateOfBirth: { type: Date },
  listingType: {
    type: String,
    enum: ['None', 'Adoption', 'Sale', 'Lost', 'Found'],
    default: 'None'
  },
  listingPrice: { type: Number, default: 0 },
  listingDescription: { type: String, default: '' },
  listingStatus: {
    type: String,
    enum: ['Active', 'Inactive', 'Sold', 'Adopted'],
    default: 'Active'
  },
  isDeleted: { type: Boolean, default: false }
}, { timestamps: true });

module.exports = mongoose.model('Pet', petSchema);
