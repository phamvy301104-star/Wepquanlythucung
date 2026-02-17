import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../services/api.service';

@Component({
  selector: 'app-admin-orders',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page-header">
      <div><h4><i class="fas fa-shopping-cart text-gold"></i> Quản lý đơn hàng</h4>
        <ol class="breadcrumb"><li><a routerLink="/admin">Dashboard</a></li><li class="active">Đơn hàng</li></ol>
      </div>
    </div>
    <div class="alert alert-success" *ngIf="msg"><i class="fas fa-check-circle mr-2"></i>{{msg}}<button class="alert-x" (click)="msg=''">&times;</button></div>

    <div class="tab-row">
      <button *ngFor="let t of tabs" class="tab-btn" [class.active]="activeTab===t.value" (click)="activeTab=t.value;load()">{{t.label}}<span class="tab-count" *ngIf="t.count">{{t.count}}</span></button>
    </div>

    <div class="card">
      <div class="card-header">
        <div class="filter-row">
          <div class="search-box"><input class="form-control" [(ngModel)]="search" placeholder="Mã đơn, tên KH..." (keyup.enter)="load()"><button class="btn btn-gold btn-sm" (click)="load()"><i class="fas fa-search"></i></button></div>
          <input type="date" class="form-control fc-sm" [(ngModel)]="dateFrom" (change)="load()" placeholder="Từ ngày">
          <input type="date" class="form-control fc-sm" [(ngModel)]="dateTo" (change)="load()" placeholder="Đến ngày">
        </div>
      </div>
      <div class="card-body p-0">
        <table class="adm-table">
          <thead><tr><th>#</th><th>Mã đơn</th><th>Khách hàng</th><th>Sản phẩm</th><th>Tổng tiền</th><th>Thanh toán</th><th>Trạng thái</th><th>Ngày đặt</th><th>Thao tác</th></tr></thead>
          <tbody>
            <tr *ngFor="let o of items; let i=index">
              <td>{{i+1}}</td>
              <td><strong class="text-gold">{{o.orderCode||o._id?.substring(0,8)}}</strong></td>
              <td><div><strong>{{o.customer?.fullName||o.customerName||'-'}}</strong><div class="sub-txt">{{o.customer?.phoneNumber||o.customerPhone||''}}</div></div></td>
              <td><span class="bk-b">{{o.items?.length||o.itemCount||0}} SP</span></td>
              <td><strong class="text-gold">{{o.totalAmount|number:'1.0-0'}}đ</strong></td>
              <td><span class="pay-b" [ngClass]="{'pay-paid':o.paymentStatus==='Paid','pay-pending':o.paymentStatus!=='Paid'}">{{o.paymentStatus==='Paid'?'Đã TT':'Chưa TT'}}</span></td>
              <td><span class="os-badge" [ngClass]="'os-'+o.status">{{statusLabel(o.status)}}</span></td>
              <td>{{(o.createdAt||o.orderDate)|date:'dd/MM/yyyy HH:mm'}}</td>
              <td><div class="act-g"><button class="ab ai" (click)="viewDetail(o)" title="Chi tiết"><i class="fas fa-eye"></i></button><button class="ab aw" (click)="openStatusModal(o)" title="Cập nhật" *ngIf="o.status!=='Cancelled'&&o.status!=='Completed'"><i class="fas fa-edit"></i></button><button class="ab" style="background:#dc3545" (click)="openCancelModal(o)" title="Hủy" *ngIf="o.status!=='Cancelled'&&o.status!=='Completed'"><i class="fas fa-ban"></i></button><button class="ab ad" (click)="openDeleteModal(o)" title="Xóa"><i class="fas fa-trash"></i></button></div></td>
            </tr>
            <tr *ngIf="!items.length&&!ld"><td colspan="9" class="empty">Không có đơn hàng nào</td></tr>
            <tr *ngIf="ld"><td colspan="9" class="empty"><i class="fas fa-spinner fa-spin fa-2x text-gold"></i></td></tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Detail Modal -->
    <div class="mo" *ngIf="showDetail" (click)="showDetail=false"><div class="md md-lg" (click)="$event.stopPropagation()">
      <div class="mh bg-g"><h5><i class="fas fa-eye mr-2"></i>Chi tiết đơn hàng #{{detailOrder?.orderCode||detailOrder?._id?.substring(0,8)}}</h5><button class="mx" (click)="showDetail=false">&times;</button></div>
      <div class="mb-modal">
        <div class="detail-grid">
          <div class="dg-item"><label>Khách hàng</label><span>{{detailOrder?.customer?.fullName||detailOrder?.customerName||'-'}}</span></div>
          <div class="dg-item"><label>Số điện thoại</label><span>{{detailOrder?.customer?.phone||detailOrder?.customer?.phoneNumber||detailOrder?.customerPhone||'-'}}</span></div>
          <div class="dg-item"><label>Email</label><span>{{detailOrder?.customer?.email||'-'}}</span></div>
          <div class="dg-item"><label>Ngày đặt</label><span>{{(detailOrder?.createdAt||detailOrder?.orderDate)|date:'dd/MM/yyyy HH:mm'}}</span></div>
          <div class="dg-item"><label>Trạng thái</label><span class="os-badge" [ngClass]="'os-'+detailOrder?.status">{{statusLabel(detailOrder?.status)}}</span></div>
          <div class="dg-item"><label>Thanh toán</label><span class="pay-b" [ngClass]="{'pay-paid':detailOrder?.paymentStatus==='Paid','pay-pending':detailOrder?.paymentStatus!=='Paid'}">{{detailOrder?.paymentMethod||'COD'}} - {{detailOrder?.paymentStatus==='Paid'?'Đã thanh toán':'Chưa thanh toán'}}</span></div>
          <div class="dg-item full"><label>Địa chỉ giao hàng</label><span>{{detailOrder?.shippingAddress?.address ? (detailOrder.shippingAddress.address + ', ' + detailOrder.shippingAddress.ward + ', ' + detailOrder.shippingAddress.district + ', ' + detailOrder.shippingAddress.city) : (detailOrder?.shippingAddress || '-')}}</span></div>
          <div class="dg-item full" *ngIf="detailOrder?.notes"><label>Ghi chú</label><span>{{detailOrder?.notes}}</span></div>
          <div class="dg-item full" *ngIf="detailOrder?.cancelReason"><label>Lý do hủy</label><span class="text-danger">{{detailOrder?.cancelReason}}</span></div>
        </div>
        <h6 class="mt-3"><i class="fas fa-box mr-1"></i> Sản phẩm ({{detailOrder?.items?.length || 0}})</h6>
        <div class="order-items-list">
          <div class="order-item-row" *ngFor="let it of detailOrder?.items; let j=index">
            <div class="oi-index">{{j+1}}</div>
            <div class="oi-img"><img [src]="getImg(it.productImage || it.product?.imageUrl || it.product?.mainImage)" alt=""></div>
            <div class="oi-info">
              <div class="oi-name">{{it.product?.name||it.name||'Sản phẩm'}}</div>
              <div class="oi-meta">Đơn giá: {{(it.unitPrice||it.price)|number:'1.0-0'}}đ &times; {{it.quantity}}</div>
            </div>
            <div class="oi-total"><strong>{{((it.unitPrice||it.price)*it.quantity)|number:'1.0-0'}}đ</strong></div>
          </div>
        </div>
        <div class="order-summary">
          <div class="os-row" *ngIf="detailOrder?.subtotal"><span>Tạm tính:</span><span>{{detailOrder.subtotal|number:'1.0-0'}}đ</span></div>
          <div class="os-row" *ngIf="detailOrder?.shippingFee"><span>Phí vận chuyển:</span><span>{{detailOrder.shippingFee|number:'1.0-0'}}đ</span></div>
          <div class="os-row" *ngIf="detailOrder?.discount"><span>Giảm giá:</span><span class="text-danger">-{{detailOrder.discount|number:'1.0-0'}}đ</span></div>
          <div class="os-row os-total"><span>Tổng cộng:</span><span class="text-gold">{{detailOrder?.totalAmount|number:'1.0-0'}}đ</span></div>
        </div>
      </div>
      <div class="mf">
        <button class="btn btn-danger" (click)="showDetail=false;openDeleteModal(detailOrder)" *ngIf="detailOrder"><i class="fas fa-trash mr-1"></i>Xóa</button>
        <button class="btn btn-sec" (click)="showDetail=false">Đóng</button>
      </div>
    </div></div>

    <!-- Status Update Modal -->
    <div class="mo" *ngIf="showStatus" (click)="showStatus=false"><div class="md" (click)="$event.stopPropagation()">
      <div class="mh bg-g"><h5><i class="fas fa-edit mr-2"></i>Cập nhật trạng thái</h5><button class="mx" (click)="showStatus=false">&times;</button></div>
      <div class="mb-modal">
        <p>Đơn hàng: <strong>{{statusOrder?.orderCode||statusOrder?._id?.substring(0,8)}}</strong></p>
        <div class="fg"><label>Trạng thái mới</label>
          <select class="form-control" [(ngModel)]="newStatus">
            <option value="Pending">Chờ xác nhận</option><option value="Confirmed">Đã xác nhận</option><option value="Processing">Đang xử lý</option><option value="Shipping">Đang giao</option><option value="Completed">Hoàn thành</option>
          </select>
        </div>
        <div class="fg"><label>Ghi chú</label><textarea class="form-control" [(ngModel)]="statusNote" rows="2"></textarea></div>
      </div>
      <div class="mf"><button class="btn btn-sec" (click)="showStatus=false">Hủy</button><button class="btn btn-gold" (click)="updateStatus()"><i class="fas fa-check mr-1"></i>Cập nhật</button></div>
    </div></div>

    <!-- Cancel Modal -->
    <div class="mo" *ngIf="showCancel" (click)="showCancel=false"><div class="md" (click)="$event.stopPropagation()">
      <div class="mh bg-r"><h5><i class="fas fa-ban mr-2"></i>Hủy đơn hàng</h5><button class="mx" (click)="showCancel=false">&times;</button></div>
      <div class="mb-modal">
        <p>Bạn có chắc muốn hủy đơn hàng <strong>{{cancelOrder?.orderCode||cancelOrder?._id?.substring(0,8)}}</strong>?</p>
        <div class="fg"><label>Lý do hủy <span class="req">*</span></label><textarea class="form-control" [(ngModel)]="cancelReason" rows="3" placeholder="Nhập lý do hủy đơn..."></textarea></div>
      </div>
      <div class="mf"><button class="btn btn-sec" (click)="showCancel=false">Đóng</button><button class="btn btn-danger" (click)="doCancel()" [disabled]="!cancelReason"><i class="fas fa-ban mr-1"></i>Xác nhận hủy</button></div>
    </div></div>

    <!-- Delete Confirm Modal -->
    <div class="mo" *ngIf="showDelete" (click)="showDelete=false"><div class="md" (click)="$event.stopPropagation()">
      <div class="mh bg-r"><h5><i class="fas fa-trash mr-2"></i>Xóa đơn hàng</h5><button class="mx" (click)="showDelete=false">&times;</button></div>
      <div class="mb-modal">
        <div class="del-warn"><i class="fas fa-exclamation-triangle"></i></div>
        <p class="text-center">Bạn có chắc chắn muốn xóa đơn hàng <strong class="text-gold">{{deleteOrder?.orderCode||deleteOrder?._id?.substring(0,8)}}</strong>?</p>
        <p class="text-center sub-txt">Khách hàng: <strong>{{deleteOrder?.customer?.fullName||deleteOrder?.customerName||'-'}}</strong> — Tổng tiền: <strong class="text-gold">{{deleteOrder?.totalAmount|number:'1.0-0'}}đ</strong></p>
        <p class="text-center" style="color:#dc3545;font-size:.85rem"><i class="fas fa-info-circle mr-1"></i>Hành động này không thể hoàn tác!</p>
      </div>
      <div class="mf"><button class="btn btn-sec" (click)="showDelete=false">Hủy bỏ</button><button class="btn btn-danger" (click)="doDelete()"><i class="fas fa-trash mr-1"></i>Xác nhận xóa</button></div>
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
    .tab-row{display:flex;gap:4px;margin-bottom:16px;flex-wrap:wrap}
    .tab-btn{padding:8px 16px;border:1px solid #dee2e6;border-radius:8px;background:#fff;cursor:pointer;font-size:.85rem;transition:all .2s;display:inline-flex;align-items:center;gap:6px}
    .tab-btn.active{background:#D4AF37;color:#fff;border-color:#D4AF37}.tab-btn:hover:not(.active){background:#f8f9fa}
    .tab-count{background:rgba(0,0,0,.15);padding:1px 6px;border-radius:10px;font-size:.75rem}
    .card{border:none;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,.08);margin-bottom:16px;background:#fff}
    .card-header{background:#fff;border-bottom:1px solid #f0f0f0;padding:12px 16px;border-radius:12px 12px 0 0!important}.card-body{padding:16px}
    .filter-row{display:flex;gap:12px;align-items:center;flex-wrap:wrap}.search-box{display:flex;gap:8px}.search-box .form-control{width:250px}.fc-sm{width:150px}
    label{display:block;font-size:.85rem;font-weight:500;margin-bottom:4px;color:#333}
    .form-control{width:100%;padding:8px 12px;border:1px solid #dee2e6;border-radius:6px;font-size:.9rem;box-sizing:border-box}.form-control:focus{border-color:#D4AF37;outline:none;box-shadow:0 0 0 3px rgba(212,175,55,.15)}
    select.form-control{appearance:auto}textarea.form-control{resize:vertical}.fg{margin-bottom:12px}
    .p-0{padding:0!important}.mr-1{margin-right:4px}.mr-2{margin-right:8px}.mt-3{margin-top:16px}.text-right{text-align:right}
    .adm-table{width:100%;border-collapse:collapse}.adm-table th,.adm-table td{padding:10px 16px;border-bottom:1px solid #f0f0f0;font-size:.85rem;vertical-align:middle}
    .adm-table thead th{background:#343a40;color:#fff;font-weight:600;white-space:nowrap}.adm-table tbody tr:nth-child(odd){background:rgba(0,0,0,.02)}.adm-table tbody tr:hover{background:rgba(212,175,55,.05)}
    .adm-table tfoot td{background:#f8f9fa;border-top:2px solid #dee2e6}
    .sm-t th,.sm-t td{padding:8px 12px;font-size:.8rem}
    .sub-txt{font-size:.75rem;color:#6c757d}
    .bk-b{background:#6c757d;color:#fff;padding:2px 8px;border-radius:4px;font-size:.75rem}
    .pay-b{padding:3px 8px;border-radius:4px;font-size:.75rem;font-weight:600}.pay-paid{background:#d4edda;color:#155724}.pay-pending{background:#fff3cd;color:#856404}
    .os-badge{padding:4px 10px;border-radius:20px;font-size:.75rem;font-weight:600;white-space:nowrap}
    .os-pending{background:#fff3cd;color:#856404}.os-confirmed{background:#cce5ff;color:#004085}.os-processing{background:#d1ecf1;color:#0c5460}
    .os-shipping{background:#e2d5f1;color:#6f42c1}.os-completed{background:#d4edda;color:#155724}.os-cancelled{background:#f8d7da;color:#721c24}
    .act-g{display:flex;gap:4px}.ab{padding:6px 10px;border:none;border-radius:4px;cursor:pointer;color:#fff;font-size:.8rem}.ai{background:#17a2b8}.aw{background:#ffc107;color:#1a1a1a}
    .empty{text-align:center;padding:30px 0;color:#adb5bd}
    .mo{position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,.5);z-index:1060;display:flex;align-items:center;justify-content:center;padding:20px}
    .md{background:#fff;border-radius:12px;max-width:500px;width:100%;max-height:90vh;overflow-y:auto;box-shadow:0 10px 40px rgba(0,0,0,.2)}.md-lg{max-width:750px}
    .mh{padding:16px 20px;border-radius:12px 12px 0 0;display:flex;justify-content:space-between;align-items:center;color:#fff}.mh h5{margin:0;font-size:1.05rem}.mx{background:none;border:none;color:#fff;font-size:1.5rem;cursor:pointer}
    .mb-modal{padding:20px}.mf{padding:12px 20px;border-top:1px solid #f0f0f0;display:flex;justify-content:flex-end;gap:8px}
    .bg-g{background:#D4AF37}.bg-r{background:#dc3545}
    .detail-grid{display:grid;grid-template-columns:1fr 1fr;gap:12px}.dg-item{display:flex;flex-direction:column}.dg-item label{font-size:.75rem;color:#6c757d;margin-bottom:2px}.dg-item span{font-size:.9rem}.dg-item.full{grid-column:1/-1}
    h6{font-weight:600;color:#1a1a1a;margin:0 0 8px}
    .req{color:#dc3545}
    .text-danger{color:#dc3545}
    .text-center{text-align:center}
    .ad{background:#dc3545}
    .order-items-list{display:flex;flex-direction:column;gap:8px;margin-bottom:16px}
    .order-item-row{display:flex;align-items:center;gap:12px;padding:10px 12px;background:#f8f9fa;border-radius:8px;border:1px solid #eee}
    .oi-index{width:24px;height:24px;background:#D4AF37;color:#fff;border-radius:50%;display:flex;align-items:center;justify-content:center;font-size:.75rem;font-weight:700;flex-shrink:0}
    .oi-img{width:56px;height:56px;border-radius:8px;overflow:hidden;flex-shrink:0;background:#e9ecef}.oi-img img{width:100%;height:100%;object-fit:cover}
    .oi-info{flex:1;min-width:0}.oi-name{font-weight:600;font-size:.9rem;color:#1a1a1a;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}.oi-meta{font-size:.78rem;color:#6c757d;margin-top:2px}
    .oi-total{font-size:.9rem;color:#D4AF37;white-space:nowrap}
    .order-summary{background:#f8f9fa;border-radius:8px;padding:12px 16px;border:1px solid #eee}.os-row{display:flex;justify-content:space-between;padding:4px 0;font-size:.85rem}.os-total{border-top:2px solid #D4AF37;margin-top:8px;padding-top:8px;font-size:1rem;font-weight:700}
    .del-warn{text-align:center;font-size:3rem;color:#ffc107;margin-bottom:12px}
  `]
})
export class AdminOrdersComponent implements OnInit {
  items: any[] = [];
  ld = false; msg = ''; search = ''; dateFrom = ''; dateTo = '';
  activeTab = '';
  tabs = [
    { label: 'Tất cả', value: '', count: 0 },
    { label: 'Chờ xác nhận', value: 'Pending', count: 0 },
    { label: 'Đã xác nhận', value: 'Confirmed', count: 0 },
    { label: 'Đang xử lý', value: 'Processing', count: 0 },
    { label: 'Đang giao', value: 'Shipping', count: 0 },
    { label: 'Hoàn thành', value: 'Completed', count: 0 },
    { label: 'Đã hủy', value: 'Cancelled', count: 0 }
  ];
  showDetail = false; detailOrder: any = null;
  showStatus = false; statusOrder: any = null; newStatus = ''; statusNote = '';
  showCancel = false; cancelOrder: any = null; cancelReason = '';
  showDelete = false; deleteOrder: any = null;

  constructor(private api: ApiService) {}
  ngOnInit() { this.load(); }

  load() {
    this.ld = true;
    const p: any = { limit: 100 };
    if (this.search) p.search = this.search;
    if (this.activeTab) p.status = this.activeTab;
    if (this.dateFrom) p.dateFrom = this.dateFrom;
    if (this.dateTo) p.dateTo = this.dateTo;
    this.api.getOrders(p).subscribe({
      next: (r: any) => {
        this.ld = false;
        const all = r.data?.orders || r.data || [];
        this.items = all;
        // count tabs from unfiltered if possible
        this.api.getOrders({ limit: 500 }).subscribe({
          next: (r2: any) => {
            const a2 = r2.data?.orders || r2.data || [];
            this.tabs[0].count = a2.length;
            ['Pending','Confirmed','Processing','Shipping','Completed','Cancelled'].forEach((s, i) => {
              this.tabs[i + 1].count = a2.filter((o: any) => o.status === s).length;
            });
          }, error: () => {}
        });
      },
      error: () => this.ld = false
    });
  }

  statusLabel(s: string) {
    const m: any = { Pending: 'Chờ xác nhận', Confirmed: 'Đã xác nhận', Processing: 'Đang xử lý', Shipping: 'Đang giao', Completed: 'Hoàn thành', Cancelled: 'Đã hủy' };
    return m[s] || s || '-';
  }

  viewDetail(o: any) {
    this.detailOrder = o;
    // fetch full detail if needed
    if (o._id) {
      this.api.getOrder(o._id).subscribe({
        next: (r: any) => { this.detailOrder = r.data?.order || r.data || o; this.showDetail = true; },
        error: () => this.showDetail = true
      });
    } else { this.showDetail = true; }
  }

  openStatusModal(o: any) { this.statusOrder = o; this.newStatus = o.status; this.statusNote = ''; this.showStatus = true; }

  updateStatus() {
    if (!this.statusOrder || !this.newStatus) return;
    this.api.updateOrderStatus(this.statusOrder._id, { status: this.newStatus, note: this.statusNote }).subscribe({
      next: () => { this.msg = 'Đã cập nhật trạng thái!'; this.showStatus = false; this.load(); setTimeout(() => this.msg = '', 3000); },
      error: () => {}
    });
  }

  openCancelModal(o: any) { this.cancelOrder = o; this.cancelReason = ''; this.showCancel = true; }

  doCancel() {
    if (!this.cancelOrder || !this.cancelReason) return;
    this.api.cancelOrder(this.cancelOrder._id, { cancelReason: this.cancelReason }).subscribe({
      next: () => { this.msg = 'Đã hủy đơn hàng!'; this.showCancel = false; this.load(); setTimeout(() => this.msg = '', 3000); },
      error: () => {}
    });
  }

  getImg(path: string): string { return this.api.getImageUrl(path); }

  openDeleteModal(o: any) { this.deleteOrder = o; this.showDelete = true; }

  doDelete() {
    if (!this.deleteOrder) return;
    this.api.deleteOrder(this.deleteOrder._id).subscribe({
      next: () => { this.msg = 'Đã xóa đơn hàng thành công!'; this.showDelete = false; this.load(); setTimeout(() => this.msg = '', 3000); },
      error: (err: any) => { this.msg = err.error?.message || 'Lỗi khi xóa đơn hàng'; this.showDelete = false; setTimeout(() => this.msg = '', 3000); }
    });
  }
}
