import { useState, useEffect, useRef } from 'react';
import { Link, NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { useCart } from '../../contexts/CartContext';
import { getImageUrl } from '../../services/api';
import './Navbar.scss';

export default function Navbar() {
  const { isLoggedIn, isAdminOrStaff, user, logout } = useAuth();
  const { totalItems } = useCart();
  const navigate = useNavigate();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [userDropdownOpen, setUserDropdownOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target as Node)) {
        setUserDropdownOpen(false);
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const navigateTo = (path: string, queryParams?: Record<string, string>) => {
    setUserDropdownOpen(false);
    setMobileMenuOpen(false);
    const search = queryParams ? '?' + new URLSearchParams(queryParams).toString() : '';
    navigate(path + search);
  };

  const handleLogout = () => {
    logout();
    setUserDropdownOpen(false);
    navigate('/');
  };

  return (
    <>
      <nav className="navbar">
        <div className="container">
          <Link to="/" className="navbar-brand">
            <span className="brand-icon">🐾</span> PetCare
          </Link>

          <button className="navbar-toggler" onClick={() => setMobileMenuOpen(!mobileMenuOpen)}>
            <span className="navbar-toggler-icon">☰</span>
          </button>

          <div className={`navbar-collapse ${mobileMenuOpen ? 'show' : ''}`}>
            <ul className="navbar-nav">
              <li className="nav-item">
                <NavLink to="/" className="nav-link" end onClick={() => setMobileMenuOpen(false)}>Trang chủ</NavLink>
              </li>
              <li className="nav-item">
                <NavLink to="/services" className="nav-link" onClick={() => setMobileMenuOpen(false)}>🐾 Dịch vụ thú cưng</NavLink>
              </li>
              <li className="nav-item">
                <NavLink to="/products" className="nav-link" onClick={() => setMobileMenuOpen(false)}>Sản phẩm</NavLink>
              </li>
              <li className="nav-item">
                <NavLink to="/pets" className="nav-link" onClick={() => setMobileMenuOpen(false)}>
                  ❤️ Thú cưng
                </NavLink>
              </li>
              <li className="nav-item">
                <NavLink to="/booking" className="nav-link" onClick={() => setMobileMenuOpen(false)}>Đặt lịch</NavLink>
              </li>
              <li className="nav-item">
                <NavLink to="/contact" className="nav-link" onClick={() => setMobileMenuOpen(false)}>Liên hệ</NavLink>
              </li>
              <li className="nav-item">
                <NavLink to="/pet-recognition" className="nav-link ai-link" onClick={() => setMobileMenuOpen(false)}>
                  ✨ AI Nhận diện
                </NavLink>
              </li>
            </ul>

            <div className="navbar-right">
              <Link to="/cart" className="cart-icon" title="Giỏ hàng">
                🛒
                {totalItems > 0 && <span className="cart-badge-count">{totalItems}</span>}
              </Link>

              {!isLoggedIn ? (
                <>
                  <Link to="/login" className="btn-outline-primary-custom">Đăng nhập</Link>
                  <Link to="/register" className="btn-primary-custom">Đăng ký</Link>
                </>
              ) : (
                <div className="dropdown" ref={dropdownRef}>
                  <button className="user-dropdown-toggle" onClick={() => setUserDropdownOpen(!userDropdownOpen)}>
                    <div className="user-avatar-circle">
                      {user?.avatarUrl ? (
                        <img src={getImageUrl(user.avatarUrl)} alt="Avatar" />
                      ) : (
                        <span>{user?.fullName?.charAt(0) || 'U'}</span>
                      )}
                    </div>
                    <span className="user-display-name">{user?.fullName}</span>
                    <span>▼</span>
                  </button>
                  <div className={`custom-dropdown-menu ${userDropdownOpen ? 'show' : ''}`}>
                    <a className="dropdown-item" onClick={() => navigateTo('/profile')}>
                      👤 Hồ sơ cá nhân
                    </a>
                    <a className="dropdown-item" onClick={() => navigateTo('/profile', { tab: 'orders' })}>
                      🛍️ Đơn hàng của tôi
                    </a>
                    <a className="dropdown-item" onClick={() => navigateTo('/profile', { tab: 'appointments' })}>
                      📅 Lịch hẹn của tôi
                    </a>
                    <a className="dropdown-item" onClick={() => navigateTo('/profile', { tab: 'pets' })}>
                      ❤️ Thú cưng của tôi
                    </a>
                    {isAdminOrStaff && (
                      <>
                        <div className="dropdown-divider" />
                        <a className="dropdown-item" onClick={() => navigateTo('/admin')}>
                          ⚙️ Trang quản trị
                        </a>
                      </>
                    )}
                    <div className="dropdown-divider" />
                    <a className="dropdown-item text-danger" onClick={handleLogout}>
                      🚪 Đăng xuất
                    </a>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      </nav>
      {userDropdownOpen && <div className="dropdown-overlay" onClick={() => setUserDropdownOpen(false)} />}
    </>
  );
}
