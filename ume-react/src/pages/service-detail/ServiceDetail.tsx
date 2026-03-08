import { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { serviceApi, reviewApi } from '../../services/api';
import { getImageUrl } from '../../services/api';
import { useAuth } from '../../contexts/AuthContext';
import toast from 'react-hot-toast';
import './ServiceDetail.scss';

const formatPrice = (price: number) =>
  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);

const formatDuration = (minutes: number) => {
  if (!minutes) return 'N/A';
  if (minutes < 60) return `${minutes} phút`;
  const h = Math.floor(minutes / 60);
  const m = minutes % 60;
  return m > 0 ? `${h} giờ ${m} phút` : `${h} giờ`;
};

const getStars = (rating: number) =>
  Array(5).fill(0).map((_, i) => (i < Math.round(rating) ? 1 : 0));

export default function ServiceDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isLoggedIn } = useAuth();

  const [service, setService] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [reviews, setReviews] = useState<any[]>([]);
  const [reviewPage, setReviewPage] = useState(1);
  const [reviewTotalPages, setReviewTotalPages] = useState(1);

  useEffect(() => {
    if (id) {
      loadService(id);
      loadReviews(id, 1);
    }
  }, [id]);

  const loadService = async (serviceId: string) => {
    setLoading(true);
    try {
      const res = await serviceApi.getById(serviceId);
      setService(res.data?.data || res.data);
    } catch {
      toast.error('Không tìm thấy dịch vụ');
      navigate('/services');
    } finally {
      setLoading(false);
    }
  };

  const loadReviews = async (serviceId: string, page: number) => {
    try {
      const res = await reviewApi.getByService(serviceId, { page, limit: 5 });
      const data = res.data?.data;
      if (page === 1) setReviews(data?.reviews || []);
      else setReviews(prev => [...prev, ...(data?.reviews || [])]);
      setReviewPage(page);
      setReviewTotalPages(data?.pagination?.pages || 1);
    } catch {
      if (page === 1) setReviews([]);
    }
  };

  const bookService = () => {
    if (!isLoggedIn) {
      toast('Vui lòng đăng nhập để đặt dịch vụ', { icon: '⚠️' });
      navigate('/login');
      return;
    }
    navigate(`/booking?service=${service._id}`);
  };

  const shareService = (method: string) => {
    const url = window.location.href;
    switch (method) {
      case 'facebook':
        window.open(`https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(url)}`, '_blank', 'noopener,noreferrer');
        break;
      case 'copy':
        navigator.clipboard.writeText(url);
        toast.success('Đã sao chép link!');
        break;
    }
  };

  const getPetTypes = () => {
    if (!service?.petTypes) return 'N/A';
    return Array.isArray(service.petTypes) ? service.petTypes.join(', ') : service.petTypes;
  };

  if (loading) return (
    <div className="loading-state"><div className="spinner"></div><p>Đang tải thông tin dịch vụ...</p></div>
  );

  if (!service) return null;

  return (
    <div className="service-detail-page">
      <div className="page-header">
        <div className="container">
          <Link to="/services" className="back-link">← Quay lại</Link>
          <h1>{service.name}</h1>
        </div>
      </div>

      <div className="container">
        <div className="service-detail-content">
          {/* Image */}
          <div className="service-image-section">
            <div className="main-image">
              <img src={getImageUrl(service.imageUrl)} alt={service.name} onError={e => { (e.target as HTMLImageElement).src = '/assets/images/no-service.svg'; }} />
            </div>
          </div>

          {/* Info */}
          <div className="service-info-section">
            <div className="service-tags">
              {service.isHotDeal && <span className="tag hot">🔥 Ưu đãi</span>}
              {service.isFeatured && <span className="tag featured">⭐ Nổi bật</span>}
            </div>

            <h2 className="service-name">{service.name}</h2>

            <div className="service-rating">
              {getStars(service.averageRating || 0).map((s, i) => (
                <span key={i} className={s ? 'star filled' : 'star'}>★</span>
              ))}
              <span className="rating-value">
                {(service.averageRating || 0).toFixed(1)} ({service.totalReviews || 0} đánh giá)
              </span>
              <span className="bookings">| {service.totalBookings || 0} lượt đặt</span>
            </div>

            <p className="service-code">Mã dịch vụ: <strong>{service.code || 'N/A'}</strong></p>
            <p className="service-desc">{service.description}</p>

            <div className="service-price-section">
              <span className="price">{formatPrice(service.price)}</span>
            </div>

            <div className="service-details-grid">
              <div className="detail-item">
                <span className="icon">⏱️</span>
                <div><label>Thời gian</label><p>{formatDuration(service.durationMinutes)}</p></div>
              </div>
              <div className="detail-item">
                <span className="icon">🚫</span>
                <div><label>Phí hủy bỏ</label><p>{formatPrice(service.cancellationFee || 0)}</p></div>
              </div>
              <div className="detail-item">
                <span className="icon">🐾</span>
                <div><label>Phù hợp cho</label><p>{getPetTypes()}</p></div>
              </div>
              <div className="detail-item">
                <span className="icon">👥</span>
                <div><label>Nhân viên cần</label><p>{service.staffRequired || 1} người</p></div>
              </div>
            </div>

            <div className="action-buttons">
              <button className="btn-primary" onClick={bookService}>📅 Đặt lịch ngay</button>
              <button className="btn-secondary" onClick={bookService}>📞 Liên hệ tư vấn</button>
            </div>

            <div className="share-section">
              <label>Chia sẻ:</label>
              <button className="share-btn" onClick={() => shareService('facebook')} title="Facebook">📘</button>
              <button className="share-btn" onClick={() => shareService('copy')} title="Sao chép link">🔗</button>
            </div>
          </div>
        </div>

        {/* Reviews */}
        {reviews.length > 0 && (
          <div className="reviews-section">
            <h3>Đánh giá từ khách hàng</h3>
            <div className="reviews-list">
              {reviews.map(review => (
                <div className="review-item" key={review._id}>
                  <div className="review-header">
                    <div className="reviewer-info">
                      <div className="avatar">{(review.userId?.name || 'K')[0]}</div>
                      <div>
                        <p className="name">{review.userId?.name || 'Khách hàng'}</p>
                        <p className="date">{new Date(review.createdAt).toLocaleDateString('vi-VN')}</p>
                      </div>
                    </div>
                    <div className="review-rating">
                      {getStars(review.rating).map((s, i) => (
                        <span key={i} className={s ? 'star filled' : 'star'}>★</span>
                      ))}
                    </div>
                  </div>
                  <p className="review-text">{review.comment}</p>
                </div>
              ))}
            </div>
            {reviewPage < reviewTotalPages && (
              <button className="btn-load-more" onClick={() => loadReviews(id!, reviewPage + 1)}>
                Xem thêm đánh giá
              </button>
            )}
          </div>
        )}

        {reviews.length === 0 && !loading && (
          <div className="no-reviews">
            <p>💬 Chưa có đánh giá nào</p>
          </div>
        )}
      </div>
    </div>
  );
}
