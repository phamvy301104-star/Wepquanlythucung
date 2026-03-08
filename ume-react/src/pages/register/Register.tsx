import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import toast from 'react-hot-toast';
import './Register.scss';

export default function Register() {
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [acceptTerms, setAcceptTerms] = useState(false);
  const [loading, setLoading] = useState(false);
  const { register } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!fullName || !email || !password || !confirmPassword) {
      toast.error('Vui lòng nhập đầy đủ thông tin');
      return;
    }
    if (password.length < 6) {
      toast.error('Mật khẩu phải có ít nhất 6 ký tự');
      return;
    }
    if (password !== confirmPassword) {
      toast.error('Mật khẩu xác nhận không khớp');
      return;
    }
    if (!acceptTerms) {
      toast.error('Vui lòng đồng ý với điều khoản sử dụng');
      return;
    }

    setLoading(true);
    try {
      await register({ fullName, email, password, phoneNumber: phoneNumber || undefined });
      toast.success('Đăng ký thành công! Chào mừng bạn đến với PetCare.');
      navigate('/');
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Đăng ký thất bại. Vui lòng thử lại.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="register-page">
      <div className="paw-decorations">
        {[1,2,3,4,5].map(i => <span key={i} className={`paw paw-${i}`}>🐾</span>)}
      </div>

      <div className="register-container">
        <div className="register-illustration">
          <div className="illustration-content">
            <div className="illustration-emoji">🐾</div>
            <h2>Tham gia PetCare</h2>
            <p>Tạo tài khoản miễn phí để trải nghiệm dịch vụ chăm sóc thú cưng tốt nhất!</p>
            <div className="illustration-benefits">
              <div className="benefit"><span className="benefit-icon">🎁</span><div><h4>Ưu đãi thành viên</h4><p>Giảm 10% cho đơn hàng đầu tiên</p></div></div>
              <div className="benefit"><span className="benefit-icon">📅</span><div><h4>Đặt lịch dễ dàng</h4><p>Quản lý lịch hẹn tiện lợi</p></div></div>
              <div className="benefit"><span className="benefit-icon">💝</span><div><h4>Tích điểm thưởng</h4><p>Nhận điểm thưởng mỗi lần sử dụng</p></div></div>
            </div>
            <div className="illustration-pets">
              <span>🐶</span><span>🐱</span><span>🐰</span><span>🦜</span><span>🐹</span>
            </div>
          </div>
        </div>

        <div className="register-form-section">
          <div className="form-wrapper">
            <div className="form-header">
              <Link to="/" className="form-logo">🐾 PetCare</Link>
              <h1>Đăng ký tài khoản</h1>
              <p>Điền thông tin để tạo tài khoản mới</p>
            </div>

            <form onSubmit={handleSubmit} className="register-form">
              <div className="form-group">
                <label htmlFor="fullName">👤 Họ và tên *</label>
                <input type="text" id="fullName" value={fullName} onChange={e => setFullName(e.target.value)} placeholder="Nhập họ và tên" required />
              </div>

              <div className="form-group">
                <label htmlFor="email">📧 Email *</label>
                <input type="email" id="email" value={email} onChange={e => setEmail(e.target.value)} placeholder="Nhập email của bạn" required autoComplete="email" />
              </div>

              <div className="form-group">
                <label htmlFor="phone">📞 Số điện thoại</label>
                <input type="tel" id="phone" value={phoneNumber} onChange={e => setPhoneNumber(e.target.value)} placeholder="Nhập số điện thoại (không bắt buộc)" />
              </div>

              <div className="form-group">
                <label htmlFor="password">🔒 Mật khẩu *</label>
                <div className="password-input">
                  <input type={showPassword ? 'text' : 'password'} id="password" value={password} onChange={e => setPassword(e.target.value)} placeholder="Ít nhất 6 ký tự" required />
                  <button type="button" className="toggle-password" onClick={() => setShowPassword(!showPassword)}>
                    {showPassword ? '🙈' : '👁️'}
                  </button>
                </div>
              </div>

              <div className="form-group">
                <label htmlFor="confirmPassword">🔒 Xác nhận mật khẩu *</label>
                <div className="password-input">
                  <input type={showConfirmPassword ? 'text' : 'password'} id="confirmPassword" value={confirmPassword} onChange={e => setConfirmPassword(e.target.value)} placeholder="Nhập lại mật khẩu" required />
                  <button type="button" className="toggle-password" onClick={() => setShowConfirmPassword(!showConfirmPassword)}>
                    {showConfirmPassword ? '🙈' : '👁️'}
                  </button>
                </div>
              </div>

              <div className="terms-check">
                <label>
                  <input type="checkbox" checked={acceptTerms} onChange={e => setAcceptTerms(e.target.checked)} />
                  Tôi đồng ý với <a href="#">Điều khoản sử dụng</a> và <a href="#">Chính sách bảo mật</a>
                </label>
              </div>

              <button type="submit" className="submit-btn" disabled={loading}>
                {loading ? '⏳ Đang xử lý...' : '✨ Đăng ký'}
              </button>
            </form>

            <div className="form-footer">
              <p>Đã có tài khoản? <Link to="/login">Đăng nhập</Link></p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
