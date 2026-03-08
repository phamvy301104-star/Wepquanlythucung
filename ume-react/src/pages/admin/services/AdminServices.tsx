import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { serviceApi, getImageUrl } from '../../../services/api';
import '../shared/admin.scss';

const fmt = (n: number) => new Intl.NumberFormat('vi-VN').format(n);

export default function AdminServices() {
  const [items, setItems] = useState<any[]>([]);
  const [categories, setCategories] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState('');

  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<any>(null);
  const [fd, setFd] = useState<any>({});
  const [imgFile, setImgFile] = useState<File | null>(null);
  const [imgPreview, setImgPreview] = useState('');
  const [saving, setSaving] = useState(false);

  const [showDelete, setShowDelete] = useState(false);
  const [deleteItem, setDeleteItem] = useState<any>(null);

  useEffect(() => { load(); loadCats(); }, []);

  async function loadCats() {
    try {
      const r = await serviceApi.getServiceCategories();
      const d = r.data?.data || r.data;
      setCategories(d?.categories || d || []);
    } catch {
      try {
        const r = await serviceApi.getCategories();
        const d = r.data?.data || r.data;
        setCategories(d || []);
      } catch {}
    }
  }

  async function load() {
    setLoading(true);
    try {
      const r = await serviceApi.getAll({ limit: '200', search: search || undefined });
      const d = r.data?.data || r.data;
      setItems(d?.services || d || []);
    } catch {}
    setLoading(false);
  }

  function openForm(item?: any) {
    setEditing(item || null);
    setFd(item ? {
      name: item.name || '', price: item.price || 0, originalPrice: item.originalPrice || 0,
      durationMinutes: item.durationMinutes || 30, category: item.category?._id || item.category || '',
      description: item.description || '', isActive: item.isActive !== false, isFeatured: item.isFeatured || false,
    } : { name: '', price: 0, originalPrice: 0, durationMinutes: 30, category: '', description: '', isActive: true, isFeatured: false });
    setImgFile(null);
    setImgPreview(item?.imageUrl ? getImageUrl(item.imageUrl) : '');
    setShowForm(true);
  }

  async function save() {
    if (!fd.name) { toast.error('Nhập tên dịch vụ'); return; }
    setSaving(true);
    try {
      const formData = new FormData();
      Object.keys(fd).forEach(k => { if (fd[k] !== undefined && fd[k] !== '') formData.append(k, String(fd[k])); });
      if (imgFile) formData.append('image', imgFile);
      if (editing) await serviceApi.update(editing._id, formData);
      else await serviceApi.create(formData);
      toast.success(editing ? 'Đã cập nhật!' : 'Đã tạo dịch vụ!');
      setShowForm(false); load();
    } catch { toast.error('Có lỗi xảy ra'); }
    setSaving(false);
  }

  async function toggleActive(item: any) {
    try {
      const formData = new FormData();
      formData.append('isActive', String(!item.isActive));
      await serviceApi.update(item._id, formData);
      load();
    } catch {}
  }

  async function doDelete() {
    if (!deleteItem) return;
    try { await serviceApi.delete(deleteItem._id); toast.success('Đã xóa!'); setShowDelete(false); load(); }
    catch { toast.error('Có lỗi xảy ra'); }
  }

  return (
    <div>
      <div className="page-header">
        <div><h4>✂️ Quản lý dịch vụ</h4>
          <ol className="breadcrumb"><li><Link to="/admin">Dashboard</Link></li><li className="active">Dịch vụ</li></ol>
        </div>
        <button className="btn btn-gold" onClick={() => openForm()}>➕ Thêm dịch vụ</button>
      </div>

      <div className="card">
        <div className="card-header">
          <div className="search-box">
            <input className="form-control" value={search} onChange={e => setSearch(e.target.value)} placeholder="Tìm dịch vụ..." onKeyDown={e => e.key === 'Enter' && load()} />
            <button className="btn btn-gold btn-sm" onClick={load}>🔍</button>
          </div>
        </div>
        <div className="card-body p-0">
          <table className="adm-table">
            <thead><tr><th>#</th><th>Ảnh</th><th>Tên</th><th>Danh mục</th><th>Giá</th><th>Thời gian</th><th>Nổi bật</th><th>Active</th><th>Thao tác</th></tr></thead>
            <tbody>
              {items.map((s, i) => (
                <tr key={s._id}>
                  <td>{i + 1}</td>
                  <td><img src={getImageUrl(s.imageUrl)} alt="" className="img-preview" /></td>
                  <td><strong>{s.name}</strong></td>
                  <td>{s.category?.name || '-'}</td>
                  <td className="text-gold fw-b">{fmt(s.price || 0)}đ</td>
                  <td>{s.durationMinutes || 30} phút</td>
                  <td>{s.isFeatured ? '⭐' : '-'}</td>
                  <td><label className="toggle-switch"><input type="checkbox" checked={s.isActive !== false} onChange={() => toggleActive(s)} /><span className="slider" /></label></td>
                  <td><div className="act-g">
                    <button className="ab ab-edit" onClick={() => openForm(s)}>✏️</button>
                    <button className="ab ab-del" onClick={() => { setDeleteItem(s); setShowDelete(true); }}>🗑️</button>
                  </div></td>
                </tr>
              ))}
              {!items.length && !loading && <tr><td colSpan={9} className="empty">Không có dịch vụ nào</td></tr>}
              {loading && <tr><td colSpan={9} className="empty">Đang tải...</td></tr>}
            </tbody>
          </table>
        </div>
      </div>

      {showForm && (
        <div className="mo" onClick={() => setShowForm(false)}>
          <div className="md md-lg" onClick={e => e.stopPropagation()}>
            <div className="mh bg-g"><h5>{editing ? '✏️ Sửa' : '➕ Thêm'} dịch vụ</h5><button className="mx" onClick={() => setShowForm(false)}>&times;</button></div>
            <div className="mb-modal">
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>Tên dịch vụ <span className="req">*</span></label><input className="form-control" value={fd.name} onChange={e => setFd({ ...fd, name: e.target.value })} /></div></div>
                <div className="adm-col-6"><div className="fg"><label>Danh mục</label>
                  <select className="form-control" value={fd.category} onChange={e => setFd({ ...fd, category: e.target.value })}>
                    <option value="">-- Chọn --</option>
                    {categories.map(c => <option key={c._id} value={c._id}>{c.name}</option>)}
                  </select>
                </div></div>
              </div>
              <div className="row">
                <div className="adm-col-4"><div className="fg"><label>Giá bán</label><input type="number" className="form-control" value={fd.price} onChange={e => setFd({ ...fd, price: Number(e.target.value) })} /></div></div>
                <div className="adm-col-4"><div className="fg"><label>Giá gốc</label><input type="number" className="form-control" value={fd.originalPrice} onChange={e => setFd({ ...fd, originalPrice: Number(e.target.value) })} /></div></div>
                <div className="adm-col-4"><div className="fg"><label>Thời gian (phút)</label><input type="number" className="form-control" value={fd.durationMinutes} onChange={e => setFd({ ...fd, durationMinutes: Number(e.target.value) })} /></div></div>
              </div>
              <div className="fg"><label>Mô tả</label><textarea className="form-control" rows={3} value={fd.description} onChange={e => setFd({ ...fd, description: e.target.value })} /></div>
              <div className="fg" style={{ display: 'flex', gap: 20 }}>
                <label><input type="checkbox" checked={fd.isActive} onChange={e => setFd({ ...fd, isActive: e.target.checked })} /> Đang hoạt động</label>
                <label><input type="checkbox" checked={fd.isFeatured} onChange={e => setFd({ ...fd, isFeatured: e.target.checked })} /> Nổi bật</label>
              </div>
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
            <div className="mh bg-r"><h5>🗑️ Xóa dịch vụ</h5><button className="mx" onClick={() => setShowDelete(false)}>&times;</button></div>
            <div className="mb-modal"><p>Bạn có chắc muốn xóa <strong>{deleteItem?.name}</strong>?</p><p className="text-danger">Hành động này không thể hoàn tác!</p></div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowDelete(false)}>Hủy</button><button className="btn btn-danger" onClick={doDelete}>🗑️ Xác nhận xóa</button></div>
          </div>
        </div>
      )}
    </div>
  );
}
