import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { contactApi } from '../../../services/api';
import '../shared/admin.scss';

function fmtD(d: string) { return d ? new Date(d).toLocaleString('vi-VN') : '-'; }

const STATUS_LABELS: Record<string, string> = { new: 'Mới', read: 'Đã đọc', replied: 'Đã trả lời', archived: 'Lưu trữ' };
const STATUS_CLASS: Record<string, string> = { new: 'os-pending', read: 'os-confirmed', replied: 'os-completed', archived: 'os-cancelled' };

export default function AdminContacts() {
  const [items, setItems] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [stats, setStats] = useState({ total: 0, newC: 0, read: 0, replied: 0 });

  const [detail, setDetail] = useState<any>(null);
  const [showDetail, setShowDetail] = useState(false);
  const [adminNote, setAdminNote] = useState('');

  const [showDelete, setShowDelete] = useState(false);
  const [deleteItem, setDeleteItem] = useState<any>(null);

  useEffect(() => { load(); }, []);

  async function load() {
    setLoading(true);
    try {
      const p: any = { limit: '200' };
      if (search) p.search = search;
      if (statusFilter) p.status = statusFilter;
      const r = await contactApi.getAll(p);
      const d = r.data?.data || r.data;
      const list = d?.contacts || d || [];
      setItems(list);
      setStats({
        total: list.length,
        newC: list.filter((c: any) => c.status === 'new').length,
        read: list.filter((c: any) => c.status === 'read').length,
        replied: list.filter((c: any) => c.status === 'replied').length,
      });
    } catch {}
    setLoading(false);
  }

  async function openDetail(item: any) {
    setDetail(item);
    setAdminNote(item.adminNote || '');
    setShowDetail(true);
    if (item.status === 'new') {
      try { await contactApi.updateStatus(item._id, 'read'); load(); } catch {}
    }
  }

  async function updateStatus(status: string) {
    if (!detail) return;
    try {
      await contactApi.updateStatus(detail._id, status, adminNote);
      toast.success(`Đã cập nhật trạng thái!`);
      setDetail({ ...detail, status, adminNote });
      load();
    } catch { toast.error('Có lỗi xảy ra'); }
  }

  async function doDelete() {
    if (!deleteItem) return;
    try { await contactApi.delete(deleteItem._id); toast.success('Đã xóa!'); setShowDelete(false); load(); }
    catch { toast.error('Có lỗi xảy ra'); }
  }

  return (
    <div>
      <div className="page-header">
        <div><h4>📧 Quản lý liên hệ</h4>
          <ol className="breadcrumb"><li><Link to="/admin">Dashboard</Link></li><li className="active">Liên hệ</li></ol>
        </div>
      </div>

      <div className="stats-row">
        <div className="stat-box"><div className="sb-icon bg-info">📧</div><div><div className="sb-num">{stats.total}</div><div className="sb-label">Tổng cộng</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-warning">🆕</div><div><div className="sb-num">{stats.newC}</div><div className="sb-label">Mới</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-info">👁️</div><div><div className="sb-num">{stats.read}</div><div className="sb-label">Đã đọc</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-success">✅</div><div><div className="sb-num">{stats.replied}</div><div className="sb-label">Đã trả lời</div></div></div>
      </div>

      <div className="card">
        <div className="card-header">
          <div className="filter-row">
            <div className="search-box">
              <input className="form-control" value={search} onChange={e => setSearch(e.target.value)} placeholder="Tìm liên hệ..." onKeyDown={e => e.key === 'Enter' && load()} />
              <button className="btn btn-gold btn-sm" onClick={load}>🔍</button>
            </div>
            <select className="form-control fc-sm" value={statusFilter} onChange={e => { setStatusFilter(e.target.value); setTimeout(load, 0); }}>
              <option value="">Tất cả</option>
              {Object.entries(STATUS_LABELS).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
            </select>
          </div>
        </div>
        <div className="card-body p-0">
          <table className="adm-table">
            <thead><tr><th>#</th><th>Họ tên</th><th>Email</th><th>SĐT</th><th>Chủ đề</th><th>Ngày gửi</th><th>Trạng thái</th><th>Thao tác</th></tr></thead>
            <tbody>
              {items.map((c, i) => (
                <tr key={c._id} className="clickable-row" onClick={() => openDetail(c)} style={c.status === 'new' ? { fontWeight: 600 } : undefined}>
                  <td>{i + 1}</td>
                  <td>{c.fullName || c.name || '-'}</td>
                  <td className="sub-txt">{c.email || '-'}</td>
                  <td>{c.phone || c.phoneNumber || '-'}</td>
                  <td style={{ maxWidth: 250 }}>{c.subject || c.message?.substring(0, 50) || '-'}</td>
                  <td>{fmtD(c.createdAt)}</td>
                  <td><span className={`os-badge ${STATUS_CLASS[c.status] || ''}`}>{STATUS_LABELS[c.status] || c.status}</span></td>
                  <td onClick={e => e.stopPropagation()}><div className="act-g">
                    <button className="ab ab-edit" onClick={() => openDetail(c)}>👁️</button>
                    <button className="ab ab-del" onClick={() => { setDeleteItem(c); setShowDelete(true); }}>🗑️</button>
                  </div></td>
                </tr>
              ))}
              {!items.length && !loading && <tr><td colSpan={8} className="empty">Không có liên hệ nào</td></tr>}
              {loading && <tr><td colSpan={8} className="empty">Đang tải...</td></tr>}
            </tbody>
          </table>
        </div>
      </div>

      {/* Detail Modal */}
      {showDetail && detail && (
        <div className="mo" onClick={() => setShowDetail(false)}>
          <div className="md md-lg" onClick={e => e.stopPropagation()}>
            <div className="mh bg-g"><h5>📧 Chi tiết liên hệ</h5><button className="mx" onClick={() => setShowDetail(false)}>&times;</button></div>
            <div className="mb-modal">
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 12 }}>
                <span className={`os-badge ${STATUS_CLASS[detail.status]}`}>{STATUS_LABELS[detail.status]}</span>
                <span className="sub-txt">{fmtD(detail.createdAt)}</span>
              </div>
              <div className="row">
                <div className="adm-col-6"><p><strong>Họ tên:</strong> {detail.fullName || detail.name}</p></div>
                <div className="adm-col-6"><p><strong>Email:</strong> {detail.email}</p></div>
              </div>
              <div className="row">
                <div className="adm-col-6"><p><strong>SĐT:</strong> {detail.phone || detail.phoneNumber || '-'}</p></div>
                <div className="adm-col-6"><p><strong>Chủ đề:</strong> {detail.subject || '-'}</p></div>
              </div>
              <div style={{ background: '#f8f9fa', borderRadius: 8, padding: 16, marginTop: 12, marginBottom: 12 }}>
                <strong>Nội dung:</strong>
                <p style={{ whiteSpace: 'pre-wrap', marginTop: 8 }}>{detail.message || '-'}</p>
              </div>
              <div className="fg">
                <label>Ghi chú admin</label>
                <textarea className="form-control" rows={3} value={adminNote} onChange={e => setAdminNote(e.target.value)} placeholder="Ghi chú nội bộ..." />
              </div>
              <div style={{ display: 'flex', gap: 8, marginTop: 12 }}>
                {detail.status !== 'replied' && <button className="btn btn-gold" onClick={() => updateStatus('replied')}>✅ Đánh dấu đã trả lời</button>}
                {detail.status !== 'archived' && <button className="btn btn-sec" onClick={() => updateStatus('archived')}>📁 Lưu trữ</button>}
                {detail.status === 'archived' && <button className="btn btn-info" onClick={() => updateStatus('read')}>📤 Bỏ lưu trữ</button>}
              </div>
            </div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowDetail(false)}>Đóng</button></div>
          </div>
        </div>
      )}

      {showDelete && (
        <div className="mo" onClick={() => setShowDelete(false)}>
          <div className="md" onClick={e => e.stopPropagation()}>
            <div className="mh bg-r"><h5>🗑️ Xóa liên hệ</h5><button className="mx" onClick={() => setShowDelete(false)}>&times;</button></div>
            <div className="mb-modal"><p>Bạn có chắc muốn xóa liên hệ này?</p></div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowDelete(false)}>Hủy</button><button className="btn btn-danger" onClick={doDelete}>🗑️ Xác nhận xóa</button></div>
          </div>
        </div>
      )}
    </div>
  );
}
