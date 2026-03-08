import { useState, useEffect } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { orderApi, appointmentApi, petApi, uploadApi } from '../../services/api';
import { getImageUrl } from '../../services/api';
import api from '../../services/api';
import toast from 'react-hot-toast';
import './Profile.scss';

const formatPrice = (p: number) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p);
const formatDate = (d: string) => new Date(d).toLocaleDateString('vi-VN', { year: 'numeric', month: '2-digit', day: '2-digit' });

const statusLabel: any = {
  Pending: 'Chờ xác nhận', Confirmed: 'Đã xác nhận', Processing: 'Đang xử lý',
  Shipping: 'Đang giao', Completed: 'Hoàn thành', Cancelled: 'Đã hủy',
  InProgress: 'Đang thực hiện', pending: 'Chờ xử lý', confirmed: 'Đã xác nhận',
  processing: 'Đang xử lý', shipped: 'Đang giao', delivered: 'Đã giao',
  completed: 'Hoàn thành', cancelled: 'Đã hủy', scheduled: 'Đã lên lịch', 'in-progress': 'Đang thực hiện',
};
const statusClass: any = {
  Pending: 'warning', Confirmed: 'info', Processing: 'primary', Shipping: 'info',
  Completed: 'success', Cancelled: 'danger', InProgress: 'primary',
  pending: 'warning', confirmed: 'info', processing: 'primary', shipped: 'info',
  delivered: 'success', completed: 'success', cancelled: 'danger', scheduled: 'info', 'in-progress': 'primary',
};

export default function Profile() {
  const { user, updateUser } = useAuth();
  const [searchParams] = useSearchParams();
  const [tab, setTab] = useState('info');
  const [profile, setProfile] = useState<any>({});
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  // Stats
  const [orderCount, setOrderCount] = useState(0);
  const [aptCount, setAptCount] = useState(0);
  const [petCount, setPetCount] = useState(0);

  // Password
  const [pwd, setPwd] = useState({ current: '', newPwd: '', confirm: '' });
  const [changingPwd, setChangingPwd] = useState(false);

  // Avatar
  const [avatarPreview, setAvatarPreview] = useState<string | null>(null);
  const [avatarFile, setAvatarFile] = useState<File | null>(null);

  // Tab data
  const [orders, setOrders] = useState<any[]>([]);
  const [appointments, setAppointments] = useState<any[]>([]);
  const [pets, setPets] = useState<any[]>([]);
  const [loadingOrders, setLoadingOrders] = useState(false);
  const [loadingApts, setLoadingApts] = useState(false);
  const [loadingPets, setLoadingPets] = useState(false);

  useEffect(() => {
    loadProfile();
    loadStats();
    const t = searchParams.get('tab');
    if (t) switchTab(t);
  }, []);

  const loadProfile = async () => {
    setLoading(true);
    try {
      const res = await api.get('/auth/profile');
      const data = res.data?.data || res.data;
      setProfile(data);
    } catch {} finally { setLoading(false); }
  };

  const loadStats = async () => {
    try {
      const [o, a, p] = await Promise.all([orderApi.getMyOrders(), appointmentApi.getMyAppointments(), petApi.getMyPets()]);
      const od = o.data?.data; const ad = a.data?.data; const pd = p.data?.data;
      setOrderCount(Array.isArray(od) ? od.length : od?.orders?.length || od?.items?.length || 0);
      setAptCount(Array.isArray(ad) ? ad.length : ad?.appointments?.length || ad?.items?.length || 0);
      setPetCount(Array.isArray(pd) ? pd.length : pd?.length || 0);
    } catch {}
  };

  const switchTab = (t: string) => {
    setTab(t);
    if (t === 'orders' && orders.length === 0) loadOrders();
    if (t === 'appointments' && appointments.length === 0) loadAppointments();
    if (t === 'pets' && pets.length === 0) loadPets();
  };

  const loadOrders = async () => {
    setLoadingOrders(true);
    try {
      const r = await orderApi.getMyOrders();
      const d = r.data?.data;
      setOrders(Array.isArray(d) ? d : d?.orders || d?.items || []);
    } catch {} finally { setLoadingOrders(false); }
  };
  const loadAppointments = async () => {
    setLoadingApts(true);
    try {
      const r = await appointmentApi.getMyAppointments();
      const d = r.data?.data;
      setAppointments(Array.isArray(d) ? d : d?.appointments || d?.items || []);
    } catch {} finally { setLoadingApts(false); }
  };
  const loadPets = async () => {
    setLoadingPets(true);
    try {
      const r = await petApi.getMyPets();
      const d = r.data?.data;
      setPets(Array.isArray(d) ? d : d?.pets || []);
    } catch {} finally { setLoadingPets(false); }
  };

  const onAvatarSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const f = e.target.files?.[0];
    if (!f) return;
    setAvatarFile(f);
    const reader = new FileReader();
    reader.onload = ev => setAvatarPreview(ev.target?.result as string);
    reader.readAsDataURL(f);
  };

  const saveProfile = async () => {
    setSaving(true);
    try {
      let avatarUrl = profile.avatarUrl;
      if (avatarFile) {
        const fd = new FormData();
        fd.append('file', avatarFile);
        fd.append('folder', 'avatars');
        const uRes = await uploadApi.upload(fd);
        avatarUrl = uRes.data?.data?.url || uRes.data?.url;
      }
      const data = {
        fullName: profile.fullName, phoneNumber: profile.phoneNumber,
        gender: profile.gender, dateOfBirth: profile.dateOfBirth,
        avatarUrl, address: profile.address,
      };
      await api.put('/auth/profile', data);
      setAvatarFile(null); setAvatarPreview(null);
      toast.success('Cập nhật hồ sơ thành công!');
      if (user) updateUser({ ...user, ...data });
      loadProfile();
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Cập nhật thất bại');
    } finally { setSaving(false); }
  };

  const changePassword = async () => {
    if (!pwd.current || !pwd.newPwd) { toast.error('Vui lòng nhập đầy đủ thông tin'); return; }
    if (pwd.newPwd !== pwd.confirm) { toast.error('Mật khẩu xác nhận không khớp'); return; }
    if (pwd.newPwd.length < 6) { toast.error('Mật khẩu mới phải có ít nhất 6 ký tự'); return; }
    setChangingPwd(true);
    try {
      await api.put('/auth/change-password', { currentPassword: pwd.current, newPassword: pwd.newPwd });
      setPwd({ current: '', newPwd: '', confirm: '' });
      toast.success('Đổi mật khẩu thành công!');
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Đổi mật khẩu thất bại');
    } finally { setChangingPwd(false); }
  };

  if (loading) return <div className="profile-page"><div className="container" style={{ padding: '100px 0', textAlign: 'center' }}>⏳ Đang tải...</div></div>;

  return (
    <div className="profile-page">
      <div className="page-header"><div className="container"><h1>👤 Hồ sơ cá nhân</h1></div></div>
      <div className="container">
        <div className="profile-layout">
          {/* Sidebar */}
          <div className="profile-sidebar">
            <div className="sidebar-card">
              <div className="avatar-section">
                <div className="avatar-wrapper">
                  <img src={avatarPreview || getImageUrl(profile.avatarUrl) || '/assets/images/default-avatar.svg'} alt="Avatar" className="avatar-img"
                    onError={e => { (e.target as HTMLImageElement).src = '/assets/images/default-avatar.svg'; }} />
                  <label className="avatar-upload">📷<input type="file" accept="image/*" onChange={onAvatarSelect} hidden /></label>
                </div>
                <h3>{profile.fullName || 'Người dùng'}</h3>
                <p className="email-text">{profile.email}</p>
              </div>
              <div className="role-badges">
                {profile.role === 'admin' && <span className="badge badge-admin">🛡️ Admin</span>}
                <span className="badge badge-customer">👤 Khách hàng</span>
              </div>
              <div className="sidebar-info">
                {profile.phoneNumber && <div className="info-item">📱 {profile.phoneNumber}</div>}
                {profile.address && <div className="info-item">📍 {typeof profile.address === 'string' ? profile.address : [profile.address?.street, profile.address?.ward, profile.address?.district, profile.address?.city].filter(Boolean).join(', ')}</div>}
                <div className="info-item">📅 Tham gia: {formatDate(profile.createdAt)}</div>
              </div>
              <div className="sidebar-stats">
                <div className="stat-item" onClick={() => switchTab('orders')}><span className="stat-number">{orderCount}</span><span className="stat-label">Đơn hàng</span></div>
                <div className="stat-item" onClick={() => switchTab('appointments')}><span className="stat-number">{aptCount}</span><span className="stat-label">Lịch hẹn</span></div>
                <div className="stat-item" onClick={() => switchTab('pets')}><span className="stat-number">{petCount}</span><span className="stat-label">Thú cưng</span></div>
              </div>
            </div>
          </div>

          {/* Content */}
          <div className="profile-content">
            <div className="profile-tabs">
              {[['info', '👤 Thông tin'], ['password', '🔒 Đổi mật khẩu'], ['orders', '📦 Đơn hàng'], ['appointments', '📅 Lịch hẹn'], ['pets', '🐾 Thú cưng']].map(([k, l]) => (
                <button key={k} className={`tab-btn ${tab === k ? 'active' : ''}`} onClick={() => switchTab(k)}>{l}
                  {k === 'orders' && orderCount > 0 && <span className="tab-badge">{orderCount}</span>}
                  {k === 'appointments' && aptCount > 0 && <span className="tab-badge">{aptCount}</span>}
                  {k === 'pets' && petCount > 0 && <span className="tab-badge">{petCount}</span>}
                </button>
              ))}
            </div>

            {/* Info tab */}
            {tab === 'info' && (
              <div className="content-card">
                <h3>✏️ Chỉnh sửa thông tin</h3>
                <div className="form-grid">
                  <div className="form-group"><label>Họ và tên *</label><input value={profile.fullName || ''} onChange={e => setProfile((p: any) => ({ ...p, fullName: e.target.value }))} /></div>
                  <div className="form-group"><label>Email</label><input value={profile.email || ''} disabled /><small>Email không thể thay đổi</small></div>
                  <div className="form-group"><label>Số điện thoại</label><input value={profile.phoneNumber || ''} onChange={e => setProfile((p: any) => ({ ...p, phoneNumber: e.target.value }))} /></div>
                  <div className="form-group"><label>Giới tính</label>
                    <select value={profile.gender || ''} onChange={e => setProfile((p: any) => ({ ...p, gender: e.target.value }))}>
                      <option value="">Chọn giới tính</option><option value="Nam">Nam</option><option value="Nữ">Nữ</option><option value="Khác">Khác</option>
                    </select>
                  </div>
                  <div className="form-group"><label>Ngày sinh</label><input type="date" value={profile.dateOfBirth?.split('T')[0] || ''} onChange={e => setProfile((p: any) => ({ ...p, dateOfBirth: e.target.value }))} /></div>
                  <div className="form-group full"><label>Địa chỉ</label><textarea value={typeof profile.address === 'string' ? profile.address : ''} onChange={e => setProfile((p: any) => ({ ...p, address: e.target.value }))} rows={3} /></div>
                </div>
                <button className="btn-save" onClick={saveProfile} disabled={saving}>{saving ? '⏳ Đang lưu...' : '💾 Lưu thay đổi'}</button>
              </div>
            )}

            {/* Password tab */}
            {tab === 'password' && (
              <div className="content-card">
                <h3>🔑 Đổi mật khẩu</h3>
                <div className="password-form">
                  <div className="form-group"><label>Mật khẩu hiện tại</label><input type="password" value={pwd.current} onChange={e => setPwd(p => ({ ...p, current: e.target.value }))} /></div>
                  <div className="form-group"><label>Mật khẩu mới</label><input type="password" value={pwd.newPwd} onChange={e => setPwd(p => ({ ...p, newPwd: e.target.value }))} placeholder="Ít nhất 6 ký tự" /></div>
                  <div className="form-group"><label>Xác nhận mật khẩu</label><input type="password" value={pwd.confirm} onChange={e => setPwd(p => ({ ...p, confirm: e.target.value }))} /></div>
                  <button className="btn-save" onClick={changePassword} disabled={changingPwd}>{changingPwd ? '⏳ Đang đổi...' : '🔑 Đổi mật khẩu'}</button>
                </div>
              </div>
            )}

            {/* Orders tab */}
            {tab === 'orders' && (
              <div className="content-card">
                <h3>📦 Đơn hàng của tôi</h3>
                {loadingOrders && <p className="loading-inline">⏳ Đang tải...</p>}
                {!loadingOrders && orders.length === 0 && <div className="empty-state"><p>📭 Chưa có đơn hàng nào</p><Link to="/products" className="btn-action">Mua sắm ngay</Link></div>}
                <div className="data-list">
                  {orders.map(o => (
                    <div key={o._id} className="data-item">
                      <div className="item-header"><span className="item-code">#{o.orderCode || o._id?.substring(0, 8)}</span><span className={`status-badge badge-${statusClass[o.status] || 'secondary'}`}>{statusLabel[o.status] || o.status}</span></div>
                      <div className="item-details"><span>📅 {formatDate(o.createdAt)}</span><span>💰 {formatPrice(o.totalAmount || 0)}</span><span>📦 {o.items?.length || 0} sản phẩm</span></div>
                    </div>
                  ))}
                </div>
                {orders.length > 0 && <Link to="/my-orders" className="view-all-link">Xem tất cả đơn hàng →</Link>}
              </div>
            )}

            {/* Appointments tab */}
            {tab === 'appointments' && (
              <div className="content-card">
                <h3>📅 Lịch hẹn của tôi</h3>
                {loadingApts && <p className="loading-inline">⏳ Đang tải...</p>}
                {!loadingApts && appointments.length === 0 && <div className="empty-state"><p>📅 Chưa có lịch hẹn nào</p><Link to="/booking" className="btn-action">Đặt lịch ngay</Link></div>}
                <div className="data-list">
                  {appointments.map(a => (
                    <div key={a._id} className="data-item">
                      <div className="item-header"><span className="item-code">{a.appointmentCode}</span><span className={`status-badge badge-${statusClass[a.status] || 'secondary'}`}>{statusLabel[a.status] || a.status}</span></div>
                      <div className="item-details">
                        <span>📅 {formatDate(a.appointmentDate || a.date)}</span>
                        <span>🕐 {a.startTime || a.timeSlot}</span>
                        {a.pet && <span>🐾 {a.pet.name}</span>}
                      </div>
                    </div>
                  ))}
                </div>
                {appointments.length > 0 && <Link to="/my-appointments" className="view-all-link">Xem tất cả lịch hẹn →</Link>}
              </div>
            )}

            {/* Pets tab */}
            {tab === 'pets' && (
              <div className="content-card">
                <h3>🐾 Thú cưng của tôi</h3>
                {loadingPets && <p className="loading-inline">⏳ Đang tải...</p>}
                {!loadingPets && pets.length === 0 && <div className="empty-state"><p>🐾 Chưa có thú cưng nào</p><Link to="/my-pets" className="btn-action">Thêm thú cưng</Link></div>}
                <div className="pets-mini-grid">
                  {pets.map(p => (
                    <div key={p._id} className="pet-card-mini">
                      <img src={getImageUrl(p.imageUrl)} alt={p.name} onError={e => { (e.target as HTMLImageElement).src = '/assets/images/default-pet.svg'; }} />
                      <div className="pet-info"><h4>{p.name}</h4><p>{p.breed || p.type}</p><span>{p.gender === 'Male' ? 'Đực' : 'Cái'} · {p.age} {p.ageUnit === 'years' ? 'tuổi' : 'tháng'}</span></div>
                    </div>
                  ))}
                </div>
                {pets.length > 0 && <Link to="/my-pets" className="view-all-link">Xem tất cả thú cưng →</Link>}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
