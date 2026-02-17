import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../services/api.service';

@Component({
  selector: 'app-admin-pets',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page-header">
      <div><h4><i class="fas fa-paw text-gold"></i> Quản lý thú cưng</h4>
        <ol class="breadcrumb"><li><a routerLink="/admin">Dashboard</a></li><li class="active">Thú cưng</li></ol>
      </div>
      <button class="btn btn-gold" (click)="openForm()"><i class="fas fa-plus mr-1"></i>Thêm thú cưng</button>
    </div>
    <div class="alert alert-success" *ngIf="msg"><i class="fas fa-check-circle mr-2"></i>{{msg}}<button class="alert-x" (click)="msg=''">&times;</button></div>

    <div class="stats-row">
      <div class="stat-box"><div class="sb-icon bg-info"><i class="fas fa-paw"></i></div><div><div class="sb-num">{{totals.total}}</div><div class="sb-label">Tổng thú cưng</div></div></div>
      <div class="stat-box"><div class="sb-icon bg-success"><i class="fas fa-tag"></i></div><div><div class="sb-num">{{totals.sale}}</div><div class="sb-label">Đang bán</div></div></div>
      <div class="stat-box"><div class="sb-icon bg-pink"><i class="fas fa-heart"></i></div><div><div class="sb-num">{{totals.adoption}}</div><div class="sb-label">Nhận nuôi</div></div></div>
      <div class="stat-box"><div class="sb-icon bg-gold"><i class="fas fa-dog"></i></div><div><div class="sb-num">{{totals.dogs}}</div><div class="sb-label">Chó</div></div></div>
    </div>

    <div class="card">
      <div class="card-header">
        <div class="filter-row">
          <div class="search-box"><input class="form-control" [(ngModel)]="search" placeholder="Tìm tên, giống..." (keyup.enter)="load()"><button class="btn btn-gold btn-sm" (click)="load()"><i class="fas fa-search"></i></button></div>
          <select class="form-control fc-sm" [(ngModel)]="filterType" (change)="load()"><option value="">Tất cả loại</option><option value="Dog">Chó</option><option value="Cat">Mèo</option><option value="Bird">Chim</option><option value="Fish">Cá</option><option value="Hamster">Hamster</option><option value="Rabbit">Thỏ</option><option value="Other">Khác</option></select>
          <select class="form-control fc-sm" [(ngModel)]="filterListing" (change)="load()"><option value="">Tất cả</option><option value="None">Chưa đăng</option><option value="Sale">Đang bán</option><option value="Adoption">Nhận nuôi</option></select>
        </div>
      </div>
      <div class="card-body p-0">
        <table class="adm-table">
          <thead><tr><th>#</th><th>Hình</th><th>Tên</th><th>Loại / Giống</th><th>Chủ sở hữu</th><th>Tuổi</th><th>Cân nặng</th><th>Tình trạng</th><th>Đăng bán</th><th>Ngày tạo</th><th>Thao tác</th></tr></thead>
          <tbody>
            <tr *ngFor="let p of items; let i=index">
              <td>{{i+1}}</td>
              <td><img [src]="getImg(p.imageUrl)" class="pet-img" onerror="this.src='assets/images/default-pet.svg'"></td>
              <td><strong>{{p.name}}</strong></td>
              <td><span class="type-b" [ngClass]="'type-'+p.type">{{typeName(p.type)}}</span><div class="sub-txt">{{p.breed||'-'}}</div></td>
              <td><div><strong>{{p.owner?.fullName||'-'}}</strong><div class="sub-txt">{{p.owner?.email||''}}</div></div></td>
              <td>{{p.age}} {{p.ageUnit==='years'?'tuổi':'tháng'}}</td>
              <td>{{p.weight?p.weight+'kg':'-'}}</td>
              <td><div class="health-tags"><span class="ht ht-ok" *ngIf="p.vaccinated"><i class="fas fa-syringe"></i></span><span class="ht ht-ok" *ngIf="p.neutered"><i class="fas fa-cut"></i></span><span class="ht ht-g" *ngIf="p.gender==='Male'"><i class="fas fa-mars"></i></span><span class="ht ht-p" *ngIf="p.gender==='Female'"><i class="fas fa-venus"></i></span></div></td>
              <td>
                <span class="list-b" [ngClass]="'list-'+p.listingType">{{listName(p.listingType)}}</span>
                <div class="sub-txt text-gold" *ngIf="p.listingPrice">{{p.listingPrice|number:'1.0-0'}}đ</div>
              </td>
              <td>{{p.createdAt|date:'dd/MM/yyyy'}}</td>
              <td><div class="act-g"><button class="ab ai" (click)="viewDetail(p)"><i class="fas fa-eye"></i></button><button class="ab aw" (click)="openForm(p)"><i class="fas fa-edit"></i></button><button class="ab ar" (click)="confirmDel(p)"><i class="fas fa-trash"></i></button></div></td>
            </tr>
            <tr *ngIf="!items.length&&!ld"><td colspan="11" class="empty">Không có thú cưng nào</td></tr>
            <tr *ngIf="ld"><td colspan="11" class="empty"><i class="fas fa-spinner fa-spin fa-2x text-gold"></i></td></tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Detail Modal -->
    <div class="mo" *ngIf="showDetail" (click)="showDetail=false"><div class="md md-lg" (click)="$event.stopPropagation()">
      <div class="mh bg-g"><h5><i class="fas fa-paw mr-2"></i>Chi tiết: {{det?.name}}</h5><button class="mx" (click)="showDetail=false">&times;</button></div>
      <div class="mb-modal">
        <div class="det-top">
          <img [src]="getImg(det?.imageUrl)" class="det-img" onerror="this.src='assets/images/default-pet.svg'">
          <div class="det-main">
            <h4>{{det?.name}}</h4>
            <span class="type-b" [ngClass]="'type-'+det?.type">{{typeName(det?.type)}}</span>
            <span class="list-b ml-1" [ngClass]="'list-'+det?.listingType">{{listName(det?.listingType)}}</span>
          </div>
        </div>
        <div class="detail-grid">
          <div class="dg-item"><label>Giống</label><span>{{det?.breed||'-'}}</span></div>
          <div class="dg-item"><label>Giới tính</label><span>{{det?.gender==='Male'?'Đực':det?.gender==='Female'?'Cái':'Chưa rõ'}}</span></div>
          <div class="dg-item"><label>Tuổi</label><span>{{det?.age}} {{det?.ageUnit==='years'?'tuổi':'tháng'}}</span></div>
          <div class="dg-item"><label>Cân nặng</label><span>{{det?.weight?det.weight+'kg':'-'}}</span></div>
          <div class="dg-item"><label>Màu lông</label><span>{{det?.color||'-'}}</span></div>
          <div class="dg-item"><label>Chủ sở hữu</label><span>{{det?.owner?.fullName||'-'}}</span></div>
          <div class="dg-item"><label>Tiêm phòng</label><span [class.text-success]="det?.vaccinated">{{det?.vaccinated?'Đã tiêm':'Chưa tiêm'}}</span></div>
          <div class="dg-item"><label>Triệt sản</label><span>{{det?.neutered?'Đã triệt':'Chưa'}}</span></div>
          <div class="dg-item full" *ngIf="det?.description"><label>Mô tả</label><span>{{det?.description}}</span></div>
          <div class="dg-item full" *ngIf="det?.healthNotes"><label>Ghi chú sức khỏe</label><span>{{det?.healthNotes}}</span></div>
          <div class="dg-item full" *ngIf="det?.listingDescription"><label>Mô tả đăng bán</label><span>{{det?.listingDescription}}</span></div>
          <div class="dg-item" *ngIf="det?.listingPrice"><label>Giá bán</label><span class="text-gold fw-bold">{{det?.listingPrice|number:'1.0-0'}}đ</span></div>
          <div class="dg-item" *ngIf="det?.listingStatus"><label>Trạng thái</label><span>{{det?.listingStatus}}</span></div>
        </div>
      </div>
      <div class="mf"><button class="btn btn-gold" (click)="showDetail=false;openForm(det)"><i class="fas fa-edit mr-1"></i>Sửa</button><button class="btn btn-sec" (click)="showDetail=false">Đóng</button></div>
    </div></div>

    <!-- Add/Edit Modal -->
    <div class="mo" *ngIf="showForm" (click)="showForm=false"><div class="md md-lg" (click)="$event.stopPropagation()">
      <div class="mh bg-g"><h5><i class="fas fa-paw mr-2"></i>{{editing?'Sửa':'Thêm'}} thú cưng</h5><button class="mx" (click)="showForm=false">&times;</button></div>
      <div class="mb-modal">
        <div class="row">
          <div class="col-8">
            <div class="row"><div class="col-6"><div class="fg"><label>Tên <span class="req">*</span></label><input class="form-control" [(ngModel)]="fd.name"></div></div><div class="col-6"><div class="fg"><label>Loại <span class="req">*</span></label><select class="form-control" [(ngModel)]="fd.type"><option value="Dog">Chó</option><option value="Cat">Mèo</option><option value="Bird">Chim</option><option value="Fish">Cá</option><option value="Hamster">Hamster</option><option value="Rabbit">Thỏ</option><option value="Other">Khác</option></select></div></div></div>
            <div class="row"><div class="col-6"><div class="fg"><label>Giống</label><input class="form-control" [(ngModel)]="fd.breed" placeholder="VD: Golden Retriever"></div></div><div class="col-6"><div class="fg"><label>Giới tính</label><select class="form-control" [(ngModel)]="fd.gender"><option value="Male">Đực</option><option value="Female">Cái</option><option value="Unknown">Chưa rõ</option></select></div></div></div>
            <div class="row"><div class="col-4"><div class="fg"><label>Tuổi</label><input type="number" class="form-control" [(ngModel)]="fd.age" min="0"></div></div><div class="col-4"><div class="fg"><label>Đơn vị</label><select class="form-control" [(ngModel)]="fd.ageUnit"><option value="months">Tháng</option><option value="years">Năm</option></select></div></div><div class="col-4"><div class="fg"><label>Cân nặng (kg)</label><input type="number" class="form-control" [(ngModel)]="fd.weight" min="0" step="0.1"></div></div></div>
            <div class="row"><div class="col-6"><div class="fg"><label>Màu lông</label><input class="form-control" [(ngModel)]="fd.color"></div></div><div class="col-6"><div class="fg ck-row"><label class="ck-label"><input type="checkbox" [(ngModel)]="fd.vaccinated"> Đã tiêm phòng</label><label class="ck-label"><input type="checkbox" [(ngModel)]="fd.neutered"> Đã triệt sản</label></div></div></div>
            <div class="fg"><label>Mô tả</label><textarea class="form-control" [(ngModel)]="fd.description" rows="2" placeholder="Mô tả thú cưng..."></textarea></div>
            <div class="fg"><label>Ghi chú sức khỏe</label><textarea class="form-control" [(ngModel)]="fd.healthNotes" rows="2" placeholder="Dị ứng, bệnh lý..."></textarea></div>
            <h6 class="sep-title"><i class="fas fa-store mr-1"></i>Thông tin đăng bán / Nhận nuôi</h6>
            <div class="row"><div class="col-4"><div class="fg"><label>Hình thức</label><select class="form-control" [(ngModel)]="fd.listingType"><option value="None">Không đăng</option><option value="Sale">Đăng bán</option><option value="Adoption">Cho nhận nuôi</option></select></div></div><div class="col-4"><div class="fg"><label>Giá (VNĐ)</label><input type="number" class="form-control" [(ngModel)]="fd.listingPrice" min="0" [disabled]="fd.listingType!=='Sale'"></div></div><div class="col-4"><div class="fg"><label>Trạng thái</label><select class="form-control" [(ngModel)]="fd.listingStatus"><option value="Active">Đang mở</option><option value="Inactive">Tạm ẩn</option><option value="Sold">Đã bán</option><option value="Adopted">Đã nhận nuôi</option></select></div></div></div>
            <div class="fg"><label>Mô tả đăng bán</label><textarea class="form-control" [(ngModel)]="fd.listingDescription" rows="2" placeholder="Thông tin thêm cho người mua..."></textarea></div>
          </div>
          <div class="col-4">
            <div class="fg"><label>Ảnh thú cưng</label><div class="img-up" (click)="fi.click()"><img *ngIf="imgPv" [src]="imgPv" class="pv-img"><div *ngIf="!imgPv" class="up-ph"><i class="fas fa-camera fa-2x"></i><span>Chọn ảnh</span></div></div><input type="file" #fi (change)="onFile($event)" accept="image/*" hidden></div>
          </div>
        </div>
      </div>
      <div class="mf"><button class="btn btn-sec" (click)="showForm=false">Hủy</button><button class="btn btn-gold" (click)="save()" [disabled]="saving"><i class="fas fa-save mr-1"></i>{{saving?'Đang lưu...':'Lưu'}}</button></div>
    </div></div>

    <!-- Delete Confirm -->
    <div class="mo" *ngIf="showDel" (click)="showDel=false"><div class="md" (click)="$event.stopPropagation()">
      <div class="mh bg-r"><h5><i class="fas fa-trash mr-2"></i>Xác nhận xóa</h5><button class="mx" (click)="showDel=false">&times;</button></div>
      <div class="mb-modal"><p>Bạn có chắc muốn xóa thú cưng <strong>{{delItem?.name}}</strong>?</p></div>
      <div class="mf"><button class="btn btn-sec" (click)="showDel=false">Hủy</button><button class="btn btn-danger" (click)="doDelete()"><i class="fas fa-trash mr-1"></i>Xóa</button></div>
    </div></div>
  `,
  styles: [`
    :host{display:block}
    .page-header{display:flex;justify-content:space-between;align-items:flex-start;margin-bottom:16px;flex-wrap:wrap;gap:8px}
    .page-header h4{font-weight:600;color:#1a1a1a;font-size:1.3rem;margin:0}.page-header h4 i{margin-right:8px}
    .text-gold{color:#D4AF37!important}.text-success{color:#28a745!important}.fw-bold{font-weight:700}
    .breadcrumb{list-style:none;display:flex;gap:8px;padding:0;margin:4px 0 0;font-size:.85rem}.breadcrumb a{color:#D4AF37;text-decoration:none}.breadcrumb .active{color:#6c757d}.breadcrumb li+li::before{content:"/";margin-right:8px;color:#adb5bd}
    .btn{padding:8px 16px;border:none;border-radius:6px;cursor:pointer;font-size:.85rem;display:inline-flex;align-items:center;gap:4px;transition:all .2s}
    .btn-gold{background:#D4AF37;color:#fff}.btn-gold:hover{background:#B8960C}.btn-sec{background:#6c757d;color:#fff}.btn-danger{background:#dc3545;color:#fff}.btn-sm{padding:6px 12px}.btn:disabled{opacity:.6}
    .alert{padding:12px 16px;border-radius:8px;margin-bottom:16px;display:flex;align-items:center;position:relative}.alert-success{background:#d4edda;color:#155724;border:1px solid #c3e6cb}.alert-x{position:absolute;right:12px;background:none;border:none;font-size:1.2rem;cursor:pointer;color:inherit}
    .stats-row{display:grid;grid-template-columns:repeat(4,1fr);gap:16px;margin-bottom:16px}
    .stat-box{display:flex;align-items:center;gap:12px;background:#fff;padding:16px;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,.08)}
    .sb-icon{width:48px;height:48px;border-radius:10px;display:flex;align-items:center;justify-content:center;font-size:1.2rem;color:#fff}
    .bg-info{background:#17a2b8}.bg-success{background:#28a745}.bg-pink{background:#ec4899}.bg-gold{background:#D4AF37}
    .sb-num{font-size:1.4rem;font-weight:700;color:#1a1a1a}.sb-label{font-size:.8rem;color:#6c757d}
    @media(max-width:991px){.stats-row{grid-template-columns:repeat(2,1fr)}}
    .card{border:none;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,.08);margin-bottom:16px;background:#fff}
    .card-header{background:#fff;border-bottom:1px solid #f0f0f0;padding:12px 16px;border-radius:12px 12px 0 0!important}.card-body{padding:16px}
    .filter-row{display:flex;gap:12px;align-items:center;flex-wrap:wrap}.search-box{display:flex;gap:8px}.search-box .form-control{width:250px}.fc-sm{width:160px}
    .row{display:flex;flex-wrap:wrap;margin:0 -8px}[class*="col-"]{padding:0 8px;box-sizing:border-box}.col-4{flex:0 0 33.333%;max-width:33.333%}.col-6{flex:0 0 50%;max-width:50%}.col-8{flex:0 0 66.666%;max-width:66.666%}
    @media(max-width:991px){[class*="col-"]{flex:0 0 100%;max-width:100%}}
    label{display:block;font-size:.85rem;font-weight:500;margin-bottom:4px;color:#333}
    .form-control{width:100%;padding:8px 12px;border:1px solid #dee2e6;border-radius:6px;font-size:.9rem;box-sizing:border-box}.form-control:focus{border-color:#D4AF37;outline:none;box-shadow:0 0 0 3px rgba(212,175,55,.15)}
    select.form-control{appearance:auto}textarea.form-control{resize:vertical}.fg{margin-bottom:12px}
    .p-0{padding:0!important}.mr-1{margin-right:4px}.mr-2{margin-right:8px}.ml-1{margin-left:4px}
    .adm-table{width:100%;border-collapse:collapse}.adm-table th,.adm-table td{padding:10px 14px;border-bottom:1px solid #f0f0f0;font-size:.83rem;vertical-align:middle}
    .adm-table thead th{background:#343a40;color:#fff;font-weight:600;white-space:nowrap}.adm-table tbody tr:nth-child(odd){background:rgba(0,0,0,.02)}.adm-table tbody tr:hover{background:rgba(212,175,55,.05)}
    .pet-img{width:44px;height:44px;border-radius:10px;object-fit:cover;border:2px solid #dee2e6}
    .sub-txt{font-size:.73rem;color:#6c757d}
    .type-b{padding:3px 8px;border-radius:4px;font-size:.73rem;font-weight:600;color:#fff}
    .type-Dog{background:#D4AF37}.type-Cat{background:#ec4899}.type-Bird{background:#3b82f6}.type-Fish{background:#06b6d4}.type-Hamster{background:#f59e0b}.type-Rabbit{background:#8b5cf6}.type-Other{background:#6c757d}
    .list-b{padding:3px 8px;border-radius:4px;font-size:.73rem;font-weight:600;color:#fff}
    .list-None{background:#6c757d}.list-Sale{background:#28a745}.list-Adoption{background:#ec4899}
    .health-tags{display:flex;gap:4px}.ht{width:24px;height:24px;border-radius:50%;display:flex;align-items:center;justify-content:center;font-size:.65rem;color:#fff}.ht-ok{background:#28a745}.ht-g{background:#3b82f6}.ht-p{background:#ec4899}
    .act-g{display:flex;gap:4px}.ab{padding:6px 10px;border:none;border-radius:4px;cursor:pointer;color:#fff;font-size:.8rem}.ai{background:#17a2b8}.aw{background:#ffc107;color:#1a1a1a}.ar{background:#dc3545}
    .empty{text-align:center;padding:30px 0;color:#adb5bd}
    .mo{position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,.5);z-index:1060;display:flex;align-items:center;justify-content:center;padding:20px}
    .md{background:#fff;border-radius:12px;max-width:500px;width:100%;max-height:90vh;overflow-y:auto;box-shadow:0 10px 40px rgba(0,0,0,.2)}.md-lg{max-width:800px}
    .mh{padding:16px 20px;border-radius:12px 12px 0 0;display:flex;justify-content:space-between;align-items:center;color:#fff}.mh h5{margin:0;font-size:1.05rem}.mx{background:none;border:none;color:#fff;font-size:1.5rem;cursor:pointer}
    .mb-modal{padding:20px}.mf{padding:12px 20px;border-top:1px solid #f0f0f0;display:flex;justify-content:flex-end;gap:8px}
    .bg-g{background:#D4AF37}.bg-r{background:#dc3545}
    .det-top{display:flex;gap:16px;align-items:center;margin-bottom:16px}.det-img{width:120px;height:120px;border-radius:12px;object-fit:cover;border:3px solid #D4AF37}.det-main h4{margin:0 0 6px;font-weight:600}
    .detail-grid{display:grid;grid-template-columns:1fr 1fr;gap:12px}.dg-item{display:flex;flex-direction:column}.dg-item label{font-size:.75rem;color:#6c757d;margin-bottom:2px}.dg-item span{font-size:.9rem}.dg-item.full{grid-column:1/-1}
    h6{font-weight:600;color:#1a1a1a;margin:0 0 8px}
    .sep-title{margin-top:16px;padding-top:16px;border-top:1px solid #eee;color:#D4AF37}
    .img-up{width:100%;aspect-ratio:1;border:2px dashed #dee2e6;border-radius:12px;cursor:pointer;display:flex;align-items:center;justify-content:center;overflow:hidden}.img-up:hover{border-color:#D4AF37}
    .pv-img{width:100%;height:100%;object-fit:cover}.up-ph{display:flex;flex-direction:column;align-items:center;color:#adb5bd;gap:8px}
    .ck-row{display:flex;flex-direction:column;gap:6px;padding-top:20px}.ck-label{display:flex;align-items:center;gap:6px;cursor:pointer;font-size:.85rem;font-weight:400}.ck-label input{width:16px;height:16px;accent-color:#D4AF37}
    .req{color:#dc3545}
  `]
})
export class AdminPetsComponent implements OnInit {
  items: any[] = [];
  ld = false; saving = false; msg = ''; search = ''; filterType = ''; filterListing = '';
  showDetail = false; det: any = null;
  showForm = false; editing: any = null; fd: any = {}; selFile: File | null = null; imgPv = '';
  showDel = false; delItem: any = null;
  totals = { total: 0, sale: 0, adoption: 0, dogs: 0 };

  constructor(private api: ApiService) {}
  ngOnInit() { this.load(); }
  getImg(path: string): string { return this.api.getImageUrl(path, 'assets/images/default-pet.svg'); }

  load() {
    this.ld = true;
    const p: any = { limit: 200 };
    if (this.search) p.search = this.search;
    if (this.filterType) p.type = this.filterType;
    if (this.filterListing) p.listingType = this.filterListing;
    this.api.getPets(p).subscribe({
      next: (r: any) => {
        this.ld = false;
        this.items = r.data?.pets || r.data || [];
        this.totals.total = this.items.length;
        this.totals.sale = this.items.filter((p: any) => p.listingType === 'Sale').length;
        this.totals.adoption = this.items.filter((p: any) => p.listingType === 'Adoption').length;
        this.totals.dogs = this.items.filter((p: any) => p.type === 'Dog').length;
      },
      error: () => this.ld = false
    });
  }

  typeName(t: string) { const m: any = { Dog: 'Chó', Cat: 'Mèo', Bird: 'Chim', Fish: 'Cá', Hamster: 'Hamster', Rabbit: 'Thỏ', Other: 'Khác' }; return m[t] || t; }
  listName(t: string) { const m: any = { None: 'Chưa đăng', Sale: 'Đang bán', Adoption: 'Nhận nuôi' }; return m[t] || t || 'Chưa đăng'; }

  viewDetail(p: any) { this.det = p; this.showDetail = true; }

  openForm(p?: any) {
    this.editing = p || null;
    this.fd = p ? {
      name: p.name || '', type: p.type || 'Dog', breed: p.breed || '', gender: p.gender || 'Unknown',
      age: p.age || 0, ageUnit: p.ageUnit || 'months', weight: p.weight || 0, color: p.color || '',
      vaccinated: !!p.vaccinated, neutered: !!p.neutered,
      description: p.description || '', healthNotes: p.healthNotes || '',
      listingType: p.listingType || 'None', listingPrice: p.listingPrice || 0,
      listingStatus: p.listingStatus || 'Active', listingDescription: p.listingDescription || ''
    } : {
      name: '', type: 'Dog', breed: '', gender: 'Unknown', age: 0, ageUnit: 'months', weight: 0, color: '',
      vaccinated: false, neutered: false, description: '', healthNotes: '',
      listingType: 'None', listingPrice: 0, listingStatus: 'Active', listingDescription: ''
    };
    this.selFile = null;
    this.imgPv = p ? this.getImg(p.imageUrl) : '';
    this.showForm = true;
  }

  onFile(e: any) { const f = e.target.files[0]; if (f) { this.selFile = f; const r = new FileReader(); r.onload = (ev: any) => this.imgPv = ev.target.result; r.readAsDataURL(f); } }

  save() {
    if (!this.fd.name) return;
    this.saving = true;
    const d = new FormData();
    Object.keys(this.fd).forEach(k => {
      if (this.fd[k] !== null && this.fd[k] !== undefined) d.append(k, this.fd[k].toString());
    });
    if (this.selFile) d.append('image', this.selFile);
    const obs = this.editing ? this.api.updatePet(this.editing._id, d) : this.api.createPet(d);
    obs.subscribe({
      next: () => { this.saving = false; this.msg = this.editing ? 'Đã cập nhật!' : 'Đã thêm thú cưng!'; this.showForm = false; this.load(); setTimeout(() => this.msg = '', 3000); },
      error: (err: any) => { this.saving = false; alert(err.error?.message || 'Lỗi'); }
    });
  }

  confirmDel(p: any) { this.delItem = p; this.showDel = true; }
  doDelete() {
    if (!this.delItem) return;
    this.api.deletePet(this.delItem._id).subscribe({
      next: () => { this.msg = 'Đã xóa!'; this.load(); this.showDel = false; setTimeout(() => this.msg = '', 3000); },
      error: () => this.showDel = false
    });
  }
}
