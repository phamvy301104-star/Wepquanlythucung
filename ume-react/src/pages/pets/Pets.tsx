import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { petApi } from '../../services/api';
import { getImageUrl } from '../../services/api';
import { useCart } from '../../contexts/CartContext';
import toast from 'react-hot-toast';
import './Pets.scss';

const formatPrice = (price: number) => {
  if (!price) return 'Miễn phí';
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
};

const petTypes = [
  { value: 'Dog', label: 'Chó' },
  { value: 'Cat', label: 'Mèo' },
  { value: 'Bird', label: 'Chim' },
  { value: 'Fish', label: 'Cá' },
  { value: 'Hamster', label: 'Hamster' },
  { value: 'Rabbit', label: 'Thỏ' },
  { value: 'Other', label: 'Khác' },
];

const typeNames: Record<string, string> = { Dog: 'Chó', Cat: 'Mèo', Bird: 'Chim', Fish: 'Cá', Hamster: 'Hamster', Rabbit: 'Thỏ', Other: 'Khác' };
const genderNames: Record<string, string> = { Male: 'Đực', Female: 'Cái', Unknown: 'Chưa rõ' };
const listingLabels: Record<string, string> = { Sale: 'Đang bán', Adoption: 'Nhận nuôi' };

export default function Pets() {
  const navigate = useNavigate();
  const { addPetToCart, items } = useCart();
  const [pets, setPets] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedType, setSelectedType] = useState('');
  const [selectedListingType, setSelectedListingType] = useState('');
  const [searchText, setSearchText] = useState('');

  useEffect(() => { loadPets(); }, []);

  const loadPets = async (type?: string, listingType?: string, search?: string) => {
    setLoading(true);
    try {
      const params: any = { limit: 200 };
      const t = type ?? selectedType;
      const lt = listingType ?? selectedListingType;
      const s = search ?? searchText;
      if (t) params.type = t;
      if (lt) params.listingType = lt;
      if (s) params.search = s;
      const res = await petApi.getAll(params);
      setPets(res.data?.data?.pets || res.data?.data || []);
    } catch {
      setPets([]);
    } finally {
      setLoading(false);
    }
  };

  const filterByType = (type: string) => {
    const newType = selectedType === type ? '' : type;
    setSelectedType(newType);
    loadPets(newType);
  };

  const filterByListingType = (type: string) => {
    const newType = selectedListingType === type ? '' : type;
    setSelectedListingType(newType);
    loadPets(undefined, newType);
  };

  const getAgeLabel = (pet: any) => {
    if (!pet.age) return '';
    const unit = pet.ageUnit === 'years' ? 'tuổi' : 'tháng';
    return `${pet.age} ${unit}`;
  };

  const isPetInCart = (pet: any) => items.some(i => i.product._id === pet._id && i.itemType === 'pet');

  const handleAddToCart = (e: React.MouseEvent, pet: any) => {
    e.stopPropagation();
    addPetToCart(pet);
    toast.success(`Đã thêm ${pet.name} vào giỏ hàng!`);
  };

  return (
    <div className="pets-page">
      <div className="page-header">
        <div className="container">
          <h1>🐾 Tất cả thú cưng</h1>
          <p>Khám phá các bé thú cưng đáng yêu tại PetCare</p>
        </div>
      </div>

      <div className="container">
        {/* Search */}
        <div className="search-bar">
          <input type="text" value={searchText} onChange={e => setSearchText(e.target.value)}
            placeholder="Tìm theo tên, giống..." onKeyDown={e => e.key === 'Enter' && loadPets()} />
          <button onClick={() => loadPets()}>🔍</button>
        </div>

        {/* Filters */}
        <div className="filter-bar">
          <div className="filter-group">
            <label>Loại thú cưng:</label>
            <div className="filter-tabs">
              <button className={!selectedType ? 'active' : ''} onClick={() => filterByType('')}>Tất cả</button>
              {petTypes.map(type => (
                <button key={type.value} className={selectedType === type.value ? 'active' : ''} onClick={() => filterByType(type.value)}>
                  {type.label}
                </button>
              ))}
            </div>
          </div>
          <div className="filter-group">
            <label>Hình thức:</label>
            <div className="filter-tabs">
              <button className={!selectedListingType ? 'active' : ''} onClick={() => filterByListingType('')}>Tất cả</button>
              <button className={selectedListingType === 'Adoption' ? 'active' : ''} onClick={() => filterByListingType('Adoption')}>
                ❤️ Nhận nuôi
              </button>
              <button className={selectedListingType === 'Sale' ? 'active' : ''} onClick={() => filterByListingType('Sale')}>
                🏷️ Mua bán
              </button>
            </div>
          </div>
        </div>

        {loading ? (
          <div className="loading-state"><div className="spinner"></div><p>Đang tải...</p></div>
        ) : pets.length === 0 ? (
          <div className="empty-state">
            <span className="icon">🐾</span>
            <p>Không tìm thấy thú cưng nào</p>
          </div>
        ) : (
          <div className="pets-grid">
            {pets.map(pet => (
              <div className="pet-card" key={pet._id} onClick={() => navigate(`/pets/${pet._id}`)}>
                <div className="pet-image">
                  <img src={getImageUrl(pet.imageUrl)} alt={pet.name} onError={e => { (e.target as HTMLImageElement).src = '/assets/images/default-pet.svg'; }} />
                  {pet.listingType && pet.listingType !== 'None' && (
                    <span className={`listing-badge ${pet.listingType === 'Adoption' ? 'adoption' : 'sale'}`}>
                      {listingLabels[pet.listingType]}
                    </span>
                  )}
                </div>
                <div className="pet-info">
                  <h3>{pet.name}</h3>
                  <div className="pet-details">
                    <span>🐾 {typeNames[pet.type] || pet.type}</span>
                    {pet.breed && <span>🧬 {pet.breed}</span>}
                    {pet.age && <span>🎂 {getAgeLabel(pet)}</span>}
                    {pet.gender && <span>⚧ {genderNames[pet.gender] || pet.gender}</span>}
                  </div>
                  <div className="pet-tags">
                    {pet.vaccinated && <span className="tag">💉 Đã tiêm phòng</span>}
                    {pet.neutered && <span className="tag">✂️ Đã triệt sản</span>}
                  </div>
                  {pet.description && <p className="pet-desc">{pet.description}</p>}
                  <div className="pet-footer">
                    {pet.listingType === 'Sale' && pet.listingPrice && (
                      <span className="pet-price">{formatPrice(pet.listingPrice)}</span>
                    )}
                    {pet.listingType === 'Adoption' && (
                      <span className="pet-price free">Miễn phí nhận nuôi</span>
                    )}
                    {pet.owner && <div className="owner-info">👤 {pet.owner.fullName || 'Ẩn danh'}</div>}
                  </div>

                  {pet.listingType === 'Sale' && pet.listingPrice && (
                    <div className="pet-buy">
                      {!isPetInCart(pet) ? (
                        <button className="btn-add-cart" onClick={e => handleAddToCart(e, pet)}>🛒 Thêm vào giỏ</button>
                      ) : (
                        <button className="btn-in-cart" disabled onClick={e => e.stopPropagation()}>✓ Đã trong giỏ</button>
                      )}
                    </div>
                  )}
                  {pet.listingType === 'Adoption' && (
                    <div className="pet-buy">
                      <button className="btn-adopt" onClick={e => handleAddToCart(e, pet)}>❤️ Nhận nuôi</button>
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
