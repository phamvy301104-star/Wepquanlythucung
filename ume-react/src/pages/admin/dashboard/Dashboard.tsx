import { useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import { Chart, registerables } from 'chart.js';
import { adminApi, appointmentApi, orderApi } from '../../../services/api';
import '../shared/admin.scss';
import './Dashboard.scss';

Chart.register(...registerables);

const fmt = (n: number) => new Intl.NumberFormat('vi-VN').format(n);

export default function Dashboard() {
  const [stats, setStats] = useState<any>({});
  const [todayAppts, setTodayAppts] = useState<any[]>([]);
  const [recentOrders, setRecentOrders] = useState<any[]>([]);
  const revenueRef = useRef<HTMLCanvasElement>(null);
  const orderRef = useRef<HTMLCanvasElement>(null);
  const chartInstances = useRef<any[]>([]);

  useEffect(() => {
    loadDashboard();
    loadAppointments();
    loadOrders();
    return () => chartInstances.current.forEach(c => c?.destroy());
  }, []);

  async function loadDashboard() {
    try {
      const r = await adminApi.getDashboard();
      const d = r.data?.data || r.data;
      if (d) {
        setStats({
          revenueToday: d.revenueToday || d.todayRevenue || 0,
          revenueMonth: d.revenueMonth || d.monthRevenue || 0,
          revenueGrowth: d.revenueGrowth || 0,
          ordersToday: d.ordersToday || d.todayOrders || 0,
          pendingOrders: d.pendingOrders || 0,
          appointmentsToday: d.appointmentsToday || d.todayAppointments || 0,
          pendingAppointments: d.pendingAppointments || 0,
          appointmentsMonth: d.appointmentsMonth || 0,
          totalCustomers: d.totalCustomers || d.totalUsers || 0,
          newCustomersMonth: d.newCustomersMonth || d.newUsersMonth || 0,
          totalProducts: d.totalProducts || 0,
          lowStock: d.lowStockProducts || d.lowStock || 0,
          totalServices: d.totalServices || 0,
          activeStaff: d.activeStaff || d.totalStaff || 0,
        });
        setTimeout(() => {
          initCharts(
            d.revenueChart || d.revenueChartData || [],
            d.orderStatusChart || d.orderStatus || {}
          );
        }, 200);
      }
    } catch { /* ignore */ }
  }

  async function loadAppointments() {
    try {
      const r = await appointmentApi.getAll({ limit: '5', sort: '-appointmentDate' });
      const d = r.data?.data || r.data;
      setTodayAppts((d?.appointments || d || []).slice(0, 5));
    } catch { /* ignore */ }
  }

  async function loadOrders() {
    try {
      const r = await orderApi.getAll({ limit: '5', sort: '-createdAt' });
      const d = r.data?.data || r.data;
      setRecentOrders((d?.orders || d || []).slice(0, 5));
    } catch { /* ignore */ }
  }

  function initCharts(revenueData: any[], orderStatus: any) {
    chartInstances.current.forEach(c => c?.destroy());
    chartInstances.current = [];

    if (revenueRef.current) {
      const labels = revenueData.map((d: any) => d.label || d.date || '');
      const data = revenueData.map((d: any) => d.totalRevenue || d.revenue || d.total || 0);
      const c = new Chart(revenueRef.current, {
        type: 'line',
        data: {
          labels: labels.length ? labels : ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'],
          datasets: [{
            label: 'Doanh thu',
            data: data.length ? data : [0, 0, 0, 0, 0, 0, 0],
            borderColor: '#667eea',
            backgroundColor: 'rgba(212,175,55,.1)',
            fill: true, tension: 0.4, borderWidth: 3,
          }]
        },
        options: {
          responsive: true, maintainAspectRatio: false,
          plugins: {
            legend: { position: 'top' },
            tooltip: { callbacks: { label: (ctx) => ctx.dataset.label + ': ' + fmt(ctx.raw as number) + 'đ' } }
          },
          scales: {
            y: {
              beginAtZero: true,
              ticks: {
                callback: (v) => {
                  const n = Number(v);
                  return n >= 1e6 ? (n / 1e6).toFixed(1) + 'M' : n >= 1e3 ? (n / 1e3).toFixed(0) + 'K' : String(n);
                }
              }
            }
          }
        }
      });
      chartInstances.current.push(c);
    }

    if (orderRef.current) {
      const c = new Chart(orderRef.current, {
        type: 'doughnut',
        data: {
          labels: ['Chờ xử lý', 'Đã xác nhận', 'Đang xử lý', 'Đang giao', 'Hoàn thành', 'Đã hủy'],
          datasets: [{
            data: [
              orderStatus.pending || 0, orderStatus.confirmed || 0, orderStatus.processing || 0,
              orderStatus.shipping || 0, orderStatus.completed || 0, orderStatus.cancelled || 0
            ],
            backgroundColor: ['#FF9800', '#2196F3', '#9C27B0', '#00BCD4', '#4CAF50', '#E53935'],
            borderWidth: 0,
          }]
        },
        options: {
          responsive: true, maintainAspectRatio: false, cutout: '60%',
          plugins: { legend: { position: 'bottom', labels: { padding: 15, usePointStyle: true, font: { size: 11 } } } }
        }
      });
      chartInstances.current.push(c);
    }
  }

  const getServiceNames = (a: any) => {
    if (a.serviceName) return a.serviceName;
    if (a.services?.length) return a.services.map((s: any) => s.name || s.service?.name).join(', ');
    return 'N/A';
  };

  const stCls: Record<string, string> = { Pending: 'pending', Confirmed: 'confirmed', InProgress: 'processing', Completed: 'completed', Cancelled: 'cancelled' };
  const stTxt: Record<string, string> = { Pending: 'Chờ', Confirmed: 'Xác nhận', InProgress: 'Đang làm', Completed: 'Xong', Cancelled: 'Hủy' };
  const ordCls: Record<string, string> = { Pending: 'pending', Confirmed: 'confirmed', Processing: 'processing', Shipping: 'shipping', Completed: 'completed', Cancelled: 'cancelled' };
  const ordTxt: Record<string, string> = { Pending: 'Chờ', Confirmed: 'Xác nhận', Processing: 'Xử lý', Shipping: 'Giao', Completed: 'Xong', Cancelled: 'Hủy' };

  return (
    <div className="dashboard">
      <div className="dash-header">
        <h2>📊 Dashboard</h2>
        <a href="/" target="_blank" rel="noopener noreferrer" className="btn-outline-gold">🏠 Xem trang chủ</a>
      </div>

      {/* Stat boxes */}
      <div className="dash-stats">
        <div className="small-box bg-gradient-gold">
          <div className="inner"><h3>{fmt(stats.revenueToday || 0)}<sup>đ</sup></h3><p>Doanh thu hôm nay</p></div>
          <div className="sb-icon-bg">💰</div>
          <div className="sb-footer">Chi tiết →</div>
        </div>
        <div className="small-box bg-gradient-info">
          <div className="inner"><h3>{fmt(stats.revenueMonth || 0)}<sup>đ</sup></h3><p>Doanh thu tháng</p></div>
          <div className="sb-icon-bg">📈</div>
          <div className="sb-footer">
            {(stats.revenueGrowth || 0) >= 0 ? `↑ ${stats.revenueGrowth}%` : `↓ ${Math.abs(stats.revenueGrowth)}%`} so tháng trước
          </div>
        </div>
        <div className="small-box bg-gradient-success">
          <div className="inner"><h3>{stats.ordersToday || 0}</h3><p>Đơn hàng hôm nay</p></div>
          <div className="sb-icon-bg">🛒</div>
          <Link to="/admin/orders" className="sb-footer"><span className="sb-badge">{stats.pendingOrders || 0}</span> chờ xử lý →</Link>
        </div>
        <div className="small-box bg-gradient-warning">
          <div className="inner"><h3>{stats.appointmentsToday || 0}</h3><p>Lịch hẹn hôm nay</p></div>
          <div className="sb-icon-bg">📅</div>
          <Link to="/admin/appointments" className="sb-footer"><span className="sb-badge">{stats.pendingAppointments || 0}</span> chờ xác nhận →</Link>
        </div>
      </div>

      {/* Quick actions */}
      <div className="card">
        <div className="card-header"><h3 className="card-title">⚡ Thao tác nhanh</h3></div>
        <div className="card-body">
          <div className="qa-grid">
            {[
              { to: '/admin/products', icon: '➕', label: 'Thêm sản phẩm' },
              { to: '/admin/orders', icon: '⏰', label: 'Đơn chờ xử lý' },
              { to: '/admin/appointments', icon: '📅', label: 'Tạo lịch hẹn' },
              { to: '/admin/pets', icon: '🐾', label: 'Thú cưng' },
              { to: '/admin/staff', icon: '👔', label: 'Quản lý NV' },
              { to: '/admin/reports', icon: '📊', label: 'Báo cáo' },
              { to: '/admin/users', icon: '👥', label: 'Khách hàng' },
              { to: '/admin/services', icon: '✂️', label: 'Dịch vụ' },
              { to: '/admin/categories', icon: '📁', label: 'Danh mục' },
              { to: '/admin/brands', icon: '🏷️', label: 'Thương hiệu' },
              { to: '/admin/promotions', icon: '💰', label: 'Khuyến mãi' },
              { to: '/admin/settings', icon: '⚙️', label: 'Cài đặt' },
            ].map(a => (
              <Link key={a.to} to={a.to} className="qa-btn">
                <span className="qa-icon">{a.icon}</span><span>{a.label}</span>
              </Link>
            ))}
          </div>
        </div>
      </div>

      {/* Charts */}
      <div className="chart-row">
        <div className="chart-col-lg">
          <div className="card">
            <div className="card-header"><h3 className="card-title">📈 Doanh thu 7 ngày</h3></div>
            <div className="card-body"><div className="chart-box"><canvas ref={revenueRef} /></div></div>
          </div>
        </div>
        <div className="chart-col-sm">
          <div className="card">
            <div className="card-header"><h3 className="card-title">🍩 Trạng thái đơn</h3></div>
            <div className="card-body"><div className="chart-box"><canvas ref={orderRef} /></div></div>
          </div>
        </div>
      </div>

      {/* Info boxes */}
      <div className="info-row">
        <div className="info-box"><span className="ib-icon" style={{ background: '#667eea' }}>👥</span>
          <div className="ib-content"><span className="ib-text">Tổng khách hàng</span><span className="ib-num">{stats.totalCustomers || 0}</span><small className="text-success">+{stats.newCustomersMonth || 0} tháng này</small></div>
        </div>
        <div className="info-box"><span className="ib-icon" style={{ background: '#6c757d' }}>📦</span>
          <div className="ib-content"><span className="ib-text">Sản phẩm</span><span className="ib-num">{stats.totalProducts || 0}</span>{(stats.lowStock || 0) > 0 && <small className="text-danger">⚠ {stats.lowStock} sắp hết</small>}</div>
        </div>
        <div className="info-box"><span className="ib-icon" style={{ background: '#28a745' }}>✂️</span>
          <div className="ib-content"><span className="ib-text">Dịch vụ</span><span className="ib-num">{stats.totalServices || 0}</span><small className="text-info">📅 {stats.appointmentsMonth || 0} lịch tháng này</small></div>
        </div>
        <div className="info-box"><span className="ib-icon" style={{ background: '#ffc107' }}>👔</span>
          <div className="ib-content"><span className="ib-text">Nhân viên</span><span className="ib-num">{stats.activeStaff || 0}</span><small>đang hoạt động</small></div>
        </div>
      </div>

      {/* Tables */}
      <div className="table-row">
        <div className="table-col">
          <div className="card">
            <div className="card-header">
              <h3 className="card-title">📅 Lịch hẹn hôm nay <span className="badge-gold">{stats.appointmentsToday || 0}</span></h3>
              <Link to="/admin/appointments" className="link-muted">Xem tất cả →</Link>
            </div>
            <div className="card-body p-0 scroll-area">
              {todayAppts.length > 0 ? (
                <table className="adm-table">
                  <thead><tr><th>Giờ</th><th>Khách hàng</th><th>Dịch vụ</th><th>TT</th></tr></thead>
                  <tbody>
                    {todayAppts.map((a, i) => (
                      <tr key={i}>
                        <td className="text-gold fw-b">{a.startTime || '--'}</td>
                        <td>{a.customerName || a.guestName || a.user?.fullName || 'N/A'}<br /><small className="text-muted">{a.customerPhone || a.guestPhone || ''}</small></td>
                        <td>{getServiceNames(a)}<br /><small className="text-muted">{a.staff?.fullName || ''}</small></td>
                        <td><span className={`os-badge os-${stCls[a.status] || 'pending'}`}>{stTxt[a.status] || a.status}</span></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              ) : (
                <div className="empty-s">📅<p>Không có lịch hẹn</p></div>
              )}
            </div>
          </div>
        </div>
        <div className="table-col">
          <div className="card">
            <div className="card-header">
              <h3 className="card-title">🛍️ Đơn hàng gần đây</h3>
              <Link to="/admin/orders" className="link-muted">Xem tất cả →</Link>
            </div>
            <div className="card-body p-0 scroll-area">
              {recentOrders.length > 0 ? (
                <table className="adm-table">
                  <thead><tr><th>Mã đơn</th><th>Khách</th><th className="text-right">Tổng</th><th>TT</th></tr></thead>
                  <tbody>
                    {recentOrders.map((o, i) => (
                      <tr key={i}>
                        <td><strong>{o.orderCode || o._id?.substring(0, 8)}</strong><br /><small className="text-muted">{new Date(o.createdAt).toLocaleDateString('vi-VN')} {new Date(o.createdAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}</small></td>
                        <td>{o.user?.fullName || o.customerName || 'N/A'}</td>
                        <td className="text-right fw-b">{fmt(o.totalAmount || 0)}đ</td>
                        <td><span className={`os-badge os-${ordCls[o.status] || 'pending'}`}>{ordTxt[o.status] || o.status}</span></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              ) : (
                <div className="empty-s">📦<p>Chưa có đơn hàng</p></div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
