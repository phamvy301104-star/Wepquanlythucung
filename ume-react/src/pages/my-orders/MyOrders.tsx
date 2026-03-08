import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { orderApi, reviewApi } from '../../services/api';
import { getImageUrl } from '../../services/api';
import api from '../../services/api';
import toast from 'react-hot-toast';
import './MyOrders.scss';

const formatPrice = (p: number) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p);
const formatDate = (d: string) => new Date(d).toLocaleDateString('vi-VN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' });

const statuses = [
  { value: '', label: 'Tất cả' }, { value: 'Pending', label: 'Chờ xác nhận' },
  { value: 'Confirmed', label: 'Đã xác nhận' }, { value: 'Processing', label: 'Đang xử lý' },
  { value: 'Shipping', label: 'Đang giao' }, { value: 'Completed', label: 'Hoàn thành' },
  { value: 'Cancelled', label: 'Đã hủy' },
];
const statusLabels: any = { Pending: 'Chờ xác nhận', Confirmed: 'Đã xác nhận', Processing: 'Đang xử lý', Shipping: 'Đang giao', Completed: 'Hoàn thành', Cancelled: 'Đã hủy' };
const ratingLabels: any = { 1: 'Rất không hài lòng', 2: 'Không hài lòng', 3: 'Bình thường', 4: 'Hài lòng', 5: 'Rất hài lòng' };

export default function MyOrders() {
  const [orders, setOrders] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [selectedStatus, setSelectedStatus] = useState('');
  const [cancellingId, setCancellingId] = useState<string | null>(null);
  const [cancelReason, setCancelReason] = useState('');
  const [selectedOrder, setSelectedOrder] = useState<any>(null);

  // Review
  const [showReview, setShowReview] = useState(false);
  const [reviewOrder, setReviewOrder] = useState<any>(null);
  const [reviewItem, setReviewItem] = useState<any>(null);
  const [reviewRating, setReviewRating] = useState(5);
  const [reviewComment, setReviewComment] = useState('');
  const [reviewSubmitting, setReviewSubmitting] = useState(false);
  const [hoverRating, setHoverRating] = useState(0);
  const [reviewedMap, setReviewedMap] = useState<Record<string, string[]>>({});

  useEffect(() => { loadOrders(); }, []);

  const loadOrders = async (status?: string) => {
    setLoading(true);
    try {
      const params: any = {};
      const s = status ?? selectedStatus;
      if (s) params.status = s;
      const r = await orderApi.getMyOrders(params);
      const list = r.data?.data?.orders || r.data?.data?.items || (Array.isArray(r.data?.data) ? r.data.data : []);
      setOrders(list);
      list.forEach((o: any) => { if (o.status === 'Completed') checkReviews(o._id); });
    } catch {} finally { setLoading(false); }
  };

  const checkReviews = async (orderId: string) => {
    try {
      const r = await api.get(`/reviews/check-order/${orderId}`);
      setReviewedMap(m => ({ ...m, [orderId]: r.data?.data?.reviewedProductIds || [] }));
    } catch {}
  };

  const isItemReviewed = (order: any, item: any) => {
    const reviewed = reviewedMap[order._id] || [];
    const pid = item.product?._id || item.product;
    return pid ? reviewed.includes(pid.toString()) : false;
  };

  const filterByStatus = (s: string) => { setSelectedStatus(s); loadOrders(s); };

  const confirmCancel = async () => {
    if (!cancellingId) return;
    try {
      await orderApi.cancel(cancellingId, { cancelReason });
      toast.success('Đã hủy đơn hàng thành công');
      setCancellingId(null);
      loadOrders();
    } catch (err: any) { toast.error(err.response?.data?.message || 'Hủy thất bại'); }
  };

  const viewOrder = async (order: any) => {
    try {
      const r = await orderApi.getById(order._id);
      setSelectedOrder(r.data?.data?.order || r.data?.data || order);
    } catch { setSelectedOrder(order); }
  };

  const openReview = (order: any, item: any) => {
    setReviewOrder(order); setReviewItem(item); setReviewRating(5); setReviewComment(''); setHoverRating(0); setShowReview(true);
  };

  const submitReview = async () => {
    if (!reviewOrder || !reviewItem) return;
    setReviewSubmitting(true);
    try {
      const pid = reviewItem.product?._id || reviewItem.product;
      const fd = new FormData();
      fd.append('productId', pid);
      fd.append('orderId', reviewOrder._id);
      fd.append('rating', reviewRating.toString());
      fd.append('comment', reviewComment);
      await reviewApi.create(fd);
      toast.success('Cảm ơn bạn đã đánh giá!');
      setReviewedMap(m => ({ ...m, [reviewOrder._id]: [...(m[reviewOrder._id] || []), pid] }));
      setShowReview(false);
    } catch (err: any) { toast.error(err.response?.data?.message || 'Gửi đánh giá thất bại'); }
    finally { setReviewSubmitting(false); }
  };

  const activeRating = hoverRating || reviewRating;

  return (
    <div className="my-orders-page">
      <div className="page-header"><div className="container"><h1>🛍️ Đơn hàng của tôi</h1></div></div>
      <div className="container">
        <div className="status-tabs">
          {statuses.map(s => (
            <button key={s.value} className={selectedStatus === s.value ? 'active' : ''} onClick={() => filterByStatus(s.value)}>{s.label}</button>
          ))}
        </div>

        {loading && <div className="loading-center">⏳ Đang tải...</div>}

        {!loading && orders.length === 0 && (
          <div className="empty-state">📭<p>Bạn chưa có đơn hàng nào</p><Link to="/products" className="btn-shop">Mua sắm ngay</Link></div>
        )}

        <div className="orders-list">
          {orders.map(order => (
            <div key={order._id} className="order-card">
              <div className="card-header">
                <div className="order-code">🧾 {order.orderCode}</div>
                <span className={`status-badge ${order.status?.toLowerCase()}`}>{statusLabels[order.status] || order.status}</span>
              </div>
              <div className="card-body">
                <div className="order-items">
                  {(order.items || []).slice(0, 3).map((item: any, i: number) => (
                    <div key={i} className="order-item">
                      <img src={getImageUrl(item.productImage || item.product?.imageUrl || item.product?.mainImage)} alt="" onError={e => { (e.target as HTMLImageElement).src = '/assets/images/no-image.svg'; }} />
                      <div className="item-info"><span className="item-name">{item.productName || item.product?.name}</span><span className="item-qty">x{item.quantity} · {formatPrice(item.unitPrice || item.price)}</span></div>
                    </div>
                  ))}
                  {(order.items?.length || 0) > 3 && <div className="more-items">... và {order.items.length - 3} sản phẩm khác</div>}
                </div>
                <div className="order-meta"><span>🕐 {formatDate(order.createdAt)}</span>{order.paymentMethod && <span>💳 {order.paymentMethod === 'COD' ? 'COD' : 'Chuyển khoản'}</span>}</div>
              </div>
              <div className="card-footer">
                <span className="total-amount">Tổng: <strong>{formatPrice(order.totalAmount)}</strong></span>
                <div className="footer-actions">
                  <button className="btn-detail" onClick={() => viewOrder(order)}>👁️ Chi tiết</button>
                  {(order.status === 'Pending' || order.status === 'Confirmed') && <button className="btn-cancel" onClick={() => { setCancellingId(order._id); setCancelReason(''); }}>❌ Hủy đơn</button>}
                  {order.status === 'Completed' && (order.items || []).map((item: any, i: number) => (
                    !isItemReviewed(order, item) ? <button key={i} className="btn-review" onClick={() => openReview(order, item)}>⭐ Đánh giá {item.productName || item.product?.name}</button>
                    : <span key={i} className="reviewed-badge">✅ Đã đánh giá {item.productName || item.product?.name}</span>
                  ))}
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* Cancel Modal */}
        {cancellingId && (
          <div className="modal-overlay" onClick={() => setCancellingId(null)}>
            <div className="modal-content" onClick={e => e.stopPropagation()}>
              <h3>Hủy đơn hàng</h3><p>Bạn có chắc muốn hủy đơn hàng này?</p>
              <textarea value={cancelReason} onChange={e => setCancelReason(e.target.value)} placeholder="Lý do hủy..." rows={3} />
              <div className="modal-actions"><button className="btn-secondary" onClick={() => setCancellingId(null)}>Đóng</button><button className="btn-danger" onClick={confirmCancel}>Xác nhận hủy</button></div>
            </div>
          </div>
        )}

        {/* Order Detail Modal */}
        {selectedOrder && (
          <div className="modal-overlay" onClick={() => setSelectedOrder(null)}>
            <div className="modal-detail" onClick={e => e.stopPropagation()}>
              <div className="modal-header-detail"><h3>🧾 Chi tiết đơn hàng #{selectedOrder.orderCode}</h3><button onClick={() => setSelectedOrder(null)}>×</button></div>
              <div className="modal-body-detail">
                <div className="detail-grid">
                  <div><label>Trạng thái</label><span className={`status-badge ${selectedOrder.status?.toLowerCase()}`}>{statusLabels[selectedOrder.status] || selectedOrder.status}</span></div>
                  <div><label>Ngày đặt</label><span>{formatDate(selectedOrder.createdAt)}</span></div>
                  <div><label>Thanh toán</label><span>{selectedOrder.paymentMethod || 'COD'} — {selectedOrder.paymentStatus === 'Paid' ? 'Đã thanh toán' : 'Chưa thanh toán'}</span></div>
                  <div className="full-w"><label>Địa chỉ giao hàng</label><span>{typeof selectedOrder.shippingAddress === 'string' ? selectedOrder.shippingAddress : [selectedOrder.shippingAddress?.address, selectedOrder.shippingAddress?.ward, selectedOrder.shippingAddress?.district, selectedOrder.shippingAddress?.city].filter(Boolean).join(', ')}</span></div>
                </div>
                <h4>📦 Sản phẩm ({selectedOrder.items?.length || 0})</h4>
                <div className="detail-items">
                  {(selectedOrder.items || []).map((it: any, j: number) => (
                    <div key={j} className="detail-item">
                      <span className="di-num">{j + 1}</span>
                      <img src={getImageUrl(it.productImage || it.product?.imageUrl)} alt="" className="di-img" onError={e => { (e.target as HTMLImageElement).src = '/assets/images/no-image.svg'; }} />
                      <div className="di-info"><div className="di-name">{it.productName || it.product?.name}</div><div className="di-meta">{formatPrice(it.unitPrice || it.price)} × {it.quantity}</div></div>
                      <div className="di-total">{formatPrice((it.unitPrice || it.price) * it.quantity)}</div>
                    </div>
                  ))}
                </div>
                <div className="detail-summary">
                  {selectedOrder.subtotal && <div className="summary-row"><span>Tạm tính:</span><span>{formatPrice(selectedOrder.subtotal)}</span></div>}
                  {selectedOrder.shippingFee && <div className="summary-row"><span>Phí vận chuyển:</span><span>{formatPrice(selectedOrder.shippingFee)}</span></div>}
                  {selectedOrder.discount && <div className="summary-row"><span>Giảm giá:</span><span style={{ color: '#dc3545' }}>-{formatPrice(selectedOrder.discount)}</span></div>}
                  <div className="summary-row total-row"><span>Tổng cộng:</span><span>{formatPrice(selectedOrder.totalAmount)}</span></div>
                </div>
              </div>
              <div className="modal-footer-detail"><button className="btn-secondary" onClick={() => setSelectedOrder(null)}>Đóng</button></div>
            </div>
          </div>
        )}

        {/* Review Modal */}
        {showReview && (
          <div className="modal-overlay" onClick={() => setShowReview(false)}>
            <div className="review-modal" onClick={e => e.stopPropagation()}>
              <button className="close-btn" onClick={() => setShowReview(false)}>×</button>
              <h3>⭐ Đánh giá sản phẩm</h3>
              {reviewItem && (
                <div className="review-product"><img src={getImageUrl(reviewItem.productImage || reviewItem.product?.imageUrl)} alt="" /><span>{reviewItem.productName || reviewItem.product?.name}</span></div>
              )}
              <div className="stars-input" onMouseLeave={() => setHoverRating(0)}>
                {[1,2,3,4,5].map(s => (
                  <span key={s} className={s <= activeRating ? 'star active' : 'star'} onClick={() => setReviewRating(s)} onMouseEnter={() => setHoverRating(s)}>★</span>
                ))}
              </div>
              <div className="rating-label">{ratingLabels[activeRating]}</div>
              <textarea value={reviewComment} onChange={e => setReviewComment(e.target.value)} placeholder="Chia sẻ trải nghiệm..." rows={4} />
              <button className="btn-submit-review" onClick={submitReview} disabled={reviewSubmitting}>{reviewSubmitting ? '⏳ Đang gửi...' : '📩 Gửi đánh giá'}</button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
