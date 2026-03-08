const express = require('express');
const router = express.Router();
const chatController = require('../controllers/chat.controller');

// POST /api/chat - Gửi tin nhắn cho AI
router.post('/', chatController.sendMessage);

// DELETE /api/chat/:sessionId - Xóa lịch sử chat
router.delete('/:sessionId', chatController.clearChat);

module.exports = router;
