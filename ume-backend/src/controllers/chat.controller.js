const { GoogleGenerativeAI } = require('@google/generative-ai');

// Khởi tạo Gemini
const genAI = new GoogleGenerativeAI(process.env.GEMINI_API_KEY);

// Lưu lịch sử chat trong memory (mỗi session)
const chatSessions = new Map();

// System prompt cho AI tư vấn thú cưng
const SYSTEM_PROMPT = `Bạn là "UME AI" - trợ lý tư vấn thú cưng thông minh của UME Pet Salon. 

🎯 VAI TRÒ:
- Tư vấn chăm sóc thú cưng (chó, mèo, hamster, thỏ, chim, cá cảnh...)
- Tư vấn dinh dưỡng, sức khỏe, hành vi thú cưng
- Giới thiệu dịch vụ của UME Pet Salon (tắm spa, cắt tỉa lông, khám sức khỏe, trông giữ...)
- Gợi ý sản phẩm phù hợp (thức ăn, đồ chơi, phụ kiện, vệ sinh...)

📋 QUY TẮC:
1. CHỈ trả lời các câu hỏi liên quan đến thú cưng, động vật, và dịch vụ/sản phẩm của cửa hàng
2. Nếu câu hỏi KHÔNG liên quan đến thú cưng/động vật, lịch sự từ chối: "Xin lỗi, mình chỉ có thể tư vấn về thú cưng và các dịch vụ của UME Pet Salon thôi ạ! 🐾"
3. Trả lời bằng tiếng Việt, thân thiện, dễ hiểu
4. Sử dụng emoji phù hợp để sinh động hơn
5. Câu trả lời ngắn gọn, tối đa 300 từ
6. Khi tư vấn sức khỏe nghiêm trọng, khuyên đưa đến bác sĩ thú y

🏪 THÔNG TIN CỬA HÀNG:
- Tên: UME Pet Salon
- Dịch vụ: Tắm spa, cắt tỉa lông, chăm sóc móng, vệ sinh tai, khám sức khỏe, trông giữ thú cưng
- Sản phẩm: Thức ăn (Royal Canin, Pedigree, Whiskas, Me-O), đồ chơi, phụ kiện, vệ sinh, quần áo, sức khỏe
- Website: umepetsalon.pro.vn`;

// POST /api/chat - Gửi tin nhắn chat
exports.sendMessage = async (req, res) => {
  try {
    const { message, sessionId } = req.body;

    if (!message || !message.trim()) {
      return res.status(400).json({
        success: false,
        message: 'Vui lòng nhập tin nhắn'
      });
    }

    if (!process.env.GEMINI_API_KEY) {
      return res.status(500).json({
        success: false,
        message: 'AI chưa được cấu hình. Vui lòng liên hệ admin.'
      });
    }

    // Tạo hoặc lấy chat session
    const sid = sessionId || `session_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    
    let chatHistory = chatSessions.get(sid) || [];

    // Giới hạn lịch sử chat (giữ 20 tin nhắn gần nhất)
    if (chatHistory.length > 40) {
      chatHistory = chatHistory.slice(-40);
    }

    // Tạo model - thử nhiều model để tránh lỗi quota
    const modelName = process.env.GEMINI_MODEL || 'gemini-2.0-flash';
    const model = genAI.getGenerativeModel({ 
      model: modelName,
      systemInstruction: SYSTEM_PROMPT
    });

    // Tạo chat với lịch sử
    const chat = model.startChat({
      history: chatHistory,
      generationConfig: {
        maxOutputTokens: 1000,
        temperature: 0.7,
      },
    });

    // Gửi tin nhắn
    const result = await chat.sendMessage(message);
    const response = result.response;
    const aiReply = response.text();

    // Cập nhật lịch sử
    chatHistory.push(
      { role: 'user', parts: [{ text: message }] },
      { role: 'model', parts: [{ text: aiReply }] }
    );
    chatSessions.set(sid, chatHistory);

    // Tự động dọn sessions cũ (> 1 giờ)
    setTimeout(() => {
      chatSessions.delete(sid);
    }, 60 * 60 * 1000);

    res.json({
      success: true,
      data: {
        reply: aiReply,
        sessionId: sid
      }
    });

  } catch (error) {
    console.error('Chat AI Error:', error.message);
    
    // Xử lý lỗi cụ thể
    if (error.message?.includes('API_KEY')) {
      return res.status(500).json({
        success: false,
        message: 'API key không hợp lệ. Vui lòng liên hệ admin.'
      });
    }

    if (error.message?.includes('429') || error.message?.includes('quota') || error.message?.includes('Too Many Requests')) {
      return res.json({
        success: true,
        data: {
          reply: 'Hiện tại hệ thống đang quá tải, vui lòng thử lại sau vài phút nhé! ⏳🐾',
          sessionId: req.body.sessionId
        }
      });
    }

    if (error.message?.includes('SAFETY')) {
      return res.json({
        success: true,
        data: {
          reply: 'Xin lỗi, mình không thể trả lời câu hỏi này. Bạn có thể hỏi về chăm sóc thú cưng nhé! 🐾',
          sessionId: req.body.sessionId
        }
      });
    }

    res.status(500).json({
      success: false,
      message: 'Có lỗi xảy ra, vui lòng thử lại sau.'
    });
  }
};

// DELETE /api/chat/:sessionId - Xóa session chat
exports.clearChat = (req, res) => {
  const { sessionId } = req.params;
  chatSessions.delete(sessionId);
  res.json({
    success: true,
    message: 'Đã xóa lịch sử chat'
  });
};
