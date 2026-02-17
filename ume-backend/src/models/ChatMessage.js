const mongoose = require('mongoose');

const chatMessageSchema = new mongoose.Schema({
  sender: { type: mongoose.Schema.Types.ObjectId, ref: 'User', required: true },
  receiver: { type: mongoose.Schema.Types.ObjectId, ref: 'User', required: true },
  conversationId: { type: String, required: true },
  message: { type: String, default: '' },
  messageType: {
    type: String,
    enum: ['Text', 'Image', 'File', 'System'],
    default: 'Text'
  },
  fileUrl: { type: String, default: '' },
  isRead: { type: Boolean, default: false },
  readAt: { type: Date },
  isDeleted: { type: Boolean, default: false }
}, { timestamps: true });

chatMessageSchema.index({ conversationId: 1, createdAt: 1 });
chatMessageSchema.index({ sender: 1, receiver: 1 });

module.exports = mongoose.model('ChatMessage', chatMessageSchema);
