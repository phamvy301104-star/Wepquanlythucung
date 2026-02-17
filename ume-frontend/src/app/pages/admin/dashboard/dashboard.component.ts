import { Component, OnInit, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../services/api.service';

declare var Chart: any;

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h2><i class="fas fa-tachometer-alt mr-2"></i>Dashboard</h2>
      <a href="/" target="_blank" class="btn-outline-gold"><i class="fas fa-home mr-1"></i> Xem trang chủ</a>
    </div>
    <div class="row">
      <div class="col-sm-6 col-md-3"><div class="small-box bg-gold"><div class="inner"><h3>{{stats.revenueToday|number:'1.0-0'}}<sup>đ</sup></h3><p>Doanh thu hôm nay</p></div><div class="sb-icon"><i class="fas fa-money-bill-wave"></i></div><div class="sb-footer">Chi tiết <i class="fas fa-arrow-circle-right"></i></div></div></div>
      <div class="col-sm-6 col-md-3"><div class="small-box bg-info"><div class="inner"><h3>{{stats.revenueMonth|number:'1.0-0'}}<sup>đ</sup></h3><p>Doanh thu tháng</p></div><div class="sb-icon"><i class="fas fa-chart-line"></i></div><div class="sb-footer"><span *ngIf="stats.revenueGrowth>=0"><i class="fas fa-arrow-up"></i> {{stats.revenueGrowth}}%</span><span *ngIf="stats.revenueGrowth<0"><i class="fas fa-arrow-down"></i> {{-stats.revenueGrowth}}%</span> so tháng trước</div></div></div>
      <div class="col-sm-6 col-md-3"><div class="small-box bg-success"><div class="inner"><h3>{{stats.ordersToday}}</h3><p>Đơn hàng hôm nay</p></div><div class="sb-icon"><i class="fas fa-shopping-cart"></i></div><a routerLink="/admin/orders" class="sb-footer"><span class="sb-badge">{{stats.pendingOrders}}</span> chờ xử lý <i class="fas fa-arrow-circle-right"></i></a></div></div>
      <div class="col-sm-6 col-md-3"><div class="small-box bg-warning"><div class="inner"><h3>{{stats.appointmentsToday}}</h3><p>Lịch hẹn hôm nay</p></div><div class="sb-icon"><i class="fas fa-calendar-check"></i></div><a routerLink="/admin/appointments" class="sb-footer"><span class="sb-badge">{{stats.pendingAppointments}}</span> chờ xác nhận <i class="fas fa-arrow-circle-right"></i></a></div></div>
    </div>
    <div class="card mb-3"><div class="card-header"><h3 class="card-title"><i class="fas fa-bolt mr-2 text-warning"></i>Thao tác nhanh</h3></div><div class="card-body py-3"><div class="row">
      <div class="col-6 col-md-2 mb-2"><a routerLink="/admin/products" class="qa-btn"><i class="fas fa-plus-circle"></i><span>Thêm sản phẩm</span></a></div>
      <div class="col-6 col-md-2 mb-2"><a routerLink="/admin/orders" class="qa-btn"><i class="fas fa-clock"></i><span>Đơn chờ xử lý</span></a></div>
      <div class="col-6 col-md-2 mb-2"><a routerLink="/admin/appointments" class="qa-btn"><i class="fas fa-calendar-plus"></i><span>Tạo lịch hẹn</span></a></div>
      <div class="col-6 col-md-2 mb-2"><a routerLink="/admin/pets" class="qa-btn"><i class="fas fa-paw"></i><span>Thú cưng</span></a></div>
      <div class="col-6 col-md-2 mb-2"><a routerLink="/admin/staff" class="qa-btn"><i class="fas fa-user-tie"></i><span>Quản lý NV</span></a></div>
      <div class="col-6 col-md-2 mb-2"><a routerLink="/admin/reports" class="qa-btn"><i class="fas fa-chart-bar"></i><span>Báo cáo</span></a></div>
      <div class="col-6 col-md-2 mb-2"><a routerLink="/admin/users" class="qa-btn"><i class="fas fa-user-friends"></i><span>Khách hàng</span></a></div>
      <div class="col-6 col-md-2 mb-2"><a routerLink="/admin/services" class="qa-btn"><i class="fas fa-cut"></i><span>Dịch vụ</span></a></div>
      <div class="col-6 col-md-2 mb-2"><a routerLink="/admin/categories" class="qa-btn"><i class="fas fa-folder"></i><span>Danh mục</span></a></div>
      <div class="col-6 col-md-2 mb-2"><a routerLink="/admin/brands" class="qa-btn"><i class="fas fa-tags"></i><span>Thương hiệu</span></a></div>
      <div class="col-6 col-md-2 mb-2"><a routerLink="/admin/promotions" class="qa-btn"><i class="fas fa-percentage"></i><span>Khuyến mãi</span></a></div>
      <div class="col-6 col-md-2 mb-2"><a routerLink="/admin/reviews" class="qa-btn"><i class="fas fa-cog"></i><span>Cài đặt</span></a></div>
    </div></div></div>
    <div class="row">
      <div class="col-lg-8"><div class="card"><div class="card-header"><h3 class="card-title"><i class="fas fa-chart-area mr-2"></i>Doanh thu 7 ngày</h3></div><div class="card-body"><div class="chart-box"><canvas id="revenueChart"></canvas></div></div></div></div>
      <div class="col-lg-4"><div class="card"><div class="card-header"><h3 class="card-title"><i class="fas fa-chart-pie mr-2"></i>Trạng thái đơn</h3></div><div class="card-body"><div class="chart-box"><canvas id="orderStatusChart"></canvas></div></div></div></div>
    </div>
    <div class="row">
      <div class="col-md-3 col-sm-6"><div class="info-box"><span class="ib-icon bg-primary-c"><i class="fas fa-users"></i></span><div class="ib-content"><span class="ib-text">Tổng khách hàng</span><span class="ib-num">{{stats.totalCustomers}}</span><small class="text-success"><i class="fas fa-user-plus"></i> +{{stats.newCustomersMonth}} tháng này</small></div></div></div>
      <div class="col-md-3 col-sm-6"><div class="info-box"><span class="ib-icon bg-secondary-c"><i class="fas fa-box"></i></span><div class="ib-content"><span class="ib-text">Sản phẩm</span><span class="ib-num">{{stats.totalProducts}}</span><small class="text-danger" *ngIf="stats.lowStock>0"><i class="fas fa-exclamation-triangle"></i> {{stats.lowStock}} sắp hết</small></div></div></div>
      <div class="col-md-3 col-sm-6"><div class="info-box"><span class="ib-icon bg-success-c"><i class="fas fa-cut"></i></span><div class="ib-content"><span class="ib-text">Dịch vụ</span><span class="ib-num">{{stats.totalServices}}</span><small class="text-info"><i class="fas fa-calendar-check"></i> {{stats.appointmentsMonth}} lịch tháng này</small></div></div></div>
      <div class="col-md-3 col-sm-6"><div class="info-box"><span class="ib-icon bg-warning-c"><i class="fas fa-user-tie"></i></span><div class="ib-content"><span class="ib-text">Nhân viên</span><span class="ib-num">{{stats.activeStaff}}</span><small>đang hoạt động</small></div></div></div>
    </div>
    <div class="row">
      <div class="col-md-6"><div class="card"><div class="card-header d-flex justify-content-between align-items-center"><h3 class="card-title"><i class="fas fa-calendar-alt mr-2 text-warning"></i>Lịch hẹn hôm nay <span class="badge-gold">{{stats.appointmentsToday}}</span></h3><a routerLink="/admin/appointments" class="link-muted">Xem tất cả →</a></div><div class="card-body p-0 scroll-area"><table class="adm-table" *ngIf="todayAppointments.length"><thead><tr><th>Giờ</th><th>Khách hàng</th><th>Dịch vụ</th><th>TT</th></tr></thead><tbody><tr *ngFor="let a of todayAppointments"><td class="text-gold fw-bold">{{a.startTime||'--'}}</td><td>{{a.customerName||a.guestName||a.user?.fullName||'N/A'}}<br><small class="text-muted">{{a.customerPhone||a.guestPhone||''}}</small></td><td>{{getServiceNames(a)}}<br><small class="text-muted">{{a.staff?.fullName||''}}</small></td><td><span [class]="'st-badge st-'+getStCls(a.status)">{{getStTxt(a.status)}}</span></td></tr></tbody></table><div class="empty-s" *ngIf="!todayAppointments.length"><i class="fas fa-calendar-times"></i><p>Không có lịch hẹn</p></div></div></div></div>
      <div class="col-md-6"><div class="card"><div class="card-header d-flex justify-content-between align-items-center"><h3 class="card-title"><i class="fas fa-shopping-bag mr-2 text-success"></i>Đơn hàng gần đây</h3><a routerLink="/admin/orders" class="link-muted">Xem tất cả →</a></div><div class="card-body p-0 scroll-area"><table class="adm-table" *ngIf="recentOrders.length"><thead><tr><th>Mã đơn</th><th>Khách</th><th class="text-right">Tổng</th><th>TT</th></tr></thead><tbody><tr *ngFor="let o of recentOrders"><td><strong>{{o.orderCode||o._id?.substring(0,8)}}</strong><br><small class="text-muted">{{o.createdAt|date:'dd/MM HH:mm'}}</small></td><td>{{o.user?.fullName||o.customerName||'N/A'}}</td><td class="text-right fw-bold">{{o.totalAmount|number:'1.0-0'}}đ</td><td><span [class]="'st-badge st-'+getOrdCls(o.status)">{{getOrdTxt(o.status)}}</span></td></tr></tbody></table><div class="empty-s" *ngIf="!recentOrders.length"><i class="fas fa-inbox"></i><p>Chưa có đơn hàng</p></div></div></div></div>
    </div>
  `,
  styles: [`
    :host{display:block}
    .d-flex{display:flex}.justify-content-between{justify-content:space-between}.align-items-center{align-items:center}
    .mb-2{margin-bottom:.5rem}.mb-3{margin-bottom:1rem}.mr-1{margin-right:.25rem}.mr-2{margin-right:.5rem}.py-3{padding:.75rem 0}.p-0{padding:0!important}
    .text-right{text-align:right}.text-muted{color:#6c757d}.text-warning{color:#ffc107!important}.text-success{color:#28a745!important}.text-danger{color:#dc3545!important}.text-info{color:#17a2b8!important}.text-gold{color:#D4AF37!important}.fw-bold{font-weight:600}
    h2{font-weight:600;color:#1a1a1a;font-size:1.5rem}h2 i{color:#D4AF37}
    .btn-outline-gold{border:1px solid #D4AF37;color:#D4AF37;background:0 0;padding:6px 16px;border-radius:4px;text-decoration:none;font-size:.85rem;cursor:pointer}.btn-outline-gold:hover{background:#D4AF37;color:#fff}
    .row{display:flex;flex-wrap:wrap;margin:0 -8px}
    .col-6{flex:0 0 50%;max-width:50%;padding:0 8px}.col-sm-6{flex:0 0 50%;max-width:50%;padding:0 8px}.col-md-2{flex:0 0 16.666%;max-width:16.666%;padding:0 8px}.col-md-3{flex:0 0 25%;max-width:25%;padding:0 8px}.col-md-6{flex:0 0 50%;max-width:50%;padding:0 8px}.col-lg-4{flex:0 0 33.333%;max-width:33.333%;padding:0 8px}.col-lg-8{flex:0 0 66.666%;max-width:66.666%;padding:0 8px}
    @media(max-width:767px){.col-sm-6,.col-md-2,.col-md-3,.col-md-6,.col-lg-4,.col-lg-8{flex:0 0 100%;max-width:100%}}
    @media(min-width:768px) and (max-width:991px){.col-lg-4,.col-lg-8{flex:0 0 100%;max-width:100%}}
    .small-box{border-radius:12px;padding:20px;color:#fff;position:relative;overflow:hidden;margin-bottom:16px}
    .small-box .inner h3{font-size:1.8rem;font-weight:700;margin:0}.small-box .inner h3 sup{font-size:.9rem}.small-box .inner p{font-size:.9rem;margin:4px 0 0;opacity:.9}
    .sb-icon{position:absolute;top:10px;right:15px;font-size:3.5rem;opacity:.2}
    .sb-footer{display:block;margin-top:12px;padding-top:10px;border-top:1px solid rgba(255,255,255,.2);color:rgba(255,255,255,.9);font-size:.8rem;text-decoration:none}.sb-footer:hover{color:#fff}
    .sb-badge{background:rgba(255,255,255,.3);padding:2px 8px;border-radius:10px;font-size:.75rem}
    .bg-gold{background:linear-gradient(135deg,#D4AF37,#B8960C)}.bg-info{background:linear-gradient(135deg,#17a2b8,#138496)}.bg-success{background:linear-gradient(135deg,#28a745,#218838)}.bg-warning{background:linear-gradient(135deg,#ffc107,#e0a800);color:#1a1a1a}.bg-warning .sb-footer{color:rgba(0,0,0,.7)}
    .card{border:none;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,.08);margin-bottom:16px;background:#fff;transition:transform .2s,box-shadow .2s}.card:hover{transform:translateY(-2px);box-shadow:0 4px 20px rgba(0,0,0,.12)}
    .card-header{background:#fff;border-bottom:1px solid #f0f0f0;padding:12px 16px;border-radius:12px 12px 0 0!important}
    .card-title{font-weight:600;color:#1a1a1a;font-size:.95rem;margin:0}.card-body{padding:16px}
    .qa-btn{display:flex;flex-direction:column;align-items:center;justify-content:center;padding:14px 8px;border-radius:8px;background:#f8f9fa;color:#333;text-decoration:none;transition:all .2s;border:1px solid #e9ecef;min-height:75px}.qa-btn:hover{background:#D4AF37;color:#fff;border-color:#D4AF37;transform:translateY(-2px)}.qa-btn i{font-size:1.2rem;margin-bottom:4px}.qa-btn span{font-size:.72rem;font-weight:500}
    .chart-box{position:relative;height:300px}
    .info-box{display:flex;align-items:stretch;background:#fff;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,.08);margin-bottom:16px;overflow:hidden}
    .ib-icon{display:flex;align-items:center;justify-content:center;width:70px;font-size:1.5rem;flex-shrink:0;color:#fff}
    .bg-primary-c{background:#667eea}.bg-secondary-c{background:#6c757d}.bg-success-c{background:#28a745}.bg-warning-c{background:#ffc107}
    .ib-content{padding:12px 16px;flex:1}.ib-text{font-size:.8rem;color:#6c757d;display:block}.ib-num{font-size:1.4rem;font-weight:700;color:#1a1a1a;display:block}
    .adm-table{width:100%;border-collapse:collapse}.adm-table th,.adm-table td{padding:10px 16px;border-bottom:1px solid #f0f0f0;font-size:.85rem}
    .adm-table thead th{background:#f8f9fa;font-weight:600;color:#1a1a1a;border-bottom:2px solid #dee2e6}
    .adm-table tbody tr:nth-child(odd){background:rgba(0,0,0,.02)}.adm-table tbody tr:hover{background:rgba(212,175,55,.05)}
    .scroll-area{max-height:400px;overflow-y:auto}
    .badge-gold{background:#D4AF37;color:#fff;padding:3px 10px;border-radius:10px;font-size:.75rem}
    .link-muted{color:#6c757d;text-decoration:none;font-size:.85rem}.link-muted:hover{color:#D4AF37}
    .st-badge{padding:4px 10px;border-radius:4px;font-size:.73rem;font-weight:500;color:#fff;white-space:nowrap}
    .st-pending{background:#FF9800}.st-confirmed{background:#2196F3}.st-progress{background:#9C27B0}.st-completed{background:#4CAF50}.st-cancelled{background:#E53935}.st-shipping{background:#00BCD4}
    .empty-s{text-align:center;padding:40px 20px;color:#adb5bd}.empty-s i{font-size:2.5rem;margin-bottom:10px;display:block;opacity:.4}.empty-s p{margin:0}
  `]
})
export class DashboardComponent implements OnInit, AfterViewInit {
  stats: any = { revenueToday:0, revenueMonth:0, revenueGrowth:0, ordersToday:0, pendingOrders:0, appointmentsToday:0, pendingAppointments:0, appointmentsMonth:0, totalCustomers:0, newCustomersMonth:0, totalProducts:0, lowStock:0, totalServices:0, activeStaff:0 };
  todayAppointments: any[] = [];
  recentOrders: any[] = [];
  revenueChartData: any[] = [];
  orderStatusData: any = {};
  constructor(private api: ApiService) {}
  ngOnInit() { this.loadDashboard(); this.loadAppointments(); this.loadOrders(); }
  ngAfterViewInit() { this.loadChartJs(); }
  loadDashboard() {
    this.api.getDashboard().subscribe({ next: (r: any) => { if (r.success && r.data) { const d = r.data; Object.assign(this.stats, { revenueToday: d.revenueToday||d.todayRevenue||0, revenueMonth: d.revenueMonth||d.monthRevenue||0, revenueGrowth: d.revenueGrowth||0, ordersToday: d.ordersToday||d.todayOrders||0, pendingOrders: d.pendingOrders||0, appointmentsToday: d.appointmentsToday||d.todayAppointments||0, pendingAppointments: d.pendingAppointments||0, appointmentsMonth: d.appointmentsMonth||0, totalCustomers: d.totalCustomers||d.totalUsers||0, newCustomersMonth: d.newCustomersMonth||d.newUsersMonth||0, totalProducts: d.totalProducts||0, lowStock: d.lowStockProducts||d.lowStock||0, totalServices: d.totalServices||0, activeStaff: d.activeStaff||d.totalStaff||0 }); this.orderStatusData = d.orderStatusChart||d.orderStatus||{}; this.revenueChartData = d.revenueChart||d.revenueChartData||[]; setTimeout(() => this.initCharts(), 200); } }, error: () => {} });
  }
  loadAppointments() { this.api.getAppointments({ limit: 5, sort: '-appointmentDate' }).subscribe({ next: (r: any) => { if (r.success) this.todayAppointments = (r.data?.appointments||r.data||[]).slice(0, 5); }, error: () => {} }); }
  loadOrders() { this.api.getOrders({ limit: 5, sort: '-createdAt' }).subscribe({ next: (r: any) => { if (r.success) this.recentOrders = (r.data?.orders||r.data||[]).slice(0, 5); }, error: () => {} }); }
  loadChartJs() { if (typeof Chart !== 'undefined') return; const s = document.createElement('script'); s.src = 'https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js'; s.onload = () => this.initCharts(); document.head.appendChild(s); }
  initCharts() { if (typeof Chart === 'undefined') return; this.initRevenue(); this.initOrderStatus(); }
  initRevenue() { const c = document.getElementById('revenueChart') as HTMLCanvasElement; if (!c) return; const e = Chart.getChart(c); if (e) e.destroy(); const labels = this.revenueChartData.map((d: any) => d.label||d.date||''); const data = this.revenueChartData.map((d: any) => d.totalRevenue||d.revenue||d.total||0); new Chart(c, { type: 'line', data: { labels: labels.length ? labels : ['T2','T3','T4','T5','T6','T7','CN'], datasets: [{ label: 'Doanh thu', data: data.length ? data : [0,0,0,0,0,0,0], borderColor: '#D4AF37', backgroundColor: 'rgba(212,175,55,.1)', fill: true, tension: .4, borderWidth: 3 }] }, options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'top' }, tooltip: { callbacks: { label: (ctx: any) => ctx.dataset.label+': '+new Intl.NumberFormat('vi-VN').format(ctx.raw)+'đ' } } }, scales: { y: { beginAtZero: true, ticks: { callback: (v: any) => v >= 1e6 ? (v/1e6).toFixed(1)+'M' : v >= 1e3 ? (v/1e3).toFixed(0)+'K' : v } } } } }); }
  initOrderStatus() { const c = document.getElementById('orderStatusChart') as HTMLCanvasElement; if (!c) return; const e = Chart.getChart(c); if (e) e.destroy(); const d = this.orderStatusData; new Chart(c, { type: 'doughnut', data: { labels: ['Chờ xử lý','Đã xác nhận','Đang xử lý','Đang giao','Hoàn thành','Đã hủy'], datasets: [{ data: [d.pending||0, d.confirmed||0, d.processing||0, d.shipping||0, d.completed||0, d.cancelled||0], backgroundColor: ['#FF9800','#2196F3','#9C27B0','#00BCD4','#4CAF50','#E53935'], borderWidth: 0 }] }, options: { responsive: true, maintainAspectRatio: false, cutout: '60%', plugins: { legend: { position: 'bottom', labels: { padding: 15, usePointStyle: true, font: { size: 11 } } } } } }); }
  getServiceNames(a: any): string { if (a.serviceName) return a.serviceName; if (a.services?.length) return a.services.map((s: any) => s.name||s.service?.name).join(', '); return 'N/A'; }
  getStCls(s: string) { const m: any = { Pending:'pending', Confirmed:'confirmed', InProgress:'progress', Completed:'completed', Cancelled:'cancelled' }; return m[s]||'pending'; }
  getStTxt(s: string) { const m: any = { Pending:'Chờ', Confirmed:'Xác nhận', InProgress:'Đang làm', Completed:'Xong', Cancelled:'Hủy' }; return m[s]||s; }
  getOrdCls(s: string) { const m: any = { Pending:'pending', Confirmed:'confirmed', Processing:'progress', Shipping:'shipping', Completed:'completed', Cancelled:'cancelled' }; return m[s]||'pending'; }
  getOrdTxt(s: string) { const m: any = { Pending:'Chờ', Confirmed:'Xác nhận', Processing:'Xử lý', Shipping:'Giao', Completed:'Xong', Cancelled:'Hủy' }; return m[s]||s; }
}
