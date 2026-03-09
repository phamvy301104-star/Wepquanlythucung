import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { staffApi, serviceApi, getImageUrl } from '../../../services/api';
import '../shared/admin.scss';

export default function AdminStaff() {
  const [items, setItems] = useState<any[]>([]);
  const [allServices, setAllServices] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [stats, setStats] = useState({ total: 0, active: 0, onLeave: 0, avgRating: 0 });

  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<any>(null);
  const [fd, setFd] = useState<any>({});
  const [imgFile, setImgFile] = useState<File | null>(null);
  const [imgPreview, setImgPreview] = useState('');
  const [saving, setSaving] = useState(false);
  const [selectedSvcs, setSelectedSvcs] = useState<string[]>([]);

  const [showDelete, setShowDelete] = useState(false);
  const [deleteItem, setDeleteItem] = useState<any>(null);

  useEffect(() => { load(); loadServices(); }, []);

  async function loadServices() {
    try {
      const r = await serviceApi.getAll({ limit: '200' });
      const d = r.data?.data || r.data;
      setAllServices(d?.services || d || []);
    } catch {}
  }

  async function load() {
    setLoading(true);
    try {
      const p: any = { limit: '200' };
      if (search) p.search = search;
      if (statusFilter) p.status = statusFilter;
      const r = await staffApi.getAll(p);
      const d = r.data?.data || r.data;
      const list = d?.staff || d || [];
      setItems(list);
      setStats({
        total: list.length,
        active: list.filter((s: any) => s.status === 'Active' || s.isActive).length,
        onLeave: list.filter((s: any) => s.status === 'OnLeave').length,
        avgRating: list.length ? (list.reduce((s: number, st: any) => s + (st.rating || 0), 0) / list.length) : 0,
      });
    } catch {}
    setLoading(false);
  }

  function openForm(item?: any) {
    setEditing(item || null);
    setFd(item ? {
      fullName: item.fullName || '', email: item.email || '', phoneNumber: item.phoneNumber || '',
      position: item.position || '', status: item.status || 'Active', hireDate: item.hireDate?.substring(0, 10) || '',
      bio: item.bio || '',
    } : { fullName: '', email: '', phoneNumber: '', position: '', status: 'Active', hireDate: '', bio: '' });
    setSelectedSvcs(item?.services?.map((s: any) => s._id || s) || []);
    setImgFile(null);
    setImgPreview(item?.avatarUrl ? getImageUrl(item.avatarUrl) : '');
    setShowForm(true);
  }

  function toggleSvc(id: string) {
    setSelectedSvcs(prev => prev.includes(id) ? prev.filter(s => s !== id) : [...prev, id]);
  }

  async function save() {
    if (!fd.fullName) { toast.error('Nhập tên nhân viên'); return; }
    setSaving(true);
    try {
      const formData = new FormData();
      Object.keys(fd).forEach(k => { if (fd[k] !== undefined && fd[k] !== '') formData.append(k, String(fd[k])); });
      selectedSvcs.forEach(s => formData.append('services', s));
      if (imgFile) formData.append('avatar', imgFile);
      if (editing) await staffApi.update(editing._id, formData);
      else await staffApi.create(formData);
      toast.success(editing ? 'Đã cập nhật!' : 'Đã tạo nhân viên!');
      setShowForm(false); load();
    } catch { toast.error('Có lỗi xảy ra'); }
    setSaving(false);
  }

  async function doDelete() {
    if (!deleteItem) return;
    try { await staffApi.delete(deleteItem._id); toast.success('Đã xóa!'); setShowDelete(false); load(); }
    catch { toast.error('Có lỗi xảy ra'); }
  }

  return (
    <div>
      <div className="page-header">
        <div><h4>👥 Quản lý nhân viên</h4>
          <ol className="breadcrumb"><li><Link to="/admin">Dashboard</Link></li><li className="active">Nhân viên</li></ol>
        </div>
        <button className="btn btn-gold" onClick={() => openForm()}>➕ Thêm nhân viên</button>
      </div>

      <div className="stats-row">
        <div className="stat-box"><div className="sb-icon bg-info">👥</div><div><div className="sb-num">{stats.total}</div><div className="sb-label">Tổng NV</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-success">✅</div><div><div className="sb-num">{stats.active}</div><div className="sb-label">Đang làm</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-warning">🏖️</div><div><div className="sb-num">{stats.onLeave}</div><div className="sb-label">Nghỉ phép</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-gold">⭐</div><div><div className="sb-num">{stats.avgRating.toFixed(1)}</div><div className="sb-label">Rating TB</div></div></div>
      </div>

      <div className="card">
        <div className="card-header">
          <div className="filter-row">
            <div className="search-box">
              <input className="form-control" value={search} onChange={e => setSearch(e.target.value)} placeholder="Tìm nhân viên..." onKeyDown={e => e.key === 'Enter' && load()} />
              <button className="btn btn-gold btn-sm" onClick={load}>🔍</button>
            </div>
            <select className="form-control fc-sm" value={statusFilter} onChange={e => { setStatusFilter(e.target.value); setTimeout(load, 0); }}>
              <option value="">Tất cả</option>
              <option value="Active">Đang làm</option>
              <option value="OnLeave">Nghỉ phép</option>
              <option value="Resigned">Đã nghỉ</option>
            </select>
          </div>
        </div>
        <div className="card-body p-0">
          <table className="adm-table">
            <thead><tr><th>#</th><th>Ảnh</th><th>Họ tên</th><th>Email</th><th>SĐT</th><th>Chức vụ</th><th>Trạng thái</th><th>Rating</th><th>Thao tác</th></tr></thead>
            <tbody>
              {items.map((s, i) => (
                <tr key={s._id}>
                  <td>{i + 1}</td>
                  <td><img src={getImageUrl(s.avatarUrl)} alt="" className="img-preview" style={{ borderRadius: '50%' }} /></td>
                  <td><strong>{s.fullName}</strong></td>
                  <td className="sub-txt">{s.email || '-'}</td>
                  <td>{s.phoneNumber || '-'}</td>
                  <td>{s.position || '-'}</td>
                  <td><span className={`os-badge os-${s.status === 'Active' ? 'completed' : s.status === 'OnLeave' ? 'pending' : 'cancelled'}`}>{s.status === 'Active' ? 'Đang làm' : s.status === 'OnLeave' ? 'Nghỉ phép' : 'Đã nghỉ'}</span></td>
                  <td>⭐ {(s.rating || 0).toFixed(1)}</td>
                  <td><div className="act-g">
                    <button className="ab ab-edit" onClick={() => openForm(s)}>✏️</button>
                    <button className="ab ab-del" onClick={() => { setDeleteItem(s); setShowDelete(true); }}>🗑️</button>
                  </div></td>
                </tr>
              ))}
              {!items.length && !loading && <tr><td colSpan={9} className="empty">Không có nhân viên nào</td></tr>}
              {loading && <tr><td colSpan={9} className="empty">Đang tải...</td></tr>}
            </tbody>
          </table>
        </div>
      </div>

      {showForm && (
        <div className="mo" onClick={() => setShowForm(false)}>
          <div className="md md-lg" onClick={e => e.stopPropagation()}>
            <div className="mh bg-g"><h5>{editing ? '✏️ Sửa' : '➕ Thêm'} nhân viên</h5><button className="mx" onClick={() => setShowForm(false)}>&times;</button></div>
            <div className="mb-modal">
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>Họ tên <span className="req">*</span></label><input className="form-control" value={fd.fullName} onChange={e => setFd({ ...fd, fullName: e.target.value })} /></div></div>
                <div className="adm-col-6"><div className="fg"><label>Email</label><input className="form-control" value={fd.email} onChange={e => setFd({ ...fd, email: e.target.value })} /></div></div>
              </div>
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>SĐT</label><input className="form-control" value={fd.phoneNumber} onChange={e => setFd({ ...fd, phoneNumber: e.target.value })} /></div></div>
                <div className="adm-col-6"><div className="fg"><label>Chức vụ</label>
                  <select className="form-control" value={fd.position} onChange={e => setFd({ ...fd, position: e.target.value })}>
                    <option value="">-- Chọn chức vụ --</option>
                    <option value="PetGroomer">Pet Groomer</option>
                    <option value="Veterinarian">Veterinarian</option>
                    <option value="Stylist">Stylist</option>
                    <option value="Barber">Barber</option>
                    <option value="Manager">Manager</option>
                    <option value="Trainee">Trainee</option>
                  </select>
                </div></div>
              </div>
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>Trạng thái</label>
                  <select className="form-control" value={fd.status} onChange={e => setFd({ ...fd, status: e.target.value })}>
                    <option value="Active">Đang làm</option><option value="OnLeave">Nghỉ phép</option><option value="Resigned">Đã nghỉ</option>
                  </select>
                </div></div>
                <div className="adm-col-6"><div className="fg"><label>Ngày bắt đầu</label><input type="date" className="form-control" value={fd.hireDate} onChange={e => setFd({ ...fd, hireDate: e.target.value })} /></div></div>
              </div>
              <div className="fg"><label>Tiểu sử</label><textarea className="form-control" rows={2} value={fd.bio} onChange={e => setFd({ ...fd, bio: e.target.value })} /></div>
              <div className="fg">
                <label>Dịch vụ phụ trách</label>
                <div style={{ maxHeight: 200, overflowY: 'auto', border: '1px solid #dee2e6', borderRadius: 6, padding: 8 }}>
                  {allServices.map(sv => (
                    <label key={sv._id} style={{ display: 'flex', alignItems: 'center', gap: 6, padding: '4px 0', cursor: 'pointer', fontSize: '0.85rem' }}>
                      <input type="checkbox" checked={selectedSvcs.includes(sv._id)} onChange={() => toggleSvc(sv._id)} />
                      {sv.name}
                    </label>
                  ))}
                </div>
              </div>
              <div className="fg"><label>Ảnh đại diện</label><input type="file" accept="image/*" onChange={e => { const f = e.target.files?.[0]; if (f) { setImgFile(f); setImgPreview(URL.createObjectURL(f)); } }} />
                {imgPreview && <img src={imgPreview} alt="" className="img-preview-lg" style={{ marginTop: 8, borderRadius: '50%' }} />}
              </div>
            </div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowForm(false)}>Hủy</button><button className="btn btn-gold" onClick={save} disabled={saving}>{saving ? 'Đang lưu...' : '💾 Lưu'}</button></div>
          </div>
        </div>
      )}

      {showDelete && (
        <div className="mo" onClick={() => setShowDelete(false)}>
          <div className="md" onClick={e => e.stopPropagation()}>
            <div className="mh bg-r"><h5>🗑️ Xóa nhân viên</h5><button className="mx" onClick={() => setShowDelete(false)}>&times;</button></div>
            <div className="mb-modal"><p>Bạn có chắc muốn xóa <strong>{deleteItem?.fullName}</strong>?</p><p className="text-danger">Hành động này không thể hoàn tác!</p></div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowDelete(false)}>Hủy</button><button className="btn btn-danger" onClick={doDelete}>🗑️ Xác nhận xóa</button></div>
          </div>
        </div>
      )}
    </div>
  );
}
