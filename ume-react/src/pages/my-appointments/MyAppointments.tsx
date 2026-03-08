import { useState, useEffect } from 'react';
import { appointmentApi, reviewApi } from '../../services/api';
import api from '../../services/api';
import toast from 'react-hot-toast';
import './MyAppointments.scss';

const formatPrice = (p: number) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p);
const formatDate = (d: string) => new Date(d).toLocaleDateString('vi-VN', { year: 'numeric', month: '2-digit', day: '2-digit' });

const statuses = [
  { value: '', label: 'Tất cả' }, { value: 'Pending', label: 'Chờ xác nhận' },
  { value: 'Confirmed', label: 'Đã xác nhận' }, { value: 'InProgress', label: 'Đang thực hiện' },
  { value: 'Completed', label: 'Hoàn thành' }, { value: 'Cancelled', label: 'Đã hủy' },
];
const statusLabels: any = { Pending: 'Chờ xác nhận', Confirmed: 'Đã xác nhận', InProgress: 'Đang thực hiện', Completed: 'Hoàn thành', Cancelled: 'Đã hủy' };
const ratingLabels: any = { 1: 'Rất không hài lòng', 2: 'Không hài lòng', 3: 'Bình thường', 4: 'Hài lòng', 5: 'Rất hài lòng' };

export default function MyAppointments() {
  const [appointments, setAppointments] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [selectedStatus, setSelectedStatus] = useState('');
  const [cancellingId, setCancellingId] = useState<string | null>(null);
  const [cancelReason, setCancelReason] = useState('');

  // Review
  const [showReview, setShowReview] = useState(false);
  const [reviewApt, setReviewApt] = useState<any>(null);
  const [reviewRating, setReviewRating] = useState(5);
  const [reviewComment, setReviewComment] = useState('');
  const [reviewSubmitting, setReviewSubmitting] = useState(false);
  const [hoverRating, setHoverRating] = useState(0);
  const [reviewedMap, setReviewedMap] = useState<Record<string, boolean>>({});

  useEffect(() => { loadAppointments(); }, []);

  const loadAppointments = async (status?: string) => {
    setLoading(true);
    try {
      const params: any = {};
      const s = status ?? selectedStatus;
      if (s) params.status = s;
      const r = await appointmentApi.getMyAppointments(params);
      const list = r.data?.data?.appointments || r.data?.data?.items || [];
      setAppointments(list);
      list.forEach((a: any) => { if (a.status === 'Completed') checkReviewed(a._id); });
    } catch {} finally { setLoading(false); }
  };

  const checkReviewed = async (id: string) => {
    try {
      const r = await api.get(`/reviews/check/${id}`);
      setReviewedMap(m => ({ ...m, [id]: r.data?.data?.reviewed || false }));
    } catch {}
  };

  const filterByStatus = (s: string) => { setSelectedStatus(s); loadAppointments(s); };

  const confirmCancel = async () => {
    if (!cancellingId) return;
    try {
      await appointmentApi.cancel(cancellingId, { cancelReason });
      toast.success('Đã hủy lịch hẹn thành công');
      setCancellingId(null);
      loadAppointments();
    } catch (err: any) { toast.error(err.response?.data?.message || 'Hủy thất bại'); }
  };

  const openReview = (apt: any) => {
    setReviewApt(apt); setReviewRating(5); setReviewComment(''); setHoverRating(0); setShowReview(true);
  };

  const submitReview = async () => {
    if (!reviewApt) return;
    setReviewSubmitting(true);
    try {
      const fd = new FormData();
      fd.append('appointmentId', reviewApt._id);
      fd.append('rating', reviewRating.toString());
      fd.append('comment', reviewComment);
      if (reviewApt.services?.length > 0) {
        const sid = reviewApt.services[0].service?._id || reviewApt.services[0].service;
        if (sid) fd.append('serviceId', sid);
      }
      await reviewApi.create(fd);
      toast.success('Cảm ơn bạn đã đánh giá!');
      setReviewedMap(m => ({ ...m, [reviewApt._id]: true }));
      setShowReview(false);
    } catch (err: any) { toast.error(err.response?.data?.message || 'Gửi đánh giá thất bại'); }
    finally { setReviewSubmitting(false); }
  };

  const activeRating = hoverRating || reviewRating;

  return (
    <div className="my-appointments-page">
      <div className="page-header"><div className="container"><h1>📅 Lịch hẹn của tôi</h1></div></div>
      <div className="container">
        <div className="status-tabs">
          {statuses.map(s => (
            <button key={s.value} className={selectedStatus === s.value ? 'active' : ''} onClick={() => filterByStatus(s.value)}>{s.label}</button>
          ))}
        </div>

        {loading && <div className="loading-center">⏳ Đang tải...</div>}
        {!loading && appointments.length === 0 && <div className="empty-state">📅<p>Bạn chưa có lịch hẹn nào</p></div>}

        <div className="appointments-list">
          {appointments.map(apt => (
            <div key={apt._id} className="appointment-card">
              <div className="card-header">
                <div className="apt-code"># {apt.appointmentCode}</div>
                <span className={`status-badge ${apt.status?.toLowerCase()}`}>{statusLabels[apt.status] || apt.status}</span>
              </div>
              <div className="card-body">
                <div className="info-row">📅 {formatDate(apt.appointmentDate)} lúc {apt.startTime}</div>
                {apt.staff && <div className="info-row">👤 {apt.staff.nickName || apt.staff.fullName}</div>}
                {apt.pet && <div className="info-row">🐾 {apt.pet.name}</div>}
                <div className="services-tags">
                  {(apt.services || []).map((s: any, i: number) => <span key={i} className="service-tag">{s.serviceName || s.service?.name}</span>)}
                </div>
              </div>
              <div className="card-footer">
                <span className="total-amount">{formatPrice(apt.finalAmount || apt.totalAmount)}</span>
                <div className="footer-actions">
                  {apt.status === 'Completed' && !reviewedMap[apt._id] && <button className="btn-review" onClick={() => openReview(apt)}>⭐ Đánh giá</button>}
                  {apt.status === 'Completed' && reviewedMap[apt._id] && <span className="reviewed-badge">✅ Đã đánh giá</span>}
                  {apt.status === 'Pending' && <button className="btn-cancel" onClick={() => { setCancellingId(apt._id); setCancelReason(''); }}>❌ Hủy lịch</button>}
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* Cancel Modal */}
        {cancellingId && (
          <div className="modal-overlay" onClick={() => setCancellingId(null)}>
            <div className="modal-content" onClick={e => e.stopPropagation()}>
              <h3>Hủy lịch hẹn</h3><p>Bạn có chắc muốn hủy lịch hẹn này?</p>
              <textarea value={cancelReason} onChange={e => setCancelReason(e.target.value)} placeholder="Lý do hủy..." rows={3} />
              <div className="modal-actions"><button className="btn-secondary" onClick={() => setCancellingId(null)}>Đóng</button><button className="btn-danger" onClick={confirmCancel}>Xác nhận hủy</button></div>
            </div>
          </div>
        )}

        {/* Review Modal */}
        {showReview && (
          <div className="modal-overlay" onClick={() => setShowReview(false)}>
            <div className="review-modal" onClick={e => e.stopPropagation()}>
              <button className="close-btn" onClick={() => setShowReview(false)}>×</button>
              <h3>⭐ Đánh giá dịch vụ</h3>
              {reviewApt && <p className="review-code">{reviewApt.appointmentCode}</p>}
              {reviewApt?.services?.length > 0 && <div className="review-services">{reviewApt.services.map((s: any, i: number) => <span key={i} className="service-tag">{s.serviceName || s.service?.name}</span>)}</div>}
              <div className="stars-input" onMouseLeave={() => setHoverRating(0)}>
                {[1,2,3,4,5].map(s => <span key={s} className={s <= activeRating ? 'star active' : 'star'} onClick={() => setReviewRating(s)} onMouseEnter={() => setHoverRating(s)}>★</span>)}
              </div>
              <div className="rating-label">{ratingLabels[activeRating]}</div>
              <textarea value={reviewComment} onChange={e => setReviewComment(e.target.value)} placeholder="Chia sẻ trải nghiệm..." rows={4} />
              <div className="review-actions"><button className="btn-secondary" onClick={() => setShowReview(false)}>Hủy</button><button className="btn-submit" onClick={submitReview} disabled={reviewSubmitting}>{reviewSubmitting ? '⏳ Đang gửi...' : '📩 Gửi đánh giá'}</button></div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
