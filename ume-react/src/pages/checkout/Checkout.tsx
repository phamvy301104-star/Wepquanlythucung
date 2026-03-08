import { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useCart } from '../../contexts/CartContext';
import { useAuth } from '../../contexts/AuthContext';
import { orderApi, settingsApi, promotionApi } from '../../services/api';
import { getImageUrl } from '../../services/api';
import toast from 'react-hot-toast';
import './Checkout.scss';

const formatPrice = (price: number) =>
  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);

export default function Checkout() {
  const navigate = useNavigate();
  const { items, totalAmount, clearCart } = useCart();
  const { user } = useAuth();

  const [settings, setSettings] = useState<any>({});
  const [shippingMethod, setShippingMethod] = useState('standard');
  const [shippingFee, setShippingFee] = useState(30000);
  const [paymentMethod, setPaymentMethod] = useState('COD');
  const [notes, setNotes] = useState('');
  const [promoCode, setPromoCode] = useState('');
  const [discount, setDiscount] = useState(0);
  const [promoApplied, setPromoApplied] = useState(false);
  const [promoName, setPromoName] = useState('');
  const [applyingPromo, setApplyingPromo] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  const [shippingAddress, setShippingAddress] = useState({
    fullName: '', phone: '', email: '', address: '', ward: '', district: '', city: ''
  });

  useEffect(() => {
    if (items.length === 0) { navigate('/cart'); return; }
    settingsApi.get().then(res => {
      const s = res.data?.data || res.data;
      setSettings(s);
      setShippingFee(s.shippingStandardPrice || 30000);
      if (s.codEnabled === false && s.bankTransferEnabled !== false) setPaymentMethod('BankTransfer');
    }).catch(() => {});
    if (user) {
      setShippingAddress({
        fullName: user.fullName || '', phone: user.phoneNumber || '', email: user.email || '',
        address: user.address?.street || '', ward: user.address?.ward || '',
        district: user.address?.district || '', city: user.address?.city || ''
      });
    }
  }, []);

  const standardPrice = settings.shippingStandardPrice || 30000;
  const expressPrice = settings.shippingExpressPrice || 50000;
  const total = totalAmount + shippingFee - discount;

  const selectShipping = (method: string) => {
    setShippingMethod(method);
    setShippingFee(method === 'express' ? expressPrice : standardPrice);
  };

  const applyPromo = async () => {
    if (!promoCode.trim()) { toast.error('Vui lòng nhập mã khuyến mãi'); return; }
    setApplyingPromo(true);
    try {
      const res = await promotionApi.validate(promoCode, totalAmount);
      if (res.data?.success) {
        const d = res.data.data;
        if (d.promotion?.type === 'FreeShipping') {
          setDiscount(shippingFee);
          toast.success('Áp dụng miễn phí vận chuyển thành công!');
        } else {
          setDiscount(d.discount || 0);
          toast.success(`Giảm ${formatPrice(d.discount || 0)}`);
        }
        setPromoApplied(true);
        setPromoName(d.promotion?.name || promoCode);
      }
    } catch (err: any) {
      setDiscount(0); setPromoApplied(false);
      toast.error(err.response?.data?.message || 'Mã không hợp lệ');
    } finally { setApplyingPromo(false); }
  };

  const removePromo = () => {
    setPromoCode(''); setDiscount(0); setPromoApplied(false); setPromoName('');
    toast('Đã hủy mã khuyến mãi', { icon: 'ℹ️' });
  };

  const getItemPrice = (item: any) =>
    item.itemType === 'pet' ? (item.product.listingPrice || 0) : item.product.price;

  const isFormValid = () => !!(shippingAddress.fullName && shippingAddress.phone && shippingAddress.email && shippingAddress.address);

  const placeOrder = async () => {
    if (!isFormValid()) { toast.error('Vui lòng điền đầy đủ thông tin giao hàng'); return; }
    setSubmitting(true);
    try {
      const orderData: any = {
        items: items.map(item => item.itemType === 'pet'
          ? { petId: item.product._id, quantity: 1 }
          : { productId: item.product._id, quantity: item.quantity }
        ),
        shippingAddress, shippingFee, paymentMethod, notes
      };
      if (promoApplied && promoCode) orderData.promotionCode = promoCode;
      const res = await orderApi.create(orderData);
      const order = res.data?.data || res.data;
      clearCart();
      sessionStorage.setItem('newOrder', JSON.stringify({
        id: order._id, code: order.orderCode, total: order.total,
        items: order.items, shippingAddress: order.shippingAddress, paymentMethod: order.paymentMethod
      }));
      navigate('/checkout-success');
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Đặt hàng thất bại');
    } finally { setSubmitting(false); }
  };

  const updateAddress = (field: string, value: string) => {
    setShippingAddress(prev => ({ ...prev, [field]: value }));
  };

  return (
    <div className="checkout-page">
      <div className="page-header">
        <div className="container"><h1>💳 Thanh toán</h1></div>
      </div>

      <div className="container">
        <div className="checkout-layout">
          <div className="checkout-form">
            {/* Shipping Info */}
            <div className="form-section">
              <h3>📍 Thông tin giao hàng</h3>
              <div className="form-grid">
                <div className="form-group full">
                  <label>Họ tên *</label>
                  <input type="text" value={shippingAddress.fullName} onChange={e => updateAddress('fullName', e.target.value)} placeholder="Nhập họ tên" />
                </div>
                <div className="form-group">
                  <label>Số điện thoại *</label>
                  <input type="tel" value={shippingAddress.phone} onChange={e => updateAddress('phone', e.target.value)} placeholder="Nhập số điện thoại" />
                </div>
                <div className="form-group">
                  <label>Email *</label>
                  <input type="email" value={shippingAddress.email} onChange={e => updateAddress('email', e.target.value)} placeholder="Nhập email" />
                </div>
                <div className="form-group full">
                  <label>Địa chỉ giao hàng *</label>
                  <textarea value={shippingAddress.address} onChange={e => updateAddress('address', e.target.value)} placeholder="Nhập địa chỉ chi tiết" rows={3} />
                </div>
              </div>
            </div>

            {/* Notes */}
            <div className="form-section">
              <h3>📝 Ghi chú</h3>
              <textarea value={notes} onChange={e => setNotes(e.target.value)} placeholder="Ghi chú cho người bán (không bắt buộc)" rows={3} />
            </div>

            {/* Shipping Method */}
            <div className="form-section">
              <h3>🚚 Phương thức giao hàng</h3>
              <div className="shipping-options">
                <label className={`shipping-option ${shippingMethod === 'standard' ? 'selected' : ''}`} onClick={() => selectShipping('standard')}>
                  <input type="radio" name="shipping" checked={shippingMethod === 'standard'} readOnly />
                  <div className="shipping-content">
                    <div><strong>Giao hàng tiêu chuẩn</strong><p>Nhận hàng trong 3 - 4 ngày</p></div>
                  </div>
                  <span className="shipping-price">{formatPrice(standardPrice)}</span>
                </label>
                <label className={`shipping-option ${shippingMethod === 'express' ? 'selected' : ''}`} onClick={() => selectShipping('express')}>
                  <input type="radio" name="shipping" checked={shippingMethod === 'express'} readOnly />
                  <div className="shipping-content">
                    <div><strong>Giao hàng hỏa tốc</strong><p>Nhận hàng trong 1 - 2 ngày</p></div>
                  </div>
                  <span className="shipping-price">{formatPrice(expressPrice)}</span>
                </label>
              </div>
            </div>

            {/* Payment */}
            <div className="form-section">
              <h3>💰 Phương thức thanh toán</h3>
              <div className="payment-options">
                {settings.codEnabled !== false && (
                  <label className={`payment-option ${paymentMethod === 'COD' ? 'selected' : ''}`}>
                    <input type="radio" name="payment" value="COD" checked={paymentMethod === 'COD'} onChange={() => setPaymentMethod('COD')} />
                    <div><strong>💵 Thanh toán khi nhận hàng (COD)</strong><p>{settings.codDescription || 'Thanh toán bằng tiền mặt khi nhận hàng'}</p></div>
                  </label>
                )}
                {settings.bankTransferEnabled !== false && (
                  <label className={`payment-option ${paymentMethod === 'BankTransfer' ? 'selected' : ''}`}>
                    <input type="radio" name="payment" value="BankTransfer" checked={paymentMethod === 'BankTransfer'} onChange={() => setPaymentMethod('BankTransfer')} />
                    <div><strong>🏦 Chuyển khoản ngân hàng</strong><p>{settings.bankDescription || 'Chuyển khoản trước khi giao hàng'}</p></div>
                  </label>
                )}
              </div>
              {paymentMethod === 'BankTransfer' && settings.bankName && (
                <div className="bank-transfer-info">
                  <h4>🏦 Thông tin chuyển khoản</h4>
                  <div className="bank-detail-row"><span>Ngân hàng:</span><strong>{settings.bankName}</strong></div>
                  {settings.bankAccountNumber && <div className="bank-detail-row"><span>Số tài khoản:</span><strong>{settings.bankAccountNumber}</strong></div>}
                  {settings.bankAccountHolder && <div className="bank-detail-row"><span>Chủ tài khoản:</span><strong>{settings.bankAccountHolder}</strong></div>}
                  {settings.bankQrImage && <img src={getImageUrl(settings.bankQrImage)} alt="QR" className="bank-qr" />}
                </div>
              )}
            </div>
          </div>

          {/* Order Summary */}
          <div className="order-summary">
            <h3>Đơn hàng của bạn</h3>
            <div className="summary-items">
              {items.map(item => (
                <div className="summary-item" key={item.product._id + (item.itemType || '')}>
                  <div className="item-info">
                    <img src={getImageUrl(item.product.imageUrl)} alt={item.product.name} />
                    <div>
                      {item.itemType === 'pet' && <span className="pet-label">🐾 Thú cưng</span>}
                      <span className="item-name">{item.product.name}</span>
                      <span className="item-qty">x{item.quantity}</span>
                    </div>
                  </div>
                  <span className="item-price">{formatPrice(getItemPrice(item) * item.quantity)}</span>
                </div>
              ))}
            </div>
            <div className="summary-divider"></div>
            <div className="summary-row"><span>Tạm tính</span><span>{formatPrice(totalAmount)}</span></div>
            <div className="summary-row"><span>Phí vận chuyển</span><span>{formatPrice(shippingFee)}</span></div>

            {/* Promo */}
            <div className="promo-section">
              {!promoApplied ? (
                <div className="promo-input">
                  <input type="text" value={promoCode} onChange={e => setPromoCode(e.target.value)} placeholder="Mã khuyến mãi..." disabled={applyingPromo} />
                  <button onClick={applyPromo} disabled={applyingPromo}>{applyingPromo ? '...' : 'Áp dụng'}</button>
                </div>
              ) : (
                <div className="promo-applied">
                  <span>✅ {promoCode.toUpperCase()} - {promoName}</span>
                  <button onClick={removePromo}>✕</button>
                </div>
              )}
            </div>

            {discount > 0 && (
              <div className="summary-row discount"><span>Giảm giá</span><span>-{formatPrice(discount)}</span></div>
            )}
            <div className="summary-divider"></div>
            <div className="summary-row total"><span>Tổng cộng</span><span>{formatPrice(total)}</span></div>

            <button className="btn-place-order" onClick={placeOrder} disabled={submitting || !isFormValid()}>
              {submitting ? '⏳ Đang xử lý...' : '✅ Đặt hàng'}
            </button>
            <Link to="/cart" className="back-to-cart">← Quay lại giỏ hàng</Link>
          </div>
        </div>
      </div>
    </div>
  );
}
