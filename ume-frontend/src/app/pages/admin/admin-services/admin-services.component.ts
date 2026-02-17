import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../services/api.service';

@Component({
  selector: 'app-admin-services',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page-header">
      <div><h4><i class="fas fa-cut text-gold"></i> Quản lý dịch vụ</h4>
        <ol class="breadcrumb"><li><a routerLink="/admin">Dashboard</a></li><li class="active">Dịch vụ</li></ol>
      </div>
      <button class="btn btn-gold" (click)="openForm()"><i class="fas fa-plus mr-1"></i>Thêm dịch vụ</button>
    </div>
    <div class="alert alert-success" *ngIf="msg"><i class="fas fa-check-circle mr-2"></i>{{msg}}<button class="alert-x" (click)="msg=''">&times;</button></div>

    <div class="card">
      <div class="card-header">
        <div class="search-box"><input class="form-control" [(ngModel)]="search" placeholder="Tìm kiếm..." (keyup.enter)="load()"><button class="btn btn-gold btn-sm" (click)="load()"><i class="fas fa-search"></i></button></div>
      </div>
      <div class="card-body p-0">
        <table class="adm-table">
          <thead><tr><th>#</th><th>Hình</th><th>Tên dịch vụ</th><th>Danh mục</th><th>Giá</th><th>Thời gian</th><th>Lượt đặt</th><th>Trạng thái</th><th>Thao tác</th></tr></thead>
          <tbody>
            <tr *ngFor="let s of items; let i=index">
              <td>{{i+1}}</td>
              <td><img [src]="getImg(s.imageUrl||s.image)" class="img-t"></td>
              <td><strong>{{s.name}}</strong><span class="star-b" *ngIf="s.isFeatured"><i class="fas fa-star"></i></span></td>
              <td><span class="cat-b">{{s.category?.name||s.categoryName||'-'}}</span></td>
              <td><strong class="text-gold">{{s.price|number:'1.0-0'}}đ</strong><br *ngIf="s.originalPrice>s.price"><small class="strike" *ngIf="s.originalPrice>s.price">{{s.originalPrice|number:'1.0-0'}}đ</small></td>
              <td>{{s.durationMinutes||0}} phút</td>
              <td><span class="bk-b">{{s.bookingCount||0}}</span></td>
              <td><label class="sw"><input type="checkbox" [checked]="s.isActive!==false" (change)="togAct(s)"><span class="sl"></span></label></td>
              <td><div class="act-g"><button class="ab aw" (click)="openForm(s)"><i class="fas fa-edit"></i></button><button class="ab ar" (click)="confirmDel(s)"><i class="fas fa-trash"></i></button></div></td>
            </tr>
            <tr *ngIf="!items.length&&!ld"><td colspan="9" class="empty">Không có dịch vụ nào</td></tr>
            <tr *ngIf="ld"><td colspan="9" class="empty"><i class="fas fa-spinner fa-spin fa-2x text-gold"></i></td></tr>
          </tbody>
        </table>
      </div>
    </div>

    <div class="mo" *ngIf="showForm" (click)="showForm=false"><div class="md md-lg" (click)="$event.stopPropagation()">
      <div class="mh bg-g"><h5><i class="fas fa-cut mr-2"></i>{{editing?'Sửa':'Thêm'}} dịch vụ</h5><button class="mx" (click)="showForm=false">&times;</button></div>
      <div class="mb-modal">
        <div class="row">
          <div class="col-8">
            <div class="fg"><label>Tên dịch vụ <span class="req">*</span></label><input class="form-control" [(ngModel)]="fd.name"></div>
            <div class="row"><div class="col-6"><div class="fg"><label>Giá <span class="req">*</span></label><input type="number" class="form-control" [(ngModel)]="fd.price" min="0"></div></div><div class="col-6"><div class="fg"><label>Giá gốc</label><input type="number" class="form-control" [(ngModel)]="fd.originalPrice" min="0"></div></div></div>
            <div class="row"><div class="col-6"><div class="fg"><label>Thời gian (phút)</label><input type="number" class="form-control" [(ngModel)]="fd.durationMinutes" min="0"></div></div><div class="col-6"><div class="fg"><label>Danh mục</label><select class="form-control" [(ngModel)]="fd.category"><option value="">-- Chọn --</option><option *ngFor="let c of svcCats" [value]="c._id">{{c.name}}</option></select></div></div></div>
            <div class="fg"><label>Mô tả</label><textarea class="form-control" [(ngModel)]="fd.description" rows="3"></textarea></div>
          </div>
          <div class="col-4">
            <div class="fg"><label>Hình ảnh</label><div class="img-up" (click)="fi.click()"><img *ngIf="imgPv" [src]="imgPv" class="pv-img"><div *ngIf="!imgPv" class="up-ph"><i class="fas fa-cloud-upload-alt"></i><span>Chọn ảnh</span></div></div><input type="file" #fi (change)="onFile($event)" accept="image/*" hidden></div>
            <div class="fg mt-2"><label class="ck-label"><input type="checkbox" [(ngModel)]="fd.isActive"> Hoạt động</label></div>
            <div class="fg"><label class="ck-label"><input type="checkbox" [(ngModel)]="fd.isFeatured"> Nổi bật</label></div>
          </div>
        </div>
      </div>
      <div class="mf"><button class="btn btn-sec" (click)="showForm=false">Hủy</button><button class="btn btn-gold" (click)="save()" [disabled]="saving"><i class="fas fa-save mr-1"></i>{{saving?'Đang lưu...':'Lưu'}}</button></div>
    </div></div>

    <div class="mo" *ngIf="showDel" (click)="showDel=false"><div class="md" (click)="$event.stopPropagation()">
      <div class="mh bg-r"><h5><i class="fas fa-trash mr-2"></i>Xác nhận xóa</h5><button class="mx" (click)="showDel=false">&times;</button></div>
      <div class="mb-modal"><p>Bạn có chắc muốn xóa dịch vụ <strong>{{delItem?.name}}</strong>?</p></div>
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
    .card{border:none;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,.08);margin-bottom:16px;background:#fff}
    .card-header{background:#fff;border-bottom:1px solid #f0f0f0;padding:12px 16px;border-radius:12px 12px 0 0!important}.card-body{padding:16px}
    .search-box{display:flex;gap:8px}.search-box .form-control{width:250px}
    .row{display:flex;flex-wrap:wrap;margin:0 -8px}[class*="col-"]{padding:0 8px;box-sizing:border-box}.col-4{flex:0 0 33.333%;max-width:33.333%}.col-6{flex:0 0 50%;max-width:50%}.col-8{flex:0 0 66.666%;max-width:66.666%}
    @media(max-width:991px){[class*="col-"]{flex:0 0 100%;max-width:100%}}
    label{display:block;font-size:.85rem;font-weight:500;margin-bottom:4px;color:#333}
    .form-control{width:100%;padding:8px 12px;border:1px solid #dee2e6;border-radius:6px;font-size:.9rem;box-sizing:border-box}.form-control:focus{border-color:#D4AF37;outline:none;box-shadow:0 0 0 3px rgba(212,175,55,.15)}
    select.form-control{appearance:auto}textarea.form-control{resize:vertical}.fg{margin-bottom:12px}
    .p-0{padding:0!important}.mr-1{margin-right:4px}.mr-2{margin-right:8px}.mt-2{margin-top:8px}
    .adm-table{width:100%;border-collapse:collapse}.adm-table th,.adm-table td{padding:10px 16px;border-bottom:1px solid #f0f0f0;font-size:.85rem;vertical-align:middle}
    .adm-table thead th{background:#343a40;color:#fff;font-weight:600;white-space:nowrap}.adm-table tbody tr:nth-child(odd){background:rgba(0,0,0,.02)}.adm-table tbody tr:hover{background:rgba(212,175,55,.05)}
    .img-t{width:50px;height:50px;object-fit:cover;border-radius:6px;border:1px solid #dee2e6}
    .star-b{background:#ffc107;color:#fff;padding:1px 5px;border-radius:4px;font-size:.65rem;margin-left:4px}
    .cat-b{background:#17a2b8;color:#fff;padding:2px 8px;border-radius:4px;font-size:.75rem}
    .bk-b{background:#6c757d;color:#fff;padding:2px 8px;border-radius:4px;font-size:.75rem}
    .strike{text-decoration:line-through;color:#adb5bd}
    .sw{position:relative;display:inline-block;width:40px;height:22px;margin:0}.sw input{opacity:0;width:0;height:0}
    .sl{position:absolute;cursor:pointer;top:0;left:0;right:0;bottom:0;background:#ccc;transition:.3s;border-radius:22px}
    .sl:before{position:absolute;content:"";height:16px;width:16px;left:3px;bottom:3px;background:#fff;transition:.3s;border-radius:50%}
    input:checked+.sl{background:#D4AF37}input:checked+.sl:before{transform:translateX(18px)}
    .act-g{display:flex;gap:4px}.ab{padding:6px 10px;border:none;border-radius:4px;cursor:pointer;color:#fff;font-size:.8rem}.aw{background:#ffc107;color:#1a1a1a}.ar{background:#dc3545}
    .empty{text-align:center;padding:30px 0;color:#adb5bd}
    .mo{position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,.5);z-index:1060;display:flex;align-items:center;justify-content:center;padding:20px}
    .md{background:#fff;border-radius:12px;max-width:500px;width:100%;max-height:90vh;overflow-y:auto;box-shadow:0 10px 40px rgba(0,0,0,.2)}.md-lg{max-width:750px}
    .mh{padding:16px 20px;border-radius:12px 12px 0 0;display:flex;justify-content:space-between;align-items:center;color:#fff}.mh h5{margin:0;font-size:1.05rem}.mx{background:none;border:none;color:#fff;font-size:1.5rem;cursor:pointer}
    .mb-modal{padding:20px}.mf{padding:12px 20px;border-top:1px solid #f0f0f0;display:flex;justify-content:flex-end;gap:8px}
    .bg-g{background:#D4AF37}.bg-r{background:#dc3545}
    .img-up{width:100%;aspect-ratio:1;border:2px dashed #dee2e6;border-radius:8px;cursor:pointer;display:flex;align-items:center;justify-content:center;overflow:hidden}.img-up:hover{border-color:#D4AF37}
    .pv-img{width:100%;height:100%;object-fit:cover}.up-ph{display:flex;flex-direction:column;align-items:center;color:#adb5bd;gap:8px}.up-ph i{font-size:2rem}
    .ck-label{display:flex;align-items:center;gap:8px;cursor:pointer;font-size:.9rem}.ck-label input{width:16px;height:16px;accent-color:#D4AF37}
    .req{color:#dc3545}
  `]
})
export class AdminServicesComponent implements OnInit {
  items: any[] = [];
  svcCats: any[] = [];
  ld = false; saving = false; msg = ''; search = '';
  showForm = false; editing: any = null; fd: any = {}; selFile: File | null = null; imgPv = '';
  showDel = false; delItem: any = null;

  constructor(private api: ApiService) {}
  ngOnInit() { this.load(); this.loadCats(); }

  getImg(path: string): string { return this.api.getImageUrl(path) || 'https://via.placeholder.com/50'; }

  load() {
    this.ld = true;
    this.api.getServices({ limit: 100, search: this.search || undefined }).subscribe({
      next: (r: any) => { this.ld = false; this.items = r.data?.services || r.data || []; },
      error: () => this.ld = false
    });
  }

  loadCats() {
    this.api.getServiceCategories().subscribe({
      next: (r: any) => { this.svcCats = r.data?.categories || r.data || []; },
      error: () => {}
    });
  }

  openForm(s?: any) {
    this.editing = s || null;
    this.fd = s ? { name: s.name, price: s.price, originalPrice: s.originalPrice || 0, durationMinutes: s.durationMinutes || 30, category: s.category?._id || s.category || '', description: s.description || '', isActive: s.isActive !== false, isFeatured: s.isFeatured || false }
      : { name: '', price: 0, originalPrice: 0, durationMinutes: 30, category: '', description: '', isActive: true, isFeatured: false };
    this.selFile = null; this.imgPv = this.getImg(s?.imageUrl || s?.image || '');
    this.showForm = true;
  }

  onFile(e: any) { const f = e.target.files[0]; if (f) { this.selFile = f; const r = new FileReader(); r.onload = (ev: any) => this.imgPv = ev.target.result; r.readAsDataURL(f); } }

  save() {
    if (!this.fd.name || !this.fd.price) return;
    this.saving = true;
    const d = new FormData();
    Object.keys(this.fd).forEach(k => { if (this.fd[k] !== null && this.fd[k] !== undefined && this.fd[k] !== '') d.append(k, this.fd[k]); });
    if (this.selFile) d.append('image', this.selFile);
    const obs = this.editing ? this.api.updateService(this.editing._id, d) : this.api.createService(d);
    obs.subscribe({ next: () => { this.saving = false; this.msg = this.editing ? 'Đã cập nhật!' : 'Đã thêm!'; this.showForm = false; this.load(); setTimeout(() => this.msg = '', 3000); }, error: () => this.saving = false });
  }

  togAct(s: any) { const d = new FormData(); d.append('isActive', String(!s.isActive)); this.api.updateService(s._id, d).subscribe({ next: () => s.isActive = !s.isActive, error: () => {} }); }
  confirmDel(s: any) { this.delItem = s; this.showDel = true; }
  doDelete() { if (!this.delItem) return; this.api.deleteService(this.delItem._id).subscribe({ next: () => { this.msg = 'Đã xóa!'; this.load(); this.showDel = false; setTimeout(() => this.msg = '', 3000); }, error: () => this.showDel = false }); }
}
