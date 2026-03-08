import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { promotionApi } from '../../../services/api';
import '../shared/admin.scss';

function fmtN(n: number) { return new Intl.NumberFormat('vi-VN').format(n); }
function fmtD(d: string) { return d ? new Date(d).toLocaleDateString('vi-VN') : '-'; }

const TYPES = [
  { value: 'Percentage', label: 'Giảm %' },
  { value: 'Fixed', label: 'Giảm cố định' },
  { value: 'FreeShipping', label: 'Miễn phí ship' },
  { value: 'BuyXGetY', label: 'Mua X tặng Y' },
];

export default function AdminPromotions() {
  const [items, setItems] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState('');
  const [stats, setStats] = useState({ total: 0, active: 0, expired: 0, totalUsed: 0 });

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
      const r = await promotionApi.getAll(p);
      const d = r.data?.data || r.data;
      const list = d?.promotions || d || [];
      setItems(list);
      const now = new Date();
      setStats({
        total: list.length,
        active: list.filter((p: any) => p.isActive && new Date(p.endDate) > now).length,
        expired: list.filter((p: any) => new Date(p.endDate) <= now).length,
        totalUsed: list.reduce((s: number, p: any) => s + (p.usedCount || 0), 0),
      });
    } catch {}
    setLoading(false);
  }

  function isExpired(item: any) { return new Date(item.endDate) <= new Date(); }
  function isActive(item: any) { return item.isActive && !isExpired(item); }

  function openForm(item?: any) {
    setEditing(item || null);
    setFd(item ? {
      code: item.code || '', name: item.name || '', type: item.type || 'Percentage',
      value: item.value || 0, minOrder: item.minOrder || 0, maxDiscount: item.maxDiscount || 0,
      usageLimit: item.usageLimit || 0, startDate: item.startDate?.substring(0, 10) || '',
      endDate: item.endDate?.substring(0, 10) || '', description: item.description || '', isActive: item.isActive !== false,
    } : { code: '', name: '', type: 'Percentage', value: 0, minOrder: 0, maxDiscount: 0, usageLimit: 0, startDate: '', endDate: '', description: '', isActive: true });
    setShowForm(true);
  }

  async function save() {
    if (!fd.code || !fd.name) { toast.error('Nhập đủ thông tin bắt buộc'); return; }
    setSaving(true);
    try {
      if (editing) await promotionApi.update(editing._id, fd);
      else await promotionApi.create(fd);
      toast.success(editing ? 'Đã cập nhật!' : 'Đã tạo khuyến mãi!');
      setShowForm(false); load();
    } catch (e: any) { toast.error(e.response?.data?.message || 'Có lỗi xảy ra'); }
    setSaving(false);
  }

  async function toggleActive(item: any) {
    try {
      await promotionApi.update(item._id, { isActive: !item.isActive });
      toast.success(item.isActive ? 'Đã tắt' : 'Đã bật');
      load();
    } catch { toast.error('Có lỗi xảy ra'); }
  }

  async function doDelete() {
    if (!deleteItem) return;
    try { await promotionApi.delete(deleteItem._id); toast.success('Đã xóa!'); setShowDelete(false); load(); }
    catch { toast.error('Có lỗi xảy ra'); }
  }

  function discountLabel(p: any) {
    if (p.type === 'Percentage') return `${p.value}%`;
    if (p.type === 'Fixed') return `${fmtN(p.value)}đ`;
    if (p.type === 'FreeShipping') return 'Miễn ship';
    return `Mua ${p.value}`;
  }

  return (
    <div>
      <div className="page-header">
        <div><h4>🎁 Quản lý khuyến mãi</h4>
          <ol className="breadcrumb"><li><Link to="/admin">Dashboard</Link></li><li className="active">Khuyến mãi</li></ol>
        </div>
        <button className="btn btn-gold" onClick={() => openForm()}>➕ Thêm khuyến mãi</button>
      </div>

      <div className="stats-row">
        <div className="stat-box"><div className="sb-icon bg-info">🎁</div><div><div className="sb-num">{stats.total}</div><div className="sb-label">Tổng cộng</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-success">✅</div><div><div className="sb-num">{stats.active}</div><div className="sb-label">Đang hoạt động</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-danger">⏰</div><div><div className="sb-num">{stats.expired}</div><div className="sb-label">Hết hạn</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-gold">🎫</div><div><div className="sb-num">{stats.totalUsed}</div><div className="sb-label">Lượt dùng</div></div></div>
      </div>

      <div className="card">
        <div className="card-header">
          <div className="filter-row">
            <div className="search-box">
              <input className="form-control" value={search} onChange={e => setSearch(e.target.value)} placeholder="Tìm mã khuyến mãi..." onKeyDown={e => e.key === 'Enter' && load()} />
              <button className="btn btn-gold btn-sm" onClick={load}>🔍</button>
            </div>
          </div>
        </div>
        <div className="card-body p-0">
          <table className="adm-table">
            <thead><tr><th>#</th><th>Mã</th><th>Tên</th><th>Loại</th><th>Giá trị</th><th>Đơn tối thiểu</th><th>Đã dùng</th><th>Thời hạn</th><th>Trạng thái</th><th>Thao tác</th></tr></thead>
            <tbody>
              {items.map((p, i) => (
                <tr key={p._id}>
                  <td>{i + 1}</td>
                  <td><strong style={{ fontFamily: 'monospace', letterSpacing: 1 }}>{p.code}</strong></td>
                  <td>{p.name}</td>
                  <td>{TYPES.find(t => t.value === p.type)?.label || p.type}</td>
                  <td className="fw-bold">{discountLabel(p)}</td>
                  <td>{p.minOrder ? `${fmtN(p.minOrder)}đ` : '-'}</td>
                  <td>{p.usedCount || 0}/{p.usageLimit || '∞'}</td>
                  <td><div className="sub-txt">{fmtD(p.startDate)} - {fmtD(p.endDate)}</div></td>
                  <td>
                    {isExpired(p) ? <span className="os-badge os-cancelled">Hết hạn</span> :
                     isActive(p) ? <span className="os-badge os-completed">Đang chạy</span> :
                     <span className="os-badge os-pending">Tạm tắt</span>}
                    <label className="toggle-switch" style={{ marginLeft: 8 }}><input type="checkbox" checked={p.isActive} onChange={() => toggleActive(p)} /><span className="toggle-slider"></span></label>
                  </td>
                  <td><div className="act-g">
                    <button className="ab ab-edit" onClick={() => openForm(p)}>✏️</button>
                    <button className="ab ab-del" onClick={() => { setDeleteItem(p); setShowDelete(true); }}>🗑️</button>
                  </div></td>
                </tr>
              ))}
              {!items.length && !loading && <tr><td colSpan={10} className="empty">Không có khuyến mãi nào</td></tr>}
              {loading && <tr><td colSpan={10} className="empty">Đang tải...</td></tr>}
            </tbody>
          </table>
        </div>
      </div>

      {showForm && (
        <div className="mo" onClick={() => setShowForm(false)}>
          <div className="md md-lg" onClick={e => e.stopPropagation()}>
            <div className="mh bg-g"><h5>{editing ? '✏️ Sửa' : '➕ Thêm'} khuyến mãi</h5><button className="mx" onClick={() => setShowForm(false)}>&times;</button></div>
            <div className="mb-modal">
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>Mã khuyến mãi <span className="req">*</span></label><input className="form-control" value={fd.code} onChange={e => setFd({ ...fd, code: e.target.value.toUpperCase() })} style={{ fontFamily: 'monospace', letterSpacing: 2 }} /></div></div>
                <div className="adm-col-6"><div className="fg"><label>Tên <span className="req">*</span></label><input className="form-control" value={fd.name} onChange={e => setFd({ ...fd, name: e.target.value })} /></div></div>
              </div>
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>Loại</label>
                  <select className="form-control" value={fd.type} onChange={e => setFd({ ...fd, type: e.target.value })}>
                    {TYPES.map(t => <option key={t.value} value={t.value}>{t.label}</option>)}
                  </select>
                </div></div>
                <div className="adm-col-6"><div className="fg"><label>Giá trị {fd.type === 'Percentage' ? '(%)' : fd.type === 'Fixed' ? '(VNĐ)' : ''}</label><input type="number" className="form-control" value={fd.value} onChange={e => setFd({ ...fd, value: +e.target.value })} /></div></div>
              </div>
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>Đơn tối thiểu (VNĐ)</label><input type="number" className="form-control" value={fd.minOrder} onChange={e => setFd({ ...fd, minOrder: +e.target.value })} /></div></div>
                <div className="adm-col-6"><div className="fg"><label>Giảm tối đa (VNĐ)</label><input type="number" className="form-control" value={fd.maxDiscount} onChange={e => setFd({ ...fd, maxDiscount: +e.target.value })} /></div></div>
              </div>
              <div className="row">
                <div className="adm-col-4"><div className="fg"><label>Giới hạn sử dụng</label><input type="number" className="form-control" value={fd.usageLimit} onChange={e => setFd({ ...fd, usageLimit: +e.target.value })} placeholder="0 = không giới hạn" /></div></div>
                <div className="adm-col-4"><div className="fg"><label>Ngày bắt đầu</label><input type="date" className="form-control" value={fd.startDate} onChange={e => setFd({ ...fd, startDate: e.target.value })} /></div></div>
                <div className="adm-col-4"><div className="fg"><label>Ngày kết thúc</label><input type="date" className="form-control" value={fd.endDate} onChange={e => setFd({ ...fd, endDate: e.target.value })} /></div></div>
              </div>
              <div className="fg"><label>Mô tả</label><textarea className="form-control" rows={2} value={fd.description} onChange={e => setFd({ ...fd, description: e.target.value })} /></div>
              <label style={{ display: 'flex', alignItems: 'center', gap: 6, cursor: 'pointer' }}>
                <input type="checkbox" checked={fd.isActive} onChange={e => setFd({ ...fd, isActive: e.target.checked })} /> Kích hoạt
              </label>
            </div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowForm(false)}>Hủy</button><button className="btn btn-gold" onClick={save} disabled={saving}>{saving ? 'Đang lưu...' : '💾 Lưu'}</button></div>
          </div>
        </div>
      )}

      {showDelete && (
        <div className="mo" onClick={() => setShowDelete(false)}>
          <div className="md" onClick={e => e.stopPropagation()}>
            <div className="mh bg-r"><h5>🗑️ Xóa khuyến mãi</h5><button className="mx" onClick={() => setShowDelete(false)}>&times;</button></div>
            <div className="mb-modal"><p>Bạn có chắc muốn xóa <strong>{deleteItem?.code}</strong>?</p></div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowDelete(false)}>Hủy</button><button className="btn btn-danger" onClick={doDelete}>🗑️ Xác nhận xóa</button></div>
          </div>
        </div>
      )}
    </div>
  );
}
