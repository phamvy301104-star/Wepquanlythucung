import { Link } from 'react-router-dom';
import { useCart } from '../../contexts/CartContext';
import { getImageUrl } from '../../services/api';
import './Cart.scss';

const formatPrice = (price: number) =>
  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);

export default function Cart() {
  const { items, totalItems, totalAmount, updateQuantity, removeItem, clearCart } = useCart();
  const shippingFee = 30000;

  const getItemPrice = (item: any) =>
    item.itemType === 'pet' ? (item.product.listingPrice || 0) : item.product.price;

  const total = items.length > 0 ? totalAmount + shippingFee : 0;

  return (
    <div className="cart-page">
      <div className="page-header">
        <div className="container">
          <h1>🛒 Giỏ hàng</h1>
          <p>{totalItems} sản phẩm trong giỏ hàng</p>
        </div>
      </div>

      <div className="container">
        {items.length === 0 ? (
          <div className="cart-empty">
            <span className="icon">🛒</span>
            <h3>Giỏ hàng trống</h3>
            <p>Bạn chưa có sản phẩm nào trong giỏ hàng</p>
            <div className="empty-links">
              <Link to="/products" className="btn-shop">🛍️ Mua sản phẩm</Link>
              <Link to="/pets" className="btn-shop btn-pets">🐾 Xem thú cưng</Link>
            </div>
          </div>
        ) : (
          <div className="cart-layout">
            {/* Cart Items */}
            <div className="cart-items">
              <div className="cart-header">
                <span>Sản phẩm</span>
                <span>Đơn giá</span>
                <span>Số lượng</span>
                <span>Thành tiền</span>
                <span></span>
              </div>

              {items.map(item => (
                <div className="cart-item" key={item.product._id + (item.itemType || '')}>
                  <div className="item-product">
                    <img src={getImageUrl(item.product.imageUrl || item.product.mainImage)} alt={item.product.name} onError={e => { (e.target as HTMLImageElement).src = '/assets/images/no-image.svg'; }} />
                    <div className="item-name">
                      {item.itemType === 'pet' && <span className="item-type-badge pet-badge">🐾 Thú cưng</span>}
                      <Link to={item.itemType === 'pet' ? '/pets' : `/products/${item.product._id}`}>{item.product.name}</Link>
                      {item.itemType === 'pet' && item.product.breed && <small className="pet-breed">{item.product.breed}</small>}
                    </div>
                  </div>
                  <div className="item-price">{formatPrice(getItemPrice(item))}</div>
                  {item.itemType !== 'pet' ? (
                    <div className="item-quantity">
                      <button onClick={() => updateQuantity(item.product._id, item.quantity - 1)}>-</button>
                      <span>{item.quantity}</span>
                      <button onClick={() => updateQuantity(item.product._id, item.quantity + 1)}>+</button>
                    </div>
                  ) : (
                    <div className="item-quantity pet-qty"><span>1</span></div>
                  )}
                  <div className="item-total">{formatPrice(getItemPrice(item) * item.quantity)}</div>
                  <button className="btn-remove" onClick={() => removeItem(item.product._id)}>🗑️</button>
                </div>
              ))}

              <div className="cart-actions">
                <Link to="/products" className="btn-continue">← Tiếp tục mua sắm</Link>
                <button className="btn-clear" onClick={clearCart}>🗑️ Xóa giỏ hàng</button>
              </div>
            </div>

            {/* Summary */}
            <div className="cart-summary">
              <h3>Tóm tắt đơn hàng</h3>
              <div className="summary-row"><span>Tạm tính</span><span>{formatPrice(totalAmount)}</span></div>
              <div className="summary-row"><span>Phí vận chuyển</span><span>{formatPrice(shippingFee)}</span></div>
              <div className="summary-divider"></div>
              <div className="summary-row total"><span>Tổng cộng</span><span>{formatPrice(total)}</span></div>
              <Link to="/checkout" className="btn-checkout">🔒 Thanh toán</Link>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
