import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import config from '../../config';
import toast from 'react-hot-toast';
import './Login.scss';

export default function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [googleLoading, setGoogleLoading] = useState(false);
  const { login, updateUser } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    // Handle auth result from popup (postMessage for same-origin, storage for cross-origin)
    function handleMessage(e: MessageEvent) {
      if (e.data?.type === 'google-auth-success' && e.data.data) {
        processGoogleAuth(e.data.data);
      } else if (e.data?.type === 'google-auth-error') {
        toast.error('Đăng nhập Google thất bại');
        setGoogleLoading(false);
      }
    }

    function handleStorage(e: StorageEvent) {
      if (e.key === 'google-auth-result' && e.newValue) {
        processStorageResult(e.newValue);
      }
    }

    function processStorageResult(raw: string) {
      try {
        const result = JSON.parse(raw);
        localStorage.removeItem('google-auth-result');
        if (result.success && result.data) {
          processGoogleAuth(result.data);
        } else {
          toast.error('Đăng nhập Google thất bại');
          setGoogleLoading(false);
        }
      } catch {
        setGoogleLoading(false);
      }
    }

    function processGoogleAuth(data: { user: any; accessToken: string; refreshToken: string }) {
      const { user, accessToken, refreshToken } = data;
      localStorage.setItem('accessToken', accessToken);
      localStorage.setItem('refreshToken', refreshToken);
      localStorage.setItem('user', JSON.stringify(user));
      updateUser(user);
      toast.success('Đăng nhập Google thành công!');
      setGoogleLoading(false);
      navigate('/');
    }

    window.addEventListener('message', handleMessage);
    window.addEventListener('storage', handleStorage);
    return () => {
      window.removeEventListener('message', handleMessage);
      window.removeEventListener('storage', handleStorage);
    };
  }, [navigate, updateUser]);

  function handleGoogleLogin() {
    setGoogleLoading(true);
    localStorage.removeItem('google-auth-result');
    const apiBase = config.apiUrl;
    const w = 500, h = 600;
    const left = window.screenX + (window.outerWidth - w) / 2;
    const top = window.screenY + (window.outerHeight - h) / 2;
    const popup = window.open(`${apiBase}/auth/google/redirect`, 'google-login', `width=${w},height=${h},left=${left},top=${top}`);
    if (!popup) {
      toast.error('Vui lòng cho phép popup');
      setGoogleLoading(false);
      return;
    }
    // Poll localStorage as fallback (storage event may not fire in all browsers)
    const interval = setInterval(() => {
      try {
        const raw = localStorage.getItem('google-auth-result');
        if (raw) {
          clearInterval(interval);
          localStorage.removeItem('google-auth-result');
          const result = JSON.parse(raw);
          if (result.success && result.data) {
            const { user, accessToken, refreshToken } = result.data;
            localStorage.setItem('accessToken', accessToken);
            localStorage.setItem('refreshToken', refreshToken);
            localStorage.setItem('user', JSON.stringify(user));
            updateUser(user);
            toast.success('Đăng nhập Google thành công!');
            navigate('/');
          } else {
            toast.error('Đăng nhập Google thất bại');
          }
          setGoogleLoading(false);
        }
        if (popup.closed && !localStorage.getItem('google-auth-result')) {
          clearInterval(interval);
          setGoogleLoading(false);
        }
      } catch (err) {
        clearInterval(interval);
        localStorage.removeItem('google-auth-result');
        toast.error('Đăng nhập Google thất bại');
        setGoogleLoading(false);
      }
    }, 500);
  }

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

            <button type="button" className="google-btn" onClick={handleGoogleLogin} disabled={googleLoading}>
              <svg width="20" height="20" viewBox="0 0 48 48"><path fill="#EA4335" d="M24 9.5c3.54 0 6.71 1.22 9.21 3.6l6.85-6.85C35.9 2.38 30.47 0 24 0 14.62 0 6.51 5.38 2.56 13.22l7.98 6.19C12.43 13.72 17.74 9.5 24 9.5z"/><path fill="#4285F4" d="M46.98 24.55c0-1.57-.15-3.09-.38-4.55H24v9.02h12.94c-.58 2.96-2.26 5.48-4.78 7.18l7.73 6c4.51-4.18 7.09-10.36 7.09-17.65z"/><path fill="#FBBC05" d="M10.53 28.59c-.48-1.45-.76-2.99-.76-4.59s.27-3.14.76-4.59l-7.98-6.19C.92 16.46 0 20.12 0 24c0 3.88.92 7.54 2.56 10.78l7.97-6.19z"/><path fill="#34A853" d="M24 48c6.48 0 11.93-2.13 15.89-5.81l-7.73-6c-2.15 1.45-4.92 2.3-8.16 2.3-6.26 0-11.57-4.22-13.47-9.91l-7.98 6.19C6.51 42.62 14.62 48 24 48z"/></svg>
              {googleLoading ? 'Đang xử lý...' : 'Đăng nhập với Google'}
            </button>

            <div className="divider"><span>hoặc</span></div>

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
