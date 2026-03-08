import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { productApi, categoryApi, brandApi } from '../../services/api';
import { getImageUrl } from '../../services/api';
import { useCart } from '../../contexts/CartContext';
import toast from 'react-hot-toast';
import './Products.scss';

const formatPrice = (price: number) =>
  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);

const getStars = (rating: number) =>
  Array(5).fill(0).map((_, i) => (i < Math.round(rating) ? 1 : 0));

export default function Products() {
  const { addToCart } = useCart();
  const [products, setProducts] = useState<any[]>([]);
  const [categories, setCategories] = useState<any[]>([]);
  const [brands, setBrands] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);

  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('');
  const [selectedBrand, setSelectedBrand] = useState('');
  const [minPrice, setMinPrice] = useState('');
  const [maxPrice, setMaxPrice] = useState('');
  const [sortBy, setSortBy] = useState('newest');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const limit = 12;

  const loadProducts = useCallback(async (page = currentPage) => {
    setLoading(true);
    try {
      const params: any = { page, limit, sort: sortBy };
      if (searchTerm) params.search = searchTerm;
      if (selectedCategory) params.category = selectedCategory;
      if (selectedBrand) params.brand = selectedBrand;
      if (minPrice) params.minPrice = minPrice;
      if (maxPrice) params.maxPrice = maxPrice;
      const res = await productApi.getAll(params);
      setProducts(res.data?.data?.products || res.data?.data || []);
      setTotalPages(res.data?.data?.pagination?.pages || 1);
    } catch {
      setProducts([]);
    } finally {
      setLoading(false);
    }
  }, [currentPage, sortBy, searchTerm, selectedCategory, selectedBrand, minPrice, maxPrice]);

  useEffect(() => {
    loadProducts();
  }, [loadProducts]);

  useEffect(() => {
    categoryApi.getAll().then(res => setCategories(res.data?.data?.categories || res.data?.data || [])).catch(() => {});
    brandApi.getAll().then(res => setBrands(res.data?.data?.brands || res.data?.data || [])).catch(() => {});
  }, []);

  const handleSearch = () => {
    setCurrentPage(1);
    loadProducts(1);
  };

  const handleCategoryChange = (id: string) => {
    setSelectedCategory(id);
    setCurrentPage(1);
  };

  const handleBrandChange = (id: string) => {
    setSelectedBrand(id);
    setCurrentPage(1);
  };

  const handleSort = (val: string) => {
    setSortBy(val);
    setCurrentPage(1);
  };

  const clearFilters = () => {
    setSearchTerm('');
    setSelectedCategory('');
    setSelectedBrand('');
    setMinPrice('');
    setMaxPrice('');
    setSortBy('newest');
    setCurrentPage(1);
  };

  const handleAddToCart = (product: any) => {
    addToCart(product, 1);
    toast.success(`Đã thêm ${product.name} vào giỏ hàng!`);
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <div className="products-page">
      <div className="page-header">
        <div className="container">
          <h1>🛍️ Sản phẩm</h1>
          <p>Khám phá các sản phẩm chất lượng cho thú cưng</p>
        </div>
      </div>

      <div className="container">
        <div className="products-layout">
          {/* Sidebar Filters */}
          <aside className="filters-sidebar">
            <div className="filter-section">
              <h3>🔍 Tìm kiếm</h3>
              <div className="search-box">
                <input
                  type="text"
                  placeholder="Tên sản phẩm..."
                  value={searchTerm}
                  onChange={e => setSearchTerm(e.target.value)}
                  onKeyDown={e => e.key === 'Enter' && handleSearch()}
                />
                <button onClick={handleSearch}>🔍</button>
              </div>
            </div>

            <div className="filter-section">
              <h3>📁 Danh mục</h3>
              <ul className="filter-list">
                <li className={!selectedCategory ? 'active' : ''} onClick={() => handleCategoryChange('')}>Tất cả</li>
                {categories.map(cat => (
                  <li key={cat._id} className={selectedCategory === cat._id ? 'active' : ''} onClick={() => handleCategoryChange(cat._id)}>
                    {cat.name}
                  </li>
                ))}
              </ul>
            </div>

            <div className="filter-section">
              <h3>🏷️ Thương hiệu</h3>
              <ul className="filter-list">
                <li className={!selectedBrand ? 'active' : ''} onClick={() => handleBrandChange('')}>Tất cả</li>
                {brands.map(b => (
                  <li key={b._id} className={selectedBrand === b._id ? 'active' : ''} onClick={() => handleBrandChange(b._id)}>
                    {b.name}
                  </li>
                ))}
              </ul>
            </div>

            <div className="filter-section">
              <h3>💰 Giá</h3>
              <div className="price-inputs">
                <input type="number" placeholder="Từ" value={minPrice} onChange={e => setMinPrice(e.target.value)} />
                <span>-</span>
                <input type="number" placeholder="Đến" value={maxPrice} onChange={e => setMaxPrice(e.target.value)} />
              </div>
              <button className="btn-apply" onClick={handleSearch}>Áp dụng</button>
            </div>

            <button className="btn-clear" onClick={clearFilters}>🗑️ Xóa bộ lọc</button>
          </aside>

          {/* Products Grid */}
          <div className="products-main">
            <div className="products-toolbar">
              <span className="results-count">{products.length} sản phẩm</span>
              <select value={sortBy} onChange={e => handleSort(e.target.value)}>
                <option value="newest">Mới nhất</option>
                <option value="price_asc">Giá: Thấp → Cao</option>
                <option value="price_desc">Giá: Cao → Thấp</option>
                <option value="name_asc">Tên: A → Z</option>
                <option value="bestselling">Bán chạy</option>
              </select>
            </div>

            {loading ? (
              <div className="loading-state"><div className="spinner"></div><p>Đang tải sản phẩm...</p></div>
            ) : products.length === 0 ? (
              <div className="empty-state">
                <span className="icon">📦</span>
                <p>Không tìm thấy sản phẩm nào</p>
                <button className="btn-primary" onClick={clearFilters}>Xóa bộ lọc</button>
              </div>
            ) : (
              <>
                <div className="products-grid">
                  {products.map(product => (
                    <div className="product-card" key={product._id}>
                      <Link to={`/products/${product._id}`} className="product-image">
                        <img src={getImageUrl(product.imageUrl)} alt={product.name} onError={e => { (e.target as HTMLImageElement).src = '/assets/images/no-product.svg'; }} />
                        {product.discountPrice && product.discountPrice < product.price && (
                          <span className="discount-badge">-{Math.round((1 - product.discountPrice / product.price) * 100)}%</span>
                        )}
                        {product.isHotDeal && <span className="hot-badge">🔥 Hot</span>}
                      </Link>
                      <div className="product-info">
                        <Link to={`/products/${product._id}`} className="product-name">{product.name}</Link>
                        <div className="product-rating">
                          {getStars(product.averageRating || 0).map((s, i) => (
                            <span key={i} className={s ? 'star filled' : 'star'}>★</span>
                          ))}
                          <span className="rating-count">({product.totalReviews || 0})</span>
                        </div>
                        <div className="product-price">
                          {product.discountPrice && product.discountPrice < product.price ? (
                            <>
                              <span className="current-price">{formatPrice(product.discountPrice)}</span>
                              <span className="original-price">{formatPrice(product.price)}</span>
                            </>
                          ) : (
                            <span className="current-price">{formatPrice(product.price)}</span>
                          )}
                        </div>
                        <button className="btn-add-cart" onClick={() => handleAddToCart(product)} disabled={product.stock === 0}>
                          {product.stock === 0 ? 'Hết hàng' : '🛒 Thêm vào giỏ'}
                        </button>
                      </div>
                    </div>
                  ))}
                </div>

                {/* Pagination */}
                {totalPages > 1 && (
                  <div className="pagination">
                    <button disabled={currentPage === 1} onClick={() => handlePageChange(currentPage - 1)}>‹</button>
                    {Array.from({ length: totalPages }, (_, i) => i + 1).map(page => (
                      <button key={page} className={currentPage === page ? 'active' : ''} onClick={() => handlePageChange(page)}>{page}</button>
                    ))}
                    <button disabled={currentPage === totalPages} onClick={() => handlePageChange(currentPage + 1)}>›</button>
                  </div>
                )}
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
