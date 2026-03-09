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
              {settings.facebook && <a href={settings.facebook} target="_blank" rel="noopener noreferrer" className="social-fb" title="Facebook"><svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor"><path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"/></svg></a>}
              {settings.instagram && <a href={settings.instagram} target="_blank" rel="noopener noreferrer" className="social-ig" title="Instagram"><svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor"><path d="M12 2.163c3.204 0 3.584.012 4.85.07 3.252.148 4.771 1.691 4.919 4.919.058 1.265.069 1.645.069 4.849 0 3.205-.012 3.584-.069 4.849-.149 3.225-1.664 4.771-4.919 4.919-1.266.058-1.644.07-4.85.07-3.204 0-3.584-.012-4.849-.07-3.26-.149-4.771-1.699-4.919-4.92-.058-1.265-.07-1.644-.07-4.849 0-3.204.013-3.583.07-4.849.149-3.227 1.664-4.771 4.919-4.919 1.266-.057 1.645-.069 4.849-.069zM12 0C8.741 0 8.333.014 7.053.072 2.695.272.273 2.69.073 7.052.014 8.333 0 8.741 0 12c0 3.259.014 3.668.072 4.948.2 4.358 2.618 6.78 6.98 6.98C8.333 23.986 8.741 24 12 24c3.259 0 3.668-.014 4.948-.072 4.354-.2 6.782-2.618 6.979-6.98.059-1.28.073-1.689.073-4.948 0-3.259-.014-3.667-.072-4.947-.196-4.354-2.617-6.78-6.979-6.98C15.668.014 15.259 0 12 0zm0 5.838a6.162 6.162 0 100 12.324 6.162 6.162 0 000-12.324zM12 16a4 4 0 110-8 4 4 0 010 8zm6.406-11.845a1.44 1.44 0 100 2.881 1.44 1.44 0 000-2.881z"/></svg></a>}
              {settings.tiktok && <a href={settings.tiktok} target="_blank" rel="noopener noreferrer" className="social-tt" title="TikTok"><svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor"><path d="M12.525.02c1.31-.02 2.61-.01 3.91-.02.08 1.53.63 3.09 1.75 4.17 1.12 1.11 2.7 1.62 4.24 1.79v4.03c-1.44-.05-2.89-.35-4.2-.97-.57-.26-1.1-.59-1.62-.93-.01 2.92.01 5.84-.02 8.75-.08 1.4-.54 2.79-1.35 3.94-1.31 1.92-3.58 3.17-5.91 3.21-1.43.08-2.86-.31-4.08-1.03-2.02-1.19-3.44-3.37-3.65-5.71-.02-.5-.03-1-.01-1.49.18-1.9 1.12-3.72 2.58-4.96 1.66-1.44 3.98-2.13 6.15-1.72.02 1.48-.04 2.96-.04 4.44-.99-.32-2.15-.23-3.02.37-.63.41-1.11 1.04-1.36 1.75-.21.51-.15 1.07-.14 1.61.24 1.64 1.82 3.02 3.5 2.87 1.12-.01 2.19-.66 2.77-1.61.19-.33.4-.67.41-1.06.1-1.79.06-3.57.07-5.36.01-4.03-.01-8.05.02-12.07z"/></svg></a>}
              {settings.youtube && <a href={settings.youtube} target="_blank" rel="noopener noreferrer" className="social-yt" title="YouTube"><svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor"><path d="M23.498 6.186a3.016 3.016 0 00-2.122-2.136C19.505 3.545 12 3.545 12 3.545s-7.505 0-9.377.505A3.017 3.017 0 00.502 6.186C0 8.07 0 12 0 12s0 3.93.502 5.814a3.016 3.016 0 002.122 2.136c1.871.505 9.376.505 9.376.505s7.505 0 9.377-.505a3.015 3.015 0 002.122-2.136C24 15.93 24 12 24 12s0-3.93-.502-5.814zM9.545 15.568V8.432L15.818 12l-6.273 3.568z"/></svg></a>}
              {settings.zalo && <a href={settings.zalo} target="_blank" rel="noopener noreferrer" className="social-zl" title="Zalo"><svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor"><path d="M12 0C5.373 0 0 5.373 0 12s5.373 12 12 12 12-5.373 12-12S18.627 0 12 0zm5.568 14.163c-.165.245-.635.48-1.248.592-.612.113-2.15.06-3.328-.492-1.178-.553-2.163-1.35-2.93-2.37-.767-1.02-1.308-2.257-1.398-3.165-.09-.908.165-1.53.472-1.905.307-.375.72-.502 1.013-.487.292.015.532.03.765.577.233.547.78 1.905.85 2.04.07.135.117.293.023.472-.095.18-.142.292-.28.45-.14.157-.293.352-.42.472-.14.127-.285.267-.122.524.163.257.723 1.193 1.553 1.933 1.065.95 1.963 1.245 2.243 1.382.28.135.443.112.607-.068.163-.18.7-.817.887-1.097.187-.28.375-.233.632-.14.257.092 1.632.77 1.912.91.28.14.467.21.535.325.068.117.068.672-.097.917z"/></svg></a>}
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
