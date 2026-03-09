import { useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import { Chart, registerables } from 'chart.js';
import { adminApi } from '../../../services/api';
import '../shared/admin.scss';

Chart.register(...registerables);

function fmtN(n: number) { return new Intl.NumberFormat('vi-VN').format(n); }

export default function AdminReports() {
  const [period, setPeriod] = useState('month');
  const [data, setData] = useState<any>(null);
  const [loading, setLoading] = useState(false);
  const barRef = useRef<HTMLCanvasElement>(null);
  const doughRef = useRef<HTMLCanvasElement>(null);
  const charts = useRef<(Chart | null)[]>([]);

  useEffect(() => { load(); }, [period]);
  useEffect(() => { return () => { charts.current.forEach(c => c?.destroy()); }; }, []);

  async function load() {
    setLoading(true);
    try {
      const r = await adminApi.getReports({ period });
      const d = r.data?.data || r.data;
      setData(d);
      setTimeout(() => drawCharts(d), 100);
    } catch {}
    setLoading(false);
  }

  function drawCharts(d: any) {
    charts.current.forEach(c => c?.destroy());
    charts.current = [];
    if (!d) return;

    // Revenue bar chart
    if (barRef.current) {
      const labels = d.revenueChart?.labels || d.chartLabels || [];
      const values = d.revenueChart?.data || d.chartData || [];
      charts.current.push(new Chart(barRef.current, {
        type: 'bar',
        data: { labels, datasets: [{ label: 'Doanh thu (VNĐ)', data: values, backgroundColor: 'rgba(102, 126, 234, 0.6)', borderColor: '#667eea', borderWidth: 1 }] },
        options: { responsive: true, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true, ticks: { callback: (v: any) => fmtN(v) } } } },
      }));
    }

    // Order status doughnut
    if (doughRef.current && d.orderStats) {
      const statuses = d.orderStats;
      charts.current.push(new Chart(doughRef.current, {
        type: 'doughnut',
        data: {
          labels: Object.keys(statuses),
          datasets: [{ data: Object.values(statuses), backgroundColor: ['#ffc107', '#17a2b8', '#6f42c1', '#007bff', '#28a745', '#dc3545'] }],
        },
        options: { responsive: true, plugins: { legend: { position: 'bottom' } } },
      }));
    }
  }

  return (
    <div>
      <div className="page-header">
        <div><h4>📊 Báo cáo & Thống kê</h4>
          <ol className="breadcrumb"><li><Link to="/admin">Dashboard</Link></li><li className="active">Báo cáo</li></ol>
        </div>
        <div style={{ display: 'flex', gap: 8 }}>
          {[{ v: 'week', l: 'Tuần' }, { v: 'month', l: 'Tháng' }, { v: 'quarter', l: 'Quý' }, { v: 'year', l: 'Năm' }].map(p => (
            <button key={p.v} className={`btn btn-sm ${period === p.v ? 'btn-gold' : 'btn-outline-gold'}`} onClick={() => setPeriod(p.v)}>{p.l}</button>
          ))}
        </div>
      </div>

      {loading && <div className="card"><div className="card-body">Đang tải...</div></div>}

      {data && (
        <>
          <div className="stats-row">
            <div className="stat-box"><div className="sb-icon bg-gold">💰</div><div><div className="sb-num">{fmtN(data.totalRevenue || 0)}đ</div><div className="sb-label">Doanh thu</div></div></div>
            <div className="stat-box"><div className="sb-icon bg-info">📦</div><div><div className="sb-num">{data.totalOrders || 0}</div><div className="sb-label">Đơn hàng</div></div></div>
            <div className="stat-box"><div className="sb-icon bg-success">📅</div><div><div className="sb-num">{data.totalAppointments || 0}</div><div className="sb-label">Lịch hẹn</div></div></div>
            <div className="stat-box"><div className="sb-icon bg-primary">👥</div><div><div className="sb-num">{data.newCustomers || 0}</div><div className="sb-label">KH mới</div></div></div>
          </div>

          <div className="row" style={{ marginBottom: 20 }}>
            <div className="adm-col-8">
              <div className="card"><div className="card-header"><h6>📈 Biểu đồ doanh thu</h6></div><div className="card-body"><canvas ref={barRef} /></div></div>
            </div>
            <div className="adm-col-4">
              <div className="card"><div className="card-header"><h6>🍩 Trạng thái đơn hàng</h6></div><div className="card-body"><canvas ref={doughRef} /></div></div>
            </div>
          </div>

          <div className="row">
            <div className="adm-col-6">
              <div className="card">
                <div className="card-header"><h6>🏆 Top sản phẩm bán chạy</h6></div>
                <div className="card-body p-0">
                  <table className="adm-table">
                    <thead><tr><th>#</th><th>Sản phẩm</th><th>Đã bán</th><th>Doanh thu</th></tr></thead>
                    <tbody>
                      {(data.topProducts || []).map((p: any, i: number) => (
                        <tr key={i}><td>{i + 1}</td><td>{p.name || p.product?.name || '-'}</td><td>{p.sold || p.quantity || 0}</td><td>{fmtN(p.revenue || 0)}đ</td></tr>
                      ))}
                      {!(data.topProducts?.length) && <tr><td colSpan={4} className="empty">Chưa có dữ liệu</td></tr>}
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
            <div className="adm-col-6">
              <div className="card">
                <div className="card-header"><h6>🛎️ Top dịch vụ được đặt</h6></div>
                <div className="card-body p-0">
                  <table className="adm-table">
                    <thead><tr><th>#</th><th>Dịch vụ</th><th>Lượt đặt</th><th>Doanh thu</th></tr></thead>
                    <tbody>
                      {(data.topServices || []).map((s: any, i: number) => (
                        <tr key={i}><td>{i + 1}</td><td>{s.name || s.service?.name || '-'}</td><td>{s.count || s.bookings || 0}</td><td>{fmtN(s.revenue || 0)}đ</td></tr>
                      ))}
                      {!(data.topServices?.length) && <tr><td colSpan={4} className="empty">Chưa có dữ liệu</td></tr>}
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          </div>

          {/* Inventory Section */}
          {data.inventory && (
            <div style={{ marginTop: 20 }}>
              <div className="stats-row">
                <div className="stat-box"><div className="sb-icon bg-info">📦</div><div><div className="sb-num">{data.inventory.totalProducts || 0}</div><div className="sb-label">Tổng SP</div></div></div>
                <div className="stat-box"><div className="sb-icon bg-success">🏷️</div><div><div className="sb-num">{fmtN(data.inventory.totalStock || 0)}</div><div className="sb-label">Tổng tồn kho</div></div></div>
                <div className="stat-box"><div className="sb-icon bg-gold">💎</div><div><div className="sb-num">{fmtN(data.inventory.totalValue || 0)}đ</div><div className="sb-label">Giá trị kho</div></div></div>
                <div className="stat-box"><div className="sb-icon bg-danger">🚫</div><div><div className="sb-num">{data.inventory.outOfStock || 0}</div><div className="sb-label">Hết hàng</div></div></div>
                <div className="stat-box"><div className="sb-icon bg-warning">⚠️</div><div><div className="sb-num">{data.inventory.lowStock || 0}</div><div className="sb-label">Sắp hết</div></div></div>
              </div>
              <div className="card">
                <div className="card-header"><h6>⚠️ Sản phẩm tồn kho thấp (≤ 10)</h6></div>
                <div className="card-body p-0">
                  <table className="adm-table">
                    <thead><tr><th>#</th><th>Sản phẩm</th><th>SKU</th><th>Tồn kho</th><th>Giá</th><th>Đã bán</th><th>Trạng thái</th></tr></thead>
                    <tbody>
                      {(data.lowStockProducts || []).map((p: any, i: number) => (
                        <tr key={p._id || i}>
                          <td>{i + 1}</td>
                          <td><strong>{p.name}</strong></td>
                          <td className="sub-txt">{p.sku || '-'}</td>
                          <td><strong style={{ color: (p.stockQuantity || 0) <= 0 ? '#dc3545' : (p.stockQuantity || 0) <= 5 ? '#ffc107' : '#28a745' }}>{p.stockQuantity || 0}</strong></td>
                          <td>{fmtN(p.price || 0)}đ</td>
                          <td>{p.soldCount || 0}</td>
                          <td><span className={`os-badge ${(p.stockQuantity || 0) <= 0 ? 'os-cancelled' : 'os-pending'}`}>{(p.stockQuantity || 0) <= 0 ? 'Hết hàng' : 'Sắp hết'}</span></td>
                        </tr>
                      ))}
                      {!(data.lowStockProducts?.length) && <tr><td colSpan={7} className="empty">Tất cả sản phẩm còn đủ hàng</td></tr>}
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}