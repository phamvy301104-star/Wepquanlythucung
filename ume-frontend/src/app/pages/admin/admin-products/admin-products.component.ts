import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../services/api.service';

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page-header">
      <div><h4><i class="fas fa-box text-gold"></i> Quản lý sản phẩm</h4>
        <ol class="breadcrumb"><li><a routerLink="/admin">Dashboard</a></li><li class="active">Sản phẩm</li></ol>
      </div>
      <button class="btn btn-gold" (click)="openForm()"><i class="fas fa-plus mr-1"></i>Thêm sản phẩm</button>
    </div>
    <div class="alert alert-success" *ngIf="msg"><i class="fas fa-check-circle mr-2"></i>{{msg}}<button class="alert-x" (click)="msg=''">&times;</button></div>

    <!-- Filter -->
    <div class="card filter-card mb-3">
      <div class="card-header ch-toggle" (click)="fOpen=!fOpen"><h3 class="card-title"><i class="fas fa-filter mr-2"></i>Bộ lọc</h3><i [class]="fOpen?'fas fa-minus':'fas fa-plus'" class="tool-icon"></i></div>
      <div class="card-body" *ngIf="fOpen">
        <div class="row">
          <div class="col-3 mb-2"><label>Tìm kiếm</label><input class="form-control" [(ngModel)]="f.search" placeholder="Tên, SKU..." (keyup.enter)="load()"></div>
          <div class="col-2 mb-2"><label>Danh mục</label><select class="form-control" [(ngModel)]="f.category" (change)="load()"><option value="">-- Tất cả --</option><option *ngFor="let c of cats" [value]="c._id">{{c.name}}</option></select></div>
          <div class="col-2 mb-2"><label>Thương hiệu</label><select class="form-control" [(ngModel)]="f.brand" (change)="load()"><option value="">-- Tất cả --</option><option *ngFor="let b of brands" [value]="b._id">{{b.name}}</option></select></div>
          <div class="col-2 mb-2"><label>Trạng thái</label><select class="form-control" [(ngModel)]="f.isActive" (change)="load()"><option value="">-- Tất cả --</option><option value="true">Hoạt động</option><option value="false">Tạm ẩn</option></select></div>
          <div class="col-3 mb-2"><label>&nbsp;</label><div class="d-flex gap-8 mt-1"><button class="btn btn-gold" (click)="load()"><i class="fas fa-search mr-1"></i>Lọc</button><button class="btn btn-sec" (click)="resetF()"><i class="fas fa-redo mr-1"></i>Reset</button></div></div>
        </div>
      </div>
    </div>

    <!-- Table -->
    <div class="card">
      <div class="card-header"><h3 class="card-title"><i class="fas fa-list mr-2"></i>Danh sách sản phẩm <span class="badge-g">{{total}}</span></h3></div>
      <div class="card-body p-0">
        <div class="table-wrap">
          <table class="adm-table">
            <thead><tr><th w="50">#</th><th w="80">Hình</th><th>SKU</th><th>Tên sản phẩm</th><th>Danh mục</th><th>Giá bán</th><th>Tồn kho</th><th>Đã bán</th><th w="100">Trạng thái</th><th w="130">Thao tác</th></tr></thead>
            <tbody>
              <tr *ngFor="let p of items; let i=index">
                <td>{{i+1+(pg-1)*ps}}</td>
                <td><img [src]="getImg(p.imageUrl||p.image)" class="img-t"></td>
                <td>{{p.sku||'-'}}</td>
                <td><strong>{{p.name}}</strong><span class="star-b" *ngIf="p.isFeatured"><i class="fas fa-star"></i></span><br *ngIf="p.averageRating>0"><small class="muted" *ngIf="p.averageRating>0"><i class="fas fa-star text-w"></i> {{p.averageRating?.toFixed(1)}}</small></td>
                <td><span class="cat-b">{{p.category?.name||p.categoryName||'-'}}</span><br><small class="muted">{{p.brand?.name||p.brandName||''}}</small></td>
                <td><strong class="text-gold">{{p.price|number:'1.0-0'}}đ</strong><br *ngIf="p.originalPrice>p.price"><small class="strike" *ngIf="p.originalPrice>p.price">{{p.originalPrice|number:'1.0-0'}}đ</small></td>
                <td><span [class]="'stk '+(p.stockQuantity<=5?'low':p.stockQuantity<=15?'med':'ok')" (click)="openStock(p)">{{p.stockQuantity||0}}</span></td>
                <td class="text-s">{{p.soldCount||0}}</td>
                <td><label class="sw"><input type="checkbox" [checked]="p.isActive!==false" (change)="togAct(p)"><span class="sl"></span></label></td>
                <td><div class="act-g"><button class="ab aw" (click)="openForm(p)" title="Sửa"><i class="fas fa-edit"></i></button><button class="ab ar" (click)="confirmDel(p)" title="Xóa"><i class="fas fa-trash"></i></button></div></td>
              </tr>
              <tr *ngIf="!items.length&&!ld"><td colspan="10" class="empty">Không tìm thấy sản phẩm nào</td></tr>
              <tr *ngIf="ld"><td colspan="10" class="empty"><i class="fas fa-spinner fa-spin fa-2x text-gold"></i></td></tr>
            </tbody>
          </table>
        </div>
        <div class="pag" *ngIf="tp>1"><span class="pi">{{(pg-1)*ps+1}}-{{pg*ps>total?total:pg*ps}} / {{total}}</span><div class="pbs"><button [disabled]="pg<=1" (click)="goPg(pg-1)"><i class="fas fa-angle-left"></i></button><button *ngFor="let n of pns" [class.active]="n===pg" (click)="goPg(n)">{{n}}</button><button [disabled]="pg>=tp" (click)="goPg(pg+1)"><i class="fas fa-angle-right"></i></button></div></div>
      </div>
    </div>

    <!-- Form Modal -->
    <div class="mo" *ngIf="showForm" (click)="showForm=false"><div class="md md-lg" (click)="$event.stopPropagation()">
      <div class="mh bg-g"><h5><i class="fas fa-box mr-2"></i>{{editing?'Sửa':'Thêm'}} sản phẩm</h5><button class="mx" (click)="showForm=false">&times;</button></div>
      <div class="mb-modal">
        <div class="row">
          <div class="col-8">
            <div class="fg"><label>Tên sản phẩm <span class="req">*</span></label><input class="form-control" [(ngModel)]="fd.name"></div>
            <div class="row"><div class="col-6"><div class="fg"><label>SKU</label><input class="form-control" [(ngModel)]="fd.sku"></div></div><div class="col-6"><div class="fg"><label>Giá bán <span class="req">*</span></label><input type="number" class="form-control" [(ngModel)]="fd.price" min="0"></div></div></div>
            <div class="row"><div class="col-6"><div class="fg"><label>Giá gốc</label><input type="number" class="form-control" [(ngModel)]="fd.originalPrice" min="0"></div></div><div class="col-6"><div class="fg"><label>Tồn kho</label><input type="number" class="form-control" [(ngModel)]="fd.stockQuantity" min="0"></div></div></div>
            <div class="row"><div class="col-6"><div class="fg"><label>Danh mục <span class="req">*</span></label><select class="form-control" [(ngModel)]="fd.category"><option value="">-- Chọn --</option><option *ngFor="let c of cats" [value]="c._id">{{c.name}}</option></select></div></div><div class="col-6"><div class="fg"><label>Thương hiệu</label><select class="form-control" [(ngModel)]="fd.brand"><option value="">-- Chọn --</option><option *ngFor="let b of brands" [value]="b._id">{{b.name}}</option></select></div></div></div>
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

    <!-- Delete Modal -->
    <div class="mo" *ngIf="showDel" (click)="showDel=false"><div class="md" (click)="$event.stopPropagation()">
      <div class="mh bg-r"><h5><i class="fas fa-trash mr-2"></i>Xác nhận xóa</h5><button class="mx" (click)="showDel=false">&times;</button></div>
      <div class="mb-modal"><p>Bạn có chắc muốn xóa <strong>{{delP?.name}}</strong>?</p><p class="muted small">Không thể hoàn tác.</p></div>
      <div class="mf"><button class="btn btn-sec" (click)="showDel=false">Hủy</button><button class="btn btn-danger" (click)="doDelete()"><i class="fas fa-trash mr-1"></i>Xóa</button></div>
    </div></div>

    <!-- Stock Modal -->
    <div class="mo" *ngIf="showStk" (click)="showStk=false"><div class="md md-sm" (click)="$event.stopPropagation()">
      <div class="mh bg-i"><h5><i class="fas fa-boxes mr-2"></i>Cập nhật tồn kho</h5><button class="mx" (click)="showStk=false">&times;</button></div>
      <div class="mb-modal"><div class="fg"><label>Sản phẩm</label><p class="fw">{{stkP?.name}}</p></div><div class="fg"><label>Số lượng mới</label><input type="number" class="form-control" [(ngModel)]="newStk" min="0"></div></div>
      <div class="mf"><button class="btn btn-sec" (click)="showStk=false">Hủy</button><button class="btn btn-info" (click)="doStock()"><i class="fas fa-save mr-1"></i>Lưu</button></div>
    </div></div>
  `,
  styles: [`
    :host{display:block}
    .page-header{display:flex;justify-content:space-between;align-items:flex-start;margin-bottom:16px;flex-wrap:wrap;gap:8px}
    .page-header h4{font-weight:600;color:#1a1a1a;font-size:1.3rem;margin:0}
    .page-header h4 i{margin-right:8px}
    .text-gold{color:#D4AF37!important}
    .breadcrumb{list-style:none;display:flex;gap:8px;padding:0;margin:4px 0 0;font-size:.85rem}
    .breadcrumb a{color:#D4AF37;text-decoration:none}.breadcrumb .active{color:#6c757d}
    .breadcrumb li+li::before{content:"/";margin-right:8px;color:#adb5bd}
    .btn{padding:8px 16px;border:none;border-radius:6px;cursor:pointer;font-size:.85rem;display:inline-flex;align-items:center;gap:4px;transition:all .2s}
    .btn-gold{background:#D4AF37;color:#fff}.btn-gold:hover{background:#B8960C}
    .btn-sec{background:#6c757d;color:#fff}.btn-sec:hover{background:#5a6268}
    .btn-danger{background:#dc3545;color:#fff}
    .btn-info{background:#17a2b8;color:#fff}
    .btn:disabled{opacity:.6;cursor:not-allowed}
    .alert{padding:12px 16px;border-radius:8px;margin-bottom:16px;display:flex;align-items:center;position:relative}
    .alert-success{background:#d4edda;color:#155724;border:1px solid #c3e6cb}
    .alert-x{position:absolute;right:12px;background:none;border:none;font-size:1.2rem;cursor:pointer;color:inherit}
    .card{border:none;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,.08);margin-bottom:16px;background:#fff}
    .card-header{background:#fff;border-bottom:1px solid #f0f0f0;padding:12px 16px;border-radius:12px 12px 0 0!important;display:flex;justify-content:space-between;align-items:center}
    .ch-toggle{cursor:pointer}.card-title{font-weight:600;color:#1a1a1a;font-size:.95rem;margin:0}
    .card-body{padding:16px}.tool-icon{color:#6c757d;font-size:.85rem}
    .filter-card .card-header{border-left:3px solid #D4AF37}
    .row{display:flex;flex-wrap:wrap;margin:0 -8px}
    [class*="col-"]{padding:0 8px;box-sizing:border-box}
    .col-2{flex:0 0 16.666%;max-width:16.666%}.col-3{flex:0 0 25%;max-width:25%}.col-4{flex:0 0 33.333%;max-width:33.333%}.col-6{flex:0 0 50%;max-width:50%}.col-8{flex:0 0 66.666%;max-width:66.666%}
    @media(max-width:991px){[class*="col-"]{flex:0 0 100%;max-width:100%}}
    @media(min-width:992px) and (max-width:1199px){.col-2{flex:0 0 25%;max-width:25%}.col-3{flex:0 0 33.333%;max-width:33.333%}}
    label{display:block;font-size:.85rem;font-weight:500;margin-bottom:4px;color:#333}
    .form-control{width:100%;padding:8px 12px;border:1px solid #dee2e6;border-radius:6px;font-size:.9rem;box-sizing:border-box}
    .form-control:focus{border-color:#D4AF37;outline:none;box-shadow:0 0 0 3px rgba(212,175,55,.15)}
    select.form-control{appearance:auto}textarea.form-control{resize:vertical}
    .fg{margin-bottom:12px}
    .d-flex{display:flex}.gap-8{gap:8px}.mt-1{margin-top:4px}.mt-2{margin-top:8px}.mr-1{margin-right:4px}.mr-2{margin-right:8px}.mb-2{margin-bottom:8px}.mb-3{margin-bottom:16px}.p-0{padding:0!important}
    .badge-g{background:#D4AF37;color:#fff;padding:3px 10px;border-radius:10px;font-size:.75rem;margin-left:8px}
    .table-wrap{overflow-x:auto}
    .adm-table{width:100%;border-collapse:collapse}
    .adm-table th,.adm-table td{padding:10px 16px;border-bottom:1px solid #f0f0f0;font-size:.85rem;vertical-align:middle}
    .adm-table thead th{background:#343a40;color:#fff;font-weight:600;white-space:nowrap}
    .adm-table tbody tr:nth-child(odd){background:rgba(0,0,0,.02)}
    .adm-table tbody tr:hover{background:rgba(212,175,55,.05)}
    .img-t{width:60px;height:60px;object-fit:cover;border-radius:6px;border:1px solid #dee2e6}
    .star-b{background:#ffc107;color:#fff;padding:1px 5px;border-radius:4px;font-size:.65rem;margin-left:4px}
    .cat-b{background:#17a2b8;color:#fff;padding:2px 8px;border-radius:4px;font-size:.75rem}
    .muted{color:#6c757d}.strike{text-decoration:line-through;color:#adb5bd}.text-s{color:#28a745}.text-w{color:#ffc107!important}
    .stk{padding:4px 10px;border-radius:4px;font-size:.8rem;font-weight:600;cursor:pointer;color:#fff}
    .stk.ok{background:#28a745}.stk.med{background:#ffc107;color:#1a1a1a}.stk.low{background:#dc3545}
    .sw{position:relative;display:inline-block;width:40px;height:22px;margin:0}
    .sw input{opacity:0;width:0;height:0}
    .sl{position:absolute;cursor:pointer;top:0;left:0;right:0;bottom:0;background:#ccc;transition:.3s;border-radius:22px}
    .sl:before{position:absolute;content:"";height:16px;width:16px;left:3px;bottom:3px;background:#fff;transition:.3s;border-radius:50%}
    input:checked+.sl{background:#D4AF37}input:checked+.sl:before{transform:translateX(18px)}
    .act-g{display:flex;gap:4px}
    .ab{padding:6px 10px;border:none;border-radius:4px;cursor:pointer;color:#fff;font-size:.8rem}
    .aw{background:#ffc107;color:#1a1a1a}.aw:hover{background:#e0a800}
    .ar{background:#dc3545}.ar:hover{background:#c82333}
    .empty{text-align:center;padding:30px 0;color:#adb5bd}
    .pag{display:flex;justify-content:space-between;align-items:center;padding:12px 16px;border-top:1px solid #f0f0f0}
    .pi{font-size:.85rem;color:#6c757d}
    .pbs{display:flex;gap:4px}
    .pbs button{padding:6px 12px;border:1px solid #dee2e6;background:#fff;border-radius:4px;cursor:pointer;font-size:.85rem}
    .pbs button.active{background:#D4AF37;color:#fff;border-color:#D4AF37}
    .pbs button:disabled{opacity:.5;cursor:not-allowed}
    .pbs button:hover:not(:disabled):not(.active){background:#f8f9fa}
    .mo{position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,.5);z-index:1060;display:flex;align-items:center;justify-content:center;padding:20px}
    .md{background:#fff;border-radius:12px;max-width:500px;width:100%;max-height:90vh;overflow-y:auto;box-shadow:0 10px 40px rgba(0,0,0,.2)}
    .md-lg{max-width:800px}.md-sm{max-width:360px}
    .mh{padding:16px 20px;border-radius:12px 12px 0 0;display:flex;justify-content:space-between;align-items:center;color:#fff}
    .mh h5{margin:0;font-size:1.05rem;display:flex;align-items:center}
    .mx{background:none;border:none;color:#fff;font-size:1.5rem;cursor:pointer;line-height:1}
    .mb-modal{padding:20px}
    .mf{padding:12px 20px;border-top:1px solid #f0f0f0;display:flex;justify-content:flex-end;gap:8px}
    .bg-g{background:#D4AF37}.bg-r{background:#dc3545}.bg-i{background:#17a2b8}
    .img-up{width:100%;aspect-ratio:1;border:2px dashed #dee2e6;border-radius:8px;cursor:pointer;display:flex;align-items:center;justify-content:center;overflow:hidden}
    .img-up:hover{border-color:#D4AF37}
    .pv-img{width:100%;height:100%;object-fit:cover}
    .up-ph{display:flex;flex-direction:column;align-items:center;color:#adb5bd;gap:8px}
    .up-ph i{font-size:2rem}
    .ck-label{display:flex;align-items:center;gap:8px;cursor:pointer;font-size:.9rem}
    .ck-label input{width:16px;height:16px;accent-color:#D4AF37}
    .fw{font-weight:600}.req{color:#dc3545}.small{font-size:.85rem}
  `]
})
export class AdminProductsComponent implements OnInit {
  items: any[] = [];
  cats: any[] = [];
  brands: any[] = [];
  ld = false;
  saving = false;
  msg = '';
  f = { search: '', category: '', brand: '', isActive: '' };
  fOpen = true;
  pg = 1; ps = 10; total = 0; tp = 0; pns: number[] = [];
  showForm = false; editing: any = null; fd: any = {}; selFile: File | null = null; imgPv = '';
  showDel = false; delP: any = null;
  showStk = false; stkP: any = null; newStk = 0;

  constructor(private api: ApiService) {}
  ngOnInit() { this.load(); this.loadCats(); this.loadBrands(); }

  getImg(path: string): string { return this.api.getImageUrl(path) || 'https://via.placeholder.com/60x60?text=No'; }

  load() {
    this.ld = true;
    const p: any = { page: this.pg, limit: this.ps };
    if (this.f.search) p.search = this.f.search;
    if (this.f.category) p.category = this.f.category;
    if (this.f.brand) p.brand = this.f.brand;
    if (this.f.isActive) p.isActive = this.f.isActive;
    this.api.getProducts(p).subscribe({ next: (r: any) => { this.ld = false; if (r.success) { this.items = r.data?.products || r.data || []; this.total = r.data?.pagination?.total || r.data?.total || this.items.length; this.tp = Math.ceil(this.total / this.ps); this.updPg(); } }, error: () => this.ld = false });
  }
  loadCats() { this.api.getCategories({ limit: 100 }).subscribe({ next: (r: any) => this.cats = r.data?.categories || r.data || [], error: () => {} }); }
  loadBrands() { this.api.getBrands({ limit: 100 }).subscribe({ next: (r: any) => this.brands = r.data?.brands || r.data || [], error: () => {} }); }
  resetF() { this.f = { search: '', category: '', brand: '', isActive: '' }; this.pg = 1; this.load(); }
  updPg() { const s = Math.max(1, this.pg - 2), e = Math.min(this.tp, this.pg + 2); this.pns = []; for (let i = s; i <= e; i++) this.pns.push(i); }
  goPg(n: number) { if (n < 1 || n > this.tp) return; this.pg = n; this.load(); }

  openForm(p?: any) {
    this.editing = p || null;
    this.fd = p ? { name: p.name, sku: p.sku || '', price: p.price, originalPrice: p.originalPrice || 0, stockQuantity: p.stockQuantity || 0, category: p.category?._id || p.category || '', brand: p.brand?._id || p.brand || '', description: p.description || '', isActive: p.isActive !== false, isFeatured: p.isFeatured || false }
      : { name: '', sku: '', price: 0, originalPrice: 0, stockQuantity: 0, category: '', brand: '', description: '', isActive: true, isFeatured: false };
    this.selFile = null; this.imgPv = this.getImg(p?.imageUrl || p?.image || '');
    this.showForm = true;
  }
  onFile(e: any) { const f = e.target.files[0]; if (f) { this.selFile = f; const r = new FileReader(); r.onload = (ev: any) => this.imgPv = ev.target.result; r.readAsDataURL(f); } }
  save() {
    if (!this.fd.name || !this.fd.price) return;
    this.saving = true;
    const d = new FormData();
    Object.keys(this.fd).forEach(k => { if (this.fd[k] !== null && this.fd[k] !== undefined) d.append(k, this.fd[k]); });
    if (this.selFile) d.append('image', this.selFile);
    const obs = this.editing ? this.api.updateProduct(this.editing._id, d) : this.api.createProduct(d);
    obs.subscribe({ next: () => { this.saving = false; this.msg = this.editing ? 'Đã cập nhật!' : 'Đã thêm!'; this.showForm = false; this.load(); setTimeout(() => this.msg = '', 3000); }, error: () => this.saving = false });
  }
  confirmDel(p: any) { this.delP = p; this.showDel = true; }
  doDelete() { if (!this.delP) return; this.api.deleteProduct(this.delP._id).subscribe({ next: () => { this.msg = 'Đã xóa!'; this.load(); this.showDel = false; setTimeout(() => this.msg = '', 3000); }, error: () => this.showDel = false }); }
  togAct(p: any) { const d = new FormData(); d.append('isActive', String(!p.isActive)); this.api.updateProduct(p._id, d).subscribe({ next: () => p.isActive = !p.isActive, error: () => {} }); }
  openStock(p: any) { this.stkP = p; this.newStk = p.stockQuantity || 0; this.showStk = true; }
  doStock() { if (!this.stkP) return; const d = new FormData(); d.append('stockQuantity', String(this.newStk)); this.api.updateProduct(this.stkP._id, d).subscribe({ next: () => { this.stkP.stockQuantity = this.newStk; this.showStk = false; this.msg = 'Đã cập nhật tồn kho!'; setTimeout(() => this.msg = '', 3000); }, error: () => this.showStk = false }); }
}
