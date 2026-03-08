import { useState, useEffect } from 'react';
import { settingsApi } from '../../services/api';
import toast from 'react-hot-toast';
import api from '../../services/api';
import './Contact.scss';

const subjects = [
  'Tư vấn dịch vụ', 'Hỏi về sản phẩm', 'Đặt lịch hẹn',
  'Khiếu nại / Góp ý', 'Hợp tác kinh doanh', 'Khác',
];

export default function Contact() {
  const [settings, setSettings] = useState<any>({});
  const [form, setForm] = useState({ fullName: '', phone: '', email: '', subject: '', message: '' });
  const [submitted, setSubmitted] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    settingsApi.get().then(res => setSettings(res.data?.data || {})).catch(() => {});
  }, []);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    setForm(f => ({ ...f, [e.target.name]: e.target.value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.fullName || !form.phone || !form.email || !form.subject || !form.message) {
      toast.error('Vui lòng điền đầy đủ thông tin'); return;
    }
    setLoading(true);
    try {
      await api.post('/contacts', form);
      setSubmitted(true);
      toast.success('Gửi liên hệ thành công!');
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Gửi thất bại');
    } finally { setLoading(false); }
  };

  const resetForm = () => {
    setForm({ fullName: '', phone: '', email: '', subject: '', message: '' });
    setSubmitted(false);
  };

  return (
    <div className="contact-page">
      <div className="page-header">
        <div className="container">
          <h1>📞 Liên hệ</h1>
          <p>Chúng tôi luôn sẵn sàng hỗ trợ bạn</p>
        </div>
      </div>

      <div className="container">
        <div className="contact-grid">
          {/* Left: Info */}
          <div className="contact-info">
            <h2>Thông tin liên hệ</h2>

            <div className="info-items">
              {settings.address && (
                <div className="info-item"><span className="info-icon">📍</span><div><h4>Địa chỉ</h4><p>{settings.address}</p></div></div>
              )}
              {settings.phone && (
                <div className="info-item"><span className="info-icon">📱</span><div><h4>Điện thoại</h4><p>{settings.phone}</p></div></div>
              )}
              {settings.email && (
                <div className="info-item"><span className="info-icon">✉️</span><div><h4>Email</h4><p>{settings.email}</p></div></div>
              )}
              {settings.workingHours && (
                <div className="info-item"><span className="info-icon">🕐</span><div><h4>Giờ làm việc</h4><p>{settings.workingHours}</p></div></div>
              )}
            </div>

            <div className="social-links">
              <h4>Kết nối với chúng tôi</h4>
              <div className="social-icons">
                {settings.facebook && <a href={settings.facebook} target="_blank" rel="noopener noreferrer" className="social-icon facebook">f</a>}
                {settings.instagram && <a href={settings.instagram} target="_blank" rel="noopener noreferrer" className="social-icon instagram">📸</a>}
                {settings.tiktok && <a href={settings.tiktok} target="_blank" rel="noopener noreferrer" className="social-icon tiktok">🎵</a>}
                {settings.youtube && <a href={settings.youtube} target="_blank" rel="noopener noreferrer" className="social-icon youtube">▶</a>}
                {settings.zalo && <a href={settings.zalo} target="_blank" rel="noopener noreferrer" className="social-icon zalo">Z</a>}
              </div>
            </div>

            {settings.mapEmbedUrl && (
              <div className="map-section">
                <iframe src={settings.mapEmbedUrl} title="Bản đồ" allowFullScreen loading="lazy" referrerPolicy="no-referrer-when-downgrade" />
              </div>
            )}
          </div>

          {/* Right: Form */}
          <div className="contact-form-section">
            {!submitted ? (
              <>
                <h2>Gửi tin nhắn</h2>
                <form onSubmit={handleSubmit}>
                  <div className="form-group">
                    <label>Họ và tên *</label>
                    <input name="fullName" value={form.fullName} onChange={handleChange} placeholder="VD: Nguyễn Văn A" />
                  </div>
                  <div className="form-row">
                    <div className="form-group">
                      <label>Số điện thoại *</label>
                      <input name="phone" value={form.phone} onChange={handleChange} placeholder="VD: 0901234567" />
                    </div>
                    <div className="form-group">
                      <label>Email *</label>
                      <input name="email" type="email" value={form.email} onChange={handleChange} placeholder="VD: email@example.com" />
                    </div>
                  </div>
                  <div className="form-group">
                    <label>Chủ đề *</label>
                    <select name="subject" value={form.subject} onChange={handleChange}>
                      <option value="">-- Chọn chủ đề --</option>
                      {subjects.map(s => <option key={s} value={s}>{s}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Nội dung *</label>
                    <textarea name="message" value={form.message} onChange={handleChange} rows={5} placeholder="Nhập nội dung tin nhắn..." />
                  </div>
                  <button type="submit" className="btn-send" disabled={loading}>{loading ? '⏳ Đang gửi...' : '📩 Gửi tin nhắn'}</button>
                </form>
              </>
            ) : (
              <div className="success-panel">
                <div className="success-icon">✅</div>
                <h2>Cảm ơn bạn!</h2>
                <p>Tin nhắn của bạn đã được gửi thành công. Chúng tôi sẽ phản hồi trong thời gian sớm nhất.</p>
                <button className="btn-new" onClick={resetForm}>📝 Gửi tin nhắn khác</button>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
