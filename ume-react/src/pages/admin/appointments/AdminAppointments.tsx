import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { appointmentApi, serviceApi, staffApi } from '../../../services/api';
import '../shared/admin.scss';

const STATUS_FLOW: Record<string, string[]> = {
  Pending: ['Confirmed', 'Cancelled'],
  Confirmed: ['InProgress', 'Cancelled'],
  InProgress: ['Completed'],
  Completed: [],
  Cancelled: [],
};
const STATUS_LABELS: Record<string, string> = { Pending: 'Chờ xác nhận', Confirmed: 'Đã xác nhận', InProgress: 'Đang thực hiện', Completed: 'Hoàn thành', Cancelled: 'Đã hủy' };
const STATUS_CLASS: Record<string, string> = { Pending: 'os-pending', Confirmed: 'os-confirmed', InProgress: 'os-processing', Completed: 'os-completed', Cancelled: 'os-cancelled' };

function fmt(d: string) { return d ? new Date(d).toLocaleString('vi-VN') : '-'; }
function fmtN(n: number) { return new Intl.NumberFormat('vi-VN').format(n); }

export default function AdminAppointments() {
  const [items, setItems] = useState<any[]>([]);
  const [services, setServices] = useState<any[]>([]);
  const [staffList, setStaffList] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [dateFilter, setDateFilter] = useState('');
  const [stats, setStats] = useState({ total: 0, pending: 0, confirmed: 0, inProgress: 0, completed: 0, cancelled: 0 });

  const [detail, setDetail] = useState<any>(null);
  const [showDetail, setShowDetail] = useState(false);

  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<any>(null);
  const [fd, setFd] = useState<any>({});
  const [pickedSvcs, setPickedSvcs] = useState<any[]>([]);
  const [saving, setSaving] = useState(false);

  const [showCancel, setShowCancel] = useState(false);
  const [cancelItem, setCancelItem] = useState<any>(null);
  const [cancelReason, setCancelReason] = useState('');

  const [showDelete, setShowDelete] = useState(false);
  const [deleteItem, setDeleteItem] = useState<any>(null);

  useEffect(() => { load(); loadRef(); }, []);

  async function loadRef() {
    try {
      const [sr, str] = await Promise.all([serviceApi.getAll({ limit: '200' }), staffApi.getAll({ limit: '200' })]);
      const sd = sr.data?.data || sr.data;
      setServices(sd?.services || sd || []);
      const std = str.data?.data || str.data;
      setStaffList(std?.staff || std || []);
    } catch {}
  }

  async function load() {
    setLoading(true);
    try {
      const p: any = { limit: '200' };
      if (search) p.search = search;
      if (statusFilter) p.status = statusFilter;
      if (dateFilter) p.date = dateFilter;
      const r = await appointmentApi.getAll(p);
      const d = r.data?.data || r.data;
      const list = d?.appointments || d || [];
      setItems(list);
      setStats({
        total: list.length,
        pending: list.filter((a: any) => a.status === 'Pending').length,
        confirmed: list.filter((a: any) => a.status === 'Confirmed').length,
        inProgress: list.filter((a: any) => a.status === 'InProgress').length,
        completed: list.filter((a: any) => a.status === 'Completed').length,
        cancelled: list.filter((a: any) => a.status === 'Cancelled').length,
      });
    } catch {}
    setLoading(false);
  }

  function openDetail(item: any) { setDetail(item); setShowDetail(true); }

  function openForm(item?: any) {
    setEditing(item || null);
    setFd(item ? {
      customerName: item.customerName || item.user?.fullName || '',
      phone: item.phone || item.user?.phoneNumber || '',
      email: item.email || item.user?.email || '',
      appointmentDate: item.appointmentDate?.substring(0, 16) || '',
      staff: item.staff?._id || item.staff || '',
      notes: item.notes || '',
      petName: item.petName || item.pet?.name || '',
    } : { customerName: '', phone: '', email: '', appointmentDate: '', staff: '', notes: '', petName: '' });
    setPickedSvcs(item?.services?.map((s: any) => ({ _id: s.service?._id || s._id || s, name: s.service?.name || s.name || '', price: s.price || s.service?.price || 0 })) || []);
    setShowForm(true);
  }

  function addSvc(svcId: string) {
    if (!svcId || pickedSvcs.find(p => p._id === svcId)) return;
    const sv = services.find(s => s._id === svcId);
    if (sv) setPickedSvcs([...pickedSvcs, { _id: sv._id, name: sv.name, price: sv.price || 0 }]);
  }

  function removeSvc(id: string) { setPickedSvcs(pickedSvcs.filter(s => s._id !== id)); }

  async function save() {
    if (!fd.customerName || !fd.appointmentDate) { toast.error('Nhập đủ thông tin bắt buộc'); return; }
    setSaving(true);
    try {
      const payload = { ...fd, services: pickedSvcs.map(s => ({ service: s._id, price: s.price })) };
      if (editing) await appointmentApi.update(editing._id, payload);
      else await appointmentApi.create(payload);
      toast.success(editing ? 'Đã cập nhật!' : 'Đã tạo lịch hẹn!');
      setShowForm(false); load();
    } catch { toast.error('Có lỗi xảy ra'); }
    setSaving(false);
  }

  async function updateStatus(item: any, newStatus: string) {
    if (newStatus === 'Cancelled') { setCancelItem(item); setCancelReason(''); setShowCancel(true); return; }
    try {
      await appointmentApi.updateStatus(item._id, newStatus);
      toast.success(`Đã chuyển sang "${STATUS_LABELS[newStatus]}"`);
      if (showDetail) setDetail({ ...detail, status: newStatus });
      load();
    } catch { toast.error('Có lỗi xảy ra'); }
  }

  async function doCancel() {
    if (!cancelItem) return;
    try {
      await appointmentApi.updateStatus(cancelItem._id, 'Cancelled', cancelReason);
      toast.success('Đã hủy lịch hẹn!'); setShowCancel(false);
      if (showDetail) setShowDetail(false);
      load();
    } catch { toast.error('Có lỗi xảy ra'); }
  }

  async function doDelete() {
    if (!deleteItem) return;
    try { await appointmentApi.delete(deleteItem._id); toast.success('Đã xóa!'); setShowDelete(false); load(); }
    catch { toast.error('Có lỗi xảy ra'); }
  }

  function totalSvcs(item: any) { return item.services?.reduce((s: number, sv: any) => s + (sv.price || sv.service?.price || 0), 0) || item.totalAmount || 0; }

  return (
    <div>
      <div className="page-header">
        <div><h4>📅 Quản lý lịch hẹn</h4>
          <ol className="breadcrumb"><li><Link to="/admin">Dashboard</Link></li><li className="active">Lịch hẹn</li></ol>
        </div>
        <button className="btn btn-gold" onClick={() => openForm()}>➕ Tạo lịch hẹn</button>
      </div>

      <div className="stats-row">
        <div className="stat-box"><div className="sb-icon bg-info">📋</div><div><div className="sb-num">{stats.total}</div><div className="sb-label">Tổng cộng</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-warning">⏳</div><div><div className="sb-num">{stats.pending}</div><div className="sb-label">Chờ xác nhận</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-info">✅</div><div><div className="sb-num">{stats.confirmed}</div><div className="sb-label">Đã xác nhận</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-primary">🔄</div><div><div className="sb-num">{stats.inProgress}</div><div className="sb-label">Đang thực hiện</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-success">🎉</div><div><div className="sb-num">{stats.completed}</div><div className="sb-label">Hoàn thành</div></div></div>
        <div className="stat-box"><div className="sb-icon bg-danger">❌</div><div><div className="sb-num">{stats.cancelled}</div><div className="sb-label">Đã hủy</div></div></div>
      </div>

      <div className="card">
        <div className="card-header">
          <div className="filter-row">
            <div className="search-box">
              <input className="form-control" value={search} onChange={e => setSearch(e.target.value)} placeholder="Tìm theo tên, SĐT..." onKeyDown={e => e.key === 'Enter' && load()} />
              <button className="btn btn-gold btn-sm" onClick={load}>🔍</button>
            </div>
            <select className="form-control fc-sm" value={statusFilter} onChange={e => { setStatusFilter(e.target.value); setTimeout(load, 0); }}>
              <option value="">Tất cả trạng thái</option>
              {Object.entries(STATUS_LABELS).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
            </select>
            <input type="date" className="form-control fc-sm" value={dateFilter} onChange={e => { setDateFilter(e.target.value); setTimeout(load, 0); }} />
          </div>
        </div>
        <div className="card-body p-0">
          <table className="adm-table">
            <thead><tr><th>#</th><th>Khách hàng</th><th>SĐT</th><th>Ngày hẹn</th><th>Dịch vụ</th><th>Tổng tiền</th><th>Trạng thái</th><th>Thao tác</th></tr></thead>
            <tbody>
              {items.map((a, i) => (
                <tr key={a._id} className="clickable-row" onClick={() => openDetail(a)}>
                  <td>{i + 1}</td>
                  <td><strong>{a.customerName || a.user?.fullName || '-'}</strong>{a.petName || a.pet?.name ? <div className="sub-txt">🐾 {a.petName || a.pet?.name}</div> : null}</td>
                  <td>{a.phone || a.user?.phoneNumber || '-'}</td>
                  <td>{fmt(a.appointmentDate)}</td>
                  <td>{a.services?.length || 0} dịch vụ</td>
                  <td className="fw-bold">{fmtN(totalSvcs(a))}đ</td>
                  <td><span className={`os-badge ${STATUS_CLASS[a.status] || ''}`}>{STATUS_LABELS[a.status] || a.status}</span></td>
                  <td onClick={e => e.stopPropagation()}><div className="act-g">
                    {STATUS_FLOW[a.status]?.length > 0 && (
                      <select className="form-control fc-sm" style={{ minWidth: 130, fontSize: '0.8rem' }} defaultValue="" onChange={e => { if (e.target.value) updateStatus(a, e.target.value); e.target.value = ''; }}>
                        <option value="" disabled>Chuyển TT</option>
                        {STATUS_FLOW[a.status].map(ns => <option key={ns} value={ns}>{STATUS_LABELS[ns]}</option>)}
                      </select>
                    )}
                    <button className="ab ab-edit" onClick={() => openForm(a)}>✏️</button>
                    <button className="ab ab-del" onClick={() => { setDeleteItem(a); setShowDelete(true); }}>🗑️</button>
                  </div></td>
                </tr>
              ))}
              {!items.length && !loading && <tr><td colSpan={8} className="empty">Không có lịch hẹn nào</td></tr>}
              {loading && <tr><td colSpan={8} className="empty">Đang tải...</td></tr>}
            </tbody>
          </table>
        </div>
      </div>

      {/* Detail Modal */}
      {showDetail && detail && (
        <div className="mo" onClick={() => setShowDetail(false)}>
          <div className="md md-lg" onClick={e => e.stopPropagation()}>
            <div className="mh bg-g"><h5>📋 Chi tiết lịch hẹn #{detail._id?.slice(-6)}</h5><button className="mx" onClick={() => setShowDetail(false)}>&times;</button></div>
            <div className="mb-modal">
              {/* Status flow */}
              <div style={{ display: 'flex', gap: 8, marginBottom: 16, flexWrap: 'wrap', alignItems: 'center' }}>
                {['Pending', 'Confirmed', 'InProgress', 'Completed'].map((st, idx) => (
                  <span key={st}>
                    {idx > 0 && <span style={{ margin: '0 4px', color: '#aaa' }}>→</span>}
                    <span className={`os-badge ${STATUS_CLASS[st]}`} style={{ opacity: detail.status === st || (Object.keys(STATUS_LABELS).indexOf(detail.status) >= Object.keys(STATUS_LABELS).indexOf(st) && detail.status !== 'Cancelled') ? 1 : 0.3 }}>{STATUS_LABELS[st]}</span>
                  </span>
                ))}
                {detail.status === 'Cancelled' && <span className="os-badge os-cancelled" style={{ marginLeft: 8 }}>❌ Đã hủy</span>}
              </div>

              <div className="row">
                <div className="adm-col-6">
                  <h6>👤 Thông tin khách hàng</h6>
                  <p><strong>Tên:</strong> {detail.customerName || detail.user?.fullName}</p>
                  <p><strong>SĐT:</strong> {detail.phone || detail.user?.phoneNumber}</p>
                  <p><strong>Email:</strong> {detail.email || detail.user?.email || '-'}</p>
                  {(detail.petName || detail.pet?.name) && <p><strong>Thú cưng:</strong> 🐾 {detail.petName || detail.pet?.name}</p>}
                </div>
                <div className="adm-col-6">
                  <h6>📅 Thông tin lịch hẹn</h6>
                  <p><strong>Ngày hẹn:</strong> {fmt(detail.appointmentDate)}</p>
                  <p><strong>Nhân viên:</strong> {detail.staff?.fullName || '-'}</p>
                  <p><strong>Trạng thái:</strong> <span className={`os-badge ${STATUS_CLASS[detail.status]}`}>{STATUS_LABELS[detail.status]}</span></p>
                  {detail.notes && <p><strong>Ghi chú:</strong> {detail.notes}</p>}
                  {detail.cancelReason && <p className="text-danger"><strong>Lý do hủy:</strong> {detail.cancelReason}</p>}
                </div>
              </div>

              {detail.services?.length > 0 && (
                <div style={{ marginTop: 16 }}>
                  <h6>🛎️ Dịch vụ</h6>
                  <table className="adm-table bordered-table">
                    <thead><tr><th>Dịch vụ</th><th>Giá</th></tr></thead>
                    <tbody>
                      {detail.services.map((sv: any, idx: number) => (
                        <tr key={idx}><td>{sv.service?.name || sv.name || '-'}</td><td>{fmtN(sv.price || sv.service?.price || 0)}đ</td></tr>
                      ))}
                      <tr><td className="fw-bold">Tổng cộng</td><td className="fw-bold">{fmtN(totalSvcs(detail))}đ</td></tr>
                    </tbody>
                  </table>
                </div>
              )}

              {STATUS_FLOW[detail.status]?.length > 0 && (
                <div style={{ marginTop: 16, display: 'flex', gap: 8 }}>
                  {STATUS_FLOW[detail.status].map(ns => (
                    <button key={ns} className={`btn ${ns === 'Cancelled' ? 'btn-danger' : 'btn-gold'}`} onClick={() => updateStatus(detail, ns)}>{STATUS_LABELS[ns]}</button>
                  ))}
                </div>
              )}
            </div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowDetail(false)}>Đóng</button></div>
          </div>
        </div>
      )}

      {/* Create/Edit Form */}
      {showForm && (
        <div className="mo" onClick={() => setShowForm(false)}>
          <div className="md md-lg" onClick={e => e.stopPropagation()}>
            <div className="mh bg-g"><h5>{editing ? '✏️ Sửa' : '➕ Tạo'} lịch hẹn</h5><button className="mx" onClick={() => setShowForm(false)}>&times;</button></div>
            <div className="mb-modal">
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>Tên khách hàng <span className="req">*</span></label><input className="form-control" value={fd.customerName} onChange={e => setFd({ ...fd, customerName: e.target.value })} /></div></div>
                <div className="adm-col-6"><div className="fg"><label>SĐT</label><input className="form-control" value={fd.phone} onChange={e => setFd({ ...fd, phone: e.target.value })} /></div></div>
              </div>
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>Email</label><input className="form-control" value={fd.email} onChange={e => setFd({ ...fd, email: e.target.value })} /></div></div>
                <div className="adm-col-6"><div className="fg"><label>Tên thú cưng</label><input className="form-control" value={fd.petName} onChange={e => setFd({ ...fd, petName: e.target.value })} /></div></div>
              </div>
              <div className="row">
                <div className="adm-col-6"><div className="fg"><label>Ngày giờ hẹn <span className="req">*</span></label><input type="datetime-local" className="form-control" value={fd.appointmentDate} onChange={e => setFd({ ...fd, appointmentDate: e.target.value })} /></div></div>
                <div className="adm-col-6"><div className="fg"><label>Nhân viên</label>
                  <select className="form-control" value={fd.staff} onChange={e => setFd({ ...fd, staff: e.target.value })}>
                    <option value="">-- Chọn nhân viên --</option>
                    {staffList.filter(s => s.status === 'Active').map(s => <option key={s._id} value={s._id}>{s.fullName}</option>)}
                  </select>
                </div></div>
              </div>
              <div className="fg"><label>Ghi chú</label><textarea className="form-control" rows={2} value={fd.notes} onChange={e => setFd({ ...fd, notes: e.target.value })} /></div>

              <div className="fg">
                <label>Thêm dịch vụ</label>
                <select className="form-control" onChange={e => { addSvc(e.target.value); e.target.value = ''; }} defaultValue="">
                  <option value="">-- Chọn dịch vụ --</option>
                  {services.filter(s => !pickedSvcs.find(p => p._id === s._id)).map(s => <option key={s._id} value={s._id}>{s.name} - {fmtN(s.price || 0)}đ</option>)}
                </select>
                {pickedSvcs.length > 0 && (
                  <table className="adm-table" style={{ marginTop: 8 }}>
                    <thead><tr><th>Dịch vụ</th><th>Giá</th><th></th></tr></thead>
                    <tbody>
                      {pickedSvcs.map(s => (
                        <tr key={s._id}><td>{s.name}</td><td>{fmtN(s.price)}đ</td><td><button className="ab ab-del" onClick={() => removeSvc(s._id)}>✕</button></td></tr>
                      ))}
                      <tr><td className="fw-bold">Tổng cộng</td><td className="fw-bold">{fmtN(pickedSvcs.reduce((t, s) => t + s.price, 0))}đ</td><td></td></tr>
                    </tbody>
                  </table>
                )}
              </div>
            </div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowForm(false)}>Hủy</button><button className="btn btn-gold" onClick={save} disabled={saving}>{saving ? 'Đang lưu...' : '💾 Lưu'}</button></div>
          </div>
        </div>
      )}

      {/* Cancel Modal */}
      {showCancel && (
        <div className="mo" onClick={() => setShowCancel(false)}>
          <div className="md" onClick={e => e.stopPropagation()}>
            <div className="mh bg-r"><h5>❌ Hủy lịch hẹn</h5><button className="mx" onClick={() => setShowCancel(false)}>&times;</button></div>
            <div className="mb-modal">
              <p>Bạn có chắc muốn hủy lịch hẹn của <strong>{cancelItem?.customerName || cancelItem?.user?.fullName}</strong>?</p>
              <div className="fg"><label>Lý do hủy</label><textarea className="form-control" rows={3} value={cancelReason} onChange={e => setCancelReason(e.target.value)} placeholder="Nhập lý do hủy..." /></div>
            </div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowCancel(false)}>Đóng</button><button className="btn btn-danger" onClick={doCancel}>❌ Xác nhận hủy</button></div>
          </div>
        </div>
      )}

      {/* Delete Modal */}
      {showDelete && (
        <div className="mo" onClick={() => setShowDelete(false)}>
          <div className="md" onClick={e => e.stopPropagation()}>
            <div className="mh bg-r"><h5>🗑️ Xóa lịch hẹn</h5><button className="mx" onClick={() => setShowDelete(false)}>&times;</button></div>
            <div className="mb-modal"><p>Bạn có chắc muốn xóa lịch hẹn này?</p><p className="text-danger">Hành động này không thể hoàn tác!</p></div>
            <div className="mf"><button className="btn btn-sec" onClick={() => setShowDelete(false)}>Hủy</button><button className="btn btn-danger" onClick={doDelete}>🗑️ Xác nhận xóa</button></div>
          </div>
        </div>
      )}
    </div>
  );
}
