import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { productApi, serviceApi, getImageUrl } from '../../services/api';
import './Home.scss';

const stats = [
  { icon: '⭐', value: '10+', label: 'Năm kinh nghiệm' },
  { icon: '👥', value: '50+', label: 'Bác sĩ & Nhân viên' },
  { icon: '❤️', value: '10K+', label: 'Thú cưng được chăm sóc' },
  { icon: '🏆', value: '100+', label: 'Dịch vụ' }
];

const whyChooseUs = [
  { icon: '🏆', title: 'Bác sĩ thú y giỏi', description: 'Đội ngũ bác sĩ thú y được đào tạo bài bản, nhiều năm kinh nghiệm' },
  { icon: '💎', title: 'Sản phẩm an toàn', description: 'Sử dụng sản phẩm chính hãng, an toàn cho thú cưng' },
  { icon: '🎨', title: 'Không gian thoáng mát', description: 'Thiết kế hiện đại, sạch sẽ, thoải mái cho thú cưng' },
  { icon: '❤️', title: 'Yêu thú cưng', description: 'Chăm sóc thú cưng với tình yêu thương như chính boss của mình' }
];

const defaultServices = [
  { name: 'Tắm gội & Vệ sinh', description: 'Dịch vụ tắm gội, vệ sinh tai, cắt móng cho thú cưng', price: '150.000đ', duration: '45 phút', image: 'https://images.unsplash.com/photo-1516734212186-a967f81ad0d7?auto=format&fit=crop&w=600&q=80' },
  { name: 'Cắt tỉa & Tạo kiểu lông', description: 'Cắt tỉa, tạo kiểu lông chuyên nghiệp theo nhiều phong cách', price: '300.000đ', duration: '90 phút', image: 'https://images.unsplash.com/photo-1591946614720-90a587da4a36?auto=format&fit=crop&w=600&q=80' },
  { name: 'Khách sạn thú cưng', description: 'Gửi thú cưng an toàn, thoải mái khi bạn đi công tác/du lịch', price: '200.000đ/ngày', duration: '24h', image: 'https://images.unsplash.com/photo-1601758228041-f3b2795255f1?auto=format&fit=crop&w=600&q=80' },
  { name: 'Khám sức khỏe thú y', description: 'Khám tổng quát, chẩn đoán bệnh và tư vấn sức khỏe cho thú cưng', price: '200.000đ', duration: '30 phút', image: 'https://images.unsplash.com/photo-1628009368231-7bb7cfcb0def?auto=format&fit=crop&w=600&q=80' },
  { name: 'Tiêm phòng vaccine', description: 'Tiêm phòng đầy đủ các loại vaccine cần thiết cho thú cưng', price: '150.000đ', duration: '15 phút', image: 'https://images.unsplash.com/photo-1587300003388-59208cc962cb?auto=format&fit=crop&w=600&q=80' },
  { name: 'Spa cao cấp', description: 'Trọn gói tắm, cắt tỉa, massage và chăm sóc đặc biệt', price: '400.000đ', duration: '120 phút', image: 'https://images.unsplash.com/photo-1548199973-03cce0bbc87b?auto=format&fit=crop&w=600&q=80' }
];

const defaultProducts = [
  { name: 'Hạt Royal Canin cho chó', price: '450.000đ', originalPrice: '560.000đ', discount: '-20%', image: 'https://images.unsplash.com/photo-1589924691995-400dc9ecc119?auto=format&fit=crop&w=400&q=80' },
  { name: 'Sữa tắm Bio cho mèo', price: '180.000đ', originalPrice: null, discount: null, image: 'https://images.unsplash.com/photo-1583337130417-3346a1be7dee?auto=format&fit=crop&w=400&q=80' },
  { name: 'Bóng đồ chơi cho chó', price: '85.000đ', originalPrice: '100.000đ', discount: '-15%', image: 'https://images.unsplash.com/photo-1535294435445-d7249524ef2e?auto=format&fit=crop&w=400&q=80' },
  { name: 'Vòng cổ thời trang', price: '120.000đ', originalPrice: null, discount: null, image: 'https://images.unsplash.com/photo-1567612529009-afe25813a308?auto=format&fit=crop&w=400&q=80' }
];

const testimonials = [
  { name: 'Ngọc Anh', title: 'Khách hàng thân thiết', text: 'Dịch vụ tuyệt vời! Bé cún nhà mình được chăm sóc rất tốt. Nhân viên rất yêu thú cưng và nhiệt tình. Chắc chắn sẽ quay lại!', avatar: 'https://randomuser.me/api/portraits/women/44.jpg', stars: 5 },
  { name: 'Minh Tuấn', title: 'Doanh nhân', text: 'Không gian spa thú cưng rất sạch sẽ và thoáng mát. Bé mèo nhà mình được tắm gội, cắt lông rất đẹp. Highly recommend!', avatar: 'https://randomuser.me/api/portraits/men/32.jpg', stars: 5 },
  { name: 'Thu Hà', title: 'Giáo viên', text: 'Gửi boss ở khách sạn thú cưng khi đi du lịch, được cập nhật hình ảnh hàng ngày. Rất yên tâm. Cảm ơn PetCare nhiều!', avatar: 'https://randomuser.me/api/portraits/women/68.jpg', stars: 4.5 }
];

function formatPrice(price: number): string {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
}

export default function Home() {
  const [featuredServices, setFeaturedServices] = useState<any[]>([]);
  const [featuredProducts, setFeaturedProducts] = useState<any[]>([]);

  useEffect(() => {
    serviceApi.getServices({ limit: 6, isFeatured: true }).then(res => {
      setFeaturedServices(res.data?.data?.items || res.data?.data || []);
    }).catch(() => {});

    productApi.getProducts({ limit: 8, isFeatured: true }).then(res => {
      const d = res.data?.data;
      setFeaturedProducts(d?.products || d?.items || (Array.isArray(d) ? d : []));
    }).catch(() => {});
  }, []);

  return (
    <>
      {/* Hero */}
      <section className="hero-section">
        <div className="container">
          <div className="hero-content">
            <h1>Yêu thương <span>boss</span> của bạn</h1>
            <p>Dịch vụ chăm sóc thú cưng chuyên nghiệp với đội ngũ yêu động vật. Chúng tôi mang đến những trải nghiệm tốt nhất cho thú cưng của bạn.</p>
            <div className="hero-buttons">
              <Link to="/booking" className="btn-primary-custom">📅 Đặt lịch chăm sóc</Link>
              <Link to="/services" className="btn-outline-light-custom">📋 Xem dịch vụ</Link>
            </div>
          </div>
        </div>
      </section>

      {/* Stats */}
      <section className="stats-section">
        <div className="container">
          <div className="stats-grid">
            {stats.map((stat, i) => (
              <div className="stat-item" key={i}>
                <span className="number">{stat.value}</span>
                <p className="label">{stat.label}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Featured Services */}
      <section className="section-padding">
        <div className="container">
          <div className="section-title">
            <h2>Dịch vụ nổi bật</h2>
            <div className="divider"></div>
            <p>Chúng tôi mang đến những dịch vụ chăm sóc thú cưng chuyên nghiệp với chất lượng hàng đầu</p>
          </div>

          <div className="grid-3">
            {(featuredServices.length > 0 ? featuredServices : []).map((service, i) => (
              <div className="service-card" key={service._id || i}>
                <img src={getImageUrl(service.imageUrl)} alt={service.name} onError={(e) => { (e.target as HTMLImageElement).src = '/assets/images/no-service.svg'; }} />
                <div className="card-body">
                  <h5>{service.name}</h5>
                  <p className="text-muted">{service.shortDescription || service.description || 'Dịch vụ chăm sóc thú cưng chuyên nghiệp'}</p>
                  <div className="card-footer-row">
                    <span className="price">{formatPrice(service.price || service.basePrice || 0)}</span>
                    <span className="duration">🕐 {service.durationMinutes || 30} phút</span>
                  </div>
                </div>
              </div>
            ))}
            {featuredServices.length === 0 && defaultServices.map((svc, i) => (
              <div className="service-card" key={i}>
                <img src={svc.image} alt={svc.name} />
                <div className="card-body">
                  <h5>{svc.name}</h5>
                  <p className="text-muted">{svc.description}</p>
                  <div className="card-footer-row">
                    <span className="price">{svc.price}</span>
                    <span className="duration">🕐 {svc.duration}</span>
                  </div>
                </div>
              </div>
            ))}
          </div>

          <div className="text-center" style={{ marginTop: '2.5rem' }}>
            <Link to="/services" className="btn-primary-custom">Xem tất cả dịch vụ →</Link>
          </div>
        </div>
      </section>

      {/* Why Choose Us */}
      <section className="why-us-section section-padding">
        <div className="container">
          <div className="section-title">
            <h2>Tại sao chọn PetCare?</h2>
            <div className="divider"></div>
            <p>Chúng tôi cam kết mang đến trải nghiệm chăm sóc thú cưng tốt nhất cho bạn</p>
          </div>

          <div className="grid-4">
            {whyChooseUs.map((item, i) => (
              <div className="feature-box" key={i}>
                <div className="icon">{item.icon}</div>
                <h4>{item.title}</h4>
                <p>{item.description}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Featured Products */}
      <section className="section-padding">
        <div className="container">
          <div className="section-title">
            <h2>Sản phẩm nổi bật</h2>
            <div className="divider"></div>
            <p>Khám phá các sản phẩm chăm sóc thú cưng chất lượng cao từ những thương hiệu uy tín</p>
          </div>

          <div className="grid-4">
            {(featuredProducts.length > 0 ? featuredProducts : []).map((product, i) => (
              <Link to={`/products/${product._id}`} className="product-card-link" key={product._id || i}>
                <div className="product-card">
                  <div className="product-image">
                    <img src={getImageUrl(product.imageUrl || product.mainImage)} alt={product.name} loading="lazy" onError={(e) => { (e.target as HTMLImageElement).src = '/assets/images/no-image.svg'; }} />
                    {product.originalPrice && product.originalPrice > product.price && (
                      <span className="badge-sale">-{Math.round((1 - product.price / product.originalPrice) * 100)}%</span>
                    )}
                  </div>
                  <div className="card-body">
                    <h5>{product.name}</h5>
                    <div>
                      <span className="price">{formatPrice(product.price)}</span>
                      {product.originalPrice && product.originalPrice > product.price && (
                        <span className="original-price">{formatPrice(product.originalPrice)}</span>
                      )}
                    </div>
                  </div>
                </div>
              </Link>
            ))}
            {featuredProducts.length === 0 && defaultProducts.map((prod, i) => (
              <div className="product-card" key={i}>
                <div className="product-image">
                  <img src={prod.image} alt={prod.name} />
                  {prod.discount && <span className="badge-sale">{prod.discount}</span>}
                </div>
                <div className="card-body">
                  <h5>{prod.name}</h5>
                  <div>
                    <span className="price">{prod.price}</span>
                    {prod.originalPrice && <span className="original-price">{prod.originalPrice}</span>}
                  </div>
                </div>
              </div>
            ))}
          </div>

          <div className="text-center" style={{ marginTop: '2.5rem' }}>
            <Link to="/products" className="btn-primary-custom">Xem tất cả sản phẩm →</Link>
          </div>
        </div>
      </section>

      {/* Testimonials */}
      <section className="testimonials-section section-padding">
        <div className="container">
          <div className="section-title">
            <h2>Khách hàng nói gì?</h2>
            <div className="divider"></div>
            <p>Những đánh giá chân thực từ khách hàng đã sử dụng dịch vụ của chúng tôi</p>
          </div>

          <div className="grid-3">
            {testimonials.map((review, i) => (
              <div className="testimonial-card" key={i}>
                <div className="quote-icon">❝</div>
                <div className="rating">
                  {Array.from({ length: Math.floor(review.stars) }, (_, j) => <span key={j}>⭐</span>)}
                </div>
                <p className="content">"{review.text}"</p>
                <div className="author">
                  <img src={review.avatar} alt={review.name} />
                  <div>
                    <div className="author-name">{review.name}</div>
                    <div className="author-title">{review.title}</div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="cta-section">
        <div className="container">
          <h2>Sẵn sàng chăm sóc boss?</h2>
          <p>Đặt lịch ngay hôm nay để trải nghiệm dịch vụ chăm sóc thú cưng chuyên nghiệp tại PetCare</p>
          <Link to="/booking" className="btn-dark-custom">📅 Đặt lịch ngay</Link>
        </div>
      </section>
    </>
  );
}
