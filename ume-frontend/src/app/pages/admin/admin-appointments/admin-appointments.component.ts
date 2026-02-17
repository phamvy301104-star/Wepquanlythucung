import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../services/api.service';

@Component({
  selector: 'app-admin-appointments',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page-header">
      <div><h4><i class="fas fa-calendar-check text-gold"></i> Quản lý lịch hẹn</h4>
        <ol class="breadcrumb"><li><a routerLink="/admin">Dashboard</a></li><li class="active">Lịch hẹn</li></ol>
      </div>
      <button class="btn btn-gold" (click)="openForm()"><i class="fas fa-plus mr-1"></i>Tạo lịch hẹn</button>
    </div>
    <div class="alert alert-success" *ngIf="msg"><i class="fas fa-check-circle mr-2"></i>{{msg}}<button class="alert-x" (click)="msg=''">&times;</button></div>

    <div class="stats-row">
      <div class="stat-box"><div class="sb-icon bg-info"><i class="fas fa-calendar"></i></div><div><div class="sb-num">{{stats.total}}</div><div class="sb-label">Tổng lịch hẹn</div></div></div>
      <div class="stat-box"><div class="sb-icon bg-warning"><i class="fas fa-clock"></i></div><div><div class="sb-num">{{stats.pending}}</div><div class="sb-label">Chờ xác nhận</div></div></div>
      <div class="stat-box"><div class="sb-icon bg-primary"><i class="fas fa-check"></i></div><div><div class="sb-num">{{stats.confirmed}}</div><div class="sb-label">Đã xác nhận</div></div></div>
      <div class="stat-box"><div class="sb-icon bg-success"><i class="fas fa-check-double"></i></div><div><div class="sb-num">{{stats.completed}}</div><div class="sb-label">Hoàn thành</div></div></div>
      <div class="stat-box"><div class="sb-icon bg-danger"><i class="fas fa-times"></i></div><div><div class="sb-num">{{stats.cancelled}}</div><div class="sb-label">Đã hủy</div></div></div>
      <div class="stat-box"><div class="sb-icon bg-gold"><i class="fas fa-money-bill"></i></div><div><div class="sb-num">{{stats.revenue|number:'1.0-0'}}đ</div><div class="sb-label">Doanh thu</div></div></div>
    </div>

    <div class="card">
      <div class="card-header">
        <div class="filter-row">
          <div class="search-box"><input class="form-control" [(ngModel)]="search" placeholder="Mã, tên KH, SĐT..." (keyup.enter)="load()"><button class="btn btn-gold btn-sm" (click)="load()"><i class="fas fa-search"></i></button></div>
          <select class="form-control fc-sm" [(ngModel)]="statusFilter" (change)="load()"><option value="">Tất cả</option><option value="Pending">Chờ xác nhận</option><option value="Confirmed">Đã xác nhận</option><option value="InProgress">Đang thực hiện</option><option value="Completed">Hoàn thành</option><option value="Cancelled">Đã hủy</option></select>
          <input type="date" class="form-control fc-sm" [(ngModel)]="dateFilter" (change)="load()">
        </div>
      </div>
      <div class="card-body p-0">
        <table class="adm-table">
          <thead><tr><th>#</th><th>Mã</th><th>Khách hàng</th><th>Nhân viên</th><th>Dịch vụ</th><th>Ngày hẹn</th><th>Giờ</th><th>Tổng tiền</th><th>Trạng thái</th><th>Thao tác</th></tr></thead>
          <tbody>
            <tr *ngFor="let a of items; let i=index" class="clickable-row" (click)="viewDetail(a)">
              <td>{{i+1}}</td>
              <td><strong class="text-gold">{{a.appointmentCode||a._id?.substring(0,8)}}</strong></td>
              <td><div><strong>{{a.customer?.fullName||a.customerName||'-'}}</strong><div class="sub-txt">{{a.customer?.phoneNumber||a.customerPhone||''}}</div></div></td>
              <td>{{a.staff?.fullName||a.staffName||'-'}}</td>
              <td><div class="svc-tags"><span class="svc-tag" *ngFor="let sv of (a.services||[]).slice(0,2)">{{sv.service?.name||sv.name||sv}}</span><span class="svc-tag more" *ngIf="(a.services||[]).length>2">+{{a.services.length-2}}</span></div></td>
              <td>{{(a.appointmentDate||a.date)|date:'dd/MM/yyyy'}}</td>
              <td>{{a.startTime||a.time||'-'}}</td>
              <td><strong class="text-gold">{{a.totalAmount|number:'1.0-0'}}đ</strong></td>
              <td><span class="os-badge" [ngClass]="'os-'+(a.status==='InProgress'?'processing':a.status?.toLowerCase())">{{apptLabel(a.status)}}</span></td>
              <td (click)="$event.stopPropagation()"><div class="act-g">
                <button class="ab ai" (click)="viewDetail(a)" title="Chi tiết"><i class="fas fa-eye"></i></button>
                <button class="ab" style="background:#6c757d" (click)="openDeleteModal(a)" title="Xóa"><i class="fas fa-trash"></i></button>
              </div></td>
            </tr>
            <tr *ngIf="!items.length&&!ld"><td colspan="10" class="empty">Không có lịch hẹn nào</td></tr>
            <tr *ngIf="ld"><td colspan="10" class="empty"><i class="fas fa-spinner fa-spin fa-2x text-gold"></i></td></tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Detail Modal -->
    <div class="mo" *ngIf="showDetail" (click)="showDetail=false"><div class="md md-lg" (click)="$event.stopPropagation()">
      <div class="mh bg-g"><h5><i class="fas fa-calendar-check mr-2"></i>Chi tiết lịch hẹn</h5><button class="mx" (click)="showDetail=false">&times;</button></div>
      <div class="mb-modal">
        <div class="detail-grid">
          <div class="dg-item"><label>Mã lịch hẹn</label><span class="text-gold fw-b">{{det?.appointmentCode||det?._id?.substring(0,8)}}</span></div>
          <div class="dg-item"><label>Trạng thái</label><span class="os-badge" [ngClass]="'os-'+(det?.status==='InProgress'?'processing':det?.status?.toLowerCase())">{{apptLabel(det?.status)}}</span></div>
          <div class="dg-item"><label>Khách hàng</label><span>{{det?.customer?.fullName||det?.customerName||'-'}}</span></div>
          <div class="dg-item"><label>Số điện thoại</label><span>{{det?.customer?.phoneNumber||det?.customerPhone||'-'}}</span></div>
          <div class="dg-item"><label>Nhân viên</label><span>{{det?.staff?.fullName||det?.staffName||'-'}}</span></div>
          <div class="dg-item"><label>Ngày hẹn</label><span>{{(det?.appointmentDate||det?.date)|date:'dd/MM/yyyy'}} - {{det?.startTime||det?.time||'-'}}</span></div>
          <div class="dg-item full"><label>Ghi chú</label><span>{{det?.notes||det?.note||'Không có'}}</span></div>
        </div>
        <div class="status-flow">
          <div class="sf-step" [class.active]="det?.status==='Pending'" [class.done]="['Confirmed','InProgress','Completed'].includes(det?.status)" [style.background]="det?.status==='Pending'?'#ffc107':'transparent'"><i class="fas fa-clock"></i> Chờ xác nhận</div>
          <i class="fas fa-chevron-right sf-arrow"></i>
          <div class="sf-step" [class.active]="det?.status==='Confirmed'" [class.done]="['InProgress','Completed'].includes(det?.status)" [style.background]="det?.status==='Confirmed'?'#007bff':'transparent'" [style.color]="det?.status==='Confirmed'?'#fff':''"><i class="fas fa-check"></i> Đã xác nhận</div>
          <i class="fas fa-chevron-right sf-arrow"></i>
          <div class="sf-step" [class.active]="det?.status==='InProgress'" [class.done]="det?.status==='Completed'" [style.background]="det?.status==='InProgress'?'#17a2b8':'transparent'" [style.color]="det?.status==='InProgress'?'#fff':''"><i class="fas fa-play"></i> Đang thực hiện</div>
          <i class="fas fa-chevron-right sf-arrow"></i>
          <div class="sf-step" [class.active]="det?.status==='Completed'" [style.background]="det?.status==='Completed'?'#28a745':'transparent'" [style.color]="det?.status==='Completed'?'#fff':''"><i class="fas fa-check-double"></i> Hoàn thành</div>
          <div class="sf-step" *ngIf="det?.status==='Cancelled'" style="background:#dc3545;color:#fff;margin-left:auto"><i class="fas fa-ban"></i> Đã hủy</div>
        </div>
        <h6 class="mt-3">Dịch vụ</h6>
        <table class="adm-table sm-t">
          <thead><tr><th>#</th><th>Dịch vụ</th><th>Thời gian</th><th>Giá</th></tr></thead>
          <tbody><tr *ngFor="let sv of det?.services;let j=index"><td>{{j+1}}</td><td>{{sv.service?.name||sv.name||sv}}</td><td>{{sv.durationMinutes||sv.service?.durationMinutes||'-'}} phút</td><td>{{(sv.price||sv.service?.price||0)|number:'1.0-0'}}đ</td></tr></tbody>
          <tfoot><tr><td colspan="3" class="text-right"><strong>Tổng cộng:</strong></td><td><strong class="text-gold">{{det?.totalAmount|number:'1.0-0'}}đ</strong></td></tr></tfoot>
        </table>
      </div>
      <div class="mf">
        <button class="btn btn-sec" (click)="showDetail=false">Đóng</button>
        <div class="mf-spacer"></div>
        <button class="btn btn-danger" (click)="detailAction('cancel')" *ngIf="det?.status!=='Cancelled'&&det?.status!=='Completed'"><i class="fas fa-ban mr-1"></i>Hủy</button>
        <button class="btn btn-confirm" (click)="detailAction('confirm')" *ngIf="det?.status==='Pending'"><i class="fas fa-check mr-1"></i>Xác nhận</button>
        <button class="btn btn-start" (click)="detailAction('start')" *ngIf="det?.status==='Confirmed'"><i class="fas fa-play mr-1"></i>Bắt đầu thực hiện</button>
        <button class="btn btn-complete" (click)="detailAction('complete')" *ngIf="det?.status==='InProgress'"><i class="fas fa-check-double mr-1"></i>Hoàn thành</button>
      </div>
    </div></div>

    <!-- Create/Edit Form Modal -->
    <div class="mo" *ngIf="showForm" (click)="showForm=false"><div class="md md-lg" (click)="$event.stopPropagation()">
      <div class="mh bg-g"><h5><i class="fas fa-calendar-plus mr-2"></i>{{editing?'Sửa':'Tạo'}} lịch hẹn</h5><button class="mx" (click)="showForm=false">&times;</button></div>
      <div class="mb-modal">
        <div class="row">
          <div class="col-6"><div class="fg"><label>Tên khách hàng <span class="req">*</span></label><input class="form-control" [(ngModel)]="fd.customerName"></div></div>
          <div class="col-6"><div class="fg"><label>Số điện thoại <span class="req">*</span></label><input class="form-control" [(ngModel)]="fd.customerPhone"></div></div>
        </div>
        <div class="row">
          <div class="col-6"><div class="fg"><label>Ngày hẹn <span class="req">*</span></label><input type="date" class="form-control" [(ngModel)]="fd.appointmentDate"></div></div>
          <div class="col-6"><div class="fg"><label>Giờ hẹn <span class="req">*</span></label><input type="time" class="form-control" [(ngModel)]="fd.startTime"></div></div>
        </div>
        <div class="fg"><label>Nhân viên</label><select class="form-control" [(ngModel)]="fd.staff"><option value="">-- Chọn nhân viên --</option><option *ngFor="let s of staffList" [value]="s._id">{{s.fullName||s.name}}</option></select></div>
        <div class="fg"><label>Dịch vụ <span class="req">*</span></label>
          <div class="svc-pick">
            <div class="svc-pick-item" *ngFor="let sv of allServices" (click)="togFormSvc(sv)" [class.selected]="isSvcSel(sv._id)">
              <div class="spi-info"><strong>{{sv.name}}</strong><span>{{sv.durationMinutes||30}} phút</span></div>
              <div class="spi-price">{{sv.price|number:'1.0-0'}}đ</div>
              <div class="spi-check"><i class="fas" [ngClass]="isSvcSel(sv._id)?'fa-check-circle':'fa-circle'"></i></div>
            </div>
          </div>
        </div>
        <div class="summary-box" *ngIf="fd.selectedServices?.length">
          <div class="sum-row"><span>Dịch vụ đã chọn:</span><strong>{{fd.selectedServices.length}}</strong></div>
          <div class="sum-row"><span>Tổng thời gian:</span><strong>{{calcTotalDuration()}} phút</strong></div>
          <div class="sum-row"><span>Tổng tiền:</span><strong class="text-gold">{{calcTotalPrice()|number:'1.0-0'}}đ</strong></div>
        </div>
        <div class="fg"><label>Ghi chú</label><textarea class="form-control" [(ngModel)]="fd.notes" rows="2"></textarea></div>
      </div>
      <div class="mf"><button class="btn btn-sec" (click)="showForm=false">Hủy</button><button class="btn btn-gold" (click)="save()" [disabled]="saving"><i class="fas fa-save mr-1"></i>{{saving?'Đang lưu...':'Lưu'}}</button></div>
    </div></div>

    <!-- Cancel Modal -->
    <div class="mo" *ngIf="showCancel" (click)="showCancel=false"><div class="md" (click)="$event.stopPropagation()">
      <div class="mh bg-r"><h5><i class="fas fa-ban mr-2"></i>Hủy lịch hẹn</h5><button class="mx" (click)="showCancel=false">&times;</button></div>
      <div class="mb-modal">
        <p>Bạn có chắc muốn hủy lịch hẹn <strong>{{cancelItem?.appointmentCode||cancelItem?._id?.substring(0,8)}}</strong>?</p>
        <div class="fg"><label>Lý do hủy <span class="req">*</span></label><textarea class="form-control" [(ngModel)]="cancelReason" rows="3" placeholder="Nhập lý do..."></textarea></div>
      </div>
      <div class="mf"><button class="btn btn-sec" (click)="showCancel=false">Đóng</button><button class="btn btn-danger" (click)="doCancel()" [disabled]="!cancelReason"><i class="fas fa-ban mr-1"></i>Xác nhận hủy</button></div>
    </div></div>

    <!-- Delete Modal -->
    <div class="mo" *ngIf="showDelete" (click)="showDelete=false"><div class="md" (click)="$event.stopPropagation()">
      <div class="mh bg-r"><h5><i class="fas fa-trash mr-2"></i>Xóa lịch hẹn</h5><button class="mx" (click)="showDelete=false">&times;</button></div>
      <div class="mb-modal"><p>Bạn có chắc muốn xóa lịch hẹn <strong>{{deleteItem?.appointmentCode||deleteItem?._id?.substring(0,8)}}</strong>?</p><p class="text-danger">Hành động này không thể hoàn tác!</p></div>
      <div class="mf"><button class="btn btn-sec" (click)="showDelete=false">Hủy</button><button class="btn btn-danger" (click)="doDelete()"><i class="fas fa-trash mr-1"></i>Xác nhận xóa</button></div>
    </div></div>

    <!-- Confirm Modal -->
    <div class="mo" *ngIf="showConfirm" (click)="showConfirm=false"><div class="md" (click)="$event.stopPropagation()">
      <div class="mh" style="background:#28a745"><h5><i class="fas fa-check mr-2"></i>Xác nhận lịch hẹn</h5><button class="mx" (click)="showConfirm=false">&times;</button></div>
      <div class="mb-modal"><p>Xác nhận lịch hẹn <strong>{{confirmItem?.appointmentCode||confirmItem?._id?.substring(0,8)}}</strong> của khách <strong>{{confirmItem?.customer?.fullName||confirmItem?.customerName}}</strong>?</p></div>
      <div class="mf"><button class="btn btn-sec" (click)="showConfirm=false">Hủy</button><button class="btn" style="background:#28a745;color:#fff" (click)="doConfirm()"><i class="fas fa-check mr-1"></i>Xác nhận</button></div>
    </div></div>
  `,
  styles: [`
    :host{display:block}
    .page-header{display:flex;justify-content:space-between;align-items:flex-start;margin-bottom:16px;flex-wrap:wrap;gap:8px}
    .page-header h4{font-weight:600;color:#1a1a1a;font-size:1.3rem;margin:0}.page-header h4 i{margin-right:8px}
    .text-gold{color:#D4AF37!important}.fw-b{font-weight:700}
    .breadcrumb{list-style:none;display:flex;gap:8px;padding:0;margin:4px 0 0;font-size:.85rem}.breadcrumb a{color:#D4AF37;text-decoration:none}.breadcrumb .active{color:#6c757d}.breadcrumb li+li::before{content:"/";margin-right:8px;color:#adb5bd}
    .btn{padding:8px 16px;border:none;border-radius:6px;cursor:pointer;font-size:.85rem;display:inline-flex;align-items:center;gap:4px;transition:all .2s}
    .btn-gold{background:#D4AF37;color:#fff}.btn-gold:hover{background:#B8960C}.btn-sec{background:#6c757d;color:#fff}.btn-danger{background:#dc3545;color:#fff}.btn-sm{padding:6px 12px}.btn:disabled{opacity:.6}
    .alert{padding:12px 16px;border-radius:8px;margin-bottom:16px;display:flex;align-items:center;position:relative}.alert-success{background:#d4edda;color:#155724;border:1px solid #c3e6cb}.alert-x{position:absolute;right:12px;background:none;border:none;font-size:1.2rem;cursor:pointer;color:inherit}
    .stats-row{display:grid;grid-template-columns:repeat(6,1fr);gap:12px;margin-bottom:16px}
    .stat-box{display:flex;align-items:center;gap:10px;background:#fff;padding:14px;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,.08)}
    .sb-icon{width:44px;height:44px;border-radius:10px;display:flex;align-items:center;justify-content:center;font-size:1.1rem;color:#fff}
    .bg-info{background:#17a2b8}.bg-success{background:#28a745}.bg-warning{background:#ffc107;color:#1a1a1a!important}.bg-gold{background:#D4AF37}.bg-primary{background:#007bff}.bg-danger{background:#dc3545}
    .sb-num{font-size:1.2rem;font-weight:700;color:#1a1a1a}.sb-label{font-size:.7rem;color:#6c757d}
    @media(max-width:1200px){.stats-row{grid-template-columns:repeat(3,1fr)}}@media(max-width:768px){.stats-row{grid-template-columns:repeat(2,1fr)}}
    .card{border:none;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,.08);margin-bottom:16px;background:#fff}
    .card-header{background:#fff;border-bottom:1px solid #f0f0f0;padding:12px 16px;border-radius:12px 12px 0 0!important}.card-body{padding:16px}
    .filter-row{display:flex;gap:12px;align-items:center;flex-wrap:wrap}.search-box{display:flex;gap:8px}.search-box .form-control{width:250px}.fc-sm{width:160px}
    .row{display:flex;flex-wrap:wrap;margin:0 -8px}[class*="col-"]{padding:0 8px;box-sizing:border-box}.col-6{flex:0 0 50%;max-width:50%}
    @media(max-width:768px){[class*="col-"]{flex:0 0 100%;max-width:100%}}
    label{display:block;font-size:.85rem;font-weight:500;margin-bottom:4px;color:#333}
    .form-control{width:100%;padding:8px 12px;border:1px solid #dee2e6;border-radius:6px;font-size:.9rem;box-sizing:border-box}.form-control:focus{border-color:#D4AF37;outline:none;box-shadow:0 0 0 3px rgba(212,175,55,.15)}
    select.form-control{appearance:auto}textarea.form-control{resize:vertical}.fg{margin-bottom:12px}
    .p-0{padding:0!important}.mr-1{margin-right:4px}.mr-2{margin-right:8px}.mt-3{margin-top:16px}.text-right{text-align:right}
    .adm-table{width:100%;border-collapse:collapse}.adm-table th,.adm-table td{padding:10px 14px;border-bottom:1px solid #f0f0f0;font-size:.83rem;vertical-align:middle}
    .adm-table thead th{background:#343a40;color:#fff;font-weight:600;white-space:nowrap}.adm-table tbody tr:nth-child(odd){background:rgba(0,0,0,.02)}.adm-table tbody tr:hover{background:rgba(212,175,55,.05)}
    .adm-table tfoot td{background:#f8f9fa;border-top:2px solid #dee2e6}
    .sm-t th,.sm-t td{padding:8px 12px;font-size:.8rem}
    .sub-txt{font-size:.75rem;color:#6c757d}
    .svc-tags{display:flex;gap:4px;flex-wrap:wrap}.svc-tag{background:#e9ecef;padding:2px 8px;border-radius:4px;font-size:.73rem}.svc-tag.more{background:#D4AF37;color:#fff}
    .os-badge{padding:4px 10px;border-radius:20px;font-size:.73rem;font-weight:600;white-space:nowrap}
    .os-pending{background:#fff3cd;color:#856404}.os-confirmed{background:#cce5ff;color:#004085}.os-processing,.os-inprogress{background:#d1ecf1;color:#0c5460}
    .os-completed{background:#d4edda;color:#155724}.os-cancelled{background:#f8d7da;color:#721c24}
    .act-g{display:flex;gap:3px}.ab{padding:5px 8px;border:none;border-radius:4px;cursor:pointer;color:#fff;font-size:.75rem}.ai{background:#17a2b8}.aw{background:#ffc107;color:#1a1a1a}
    .empty{text-align:center;padding:30px 0;color:#adb5bd}
    .mo{position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,.5);z-index:1060;display:flex;align-items:center;justify-content:center;padding:20px}
    .md{background:#fff;border-radius:12px;max-width:500px;width:100%;max-height:90vh;overflow-y:auto;box-shadow:0 10px 40px rgba(0,0,0,.2)}.md-lg{max-width:750px}
    .mh{padding:16px 20px;border-radius:12px 12px 0 0;display:flex;justify-content:space-between;align-items:center;color:#fff}.mh h5{margin:0;font-size:1.05rem}.mx{background:none;border:none;color:#fff;font-size:1.5rem;cursor:pointer}
    .mb-modal{padding:20px}.mf{padding:12px 20px;border-top:1px solid #f0f0f0;display:flex;justify-content:flex-end;gap:8px}
    .bg-g{background:#D4AF37}.bg-r{background:#dc3545}
    .detail-grid{display:grid;grid-template-columns:1fr 1fr;gap:12px}.dg-item{display:flex;flex-direction:column}.dg-item label{font-size:.75rem;color:#6c757d;margin-bottom:2px}.dg-item span{font-size:.9rem}.dg-item.full{grid-column:1/-1}
    h6{font-weight:600;color:#1a1a1a;margin:0 0 8px}
    .svc-pick{max-height:250px;overflow-y:auto;border:1px solid #dee2e6;border-radius:8px}
    .svc-pick-item{display:flex;align-items:center;padding:10px 14px;border-bottom:1px solid #f0f0f0;cursor:pointer;transition:background .15s}.svc-pick-item:hover{background:#f8f9fa}.svc-pick-item.selected{background:rgba(212,175,55,.08);border-left:3px solid #D4AF37}
    .spi-info{flex:1}.spi-info strong{display:block;font-size:.85rem}.spi-info span{font-size:.75rem;color:#6c757d}
    .spi-price{font-weight:600;color:#D4AF37;margin-right:12px;font-size:.85rem}
    .spi-check{font-size:1.1rem}.spi-check .fa-check-circle{color:#D4AF37}.spi-check .fa-circle{color:#dee2e6}
    .summary-box{background:#f8f9fa;border:1px solid #e9ecef;border-radius:8px;padding:12px 16px;margin-bottom:12px}
    .sum-row{display:flex;justify-content:space-between;padding:4px 0;font-size:.85rem}
    .req{color:#dc3545}
    .text-danger{color:#dc3545;font-size:.85rem}
    .clickable-row{cursor:pointer;transition:background .15s}.clickable-row:hover{background:rgba(212,175,55,.08)!important}
    .mf-spacer{flex:1}
    .btn-confirm{background:#28a745;color:#fff}.btn-confirm:hover{background:#218838}
    .btn-start{background:#17a2b8;color:#fff}.btn-start:hover{background:#138496}
    .btn-complete{background:#28a745;color:#fff}.btn-complete:hover{background:#218838}
    .status-flow{display:flex;align-items:center;gap:8px;margin:16px 0 0;padding:12px 16px;background:#f8f9fa;border-radius:8px;border:1px solid #e9ecef}
    .sf-step{display:flex;align-items:center;gap:6px;font-size:.8rem;color:#adb5bd;padding:4px 10px;border-radius:20px}
    .sf-step.active{color:#fff;font-weight:600}
    .sf-step.done{color:#28a745}.sf-arrow{color:#dee2e6;font-size:.7rem}
  `]
})
export class AdminAppointmentsComponent implements OnInit {
  items: any[] = [];
  allServices: any[] = [];
  staffList: any[] = [];
  ld = false; saving = false; msg = ''; search = ''; statusFilter = ''; dateFilter = '';
  stats = { total: 0, pending: 0, confirmed: 0, completed: 0, cancelled: 0, revenue: 0 };

  showDetail = false; det: any = null;
  showForm = false; editing: any = null; fd: any = {};
  showCancel = false; cancelItem: any = null; cancelReason = '';
  showConfirm = false; confirmItem: any = null;
  showDelete = false; deleteItem: any = null;

  constructor(private api: ApiService) {}
  ngOnInit() { this.load(); this.loadRefs(); }

  load() {
    this.ld = true;
    const p: any = { limit: 200 };
    if (this.search) p.search = this.search;
    if (this.statusFilter) p.status = this.statusFilter;
    if (this.dateFilter) p.date = this.dateFilter;
    this.api.getAppointments(p).subscribe({
      next: (r: any) => {
        this.ld = false;
        this.items = r.data?.appointments || r.data || [];
        this.calcStats();
      },
      error: () => this.ld = false
    });
  }

  calcStats() {
    this.stats.total = this.items.length;
    this.stats.pending = this.items.filter(a => a.status === 'Pending').length;
    this.stats.confirmed = this.items.filter(a => a.status === 'Confirmed').length;
    this.stats.completed = this.items.filter(a => a.status === 'Completed').length;
    this.stats.cancelled = this.items.filter(a => a.status === 'Cancelled').length;
    this.stats.revenue = this.items.filter(a => a.status === 'Completed').reduce((s, a) => s + (a.totalAmount || 0), 0);
  }

  loadRefs() {
    this.api.getServices({ limit: 100 }).subscribe({ next: (r: any) => { this.allServices = r.data?.services || r.data || []; }, error: () => {} });
    this.api.getStaffList({ limit: 100, status: 'active' }).subscribe({ next: (r: any) => { this.staffList = r.data?.staff || r.data || []; }, error: () => {} });
  }

  apptLabel(s: string) {
    const m: any = { Pending: 'Chờ xác nhận', Confirmed: 'Đã xác nhận', InProgress: 'Đang thực hiện', Completed: 'Hoàn thành', Cancelled: 'Đã hủy' };
    return m[s] || s || '-';
  }

  viewDetail(a: any) {
    if (a._id) {
      this.api.getAppointment(a._id).subscribe({
        next: (r: any) => { this.det = r.data?.appointment || r.data || a; this.showDetail = true; },
        error: () => { this.det = a; this.showDetail = true; }
      });
    } else { this.det = a; this.showDetail = true; }
  }

  detailAction(action: string) {
    if (!this.det?._id) return;
    const id = this.det._id;
    if (action === 'confirm') {
      this.api.updateAppointmentStatus(id, { status: 'Confirmed' }).subscribe({
        next: () => { this.msg = 'Đã xác nhận lịch hẹn!'; this.det.status = 'Confirmed'; this.load(); setTimeout(() => this.msg = '', 3000); },
        error: () => {}
      });
    } else if (action === 'start') {
      this.api.updateAppointmentStatus(id, { status: 'InProgress' }).subscribe({
        next: () => { this.msg = 'Đã bắt đầu thực hiện!'; this.det.status = 'InProgress'; this.load(); setTimeout(() => this.msg = '', 3000); },
        error: () => {}
      });
    } else if (action === 'complete') {
      this.api.updateAppointmentStatus(id, { status: 'Completed' }).subscribe({
        next: () => { this.msg = 'Đã hoàn thành lịch hẹn!'; this.det.status = 'Completed'; this.load(); setTimeout(() => this.msg = '', 3000); },
        error: () => {}
      });
    } else if (action === 'cancel') {
      this.showDetail = false;
      this.openCancelModal(this.det);
    }
  }

  openForm(a?: any) {
    this.editing = a || null;
    this.fd = a ? {
      customerName: a.customer?.fullName || a.customerName || '',
      customerPhone: a.customer?.phoneNumber || a.customerPhone || '',
      appointmentDate: a.appointmentDate ? a.appointmentDate.substring(0, 10) : a.date ? new Date(a.date).toISOString().substring(0, 10) : '',
      startTime: a.startTime || a.time || '',
      staff: a.staff?._id || a.staff || '',
      notes: a.notes || a.note || '',
      selectedServices: (a.services || []).map((sv: any) => sv.service?._id || sv._id || sv)
    } : { customerName: '', customerPhone: '', appointmentDate: '', startTime: '', staff: '', notes: '', selectedServices: [] };
    this.showForm = true;
  }

  isSvcSel(id: string) { return this.fd.selectedServices?.includes(id); }
  togFormSvc(sv: any) {
    if (!this.fd.selectedServices) this.fd.selectedServices = [];
    const idx = this.fd.selectedServices.indexOf(sv._id);
    if (idx >= 0) this.fd.selectedServices.splice(idx, 1);
    else this.fd.selectedServices.push(sv._id);
  }

  calcTotalDuration() { return this.fd.selectedServices?.reduce((t: number, id: string) => { const sv = this.allServices.find((s: any) => s._id === id); return t + (sv?.durationMinutes || 30); }, 0) || 0; }
  calcTotalPrice() { return this.fd.selectedServices?.reduce((t: number, id: string) => { const sv = this.allServices.find((s: any) => s._id === id); return t + (sv?.price || 0); }, 0) || 0; }

  save() {
    if (!this.fd.customerName || !this.fd.customerPhone || !this.fd.appointmentDate || !this.fd.startTime || !this.fd.selectedServices?.length) return;
    this.saving = true;
    const body: any = {
      customerName: this.fd.customerName,
      customerPhone: this.fd.customerPhone,
      appointmentDate: this.fd.appointmentDate,
      startTime: this.fd.startTime,
      staff: this.fd.staff || undefined,
      services: this.fd.selectedServices,
      notes: this.fd.notes,
      totalAmount: this.calcTotalPrice()
    };
    const obs = this.editing ? this.api.updateAppointmentStatus(this.editing._id, body) : this.api.createAppointment(body);
    obs.subscribe({
      next: () => { this.saving = false; this.msg = this.editing ? 'Đã cập nhật!' : 'Đã tạo lịch hẹn!'; this.showForm = false; this.load(); setTimeout(() => this.msg = '', 3000); },
      error: () => this.saving = false
    });
  }

  confirmAppt(a: any) { this.confirmItem = a; this.showConfirm = true; }
  doConfirm() {
    if (!this.confirmItem) return;
    this.api.updateAppointmentStatus(this.confirmItem._id, { status: 'Confirmed' }).subscribe({
      next: () => { this.msg = 'Đã xác nhận lịch hẹn!'; this.showConfirm = false; this.load(); setTimeout(() => this.msg = '', 3000); },
      error: () => {}
    });
  }

  startProgress(a: any) {
    this.api.updateAppointmentStatus(a._id, { status: 'InProgress' }).subscribe({
      next: () => { this.msg = 'Đã bắt đầu thực hiện!'; this.load(); setTimeout(() => this.msg = '', 3000); },
      error: () => {}
    });
  }

  completeAppt(a: any) {
    this.api.updateAppointmentStatus(a._id, { status: 'Completed' }).subscribe({
      next: () => { this.msg = 'Đã hoàn thành lịch hẹn!'; this.load(); setTimeout(() => this.msg = '', 3000); },
      error: () => {}
    });
  }

  openDeleteModal(a: any) { this.deleteItem = a; this.showDelete = true; }
  doDelete() {
    if (!this.deleteItem) return;
    this.api.deleteAppointment(this.deleteItem._id).subscribe({
      next: () => { this.msg = 'Đã xóa lịch hẹn!'; this.showDelete = false; this.load(); setTimeout(() => this.msg = '', 3000); },
      error: () => {}
    });
  }

  openCancelModal(a: any) { this.cancelItem = a; this.cancelReason = ''; this.showCancel = true; }
  doCancel() {
    if (!this.cancelItem || !this.cancelReason) return;
    this.api.cancelAppointment(this.cancelItem._id, { cancelReason: this.cancelReason }).subscribe({
      next: () => { this.msg = 'Đã hủy lịch hẹn!'; this.showCancel = false; this.load(); setTimeout(() => this.msg = '', 3000); },
      error: () => {}
    });
  }
}
