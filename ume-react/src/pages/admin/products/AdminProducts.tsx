import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { productApi, categoryApi, brandApi } from '../../../services/api';
import { getImageUrl } from '../../../services/api';
import '../shared/admin.scss';

const fmt = (n: number) => new Intl.NumberFormat('vi-VN').format(n);

export default function AdminProducts() {
  const [items, setItems] = useState<any[]>([]);
  const [categories, setCategories] = useState<any[]>([]);
  const [brands, setBrands] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState('');
  const [catFilter, setCatFilter] = useState('');
  const [brandFilter, setBrandFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  // Modal states
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<any>(null);
  const [fd, setFd] = useState<any>({});
  const [imgFile, setImgFile] = useState<File | null>(null);
  const [imgPreview, setImgPreview] = useState('');
  const [saving, setSaving] = useState(false);

  const [showDelete, setShowDelete] = useState(false);
  const [deleteItem, setDeleteItem] = useState<any>(null);

  const [showStock, setShowStock] = useState(false);
  const [stockItem, setStockItem] = useState<any>(null);
  const [stockVal, setStockVal] = useState(0);

  useEffect(() => { load(); loadRefs(); }, [page]);

  async function loadRefs() {
    try { const r = await categoryApi.getAll({ limit: '200' }); setCategories(r.data?.data?.categories || r.data?.data || []); } catch {}
    try { const r = await brandApi.getAll({ limit: '200' }); setBrands(r.data?.data?.brands || r.data?.data || []); } catch {}
  }

  async function load() {
    setLoading(true);
    try {
      const p: any = { page: String(page), limit: '20' };
      if (search) p.search = search;
      if (catFilter) p.category = catFilter;
      if (brandFilter) p.brand = brandFilter;
      if (statusFilter) p.isActive = statusFilter;
      const r = await productApi.getAll(p);
      const d = r.data?.data || r.data;
      setItems(d?.products || d || []);
      setTotalPages(d?.totalPages || 1);
    } catch { /* ignore */ }
    setLoading(false);
  }

  function openForm(item?: any) {
    setEditing(item || null);
    setFd(item ? {
      name: item.name || '', sku: item.sku || '', price: item.price || 0, originalPrice: item.originalPrice || 0,
      stock: item.stock || 0, category: item.category?._id || item.category || '',
      brand: item.brand?._id || item.brand || '', description: item.description || '',
      isActive: item.isActive !== false, isFeatured: item.isFeatured || false,
    } : { name: '', sku: '', price: 0, originalPrice: 0, stock: 0, category: '', brand: '', description: '', isActive: true, isFeatured: false });
    setImgFile(null);
    setImgPreview(item?.imageUrl ? getImageUrl(item.imageUrl) : '');
    setShowForm(true);
  }

  async function save() {
    if (!fd.name) { toast.error('Nhập tên sản phẩm'); return; }
    setSaving(true);
    try {
      const formData = new FormData();
      Object.keys(fd).forEach(k => { if (fd[k] !== undefined && fd[k] !== '') formData.append(k, String(fd[k])); });
      if (imgFile) formData.append('image', imgFile);
      if (editing) await productApi.update(editing._id, formData);
      else await productApi.create(formData);
      toast.success(editing ? 'Đã cập nhật!' : 'Đã tạo sản phẩm!');
      setShowForm(false);
      load();
    } catch { toast.error('Có lỗi xảy ra'); }
    setSaving(false);
  }

  async function toggleActive(item: any) {
    try {
      const formData = new FormData();
      formData.append('isActive', String(!item.isActive));
      await productApi.update(item._id, formData);
      load();
    } catch {}
  }

  function openStock(item: any) { setStockItem(item); setStockVal(item.stock || 0); setShowStock(true); }
  async function doStock() {
    if (!stockItem) return;
    try {
      const formData = new FormData();
      formData.append('stock', String(stockVal));
      await productApi.update(stockItem._id, formData);
      toast.success('Đã cập nhật tồn kho!');
      setShowStock(false);
      load();
    } catch { toast.error('Có lỗi xảy ra'); }
  }

  function confirmDelete(item: any) { setDeleteItem(item); setShowDelete(true); }
  async function doDelete() {
    if (!deleteItem) return;
    try { await productApi.delete(deleteItem._id); toast.success('Đã xóa!'); setShowDelete(false); load(); }
    catch { toast.error('Có lỗi xảy ra'); }
  }

  function handleImgChange(e: React.ChangeEvent<HTMLInputElement>) {
    const f = e.target.files?.[0];
    if (f) { setImgFile(f); setImgPreview(URL.createObjectURL(f)); }
  }

  return (
    <div>
      <div className="page-header">
        <div>
          <h4>📦 Quản lý sản phẩm</h4>
          <ol className="breadcrumb"><li><Link to="/admin">Dashboard</Link></li><li className="active">Sản phẩm</li></ol>
        </div>
        <button className="btn btn-gold" onClick={() => openForm()}>➕ Thêm sản phẩm</button>
      </div>

      <div className="card">
        <div className="card-header">
          <div className="filter-row">
            <div className="search-box">
              <input className="form-control" value={search} onChange={e => setSearch(e.target.value)} placeholder="Tìm sản phẩm..." onKeyDown={e => e.key === 'Enter' && load()} />
              <button className="btn btn-gold btn-sm" onClick={() => load()}>🔍</button>
            </div>
            <select className="form-control fc-sm" value={catFilter} onChange={e => { setCatFilter(e.target.value); setPage(1); setTimeout(load, 0); }}>
              <option value="">Tất cả danh mục</option>
              {categories.map(c => <option key={c._id} value={c._id}>{c.name}</option>)}
            </select>
            <select className="form-control fc-sm" value={brandFilter} onChange={e => { setBrandFilter(e.target.value); setPage(1); setTimeout(load, 0); }}>
              <option value="">Tất cả thương hiệu</option>
              {brands.map(b => <option key={b._id} value={b._id}>{b.name}</option>)}
            </select>
            <select className="form-control fc-sm" value={statusFilter} onChange={e => { setStatusFilter(e.target.value); setPage(1); setTimeout(load, 0); }}>
              <option value="">Tất cả trạng thái</option>
              <option value="true">Đang bán</option>
              <option value="false">Ngừng bán</option>
            </select>
          </div>
        </div>
        <div className="card-body p-0">
          <table className="adm-table">
            <thead>
              <tr><th>#</th><th>Ảnh</th><th>SKU</th><th>Tên</th><th>Danh mục</th><th>Giá</th><th>Kho</th><th>Đã bán</th><th>Active</th><th>Thao tác</th></tr>
            </thead>
            <tbody>
              {items.map((p, i) => (
                <tr key={p._id}>
                  <td>{(page - 1) * 20 + i + 1}</td>
                  <td><img src={getImageUrl(p.imageUrl)} alt="" className="img-preview" /></td>
                  <td>{p.sku || '-'}</td>
                  <td><strong>{p.name}</strong></td>
                  <td>{p.category?.name || '-'}</td>
                  <td className="text-gold fw-b">{fmt(p.price || 0)}đ</td>
                  <td>
                    <span style={{ cursor: 'pointer', textDecoration: 'underline' }} onClick={() => openStock(p)}>
                      {p.stock || 0}
                    </span>
                  </td>
                  <td>{p.soldCount || 0}</td>
                  <td>
                    <label className="toggle-switch">
                      <input type="checkbox" checked={p.isActive !== false} onChange={() => toggleActive(p)} />
                      <span className="slider" />
                    </label>
                  </td>
                  <td>
                    <div className="act-g">
                      <button className="ab ab-edit" onClick={() => openForm(p)} title="Sửa">✏️</button>
                      <button className="ab ab-del" onClick={() => confirmDelete(p)} title="Xóa">🗑️</button>
                    </div>
                  </td>
                </tr>
              ))}
              {!items.length && !loading && <tr><td colSpan={10} className="empty">Không có sản phẩm nào</td></tr>}
              {loading && <tr><td colSpan={10} className="empty">Đang tải...</td></tr>}
            </tbody>
          </table>
          {totalPages > 1 && (
            <div className="pagination">
              <button className="page-btn" disabled={page <= 1} onClick={() => setPage(page - 1)}>‹</button>
              {Array.from({ length: totalPages }, (_, i) => i + 1).slice(Math.max(0, page - 3), page + 2).map(p => (
                <button key={p} className={`page-btn${p === page ? ' active' : ''}`} onClick={() => setPage(p)}>{p}</button>
              ))}
              <button className="page-btn" disabled={page >= totalPages} onClick={() => setPage(page + 1)}>›</button>
            </div>
          )}
        </div>
      </div>

      {/* Create/Edit Modal */}
      {showForm && (
        <div className="mo" onClick={() => setShowForm(false)}>
          <div className="md md-lg" onClick={e => e.stopPropagation()}>
            <div className="mh bg-g"><h5>{editing ? '✏️ Sửa sản phẩm' : '➕ Thêm sản phẩm'}</h5><button className="mx" onClick={() => setShowForm(false)}>&times;</button></div>
            <div className="mb-modal">
              <div className="row">
                <div className="adm-col-6">
                  <div className="fg"><label>Tên sản phẩm <span className="req">*</span></label><input className="form-control" value={fd.name} onChange={e => setFd({ ...fd, name: e.target.value })} /></div>
                </div>
                <div className="adm-col-6">
                  <div className="fg"><label>SKU</label><input className="form-control" value={fd.sku} onChange={e => setFd({ ...fd, sku: e.target.value })} /></div>
                </div>
              </div>
              <div className="row">
                <div className="adm-col-6">
                  <div className="fg"><label>Giá bán <span className="req">*</span></label><input type="number" className="form-control" value={fd.price} onChange={e => setFd({ ...fd, price: Number(e.target.value) })} /></div>
                </div>
                <div className="adm-col-6">
                  <div className="fg"><label>Giá gốc</label><input type="number" className="form-control" value={fd.originalPrice} onChange={e => setFd({ ...fd, originalPrice: Number(e.target.value) })} /></div>
                </div>
              </div>
              <div className="row">
                <div className="adm-col-6">
                  <div className="fg"><label>Tồn kho</label><input type="number" className="form-control" value={fd.stock} onChange={e => setFd({ ...fd, stock: Number(e.target.value) })} /></div>
                </div>
                <div className="adm-col-6">
                  <div className="fg"><label>Danh mục</label>
                    <select className="form-control" value={fd.category} onChange={e => setFd({ ...fd, category: e.target.value })}>
                      <option value="">-- Chọn --</option>
                      {categories.map(c => <option key={c._id} value={c._id}>{c.name}</option>)}
                    </select>
                  </div>
                </div>
              </div>
              <div className="row">
                <div className="adm-col-6">
                  <div className="fg"><label>Thương hiệu</label>
                    <select className="form-control" value={fd.brand} onChange={e => setFd({ ...fd, brand: e.target.value })}>
                      <option value="">-- Chọn --</option>
                      {brands.map(b => <option key={b._id} value={b._id}>{b.name}</option>)}
                    </select>
                  </div>
                </div>
                <div className="adm-col-6">
                  <div className="fg" style={{ display: 'flex', gap: 20, paddingTop: 24 }}>
                    <label><input type="checkbox" checked={fd.isActive} onChange={e => setFd({ ...fd, isActive: e.target.checked })} /> Đang bán</label>
                    <label><input type="checkbox" checked={fd.isFeatured} onChange={e => setFd({ ...fd, isFeatured: e.target.checked })} /> Nổi bật</label>
                  </div>
                </div>
              </div>
              <div className="fg"><label>Mô tả</label><textarea className="form-control" rows={3} value={fd.description} onChange={e => setFd({ ...fd, description: e.target.value })} /></div>
              <div className="fg">
                <label>Hình ảnh</label>
                <input type="file" accept="image/*" onChange={handleImgChange} />
                {imgPreview && <img src={imgPreview} alt="Preview" className="img-preview-lg" style={{ marginTop: 8 }} />}
              </div>
            </div>
            <div className="mf">
              <button className="btn btn-sec" onClick={() => setShowForm(false)}>Hủy</button>
              <button className="btn btn-gold" onClick={save} disabled={saving}>{saving ? 'Đang lưu...' : '💾 Lưu'}</button>
            </div>
          </div>
        </div>
      )}

      {/* Stock Modal */}
      {showStock && (
        <div className="mo" onClick={() => setShowStock(false)}>
          <div className="md" onClick={e => e.stopPropagation()}>
            <div className="mh bg-g"><h5>📦 Cập nhật tồn kho</h5><button className="mx" onClick={() => setShowStock(false)}>&times;</button></div>
            <div className="mb-modal">
              <p>Sản phẩm: <strong>{stockItem?.name}</strong></p>
              <div className="fg"><label>Số lượng tồn kho</label><input type="number" className="form-control" value={stockVal} onChange={e => setStockVal(Number(e.target.value))} /></div>
            </div>
            <div className="mf">
              <button className="btn btn-sec" onClick={() => setShowStock(false)}>Hủy</button>
              <button className="btn btn-gold" onClick={doStock}>💾 Cập nhật</button>
            </div>
          </div>
        </div>
      )}

      {/* Delete Modal */}
      {showDelete && (
        <div className="mo" onClick={() => setShowDelete(false)}>
          <div className="md" onClick={e => e.stopPropagation()}>
            <div className="mh bg-r"><h5>🗑️ Xóa sản phẩm</h5><button className="mx" onClick={() => setShowDelete(false)}>&times;</button></div>
            <div className="mb-modal">
              <p>Bạn có chắc muốn xóa sản phẩm <strong>{deleteItem?.name}</strong>?</p>
              <p className="text-danger">Hành động này không thể hoàn tác!</p>
            </div>
            <div className="mf">
              <button className="btn btn-sec" onClick={() => setShowDelete(false)}>Hủy</button>
              <button className="btn btn-danger" onClick={doDelete}>🗑️ Xác nhận xóa</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
