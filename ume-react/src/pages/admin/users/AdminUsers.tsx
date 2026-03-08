import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { userApi, getImageUrl } from '../../../services/api';
import '../shared/admin.scss';

function fmtD(d: string) { return d ? new Date(d).toLocaleDateString('vi-VN') : '-'; }

export default function AdminUsers() {
  const [items, setItems] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState('');
  const [roleFilter, setRoleFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [stats, setStats] = useState({ total: 0, admins: 0, active: 0, blocked: 0 });

  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<any>(null);
  const [fd, setFd] = useState<any>({});
  const [saving, setSaving] = useState(false);

  const [showDelete, setShowDelete] = useState(false);
  const [deleteItem, setDeleteItem] = useState<any>(null);

  useEffect(() => { load(); }, []);

  async function load() {
    setLoading(true);
    try {
      const p: any = { limit: '200' };
      if (search) p.search = search;
      if (roleFilter) p.role = roleFilter;
      if (statusFilter) p.status = statusFilter;
      const r = await userApi.getAll(p);
      const d = r.data?.data || r.data;
      const list = d?.users || d || [];
      setItems(list);
      setStats({
        total: list.length,
        admins: list.filter((u: any) => u.role === 'Admin').length,
        active: list.filter((u: any) => u.isActive !== false).length,
        blocked: list.filter((u: any) => u.isActive === false).length,
      });
    } catch {}
    setLoading(false);
  }

  function openForm(item?: any) {
    setEditing(item || null);
    setFd(item ? {
      fullName: item.fullName || '', email: item.email || '', phoneNumber: item.phoneNumber || '',
      role: item.role || 'Customer', isActive: item.isActive !== false,
      address: item.address || '', city: item.city || '', district: item.district || '', ward: item.ward || '',
    } : { fullName: '', email: '', phoneNumber: '', role: 'Staff', isActive: true, password: '', address: '', city: '', district: '', ward: '' });
    setShowForm(true);
  }

  async function save() {
    if (!fd.fullName || !fd.email) { toast.error('Nhập đủ thông tin bắt buộc'); return; }
    if (!editing && !fd.password) { toast.error('Nhập mật khẩu'); return; }
    setSaving(true);
    try {
      const payload: any = { ...fd };
      if (editing && !payload.password) delete payload.password;
      if (editing) await userApi.update(editing._id, payload);
      else await userApi.create(payload);
      toast.success(editing ? 'Đã cập nhật!' : 'Đã tạo tài khoản!');
      setShowForm(false); load();
    } catch (e: any) { toast.error(e.response?.data?.message || 'Có lỗi xảy ra'); }
    setSaving(false);
  }

  async function toggleActive(item: any) {
    try {
      await userApi.update(item._id, { isActive: !item.isActive });
      toast.success(item.isActive ? 'Đã khóa tài khoản' : 'Đã mở khóa');
      load();
    } catch { toast.error('Có lỗi xảy ra'); }
  }

  async function doDelete() {
    if (!deleteItem) return;
    try { await userApi.delete(deleteItem._id); toast.success('Đã xóa!'); setShowDelete(false); load(); }
    catch { toast.error('Có lỗi xảy ra'); }
  }

  return (
    <div>
      <div className="page-header">
        <div><h4>👤 Quản lý người dùng</h4>
          <ol className="breadcrumb"><li><Link to="/admin">Dashboard</Link></li><li className="active">Người dùng</li></ol>
        </div>
        <button className="btn btn-gold" onClick={() => openForm()}>➕ Thêm người dùng</button>
      </div>

      <div className="stats-row">
        <div className="stat-box"><div className="sb-icon bg-info">👥</div><div><div className="sb-num">{stats.total}</div><div className="sb-label">Tổng người dùng</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-gold">👑</div><div><div className="sb-num">{stats.admins}</div><div className="sb-label">Quản trị viên</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-success">✅</div><div><div className="sb-num">{stats.active}</div><div className="sb-label">Hoạt động</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-danger">🚫</div><div><div className="sb-num">{stats.blocked}</div><div className="sb-label">Bị khóa</div></div></div>
      </div>

      <div className="card">
        <div className="card-header">
          <div className="filter-row">
            <div className="search-box">
              <input className="form-control" value={search} onChange={e => setSearch(e.target.value)} placeholder="Tìm người dùng..." onKeyDown={e => e.key === 'Enter' && load()} />
              <button className="btn btn-gold btn-sm" onClick={load}>🔍</button>
            </div>
            <select className="form-control fc-sm" value={roleFilter} onChange={e => { setRoleFilter(e.target.value); setTimeout(load, 0); }}>
              <option value="">Tất cả vai trò</option>
              <option value="Admin">Admin</option>
              <option value="Staff">Nhân viên</option>
              <option value="Customer">Khách hàng</option>
            </select>
            <select className="form-control fc-sm" value={statusFilter} onChange={e => { setStatusFilter(e.target.value); setTimeout(load, 0); }}>
              <option value="">Tất cả trạng thái</option>
              <option value="active">Hoạt động</option>
              <option value="blocked">Bị khóa</option>
            </select>
          </div>
        </div>
        <div className="card-body p-0">
          <table className="adm-table">
            <thead><tr><th>#</th><th>Avatar</th><th>Họ tên</th><th>Email</th><th>SĐT</th><th>Vai trò</th><th>Ngày tạo</th><th>Trạng thái</th><th>Thao tác</th></tr></thead>
            <tbody>
              {items.map((u, i) => (
                <tr key={u._id}>
                  <td>{i + 1}</td>
                  <td><img src={getImageUrl(u.avatarUrl)} alt="" className="img-preview" style={{ borderRadius: '50%' }} /></td>
                  <td><strong>{u.fullName}</strong>{u.googleId && <span className="sub-txt"> (Google)</span>}</td>
                  <td className="sub-txt">{u.email}</td>
                  <td>{u.phoneNumber || '-'}</td>
                  <td><span className={`os-badge ${u.role === 'Admin' ? 'os-confirmed' : u.role === 'Staff' ? 'os-processing' : 'os-pending'}`}>{u.role === 'Admin' ? '👑 Admin' : u.role === 'Staff' ? '👥 Nhân viên' : '👤 Khách hàng'}</span></td>
                  <td>{fmtD(u.createdAt)}</td>
                  <td>
                    <label className="toggle-switch" title={u.isActive !== false ? 'Đang hoạt động' : 'Đã khóa'}>
                      <input type="checkbox" checked={u.isActive !== false} onChange={() => toggleActive(u)} disabled={u.role === 'Admin'} />
                      <span className="toggle-slider"></span>
                    </label>
                  </td>
                  <td><div className="act-g">
                    {u.role !== 'Admin' && <button className="ab ab-edit" onClick={() => openForm(u)}>✏️</button>}
                    {u.role !== 'Admin' && <button className="ab ab-del" onClick={() => { setDeleteItem(u); setShowDelete(true); }}>🗑️</button>}
                  </div></td>
                </tr>
              ))}
              {!items.length && !loading && <tr><td colSpan={9} className="empty">Không có người dùng nào</td></tr>}
              {loading && <tr><td colSpan={9} className="empty">Đang tải...</td></tr>}
            </tbody>
          </table>
        </div>
      </div>

      {showForm && (
        <div className="mo" onClick={() => setShowForm(false)}>
          <div className="md md-lg" onClick={e => e.stopPropagation()}>
            <div className="mh bg-g"><h5>{editing ? '✏️ Sửa' : '➕ Thêm'} người dùng</h5><button className="mx" onClick={() => setShowForm(false)}>&times;</button></div>
            <div className="mb-modal">
              <h6>Thông tin cá nhân</h6>
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>Họ tên <span className="req">*</span></label><input className="form-control" value={fd.fullName} onChange={e => setFd({ ...fd, fullName: e.target.value })} /></div></div>
                <div className="adm-col-6"><div className="fg"><label>Email <span className="req">*</span></label><input className="form-control" type="email" value={fd.email} onChange={e => setFd({ ...fd, email: e.target.value })} disabled={!!editing} /></div></div>
              </div>
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>SĐT</label><input className="form-control" value={fd.phoneNumber} onChange={e => setFd({ ...fd, phoneNumber: e.target.value })} /></div></div>
                <div className="adm-col-6"><div className="fg"><label>{editing ? 'Mật khẩu mới (bỏ trống nếu không đổi)' : 'Mật khẩu'} {!editing && <span className="req">*</span>}</label><input className="form-control" type="password" value={fd.password || ''} onChange={e => setFd({ ...fd, password: e.target.value })} /></div></div>
              </div>
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>Vai trò</label>
                  <select className="form-control" value={fd.role} onChange={e => setFd({ ...fd, role: e.target.value })}>
                    <option value="Staff">Nhân viên</option><option value="Admin">Admin</option><option value="Customer">Khách hàng</option>
                  </select>
                </div></div>
                <div className="adm-col-6"><div className="fg"><label>Trạng thái</label>
                  <select className="form-control" value={fd.isActive ? 'active' : 'blocked'} onChange={e => setFd({ ...fd, isActive: e.target.value === 'active' })}>
                    <option value="active">Hoạt động</option><option value="blocked">Khóa</option>
                  </select>
                </div></div>
              </div>
              <h6 style={{ marginTop: 16 }}>Địa chỉ</h6>
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>Địa chỉ</label><input className="form-control" value={fd.address} onChange={e => setFd({ ...fd, address: e.target.value })} /></div></div>
                <div className="adm-col-6"><div className="fg"><label>Phường/Xã</label><input className="form-control" value={fd.ward} onChange={e => setFd({ ...fd, ward: e.target.value })} /></div></div>
              </div>
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>Quận/Huyện</label><input className="form-control" value={fd.district} onChange={e => setFd({ ...fd, district: e.target.value })} /></div></div>
                <div className="adm-col-6"><div className="fg"><label>Tỉnh/TP</label><input className="form-control" value={fd.city} onChange={e => setFd({ ...fd, city: e.target.value })} /></div></div>
              </div>
            </div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowForm(false)}>Hủy</button><button className="btn btn-gold" onClick={save} disabled={saving}>{saving ? 'Đang lưu...' : '💾 Lưu'}</button></div>
          </div>
        </div>
      )}

      {showDelete && (
        <div className="mo" onClick={() => setShowDelete(false)}>
          <div className="md" onClick={e => e.stopPropagation()}>
            <div className="mh bg-r"><h5>🗑️ Xóa người dùng</h5><button className="mx" onClick={() => setShowDelete(false)}>&times;</button></div>
            <div className="mb-modal"><p>Bạn có chắc muốn xóa <strong>{deleteItem?.fullName}</strong>?</p><p className="text-danger">Hành động này không thể hoàn tác!</p></div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowDelete(false)}>Hủy</button><button className="btn btn-danger" onClick={doDelete}>🗑️ Xác nhận xóa</button></div>
          </div>
        </div>
      )}
    </div>
  );
}
