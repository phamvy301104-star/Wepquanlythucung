import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import toast from 'react-hot-toast';
import './Login.scss';

export default function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email || !password) {
      toast.error('Vui lòng nhập đầy đủ thông tin');
      return;
    }

    setLoading(true);
    try {
      await login({ email, password });
      toast.success('Đăng nhập thành công!');
      navigate('/');
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Đăng nhập thất bại. Vui lòng thử lại.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <div className="paw-decorations">
        {[1,2,3,4,5,6].map(i => <span key={i} className={`paw paw-${i}`}>🐾</span>)}
      </div>

      <div className="login-container">
        <div className="login-illustration">
          <div className="illustration-content">
            <div className="illustration-emoji">🐕‍🦺</div>
            <h2>Chào mừng trở lại!</h2>
            <p>Đăng nhập để đặt lịch chăm sóc thú cưng, mua sắm sản phẩm và quản lý thú cưng của bạn.</p>
            <div className="illustration-features">
              <div className="feature">✅ Đặt lịch nhanh chóng</div>
              <div className="feature">✅ Theo dõi lịch hẹn</div>
              <div className="feature">✅ Ưu đãi độc quyền</div>
              <div className="feature">✅ Quản lý thú cưng</div>
            </div>
            <div className="illustration-pets">
              <span>🐶</span><span>🐱</span><span>🐰</span><span>🐹</span>
            </div>
          </div>
        </div>

        <div className="login-form-section">
          <div className="form-wrapper">
            <div className="form-header">
              <Link to="/" className="form-logo">🐾 PetCare</Link>
              <h1>Đăng nhập</h1>
              <p>Nhập thông tin tài khoản của bạn</p>
            </div>

            <div className="divider"><span>Đăng nhập với email</span></div>

            <form onSubmit={handleSubmit} className="login-form">
              <div className="form-group">
                <label htmlFor="email">📧 Email</label>
                <input type="email" id="email" value={email} onChange={e => setEmail(e.target.value)} placeholder="Nhập email của bạn" required autoComplete="email" />
              </div>

              <div className="form-group">
                <label htmlFor="password">🔒 Mật khẩu</label>
                <div className="password-input">
                  <input type={showPassword ? 'text' : 'password'} id="password" value={password} onChange={e => setPassword(e.target.value)} placeholder="Nhập mật khẩu" required autoComplete="current-password" />
                  <button type="button" className="toggle-password" onClick={() => setShowPassword(!showPassword)}>
                    {showPassword ? '🙈' : '👁️'}
                  </button>
                </div>
              </div>

              <div className="form-options">
                <label className="remember-me">
                  <input type="checkbox" /> Ghi nhớ đăng nhập
                </label>
                <a href="#" className="forgot-password">Quên mật khẩu?</a>
              </div>

              <button type="submit" className="submit-btn" disabled={loading}>
                {loading ? '⏳ Đang xử lý...' : '🔑 Đăng nhập'}
              </button>
            </form>

            <div className="form-footer">
              <p>Chưa có tài khoản? <Link to="/register">Đăng ký ngay</Link></p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
