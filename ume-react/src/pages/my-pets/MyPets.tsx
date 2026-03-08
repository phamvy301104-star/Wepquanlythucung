import { useState, useEffect } from 'react';
import { petApi } from '../../services/api';
import { getImageUrl } from '../../services/api';
import toast from 'react-hot-toast';
import './MyPets.scss';

const petTypes = [
  { value: 'Dog', label: 'Chó' }, { value: 'Cat', label: 'Mèo' }, { value: 'Bird', label: 'Chim' },
  { value: 'Fish', label: 'Cá' }, { value: 'Hamster', label: 'Hamster' }, { value: 'Rabbit', label: 'Thỏ' },
  { value: 'Other', label: 'Khác' },
];
const genderOptions = [{ value: 'Male', label: 'Đực' }, { value: 'Female', label: 'Cái' }, { value: 'Unknown', label: 'Chưa rõ' }];
const ageUnits = [{ value: 'months', label: 'Tháng' }, { value: 'years', label: 'Năm' }];

const typeNames: any = { Dog: 'Chó', Cat: 'Mèo', Bird: 'Chim', Fish: 'Cá', Hamster: 'Hamster', Rabbit: 'Thỏ', Other: 'Khác' };
const genderNames: any = { Male: 'Đực', Female: 'Cái', Unknown: 'Chưa rõ' };

const defaultForm = { name: '', type: 'Dog', breed: '', age: '', ageUnit: 'months', weight: '', gender: 'Male', color: '', description: '', healthNotes: '', vaccinated: false, neutered: false };

export default function MyPets() {
  const [pets, setPets] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [editingPet, setEditingPet] = useState<any>(null);
  const [form, setForm] = useState<any>({ ...defaultForm });
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => { loadPets(); }, []);

  const loadPets = async () => {
    setLoading(true);
    try {
      const r = await petApi.getMyPets();
      setPets(r.data?.data?.pets || r.data?.data || []);
    } catch {} finally { setLoading(false); }
  };

  const openAdd = () => { setEditingPet(null); setForm({ ...defaultForm }); setImageFile(null); setImagePreview(null); setShowModal(true); };
  const openEdit = (pet: any) => {
    setEditingPet(pet);
    setForm({ name: pet.name, type: pet.type, breed: pet.breed || '', age: pet.age || '', ageUnit: pet.ageUnit || 'months', weight: pet.weight || '', gender: pet.gender, color: pet.color || '', description: pet.description || '', healthNotes: pet.healthNotes || '', vaccinated: pet.vaccinated || false, neutered: pet.neutered || false });
    setImageFile(null);
    setImagePreview(pet.imageUrl ? getImageUrl(pet.imageUrl) : null);
    setShowModal(true);
  };

  const onImageSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const f = e.target.files?.[0];
    if (!f) return;
    setImageFile(f);
    const reader = new FileReader();
    reader.onload = ev => setImagePreview(ev.target?.result as string);
    reader.readAsDataURL(f);
  };

  const savePet = async () => {
    if (!form.name) { toast.error('Vui lòng nhập tên thú cưng'); return; }
    setSubmitting(true);
    try {
      const fd = new FormData();
      Object.keys(form).forEach(k => { if (form[k] !== null && form[k] !== undefined && form[k] !== '') fd.append(k, form[k].toString()); });
      if (imageFile) fd.append('image', imageFile);
      if (editingPet) {
        await petApi.update(editingPet._id, fd);
        toast.success('Cập nhật thú cưng thành công!');
      } else {
        await petApi.create(fd);
        toast.success('Thêm thú cưng thành công!');
      }
      setShowModal(false);
      loadPets();
    } catch (err: any) { toast.error(err.response?.data?.message || 'Thao tác thất bại'); }
    finally { setSubmitting(false); }
  };

  const deletePet = async (pet: any) => {
    if (!window.confirm(`Bạn có chắc muốn xóa ${pet.name}?`)) return;
    try { await petApi.delete(pet._id); toast.success('Đã xóa thú cưng'); loadPets(); }
    catch (err: any) { toast.error(err.response?.data?.message || 'Xóa thất bại'); }
  };

  const getAgeLabel = (pet: any) => pet.age ? `${pet.age} ${pet.ageUnit === 'years' ? 'tuổi' : 'tháng'}` : 'Không rõ';

  return (
    <div className="my-pets-page">
      <div className="page-header"><div className="container"><h1>🐾 Thú cưng của tôi</h1></div></div>
      <div className="container">
        <div className="toolbar"><span>{pets.length} thú cưng</span><button className="btn-add" onClick={openAdd}>➕ Thêm thú cưng</button></div>

        {loading && <div className="loading-center">⏳ Đang tải...</div>}
        {!loading && pets.length === 0 && (
          <div className="empty-state">🐾<p>Bạn chưa thêm thú cưng nào</p><button className="btn-add-first" onClick={openAdd}>➕ Thêm thú cưng đầu tiên</button></div>
        )}

        <div className="pets-grid">
          {pets.map(pet => (
            <div key={pet._id} className="pet-card">
              <div className="pet-image"><img src={getImageUrl(pet.imageUrl) || '/assets/images/default-pet.svg'} alt={pet.name} onError={e => { (e.target as HTMLImageElement).src = '/assets/images/default-pet.svg'; }} /></div>
              <div className="pet-info">
                <h3>{pet.name}</h3>
                <div className="pet-details">
                  <span>🐾 {typeNames[pet.type] || pet.type}</span>
                  {pet.breed && <span>🧬 {pet.breed}</span>}
                  <span>🎂 {getAgeLabel(pet)}</span>
                  {pet.weight && <span>⚖️ {pet.weight} kg</span>}
                  <span>{pet.gender === 'Male' ? '♂️' : '♀️'} {genderNames[pet.gender] || pet.gender}</span>
                </div>
                <div className="pet-tags">
                  {pet.vaccinated && <span className="tag success">💉 Đã tiêm phòng</span>}
                  {pet.neutered && <span className="tag success">✂️ Đã triệt sản</span>}
                </div>
                {pet.description && <p className="pet-desc">{pet.description}</p>}
                <div className="pet-actions">
                  <button className="btn-edit" onClick={() => openEdit(pet)}>✏️ Sửa</button>
                  <button className="btn-delete" onClick={() => deletePet(pet)}>🗑️ Xóa</button>
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* Modal */}
        {showModal && (
          <div className="modal-overlay" onClick={() => setShowModal(false)}>
            <div className="modal-content" onClick={e => e.stopPropagation()}>
              <div className="modal-header"><h3>{editingPet ? 'Chỉnh sửa thú cưng' : 'Thêm thú cưng mới'}</h3><button className="btn-close" onClick={() => setShowModal(false)}>×</button></div>
              <div className="modal-body">
                <div className="image-upload" onClick={() => document.getElementById('pet-img-input')?.click()}>
                  {imagePreview ? <img src={imagePreview} alt="preview" /> : <div className="upload-placeholder">📷<span>Chọn ảnh</span></div>}
                  <input id="pet-img-input" type="file" accept="image/*" onChange={onImageSelect} hidden />
                </div>
                <div className="form-grid">
                  <div className="form-group"><label>Tên *</label><input value={form.name} onChange={e => setForm((f: any) => ({ ...f, name: e.target.value }))} /></div>
                  <div className="form-group"><label>Loại</label><select value={form.type} onChange={e => setForm((f: any) => ({ ...f, type: e.target.value }))}>{petTypes.map(t => <option key={t.value} value={t.value}>{t.label}</option>)}</select></div>
                  <div className="form-group"><label>Giống</label><input value={form.breed} onChange={e => setForm((f: any) => ({ ...f, breed: e.target.value }))} /></div>
                  <div className="form-group"><label>Giới tính</label><select value={form.gender} onChange={e => setForm((f: any) => ({ ...f, gender: e.target.value }))}>{genderOptions.map(g => <option key={g.value} value={g.value}>{g.label}</option>)}</select></div>
                  <div className="form-group"><label>Tuổi</label>
                    <div className="age-input"><input type="number" value={form.age} onChange={e => setForm((f: any) => ({ ...f, age: e.target.value }))} min={0} /><select value={form.ageUnit} onChange={e => setForm((f: any) => ({ ...f, ageUnit: e.target.value }))}>{ageUnits.map(u => <option key={u.value} value={u.value}>{u.label}</option>)}</select></div>
                  </div>
                  <div className="form-group"><label>Cân nặng (kg)</label><input type="number" value={form.weight} onChange={e => setForm((f: any) => ({ ...f, weight: e.target.value }))} min={0} step={0.1} /></div>
                  <div className="form-group"><label>Màu lông</label><input value={form.color} onChange={e => setForm((f: any) => ({ ...f, color: e.target.value }))} /></div>
                  <div className="form-group full"><label>Mô tả</label><textarea value={form.description} onChange={e => setForm((f: any) => ({ ...f, description: e.target.value }))} rows={2} /></div>
                  <div className="form-group full"><label>Ghi chú sức khỏe</label><textarea value={form.healthNotes} onChange={e => setForm((f: any) => ({ ...f, healthNotes: e.target.value }))} rows={2} /></div>
                  <div className="form-group checkbox-group"><label><input type="checkbox" checked={form.vaccinated} onChange={e => setForm((f: any) => ({ ...f, vaccinated: e.target.checked }))} /> Đã tiêm phòng</label></div>
                  <div className="form-group checkbox-group"><label><input type="checkbox" checked={form.neutered} onChange={e => setForm((f: any) => ({ ...f, neutered: e.target.checked }))} /> Đã triệt sản</label></div>
                </div>
              </div>
              <div className="modal-footer"><button className="btn-secondary" onClick={() => setShowModal(false)}>Hủy</button><button className="btn-primary" onClick={savePet} disabled={submitting}>{submitting ? '⏳ Đang lưu...' : (editingPet ? 'Cập nhật' : 'Thêm mới')}</button></div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
