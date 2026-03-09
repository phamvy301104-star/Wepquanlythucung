import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { reviewApi, getImageUrl } from '../../../services/api';
import '../shared/admin.scss';

function fmtD(d: string) { return d ? new Date(d).toLocaleDateString('vi-VN') : '-'; }
function stars(n: number) { return '★'.repeat(n) + '☆'.repeat(5 - n); }

export default function AdminReviews() {
  const [items, setItems] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState('');
  const [ratingFilter, setRatingFilter] = useState('');
  const [approvalFilter, setApprovalFilter] = useState('');
  const [stats, setStats] = useState({ total: 0, avg: 0, pending: 0, approved: 0 });

  const [showReply, setShowReply] = useState(false);
  const [replyItem, setReplyItem] = useState<any>(null);
  const [replyText, setReplyText] = useState('');

  const [showDelete, setShowDelete] = useState(false);
  const [deleteItem, setDeleteItem] = useState<any>(null);

  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  useEffect(() => { load(); }, [page]);

  async function load() {
    setLoading(true);
    try {
      const p: any = { page: String(page), limit: '20' };
      if (search) p.search = search;
      if (ratingFilter) p.rating = ratingFilter;
      if (approvalFilter) p.isApproved = approvalFilter;
      const r = await reviewApi.getAll(p);
      const d = r.data?.data || r.data;
      const list = d?.reviews || d || [];
      setItems(list);
      setTotalPages(d?.totalPages || 1);
      setStats({
        total: d?.total || list.length,
        avg: list.length ? (list.reduce((s: number, rv: any) => s + (rv.rating || 0), 0) / list.length) : 0,
        pending: list.filter((rv: any) => !rv.isApproved).length,
        approved: list.filter((rv: any) => rv.isApproved).length,
      });
    } catch {}
    setLoading(false);
  }

  async function toggleApproval(item: any) {
    try {
      await reviewApi.reply(item._id, { isApproved: !item.isApproved });
      toast.success(item.isApproved ? 'Đã ẩn đánh giá' : 'Đã duyệt đánh giá');
      load();
    } catch { toast.error('Có lỗi xảy ra'); }
  }

  function openReply(item: any) {
    setReplyItem(item);
    setReplyText(item.adminReply || '');
    setShowReply(true);
  }

  async function submitReply() {
    if (!replyItem || !replyText.trim()) { toast.error('Nhập nội dung phản hồi'); return; }
    try {
      await reviewApi.reply(replyItem._id, { adminReply: replyText });
      toast.success('Đã gửi phản hồi!');
      setShowReply(false); load();
    } catch { toast.error('Có lỗi xảy ra'); }
  }

  async function doDelete() {
    if (!deleteItem) return;
    try { await reviewApi.delete(deleteItem._id); toast.success('Đã xóa!'); setShowDelete(false); load(); }
    catch { toast.error('Có lỗi xảy ra'); }
  }

  return (
    <div>
      <div className="page-header">
        <div><h4>⭐ Quản lý đánh giá</h4>
          <ol className="breadcrumb"><li><Link to="/admin">Dashboard</Link></li><li className="active">Đánh giá</li></ol>
        </div>
      </div>

      <div className="stats-row">
        <div className="stat-box"><div className="sb-icon bg-info">⭐</div><div><div className="sb-num">{stats.total}</div><div className="sb-label">Tổng đánh giá</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-gold">🌟</div><div><div className="sb-num">{stats.avg.toFixed(1)}</div><div className="sb-label">Điểm TB</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-warning">⏳</div><div><div className="sb-num">{stats.pending}</div><div className="sb-label">Chờ duyệt</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-success">✅</div><div><div className="sb-num">{stats.approved}</div><div className="sb-label">Đã duyệt</div></div></div>
      </div>

      <div className="card">
        <div className="card-header">
          <div className="filter-row">
            <div className="search-box">
              <input className="form-control" value={search} onChange={e => setSearch(e.target.value)} placeholder="Tìm đánh giá..." onKeyDown={e => e.key === 'Enter' && load()} />
              <button className="btn btn-gold btn-sm" onClick={load}>🔍</button>
            </div>
            <select className="form-control fc-sm" value={ratingFilter} onChange={e => { setRatingFilter(e.target.value); setTimeout(load, 0); }}>
              <option value="">Tất cả sao</option>
              {[5, 4, 3, 2, 1].map(n => <option key={n} value={String(n)}>{n} sao</option>)}
            </select>
            <select className="form-control fc-sm" value={approvalFilter} onChange={e => { setApprovalFilter(e.target.value); setTimeout(load, 0); }}>
              <option value="">Tất cả</option>
              <option value="true">Đã duyệt</option>
              <option value="false">Chờ duyệt</option>
            </select>
          </div>
        </div>
        <div className="card-body p-0">
          <table className="adm-table">
            <thead><tr><th>#</th><th>Khách hàng</th><th>Sản phẩm/DV</th><th>Đánh giá</th><th>Nội dung</th><th>Ngày</th><th>Duyệt</th><th>Thao tác</th></tr></thead>
            <tbody>
              {items.map((rv, i) => (
                <tr key={rv._id}>
                  <td>{(page - 1) * 20 + i + 1}</td>
                  <td><strong>{rv.user?.fullName || '-'}</strong><div className="sub-txt">{rv.user?.email}</div></td>
                  <td>{rv.product?.name || rv.service?.name || rv.pet?.name || '-'}<div className="sub-txt">{rv.type || '-'}</div></td>
                  <td><span style={{ color: '#f5a623', fontSize: '1rem' }}>{stars(rv.rating || 0)}</span></td>
                  <td style={{ maxWidth: 250, whiteSpace: 'normal' }}>
                    {rv.comment || '-'}
                    {rv.images?.length > 0 && (
                      <div style={{ display: 'flex', gap: 4, marginTop: 6, flexWrap: 'wrap' }}>
                        {rv.images.map((img: string, idx: number) => (
                          <a key={idx} href={getImageUrl(img)} target="_blank" rel="noopener noreferrer">
                            <img src={getImageUrl(img)} alt="" style={{ width: 48, height: 48, objectFit: 'cover', borderRadius: 4, border: '1px solid #dee2e6' }} />
                          </a>
                        ))}
                      </div>
                    )}
                    {rv.adminReply && <div className="sub-txt" style={{ marginTop: 4 }}>💬 Admin: {rv.adminReply}</div>}
                  </td>
                  <td>{fmtD(rv.createdAt)}</td>
                  <td>
                    <label className="toggle-switch"><input type="checkbox" checked={rv.isApproved} onChange={() => toggleApproval(rv)} /><span className="toggle-slider"></span></label>
                  </td>
                  <td><div className="act-g">
                    <button className="ab ab-edit" title="Phản hồi" onClick={() => openReply(rv)}>💬</button>
                    <button className="ab ab-del" onClick={() => { setDeleteItem(rv); setShowDelete(true); }}>🗑️</button>
                  </div></td>
                </tr>
              ))}
              {!items.length && !loading && <tr><td colSpan={8} className="empty">Không có đánh giá nào</td></tr>}
              {loading && <tr><td colSpan={8} className="empty">Đang tải...</td></tr>}
            </tbody>
          </table>
        </div>
        {totalPages > 1 && (
          <div className="card-footer" style={{ display: 'flex', justifyContent: 'center', gap: 4 }}>
            {Array.from({ length: totalPages }, (_, i) => (
              <button key={i} className={`btn btn-sm ${page === i + 1 ? 'btn-gold' : 'btn-outline-gold'}`} onClick={() => setPage(i + 1)}>{i + 1}</button>
            ))}
          </div>
        )}
      </div>

      {/* Reply Modal */}
      {showReply && replyItem && (
        <div className="mo" onClick={() => setShowReply(false)}>
          <div className="md" onClick={e => e.stopPropagation()}>
            <div className="mh bg-g"><h5>💬 Phản hồi đánh giá</h5><button className="mx" onClick={() => setShowReply(false)}>&times;</button></div>
            <div className="mb-modal">
              <div style={{ background: '#f8f9fa', borderRadius: 8, padding: 12, marginBottom: 12 }}>
                <p><strong>{replyItem.user?.fullName}</strong> <span style={{ color: '#f5a623' }}>{stars(replyItem.rating)}</span></p>
                <p>{replyItem.comment || '(Không có nội dung)'}</p>
              </div>
              <div className="fg"><label>Nội dung phản hồi</label><textarea className="form-control" rows={4} value={replyText} onChange={e => setReplyText(e.target.value)} placeholder="Nhập phản hồi..." /></div>
            </div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowReply(false)}>Hủy</button><button className="btn btn-gold" onClick={submitReply}>💬 Gửi phản hồi</button></div>
          </div>
        </div>
      )}

      {showDelete && (
        <div className="mo" onClick={() => setShowDelete(false)}>
          <div className="md" onClick={e => e.stopPropagation()}>
            <div className="mh bg-r"><h5>🗑️ Xóa đánh giá</h5><button className="mx" onClick={() => setShowDelete(false)}>&times;</button></div>
            <div className="mb-modal"><p>Bạn có chắc muốn xóa đánh giá này?</p></div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowDelete(false)}>Hủy</button><button className="btn btn-danger" onClick={doDelete}>🗑️ Xác nhận xóa</button></div>
          </div>
        </div>
      )}
    </div>
  );
}
