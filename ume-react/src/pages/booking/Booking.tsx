import { useState, useEffect } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { serviceApi, staffApi, appointmentApi, petApi } from '../../services/api';
import { getImageUrl } from '../../services/api';
import toast from 'react-hot-toast';
import './Booking.scss';

const formatPrice = (price: number) =>
  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);

const formatDuration = (mins: number) => {
  if (mins < 60) return `${mins} phút`;
  const h = Math.floor(mins / 60);
  const m = mins % 60;
  return m > 0 ? `${h}h${m}p` : `${h} giờ`;
};

const petTypes = [
  { value: 'Dog', label: 'Chó' }, { value: 'Cat', label: 'Mèo' },
  { value: 'Bird', label: 'Chim' }, { value: 'Hamster', label: 'Hamster' },
  { value: 'Rabbit', label: 'Thỏ' }, { value: 'Fish', label: 'Cá' },
  { value: 'Other', label: 'Khác' },
];
const petGenders = [
  { value: 'Male', label: 'Đực' }, { value: 'Female', label: 'Cái' },
  { value: 'Neutered', label: 'Đã triệt sản' },
];

export default function Booking() {
  const [searchParams] = useSearchParams();
  const [step, setStep] = useState(1);

  // Step 1
  const [services, setServices] = useState<any[]>([]);
  const [selectedServices, setSelectedServices] = useState<any[]>([]);

  // Step 2
  const [availableStaff, setAvailableStaff] = useState<any[]>([]);
  const [selectedStaff, setSelectedStaff] = useState<any>(null);

  // Step 3
  const [selectedDate, setSelectedDate] = useState(new Date().toISOString().split('T')[0]);
  const [selectedTime, setSelectedTime] = useState('');
  const [timeSlots, setTimeSlots] = useState<string[]>([]);
  const minDate = new Date().toISOString().split('T')[0];

  // Step 4
  const [myPets, setMyPets] = useState<any[]>([]);
  const [selectedPet, setSelectedPet] = useState<any>(null);
  const [describePet, setDescribePet] = useState(false);
  const [petDesc, setPetDesc] = useState({ name: '', type: '', gender: '', age: 0, weight: 0, notes: '' });

  // Step 5
  const [notes, setNotes] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [showSuccess, setShowSuccess] = useState(false);
  const [bookedCode, setBookedCode] = useState('');
  const [, setLoading] = useState(false);

  useEffect(() => {
    loadServices();
    loadMyPets();
  }, []);

  const loadServices = async () => {
    setLoading(true);
    try {
      const res = await serviceApi.getAll({ limit: 100 });
      const list = res.data?.data?.services || res.data?.data?.items || res.data?.data || [];
      setServices(list);
      const preselect = searchParams.get('service');
      if (preselect) {
        const found = list.find((s: any) => s._id === preselect);
        if (found) setSelectedServices([found]);
      }
    } catch {} finally { setLoading(false); }
  };

  const loadMyPets = async () => {
    try {
      const res = await petApi.getMyPets();
      setMyPets(res.data?.data?.items || res.data?.data || []);
    } catch {}
  };

  const loadStaff = async () => {
    try {
      const ids = selectedServices.map(s => s._id).join(',');
      const res = await staffApi.getAvailable({ services: ids, date: selectedDate });
      setAvailableStaff(res.data?.data?.items || res.data?.data || []);
    } catch {}
  };

  const generateTimeSlots = () => {
    const slots: string[] = [];
    for (let h = 8; h <= 19; h++) {
      slots.push(`${h.toString().padStart(2, '0')}:00`);
      if (h < 19) slots.push(`${h.toString().padStart(2, '0')}:30`);
    }
    setTimeSlots(slots);
  };

  const toggleService = (s: any) => {
    setSelectedServices(prev => {
      const idx = prev.findIndex(ss => ss._id === s._id);
      return idx > -1 ? prev.filter(ss => ss._id !== s._id) : [...prev, s];
    });
  };

  const totalAmount = selectedServices.reduce((sum, s) => sum + (s.price || 0), 0);
  const totalDuration = selectedServices.reduce((sum, s) => sum + (s.durationMinutes || 0), 0);

  const nextStep = () => {
    if (step === 1 && selectedServices.length === 0) { toast.error('Chọn ít nhất một dịch vụ'); return; }
    if (step === 3 && !selectedDate) { toast.error('Chọn ngày'); return; }
    if (step === 3 && !selectedTime) { toast.error('Chọn giờ'); return; }
    if (step < 5) {
      const next = step + 1;
      setStep(next);
      if (next === 2) loadStaff();
      if (next === 3) generateTimeSlots();
    }
  };

  const submitBooking = async () => {
    setSubmitting(true);
    try {
      const data: any = {
        services: selectedServices.map(s => s._id),
        appointmentDate: selectedDate,
        startTime: selectedTime,
        notes,
      };
      if (selectedStaff) data.staffId = selectedStaff._id;
      if (selectedPet) data.petId = selectedPet._id;
      if (describePet && petDesc.name) data.petDescription = petDesc;
      const res = await appointmentApi.create(data);
      setBookedCode(res.data?.data?.appointmentCode || res.data?.data?.appointment?.appointmentCode || '');
      setShowSuccess(true);
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Đặt lịch thất bại');
    } finally { setSubmitting(false); }
  };

  return (
    <div className="booking-page">
      <div className="page-header">
        <div className="container">
          <h1>📅 Đặt lịch hẹn</h1>
          <p>Đặt lịch chăm sóc thú cưng chỉ trong vài bước</p>
        </div>
      </div>

      <div className="container">
        {/* Stepper */}
        <div className="stepper">
          {[1, 2, 3, 4, 5].map(s => (
            <div key={s} className={`step ${step === s ? 'active' : ''} ${step > s ? 'completed' : ''}`}>
              <div className="step-circle">{step > s ? '✓' : s}</div>
              <span className="step-label">{['Dịch vụ', 'Nhân viên', 'Ngày giờ', 'Thú cưng', 'Xác nhận'][s - 1]}</span>
            </div>
          ))}
        </div>

        {/* Step 1 */}
        {step === 1 && (
          <div className="step-content">
            <h2>Chọn dịch vụ</h2>
            <div className="services-list">
              {services.map(s => (
                <div key={s._id} className={`service-item ${selectedServices.some(ss => ss._id === s._id) ? 'selected' : ''}`} onClick={() => toggleService(s)}>
                  <div className="service-check">{selectedServices.some(ss => ss._id === s._id) ? '✅' : '⭕'}</div>
                  {s.imageUrl && <div className="service-img"><img src={getImageUrl(s.imageUrl)} alt={s.name} /></div>}
                  <div className="service-info">
                    <h4>{s.name}</h4>
                    <p>{s.description}</p>
                    <div className="service-meta">
                      <span>⏱️ {formatDuration(s.durationMinutes)}</span>
                      <span className="service-price">{formatPrice(s.price)}</span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
            {selectedServices.length > 0 && (
              <div className="selection-summary">
                <span>Đã chọn {selectedServices.length} dịch vụ</span>
                <span>Tổng: {formatPrice(totalAmount)} · {formatDuration(totalDuration)}</span>
              </div>
            )}
          </div>
        )}

        {/* Step 2 */}
        {step === 2 && (
          <div className="step-content">
            <h2>Chọn nhân viên (tùy chọn)</h2>
            <div className="staff-grid">
              {availableStaff.map(staff => (
                <div key={staff._id} className={`staff-card ${selectedStaff?._id === staff._id ? 'selected' : ''}`} onClick={() => setSelectedStaff(staff)}>
                  <img src={getImageUrl(staff.avatarUrl)} alt={staff.fullName} className="staff-avatar" onError={e => { (e.target as HTMLImageElement).src = '/assets/images/default-avatar.svg'; }} />
                  <h4>{staff.nickName || staff.fullName}</h4>
                  <p className="staff-position">{staff.position}</p>
                  <div className="staff-rating">⭐ {staff.averageRating?.toFixed(1) || '0.0'}</div>
                </div>
              ))}
            </div>
            {availableStaff.length === 0 && <p className="skip-note">Không có nhân viên khả dụng. Hệ thống sẽ tự động chỉ định.</p>}
            <button className="btn-skip" onClick={() => { setSelectedStaff(null); nextStep(); }}>Bỏ qua - Để hệ thống chọn</button>
          </div>
        )}

        {/* Step 3 */}
        {step === 3 && (
          <div className="step-content">
            <h2>Chọn ngày và giờ</h2>
            <div className="datetime-grid">
              <div className="date-section">
                <label>Ngày hẹn</label>
                <input type="date" value={selectedDate} min={minDate} onChange={e => { setSelectedDate(e.target.value); setSelectedTime(''); loadStaff(); }} />
              </div>
              <div className="time-section">
                <label>Giờ hẹn</label>
                <div className="time-slots">
                  {timeSlots.map(slot => (
                    <button key={slot} className={selectedTime === slot ? 'selected' : ''} onClick={() => setSelectedTime(slot)}>{slot}</button>
                  ))}
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Step 4 */}
        {step === 4 && (
          <div className="step-content">
            <h2>Chọn thú cưng (tùy chọn)</h2>
            <div className="pet-mode-tabs">
              <button className={!describePet ? 'active' : ''} onClick={() => setDescribePet(false)}>📋 Chọn có sẵn</button>
              <button className={describePet ? 'active' : ''} onClick={() => { setDescribePet(true); setSelectedPet(null); }}>✏️ Miêu tả thú cưng</button>
            </div>

            {!describePet && (
              <>
                {myPets.length > 0 ? (
                  <div className="pets-grid">
                    {myPets.map(pet => (
                      <div key={pet._id} className={`pet-card ${selectedPet?._id === pet._id ? 'selected' : ''}`} onClick={() => { setSelectedPet(pet); setDescribePet(false); }}>
                        <img src={getImageUrl(pet.imageUrl)} alt={pet.name} onError={e => { (e.target as HTMLImageElement).src = '/assets/images/default-pet.svg'; }} />
                        <h4>{pet.name}</h4>
                        <p>{pet.type} - {pet.breed}</p>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="skip-note">Bạn chưa có thú cưng nào. <Link to="/my-pets">Thêm thú cưng</Link> hoặc miêu tả bên dưới.</p>
                )}
              </>
            )}

            {describePet && (
              <div className="pet-describe-form">
                <div className="form-row">
                  <div className="form-group">
                    <label>Tên thú cưng *</label>
                    <input value={petDesc.name} onChange={e => setPetDesc(p => ({ ...p, name: e.target.value }))} placeholder="VD: Milo, Lucky..." />
                  </div>
                  <div className="form-group">
                    <label>Loại *</label>
                    <select value={petDesc.type} onChange={e => setPetDesc(p => ({ ...p, type: e.target.value }))}>
                      <option value="">-- Chọn loại --</option>
                      {petTypes.map(t => <option key={t.value} value={t.value}>{t.label}</option>)}
                    </select>
                  </div>
                </div>
                <div className="form-row">
                  <div className="form-group">
                    <label>Giới tính</label>
                    <select value={petDesc.gender} onChange={e => setPetDesc(p => ({ ...p, gender: e.target.value }))}>
                      <option value="">-- Chọn --</option>
                      {petGenders.map(g => <option key={g.value} value={g.value}>{g.label}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Tuổi (năm)</label>
                    <input type="number" value={petDesc.age || ''} onChange={e => setPetDesc(p => ({ ...p, age: +e.target.value }))} min={0} max={30} />
                  </div>
                </div>
                <div className="form-row">
                  <div className="form-group">
                    <label>Cân nặng (kg)</label>
                    <input type="number" value={petDesc.weight || ''} onChange={e => setPetDesc(p => ({ ...p, weight: +e.target.value }))} min={0} step={0.1} />
                  </div>
                  <div className="form-group">
                    <label>Ghi chú sức khỏe</label>
                    <input value={petDesc.notes} onChange={e => setPetDesc(p => ({ ...p, notes: e.target.value }))} placeholder="Dị ứng, bệnh lý..." />
                  </div>
                </div>
              </div>
            )}

            <button className="btn-skip" onClick={() => { setSelectedPet(null); setDescribePet(false); nextStep(); }}>Bỏ qua</button>
          </div>
        )}

        {/* Step 5 */}
        {step === 5 && (
          <div className="step-content">
            <h2>Xác nhận đặt lịch</h2>
            <div className="confirm-card">
              <div className="confirm-section">
                <h4>🔔 Dịch vụ đã chọn</h4>
                <ul>{selectedServices.map(s => <li key={s._id}>{s.name} - {formatPrice(s.price)} ({formatDuration(s.durationMinutes)})</li>)}</ul>
              </div>
              {selectedStaff && <div className="confirm-section"><h4>👤 Nhân viên</h4><p>{selectedStaff.nickName || selectedStaff.fullName}</p></div>}
              <div className="confirm-section"><h4>📅 Ngày giờ</h4><p>{selectedDate} lúc {selectedTime}</p></div>
              {selectedPet && <div className="confirm-section"><h4>🐾 Thú cưng</h4><p>{selectedPet.name} ({selectedPet.type})</p></div>}
              {describePet && petDesc.name && (
                <div className="confirm-section"><h4>🐾 Thú cưng (miêu tả)</h4><p><strong>{petDesc.name}</strong> - {petDesc.type}</p></div>
              )}
              <div className="confirm-section">
                <h4>📝 Ghi chú</h4>
                <textarea value={notes} onChange={e => setNotes(e.target.value)} placeholder="Ghi chú thêm..." rows={3} />
              </div>
              <div className="confirm-total"><span>Tổng cộng:</span><strong>{formatPrice(totalAmount)}</strong></div>
            </div>
          </div>
        )}

        {/* Nav */}
        <div className="step-actions">
          {step > 1 && <button className="btn-prev" onClick={() => setStep(s => s - 1)}>← Quay lại</button>}
          <div className="spacer"></div>
          {step < 5 && <button className="btn-next" onClick={nextStep}>Tiếp theo →</button>}
          {step === 5 && <button className="btn-submit" onClick={submitBooking} disabled={submitting}>{submitting ? '⏳ Đang xử lý...' : '✅ Xác nhận đặt lịch'}</button>}
        </div>

        {/* Success Modal */}
        {showSuccess && (
          <div className="success-overlay">
            <div className="success-modal">
              <div className="success-icon">✅</div>
              <h2>Đặt lịch thành công!</h2>
              <p>Lịch hẹn của bạn đã được tạo thành công</p>
              {bookedCode && <p className="success-code">Mã lịch hẹn: <strong>{bookedCode}</strong></p>}
              <div className="success-details">
                <div>📅 {selectedDate} lúc {selectedTime}</div>
                {selectedStaff && <div>👤 {selectedStaff.nickName || selectedStaff.fullName}</div>}
                <div>🔔 {selectedServices.length} dịch vụ · {formatPrice(totalAmount)}</div>
              </div>
              <Link className="btn-go" to="/my-appointments">📋 Xem lịch hẹn của tôi</Link>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
