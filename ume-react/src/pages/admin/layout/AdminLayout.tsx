import { useState, useEffect, useRef } from 'react';
import { Outlet, NavLink, Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../../contexts/AuthContext';
import { adminApi, notificationApi, contactApi } from '../../../services/api';
import './AdminLayout.scss';

export default function AdminLayout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const isAdmin = user?.role === 'Admin';
  const [collapsed, setCollapsed] = useState(false);
  const [isMobile, setIsMobile] = useState(window.innerWidth < 992);
  const [notifOpen, setNotifOpen] = useState(false);
  const [userOpen, setUserOpen] = useState(false);
  const [notifications, setNotifications] = useState<any[]>([]);
  const [unread, setUnread] = useState(0);
  const [pendingOrders, setPendingOrders] = useState(0);
  const [pendingAppts, setPendingAppts] = useState(0);
  const [newContacts, setNewContacts] = useState(0);
  const dropRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleResize = () => {
      const mobile = window.innerWidth < 992;
      setIsMobile(mobile);
      if (mobile) setCollapsed(true);
    };
    handleResize();
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  useEffect(() => {
    loadCounts();
  }, []);

  async function loadCounts() {
    try {
      const r = await adminApi.getDashboard();
      const d = r.data?.data || r.data;
      if (d) {
        setPendingOrders(d.pendingOrders || 0);
        setPendingAppts(d.pendingAppointments || 0);
      }
    } catch { /* ignore */ }
    try {
      const r = await contactApi.getAll({ limit: '1' });
      const d = r.data?.data || r.data;
      setNewContacts(d?.counts?.new || 0);
    } catch { /* ignore */ }
    try {
      const r = await notificationApi.getAll({ limit: '10' });
      const d = r.data?.data || r.data;
      const list = (d?.notifications || d || []).slice(0, 10);
      setNotifications(list);
      setUnread(list.filter((n: any) => !n.isRead).length);
    } catch { /* ignore */ }
  }

  function handleLogout() {
    logout();
    navigate('/login');
  }

  function toggleFullscreen() {
    if (!document.fullscreenElement) document.documentElement.requestFullscreen();
    else document.exitFullscreen();
  }

  function getNotifIcon(type: string) {
    const m: Record<string, string> = { NewAppointment: '📅', NewOrder: '📦', LowStock: '⚠️', NewReview: '⭐' };
    return m[type] || '🔔';
  }
  function getNotifClass(type: string) {
    const m: Record<string, string> = { NewAppointment: 'bg-success', NewOrder: 'bg-primary', LowStock: 'bg-warn' };
    return m[type] || 'bg-notif-info';
  }

  const avatarUrl = `https://ui-avatars.com/api/?name=${encodeURIComponent(user?.fullName || 'Admin')}&background=667eea&color=fff&size=40&rounded=true`;

  return (
    <div className={`admin-wrapper${collapsed ? ' sidebar-collapsed' : ''}`}>
      <aside className="admin-sidebar">
        <div className="sidebar-brand">
          <img src="https://ui-avatars.com/api/?name=UME&background=667eea&color=fff&size=40&rounded=true" alt="Logo" className="brand-logo" />
          {!collapsed && <span className="brand-text">UME Admin</span>}
        </div>
        <div className="sidebar-user">
          <img src={avatarUrl} className="user-avatar" alt="Avatar" />
          {!collapsed && (
            <div className="user-info">
              <span className="user-name">{user?.fullName || 'Admin'}</span>
              <span className="user-role">{isAdmin ? 'Quản trị viên' : 'Nhân viên'}</span>
            </div>
          )}
        </div>
        <nav className="sidebar-nav">
          {isAdmin && (
            <NavLink to="/admin" end className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}>
              <span className="nav-icon">📊</span>
              {!collapsed && <span>Dashboard</span>}
            </NavLink>
          )}

          {!collapsed && <div className="nav-header">QUẢN LÝ BÁN HÀNG</div>}
          <NavLink to="/admin/products" className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}>
            <span className="nav-icon">📦</span>
            {!collapsed && <span>Sản phẩm</span>}
          </NavLink>
          {isAdmin && (
            <NavLink to="/admin/orders" className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}>
              <span className="nav-icon">🛒</span>
              {!collapsed && <span>Đơn hàng</span>}
              {pendingOrders > 0 && !collapsed && <span className="nav-badge bg-info">{pendingOrders}</span>}
            </NavLink>
          )}
          <NavLink to="/admin/categories" className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}>
            <span className="nav-icon">📁</span>
            {!collapsed && <span>Danh mục</span>}
          </NavLink>
          <NavLink to="/admin/brands" className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}>
            <span className="nav-icon">🏷️</span>
            {!collapsed && <span>Thương hiệu</span>}
          </NavLink>

          {!collapsed && <div className="nav-header">QUẢN LÝ DỊCH VỤ</div>}
          <NavLink to="/admin/appointments" className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}>
            <span className="nav-icon">📅</span>
            {!collapsed && <span>Lịch hẹn</span>}
            {pendingAppts > 0 && !collapsed && <span className="nav-badge bg-warning">{pendingAppts}</span>}
          </NavLink>
          <NavLink to="/admin/services" className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}>
            <span className="nav-icon">✂️</span>
            {!collapsed && <span>Dịch vụ</span>}
          </NavLink>
          {isAdmin && (
            <NavLink to="/admin/staff" className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}>
              <span className="nav-icon">👥</span>
              {!collapsed && <span>Nhân viên</span>}
            </NavLink>
          )}

          {!collapsed && <div className="nav-header">THÚ CƯNG</div>}
          <NavLink to="/admin/pets" className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}>
            <span className="nav-icon">🐾</span>
            {!collapsed && <span>Quản lý thú cưng</span>}
          </NavLink>

          {isAdmin && (
            <>
              {!collapsed && <div className="nav-header">TÀI KHOẢN</div>}
              <NavLink to="/admin/users" className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}>
                <span className="nav-icon">⚙️</span>
                {!collapsed && <span>Quản lý tài khoản</span>}
              </NavLink>
              <NavLink to="/admin/reviews" className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}>
                <span className="nav-icon">⭐</span>
                {!collapsed && <span>Đánh giá</span>}
              </NavLink>

              {!collapsed && <div className="nav-header">MARKETING</div>}
              <NavLink to="/admin/promotions" className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}>
                <span className="nav-icon">💰</span>
                {!collapsed && <span>Khuyến mãi</span>}
              </NavLink>

              {!collapsed && <div className="nav-header">BÁO CÁO</div>}
              <NavLink to="/admin/reports" className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}>
                <span className="nav-icon">📈</span>
                {!collapsed && <span>Báo cáo</span>}
              </NavLink>

              {!collapsed && <div className="nav-header">HỖ TRỢ</div>}
              <NavLink to="/admin/contacts" className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}>
                <span className="nav-icon">✉️</span>
                {!collapsed && <span>Liên hệ</span>}
                {newContacts > 0 && !collapsed && <span className="nav-badge bg-danger">{newContacts}</span>}
              </NavLink>

              {!collapsed && <div className="nav-header">CÀI ĐẶT</div>}
              <NavLink to="/admin/settings" className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}>
                <span className="nav-icon">🔧</span>
                {!collapsed && <span>Cài đặt liên hệ</span>}
              </NavLink>
            </>
          )}
        </nav>
      </aside>

      <div className="admin-main">
        <header className="admin-header">
          <div className="header-left">
            <button className="btn-toggle" onClick={() => setCollapsed(!collapsed)}>☰</button>
            <Link to="/admin" className="header-link d-none-sm">Trang chủ</Link>
          </div>
          <div className="header-right">
            {/* Notifications */}
            <div className="header-dropdown" ref={dropRef}>
              <button className="header-icon-btn" onClick={() => { setNotifOpen(!notifOpen); setUserOpen(false); }}>
                🔔
                {unread > 0 && <span className="notification-badge">{unread}</span>}
              </button>
              {notifOpen && (
                <div className="dropdown-panel notifications-panel">
                  <div className="dropdown-panel-header">
                    <span>Thông báo</span>
                    <button className="btn-clear" onClick={() => { setNotifications([]); setUnread(0); }}>🗑️</button>
                  </div>
                  <div className="dropdown-panel-body">
                    {notifications.length === 0 && (
                      <div className="empty-state">🔕<p>Chưa có thông báo nào</p></div>
                    )}
                    {notifications.map((n, i) => (
                      <div key={i} className={`notification-item${!n.isRead ? ' unread' : ''}`}>
                        <div className={`notif-icon ${getNotifClass(n.type)}`}>{getNotifIcon(n.type)}</div>
                        <div className="notif-content">
                          <p className="notif-message">{n.message}</p>
                          <small className="notif-time">{new Date(n.createdAt).toLocaleDateString('vi-VN')} {new Date(n.createdAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}</small>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>

            <button className="header-icon-btn d-none-md" onClick={toggleFullscreen}>⛶</button>

            {/* User menu */}
            <div className="header-dropdown">
              <button className="user-menu-btn" onClick={() => { setUserOpen(!userOpen); setNotifOpen(false); }}>
                <img src={avatarUrl} alt="User" />
                <span className="d-none-md">{user?.fullName || 'Admin'}</span>
              </button>
              {userOpen && (
                <div className="dropdown-panel user-panel">
                  <div className="user-panel-header">
                    <img src={`https://ui-avatars.com/api/?name=${encodeURIComponent(user?.fullName || 'Admin')}&background=667eea&color=fff&size=80&rounded=true`} alt="User" />
                    <p className="user-panel-name">{user?.fullName || 'Admin'}</p>
                    <small>{isAdmin ? 'Quản trị viên' : 'Nhân viên'}</small>
                  </div>
                  <div className="user-panel-footer">
                    <Link to="/profile" className="btn-default" onClick={() => setUserOpen(false)}>Hồ sơ</Link>
                    <button className="btn-default" onClick={handleLogout}>Đăng xuất</button>
                  </div>
                </div>
              )}
            </div>
          </div>
        </header>

        <div className="admin-content">
          <Outlet />
        </div>

        <footer className="admin-footer">
          <span>© 2025 <a href="/">UME Pet Salon</a>. All rights reserved.</span>
          <span>Version 2.0</span>
        </footer>
      </div>

      {!collapsed && isMobile && <div className="sidebar-overlay" onClick={() => setCollapsed(true)} />}
      {(notifOpen || userOpen) && <div className="dropdown-overlay" onClick={() => { setNotifOpen(false); setUserOpen(false); }} />}
    </div>
  );
}
