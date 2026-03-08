import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { settingsApi, serviceApi } from '../../services/api';
import './Footer.scss';

export default function Footer() {
  const [settings, setSettings] = useState<any>({});
  const [serviceCategories, setServiceCategories] = useState<any[]>([]);

  useEffect(() => {
    settingsApi.getSettings().then(res => setSettings(res.data?.data || res.data)).catch(() => {});
    serviceApi.getServiceCategories().then(res => {
      const items = res.data?.data?.items || res.data?.data || res.data || [];
      setServiceCategories(items.slice(0, 5));
    }).catch(() => {});
  }, []);

  return (
    <footer className="site-footer">
      <div className="container">
        <div className="footer-grid">
          <div className="footer-col">
            <h5 className="footer-title">🐾 {settings.storeName || 'PetCare'}</h5>
            <p className="footer-about">
              {settings.storeDescription || 'Dịch vụ chăm sóc thú cưng chuyên nghiệp với đội ngũ yêu thương động vật. Chúng tôi cam kết mang đến trải nghiệm tốt nhất cho boss của bạn.'}
            </p>
            <div className="footer-social">
              {settings.facebook && <a href={settings.facebook} target="_blank" rel="noopener noreferrer">📘</a>}
              {settings.instagram && <a href={settings.instagram} target="_blank" rel="noopener noreferrer">📷</a>}
              {settings.tiktok && <a href={settings.tiktok} target="_blank" rel="noopener noreferrer">🎵</a>}
              {settings.youtube && <a href={settings.youtube} target="_blank" rel="noopener noreferrer">📺</a>}
              {settings.zalo && <a href={settings.zalo} target="_blank" rel="noopener noreferrer">💬</a>}
            </div>
          </div>

          <div className="footer-col">
            <h5 className="footer-title">Liên kết nhanh</h5>
            <ul className="footer-links">
              <li><Link to="/">Trang chủ</Link></li>
              <li><Link to="/services">Dịch vụ</Link></li>
              <li><Link to="/products">Sản phẩm</Link></li>
              <li><Link to="/pets">Thú cưng</Link></li>
              <li><Link to="/booking">Đặt lịch</Link></li>
              <li><Link to="/contact">Liên hệ</Link></li>
            </ul>
          </div>

          <div className="footer-col">
            <h5 className="footer-title">Dịch vụ</h5>
            <ul className="footer-links">
              {serviceCategories.map(cat => (
                <li key={cat._id}>
                  <Link to={`/services?category=${cat._id}`}>{cat.name}</Link>
                </li>
              ))}
              {serviceCategories.length === 0 && (
                <li><Link to="/services">Xem tất cả dịch vụ</Link></li>
              )}
            </ul>
          </div>

          <div className="footer-col">
            <h5 className="footer-title">Liên hệ</h5>
            <ul className="footer-contact">
              {settings.address && (
                <li>📍 <span>{settings.address}</span></li>
              )}
              {settings.phone && (
                <li>📞 <a href={`tel:${settings.phone}`}>{settings.phone}</a></li>
              )}
              {settings.email && (
                <li>📧 <a href={`mailto:${settings.email}`}>{settings.email}</a></li>
              )}
              {settings.workingHours && (
                <li>🕐 <span>{settings.workingHours}</span></li>
              )}
            </ul>
          </div>
        </div>

        <div className="footer-bottom">
          <p>&copy; 2025 {settings.storeName || 'PetCare'}. All rights reserved.</p>
        </div>
      </div>
    </footer>
  );
}
