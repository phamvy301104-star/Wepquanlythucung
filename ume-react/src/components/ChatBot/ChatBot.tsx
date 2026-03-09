import { useState, useRef, useEffect } from 'react';
import { chatApi } from '../../services/api';
import './ChatBot.scss';

interface Message {
  id: string;
  text: string;
  sender: 'user' | 'bot';
  timestamp: Date;
}

const QUICK_QUESTIONS = [
  '🐶 Tư vấn dịch vụ tắm spa cho chó',
  '🐱 Mèo bị nôn phải làm sao?',
  '📦 Gợi ý thức ăn cho thú cưng',
  '📅 Tôi muốn đặt lịch hẹn',
  '💊 Chó bị rụng lông nhiều',
  '🛒 Có sản phẩm gì mới không?',
];

export default function ChatBot() {
  const [isOpen, setIsOpen] = useState(false);
  const [messages, setMessages] = useState<Message[]>([
    {
      id: 'welcome',
      text: 'Xin chào! 🐾 Mình là UME AI - trợ lý tư vấn thú cưng.\n\nMình có thể giúp bạn:\n• Tư vấn sản phẩm & dịch vụ\n• Đặt lịch hẹn\n• Giải đáp vấn đề sức khỏe thú cưng\n\nBạn cần hỗ trợ gì nào? 😊',
      sender: 'bot',
      timestamp: new Date(),
    },
  ]);
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(false);
  const [sessionId, setSessionId] = useState<string | undefined>();
  const [showQuick, setShowQuick] = useState(true);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  useEffect(() => {
    if (isOpen) inputRef.current?.focus();
  }, [isOpen]);

  const sendMessage = async (text: string) => {
    if (!text.trim() || loading) return;

    const userMsg: Message = {
      id: `u_${Date.now()}`,
      text: text.trim(),
      sender: 'user',
      timestamp: new Date(),
    };

    setMessages((prev) => [...prev, userMsg]);
    setInput('');
    setShowQuick(false);
    setLoading(true);

    try {
      const res = await chatApi.sendMessage(text.trim(), sessionId);
      const data = res.data;

      if (data.success) {
        if (data.data.sessionId) setSessionId(data.data.sessionId);

        const botMsg: Message = {
          id: `b_${Date.now()}`,
          text: data.data.reply,
          sender: 'bot',
          timestamp: new Date(),
        };
        setMessages((prev) => [...prev, botMsg]);
      } else {
        setMessages((prev) => [
          ...prev,
          {
            id: `e_${Date.now()}`,
            text: 'Xin lỗi, có lỗi xảy ra. Vui lòng thử lại! 🐾',
            sender: 'bot',
            timestamp: new Date(),
          },
        ]);
      }
    } catch {
      setMessages((prev) => [
        ...prev,
        {
          id: `e_${Date.now()}`,
          text: 'Không thể kết nối đến server. Vui lòng thử lại sau! 🐾',
          sender: 'bot',
          timestamp: new Date(),
        },
      ]);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    sendMessage(input);
  };

  const handleClearChat = async () => {
    if (sessionId) {
      try {
        await chatApi.clearHistory(sessionId);
      } catch { /* ignore */ }
    }
    setSessionId(undefined);
    setShowQuick(true);
    setMessages([
      {
        id: 'welcome',
        text: 'Xin chào! 🐾 Mình là UME AI - trợ lý tư vấn thú cưng.\n\nMình có thể giúp bạn:\n• Tư vấn sản phẩm & dịch vụ\n• Đặt lịch hẹn\n• Giải đáp vấn đề sức khỏe thú cưng\n\nBạn cần hỗ trợ gì nào? 😊',
        sender: 'bot',
        timestamp: new Date(),
      },
    ]);
  };

  const formatMessage = (text: string) => {
    return text.split('\n').map((line, i) => (
      <span key={i}>
        {line}
        {i < text.split('\n').length - 1 && <br />}
      </span>
    ));
  };

  return (
    <>
      {/* Floating button */}
      <button
        className={`chatbot-toggle ${isOpen ? 'active' : ''}`}
        onClick={() => setIsOpen(!isOpen)}
        aria-label="Mở chatbot tư vấn"
      >
        {isOpen ? '✕' : '🐾'}
        {!isOpen && <span className="chatbot-badge">AI</span>}
      </button>

      {/* Chat window */}
      {isOpen && (
        <div className="chatbot-window">
          {/* Header */}
          <div className="chatbot-header">
            <div className="chatbot-header-info">
              <div className="chatbot-avatar">🐾</div>
              <div>
                <h3>UME AI Assistant</h3>
                <span className="chatbot-status">● Đang hoạt động</span>
              </div>
            </div>
            <div className="chatbot-header-actions">
              <button onClick={handleClearChat} title="Xóa lịch sử chat">🗑️</button>
              <button onClick={() => setIsOpen(false)} title="Đóng">✕</button>
            </div>
          </div>

          {/* Messages */}
          <div className="chatbot-messages">
            {messages.map((msg) => (
              <div key={msg.id} className={`chatbot-msg ${msg.sender}`}>
                {msg.sender === 'bot' && <div className="chatbot-msg-avatar">🐾</div>}
                <div className="chatbot-msg-bubble">
                  <div className="chatbot-msg-text">{formatMessage(msg.text)}</div>
                  <div className="chatbot-msg-time">
                    {msg.timestamp.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
                  </div>
                </div>
              </div>
            ))}

            {loading && (
              <div className="chatbot-msg bot">
                <div className="chatbot-msg-avatar">🐾</div>
                <div className="chatbot-msg-bubble">
                  <div className="chatbot-typing">
                    <span></span><span></span><span></span>
                  </div>
                </div>
              </div>
            )}

            <div ref={messagesEndRef} />
          </div>

          {/* Quick questions */}
          {showQuick && (
            <div className="chatbot-quick">
              {QUICK_QUESTIONS.map((q, i) => (
                <button key={i} onClick={() => sendMessage(q)}>
                  {q}
                </button>
              ))}
            </div>
          )}

          {/* Input */}
          <form className="chatbot-input" onSubmit={handleSubmit}>
            <input
              ref={inputRef}
              type="text"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              placeholder="Nhập câu hỏi về thú cưng..."
              disabled={loading}
              maxLength={500}
            />
            <button type="submit" disabled={loading || !input.trim()}>
              ➤
            </button>
          </form>
        </div>
      )}
    </>
  );
}
