const mongoose = require('mongoose');

const notificationSchema = new mongoose.Schema({
  recipient: { type: mongoose.Schema.Types.ObjectId, ref: 'User', required: true },
  title: { type: String, required: true },
  message: { type: String, required: true },
  type: {
    type: String,
    enum: ['Order', 'Appointment', 'Promotion', 'System', 'Chat', 'Review', 'Pet'],
    default: 'System'
  },
  referenceId: { type: String, default: '' },
  referenceType: { type: String, default: '' },
  imageUrl: { type: String, default: '' },
  isRead: { type: Boolean, default: false },
  readAt: { type: Date },
  data: { type: mongoose.Schema.Types.Mixed }
}, { timestamps: true });

notificationSchema.index({ recipient: 1, createdAt: -1 });
notificationSchema.index({ recipient: 1, isRead: 1 });

module.exports = mongoose.model('Notification', notificationSchema);
