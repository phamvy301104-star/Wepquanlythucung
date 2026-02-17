import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { ApiService } from '../../../services/api.service';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <!-- Admin Layout matching AdminLTE -->
    <div class="admin-wrapper" [class.sidebar-collapsed]="sidebarCollapsed">
      <!-- Sidebar -->
      <aside class="admin-sidebar">
        <div class="sidebar-brand">
          <img src="https://ui-avatars.com/api/?name=UME&background=D4AF37&color=fff&size=40&rounded=true" alt="Logo" class="brand-logo">
          <span class="brand-text" *ngIf="!sidebarCollapsed">UME Admin</span>
        </div>

        <div class="sidebar-user">
          <img [src]="'https://ui-avatars.com/api/?name=' + (auth.currentUser?.fullName || 'Admin') + '&background=D4AF37&color=fff&size=40&rounded=true'" class="user-avatar" alt="Avatar">
          <div class="user-info" *ngIf="!sidebarCollapsed">
            <span class="user-name">{{ auth.currentUser?.fullName || 'Admin' }}</span>
            <span class="user-role">{{ auth.isAdmin ? 'Quản trị viên' : 'Nhân viên' }}</span>
          </div>
        </div>

        <nav class="sidebar-nav">
          <!-- Dashboard -->
          <a routerLink="/admin" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}" class="nav-item" *ngIf="auth.isAdmin">
            <i class="fas fa-tachometer-alt"></i>
            <span *ngIf="!sidebarCollapsed">Dashboard</span>
          </a>

          <div class="nav-header" *ngIf="!sidebarCollapsed">QUẢN LÝ BÁN HÀNG</div>

          <a routerLink="/admin/products" routerLinkActive="active" class="nav-item">
            <i class="fas fa-box"></i>
            <span *ngIf="!sidebarCollapsed">Sản phẩm</span>
          </a>
          <a routerLink="/admin/orders" routerLinkActive="active" class="nav-item" *ngIf="auth.isAdmin">
            <i class="fas fa-shopping-cart"></i>
            <span *ngIf="!sidebarCollapsed">Đơn hàng</span>
            <span class="nav-badge bg-info" *ngIf="pendingOrders > 0 && !sidebarCollapsed">{{ pendingOrders }}</span>
          </a>
          <a routerLink="/admin/categories" routerLinkActive="active" class="nav-item">
            <i class="fas fa-folder"></i>
            <span *ngIf="!sidebarCollapsed">Danh mục</span>
          </a>
          <a routerLink="/admin/brands" routerLinkActive="active" class="nav-item">
            <i class="fas fa-tags"></i>
            <span *ngIf="!sidebarCollapsed">Thương hiệu</span>
          </a>

          <div class="nav-header" *ngIf="!sidebarCollapsed">QUẢN LÝ DỊCH VỤ</div>

          <a routerLink="/admin/appointments" routerLinkActive="active" class="nav-item">
            <i class="fas fa-calendar-alt"></i>
            <span *ngIf="!sidebarCollapsed">Lịch hẹn</span>
            <span class="nav-badge bg-warning" *ngIf="pendingAppointments > 0 && !sidebarCollapsed">{{ pendingAppointments }}</span>
          </a>
          <a routerLink="/admin/services" routerLinkActive="active" class="nav-item">
            <i class="fas fa-cut"></i>
            <span *ngIf="!sidebarCollapsed">Dịch vụ</span>
          </a>
          <a routerLink="/admin/staff" routerLinkActive="active" class="nav-item" *ngIf="auth.isAdmin">
            <i class="fas fa-users"></i>
            <span *ngIf="!sidebarCollapsed">Nhân viên</span>
          </a>

          <div class="nav-header" *ngIf="!sidebarCollapsed">THÚ CƯNG</div>

          <a routerLink="/admin/pets" routerLinkActive="active" class="nav-item">
            <i class="fas fa-paw"></i>
            <span *ngIf="!sidebarCollapsed">Quản lý thú cưng</span>
          </a>

          <div class="nav-header" *ngIf="!sidebarCollapsed && auth.isAdmin">TÀI KHOẢN</div>

          <a routerLink="/admin/users" routerLinkActive="active" class="nav-item" *ngIf="auth.isAdmin">
            <i class="fas fa-user-cog"></i>
            <span *ngIf="!sidebarCollapsed">Quản lý tài khoản</span>
          </a>
          <a routerLink="/admin/reviews" routerLinkActive="active" class="nav-item" *ngIf="auth.isAdmin">
            <i class="fas fa-star"></i>
            <span *ngIf="!sidebarCollapsed">Đánh giá</span>
          </a>

          <div class="nav-header" *ngIf="!sidebarCollapsed && auth.isAdmin">MARKETING</div>

          <a routerLink="/admin/promotions" routerLinkActive="active" class="nav-item" *ngIf="auth.isAdmin">
            <i class="fas fa-percentage"></i>
            <span *ngIf="!sidebarCollapsed">Khuyến mãi</span>
          </a>

          <div class="nav-header" *ngIf="!sidebarCollapsed && auth.isAdmin">BÁO CÁO</div>

          <a routerLink="/admin/reports" routerLinkActive="active" class="nav-item" *ngIf="auth.isAdmin">
            <i class="fas fa-chart-bar"></i>
            <span *ngIf="!sidebarCollapsed">Báo cáo</span>
          </a>

          <div class="nav-header" *ngIf="!sidebarCollapsed && auth.isAdmin">CÀI ĐẶT</div>

          <a routerLink="/admin/settings" routerLinkActive="active" class="nav-item" *ngIf="auth.isAdmin">
            <i class="fas fa-cog"></i>
            <span *ngIf="!sidebarCollapsed">Cài đặt liên hệ</span>
          </a>
        </nav>
      </aside>

      <!-- Main Content -->
      <div class="admin-main">
        <!-- Top Header -->
        <header class="admin-header">
          <div class="header-left">
            <button class="btn-toggle-sidebar" (click)="toggleSidebar()">
              <i class="fas fa-bars"></i>
            </button>
            <a routerLink="/admin" class="header-link d-none d-sm-inline">Trang chủ</a>
          </div>
          <div class="header-right">
            <!-- Notifications -->
            <div class="header-dropdown" [class.open]="notifDropdownOpen">
              <button class="header-icon-btn" (click)="toggleNotifDropdown()">
                <i class="far fa-bell"></i>
                <span class="notification-badge" *ngIf="unreadNotifications > 0">{{ unreadNotifications }}</span>
              </button>
              <div class="dropdown-panel notifications-panel" *ngIf="notifDropdownOpen">
                <div class="dropdown-panel-header">
                  <span>Thông báo</span>
                  <button class="btn-clear" (click)="clearNotifications()"><i class="fas fa-trash-alt"></i></button>
                </div>
                <div class="dropdown-panel-body">
                  <div class="empty-state" *ngIf="notifications.length === 0">
                    <i class="far fa-bell-slash"></i>
                    <p>Chưa có thông báo nào</p>
                  </div>
                  <a *ngFor="let notif of notifications" class="notification-item" [class.unread]="!notif.isRead">
                    <div class="notif-icon" [ngClass]="getNotifIconClass(notif.type)">
                      {{ getNotifIcon(notif.type) }}
                    </div>
                    <div class="notif-content">
                      <p class="notif-message">{{ notif.message }}</p>
                      <small class="notif-time">{{ notif.createdAt | date:'dd/MM HH:mm' }}</small>
                    </div>
                  </a>
                </div>
              </div>
            </div>

            <!-- Fullscreen -->
            <button class="header-icon-btn d-none d-md-inline-flex" (click)="toggleFullscreen()">
              <i class="fas fa-expand-arrows-alt"></i>
            </button>

            <!-- User Menu -->
            <div class="header-dropdown" [class.open]="userDropdownOpen">
              <button class="user-menu-btn" (click)="toggleUserDropdown()">
                <img [src]="'https://ui-avatars.com/api/?name=' + (auth.currentUser?.fullName || 'Admin') + '&background=D4AF37&color=fff&size=32&rounded=true'" alt="User">
                <span class="d-none d-md-inline">{{ auth.currentUser?.fullName || 'Admin' }}</span>
              </button>
              <div class="dropdown-panel user-panel" *ngIf="userDropdownOpen">
                <div class="user-panel-header">
                  <img [src]="'https://ui-avatars.com/api/?name=' + (auth.currentUser?.fullName || 'Admin') + '&background=D4AF37&color=fff&size=80&rounded=true'" alt="User">
                  <p class="user-panel-name">{{ auth.currentUser?.fullName || 'Admin' }}</p>
                  <small>{{ auth.isAdmin ? 'Quản trị viên' : 'Nhân viên' }}</small>
                </div>
                <div class="user-panel-footer">
                  <a routerLink="/profile" class="btn btn-default btn-sm" (click)="userDropdownOpen = false">Hồ sơ</a>
                  <button class="btn btn-default btn-sm" (click)="logout()">Đăng xuất</button>
                </div>
              </div>
            </div>
          </div>
        </header>

        <!-- Page Content -->
        <div class="admin-content">
          <router-outlet></router-outlet>
        </div>

        <!-- Footer -->
        <footer class="admin-footer">
          <span>© 2025 <a href="/">UME Pet Salon</a>. All rights reserved.</span>
          <span class="float-right">Version 2.0</span>
        </footer>
      </div>
    </div>

    <!-- Overlay for mobile -->
    <div class="sidebar-overlay" *ngIf="!sidebarCollapsed && isMobile" (click)="toggleSidebar()"></div>
    <!-- Dropdown overlay -->
    <div class="dropdown-overlay" *ngIf="notifDropdownOpen || userDropdownOpen" (click)="closeDropdowns()"></div>
  `,
  styles: [`
    :host { display: block; }

    .admin-wrapper {
      display: flex;
      min-height: 100vh;
      background: #f4f6f9;
    }

    /* ===== SIDEBAR ===== */
    .admin-sidebar {
      width: 250px;
      height: 100vh;
      background: #343a40;
      color: #c2c7d0;
      position: fixed;
      top: 0;
      left: 0;
      z-index: 1040;
      transition: width 0.3s ease;
      overflow-x: hidden;
      overflow-y: auto;
    }
    .sidebar-collapsed .admin-sidebar {
      width: 60px;
    }

    .sidebar-brand {
      background: #1a1a1a;
      padding: 12px 16px;
      display: flex;
      align-items: center;
      gap: 10px;
      border-bottom: 1px solid #4b545c;
      height: 57px;
    }
    .brand-logo { width: 33px; height: 33px; border-radius: 50%; }
    .brand-text {
      color: #D4AF37;
      font-weight: 700;
      font-size: 1.1rem;
      white-space: nowrap;
    }

    .sidebar-user {
      padding: 15px 16px;
      display: flex;
      align-items: center;
      gap: 10px;
      border-bottom: 1px solid rgba(255,255,255,0.1);
    }
    .user-avatar { width: 34px; height: 34px; border-radius: 50%; }
    .user-info { display: flex; flex-direction: column; }
    .user-name { color: #fff; font-size: 0.875rem; white-space: nowrap; }
    .user-role { font-size: 0.75rem; color: #adb5bd; }

    .sidebar-nav { padding: 8px 0 80px; }
    .nav-header {
      padding: 16px 16px 8px;
      font-size: 0.7rem;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      color: #adb5bd;
      white-space: nowrap;
    }

    .nav-item {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 10px 16px;
      color: #c2c7d0;
      text-decoration: none;
      transition: all 0.2s;
      border-left: 3px solid transparent;
      white-space: nowrap;
      position: relative;
    }
    .nav-item:hover {
      background: rgba(212,175,55,0.15);
      color: #D4AF37;
    }
    .nav-item.active {
      background: #D4AF37;
      color: #fff;
      border-left-color: #B8960C;
    }
    .nav-item i { width: 20px; text-align: center; font-size: 0.9rem; flex-shrink: 0; }

    .nav-badge {
      margin-left: auto;
      padding: 2px 8px;
      border-radius: 10px;
      font-size: 0.7rem;
      color: #fff;
    }
    .bg-info { background: #17a2b8; }
    .bg-warning { background: #ffc107; color: #1a1a1a !important; }

    /* ===== MAIN ===== */
    .admin-main {
      flex: 1;
      margin-left: 250px;
      display: flex;
      flex-direction: column;
      min-height: 100vh;
      transition: margin-left 0.3s ease;
    }
    .sidebar-collapsed .admin-main { margin-left: 60px; }

    /* ===== HEADER ===== */
    .admin-header {
      background: #fff;
      border-bottom: 1px solid #dee2e6;
      padding: 0 16px;
      height: 57px;
      display: flex;
      align-items: center;
      justify-content: space-between;
      position: sticky;
      top: 0;
      z-index: 1030;
    }
    .header-left { display: flex; align-items: center; gap: 12px; }
    .header-right { display: flex; align-items: center; gap: 8px; }

    .btn-toggle-sidebar {
      background: none;
      border: none;
      padding: 8px 12px;
      font-size: 1.1rem;
      color: #6c757d;
      cursor: pointer;
    }
    .btn-toggle-sidebar:hover { color: #D4AF37; }

    .header-link {
      color: #6c757d;
      text-decoration: none;
      font-size: 0.9rem;
    }
    .header-link:hover { color: #D4AF37; }

    .header-icon-btn {
      background: none;
      border: none;
      padding: 8px 12px;
      color: #6c757d;
      cursor: pointer;
      position: relative;
      font-size: 1rem;
      display: inline-flex;
      align-items: center;
    }
    .header-icon-btn:hover { color: #D4AF37; }

    .notification-badge {
      position: absolute;
      top: 2px;
      right: 2px;
      background: #dc3545;
      color: #fff;
      font-size: 0.65rem;
      padding: 1px 5px;
      border-radius: 10px;
      min-width: 16px;
      text-align: center;
    }

    .user-menu-btn {
      display: flex;
      align-items: center;
      gap: 8px;
      background: none;
      border: none;
      padding: 4px 8px;
      cursor: pointer;
      color: #6c757d;
      font-size: 0.9rem;
    }
    .user-menu-btn img { width: 32px; height: 32px; border-radius: 50%; }
    .user-menu-btn:hover { color: #D4AF37; }

    /* Dropdowns */
    .header-dropdown { position: relative; }
    .dropdown-panel {
      position: absolute;
      top: 100%;
      right: 0;
      background: #fff;
      border: 1px solid #dee2e6;
      border-radius: 8px;
      box-shadow: 0 4px 20px rgba(0,0,0,0.15);
      z-index: 1050;
      min-width: 300px;
    }
    .dropdown-panel-header {
      padding: 12px 16px;
      font-weight: 600;
      border-bottom: 1px solid #dee2e6;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .btn-clear {
      background: none;
      border: none;
      color: #adb5bd;
      cursor: pointer;
      font-size: 0.85rem;
    }
    .btn-clear:hover { color: #dc3545; }
    .dropdown-panel-body { max-height: 350px; overflow-y: auto; }

    .empty-state {
      text-align: center;
      padding: 30px;
      color: #adb5bd;
    }
    .empty-state i { font-size: 2.5rem; display: block; margin-bottom: 10px; opacity: 0.3; }

    .notification-item {
      display: flex;
      align-items: flex-start;
      gap: 10px;
      padding: 10px 16px;
      border-bottom: 1px solid #f0f0f0;
      text-decoration: none;
      color: inherit;
      cursor: pointer;
    }
    .notification-item:hover { background: #f8f9fa; }
    .notification-item.unread { background: #f0f8ff; }

    .notif-icon {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 16px;
      flex-shrink: 0;
    }
    .notif-icon.bg-success { background: #d4edda; }
    .notif-icon.bg-primary { background: #cce5ff; }
    .notif-icon.bg-warn { background: #fff3cd; }
    .notif-icon.bg-notif-info { background: #d1ecf1; }

    .notif-message { margin: 0; font-size: 0.85rem; line-height: 1.3; }
    .notif-time { color: #adb5bd; }

    .user-panel { min-width: 260px; }
    .user-panel-header {
      background: #D4AF37;
      padding: 20px;
      text-align: center;
      color: #fff;
      border-radius: 8px 8px 0 0;
    }
    .user-panel-header img { width: 80px; height: 80px; border-radius: 50%; margin-bottom: 8px; }
    .user-panel-name { font-weight: 600; margin: 4px 0 0; }
    .user-panel-footer {
      padding: 12px;
      display: flex;
      justify-content: space-between;
    }
    .btn-default {
      background: #f8f9fa;
      border: 1px solid #dee2e6;
      border-radius: 4px;
      padding: 6px 16px;
      cursor: pointer;
      text-decoration: none;
      color: #333;
      font-size: 0.85rem;
    }
    .btn-default:hover { background: #e9ecef; }

    /* ===== CONTENT ===== */
    .admin-content {
      flex: 1;
      padding: 20px;
    }

    /* ===== FOOTER ===== */
    .admin-footer {
      background: #fff;
      border-top: 1px solid #dee2e6;
      padding: 12px 20px;
      font-size: 0.85rem;
      color: #6c757d;
      display: flex;
      justify-content: space-between;
    }
    .admin-footer a { color: #D4AF37; text-decoration: none; }

    /* ===== OVERLAY ===== */
    .sidebar-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0,0,0,0.5);
      z-index: 1035;
    }
    .dropdown-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      z-index: 1045;
    }

    /* ===== RESPONSIVE ===== */
    @media (max-width: 991px) {
      .admin-sidebar {
        transform: translateX(-100%);
        width: 250px;
      }
      .admin-wrapper:not(.sidebar-collapsed) .admin-sidebar {
        transform: translateX(0);
      }
      .admin-main { margin-left: 0 !important; }
      .sidebar-collapsed .admin-sidebar { width: 250px; transform: translateX(-100%); }
    }

    .float-right { margin-left: auto; }
    .d-none { display: none !important; }
    @media (min-width: 576px) { .d-sm-inline { display: inline !important; } }
    @media (min-width: 768px) { .d-md-inline { display: inline !important; } .d-md-inline-flex { display: inline-flex !important; } }
  `]
})
export class AdminLayoutComponent implements OnInit {
  sidebarCollapsed = false;
  isMobile = false;
  notifDropdownOpen = false;
  userDropdownOpen = false;
  unreadNotifications = 0;
  pendingOrders = 0;
  pendingAppointments = 0;
  notifications: any[] = [];

  constructor(
    public auth: AuthService,
    private api: ApiService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.isMobile = window.innerWidth < 992;
    if (this.isMobile) this.sidebarCollapsed = true;
    window.addEventListener('resize', () => {
      this.isMobile = window.innerWidth < 992;
    });
    this.loadDashboardCounts();
  }

  loadDashboardCounts(): void {
    this.api.getDashboard().subscribe({
      next: (res: any) => {
        if (res.success) {
          this.pendingOrders = res.data?.pendingOrders || 0;
          this.pendingAppointments = res.data?.pendingAppointments || 0;
        }
      },
      error: () => {}
    });
    this.api.getNotifications().subscribe({
      next: (res: any) => {
        if (res.success) {
          this.notifications = (res.data?.notifications || res.data || []).slice(0, 10);
          this.unreadNotifications = this.notifications.filter((n: any) => !n.isRead).length;
        }
      },
      error: () => {}
    });
  }

  toggleSidebar(): void {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }

  toggleNotifDropdown(): void {
    this.notifDropdownOpen = !this.notifDropdownOpen;
    this.userDropdownOpen = false;
  }

  toggleUserDropdown(): void {
    this.userDropdownOpen = !this.userDropdownOpen;
    this.notifDropdownOpen = false;
  }

  closeDropdowns(): void {
    this.notifDropdownOpen = false;
    this.userDropdownOpen = false;
  }

  clearNotifications(): void {
    this.notifications = [];
    this.unreadNotifications = 0;
  }

  toggleFullscreen(): void {
    if (!document.fullscreenElement) {
      document.documentElement.requestFullscreen();
    } else {
      document.exitFullscreen();
    }
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  getNotifIcon(type: string): string {
    switch (type) {
      case 'NewAppointment': return '📅';
      case 'NewOrder': return '📦';
      case 'LowStock': return '⚠️';
      case 'NewReview': return '⭐';
      default: return '🔔';
    }
  }

  getNotifIconClass(type: string): string {
    switch (type) {
      case 'NewAppointment': return 'bg-success';
      case 'NewOrder': return 'bg-primary';
      case 'LowStock': return 'bg-warn';
      default: return 'bg-notif-info';
    }
  }
}
