import { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { productApi, reviewApi } from '../../services/api';
import { getImageUrl } from '../../services/api';
import { useCart } from '../../contexts/CartContext';
import { useAuth } from '../../contexts/AuthContext';
import toast from 'react-hot-toast';
import './ProductDetail.scss';

const formatPrice = (price: number) =>
  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);

const getStars = (rating: number) =>
  Array(5).fill(0).map((_, i) => (i < Math.round(rating) ? 1 : 0));

const getTimeAgo = (date: string) => {
  const diff = Date.now() - new Date(date).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 60) return `${mins} phút trước`;
  const hrs = Math.floor(mins / 60);
  if (hrs < 24) return `${hrs} giờ trước`;
  const days = Math.floor(hrs / 24);
  if (days < 30) return `${days} ngày trước`;
  return new Date(date).toLocaleDateString('vi-VN');
};

export default function ProductDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { addToCart } = useCart();
  const { isLoggedIn } = useAuth();

  const [product, setProduct] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [quantity, setQuantity] = useState(1);
  const [selectedImage, setSelectedImage] = useState('');
  const [allImages, setAllImages] = useState<string[]>([]);

  const [reviews, setReviews] = useState<any[]>([]);
  const [reviewPage, setReviewPage] = useState(1);
  const [reviewTotalPages, setReviewTotalPages] = useState(1);

  const [showReviewForm, setShowReviewForm] = useState(false);
  const [newRating, setNewRating] = useState(5);
  const [hoverRating, setHoverRating] = useState(0);
  const [newComment, setNewComment] = useState('');
  const [reviewImages, setReviewImages] = useState<File[]>([]);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (id) {
      loadProduct(id);
      loadReviews(id, 1);
    }
  }, [id]);

  const loadProduct = async (productId: string) => {
    setLoading(true);
    try {
      const res = await productApi.getById(productId);
      const p = res.data?.data || res.data;
      setProduct(p);
      const imgs: string[] = [];
      if (p.imageUrl) imgs.push(p.imageUrl);
      if (p.images?.length) imgs.push(...p.images);
      setAllImages(imgs);
      setSelectedImage(imgs[0] || '');
    } catch {
      toast.error('Không tìm thấy sản phẩm');
      navigate('/products');
    } finally {
      setLoading(false);
    }
  };

  const loadReviews = async (productId: string, page: number) => {
    try {
      const res = await reviewApi.getByProduct(productId, { page, limit: 5 });
      const data = res.data?.data;
      if (page === 1) {
        setReviews(data?.reviews || []);
      } else {
        setReviews(prev => [...prev, ...(data?.reviews || [])]);
      }
      setReviewPage(page);
      setReviewTotalPages(data?.pagination?.pages || 1);
    } catch {
      if (page === 1) setReviews([]);
    }
  };

  const handleAddToCart = () => {
    if (!product) return;
    addToCart(product, quantity);
    toast.success(`Đã thêm ${product.name} vào giỏ hàng!`);
  };

  const getDiscountPercent = () => {
    if (!product?.discountPrice || product.discountPrice >= product.price) return 0;
    return Math.round((1 - product.discountPrice / product.price) * 100);
  };

  const handleSubmitReview = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!isLoggedIn) { toast.error('Vui lòng đăng nhập'); navigate('/login'); return; }
    if (!newComment.trim()) { toast.error('Vui lòng nhập nội dung đánh giá'); return; }
    setSubmitting(true);
    try {
      const formData = new FormData();
      formData.append('productId', id!);
      formData.append('rating', String(newRating));
      formData.append('comment', newComment);
      reviewImages.forEach(f => formData.append('images', f));
      await reviewApi.create(formData);
      toast.success('Đánh giá thành công!');
      setShowReviewForm(false);
      setNewComment('');
      setNewRating(5);
      setReviewImages([]);
      loadReviews(id!, 1);
      loadProduct(id!);
    } catch {
      toast.error('Lỗi khi gửi đánh giá');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return (
    <div className="loading-state"><div className="spinner"></div><p>Đang tải sản phẩm...</p></div>
  );

  if (!product) return null;

  const displayPrice = product.discountPrice && product.discountPrice < product.price ? product.discountPrice : product.price;

  return (
    <div className="product-detail-page">
      <div className="container">
        {/* Breadcrumb */}
        <div className="breadcrumb">
          <Link to="/">Trang chủ</Link>
          <span>/</span>
          <Link to="/products">Sản phẩm</Link>
          <span>/</span>
          <span>{product.name}</span>
        </div>

        <div className="product-detail-content">
          {/* Image Gallery */}
          <div className="image-gallery">
            <div className="main-image">
              <img src={getImageUrl(selectedImage)} alt={product.name} onError={e => { (e.target as HTMLImageElement).src = '/assets/images/no-product.svg'; }} />
              {getDiscountPercent() > 0 && <span className="discount-badge">-{getDiscountPercent()}%</span>}
            </div>
            {allImages.length > 1 && (
              <div className="thumbnails">
                {allImages.map((img, i) => (
                  <div key={i} className={`thumb ${selectedImage === img ? 'active' : ''}`} onClick={() => setSelectedImage(img)}>
                    <img src={getImageUrl(img)} alt={`${product.name} ${i + 1}`} />
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Product Info */}
          <div className="product-info">
            {product.isHotDeal && <span className="hot-tag">🔥 Ưu đãi hot</span>}
            {product.isFeatured && <span className="featured-tag">⭐ Nổi bật</span>}
            <h1>{product.name}</h1>
            <div className="meta">
              <span className="code">SKU: {product.code || product.sku || 'N/A'}</span>
              {product.brand && <span className="brand">Thương hiệu: <strong>{typeof product.brand === 'string' ? product.brand : product.brand.name}</strong></span>}
            </div>

            <div className="rating-row">
              {getStars(product.averageRating || 0).map((s, i) => (
                <span key={i} className={s ? 'star filled' : 'star'}>★</span>
              ))}
              <span className="rating-text">{(product.averageRating || 0).toFixed(1)} ({product.totalReviews || 0} đánh giá)</span>
              <span className="sold">| Đã bán: {product.soldCount || 0}</span>
            </div>

            <div className="price-section">
              <span className="current-price">{formatPrice(displayPrice)}</span>
              {getDiscountPercent() > 0 && (
                <>
                  <span className="original-price">{formatPrice(product.price)}</span>
                  <span className="save-badge">Tiết kiệm {getDiscountPercent()}%</span>
                </>
              )}
            </div>

            <div className="stock-info">
              {product.stock > 0 ? (
                <span className="in-stock">✓ Còn hàng ({product.stock})</span>
              ) : (
                <span className="out-stock">✗ Hết hàng</span>
              )}
            </div>

            {/* Quantity */}
            <div className="quantity-section">
              <label>Số lượng:</label>
              <div className="qty-control">
                <button onClick={() => setQuantity(q => Math.max(1, q - 1))}>-</button>
                <span>{quantity}</span>
                <button onClick={() => setQuantity(q => Math.min(product.stock || 99, q + 1))}>+</button>
              </div>
            </div>

            <div className="action-buttons">
              <button className="btn-add-cart" onClick={handleAddToCart} disabled={product.stock === 0}>
                🛒 Thêm vào giỏ hàng
              </button>
              <button className="btn-buy-now" onClick={() => { handleAddToCart(); navigate('/cart'); }} disabled={product.stock === 0}>
                ⚡ Mua ngay
              </button>
            </div>
          </div>
        </div>

        {/* Description */}
        {product.description && (
          <div className="description-section">
            <h3>📋 Mô tả sản phẩm</h3>
            <div className="desc-content" dangerouslySetInnerHTML={{ __html: product.description }} />
          </div>
        )}

        {/* Reviews */}
        <div className="reviews-section">
          <div className="reviews-header">
            <h3>⭐ Đánh giá ({product.totalReviews || 0})</h3>
            {isLoggedIn && (
              <button className="btn-write-review" onClick={() => setShowReviewForm(!showReviewForm)}>
                ✍️ Viết đánh giá
              </button>
            )}
          </div>

          {showReviewForm && (
            <form className="review-form" onSubmit={handleSubmitReview}>
              <div className="rating-select">
                <label>Đánh giá:</label>
                <div className="stars-select">
                  {[1, 2, 3, 4, 5].map(s => (
                    <span key={s} className={`star ${s <= (hoverRating || newRating) ? 'filled' : ''}`}
                      onMouseEnter={() => setHoverRating(s)} onMouseLeave={() => setHoverRating(0)}
                      onClick={() => setNewRating(s)}>★</span>
                  ))}
                </div>
              </div>
              <textarea placeholder="Nhập nhận xét của bạn..." value={newComment} onChange={e => setNewComment(e.target.value)} rows={4} />
              <input type="file" multiple accept="image/*" onChange={e => setReviewImages(Array.from(e.target.files || []))} />
              <div className="form-actions">
                <button type="submit" className="btn-submit" disabled={submitting}>
                  {submitting ? 'Đang gửi...' : 'Gửi đánh giá'}
                </button>
                <button type="button" className="btn-cancel" onClick={() => setShowReviewForm(false)}>Hủy</button>
              </div>
            </form>
          )}

          {reviews.length > 0 ? (
            <div className="reviews-list">
              {reviews.map(review => (
                <div className="review-item" key={review._id}>
                  <div className="review-header">
                    <div className="reviewer">
                      <div className="avatar">{(review.userId?.name || review.userId?.fullName || 'K')[0]}</div>
                      <div>
                        <p className="name">{review.userId?.name || review.userId?.fullName || 'Khách hàng'}</p>
                        <p className="date">{getTimeAgo(review.createdAt)}</p>
                      </div>
                    </div>
                    <div className="review-stars">
                      {getStars(review.rating).map((s, i) => (
                        <span key={i} className={s ? 'star filled' : 'star'}>★</span>
                      ))}
                    </div>
                  </div>
                  <p className="review-text">{review.comment}</p>
                  {review.images?.length > 0 && (
                    <div className="review-images">
                      {review.images.map((img: string, i: number) => (
                        <img key={i} src={getImageUrl(img)} alt="Review" />
                      ))}
                    </div>
                  )}
                </div>
              ))}
              {reviewPage < reviewTotalPages && (
                <button className="btn-load-more" onClick={() => loadReviews(id!, reviewPage + 1)}>
                  Xem thêm đánh giá
                </button>
              )}
            </div>
          ) : (
            <div className="no-reviews">
              <p>💬 Chưa có đánh giá nào</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
