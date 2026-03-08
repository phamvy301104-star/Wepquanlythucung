import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './CheckoutSuccess.scss';

const formatPrice = (price: number) =>
  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);

export default function CheckoutSuccess() {
  const navigate = useNavigate();
  const [order, setOrder] = useState<any>(null);

  useEffect(() => {
    const data = sessionStorage.getItem('newOrder');
    if (!data) { navigate('/'); return; }
    setOrder(JSON.parse(data));
  }, []);

  if (!order) return null;

  return (
    <div className="success-container">
      <div className="success-header">
        <div className="success-icon">✅</div>
        <h1>Đặt hàng thành công!</h1>
        <p>Cảm ơn bạn đã đặt hàng. Chúng tôi sẽ liên hệ xác nhận sớm nhất.</p>
      </div>

      <div className="success-content">
        <div className="info-section">
          <h3>🧾 Chi tiết đơn hàng</h3>
          <div className="info-grid">
            <div className="info-item">
              <label>Mã đơn hàng</label>
              <p className="order-id">{order.code}</p>
            </div>
            <div className="info-item">
              <label>Tổng thanh toán</label>
              <p className="order-total">{formatPrice(order.total)}</p>
            </div>
          </div>
        </div>

        {order.items?.length > 0 && (
          <div className="items-section">
            <h3>📦 Danh sách sản phẩm</h3>
            <div className="items-list">
              {order.items.map((item: any, i: number) => (
                <div className="item-row" key={i}>
                  <div className="item-name">
                    {item.productName || item.petName}
                    <span className="qty">SL: {item.quantity || 1}</span>
                  </div>
                  <div className="item-price">{formatPrice(item.subtotal || item.price)}</div>
                </div>
              ))}
            </div>
          </div>
        )}

        {order.shippingAddress && (
          <div className="shipping-section">
            <h3>🚚 Thông tin giao hàng</h3>
            <div className="shipping-details">
              <div className="shipping-row"><span>Người nhận</span><span>{order.shippingAddress.fullName}</span></div>
              <div className="shipping-row"><span>Số điện thoại</span><span>{order.shippingAddress.phone}</span></div>
              <div className="shipping-row">
                <span>Địa chỉ</span>
                <span>{[order.shippingAddress.address, order.shippingAddress.ward, order.shippingAddress.district, order.shippingAddress.city].filter(Boolean).join(', ')}</span>
              </div>
              {order.paymentMethod && (
                <div className="shipping-row">
                  <span>Thanh toán</span>
                  <span>{order.paymentMethod === 'COD' ? '💵 Thanh toán khi nhận hàng' : '🏦 Chuyển khoản ngân hàng'}</span>
                </div>
              )}
            </div>
          </div>
        )}

        <div className="action-buttons">
          <button className="btn-continue" onClick={() => { sessionStorage.removeItem('newOrder'); navigate('/products'); }}>
            🛍️ Tiếp tục mua sắm
          </button>
          <button className="btn-home" onClick={() => { sessionStorage.removeItem('newOrder'); navigate('/'); }}>
            🏠 Về trang chủ
          </button>
        </div>
      </div>
    </div>
  );
}
