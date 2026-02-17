const jwt = require('jsonwebtoken');

module.exports = (io) => {
  // Auth middleware for Socket.IO
  io.use((socket, next) => {
    const token = socket.handshake.auth.token || socket.handshake.query.token;
    if (token) {
      try {
        const decoded = jwt.verify(token, process.env.JWT_SECRET);
        socket.userId = decoded.userId;
        socket.userRole = decoded.role;
      } catch (err) {
        // Allow connection but without auth
      }
    }
    next();
  });

  io.on('connection', (socket) => {
    console.log(`🔌 Client connected: ${socket.id}`);

    // Join user's personal room
    if (socket.userId) {
      socket.join(socket.userId);
      console.log(`👤 User ${socket.userId} joined personal room`);
    }

    // Admin room
    if (socket.userRole === 'Admin') {
      socket.join('admin');
    }

    // Chat
    socket.on('joinChat', (conversationId) => {
      socket.join(`chat:${conversationId}`);
    });

    socket.on('leaveChat', (conversationId) => {
      socket.leave(`chat:${conversationId}`);
    });

    socket.on('sendMessage', (data) => {
      io.to(`chat:${data.conversationId}`).emit('newMessage', data);
    });

    socket.on('typing', (data) => {
      socket.to(`chat:${data.conversationId}`).emit('userTyping', {
        userId: socket.userId,
        conversationId: data.conversationId
      });
    });

    socket.on('stopTyping', (data) => {
      socket.to(`chat:${data.conversationId}`).emit('userStoppedTyping', {
        userId: socket.userId,
        conversationId: data.conversationId
      });
    });

    socket.on('disconnect', () => {
      console.log(`🔌 Client disconnected: ${socket.id}`);
    });
  });
};
