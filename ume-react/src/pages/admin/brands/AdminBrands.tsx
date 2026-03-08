import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { brandApi, getImageUrl } from '../../../services/api';
import '../shared/admin.scss';

export default function AdminBrands() {
  const [items, setItems] = useState<any[]>([]);
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

  useEffect(() => { load(); }, []);

  async function load() {
    setLoading(true);
    try {
      const r = await brandApi.getAll({ limit: '200', search: search || undefined });
      const d = r.data?.data || r.data;
      setItems(d?.brands || d || []);
    } catch {}
    setLoading(false);
  }

  function openForm(item?: any) {
    setEditing(item || null);
    setFd(item ? {
      name: item.name || '', description: item.description || '',
      displayOrder: item.displayOrder || 0, isActive: item.isActive !== false,
    } : { name: '', description: '', displayOrder: 0, isActive: true });
    setImgFile(null);
    setImgPreview(item?.logoUrl ? getImageUrl(item.logoUrl) : '');
    setShowForm(true);
  }

  async function save() {
    if (!fd.name) { toast.error('Nhập tên thương hiệu'); return; }
    setSaving(true);
    try {
      const formData = new FormData();
      Object.keys(fd).forEach(k => { if (fd[k] !== undefined && fd[k] !== '') formData.append(k, String(fd[k])); });
      if (imgFile) formData.append('logo', imgFile);
      if (editing) await brandApi.update(editing._id, formData);
      else await brandApi.create(formData);
      toast.success(editing ? 'Đã cập nhật!' : 'Đã tạo thương hiệu!');
      setShowForm(false); load();
    } catch { toast.error('Có lỗi xảy ra'); }
    setSaving(false);
  }

  async function toggleActive(item: any) {
    try {
      const formData = new FormData();
      formData.append('isActive', String(!item.isActive));
      await brandApi.update(item._id, formData);
      load();
    } catch {}
  }

  async function doDelete() {
    if (!deleteItem) return;
    try { await brandApi.delete(deleteItem._id); toast.success('Đã xóa!'); setShowDelete(false); load(); }
    catch { toast.error('Có lỗi xảy ra'); }
  }

  return (
    <div>
      <div className="page-header">
        <div><h4>🏷️ Quản lý thương hiệu</h4>
          <ol className="breadcrumb"><li><Link to="/admin">Dashboard</Link></li><li className="active">Thương hiệu</li></ol>
        </div>
        <button className="btn btn-gold" onClick={() => openForm()}>➕ Thêm thương hiệu</button>
      </div>

      <div className="card">
        <div className="card-header">
          <div className="search-box">
            <input className="form-control" value={search} onChange={e => setSearch(e.target.value)} placeholder="Tìm thương hiệu..." onKeyDown={e => e.key === 'Enter' && load()} />
            <button className="btn btn-gold btn-sm" onClick={load}>🔍</button>
          </div>
        </div>
        <div className="card-body p-0">
          <table className="adm-table">
            <thead><tr><th>#</th><th>Logo</th><th>Tên</th><th>Mô tả</th><th>SP</th><th>Thứ tự</th><th>Active</th><th>Thao tác</th></tr></thead>
            <tbody>
              {items.map((b, i) => (
                <tr key={b._id}>
                  <td>{i + 1}</td>
                  <td><img src={getImageUrl(b.logoUrl)} alt="" className="img-preview" /></td>
                  <td><strong>{b.name}</strong></td>
                  <td className="sub-txt">{(b.description || '').substring(0, 60)}</td>
                  <td>{b.productCount || 0}</td>
                  <td>{b.displayOrder || 0}</td>
                  <td><label className="toggle-switch"><input type="checkbox" checked={b.isActive !== false} onChange={() => toggleActive(b)} /><span className="slider" /></label></td>
                  <td><div className="act-g">
                    <button className="ab ab-edit" onClick={() => openForm(b)}>✏️</button>
                    <button className="ab ab-del" onClick={() => { setDeleteItem(b); setShowDelete(true); }}>🗑️</button>
                  </div></td>
                </tr>
              ))}
              {!items.length && !loading && <tr><td colSpan={8} className="empty">Không có thương hiệu nào</td></tr>}
              {loading && <tr><td colSpan={8} className="empty">Đang tải...</td></tr>}
            </tbody>
          </table>
        </div>
      </div>

      {showForm && (
        <div className="mo" onClick={() => setShowForm(false)}>
          <div className="md" onClick={e => e.stopPropagation()}>
            <div className="mh bg-g"><h5>{editing ? '✏️ Sửa' : '➕ Thêm'} thương hiệu</h5><button className="mx" onClick={() => setShowForm(false)}>&times;</button></div>
            <div className="mb-modal">
              <div className="fg"><label>Tên <span className="req">*</span></label><input className="form-control" value={fd.name} onChange={e => setFd({ ...fd, name: e.target.value })} /></div>
              <div className="fg"><label>Mô tả</label><textarea className="form-control" rows={3} value={fd.description} onChange={e => setFd({ ...fd, description: e.target.value })} /></div>
              <div className="fg"><label>Thứ tự</label><input type="number" className="form-control" value={fd.displayOrder} onChange={e => setFd({ ...fd, displayOrder: Number(e.target.value) })} /></div>
              <div className="fg"><label><input type="checkbox" checked={fd.isActive} onChange={e => setFd({ ...fd, isActive: e.target.checked })} /> Hiển thị</label></div>
              <div className="fg"><label>Logo</label><input type="file" accept="image/*" onChange={e => { const f = e.target.files?.[0]; if (f) { setImgFile(f); setImgPreview(URL.createObjectURL(f)); } }} />
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
            <div className="mh bg-r"><h5>🗑️ Xóa thương hiệu</h5><button className="mx" onClick={() => setShowDelete(false)}>&times;</button></div>
            <div className="mb-modal"><p>Bạn có chắc muốn xóa <strong>{deleteItem?.name}</strong>?</p><p className="text-danger">Hành động này không thể hoàn tác!</p></div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowDelete(false)}>Hủy</button><button className="btn btn-danger" onClick={doDelete}>🗑️ Xác nhận xóa</button></div>
          </div>
        </div>
      )}
    </div>
  );
}
