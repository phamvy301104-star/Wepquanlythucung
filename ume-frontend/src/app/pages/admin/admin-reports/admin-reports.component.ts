import { Component, OnInit, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../services/api.service';

declare var Chart: any;

@Component({
  selector: 'app-admin-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <h2><i class="fas fa-chart-bar"></i> Báo cáo & Thống kê</h2>
    </div>

    <!-- Summary Cards -->
    <div class="row mb-3">
      <div class="col-md-3 col-sm-6">
        <div class="stat-card bg-gold clickable" (click)="openRevenueDetail()">
          <div class="stat-icon"><i class="fas fa-money-bill-wave"></i></div>
          <div class="stat-info">
            <span class="stat-label">Tổng doanh thu <i class="fas fa-external-link-alt" style="font-size:.6rem;margin-left:4px;opacity:.7"></i></span>
            <span class="stat-value">{{ stats.totalRevenue | number:'1.0-0' }}đ</span>
          </div>
        </div>
      </div>
      <div class="col-md-3 col-sm-6">
        <div class="stat-card bg-info-c">
          <div class="stat-icon"><i class="fas fa-shopping-cart"></i></div>
          <div class="stat-info">
            <span class="stat-label">Tổng đơn hàng</span>
            <span class="stat-value">{{ stats.totalOrders }}</span>
          </div>
        </div>
      </div>
      <div class="col-md-3 col-sm-6">
        <div class="stat-card bg-success-c">
          <div class="stat-icon"><i class="fas fa-calendar-check"></i></div>
          <div class="stat-info">
            <span class="stat-label">Tổng lịch hẹn</span>
            <span class="stat-value">{{ stats.totalAppointments }}</span>
          </div>
        </div>
      </div>
      <div class="col-md-3 col-sm-6">
        <div class="stat-card bg-purple">
          <div class="stat-icon"><i class="fas fa-users"></i></div>
          <div class="stat-info">
            <span class="stat-label">Tổng khách hàng</span>
            <span class="stat-value">{{ stats.totalCustomers }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Revenue Detail Modal -->
    <div class="modal-overlay" *ngIf="showRevenueModal" (click)="showRevenueModal = false">
      <div class="modal-content modal-lg" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <h3><i class="fas fa-money-bill-wave text-gold mr-2"></i> Chi tiết doanh thu</h3>
          <button class="btn-close-modal" (click)="showRevenueModal = false"><i class="fas fa-times"></i></button>
        </div>

        <!-- Revenue Summary -->
        <div class="revenue-summary">
          <div class="rev-box">
            <i class="fas fa-shopping-cart"></i>
            <div>
              <span class="rev-label">Doanh thu đơn hàng</span>
              <span class="rev-value text-gold">{{ revenueOrdersTotal | number:'1.0-0' }}đ</span>
            </div>
          </div>
          <div class="rev-box">
            <i class="fas fa-calendar-check"></i>
            <div>
              <span class="rev-label">Doanh thu dịch vụ</span>
              <span class="rev-value text-gold">{{ revenueAppointmentsTotal | number:'1.0-0' }}đ</span>
            </div>
          </div>
          <div class="rev-box total">
            <i class="fas fa-coins"></i>
            <div>
              <span class="rev-label">Tổng cộng</span>
              <span class="rev-value text-gold">{{ revenueOrdersTotal + revenueAppointmentsTotal | number:'1.0-0' }}đ</span>
            </div>
          </div>
        </div>

        <!-- Tabs -->
        <div class="rev-tabs">
          <button [class.active]="revenueTab === 'orders'" (click)="revenueTab = 'orders'">
            <i class="fas fa-shopping-cart mr-2"></i> Đơn hàng hoàn thành ({{ completedOrders.length }})
          </button>
          <button [class.active]="revenueTab === 'appointments'" (click)="revenueTab = 'appointments'">
            <i class="fas fa-calendar-check mr-2"></i> Dịch vụ hoàn thành ({{ completedAppointments.length }})
          </button>
        </div>

        <div class="modal-body">
          <!-- Orders tab -->
          <div *ngIf="revenueTab === 'orders'">
            <div class="loading-spinner" *ngIf="loadingRevenue"><i class="fas fa-spinner fa-spin"></i> Đang tải...</div>
            <table class="adm-table" *ngIf="!loadingRevenue && completedOrders.length">
              <thead>
                <tr>
                  <th>#</th>
                  <th>Mã đơn</th>
                  <th>Khách hàng</th>
                  <th>Sản phẩm</th>
                  <th>Ngày hoàn thành</th>
                  <th>Tổng tiền</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let o of completedOrders; let i = index">
                  <td>{{ i + 1 }}</td>
                  <td><span class="code-badge">{{ o.orderCode }}</span></td>
                  <td>{{ o.customer?.fullName || 'N/A' }}</td>
                  <td>
                    <div class="items-list">
                      <span *ngFor="let item of o.items?.slice(0, 2)">{{ item.productName }} x{{ item.quantity }}</span>
                      <span class="more" *ngIf="o.items?.length > 2">+{{ o.items.length - 2 }} sản phẩm khác</span>
                    </div>
                  </td>
                  <td>{{ o.completedAt || o.updatedAt | date:'dd/MM/yyyy HH:mm' }}</td>
                  <td class="text-gold fw-bold">{{ o.totalAmount | number:'1.0-0' }}đ</td>
                </tr>
              </tbody>
            </table>
            <div class="empty-s" *ngIf="!loadingRevenue && !completedOrders.length">
              <i class="fas fa-box-open"></i><p>Chưa có đơn hàng hoàn thành</p>
            </div>
          </div>

          <!-- Appointments tab -->
          <div *ngIf="revenueTab === 'appointments'">
            <div class="loading-spinner" *ngIf="loadingRevenue"><i class="fas fa-spinner fa-spin"></i> Đang tải...</div>
            <table class="adm-table" *ngIf="!loadingRevenue && completedAppointments.length">
              <thead>
                <tr>
                  <th>#</th>
                  <th>Mã lịch hẹn</th>
                  <th>Khách hàng</th>
                  <th>Dịch vụ</th>
                  <th>Ngày hoàn thành</th>
                  <th>Tổng tiền</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let a of completedAppointments; let i = index">
                  <td>{{ i + 1 }}</td>
                  <td><span class="code-badge">{{ a.appointmentCode }}</span></td>
                  <td>{{ a.customer?.fullName || 'N/A' }}</td>
                  <td>
                    <div class="items-list">
                      <span *ngFor="let s of a.services?.slice(0, 2)">{{ s.serviceName }}</span>
                      <span class="more" *ngIf="a.services?.length > 2">+{{ a.services.length - 2 }} dịch vụ khác</span>
                    </div>
                  </td>
                  <td>{{ a.completedAt || a.updatedAt | date:'dd/MM/yyyy HH:mm' }}</td>
                  <td class="text-gold fw-bold">{{ (a.finalAmount || a.totalAmount) | number:'1.0-0' }}đ</td>
                </tr>
              </tbody>
            </table>
            <div class="empty-s" *ngIf="!loadingRevenue && !completedAppointments.length">
              <i class="fas fa-calendar-times"></i><p>Chưa có dịch vụ hoàn thành</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Filters -->
    <div class="card mb-3">
      <div class="card-body">
        <div class="filter-row">
          <select [(ngModel)]="period" (change)="loadRevenueChart()" class="form-control select-w">
            <option value="daily">Theo ngày</option>
            <option value="monthly">Theo tháng</option>
            <option value="yearly">Theo năm</option>
          </select>
          <select [(ngModel)]="year" (change)="loadRevenueChart()" class="form-control select-w">
            <option *ngFor="let y of years" [value]="y">{{ y }}</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Charts -->
    <div class="row">
      <div class="col-lg-8">
        <div class="card">
          <div class="card-header"><h3 class="card-title"><i class="fas fa-chart-line mr-2 text-gold"></i>Biểu đồ doanh thu</h3></div>
          <div class="card-body"><div class="chart-box"><canvas id="reportRevenueChart"></canvas></div></div>
        </div>
      </div>
      <div class="col-lg-4">
        <div class="card">
          <div class="card-header"><h3 class="card-title"><i class="fas fa-chart-pie mr-2 text-gold"></i>Trạng thái đơn hàng</h3></div>
          <div class="card-body"><div class="chart-box"><canvas id="reportOrderChart"></canvas></div></div>
        </div>
      </div>
    </div>

    <!-- Top Products -->
    <div class="row">
      <div class="col-md-6">
        <div class="card">
          <div class="card-header"><h3 class="card-title"><i class="fas fa-trophy mr-2 text-warning"></i>Sản phẩm bán chạy</h3></div>
          <div class="card-body p-0">
            <table class="adm-table" *ngIf="topProducts.length">
              <thead><tr><th>#</th><th>Sản phẩm</th><th>Đã bán</th><th>Doanh thu</th></tr></thead>
              <tbody>
                <tr *ngFor="let p of topProducts; let i = index">
                  <td><span class="rank-badge">{{ i + 1 }}</span></td>
                  <td>{{ p.name }}</td>
                  <td>{{ p.soldCount }}</td>
                  <td class="text-gold fw-bold">{{ p.revenue | number:'1.0-0' }}đ</td>
                </tr>
              </tbody>
            </table>
            <div class="empty-s" *ngIf="!topProducts.length"><i class="fas fa-box-open"></i><p>Chưa có dữ liệu</p></div>
          </div>
        </div>
      </div>
      <div class="col-md-6">
        <div class="card">
          <div class="card-header"><h3 class="card-title"><i class="fas fa-star mr-2 text-info"></i>Dịch vụ phổ biến</h3></div>
          <div class="card-body p-0">
            <table class="adm-table" *ngIf="topServices.length">
              <thead><tr><th>#</th><th>Dịch vụ</th><th>Lượt đặt</th><th>Đánh giá</th></tr></thead>
              <tbody>
                <tr *ngFor="let s of topServices; let i = index">
                  <td><span class="rank-badge">{{ i + 1 }}</span></td>
                  <td>{{ s.name }}</td>
                  <td>{{ s.bookingCount || 0 }}</td>
                  <td>
                    <span class="text-warning">★</span> {{ s.averageRating?.toFixed(1) || 'N/A' }}
                  </td>
                </tr>
              </tbody>
            </table>
            <div class="empty-s" *ngIf="!topServices.length"><i class="fas fa-cut"></i><p>Chưa có dữ liệu</p></div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
    .page-header h2 { font-weight: 600; font-size: 1.4rem; color: #1a1a1a; margin: 0; }
    .page-header h2 i { color: #D4AF37; margin-right: 8px; }
    .row { display: flex; flex-wrap: wrap; margin: 0 -8px; }
    .col-sm-6 { flex: 0 0 50%; max-width: 50%; padding: 0 8px; }
    .col-md-3 { flex: 0 0 25%; max-width: 25%; padding: 0 8px; }
    .col-md-6 { flex: 0 0 50%; max-width: 50%; padding: 0 8px; }
    .col-lg-4 { flex: 0 0 33.333%; max-width: 33.333%; padding: 0 8px; }
    .col-lg-8 { flex: 0 0 66.666%; max-width: 66.666%; padding: 0 8px; }
    @media (max-width: 767px) { .col-sm-6,.col-md-3,.col-md-6,.col-lg-4,.col-lg-8 { flex: 0 0 100%; max-width: 100%; } }
    .mb-3 { margin-bottom: 16px; }
    .mr-2 { margin-right: 8px; }
    .p-0 { padding: 0 !important; }
    .card { border: none; border-radius: 12px; box-shadow: 0 2px 12px rgba(0,0,0,.08); background: #fff; margin-bottom: 16px; }
    .card-header { background: #fff; border-bottom: 1px solid #f0f0f0; padding: 12px 16px; border-radius: 12px 12px 0 0 !important; }
    .card-title { font-weight: 600; color: #1a1a1a; font-size: .95rem; margin: 0; }
    .card-body { padding: 16px; }
    .stat-card { display: flex; align-items: center; gap: 16px; padding: 20px; border-radius: 12px; color: #fff; margin-bottom: 16px; }
    .stat-icon { font-size: 2rem; opacity: .8; }
    .stat-info { display: flex; flex-direction: column; }
    .stat-label { font-size: .8rem; opacity: .9; }
    .stat-value { font-size: 1.5rem; font-weight: 700; }
    .bg-gold { background: linear-gradient(135deg, #D4AF37, #B8960C); }
    .bg-info-c { background: linear-gradient(135deg, #17a2b8, #138496); }
    .bg-success-c { background: linear-gradient(135deg, #28a745, #218838); }
    .bg-purple { background: linear-gradient(135deg, #6f42c1, #5a32a3); }
    .filter-row { display: flex; gap: 10px; flex-wrap: wrap; }
    .form-control { padding: 8px 12px; border: 1px solid #dee2e6; border-radius: 6px; font-size: .85rem; }
    .form-control:focus { outline: none; border-color: #D4AF37; }
    .select-w { width: 160px; }
    .chart-box { position: relative; height: 300px; }
    .adm-table { width: 100%; border-collapse: collapse; }
    .adm-table th, .adm-table td { padding: 10px 14px; border-bottom: 1px solid #f0f0f0; font-size: .85rem; }
    .adm-table thead th { background: #f8f9fa; font-weight: 600; }
    .adm-table tbody tr:hover { background: rgba(212,175,55,.05); }
    .rank-badge { display: inline-flex; align-items: center; justify-content: center; width: 24px; height: 24px; border-radius: 50%; background: #D4AF37; color: #fff; font-size: .75rem; font-weight: 700; }
    .text-gold { color: #D4AF37; }
    .text-warning { color: #ffc107; }
    .text-info { color: #17a2b8; }
    .fw-bold { font-weight: 600; }
    .empty-s { text-align: center; padding: 40px 20px; color: #adb5bd; }
    .empty-s i { font-size: 2.5rem; margin-bottom: 10px; display: block; opacity: .4; }
    .empty-s p { margin: 0; }
    .clickable { cursor: pointer; transition: transform .15s, box-shadow .15s; }
    .clickable:hover { transform: translateY(-2px); box-shadow: 0 6px 20px rgba(212,175,55,.3); }
    .modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,.5); z-index: 1000; display: flex; align-items: center; justify-content: center; animation: fadeIn .2s; }
    .modal-content { background: #fff; border-radius: 12px; width: 95%; max-width: 960px; max-height: 85vh; display: flex; flex-direction: column; box-shadow: 0 20px 60px rgba(0,0,0,.2); animation: slideUp .25s; }
    .modal-header { display: flex; justify-content: space-between; align-items: center; padding: 16px 20px; border-bottom: 1px solid #f0f0f0; }
    .modal-header h3 { font-size: 1.1rem; font-weight: 600; margin: 0; color: #1a1a1a; }
    .btn-close-modal { background: none; border: none; font-size: 1.2rem; cursor: pointer; color: #666; padding: 4px 8px; border-radius: 6px; }
    .btn-close-modal:hover { background: #f0f0f0; color: #333; }
    .modal-body { padding: 16px 20px; overflow-y: auto; flex: 1; }
    .revenue-summary { display: flex; gap: 12px; padding: 16px 20px; background: #fafafa; border-bottom: 1px solid #f0f0f0; flex-wrap: wrap; }
    .rev-box { display: flex; align-items: center; gap: 10px; padding: 12px 16px; background: #fff; border-radius: 10px; border: 1px solid #eee; flex: 1; min-width: 200px; }
    .rev-box i { font-size: 1.3rem; color: #888; }
    .rev-box.total { background: linear-gradient(135deg, #fffbf0, #fff8e1); border-color: #D4AF37; }
    .rev-box.total i { color: #D4AF37; }
    .rev-label { display: block; font-size: .75rem; color: #888; }
    .rev-value { display: block; font-size: 1.1rem; font-weight: 700; }
    .rev-tabs { display: flex; gap: 0; border-bottom: 2px solid #f0f0f0; padding: 0 20px; }
    .rev-tabs button { padding: 10px 18px; border: none; background: none; font-size: .85rem; font-weight: 500; color: #888; cursor: pointer; border-bottom: 2px solid transparent; margin-bottom: -2px; transition: all .15s; }
    .rev-tabs button.active { color: #D4AF37; border-bottom-color: #D4AF37; }
    .rev-tabs button:hover { color: #333; }
    .code-badge { display: inline-block; padding: 2px 8px; background: #f0f0f0; border-radius: 4px; font-size: .8rem; font-weight: 600; color: #555; font-family: monospace; }
    .items-list { display: flex; flex-direction: column; gap: 2px; }
    .items-list span { font-size: .82rem; }
    .items-list .more { color: #888; font-style: italic; }
    .loading-spinner { text-align: center; padding: 30px; color: #adb5bd; }
    @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
    @keyframes slideUp { from { opacity: 0; transform: translateY(20px); } to { opacity: 1; transform: translateY(0); } }
  `]
})
export class AdminReportsComponent implements OnInit, AfterViewInit {
  stats: any = { totalRevenue: 0, totalOrders: 0, totalAppointments: 0, totalCustomers: 0 };
  topProducts: any[] = [];
  topServices: any[] = [];
  revenueData: any[] = [];
  orderStatusData: any = {};
  period = 'monthly';
  year = new Date().getFullYear();
  years: number[] = [];

  // Revenue detail modal
  showRevenueModal = false;
  revenueTab = 'orders';
  completedOrders: any[] = [];
  completedAppointments: any[] = [];
  revenueOrdersTotal = 0;
  revenueAppointmentsTotal = 0;
  loadingRevenue = false;

  constructor(private api: ApiService) {
    for (let y = new Date().getFullYear(); y >= 2024; y--) this.years.push(y);
  }

  ngOnInit(): void {
    this.loadDashboard();
    this.loadRevenueChart();
    this.loadTopProducts();
    this.loadTopServices();
  }

  ngAfterViewInit(): void {
    this.loadChartJs();
  }

  openRevenueDetail(): void {
    this.showRevenueModal = true;
    this.revenueTab = 'orders';
    this.loadingRevenue = true;

    // Load completed orders
    this.api.getOrders({ status: 'Completed', limit: 200, sort: '-updatedAt' }).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.completedOrders = res.data?.orders || res.data || [];
          this.revenueOrdersTotal = this.completedOrders.reduce((sum: number, o: any) => sum + (o.totalAmount || 0), 0);
        }
        this.checkRevenueLoaded();
      },
      error: () => { this.checkRevenueLoaded(); }
    });

    // Load completed appointments
    this.api.getAppointments({ status: 'Completed', limit: 200, sort: '-updatedAt' }).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.completedAppointments = res.data?.appointments || res.data || [];
          this.revenueAppointmentsTotal = this.completedAppointments.reduce(
            (sum: number, a: any) => sum + (a.finalAmount || a.totalAmount || 0), 0
          );
        }
        this.checkRevenueLoaded();
      },
      error: () => { this.checkRevenueLoaded(); }
    });
  }

  private revenueLoadCount = 0;
  private checkRevenueLoaded(): void {
    this.revenueLoadCount++;
    if (this.revenueLoadCount >= 2) {
      this.loadingRevenue = false;
      this.revenueLoadCount = 0;
    }
  }

  loadDashboard(): void {
    this.api.getDashboard().subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          const d = res.data;
          this.stats = {
            totalRevenue: d.revenueMonth || 0,
            totalOrders: d.totalOrders || 0,
            totalAppointments: d.totalAppointments || 0,
            totalCustomers: d.totalCustomers || 0
          };
          this.orderStatusData = d.orderStatusChart || {};
          setTimeout(() => this.initOrderChart(), 300);
        }
      },
      error: () => {}
    });
  }

  loadRevenueChart(): void {
    this.api.getRevenueChart({ period: this.period, year: this.year }).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.revenueData = res.data || [];
          setTimeout(() => this.initRevenueChart(), 300);
        }
      },
      error: () => {}
    });
  }

  loadTopProducts(): void {
    this.api.getProducts({ limit: 10, sort: '-soldCount' }).subscribe({
      next: (res: any) => {
        if (res.success) {
          const products = res.data?.products || res.data || [];
          this.topProducts = products.slice(0, 5).map((p: any) => ({
            name: p.name,
            soldCount: p.soldCount || 0,
            revenue: (p.soldCount || 0) * (p.price || 0)
          }));
        }
      },
      error: () => {}
    });
  }

  loadTopServices(): void {
    this.api.getServices({ limit: 10, sort: '-averageRating' }).subscribe({
      next: (res: any) => {
        if (res.success) {
          const services = res.data?.services || res.data || [];
          this.topServices = services.slice(0, 5).map((s: any) => ({
            name: s.name,
            bookingCount: s.totalReviews || 0,
            averageRating: s.averageRating || 0
          }));
        }
      },
      error: () => {}
    });
  }

  loadChartJs(): void {
    if (typeof Chart !== 'undefined') return;
    const s = document.createElement('script');
    s.src = 'https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js';
    s.onload = () => { this.initRevenueChart(); this.initOrderChart(); };
    document.head.appendChild(s);
  }

  initRevenueChart(): void {
    if (typeof Chart === 'undefined') return;
    const c = document.getElementById('reportRevenueChart') as HTMLCanvasElement;
    if (!c) return;
    const e = Chart.getChart(c);
    if (e) e.destroy();

    const labels = this.revenueData.map((d: any) => d._id || d.label || '');
    const data = this.revenueData.map((d: any) => d.revenue || d.totalRevenue || d.total || 0);

    new Chart(c, {
      type: 'bar',
      data: {
        labels: labels.length ? labels : ['Chưa có dữ liệu'],
        datasets: [{
          label: 'Doanh thu',
          data: data.length ? data : [0],
          backgroundColor: 'rgba(212,175,55,0.7)',
          borderColor: '#D4AF37',
          borderWidth: 1,
          borderRadius: 6
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false }, tooltip: { callbacks: { label: (ctx: any) => new Intl.NumberFormat('vi-VN').format(ctx.raw) + 'đ' } } },
        scales: { y: { beginAtZero: true, ticks: { callback: (v: any) => v >= 1e6 ? (v / 1e6).toFixed(1) + 'M' : v >= 1e3 ? (v / 1e3).toFixed(0) + 'K' : v } } }
      }
    });
  }

  initOrderChart(): void {
    if (typeof Chart === 'undefined') return;
    const c = document.getElementById('reportOrderChart') as HTMLCanvasElement;
    if (!c) return;
    const e = Chart.getChart(c);
    if (e) e.destroy();

    const d = this.orderStatusData;
    new Chart(c, {
      type: 'doughnut',
      data: {
        labels: ['Chờ xử lý', 'Đã xác nhận', 'Đang xử lý', 'Đang giao', 'Hoàn thành', 'Đã hủy'],
        datasets: [{
          data: [d.pending || 0, d.confirmed || 0, d.processing || 0, d.shipping || 0, d.completed || 0, d.cancelled || 0],
          backgroundColor: ['#FF9800', '#2196F3', '#9C27B0', '#00BCD4', '#4CAF50', '#E53935'],
          borderWidth: 0
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '60%',
        plugins: { legend: { position: 'bottom', labels: { padding: 12, usePointStyle: true, font: { size: 11 } } } }
      }
    });
  }
}
