const axios = require('axios');
const Product = require('../models/Product');
const Service = require('../models/Service');
const ServiceCategory = require('../models/ServiceCategory');

// Groq API config (free, fast - Llama 3.3 70B)
const GROQ_API_URL = 'https://api.groq.com/openai/v1/chat/completions';

// Lưu lịch sử chat trong memory (mỗi session)
const chatSessions = new Map();

// Cache dữ liệu sản phẩm/dịch vụ (refresh mỗi 10 phút)
let cachedShopData = null;
let cacheTimestamp = 0;
const CACHE_TTL = 10 * 60 * 1000;

async function getShopData() {
  const now = Date.now();
  if (cachedShopData && (now - cacheTimestamp) < CACHE_TTL) {
    return cachedShopData;
  }

  try {
    const [products, services, serviceCategories] = await Promise.all([
      Product.find({ isActive: true }).select('name price originalPrice category description').populate('category', 'name').lean(),
      Service.find({ isActive: true }).select('name price durationMinutes category description').populate('category', 'name').lean(),
      ServiceCategory.find({ isActive: true }).select('name description').lean(),
    ]);

    const productList = products.map(p => {
      const catName = p.category?.name || 'Khác';
      const priceStr = p.price ? `${p.price.toLocaleString('vi-VN')}đ` : '';
      return `- ${p.name} (${catName}) – ${priceStr}`;
    }).join('\n');

    const serviceList = services.map(s => {
      const catName = s.category?.name || 'Khác';
      const priceStr = s.price ? `${s.price.toLocaleString('vi-VN')}đ` : '';
      const dur = s.durationMinutes ? ` (~${s.durationMinutes} phút)` : '';
      return `- ${s.name} (${catName}) – ${priceStr}${dur}`;
    }).join('\n');

    const categoryList = serviceCategories.map(c => `- ${c.name}: ${c.description || ''}`).join('\n');

    cachedShopData = { productList, serviceList, categoryList };
    cacheTimestamp = now;
    return cachedShopData;
  } catch (err) {
    console.error('Error loading shop data for AI:', err.message);
    return { productList: '', serviceList: '', categoryList: '' };
  }
}

function buildSystemPrompt(shopData) {
  return `Bạn là "UME AI" - trợ lý tư vấn thú cưng thông minh của UME Pet Salon.

🎯 VAI TRÒ:
- Tư vấn chăm sóc thú cưng (chó, mèo, hamster, thỏ, chim, cá cảnh...)
- Tư vấn dinh dưỡng, sức khỏe, hành vi thú cưng
- Chẩn đoán sơ bộ các bệnh phổ biến ở thú cưng và đưa ra lời khuyên
- Giới thiệu dịch vụ cụ thể của UME Pet Salon phù hợp với nhu cầu
- Gợi ý sản phẩm cụ thể có trong cửa hàng
- Hướng dẫn đặt lịch hẹn dịch vụ

🏥 TƯ VẤN BỆNH THÚ CƯNG:
- Khi người dùng hỏi về triệu chứng bệnh, hãy mô tả bệnh phổ biến liên quan, nguyên nhân, cách phòng ngừa
- Luôn khuyên đưa đến bác sĩ thú y nếu triệu chứng nghiêm trọng
- Gợi ý dịch vụ khám sức khỏe hoặc sản phẩm hỗ trợ nếu có

📅 HƯỚNG DẪN ĐẶT LỊCH:
- Khi người dùng muốn đặt lịch, hãy hướng dẫn: "Bạn có thể đặt lịch trực tiếp tại trang Đặt lịch trên website (umepetsalon.pro.vn/booking) hoặc cho mình biết dịch vụ bạn muốn, mình sẽ tư vấn chi tiết!"
- Gợi ý dịch vụ phù hợp kèm giá và thời gian thực hiện

📋 QUY TẮC:
1. CHỈ trả lời các câu hỏi liên quan đến thú cưng, động vật, và dịch vụ/sản phẩm của cửa hàng
2. Nếu câu hỏi KHÔNG liên quan đến thú cưng/động vật, lịch sự từ chối: "Xin lỗi, mình chỉ có thể tư vấn về thú cưng và các dịch vụ của UME Pet Salon thôi ạ! 🐾"
3. Trả lời bằng tiếng Việt, thân thiện, dễ hiểu
4. Sử dụng emoji phù hợp để sinh động hơn
5. Câu trả lời ngắn gọn, tối đa 300 từ
6. Khi gợi ý sản phẩm/dịch vụ, ưu tiên sản phẩm/dịch vụ THỰC TẾ có trong danh sách bên dưới
7. Khi tư vấn sức khỏe nghiêm trọng, luôn khuyên đưa đến bác sĩ thú y

🏪 THÔNG TIN CỬA HÀNG:
- Tên: UME Pet Salon
- Website: umepetsalon.pro.vn
- Trang đặt lịch: umepetsalon.pro.vn/booking
- Trang sản phẩm: umepetsalon.pro.vn/products
- Trang dịch vụ: umepetsalon.pro.vn/services

📦 DANH SÁCH SẢN PHẨM HIỆN CÓ:
${shopData.productList || '(Đang cập nhật)'}

💆 DANH MỤC DỊCH VỤ:
${shopData.categoryList || '(Đang cập nhật)'}

🛎️ CÁC DỊCH VỤ CỤ THỂ:
${shopData.serviceList || '(Đang cập nhật)'}`;
}

// POST /api/chat - Gửi tin nhắn chat (Groq API - Llama 3.3)
exports.sendMessage = async (req, res) => {
  try {
    const { message, sessionId } = req.body;

    if (!message || !message.trim()) {
      return res.status(400).json({
        success: false,
        message: 'Vui lòng nhập tin nhắn'
      });
    }

    const apiKey = process.env.GROQ_API_KEY;
    if (!apiKey) {
      return res.status(500).json({
        success: false,
        message: 'AI chưa được cấu hình. Vui lòng liên hệ admin.'
      });
    }

    // Tạo hoặc lấy chat session
    const sid = sessionId || `session_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;

    let chatHistory = chatSessions.get(sid) || [];

    // Giới hạn lịch sử chat (giữ 20 cặp tin nhắn gần nhất)
    if (chatHistory.length > 40) {
      chatHistory = chatHistory.slice(-40);
    }

    // Load dữ liệu sản phẩm/dịch vụ thực tế
    const shopData = await getShopData();
    const systemPrompt = buildSystemPrompt(shopData);

    // Build messages cho OpenAI-compatible API
    const messages = [
      { role: 'system', content: systemPrompt },
      ...chatHistory,
      { role: 'user', content: message.trim() }
    ];

    // Gọi Groq API
    const modelName = process.env.GROQ_MODEL || 'llama-3.3-70b-versatile';
    const response = await axios.post(GROQ_API_URL, {
      model: modelName,
      messages,
      max_tokens: 1000,
      temperature: 0.7,
    }, {
      headers: {
        'Authorization': `Bearer ${apiKey}`,
        'Content-Type': 'application/json',
      },
      timeout: 30000,
    });

    const aiReply = response.data.choices[0].message.content;

    // Cập nhật lịch sử
    chatHistory.push(
      { role: 'user', content: message.trim() },
      { role: 'assistant', content: aiReply }
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
    const errMsg = error.response?.data?.error?.message || error.message;
    console.error('Chat AI Error:', errMsg);

    if (error.response?.status === 401) {
      return res.status(500).json({
        success: false,
        message: 'API key không hợp lệ. Vui lòng liên hệ admin.'
      });
    }

    if (error.response?.status === 429) {
      return res.json({
        success: true,
        data: {
          reply: 'Hiện tại hệ thống đang quá tải, vui lòng thử lại sau vài phút nhé! ⏳🐾',
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
