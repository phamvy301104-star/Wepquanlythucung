import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../services/api.service';

@Component({
  selector: 'app-admin-staff',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page-header">
      <div><h4><i class="fas fa-user-tie text-gold"></i> Quản lý nhân viên</h4>
        <ol class="breadcrumb"><li><a routerLink="/admin">Dashboard</a></li><li class="active">Nhân viên</li></ol>
      </div>
      <button class="btn btn-gold" (click)="openForm()"><i class="fas fa-plus mr-1"></i>Thêm nhân viên</button>
    </div>
    <div class="alert alert-success" *ngIf="msg"><i class="fas fa-check-circle mr-2"></i>{{msg}}<button class="alert-x" (click)="msg=''">&times;</button></div>

    <div class="stats-row">
      <div class="stat-box"><div class="sb-icon bg-info"><i class="fas fa-users"></i></div><div><div class="sb-num">{{totals.total||0}}</div><div class="sb-label">Tổng nhân viên</div></div></div>
      <div class="stat-box"><div class="sb-icon bg-success"><i class="fas fa-check-circle"></i></div><div><div class="sb-num">{{totals.active||0}}</div><div class="sb-label">Đang làm</div></div></div>
      <div class="stat-box"><div class="sb-icon bg-warning"><i class="fas fa-pause-circle"></i></div><div><div class="sb-num">{{totals.leave||0}}</div><div class="sb-label">Nghỉ phép</div></div></div>
      <div class="stat-box"><div class="sb-icon bg-gold"><i class="fas fa-star"></i></div><div><div class="sb-num">{{totals.avgRating||'0.0'}}</div><div class="sb-label">Đánh giá TB</div></div></div>
    </div>

    <div class="card">
      <div class="card-header">
        <div class="filter-row">
          <div class="search-box"><input class="form-control" [(ngModel)]="search" placeholder="Tìm kiếm..." (keyup.enter)="load()"><button class="btn btn-gold btn-sm" (click)="load()"><i class="fas fa-search"></i></button></div>
          <select class="form-control fc-sm" [(ngModel)]="statusFilter" (change)="load()"><option value="">Tất cả trạng thái</option><option value="Active">Đang làm</option><option value="Resigned">Nghỉ việc</option><option value="OnLeave">Nghỉ phép</option></select>
        </div>
      </div>
      <div class="card-body p-0">
        <table class="adm-table">
          <thead><tr><th>#</th><th>Nhân viên</th><th>Chức vụ</th><th>Liên hệ</th><th>Ngày vào</th><th>Lịch hẹn</th><th>Đánh giá</th><th>Trạng thái</th><th>Thao tác</th></tr></thead>
          <tbody>
            <tr *ngFor="let s of items; let i=index">
              <td>{{i+1}}</td>
              <td><div class="staff-cell"><img [src]="getImg(s.avatarUrl||s.avatar)" class="avatar"><div><strong>{{s.fullName||s.name}}</strong><div class="sub-txt">{{s.email}}</div></div></div></td>
              <td><span class="pos-b">{{s.position||s.role||'Nhân viên'}}</span></td>
              <td><span><i class="fas fa-phone fa-sm mr-1"></i>{{s.phoneNumber||'-'}}</span></td>
              <td>{{(s.hireDate||s.createdAt)|date:'dd/MM/yyyy'}}</td>
              <td><span class="bk-b">{{s.appointmentCount||0}}</span></td>
              <td><span class="rating"><i class="fas fa-star text-gold"></i> {{s.averageRating||0|number:'1.1-1'}}</span></td>
              <td><span class="st-badge" [ngClass]="{'st-active':s.status==='Active','st-leave':s.status==='OnLeave','st-inactive':s.status==='Resigned'}">{{s.status==='Active'?'Đang làm':s.status==='OnLeave'?'Nghỉ phép':'Nghỉ việc'}}</span></td>
              <td><div class="act-g"><button class="ab aw" (click)="openForm(s)"><i class="fas fa-edit"></i></button><button class="ab ar" (click)="confirmDel(s)"><i class="fas fa-trash"></i></button></div></td>
            </tr>
            <tr *ngIf="!items.length&&!ld"><td colspan="9" class="empty">Không có nhân viên nào</td></tr>
            <tr *ngIf="ld"><td colspan="9" class="empty"><i class="fas fa-spinner fa-spin fa-2x text-gold"></i></td></tr>
          </tbody>
        </table>
      </div>
    </div>

    <div class="mo" *ngIf="showForm" (click)="showForm=false"><div class="md md-lg" (click)="$event.stopPropagation()">
      <div class="mh bg-g"><h5><i class="fas fa-user-tie mr-2"></i>{{editing?'Sửa':'Thêm'}} nhân viên</h5><button class="mx" (click)="showForm=false">&times;</button></div>
      <div class="mb-modal">
        <div class="row">
          <div class="col-8">
            <div class="row"><div class="col-6"><div class="fg"><label>Họ tên <span class="req">*</span></label><input class="form-control" [(ngModel)]="fd.fullName"></div></div><div class="col-6"><div class="fg"><label>Email <span class="req">*</span></label><input type="email" class="form-control" [(ngModel)]="fd.email"></div></div></div>
            <div class="row"><div class="col-6"><div class="fg"><label>Số điện thoại</label><input class="form-control" [(ngModel)]="fd.phoneNumber"></div></div><div class="col-6"><div class="fg"><label>Chức vụ</label><input class="form-control" [(ngModel)]="fd.position" placeholder="Thợ cắt tóc"></div></div></div>
            <div class="row"><div class="col-6"><div class="fg"><label>Trạng thái</label><select class="form-control" [(ngModel)]="fd.status"><option value="Active">Đang làm</option><option value="OnLeave">Nghỉ phép</option><option value="Resigned">Nghỉ việc</option></select></div></div><div class="col-6"><div class="fg"><label>Ngày vào làm</label><input type="date" class="form-control" [(ngModel)]="fd.hireDate"></div></div></div>
            <div class="fg"><label>Mô tả / Ghi chú</label><textarea class="form-control" [(ngModel)]="fd.bio" rows="3"></textarea></div>
            <div class="fg"><label>Dịch vụ đảm nhận</label>
              <div class="svc-grid">
                <label class="ck-label" *ngFor="let sv of allServices"><input type="checkbox" [checked]="fd.services?.includes(sv._id)" (change)="togSvc(sv._id,$event)">{{sv.name}}</label>
              </div>
            </div>
          </div>
          <div class="col-4">
            <div class="fg"><label>Ảnh đại diện</label><div class="img-up" (click)="fi.click()"><img *ngIf="imgPv" [src]="imgPv" class="pv-img"><div *ngIf="!imgPv" class="up-ph"><i class="fas fa-user fa-2x"></i><span>Chọn ảnh</span></div></div><input type="file" #fi (change)="onFile($event)" accept="image/*" hidden></div>
          </div>
        </div>
      </div>
      <div class="mf"><button class="btn btn-sec" (click)="showForm=false">Hủy</button><button class="btn btn-gold" (click)="save()" [disabled]="saving"><i class="fas fa-save mr-1"></i>{{saving?'Đang lưu...':'Lưu'}}</button></div>
    </div></div>

    <div class="mo" *ngIf="showDel" (click)="showDel=false"><div class="md" (click)="$event.stopPropagation()">
      <div class="mh bg-r"><h5><i class="fas fa-trash mr-2"></i>Xác nhận xóa</h5><button class="mx" (click)="showDel=false">&times;</button></div>
      <div class="mb-modal"><p>Bạn có chắc muốn xóa nhân viên <strong>{{delItem?.fullName||delItem?.name}}</strong>?</p></div>
      <div class="mf"><button class="btn btn-sec" (click)="showDel=false">Hủy</button><button class="btn btn-danger" (click)="doDelete()"><i class="fas fa-trash mr-1"></i>Xóa</button></div>
    </div></div>
  `,
  styles: [`
    :host{display:block}
    .page-header{display:flex;justify-content:space-between;align-items:flex-start;margin-bottom:16px;flex-wrap:wrap;gap:8px}
    .page-header h4{font-weight:600;color:#1a1a1a;font-size:1.3rem;margin:0}.page-header h4 i{margin-right:8px}
    .text-gold{color:#D4AF37!important}
    .breadcrumb{list-style:none;display:flex;gap:8px;padding:0;margin:4px 0 0;font-size:.85rem}.breadcrumb a{color:#D4AF37;text-decoration:none}.breadcrumb .active{color:#6c757d}.breadcrumb li+li::before{content:"/";margin-right:8px;color:#adb5bd}
    .btn{padding:8px 16px;border:none;border-radius:6px;cursor:pointer;font-size:.85rem;display:inline-flex;align-items:center;gap:4px;transition:all .2s}
    .btn-gold{background:#D4AF37;color:#fff}.btn-gold:hover{background:#B8960C}.btn-sec{background:#6c757d;color:#fff}.btn-danger{background:#dc3545;color:#fff}.btn-sm{padding:6px 12px}.btn:disabled{opacity:.6}
    .alert{padding:12px 16px;border-radius:8px;margin-bottom:16px;display:flex;align-items:center;position:relative}.alert-success{background:#d4edda;color:#155724;border:1px solid #c3e6cb}.alert-x{position:absolute;right:12px;background:none;border:none;font-size:1.2rem;cursor:pointer;color:inherit}
    .stats-row{display:grid;grid-template-columns:repeat(4,1fr);gap:16px;margin-bottom:16px}
    .stat-box{display:flex;align-items:center;gap:12px;background:#fff;padding:16px;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,.08)}
    .sb-icon{width:48px;height:48px;border-radius:10px;display:flex;align-items:center;justify-content:center;font-size:1.2rem;color:#fff}
    .bg-info{background:#17a2b8}.bg-success{background:#28a745}.bg-warning{background:#ffc107;color:#1a1a1a!important}.bg-gold{background:#D4AF37}
    .sb-num{font-size:1.4rem;font-weight:700;color:#1a1a1a}.sb-label{font-size:.8rem;color:#6c757d}
    @media(max-width:991px){.stats-row{grid-template-columns:repeat(2,1fr)}}
    .card{border:none;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,.08);margin-bottom:16px;background:#fff}
    .card-header{background:#fff;border-bottom:1px solid #f0f0f0;padding:12px 16px;border-radius:12px 12px 0 0!important}.card-body{padding:16px}
    .filter-row{display:flex;gap:12px;align-items:center;flex-wrap:wrap}.search-box{display:flex;gap:8px}.search-box .form-control{width:250px}.fc-sm{width:180px}
    .row{display:flex;flex-wrap:wrap;margin:0 -8px}[class*="col-"]{padding:0 8px;box-sizing:border-box}.col-4{flex:0 0 33.333%;max-width:33.333%}.col-6{flex:0 0 50%;max-width:50%}.col-8{flex:0 0 66.666%;max-width:66.666%}
    @media(max-width:991px){[class*="col-"]{flex:0 0 100%;max-width:100%}}
    label{display:block;font-size:.85rem;font-weight:500;margin-bottom:4px;color:#333}
    .form-control{width:100%;padding:8px 12px;border:1px solid #dee2e6;border-radius:6px;font-size:.9rem;box-sizing:border-box}.form-control:focus{border-color:#D4AF37;outline:none;box-shadow:0 0 0 3px rgba(212,175,55,.15)}
    select.form-control{appearance:auto}textarea.form-control{resize:vertical}.fg{margin-bottom:12px}
    .p-0{padding:0!important}.mr-1{margin-right:4px}.mr-2{margin-right:8px}.mt-2{margin-top:8px}
    .adm-table{width:100%;border-collapse:collapse}.adm-table th,.adm-table td{padding:10px 16px;border-bottom:1px solid #f0f0f0;font-size:.85rem;vertical-align:middle}
    .adm-table thead th{background:#343a40;color:#fff;font-weight:600;white-space:nowrap}.adm-table tbody tr:nth-child(odd){background:rgba(0,0,0,.02)}.adm-table tbody tr:hover{background:rgba(212,175,55,.05)}
    .staff-cell{display:flex;align-items:center;gap:10px}.avatar{width:40px;height:40px;border-radius:50%;object-fit:cover;border:2px solid #dee2e6}
    .sub-txt{font-size:.75rem;color:#6c757d}
    .pos-b{background:#6f42c1;color:#fff;padding:2px 8px;border-radius:4px;font-size:.75rem}
    .bk-b{background:#6c757d;color:#fff;padding:2px 8px;border-radius:4px;font-size:.75rem}
    .rating{font-weight:600;font-size:.9rem}
    .st-badge{padding:4px 10px;border-radius:20px;font-size:.75rem;font-weight:600}
    .st-active{background:#d4edda;color:#155724}.st-leave{background:#fff3cd;color:#856404}.st-inactive{background:#f8d7da;color:#721c24}
    .act-g{display:flex;gap:4px}.ab{padding:6px 10px;border:none;border-radius:4px;cursor:pointer;color:#fff;font-size:.8rem}.aw{background:#ffc107;color:#1a1a1a}.ar{background:#dc3545}
    .empty{text-align:center;padding:30px 0;color:#adb5bd}
    .mo{position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,.5);z-index:1060;display:flex;align-items:center;justify-content:center;padding:20px}
    .md{background:#fff;border-radius:12px;max-width:500px;width:100%;max-height:90vh;overflow-y:auto;box-shadow:0 10px 40px rgba(0,0,0,.2)}.md-lg{max-width:750px}
    .mh{padding:16px 20px;border-radius:12px 12px 0 0;display:flex;justify-content:space-between;align-items:center;color:#fff}.mh h5{margin:0;font-size:1.05rem}.mx{background:none;border:none;color:#fff;font-size:1.5rem;cursor:pointer}
    .mb-modal{padding:20px}.mf{padding:12px 20px;border-top:1px solid #f0f0f0;display:flex;justify-content:flex-end;gap:8px}
    .bg-g{background:#D4AF37}.bg-r{background:#dc3545}
    .img-up{width:100%;aspect-ratio:1;border:2px dashed #dee2e6;border-radius:8px;cursor:pointer;display:flex;align-items:center;justify-content:center;overflow:hidden}.img-up:hover{border-color:#D4AF37}
    .pv-img{width:100%;height:100%;object-fit:cover}.up-ph{display:flex;flex-direction:column;align-items:center;color:#adb5bd;gap:8px}.up-ph i{font-size:2rem}
    .ck-label{display:flex;align-items:center;gap:8px;cursor:pointer;font-size:.85rem}.ck-label input{width:16px;height:16px;accent-color:#D4AF37}
    .svc-grid{display:grid;grid-template-columns:repeat(2,1fr);gap:6px;max-height:160px;overflow-y:auto;padding:8px;border:1px solid #dee2e6;border-radius:6px}
    .req{color:#dc3545}
  `]
})
export class AdminStaffComponent implements OnInit {
  items: any[] = [];
  allServices: any[] = [];
  ld = false; saving = false; msg = ''; search = ''; statusFilter = '';
  showForm = false; editing: any = null; fd: any = {}; selFile: File | null = null; imgPv = '';
  showDel = false; delItem: any = null;
  totals = { total: 0, active: 0, leave: 0, avgRating: '0.0' };

  constructor(private api: ApiService) {}
  ngOnInit() { this.load(); this.loadSvcs(); }
  getImg(path: string): string { return this.api.getImageUrl(path) || 'https://via.placeholder.com/40'; }
  load() {
    this.ld = true;
    const p: any = { limit: 100 };
    if (this.search) p.search = this.search;
    if (this.statusFilter) p.status = this.statusFilter;
    this.api.getStaffList(p).subscribe({
      next: (r: any) => {
        this.ld = false;
        this.items = r.data?.staff || r.data || [];
        this.totals.total = this.items.length;
        this.totals.active = this.items.filter((s: any) => s.status === 'Active').length;
        this.totals.leave = this.items.filter((s: any) => s.status === 'OnLeave').length;
        const ratings = this.items.filter((s: any) => s.averageRating).map((s: any) => s.averageRating);
        this.totals.avgRating = ratings.length ? (ratings.reduce((a: number, b: number) => a + b, 0) / ratings.length).toFixed(1) : '0.0';
      },
      error: () => this.ld = false
    });
  }

  loadSvcs() {
    this.api.getServices({ limit: 100 }).subscribe({
      next: (r: any) => { this.allServices = r.data?.services || r.data || []; },
      error: () => {}
    });
  }

  openForm(s?: any) {
    this.editing = s || null;
    this.fd = s ? { fullName: s.fullName || s.name || '', email: s.email || '', phoneNumber: s.phoneNumber || '', position: s.position || '', status: s.status || 'Active', hireDate: s.hireDate ? s.hireDate.substring(0, 10) : '', bio: s.bio || '', services: s.services?.map((sv: any) => sv._id || sv) || [] }
      : { fullName: '', email: '', phoneNumber: '', position: '', status: 'Active', hireDate: '', bio: '', services: [] };
    this.selFile = null; this.imgPv = this.getImg(s?.avatarUrl || s?.avatar || '');
    this.showForm = true;
  }

  togSvc(id: string, e: any) {
    if (!this.fd.services) this.fd.services = [];
    if (e.target.checked) { if (!this.fd.services.includes(id)) this.fd.services.push(id); }
    else { this.fd.services = this.fd.services.filter((x: string) => x !== id); }
  }

  onFile(e: any) { const f = e.target.files[0]; if (f) { this.selFile = f; const r = new FileReader(); r.onload = (ev: any) => this.imgPv = ev.target.result; r.readAsDataURL(f); } }

  save() {
    if (!this.fd.fullName || !this.fd.email) return;
    this.saving = true;
    const d = new FormData();
    Object.keys(this.fd).forEach(k => {
      if (k === 'services') { this.fd.services.forEach((sv: string) => d.append('services[]', sv)); }
      else if (this.fd[k] !== null && this.fd[k] !== undefined && this.fd[k] !== '') d.append(k, this.fd[k]);
    });
    if (this.selFile) d.append('avatar', this.selFile);
    const obs = this.editing ? this.api.updateStaff(this.editing._id, d) : this.api.createStaff(d);
    obs.subscribe({ next: () => { this.saving = false; this.msg = this.editing ? 'Đã cập nhật!' : 'Đã thêm!'; this.showForm = false; this.load(); setTimeout(() => this.msg = '', 3000); }, error: () => this.saving = false });
  }

  confirmDel(s: any) { this.delItem = s; this.showDel = true; }
  doDelete() { if (!this.delItem) return; this.api.deleteStaff(this.delItem._id).subscribe({ next: () => { this.msg = 'Đã xóa!'; this.load(); this.showDel = false; setTimeout(() => this.msg = '', 3000); }, error: () => this.showDel = false }); }
}
