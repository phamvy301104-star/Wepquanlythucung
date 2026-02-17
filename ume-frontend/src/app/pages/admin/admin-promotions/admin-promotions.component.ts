import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../services/api.service';

@Component({
  selector: 'app-admin-promotions',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <h2><i class="fas fa-percentage"></i> Quản lý khuyến mãi</h2>
      <button class="btn btn-gold" (click)="showForm = true; resetForm()"><i class="fas fa-plus"></i> Thêm khuyến mãi</button>
    </div>

    <!-- Table -->
    <div class="card">
      <div class="card-body p-0">
        <div class="table-responsive">
          <table class="adm-table">
            <thead>
              <tr>
                <th>Mã</th>
                <th>Tên</th>
                <th>Loại</th>
                <th>Giá trị</th>
                <th>Đã dùng</th>
                <th>Thời gian</th>
                <th>Trạng thái</th>
                <th>Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let promo of promotions">
                <td><strong class="text-gold">{{ promo.code }}</strong></td>
                <td>{{ promo.name }}</td>
                <td>{{ getTypeName(promo.type) }}</td>
                <td>
                  <span *ngIf="promo.type === 'Percentage'">{{ promo.value }}%</span>
                  <span *ngIf="promo.type === 'FixedAmount'">{{ promo.value | number:'1.0-0' }}đ</span>
                  <span *ngIf="promo.type === 'FreeShipping'">Miễn ship</span>
                  <span *ngIf="promo.type === 'BuyXGetY'">Mua X tặng Y</span>
                  <br>
                  <small class="text-muted" *ngIf="promo.minOrderAmount">Tối thiểu: {{ promo.minOrderAmount | number:'1.0-0' }}đ</small>
                </td>
                <td>{{ promo.usedCount }}<span *ngIf="promo.usageLimit > 0">/{{ promo.usageLimit }}</span></td>
                <td>
                  {{ promo.startDate | date:'dd/MM/yyyy' }}<br>
                  <small class="text-muted">→ {{ promo.endDate | date:'dd/MM/yyyy' }}</small>
                </td>
                <td>
                  <span [class]="'st-badge ' + (promo.isActive ? 'st-active' : 'st-inactive')">
                    {{ promo.isActive ? 'Hoạt động' : 'Tắt' }}
                  </span>
                  <span *ngIf="isExpired(promo)" class="st-badge st-expired">Hết hạn</span>
                </td>
                <td>
                  <button class="btn-icon btn-edit" (click)="editPromo(promo)"><i class="fas fa-edit"></i></button>
                  <button class="btn-icon btn-danger" (click)="deletePromo(promo)"><i class="fas fa-trash"></i></button>
                </td>
              </tr>
              <tr *ngIf="promotions.length === 0">
                <td colspan="8" class="text-center py-4 text-muted">Chưa có khuyến mãi nào</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Pagination -->
    <div class="pagination" *ngIf="pagination.pages > 1">
      <button [disabled]="pagination.page <= 1" (click)="goPage(pagination.page - 1)">«</button>
      <button *ngFor="let p of getPages()" [class.active]="p === pagination.page" (click)="goPage(p)">{{ p }}</button>
      <button [disabled]="pagination.page >= pagination.pages" (click)="goPage(pagination.page + 1)">»</button>
    </div>

    <!-- Modal Form -->
    <div class="modal-overlay" *ngIf="showForm" (click)="showForm = false">
      <div class="modal-box" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <h3>{{ editingId ? 'Sửa' : 'Thêm' }} khuyến mãi</h3>
          <button class="btn-close" (click)="showForm = false">&times;</button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label>Mã khuyến mãi *</label>
            <input type="text" [(ngModel)]="form.code" class="form-control" placeholder="VD: SALE20">
          </div>
          <div class="form-group">
            <label>Tên *</label>
            <input type="text" [(ngModel)]="form.name" class="form-control" placeholder="Giảm 20% đơn hàng">
          </div>
          <div class="form-group">
            <label>Mô tả</label>
            <textarea [(ngModel)]="form.description" class="form-control" rows="2"></textarea>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Loại *</label>
              <select [(ngModel)]="form.type" class="form-control">
                <option value="Percentage">Phần trăm (%)</option>
                <option value="FixedAmount">Số tiền cố định</option>
                <option value="FreeShipping">Miễn phí ship</option>
                <option value="BuyXGetY">Mua X tặng Y</option>
              </select>
            </div>
            <div class="form-group">
              <label>Giá trị *</label>
              <input type="number" [(ngModel)]="form.value" class="form-control" [placeholder]="form.type === 'Percentage' ? 'VD: 20' : 'VD: 50000'">
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Đơn tối thiểu</label>
              <input type="number" [(ngModel)]="form.minOrderAmount" class="form-control" placeholder="0">
            </div>
            <div class="form-group">
              <label>Giảm tối đa</label>
              <input type="number" [(ngModel)]="form.maxDiscountAmount" class="form-control" placeholder="0 = không giới hạn">
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Giới hạn sử dụng</label>
              <input type="number" [(ngModel)]="form.usageLimit" class="form-control" placeholder="0 = không giới hạn">
            </div>
            <div class="form-group">
              <label>Mỗi user</label>
              <input type="number" [(ngModel)]="form.perUserLimit" class="form-control" placeholder="1">
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Ngày bắt đầu *</label>
              <input type="date" [(ngModel)]="form.startDate" class="form-control">
            </div>
            <div class="form-group">
              <label>Ngày kết thúc *</label>
              <input type="date" [(ngModel)]="form.endDate" class="form-control">
            </div>
          </div>
          <div class="form-group">
            <label>
              <input type="checkbox" [(ngModel)]="form.isActive"> Hoạt động
            </label>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-outline" (click)="showForm = false">Hủy</button>
          <button class="btn btn-gold" (click)="savePromo()">{{ editingId ? 'Cập nhật' : 'Tạo mới' }}</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
    .page-header h2 { font-weight: 600; font-size: 1.4rem; color: #1a1a1a; margin: 0; }
    .page-header h2 i { color: #D4AF37; margin-right: 8px; }
    .card { border: none; border-radius: 12px; box-shadow: 0 2px 12px rgba(0,0,0,.08); background: #fff; margin-bottom: 16px; }
    .card-body { padding: 16px; }
    .p-0 { padding: 0 !important; }
    .table-responsive { overflow-x: auto; }
    .adm-table { width: 100%; border-collapse: collapse; }
    .adm-table th, .adm-table td { padding: 10px 14px; border-bottom: 1px solid #f0f0f0; font-size: .85rem; }
    .adm-table thead th { background: #f8f9fa; font-weight: 600; white-space: nowrap; }
    .adm-table tbody tr:hover { background: rgba(212,175,55,.05); }
    .text-gold { color: #D4AF37; }
    .text-muted { color: #6c757d; }
    .text-center { text-align: center; }
    .py-4 { padding: 30px 0; }
    .st-badge { padding: 3px 10px; border-radius: 4px; font-size: .73rem; font-weight: 500; color: #fff; display: inline-block; margin: 2px 0; }
    .st-active { background: #28a745; }
    .st-inactive { background: #6c757d; }
    .st-expired { background: #dc3545; }
    .btn { padding: 8px 16px; border: none; border-radius: 6px; cursor: pointer; font-size: .85rem; }
    .btn-gold { background: #D4AF37; color: #fff; }
    .btn-gold:hover { background: #B8960C; }
    .btn-outline { background: #f8f9fa; border: 1px solid #dee2e6; color: #333; }
    .btn-icon { border: none; padding: 6px 10px; border-radius: 4px; cursor: pointer; font-size: .8rem; margin-right: 4px; }
    .btn-edit { background: #17a2b8; color: #fff; }
    .btn-edit:hover { background: #138496; }
    .btn-danger { background: #dc3545; color: #fff; }
    .btn-danger:hover { background: #c82333; }
    .modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,.5); z-index: 2000; display: flex; align-items: center; justify-content: center; }
    .modal-box { background: #fff; border-radius: 12px; width: 580px; max-width: 95vw; max-height: 90vh; overflow-y: auto; }
    .modal-header { display: flex; justify-content: space-between; align-items: center; padding: 16px 20px; border-bottom: 1px solid #f0f0f0; }
    .modal-header h3 { margin: 0; font-size: 1.1rem; font-weight: 600; }
    .btn-close { background: none; border: none; font-size: 1.5rem; cursor: pointer; color: #6c757d; }
    .modal-body { padding: 20px; }
    .modal-footer { padding: 12px 20px; border-top: 1px solid #f0f0f0; display: flex; justify-content: flex-end; gap: 8px; }
    .form-group { margin-bottom: 12px; }
    .form-group label { display: block; font-size: .85rem; font-weight: 500; margin-bottom: 4px; color: #333; }
    .form-control { width: 100%; padding: 8px 12px; border: 1px solid #dee2e6; border-radius: 6px; font-size: .85rem; box-sizing: border-box; }
    .form-control:focus { outline: none; border-color: #D4AF37; }
    .form-row { display: flex; gap: 12px; }
    .form-row .form-group { flex: 1; }
    .pagination { display: flex; justify-content: center; gap: 4px; margin-top: 16px; }
    .pagination button { padding: 6px 12px; border: 1px solid #dee2e6; background: #fff; border-radius: 4px; cursor: pointer; font-size: .85rem; }
    .pagination button.active { background: #D4AF37; color: #fff; border-color: #D4AF37; }
    .pagination button:disabled { opacity: .5; cursor: not-allowed; }
  `]
})
export class AdminPromotionsComponent implements OnInit {
  promotions: any[] = [];
  pagination = { page: 1, limit: 20, total: 0, pages: 0 };
  showForm = false;
  editingId = '';
  form: any = {};

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadPromotions();
  }

  resetForm(): void {
    this.editingId = '';
    this.form = {
      code: '', name: '', description: '', type: 'Percentage', value: 0,
      minOrderAmount: 0, maxDiscountAmount: 0, usageLimit: 0, perUserLimit: 1,
      startDate: '', endDate: '', isActive: true
    };
  }

  loadPromotions(): void {
    this.api.get('/promotions', { page: this.pagination.page, limit: this.pagination.limit }).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.promotions = res.data?.promotions || [];
          if (res.data?.pagination) this.pagination = { ...this.pagination, ...res.data.pagination };
        }
      },
      error: () => {}
    });
  }

  editPromo(promo: any): void {
    this.editingId = promo._id;
    this.form = {
      code: promo.code, name: promo.name, description: promo.description || '',
      type: promo.type, value: promo.value,
      minOrderAmount: promo.minOrderAmount || 0,
      maxDiscountAmount: promo.maxDiscountAmount || 0,
      usageLimit: promo.usageLimit || 0,
      perUserLimit: promo.perUserLimit || 1,
      startDate: promo.startDate?.substring(0, 10) || '',
      endDate: promo.endDate?.substring(0, 10) || '',
      isActive: promo.isActive
    };
    this.showForm = true;
  }

  savePromo(): void {
    if (!this.form.code || !this.form.name || !this.form.startDate || !this.form.endDate) {
      alert('Vui lòng điền đầy đủ thông tin bắt buộc');
      return;
    }
    const data = { ...this.form };
    const obs = this.editingId
      ? this.api.put(`/promotions/${this.editingId}`, data)
      : this.api.post('/promotions', data);

    obs.subscribe({
      next: (res: any) => {
        if (res.success) {
          this.showForm = false;
          this.loadPromotions();
        } else {
          alert(res.message || 'Lỗi');
        }
      },
      error: (err: any) => alert(err.error?.message || 'Lỗi server')
    });
  }

  deletePromo(promo: any): void {
    if (!confirm(`Xóa khuyến mãi "${promo.name}"?`)) return;
    this.api.delete(`/promotions/${promo._id}`).subscribe({
      next: () => this.loadPromotions(),
      error: () => alert('Lỗi khi xóa')
    });
  }

  isExpired(promo: any): boolean {
    return new Date(promo.endDate) < new Date();
  }

  getTypeName(type: string): string {
    const m: any = { Percentage: 'Phần trăm', FixedAmount: 'Cố định', FreeShipping: 'Free ship', BuyXGetY: 'Mua X tặng Y' };
    return m[type] || type;
  }

  goPage(p: number): void {
    this.pagination.page = p;
    this.loadPromotions();
  }

  getPages(): number[] {
    const pages = [];
    for (let i = 1; i <= this.pagination.pages; i++) pages.push(i);
    return pages;
  }
}
