import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { settingsApi, uploadApi, getImageUrl } from '../../../services/api';
import '../shared/admin.scss';

const TABS = [
  { key: 'contact', icon: '📇', label: 'Liên hệ' },
  { key: 'shipping', icon: '🚚', label: 'Vận chuyển' },
  { key: 'payment', icon: '💳', label: 'Thanh toán' },
  { key: 'policy', icon: '📄', label: 'Chính sách' },
];

const defaults: any = {
  storeName: '', storeDescription: '', address: '', phone: '', email: '', workingHours: '',
  facebook: '', instagram: '', tiktok: '', youtube: '', zalo: '', mapEmbedUrl: '',
  shippingStandardPrice: 30000, shippingExpressPrice: 50000, freeShipStandardThreshold: 500000, freeShipExpressThreshold: 1000000,
  codEnabled: true, codDescription: '', bankTransferEnabled: true, bankName: '', bankAccountNumber: '', bankAccountHolder: '', bankBranch: '', bankDescription: '', bankQrImage: '',
  shippingPolicy: '', returnPolicy: '',
};

export default function AdminSettings() {
  const [activeTab, setActiveTab] = useState('contact');
  const [s, setS] = useState<any>({ ...defaults });
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [uploadingQr, setUploadingQr] = useState(false);

  useEffect(() => { loadData(); }, []);

  async function loadData() {
    setLoading(true);
    try {
      const r = await settingsApi.getSettings();
      const d = r.data?.data || r.data;
      setS({ ...defaults, ...d });
    } catch {}
    setLoading(false);
  }

  function u(field: string, value: any) { setS((prev: any) => ({ ...prev, [field]: value })); }

  async function save() {
    setSaving(true);
    try {
      await settingsApi.updateSettings(s);
      toast.success('Đã lưu cài đặt!');
    } catch { toast.error('Có lỗi xảy ra'); }
    setSaving(false);
  }

  async function onQrUpload(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > 5 * 1024 * 1024) { toast.error('File không quá 5MB'); return; }
    setUploadingQr(true);
    try {
      const formData = new FormData();
      formData.append('file', file);
      formData.append('folder', 'settings');
      const r = await uploadApi.upload(formData);
      const url = r.data?.data?.url || r.data?.url || r.data;
      u('bankQrImage', url);
      toast.success('Đã upload QR!');
    } catch { toast.error('Upload thất bại'); }
    setUploadingQr(false);
  }

  if (loading) return <div className="card"><div className="card-body">Đang tải cài đặt...</div></div>;

  return (
    <div>
      <div className="page-header">
        <div><h4>⚙️ Cài đặt hệ thống</h4>
          <ol className="breadcrumb"><li><Link to="/admin">Dashboard</Link></li><li className="active">Cài đặt</li></ol>
        </div>
      </div>

      <div style={{ display: 'flex', gap: 8, marginBottom: 20, flexWrap: 'wrap' }}>
        {TABS.map(t => (
          <button key={t.key} className={`btn ${activeTab === t.key ? 'btn-gold' : 'btn-outline-gold'}`} onClick={() => setActiveTab(t.key)}>{t.icon} {t.label}</button>
        ))}
      </div>

      {/* Contact Tab */}
      {activeTab === 'contact' && (
        <div className="row">
          <div className="adm-col-6">
            <div className="card" style={{ marginBottom: 16 }}>
              <div className="card-header"><h6>🏪 Thông tin cửa hàng</h6></div>
              <div className="card-body">
                <div className="fg"><label>Tên cửa hàng</label><input className="form-control" value={s.storeName} onChange={e => u('storeName', e.target.value)} /></div>
                <div className="fg"><label>Mô tả</label><textarea className="form-control" rows={3} value={s.storeDescription} onChange={e => u('storeDescription', e.target.value)} /></div>
              </div>
            </div>
            <div className="card" style={{ marginBottom: 16 }}>
              <div className="card-header"><h6>📞 Liên hệ</h6></div>
              <div className="card-body">
                <div className="fg"><label>Địa chỉ</label><input className="form-control" value={s.address} onChange={e => u('address', e.target.value)} /></div>
                <div className="fg"><label>SĐT</label><input className="form-control" value={s.phone} onChange={e => u('phone', e.target.value)} /></div>
                <div className="fg"><label>Email</label><input className="form-control" type="email" value={s.email} onChange={e => u('email', e.target.value)} /></div>
                <div className="fg"><label>Giờ làm việc</label><input className="form-control" value={s.workingHours} onChange={e => u('workingHours', e.target.value)} /></div>
              </div>
            </div>
          </div>
          <div className="adm-col-6">
            <div className="card" style={{ marginBottom: 16 }}>
              <div className="card-header"><h6>🌐 Mạng xã hội</h6></div>
              <div className="card-body">
                {['facebook', 'instagram', 'tiktok', 'youtube', 'zalo'].map(k => (
                  <div className="fg" key={k}><label>{k.charAt(0).toUpperCase() + k.slice(1)}</label><input className="form-control" type="url" value={s[k]} onChange={e => u(k, e.target.value)} placeholder={`https://...`} /></div>
                ))}
              </div>
            </div>
            <div className="card" style={{ marginBottom: 16 }}>
              <div className="card-header"><h6>🗺️ Google Maps</h6></div>
              <div className="card-body">
                <div className="fg"><label>Embed URL</label><textarea className="form-control" rows={2} value={s.mapEmbedUrl} onChange={e => u('mapEmbedUrl', e.target.value)} placeholder="https://www.google.com/maps/embed?..." /></div>
                {s.mapEmbedUrl && <iframe src={s.mapEmbedUrl} width="100%" height="200" style={{ border: 0, borderRadius: 8, marginTop: 8 }} allowFullScreen loading="lazy" title="map" />}
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Shipping Tab */}
      {activeTab === 'shipping' && (
        <div className="row">
          <div className="adm-col-6">
            <div className="card" style={{ marginBottom: 16 }}>
              <div className="card-header"><h6>💰 Phí vận chuyển</h6></div>
              <div className="card-body">
                <div className="fg"><label>Giao hàng tiêu chuẩn (VNĐ)</label><input type="number" className="form-control" value={s.shippingStandardPrice} onChange={e => u('shippingStandardPrice', +e.target.value)} /></div>
                <div className="fg"><label>Giao hàng nhanh (VNĐ)</label><input type="number" className="form-control" value={s.shippingExpressPrice} onChange={e => u('shippingExpressPrice', +e.target.value)} /></div>
              </div>
            </div>
          </div>
          <div className="adm-col-6">
            <div className="card" style={{ marginBottom: 16 }}>
              <div className="card-header"><h6>🎁 Miễn phí vận chuyển</h6></div>
              <div className="card-body">
                <div className="fg"><label>Ngưỡng miễn ship tiêu chuẩn (VNĐ)</label><input type="number" className="form-control" value={s.freeShipStandardThreshold} onChange={e => u('freeShipStandardThreshold', +e.target.value)} /></div>
                <div className="fg"><label>Ngưỡng miễn ship nhanh (VNĐ)</label><input type="number" className="form-control" value={s.freeShipExpressThreshold} onChange={e => u('freeShipExpressThreshold', +e.target.value)} /></div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Payment Tab */}
      {activeTab === 'payment' && (
        <div className="row">
          <div className="adm-col-6">
            <div className="card" style={{ marginBottom: 16 }}>
              <div className="card-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <h6>💵 Thanh toán khi nhận hàng (COD)</h6>
                <label className="toggle-switch"><input type="checkbox" checked={s.codEnabled} onChange={e => u('codEnabled', e.target.checked)} /><span className="toggle-slider"></span></label>
              </div>
              <div className="card-body">
                <div className="fg"><label>Mô tả</label><textarea className="form-control" rows={3} value={s.codDescription} onChange={e => u('codDescription', e.target.value)} /></div>
              </div>
            </div>
          </div>
          <div className="adm-col-6">
            <div className="card" style={{ marginBottom: 16 }}>
              <div className="card-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <h6>🏦 Chuyển khoản ngân hàng</h6>
                <label className="toggle-switch"><input type="checkbox" checked={s.bankTransferEnabled} onChange={e => u('bankTransferEnabled', e.target.checked)} /><span className="toggle-slider"></span></label>
              </div>
              {s.bankTransferEnabled && (
                <div className="card-body">
                  <div className="fg"><label>Tên ngân hàng</label><input className="form-control" value={s.bankName} onChange={e => u('bankName', e.target.value)} /></div>
                  <div className="fg"><label>Số tài khoản</label><input className="form-control" value={s.bankAccountNumber} onChange={e => u('bankAccountNumber', e.target.value)} /></div>
                  <div className="fg"><label>Chủ tài khoản</label><input className="form-control" value={s.bankAccountHolder} onChange={e => u('bankAccountHolder', e.target.value)} /></div>
                  <div className="fg"><label>Chi nhánh</label><input className="form-control" value={s.bankBranch} onChange={e => u('bankBranch', e.target.value)} /></div>
                  <div className="fg"><label>Mô tả</label><textarea className="form-control" rows={2} value={s.bankDescription} onChange={e => u('bankDescription', e.target.value)} /></div>
                  <div className="fg">
                    <label>Ảnh QR</label>
                    <input type="file" accept="image/*" onChange={onQrUpload} disabled={uploadingQr} />
                    {uploadingQr && <div className="sub-txt">Đang upload...</div>}
                    {s.bankQrImage && (
                      <div style={{ marginTop: 8, position: 'relative', display: 'inline-block' }}>
                        <img src={getImageUrl(s.bankQrImage)} alt="QR" style={{ maxWidth: 200, borderRadius: 8 }} />
                        <button className="btn btn-danger btn-sm" style={{ position: 'absolute', top: 4, right: 4 }} onClick={() => u('bankQrImage', '')}>✕</button>
                      </div>
                    )}
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Policy Tab */}
      {activeTab === 'policy' && (
        <div>
          <div className="card" style={{ marginBottom: 16 }}>
            <div className="card-header"><h6>🚚 Chính sách giao hàng</h6></div>
            <div className="card-body">
              <textarea className="form-control" rows={8} value={s.shippingPolicy} onChange={e => u('shippingPolicy', e.target.value)} placeholder="Nhập chính sách giao hàng..." />
            </div>
          </div>
          <div className="card" style={{ marginBottom: 16 }}>
            <div className="card-header"><h6>🔄 Chính sách đổi trả</h6></div>
            <div className="card-body">
              <textarea className="form-control" rows={5} value={s.returnPolicy} onChange={e => u('returnPolicy', e.target.value)} placeholder="Nhập chính sách đổi trả..." />
            </div>
          </div>
        </div>
      )}

      <div style={{ textAlign: 'right', marginTop: 20 }}>
        <button className="btn btn-gold" onClick={save} disabled={saving} style={{ padding: '10px 30px' }}>{saving ? 'Đang lưu...' : '💾 Lưu cài đặt'}</button>
      </div>
    </div>
  );
}
