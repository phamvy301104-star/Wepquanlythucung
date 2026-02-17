import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../services/api.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page-header">
      <div><h4><i class="fas fa-users text-gold"></i> Quản lý tài khoản</h4>
        <ol class="breadcrumb"><li><a routerLink="/admin">Dashboard</a></li><li class="active">Tài khoản</li></ol>
      </div>
      <button class="btn btn-gold" (click)="openCreate()"><i class="fas fa-plus mr-1"></i> Thêm nhân viên</button>
    </div>

    <div class="alert alert-success" *ngIf="msg"><i class="fas fa-check-circle mr-2"></i>{{msg}}<button class="alert-x" (click)="msg=''">&times;</button></div>
    <div class="alert alert-danger" *ngIf="errMsg"><i class="fas fa-exclamation-circle mr-2"></i>{{errMsg}}<button class="alert-x" (click)="errMsg=''">&times;</button></div>

    <div class="stats-row">
      <div class="stat-box"><div class="sb-icon bg-info"><i class="fas fa-users"></i></div><div><div class="sb-num">{{totals.total}}</div><div class="sb-label">Tổng tài khoản</div></div></div>
      <div class="stat-box"><div class="sb-icon bg-success"><i class="fas fa-user-check"></i></div><div><div class="sb-num">{{totals.active}}</div><div class="sb-label">Hoạt động</div></div></div>
      <div class="stat-box"><div class="sb-icon bg-primary"><i class="fas fa-user-tie"></i></div><div><div class="sb-num">{{totals.staff}}</div><div class="sb-label">Nhân viên</div></div></div>
      <div class="stat-box"><div class="sb-icon bg-warning"><i class="fas fa-user-shield"></i></div><div><div class="sb-num">{{totals.admin}}</div><div class="sb-label">Admin</div></div></div>
    </div>

    <div class="card">
      <div class="card-header">
        <div class="filter-row">
          <div class="search-box"><input class="form-control" [(ngModel)]="search" placeholder="Tên, email, SĐT..." (keyup.enter)="load()"><button class="btn btn-gold btn-sm" (click)="load()"><i class="fas fa-search"></i></button></div>
          <select class="form-control fc-sm" [(ngModel)]="roleFilter" (change)="load()"><option value="">Tất cả vai trò</option><option value="Admin">Admin</option><option value="Staff">Nhân viên</option></select>
          <select class="form-control fc-sm" [(ngModel)]="statusFilter" (change)="load()"><option value="">Tất cả TT</option><option value="active">Hoạt động</option><option value="inactive">Bị khóa</option></select>
        </div>
      </div>
      <div class="card-body p-0">
        <table class="adm-table">
          <thead><tr><th>#</th><th>Người dùng</th><th>Email</th><th>Số điện thoại</th><th>Vai trò</th><th>Ngày tạo</th><th>Trạng thái</th><th>Thao tác</th></tr></thead>
          <tbody>
            <tr *ngFor="let u of items; let i=index" [class.row-admin]="u.role==='Admin'">
              <td>{{i+1}}</td>
              <td>
                <div class="user-cell">
                  <img [src]="getImg(u.avatarUrl||u.avatar) || 'https://ui-avatars.com/api/?name='+(u.fullName||u.name||'U')+'&background=D4AF37&color=fff'" class="avatar">
                  <div>
                    <strong>{{u.fullName||u.name||'Chưa đặt tên'}}</strong>
                    <span class="admin-badge" *ngIf="u.role==='Admin'"><i class="fas fa-shield-alt"></i> Bảo vệ</span>
                  </div>
                </div>
              </td>
              <td><span class="email-txt">{{u.email}}</span></td>
              <td>{{u.phoneNumber||'-'}}</td>
              <td><span class="role-b" [ngClass]="'role-'+u.role">{{getRoleLabel(u.role)}}</span></td>
              <td>{{u.createdAt|date:'dd/MM/yyyy'}}</td>
              <td>
                <label class="sw" *ngIf="u.role!=='Admin'"><input type="checkbox" [checked]="u.isActive!==false" (change)="togActive(u)"><span class="sl"></span></label>
                <span class="status-active" *ngIf="u.role==='Admin'"><i class="fas fa-check-circle"></i> Hoạt động</span>
              </td>
              <td>
                <div class="act-g">
                  <button class="ab ai" (click)="viewDetail(u)" title="Xem chi tiết"><i class="fas fa-eye"></i></button>
                  <button class="ab aw" (click)="openEdit(u)" *ngIf="u.role!=='Admin'" title="Chỉnh sửa"><i class="fas fa-edit"></i></button>
                  <button class="ab ad" (click)="confirmDelete(u)" *ngIf="u.role!=='Admin'" title="Xóa tài khoản"><i class="fas fa-trash"></i></button>
                  <span class="protected-label" *ngIf="u.role==='Admin'"><i class="fas fa-lock"></i></span>
                </div>
              </td>
            </tr>
            <tr *ngIf="!items.length&&!ld"><td colspan="8" class="empty">Không có tài khoản nào</td></tr>
            <tr *ngIf="ld"><td colspan="8" class="empty"><i class="fas fa-spinner fa-spin fa-2x text-gold"></i></td></tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Detail Modal -->
    <div class="mo" *ngIf="showDetail" (click)="showDetail=false"><div class="md md-lg" (click)="$event.stopPropagation()">
      <div class="mh bg-g"><h5><i class="fas fa-user mr-2"></i>Chi tiết tài khoản</h5><button class="mx" (click)="showDetail=false">&times;</button></div>
      <div class="mb-modal">
        <div class="prof-header">
          <img [src]="getImg(detUser?.avatarUrl||detUser?.avatar) || 'https://ui-avatars.com/api/?name='+(detUser?.fullName||'U')+'&background=D4AF37&color=fff&size=80'" class="prof-avatar">
          <div>
            <h5>{{detUser?.fullName||detUser?.name||'Chưa đặt tên'}}</h5>
            <span class="role-b" [ngClass]="'role-'+detUser?.role">{{getRoleLabel(detUser?.role)}}</span>
            <span class="admin-badge ml-2" *ngIf="detUser?.role==='Admin'"><i class="fas fa-shield-alt"></i> Tài khoản được bảo vệ</span>
          </div>
        </div>
        <div class="detail-grid mt-3">
          <div class="dg-item"><label>Email</label><span>{{detUser?.email}}</span></div>
          <div class="dg-item"><label>Số điện thoại</label><span>{{detUser?.phoneNumber||'-'}}</span></div>
          <div class="dg-item"><label>Ngày tạo</label><span>{{detUser?.createdAt|date:'dd/MM/yyyy HH:mm'}}</span></div>
          <div class="dg-item"><label>Trạng thái</label><span [class.text-success]="detUser?.isActive!==false" [class.text-danger]="detUser?.isActive===false">{{detUser?.isActive!==false?'Hoạt động':'Bị khóa'}}</span></div>
          <div class="dg-item full"><label>Địa chỉ</label><span>{{detUser?.address||'-'}}</span></div>
          <div class="dg-item full" *ngIf="detUser?.role==='Staff'">
            <label>Quyền truy cập</label>
            <span class="perm-list">Sản phẩm, Danh mục, Thương hiệu, Lịch hẹn, Dịch vụ, Thú cưng</span>
          </div>
          <div class="dg-item full" *ngIf="detUser?.role==='Admin'">
            <label>Quyền truy cập</label>
            <span class="perm-list">Toàn quyền quản trị hệ thống</span>
          </div>
        </div>
      </div>
      <div class="mf"><button class="btn btn-sec" (click)="showDetail=false">Đóng</button></div>
    </div></div>

    <!-- Create/Edit Modal -->
    <div class="mo" *ngIf="showForm" (click)="showForm=false"><div class="md" (click)="$event.stopPropagation()">
      <div class="mh bg-g"><h5><i class="fas" [ngClass]="isEditing?'fa-edit':'fa-user-plus'" class="mr-2"></i>{{isEditing?'Chỉnh sửa tài khoản':'Thêm tài khoản nhân viên'}}</h5><button class="mx" (click)="showForm=false">&times;</button></div>
      <div class="mb-modal">
        <div class="fg"><label>Họ tên <span class="req">*</span></label><input class="form-control" [(ngModel)]="fd.fullName" placeholder="Nhập họ tên"></div>
        <div class="fg"><label>Email <span class="req">*</span></label><input class="form-control" [(ngModel)]="fd.email" [disabled]="isEditing" placeholder="Nhập email" type="email"></div>
        <div class="fg" *ngIf="!isEditing"><label>Mật khẩu <span class="req">*</span></label><input class="form-control" [(ngModel)]="fd.password" placeholder="Nhập mật khẩu" type="password"></div>
        <div class="fg" *ngIf="isEditing"><label>Mật khẩu mới (để trống nếu không đổi)</label><input class="form-control" [(ngModel)]="fd.password" placeholder="Nhập mật khẩu mới" type="password"></div>
        <div class="fg"><label>Số điện thoại</label><input class="form-control" [(ngModel)]="fd.phoneNumber" placeholder="Nhập số điện thoại"></div>
        <div class="fg" *ngIf="isEditing"><label>Vai trò</label>
          <select class="form-control" [(ngModel)]="fd.role" disabled>
            <option value="Staff">Nhân viên</option>
          </select>
        </div>
        <div class="fg"><label>Địa chỉ</label><textarea class="form-control" [(ngModel)]="fd.address" rows="2" placeholder="Nhập địa chỉ"></textarea></div>
        <div class="fg" *ngIf="isEditing"><label class="ck-label"><input type="checkbox" [(ngModel)]="fd.isActive"> Hoạt động</label></div>

        <!-- Permission Info -->
        <div class="perm-info" *ngIf="fd.role==='Staff'">
          <h6><i class="fas fa-key"></i> Quyền truy cập của Nhân viên</h6>
          <ul>
            <li><i class="fas fa-check text-success"></i> Sản phẩm - Xem, thêm, sửa, xóa</li>
            <li><i class="fas fa-check text-success"></i> Danh mục - Xem, thêm, sửa, xóa</li>
            <li><i class="fas fa-check text-success"></i> Thương hiệu - Xem, thêm, sửa, xóa</li>
            <li><i class="fas fa-check text-success"></i> Lịch hẹn - Xem, cập nhật trạng thái</li>
            <li><i class="fas fa-check text-success"></i> Dịch vụ - Xem, thêm, sửa, xóa</li>
            <li><i class="fas fa-check text-success"></i> Thú cưng - Xem, thêm, sửa, xóa</li>
            <li><i class="fas fa-times text-danger"></i> Đơn hàng, Tài khoản, Cài đặt, Khuyến mãi, Báo cáo</li>
          </ul>
        </div>

      </div>
      <div class="mf"><button class="btn btn-sec" (click)="showForm=false">Hủy</button><button class="btn btn-gold" (click)="saveForm()" [disabled]="saving"><i class="fas fa-save mr-1"></i>{{saving?'Đang lưu...':'Lưu'}}</button></div>
    </div></div>

    <!-- Delete Confirm Modal -->
    <div class="mo" *ngIf="showDeleteConfirm" (click)="showDeleteConfirm=false"><div class="md md-sm" (click)="$event.stopPropagation()">
      <div class="mh bg-danger"><h5><i class="fas fa-exclamation-triangle mr-2"></i>Xác nhận</h5><button class="mx" (click)="showDeleteConfirm=false">&times;</button></div>
      <div class="mb-modal text-center">
        <i class="fas fa-user-times fa-3x text-danger mb-3" style="display:block"></i>
        <p>Bạn có chắc muốn <strong class="text-danger">xóa vĩnh viễn</strong> tài khoản <strong>{{deleteTarget?.fullName||deleteTarget?.name}}</strong>?</p>
        <p class="text-muted small">Tài khoản sẽ bị xóa hoàn toàn và không thể khôi phục.</p>
      </div>
      <div class="mf"><button class="btn btn-sec" (click)="showDeleteConfirm=false">Hủy</button><button class="btn btn-danger" (click)="doDelete()" [disabled]="saving"><i class="fas fa-trash mr-1"></i>Xóa</button></div>
    </div></div>
  `,
  styles: [`
    :host{display:block}
    .page-header{display:flex;justify-content:space-between;align-items:flex-start;margin-bottom:16px;flex-wrap:wrap;gap:8px}
    .page-header h4{font-weight:600;color:#1a1a1a;font-size:1.3rem;margin:0}.page-header h4 i{margin-right:8px}
    .text-gold{color:#D4AF37!important}.text-success{color:#28a745!important}.text-danger{color:#dc3545!important}.text-muted{color:#6c757d!important}
    .breadcrumb{list-style:none;display:flex;gap:8px;padding:0;margin:4px 0 0;font-size:.85rem}.breadcrumb a{color:#D4AF37;text-decoration:none}.breadcrumb .active{color:#6c757d}.breadcrumb li+li::before{content:"/";margin-right:8px;color:#adb5bd}
    .btn{padding:8px 16px;border:none;border-radius:6px;cursor:pointer;font-size:.85rem;display:inline-flex;align-items:center;gap:4px;transition:all .2s}
    .btn-gold{background:#D4AF37;color:#fff}.btn-gold:hover{background:#B8960C}.btn-sec{background:#6c757d;color:#fff}.btn-danger{background:#dc3545;color:#fff}.btn-sm{padding:6px 12px}.btn:disabled{opacity:.6}
    .alert{padding:12px 16px;border-radius:8px;margin-bottom:16px;display:flex;align-items:center;position:relative}.alert-success{background:#d4edda;color:#155724;border:1px solid #c3e6cb}.alert-danger{background:#f8d7da;color:#721c24;border:1px solid #f5c6cb}.alert-x{position:absolute;right:12px;background:none;border:none;font-size:1.2rem;cursor:pointer;color:inherit}
    .stats-row{display:grid;grid-template-columns:repeat(4,1fr);gap:16px;margin-bottom:16px}
    .stat-box{display:flex;align-items:center;gap:12px;background:#fff;padding:16px;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,.08)}
    .sb-icon{width:48px;height:48px;border-radius:10px;display:flex;align-items:center;justify-content:center;font-size:1.2rem;color:#fff}
    .bg-info{background:#17a2b8}.bg-success{background:#28a745}.bg-warning{background:#ffc107;color:#1a1a1a!important}.bg-gold{background:#D4AF37}.bg-primary{background:#007bff}
    .sb-num{font-size:1.4rem;font-weight:700;color:#1a1a1a}.sb-label{font-size:.8rem;color:#6c757d}
    @media(max-width:991px){.stats-row{grid-template-columns:repeat(2,1fr)}}
    .card{border:none;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,.08);margin-bottom:16px;background:#fff}
    .card-header{background:#fff;border-bottom:1px solid #f0f0f0;padding:12px 16px;border-radius:12px 12px 0 0!important}.card-body{padding:16px}
    .filter-row{display:flex;gap:12px;align-items:center;flex-wrap:wrap}.search-box{display:flex;gap:8px}.search-box .form-control{width:250px}.fc-sm{width:160px}
    label{display:block;font-size:.85rem;font-weight:500;margin-bottom:4px;color:#333}
    .form-control{width:100%;padding:8px 12px;border:1px solid #dee2e6;border-radius:6px;font-size:.9rem;box-sizing:border-box}.form-control:focus{border-color:#D4AF37;outline:none;box-shadow:0 0 0 3px rgba(212,175,55,.15)}
    .form-control:disabled{background:#e9ecef;cursor:not-allowed}
    select.form-control{appearance:auto}textarea.form-control{resize:vertical}.fg{margin-bottom:12px}
    .req{color:#dc3545}
    .p-0{padding:0!important}.mr-1{margin-right:4px}.mr-2{margin-right:8px}.ml-2{margin-left:8px}.mt-3{margin-top:16px}.mb-3{margin-bottom:16px}.small{font-size:.82rem}
    .text-center{text-align:center}
    .adm-table{width:100%;border-collapse:collapse}.adm-table th,.adm-table td{padding:10px 14px;border-bottom:1px solid #f0f0f0;font-size:.83rem;vertical-align:middle}
    .adm-table thead th{background:#343a40;color:#fff;font-weight:600;white-space:nowrap}.adm-table tbody tr:nth-child(odd){background:rgba(0,0,0,.02)}.adm-table tbody tr:hover{background:rgba(212,175,55,.05)}
    .row-admin{background:rgba(248,215,218,.15)!important}
    .user-cell{display:flex;align-items:center;gap:10px}.avatar{width:40px;height:40px;border-radius:50%;object-fit:cover;border:2px solid #dee2e6}
    .email-txt{font-size:.82rem;color:#6c757d}
    .admin-badge{display:inline-flex;align-items:center;gap:4px;font-size:.7rem;color:#856404;background:#fff3cd;padding:2px 8px;border-radius:10px;margin-left:6px}
    .role-b{padding:3px 10px;border-radius:20px;font-size:.73rem;font-weight:600}.role-Admin{background:#f8d7da;color:#721c24}.role-Customer,.role-User{background:#d4edda;color:#155724}.role-Staff{background:#cce5ff;color:#004085}
    .status-active{color:#28a745;font-size:.82rem;font-weight:500}
    .protected-label{color:#adb5bd;font-size:.9rem}
    .sw{position:relative;display:inline-block;width:40px;height:22px;margin:0}.sw input{opacity:0;width:0;height:0}
    .sl{position:absolute;cursor:pointer;top:0;left:0;right:0;bottom:0;background:#ccc;transition:.3s;border-radius:22px}
    .sl:before{position:absolute;content:"";height:16px;width:16px;left:3px;bottom:3px;background:#fff;transition:.3s;border-radius:50%}
    input:checked+.sl{background:#D4AF37}input:checked+.sl:before{transform:translateX(18px)}
    .act-g{display:flex;gap:4px}.ab{padding:6px 10px;border:none;border-radius:4px;cursor:pointer;color:#fff;font-size:.8rem}.ai{background:#17a2b8}.aw{background:#ffc107;color:#1a1a1a}.ad{background:#dc3545}
    .empty{text-align:center;padding:30px 0;color:#adb5bd}
    .mo{position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,.5);z-index:1060;display:flex;align-items:center;justify-content:center;padding:20px}
    .md{background:#fff;border-radius:12px;max-width:550px;width:100%;max-height:90vh;overflow-y:auto;box-shadow:0 10px 40px rgba(0,0,0,.2)}.md-lg{max-width:700px}.md-sm{max-width:420px}
    .mh{padding:16px 20px;border-radius:12px 12px 0 0;display:flex;justify-content:space-between;align-items:center;color:#fff}.mh h5{margin:0;font-size:1.05rem}.mx{background:none;border:none;color:#fff;font-size:1.5rem;cursor:pointer}
    .mb-modal{padding:20px}.mf{padding:12px 20px;border-top:1px solid #f0f0f0;display:flex;justify-content:flex-end;gap:8px}
    .bg-g{background:#D4AF37}.bg-danger{background:#dc3545}
    .prof-header{display:flex;align-items:center;gap:16px}.prof-avatar{width:80px;height:80px;border-radius:50%;border:3px solid #D4AF37;object-fit:cover}.prof-header h5{margin:0 0 4px;font-weight:600}
    .detail-grid{display:grid;grid-template-columns:1fr 1fr;gap:12px}.dg-item{display:flex;flex-direction:column}.dg-item label{font-size:.75rem;color:#6c757d;margin-bottom:2px}.dg-item span{font-size:.9rem}.dg-item.full{grid-column:1/-1}
    .perm-list{color:#004085;font-size:.85rem}
    .ck-label{display:flex;align-items:center;gap:8px;cursor:pointer;font-size:.9rem}.ck-label input{width:16px;height:16px;accent-color:#D4AF37}
    .perm-info{background:#f8f9fa;border:1px solid #e9ecef;border-radius:8px;padding:14px;margin-top:8px}
    .perm-info h6{margin:0 0 10px;font-size:.88rem;color:#333;display:flex;align-items:center;gap:6px}
    .perm-info ul{list-style:none;padding:0;margin:0}.perm-info li{padding:4px 0;font-size:.82rem;display:flex;align-items:center;gap:6px}
    .perm-info li i{width:14px;text-align:center}
  `]
})
export class AdminUsersComponent implements OnInit {
  items: any[] = [];
  ld = false; saving = false; msg = ''; errMsg = ''; search = ''; roleFilter = ''; statusFilter = '';
  totals = { total: 0, active: 0, admin: 0, staff: 0 };

  showDetail = false; detUser: any = null;
  showForm = false; isEditing = false; editUser: any = null;
  fd: any = {};

  showDeleteConfirm = false; deleteTarget: any = null;

  constructor(private api: ApiService, public auth: AuthService) {}
  ngOnInit() { this.load(); }

  getImg(path: string): string { return this.api.getImageUrl(path); }

  getRoleLabel(role: string): string {
    switch (role) {
      case 'Admin': return 'Admin';
      case 'Staff': return 'Nhân viên';
      case 'Customer': case 'User': return 'Khách hàng';
      default: return role || '';
    }
  }

  load() {
    this.ld = true;
    const p: any = { limit: 200 };
    if (this.search) p.search = this.search;
    if (this.roleFilter) p.role = this.roleFilter;
    if (this.statusFilter === 'active') p.isActive = 'true';
    if (this.statusFilter === 'inactive') p.isActive = 'false';
    this.api.getUsers(p).subscribe({
      next: (r: any) => {
        this.ld = false;
        this.items = r.data?.users || r.data || [];
        this.calcTotals();
      },
      error: () => this.ld = false
    });
  }

  calcTotals() {
    this.totals.total = this.items.length;
    this.totals.active = this.items.filter(u => u.isActive !== false).length;
    this.totals.admin = this.items.filter(u => u.role === 'Admin').length;
    this.totals.staff = this.items.filter(u => u.role === 'Staff').length;
  }

  viewDetail(u: any) { this.detUser = u; this.showDetail = true; }

  openCreate() {
    this.isEditing = false;
    this.editUser = null;
    this.fd = { fullName: '', email: '', password: '', phoneNumber: '', role: 'Staff', address: '', isActive: true };
    this.showForm = true;
  }

  openEdit(u: any) {
    if (u.role === 'Admin') return; // Cannot edit Admin
    this.isEditing = true;
    this.editUser = u;
    this.fd = {
      fullName: u.fullName || u.name || '',
      email: u.email,
      phoneNumber: u.phoneNumber || '',
      role: u.role || 'Customer',
      address: u.address || '',
      isActive: u.isActive !== false,
      password: ''
    };
    this.showForm = true;
  }

  saveForm() {
    if (!this.fd.fullName?.trim()) { this.errMsg = 'Vui lòng nhập họ tên'; setTimeout(() => this.errMsg = '', 3000); return; }
    if (!this.fd.email?.trim()) { this.errMsg = 'Vui lòng nhập email'; setTimeout(() => this.errMsg = '', 3000); return; }
    if (!this.isEditing && !this.fd.password?.trim()) { this.errMsg = 'Vui lòng nhập mật khẩu'; setTimeout(() => this.errMsg = '', 3000); return; }

    this.saving = true;

    if (this.isEditing && this.editUser) {
      const data: any = {
        fullName: this.fd.fullName,
        phoneNumber: this.fd.phoneNumber,
        role: this.fd.role,
        address: this.fd.address,
        isActive: this.fd.isActive
      };
      if (this.fd.password?.trim()) data.password = this.fd.password;

      this.api.updateUser(this.editUser._id, data).subscribe({
        next: () => { this.saving = false; this.msg = 'Đã cập nhật tài khoản!'; this.showForm = false; this.load(); setTimeout(() => this.msg = '', 3000); },
        error: (err: any) => { this.saving = false; this.errMsg = err.error?.message || 'Lỗi cập nhật'; setTimeout(() => this.errMsg = '', 3000); }
      });
    } else {
      this.api.createUser(this.fd).subscribe({
        next: () => { this.saving = false; this.msg = 'Đã tạo tài khoản thành công!'; this.showForm = false; this.load(); setTimeout(() => this.msg = '', 3000); },
        error: (err: any) => { this.saving = false; this.errMsg = err.error?.message || 'Lỗi tạo tài khoản'; setTimeout(() => this.errMsg = '', 3000); }
      });
    }
  }

  confirmDelete(u: any) {
    if (u.role === 'Admin') return;
    this.deleteTarget = u;
    this.showDeleteConfirm = true;
  }

  doDelete() {
    if (!this.deleteTarget) return;
    this.saving = true;
    this.api.deleteUser(this.deleteTarget._id).subscribe({
      next: () => { this.saving = false; this.msg = 'Đã xóa tài khoản thành công!'; this.showDeleteConfirm = false; this.load(); setTimeout(() => this.msg = '', 3000); },
      error: (err: any) => { this.saving = false; this.errMsg = err.error?.message || 'Lỗi'; setTimeout(() => this.errMsg = '', 3000); }
    });
  }

  togActive(u: any) {
    if (u.role === 'Admin') return;
    const newState = !(u.isActive !== false);
    this.api.updateUser(u._id, { isActive: newState }).subscribe({
      next: () => { u.isActive = newState; this.calcTotals(); },
      error: (err: any) => { this.errMsg = err.error?.message || 'Lỗi cập nhật trạng thái'; setTimeout(() => this.errMsg = '', 3000); }
    });
  }
}
