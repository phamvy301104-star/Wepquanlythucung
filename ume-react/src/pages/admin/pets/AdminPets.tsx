import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { petApi, getImageUrl } from '../../../services/api';
import '../shared/admin.scss';

function fmtN(n: number) { return new Intl.NumberFormat('vi-VN').format(n); }

export default function AdminPets() {
  const [items, setItems] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [listingFilter, setListingFilter] = useState('');
  const [stats, setStats] = useState({ total: 0, dogs: 0, cats: 0, forSale: 0 });

  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<any>(null);
  const [fd, setFd] = useState<any>({});
  const [imgFile, setImgFile] = useState<File | null>(null);
  const [imgPreview, setImgPreview] = useState('');
  const [saving, setSaving] = useState(false);

  const [detail, setDetail] = useState<any>(null);
  const [showDetail, setShowDetail] = useState(false);

  const [showDelete, setShowDelete] = useState(false);
  const [deleteItem, setDeleteItem] = useState<any>(null);

  useEffect(() => { load(); }, []);

  async function load() {
    setLoading(true);
    try {
      const p: any = { limit: '200' };
      if (search) p.search = search;
      if (typeFilter) p.type = typeFilter;
      if (listingFilter) p.isForSale = listingFilter;
      const r = await petApi.getAll(p);
      const d = r.data?.data || r.data;
      const list = d?.pets || d || [];
      setItems(list);
      setStats({
        total: list.length,
        dogs: list.filter((p: any) => p.type === 'Dog' || p.type === 'Chó').length,
        cats: list.filter((p: any) => p.type === 'Cat' || p.type === 'Mèo').length,
        forSale: list.filter((p: any) => p.isForSale || p.price > 0).length,
      });
    } catch {}
    setLoading(false);
  }

  function openForm(item?: any) {
    setEditing(item || null);
    setFd(item ? {
      name: item.name || '', type: item.type || 'Dog', breed: item.breed || '',
      age: item.age || '', gender: item.gender || 'Male', weight: item.weight || '',
      color: item.color || '', description: item.description || '', price: item.price || 0,
      isForSale: item.isForSale || false, isActive: item.isActive !== false,
      healthStatus: item.healthStatus || 'Healthy', vaccinated: item.vaccinated || false,
    } : { name: '', type: 'Dog', breed: '', age: '', gender: 'Male', weight: '', color: '', description: '', price: 0, isForSale: false, isActive: true, healthStatus: 'Healthy', vaccinated: false });
    setImgFile(null);
    setImgPreview(item?.imageUrl ? getImageUrl(item.imageUrl) : '');
    setShowForm(true);
  }

  async function save() {
    if (!fd.name) { toast.error('Nhập tên thú cưng'); return; }
    setSaving(true);
    try {
      const formData = new FormData();
      Object.keys(fd).forEach(k => { if (fd[k] !== undefined && fd[k] !== '') formData.append(k, String(fd[k])); });
      if (imgFile) formData.append('image', imgFile);
      if (editing) await petApi.update(editing._id, formData);
      else await petApi.create(formData);
      toast.success(editing ? 'Đã cập nhật!' : 'Đã thêm thú cưng!');
      setShowForm(false); load();
    } catch { toast.error('Có lỗi xảy ra'); }
    setSaving(false);
  }

  async function toggleActive(item: any) {
    try {
      const formData = new FormData();
      formData.append('isActive', String(!item.isActive));
      await petApi.update(item._id, formData);
      toast.success(item.isActive ? 'Đã ẩn' : 'Đã hiển thị');
      load();
    } catch { toast.error('Có lỗi xảy ra'); }
  }

  async function doDelete() {
    if (!deleteItem) return;
    try { await petApi.delete(deleteItem._id); toast.success('Đã xóa!'); setShowDelete(false); load(); }
    catch { toast.error('Có lỗi xảy ra'); }
  }

  return (
    <div>
      <div className="page-header">
        <div><h4>🐾 Quản lý thú cưng</h4>
          <ol className="breadcrumb"><li><Link to="/admin">Dashboard</Link></li><li className="active">Thú cưng</li></ol>
        </div>
        <button className="btn btn-gold" onClick={() => openForm()}>➕ Thêm thú cưng</button>
      </div>

      <div className="stats-row">
        <div className="stat-box"><div className="sb-icon bg-info">🐾</div><div><div className="sb-num">{stats.total}</div><div className="sb-label">Tổng cộng</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-warning">🐕</div><div><div className="sb-num">{stats.dogs}</div><div className="sb-label">Chó</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-success">🐈</div><div><div className="sb-num">{stats.cats}</div><div className="sb-label">Mèo</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-gold">💰</div><div><div className="sb-num">{stats.forSale}</div><div className="sb-label">Đang bán</div></div></div>
      </div>

      <div className="card">
        <div className="card-header">
          <div className="filter-row">
            <div className="search-box">
              <input className="form-control" value={search} onChange={e => setSearch(e.target.value)} placeholder="Tìm thú cưng..." onKeyDown={e => e.key === 'Enter' && load()} />
              <button className="btn btn-gold btn-sm" onClick={load}>🔍</button>
            </div>
            <select className="form-control fc-sm" value={typeFilter} onChange={e => { setTypeFilter(e.target.value); setTimeout(load, 0); }}>
              <option value="">Tất cả loại</option>
              <option value="Dog">Chó</option>
              <option value="Cat">Mèo</option>
            </select>
            <select className="form-control fc-sm" value={listingFilter} onChange={e => { setListingFilter(e.target.value); setTimeout(load, 0); }}>
              <option value="">Tất cả</option>
              <option value="true">Đang bán</option>
              <option value="false">Không bán</option>
            </select>
          </div>
        </div>
        <div className="card-body p-0">
          <table className="adm-table">
            <thead><tr><th>#</th><th>Ảnh</th><th>Tên</th><th>Loại</th><th>Giống</th><th>Tuổi</th><th>Giá</th><th>Sức khỏe</th><th>Hiển thị</th><th>Thao tác</th></tr></thead>
            <tbody>
              {items.map((p, i) => (
                <tr key={p._id} className="clickable-row" onClick={() => { setDetail(p); setShowDetail(true); }}>
                  <td>{i + 1}</td>
                  <td><img src={getImageUrl(p.imageUrl)} alt="" className="img-preview" /></td>
                  <td><strong>{p.name}</strong>{p.gender && <div className="sub-txt">{p.gender === 'Male' ? '♂ Đực' : '♀ Cái'}</div>}</td>
                  <td>{p.type === 'Dog' ? '🐕 Chó' : p.type === 'Cat' ? '🐈 Mèo' : p.type}</td>
                  <td>{p.breed || '-'}</td>
                  <td>{p.age || '-'}</td>
                  <td className="fw-bold">{p.isForSale ? `${fmtN(p.price || 0)}đ` : '-'}</td>
                  <td><span className={`os-badge ${p.healthStatus === 'Healthy' ? 'os-completed' : 'os-pending'}`}>{p.healthStatus || 'N/A'}{p.vaccinated && ' 💉'}</span></td>
                  <td onClick={e => e.stopPropagation()}>
                    <label className="toggle-switch"><input type="checkbox" checked={p.isActive !== false} onChange={() => toggleActive(p)} /><span className="toggle-slider"></span></label>
                  </td>
                  <td onClick={e => e.stopPropagation()}><div className="act-g">
                    <button className="ab ab-edit" onClick={() => openForm(p)}>✏️</button>
                    <button className="ab ab-del" onClick={() => { setDeleteItem(p); setShowDelete(true); }}>🗑️</button>
                  </div></td>
                </tr>
              ))}
              {!items.length && !loading && <tr><td colSpan={10} className="empty">Không có thú cưng nào</td></tr>}
              {loading && <tr><td colSpan={10} className="empty">Đang tải...</td></tr>}
            </tbody>
          </table>
        </div>
      </div>

      {/* Detail Modal */}
      {showDetail && detail && (
        <div className="mo" onClick={() => setShowDetail(false)}>
          <div className="md md-lg" onClick={e => e.stopPropagation()}>
            <div className="mh bg-g"><h5>🐾 {detail.name}</h5><button className="mx" onClick={() => setShowDetail(false)}>&times;</button></div>
            <div className="mb-modal">
              <div className="row">
                <div className="adm-col-4">{detail.imageUrl && <img src={getImageUrl(detail.imageUrl)} alt="" style={{ width: '100%', borderRadius: 8 }} />}</div>
                <div className="adm-col-8">
                  <p><strong>Loại:</strong> {detail.type === 'Dog' ? '🐕 Chó' : '🐈 Mèo'}</p>
                  <p><strong>Giống:</strong> {detail.breed || '-'}</p>
                  <p><strong>Tuổi:</strong> {detail.age || '-'}</p>
                  <p><strong>Giới tính:</strong> {detail.gender === 'Male' ? '♂ Đực' : '♀ Cái'}</p>
                  <p><strong>Cân nặng:</strong> {detail.weight ? `${detail.weight} kg` : '-'}</p>
                  <p><strong>Màu sắc:</strong> {detail.color || '-'}</p>
                  <p><strong>Sức khỏe:</strong> <span className={`os-badge ${detail.healthStatus === 'Healthy' ? 'os-completed' : 'os-pending'}`}>{detail.healthStatus}</span> {detail.vaccinated && '💉 Đã tiêm phòng'}</p>
                  {detail.isForSale && <p><strong>Giá:</strong> <span className="fw-bold" style={{ color: '#667eea' }}>{fmtN(detail.price || 0)}đ</span></p>}
                  {detail.description && <p><strong>Mô tả:</strong> {detail.description}</p>}
                </div>
              </div>
            </div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowDetail(false)}>Đóng</button><button className="btn btn-gold" onClick={() => { setShowDetail(false); openForm(detail); }}>✏️ Chỉnh sửa</button></div>
          </div>
        </div>
      )}

      {/* Create/Edit Form */}
      {showForm && (
        <div className="mo" onClick={() => setShowForm(false)}>
          <div className="md md-lg" onClick={e => e.stopPropagation()}>
            <div className="mh bg-g"><h5>{editing ? '✏️ Sửa' : '➕ Thêm'} thú cưng</h5><button className="mx" onClick={() => setShowForm(false)}>&times;</button></div>
            <div className="mb-modal">
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>Tên <span className="req">*</span></label><input className="form-control" value={fd.name} onChange={e => setFd({ ...fd, name: e.target.value })} /></div></div>
                <div className="adm-col-6"><div className="fg"><label>Loại</label>
                  <select className="form-control" value={fd.type} onChange={e => setFd({ ...fd, type: e.target.value })}>
                    <option value="Dog">Chó</option><option value="Cat">Mèo</option>
                  </select>
                </div></div>
              </div>
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>Giống</label><input className="form-control" value={fd.breed} onChange={e => setFd({ ...fd, breed: e.target.value })} /></div></div>
                <div className="adm-col-6"><div className="fg"><label>Tuổi</label><input className="form-control" value={fd.age} onChange={e => setFd({ ...fd, age: e.target.value })} placeholder="VD: 2 tháng" /></div></div>
              </div>
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>Giới tính</label>
                  <select className="form-control" value={fd.gender} onChange={e => setFd({ ...fd, gender: e.target.value })}>
                    <option value="Male">Đực</option><option value="Female">Cái</option>
                  </select>
                </div></div>
                <div className="adm-col-6"><div className="fg"><label>Cân nặng (kg)</label><input type="number" className="form-control" value={fd.weight} onChange={e => setFd({ ...fd, weight: e.target.value })} /></div></div>
              </div>
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>Màu sắc</label><input className="form-control" value={fd.color} onChange={e => setFd({ ...fd, color: e.target.value })} /></div></div>
                <div className="adm-col-6"><div className="fg"><label>Giá (nếu bán)</label><input type="number" className="form-control" value={fd.price} onChange={e => setFd({ ...fd, price: +e.target.value })} /></div></div>
              </div>
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>Sức khỏe</label>
                  <select className="form-control" value={fd.healthStatus} onChange={e => setFd({ ...fd, healthStatus: e.target.value })}>
                    <option value="Healthy">Khỏe mạnh</option><option value="Sick">Đang bệnh</option><option value="Recovering">Đang phục hồi</option>
                  </select>
                </div></div>
                <div className="adm-col-6" style={{ display: 'flex', gap: 16, alignItems: 'center', paddingTop: 28 }}>
                  <label style={{ display: 'flex', alignItems: 'center', gap: 6, cursor: 'pointer' }}>
                    <input type="checkbox" checked={fd.isForSale} onChange={e => setFd({ ...fd, isForSale: e.target.checked })} /> Đăng bán
                  </label>
                  <label style={{ display: 'flex', alignItems: 'center', gap: 6, cursor: 'pointer' }}>
                    <input type="checkbox" checked={fd.vaccinated} onChange={e => setFd({ ...fd, vaccinated: e.target.checked })} /> Đã tiêm phòng
                  </label>
                  <label style={{ display: 'flex', alignItems: 'center', gap: 6, cursor: 'pointer' }}>
                    <input type="checkbox" checked={fd.isActive} onChange={e => setFd({ ...fd, isActive: e.target.checked })} /> Hiển thị
                  </label>
                </div>
              </div>
              <div className="fg"><label>Mô tả</label><textarea className="form-control" rows={3} value={fd.description} onChange={e => setFd({ ...fd, description: e.target.value })} /></div>
              <div className="fg"><label>Hình ảnh</label><input type="file" accept="image/*" onChange={e => { const f = e.target.files?.[0]; if (f) { setImgFile(f); setImgPreview(URL.createObjectURL(f)); } }} />
                {imgPreview && <img src={imgPreview} alt="" className="img-preview-lg" style={{ marginTop: 8 }} />}
              </div>
            </div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowForm(false)}>Hủy</button><button className="btn btn-gold" onClick={save} disabled={saving}>{saving ? 'Đang lưu...' : '💾 Lưu'}</button></div>
          </div>
        </div>
      )}

      {showDelete && (
        <div className="mo" onClick={() => setShowDelete(false)}>
          <div className="md" onClick={e => e.stopPropagation()}>
            <div className="mh bg-r"><h5>🗑️ Xóa thú cưng</h5><button className="mx" onClick={() => setShowDelete(false)}>&times;</button></div>
            <div className="mb-modal"><p>Bạn có chắc muốn xóa <strong>{deleteItem?.name}</strong>?</p><p className="text-danger">Hành động này không thể hoàn tác!</p></div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowDelete(false)}>Hủy</button><button className="btn btn-danger" onClick={doDelete}>🗑️ Xác nhận xóa</button></div>
          </div>
        </div>
      )}
    </div>
  );
}
