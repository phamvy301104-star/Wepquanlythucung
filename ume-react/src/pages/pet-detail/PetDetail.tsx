import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { petApi } from '../../services/api';
import { getImageUrl } from '../../services/api';
import { useCart } from '../../contexts/CartContext';
import { useAuth } from '../../contexts/AuthContext';
import toast from 'react-hot-toast';
import './PetDetail.scss';

const formatPrice = (price: number) => {
  if (!price) return 'Miễn phí';
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
};

const typeNames: Record<string, string> = { Dog: 'Chó', Cat: 'Mèo', Bird: 'Chim', Fish: 'Cá', Hamster: 'Hamster', Rabbit: 'Thỏ', Other: 'Khác' };
const genderNames: Record<string, string> = { Male: 'Đực', Female: 'Cái', Unknown: 'Chưa rõ' };
const listingLabels: Record<string, string> = { Sale: 'Đang bán', Adoption: 'Nhận nuôi' };

export default function PetDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { addPetToCart, items } = useCart();
  const { user, isLoggedIn } = useAuth();

  const [pet, setPet] = useState<any>(null);
  const [similarPets, setSimilarPets] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [isOwner, setIsOwner] = useState(false);

  useEffect(() => {
    if (id) loadPet(id);
  }, [id]);

  const loadPet = async (petId: string) => {
    setLoading(true);
    try {
      const res = await petApi.getById(petId);
      const p = res.data?.data || res.data;
      setPet(p);
      setIsOwner(!!isLoggedIn && p?.owner?._id === user?._id);
      loadSimilarPets(p);
    } catch {
      toast.error('Không tìm thấy thú cưng');
      navigate('/pets');
    } finally {
      setLoading(false);
    }
  };

  const loadSimilarPets = async (currentPet: any) => {
    if (!currentPet) return;
    try {
      const res = await petApi.getAll({ type: currentPet.type, limit: 4 });
      const all = res.data?.data?.pets || res.data?.data || [];
      setSimilarPets(all.filter((p: any) => p._id !== currentPet._id).slice(0, 4));
    } catch {}
  };

  const qrCodeUrl = `https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=${encodeURIComponent(typeof window !== 'undefined' ? window.location.href : '')}`;

  const getAgeLabel = (p: any) => {
    if (!p?.age) return 'N/A';
    const unit = p.ageUnit === 'years' ? 'tuổi' : 'tháng';
    return `${p.age} ${unit}`;
  };

  const getPetCode = () => {
    if (!pet) return '';
    if (pet.code) return pet.code;
    const d = new Date(pet.createdAt);
    const y = d.getFullYear().toString().slice(-2);
    const m = (d.getMonth() + 1).toString().padStart(2, '0');
    const day = d.getDate().toString().padStart(2, '0');
    const idSlice = pet._id?.slice(-4) || '0000';
    return `PET${y}${m}${day}${idSlice}`;
  };

  const isPetInCart = () => items.some(i => i.product._id === pet?._id && i.itemType === 'pet');

  const handleAddToCart = () => {
    if (!isLoggedIn) { toast('Vui lòng đăng nhập', { icon: '⚠️' }); navigate('/login'); return; }
    addPetToCart(pet);
    toast.success(`Đã thêm ${pet.name} vào giỏ hàng!`);
  };

  if (loading) return (
    <div className="loading-state"><div className="spinner"></div><p>Đang tải thông tin thú cưng...</p></div>
  );

  if (!pet) return null;

  return (
    <div className="pet-detail-page">
      <div className="container">
        <div className="pet-detail-content">
          {/* Left: Image + QR */}
          <div className="pet-image-section">
            <div className="main-image">
              <img src={getImageUrl(pet.imageUrl)} alt={pet.name} onError={e => { (e.target as HTMLImageElement).src = '/assets/images/default-pet.svg'; }} />
            </div>
            <div className="qr-section">
              <p>📱 Quét mã QR để chia sẻ</p>
              <img src={qrCodeUrl} alt="QR Code" className="qr-image" />
            </div>
          </div>

          {/* Right: Info */}
          <div className="pet-info-section">
            <h2 className="pet-name">🐾 {pet.name}</h2>
            <p className="pet-code">Mã: {getPetCode()}</p>

            {pet.listingType === 'Adoption' && !isOwner && (
              <div className="adoption-banner">❤️ Thú cưng cần tìm nhà yêu thương</div>
            )}
            {isOwner && (
              <div className="owner-banner">✓ Đây là thú cưng của bạn</div>
            )}

            {/* Price */}
            {pet.listingType === 'Sale' && !isOwner && (
              <div className="price-section">
                <span className="price">{formatPrice(pet.listingPrice)}</span>
                {pet.originalPrice && pet.originalPrice > pet.listingPrice && (
                  <span className="original-price">{formatPrice(pet.originalPrice)}</span>
                )}
              </div>
            )}
            {pet.listingType === 'Adoption' && !isOwner && (
              <div className="price-section adoption">
                <span className="price">{pet.listingPrice ? formatPrice(pet.listingPrice) : 'Miễn phí'}</span>
                {pet.listingPrice > 0 && <span className="note">Phí hỗ trợ chăm sóc</span>}
              </div>
            )}

            {/* Tags */}
            <div className="pet-tags">
              {pet.isFeatured && <span className="tag featured">⭐ Nổi bật</span>}
              {pet.vaccinated && <span className="tag vaccinated">✓ Đã tiêm phòng</span>}
              {pet.neutered && <span className="tag neutered">✓ Đã triệt sản</span>}
              {pet.microchipId && <span className="tag microchip">✓ Có microchip</span>}
            </div>

            {/* Details Table */}
            <div className="details-table">
              <div className="detail-row"><span className="label">Loài</span><span className="value">{typeNames[pet.type] || pet.type}</span></div>
              <div className="detail-row"><span className="label">Giống</span><span className="value">{pet.breed || 'N/A'}</span></div>
              <div className="detail-row"><span className="label">Giới tính</span><span className="value">{genderNames[pet.gender] || pet.gender}</span></div>
              <div className="detail-row"><span className="label">Tuổi</span><span className="value">{getAgeLabel(pet)}</span></div>
              {pet.color && <div className="detail-row"><span className="label">Màu sắc</span><span className="value">{pet.color}</span></div>}
              {pet.weight && <div className="detail-row"><span className="label">Cân nặng</span><span className="value">{pet.weight.toFixed(2)} {pet.weightUnit || 'kg'}</span></div>}
              {pet.healthNotes && <div className="detail-row"><span className="label">Sức khỏe</span><span className="value">{pet.healthNotes}</span></div>}
              {pet.origin && <div className="detail-row"><span className="label">Xuất xứ</span><span className="value">{pet.origin}</span></div>}
            </div>

            {/* Vaccination */}
            {pet.vaccinated && pet.vaccinationDetails && (
              <div className="vaccination-section">
                <h4>💉 Chi tiết tiêm phòng</h4>
                <p>{pet.vaccinationDetails}</p>
              </div>
            )}

            {/* Actions */}
            {pet.listingType === 'Sale' && !isOwner && (
              <div className="action-buttons">
                {!isPetInCart() ? (
                  <button className="btn-primary" onClick={handleAddToCart}>🛒 Thêm vào giỏ hàng</button>
                ) : (
                  <button className="btn-in-cart" disabled>✓ Đã trong giỏ hàng</button>
                )}
                <button className="btn-secondary">📞 Hoặc liên hệ: 0901 234 567</button>
              </div>
            )}
            {pet.listingType === 'Adoption' && !isOwner && (
              <div className="action-buttons">
                <button className="btn-adopt">❤️ Liên hệ nhận nuôi: 0901 234 567</button>
              </div>
            )}

            <div className="view-count">👁️ {pet.viewCount || 0} lượt xem</div>
          </div>
        </div>

        {/* Description */}
        {(pet.description || pet.listingDescription) && (
          <div className="description-section">
            <h3>📋 Mô tả chi tiết</h3>
            <p>{pet.listingDescription || pet.description}</p>
          </div>
        )}

        {/* Similar Pets */}
        {similarPets.length > 0 && (
          <div className="similar-section">
            <h3>🐾 Thú cưng tương tự</h3>
            <div className="similar-grid">
              {similarPets.map(p => (
                <div className="similar-card" key={p._id} onClick={() => navigate(`/pets/${p._id}`)}>
                  <div className="similar-image">
                    <img src={getImageUrl(p.imageUrl)} alt={p.name} onError={e => { (e.target as HTMLImageElement).src = '/assets/images/default-pet.svg'; }} />
                    {p.listingType !== 'None' && (
                      <span className={`listing-badge ${p.listingType === 'Adoption' ? 'adoption' : 'sale'}`}>
                        {listingLabels[p.listingType]}
                      </span>
                    )}
                  </div>
                  <div className="similar-info">
                    <h4>{p.name}</h4>
                    <p>{typeNames[p.type] || p.type} • {p.breed}</p>
                    {p.listingType === 'Sale' && <span className="price">{formatPrice(p.listingPrice)}</span>}
                    {p.listingType === 'Adoption' && <span className="price free">Miễn phí</span>}
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
