import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { orderApi, getImageUrl } from '../../../services/api';
import '../shared/admin.scss';

const STATUS_LABELS: Record<string, string> = { Pending: 'Chờ xử lý', Confirmed: 'Đã xác nhận', Processing: 'Đang xử lý', Shipping: 'Đang giao', Delivered: 'Đã giao', Completed: 'Hoàn thành', Cancelled: 'Đã hủy', Returned: 'Đã trả' };
const STATUS_CLASS: Record<string, string> = { Pending: 'os-pending', Confirmed: 'os-confirmed', Processing: 'os-processing', Shipping: 'os-shipping', Delivered: 'os-completed', Completed: 'os-completed', Cancelled: 'os-cancelled', Returned: 'os-cancelled' };
const STATUS_FLOW: Record<string, string[]> = { Pending: ['Confirmed', 'Cancelled'], Confirmed: ['Processing', 'Cancelled'], Processing: ['Shipping'], Shipping: ['Delivered'], Delivered: ['Completed'], Completed: [], Cancelled: [], Returned: [] };

function fmtN(n: number) { return new Intl.NumberFormat('vi-VN').format(n); }
function fmtD(d: string) { return d ? new Date(d).toLocaleString('vi-VN') : '-'; }

export default function AdminOrders() {
  const [items, setItems] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const [stats, setStats] = useState({ total: 0, pending: 0, processing: 0, shipped: 0, delivered: 0, cancelled: 0, revenue: 0 });

  const [detail, setDetail] = useState<any>(null);
  const [showDetail, setShowDetail] = useState(false);

  const [showCancel, setShowCancel] = useState(false);
  const [cancelItem, setCancelItem] = useState<any>(null);
  const [cancelReason, setCancelReason] = useState('');

  const [showDelete, setShowDelete] = useState(false);
  const [deleteItem, setDeleteItem] = useState<any>(null);

  useEffect(() => { load(); }, []);

  async function load() {
    setLoading(true);
    try {
      const p: any = { limit: '200' };
      if (search) p.search = search;
      if (statusFilter) p.status = statusFilter;
      if (dateFrom) p.dateFrom = dateFrom;
      if (dateTo) p.dateTo = dateTo;
      const r = await orderApi.getAll(p);
      const d = r.data?.data || r.data;
      const list = d?.orders || d || [];
      setItems(list);
      setStats({
        total: list.length,
        pending: list.filter((o: any) => o.status === 'Pending').length,
        processing: list.filter((o: any) => o.status === 'Processing' || o.status === 'Confirmed').length,
        shipped: list.filter((o: any) => o.status === 'Shipping').length,
        delivered: list.filter((o: any) => o.status === 'Delivered').length,
        cancelled: list.filter((o: any) => o.status === 'Cancelled').length,
        revenue: list.filter((o: any) => o.status === 'Delivered').reduce((s: number, o: any) => s + (o.totalAmount || 0), 0),
      });
    } catch {}
    setLoading(false);
  }

  async function updateStatus(item: any, newStatus: string) {
    if (newStatus === 'Cancelled') { setCancelItem(item); setCancelReason(''); setShowCancel(true); return; }
    try {
      await orderApi.updateStatus(item._id, newStatus);
      toast.success(`Đã chuyển sang "${STATUS_LABELS[newStatus]}"`);
      if (showDetail && detail?._id === item._id) setDetail({ ...detail, status: newStatus });
      load();
    } catch { toast.error('Có lỗi xảy ra'); }
  }

  async function doCancel() {
    if (!cancelItem) return;
    try {
      await orderApi.updateStatus(cancelItem._id, 'Cancelled', cancelReason);
      toast.success('Đã hủy đơn hàng!'); setShowCancel(false);
      if (showDetail) setShowDetail(false);
      load();
    } catch { toast.error('Có lỗi xảy ra'); }
  }

  async function doDelete() {
    if (!deleteItem) return;
    try { await orderApi.delete(deleteItem._id); toast.success('Đã xóa!'); setShowDelete(false); load(); }
    catch { toast.error('Có lỗi xảy ra'); }
  }

  return (
    <div>
      <div className="page-header">
        <div><h4>📦 Quản lý đơn hàng</h4>
          <ol className="breadcrumb"><li><Link to="/admin">Dashboard</Link></li><li className="active">Đơn hàng</li></ol>
        </div>
      </div>

      <div className="stats-row">
        <div className="stat-box"><div className="sb-icon bg-info">📦</div><div><div className="sb-num">{stats.total}</div><div className="sb-label">Tổng đơn</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-warning">⏳</div><div><div className="sb-num">{stats.pending}</div><div className="sb-label">Chờ xử lý</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-info">🔄</div><div><div className="sb-num">{stats.processing}</div><div className="sb-label">Đang xử lý</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-primary">🚚</div><div><div className="sb-num">{stats.shipped}</div><div className="sb-label">Đang giao</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-success">✅</div><div><div className="sb-num">{stats.delivered}</div><div className="sb-label">Đã giao</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-gold">💰</div><div><div className="sb-num">{fmtN(stats.revenue)}đ</div><div className="sb-label">Doanh thu</div></div></div>
      </div>

      <div className="card">
        <div className="card-header">
          <div className="filter-row">
            <div className="search-box">
              <input className="form-control" value={search} onChange={e => setSearch(e.target.value)} placeholder="Tìm đơn hàng..." onKeyDown={e => e.key === 'Enter' && load()} />
              <button className="btn btn-gold btn-sm" onClick={load}>🔍</button>
            </div>
            <select className="form-control fc-sm" value={statusFilter} onChange={e => { setStatusFilter(e.target.value); setTimeout(load, 0); }}>
              <option value="">Tất cả trạng thái</option>
              {Object.entries(STATUS_LABELS).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
            </select>
            <input type="date" className="form-control fc-sm" value={dateFrom} onChange={e => { setDateFrom(e.target.value); setTimeout(load, 0); }} title="Từ ngày" />
            <input type="date" className="form-control fc-sm" value={dateTo} onChange={e => { setDateTo(e.target.value); setTimeout(load, 0); }} title="Đến ngày" />
          </div>
        </div>
        <div className="card-body p-0">
          <table className="adm-table">
            <thead><tr><th>Mã đơn</th><th>Khách hàng</th><th>SĐT</th><th>SP</th><th>Tổng tiền</th><th>Thanh toán</th><th>Trạng thái</th><th>Ngày đặt</th><th>Thao tác</th></tr></thead>
            <tbody>
              {items.map(o => (
                <tr key={o._id} className="clickable-row" onClick={() => { setDetail(o); setShowDetail(true); }}>
                  <td><strong>#{o.orderNumber || o._id?.slice(-6)}</strong></td>
                  <td>{o.shippingAddress?.fullName || o.user?.fullName || '-'}</td>
                  <td>{o.shippingAddress?.phone || o.user?.phoneNumber || '-'}</td>
                  <td>{o.items?.length || 0}</td>
                  <td className="fw-bold">{fmtN(o.totalAmount || 0)}đ</td>
                  <td><span className={`os-badge ${o.paymentStatus === 'Paid' ? 'os-completed' : 'os-pending'}`}>{o.paymentStatus === 'Paid' ? 'Đã TT' : 'Chưa TT'}</span></td>
                  <td><span className={`os-badge ${STATUS_CLASS[o.status] || ''}`}>{STATUS_LABELS[o.status] || o.status}</span></td>
                  <td>{fmtD(o.createdAt)}</td>
                  <td onClick={e => e.stopPropagation()}><div className="act-g">
                    {STATUS_FLOW[o.status]?.length > 0 && (
                      <select className="form-control fc-sm" style={{ minWidth: 130, fontSize: '0.8rem' }} defaultValue="" onChange={e => { if (e.target.value) updateStatus(o, e.target.value); e.target.value = ''; }}>
                        <option value="" disabled>Chuyển trạng thái</option>
                        {STATUS_FLOW[o.status].map(ns => <option key={ns} value={ns}>{STATUS_LABELS[ns]}</option>)}
                      </select>
                    )}
                    <button className="ab ab-del" onClick={() => { setDeleteItem(o); setShowDelete(true); }}>🗑️</button>
                  </div></td>
                </tr>
              ))}
              {!items.length && !loading && <tr><td colSpan={9} className="empty">Không có đơn hàng nào</td></tr>}
              {loading && <tr><td colSpan={9} className="empty">Đang tải...</td></tr>}
            </tbody>
          </table>
        </div>
      </div>

      {/* Detail Modal */}
      {showDetail && detail && (
        <div className="mo" onClick={() => setShowDetail(false)}>
          <div className="md md-lg" onClick={e => e.stopPropagation()}>
            <div className="mh bg-g"><h5>📦 Chi tiết đơn hàng #{detail.orderNumber || detail._id?.slice(-6)}</h5><button className="mx" onClick={() => setShowDetail(false)}>&times;</button></div>
            <div className="mb-modal">
              <div style={{ display: 'flex', gap: 8, marginBottom: 16, flexWrap: 'wrap' }}>
                <span className={`os-badge ${STATUS_CLASS[detail.status]}`}>{STATUS_LABELS[detail.status]}</span>
                <span className={`os-badge ${detail.paymentStatus === 'Paid' ? 'os-completed' : 'os-pending'}`}>{detail.paymentStatus === 'Paid' ? '💳 Đã thanh toán' : '💳 Chưa thanh toán'}</span>
              </div>
              <div className="row">
                <div className="adm-col-6">
                  <h6>👤 Khách hàng</h6>
                  <p><strong>Tên:</strong> {detail.shippingAddress?.fullName || detail.user?.fullName}</p>
                  <p><strong>SĐT:</strong> {detail.shippingAddress?.phone || detail.user?.phoneNumber}</p>
                  <p><strong>Email:</strong> {detail.user?.email || '-'}</p>
                </div>
                <div className="adm-col-6">
                  <h6>🚚 Giao hàng</h6>
                  <p><strong>Địa chỉ:</strong> {detail.shippingAddress?.address || '-'}</p>
                  <p><strong>Phường/Xã:</strong> {detail.shippingAddress?.ward || '-'}</p>
                  <p><strong>Quận/Huyện:</strong> {detail.shippingAddress?.district || '-'}</p>
                  <p><strong>Tỉnh/TP:</strong> {detail.shippingAddress?.city || '-'}</p>
                  <p><strong>Phương thức:</strong> {detail.paymentMethod === 'COD' ? 'Thanh toán khi nhận hàng' : detail.paymentMethod || '-'}</p>
                </div>
              </div>

              <h6 style={{ marginTop: 16 }}>🛒 Sản phẩm</h6>
              <table className="adm-table bordered-table">
                <thead><tr><th>Ảnh</th><th>Sản phẩm</th><th>Giá</th><th>SL</th><th>Thành tiền</th></tr></thead>
                <tbody>
                  {detail.items?.map((item: any, idx: number) => (
                    <tr key={idx}>
                      <td><img src={getImageUrl(item.product?.imageUrl || item.productImage)} alt="" className="img-preview" /></td>
                      <td>{item.product?.name || item.productName || item.name || '-'}{item.type === 'pet' && <span className="os-badge os-confirmed" style={{ marginLeft: 4 }}>Thú cưng</span>}</td>
                      <td>{fmtN(item.unitPrice || item.price || 0)}đ</td>
                      <td>{item.quantity || 1}</td>
                      <td className="fw-bold">{fmtN((item.unitPrice || item.price || 0) * (item.quantity || 1))}đ</td>
                    </tr>
                  ))}
                </tbody>
              </table>
              <div style={{ marginTop: 12, textAlign: 'right' }}>
                {detail.subtotal && <p>Tạm tính: <strong>{fmtN(detail.subtotal)}đ</strong></p>}
                {detail.shippingFee != null && <p>Phí vận chuyển: <strong>{fmtN(detail.shippingFee)}đ</strong></p>}
                {detail.discount > 0 && <p className="text-success">Giảm giá: <strong>-{fmtN(detail.discount)}đ</strong></p>}
                <p style={{ fontSize: '1.2rem' }}>Tổng cộng: <strong style={{ color: '#667eea' }}>{fmtN(detail.totalAmount || 0)}đ</strong></p>
              </div>
              {detail.cancelReason && <p className="text-danger" style={{ marginTop: 12 }}><strong>Lý do hủy:</strong> {detail.cancelReason}</p>}
              {detail.notes && <p style={{ marginTop: 8 }}><strong>Ghi chú:</strong> {detail.notes}</p>}

              {STATUS_FLOW[detail.status]?.length > 0 && (
                <div style={{ marginTop: 16, display: 'flex', gap: 8 }}>
                  {STATUS_FLOW[detail.status].map(ns => (
                    <button key={ns} className={`btn ${ns === 'Cancelled' ? 'btn-danger' : 'btn-gold'}`} onClick={() => updateStatus(detail, ns)}>{STATUS_LABELS[ns]}</button>
                  ))}
                </div>
              )}
            </div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowDetail(false)}>Đóng</button></div>
          </div>
        </div>
      )}

      {/* Cancel Modal */}
      {showCancel && (
        <div className="mo" onClick={() => setShowCancel(false)}>
          <div className="md" onClick={e => e.stopPropagation()}>
            <div className="mh bg-r"><h5>❌ Hủy đơn hàng</h5><button className="mx" onClick={() => setShowCancel(false)}>&times;</button></div>
            <div className="mb-modal">
              <p>Bạn có chắc muốn hủy đơn hàng <strong>#{cancelItem?.orderNumber || cancelItem?._id?.slice(-6)}</strong>?</p>
              <div className="fg"><label>Lý do hủy</label><textarea className="form-control" rows={3} value={cancelReason} onChange={e => setCancelReason(e.target.value)} placeholder="Nhập lý do hủy..." /></div>
            </div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowCancel(false)}>Đóng</button><button className="btn btn-danger" onClick={doCancel}>❌ Xác nhận hủy</button></div>
          </div>
        </div>
      )}

      {/* Delete Modal */}
      {showDelete && (
        <div className="mo" onClick={() => setShowDelete(false)}>
          <div className="md" onClick={e => e.stopPropagation()}>
            <div className="mh bg-r"><h5>🗑️ Xóa đơn hàng</h5><button className="mx" onClick={() => setShowDelete(false)}>&times;</button></div>
            <div className="mb-modal"><p>Bạn có chắc muốn xóa đơn hàng này?</p><p className="text-danger">Hành động này không thể hoàn tác!</p></div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowDelete(false)}>Hủy</button><button className="btn btn-danger" onClick={doDelete}>🗑️ Xác nhận xóa</button></div>
          </div>
        </div>
      )}
    </div>
  );
}
