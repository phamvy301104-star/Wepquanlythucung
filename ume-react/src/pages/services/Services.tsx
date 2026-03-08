import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { serviceApi } from '../../services/api';
import { getImageUrl } from '../../services/api';
import './Services.scss';

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

export default function Services() {
  const navigate = useNavigate();
  const [services, setServices] = useState<any[]>([]);
  const [categories, setCategories] = useState<any[]>([]);
  const [selectedCategory, setSelectedCategory] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadServices();
    loadCategories();
  }, []);

  const loadServices = async (categoryId?: string) => {
    setLoading(true);
    try {
      const params: any = { limit: 100 };
      if (categoryId) params.category = categoryId;
      const res = await serviceApi.getAll(params);
      setServices(res.data?.data?.services || res.data?.data || []);
    } catch {
      setServices([]);
    } finally {
      setLoading(false);
    }
  };

  const loadCategories = async () => {
    try {
      const res = await serviceApi.getCategories();
      setCategories(res.data?.data?.categories || res.data?.data || []);
    } catch {}
  };

  const filterByCategory = (catId: string) => {
    setSelectedCategory(catId);
    loadServices(catId || undefined);
  };

  const bookService = (serviceId: string) => {
    navigate(`/booking?service=${serviceId}`);
  };

  return (
    <div className="services-page">
      <div className="page-header">
        <div className="container">
          <h1>✂️ Dịch vụ của chúng tôi</h1>
          <p>Chăm sóc thú cưng chuyên nghiệp với đội ngũ giàu kinh nghiệm</p>
        </div>
      </div>

      <div className="container">
        {/* Category Tabs */}
        <div className="category-tabs">
          <button className={!selectedCategory ? 'active' : ''} onClick={() => filterByCategory('')}>
            Tất cả
          </button>
          {categories.map(cat => (
            <button key={cat._id} className={selectedCategory === cat._id ? 'active' : ''} onClick={() => filterByCategory(cat._id)}>
              {cat.name}
            </button>
          ))}
        </div>

        {loading ? (
          <div className="loading-state"><div className="spinner"></div><p>Đang tải dịch vụ...</p></div>
        ) : services.length === 0 ? (
          <div className="empty-state">
            <span className="icon">🐾</span>
            <p>Không tìm thấy dịch vụ nào</p>
          </div>
        ) : (
          <div className="services-grid">
            {services.map(service => (
              <div className="service-card" key={service._id}>
                <Link to={`/services/${service._id}`} className="service-image">
                  <img src={getImageUrl(service.imageUrl)} alt={service.name} onError={e => { (e.target as HTMLImageElement).src = '/assets/images/no-service.svg'; }} />
                  {service.isHotDeal && <span className="hot-badge">🔥 Ưu đãi</span>}
                  {service.isFeatured && <span className="featured-badge">⭐</span>}
                </Link>
                <div className="service-info">
                  <Link to={`/services/${service._id}`} className="service-name">{service.name}</Link>
                  <p className="service-desc">{service.description}</p>
                  <div className="service-meta">
                    <span className="duration">⏱️ {formatDuration(service.durationMinutes)}</span>
                    <div className="rating">
                      {getStars(service.averageRating || 0).map((s, i) => (
                        <span key={i} className={s ? 'star filled' : 'star'}>★</span>
                      ))}
                      <span className="count">({service.totalReviews || 0})</span>
                    </div>
                  </div>
                  <div className="service-footer">
                    <span className="price">{formatPrice(service.price)}</span>
                    <button className="btn-book" onClick={() => bookService(service._id)}>Đặt lịch</button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
