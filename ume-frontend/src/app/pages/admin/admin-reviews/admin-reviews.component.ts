import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../services/api.service';

@Component({
  selector: 'app-admin-reviews',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <h2><i class="fas fa-star"></i> Quản lý đánh giá</h2>
    </div>

    <!-- Filters -->
    <div class="card mb-3">
      <div class="card-body">
        <div class="filter-row">
          <select [(ngModel)]="filterApproved" (change)="loadReviews()" class="form-control">
            <option value="">Tất cả trạng thái</option>
            <option value="true">Đã duyệt</option>
            <option value="false">Chưa duyệt</option>
          </select>
          <button class="btn btn-gold" (click)="loadReviews()"><i class="fas fa-sync"></i> Làm mới</button>
        </div>
      </div>
    </div>

    <!-- Reviews List -->
    <div class="card">
      <div class="card-body p-0">
        <div class="table-responsive">
          <table class="adm-table">
            <thead>
              <tr>
                <th>Khách hàng</th>
                <th>Đối tượng</th>
                <th>Đánh giá</th>
                <th>Nội dung</th>
                <th>Phản hồi</th>
                <th>Ngày</th>
                <th>Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let review of reviews">
                <td>
                  <strong>{{ review.customer?.fullName || 'N/A' }}</strong>
                </td>
                <td>
                  <span *ngIf="review.product" class="target-badge bg-product">
                    <i class="fas fa-box"></i> {{ review.product?.name }}
                  </span>
                  <span *ngIf="review.service" class="target-badge bg-service">
                    <i class="fas fa-cut"></i> {{ review.service?.name }}
                  </span>
                  <span *ngIf="review.staff" class="target-badge bg-staff">
                    <i class="fas fa-user"></i> {{ review.staff?.fullName }}
                  </span>
                  <span *ngIf="review.appointment && !review.service && !review.product" class="target-badge bg-appointment">
                    <i class="fas fa-calendar-check"></i> {{ review.appointment?.appointmentCode || 'Lịch hẹn' }}
                  </span>
                </td>
                <td>
                  <div class="stars">
                    <span *ngFor="let s of [1,2,3,4,5]" [class.filled]="s <= review.rating">★</span>
                  </div>
                </td>
                <td class="review-content">
                  <strong *ngIf="review.title">{{ review.title }}</strong>
                  <p>{{ review.comment || 'Không có nội dung' }}</p>
                  <div class="review-images" *ngIf="review.images?.length">
                    <img *ngFor="let img of review.images" [src]="apiUrl + img" class="review-thumb">
                  </div>
                </td>
                <td>
                  <div *ngIf="review.reply" class="reply-box">
                    <p>{{ review.reply }}</p>
                    <small class="text-muted">{{ review.repliedAt | date:'dd/MM HH:mm' }}</small>
                  </div>
                  <div *ngIf="!review.reply && !review.showReplyForm">
                    <button class="btn btn-sm btn-outline" (click)="review.showReplyForm = true">Phản hồi</button>
                  </div>
                  <div *ngIf="review.showReplyForm" class="reply-form">
                    <textarea [(ngModel)]="review.replyText" placeholder="Nhập phản hồi..." rows="2" class="form-control"></textarea>
                    <div class="reply-actions">
                      <button class="btn btn-sm btn-gold" (click)="replyReview(review)">Gửi</button>
                      <button class="btn btn-sm btn-outline" (click)="review.showReplyForm = false">Hủy</button>
                    </div>
                  </div>
                </td>
                <td>{{ review.createdAt | date:'dd/MM/yyyy' }}</td>
                <td>
                  <button class="btn-icon btn-danger" (click)="deleteReview(review)" title="Xóa"><i class="fas fa-trash"></i></button>
                </td>
              </tr>
              <tr *ngIf="reviews.length === 0">
                <td colspan="7" class="text-center py-4 text-muted">Chưa có đánh giá nào</td>
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
  `,
  styles: [`
    :host { display: block; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
    .page-header h2 { font-weight: 600; font-size: 1.4rem; color: #1a1a1a; margin: 0; }
    .page-header h2 i { color: #D4AF37; margin-right: 8px; }
    .card { border: none; border-radius: 12px; box-shadow: 0 2px 12px rgba(0,0,0,.08); background: #fff; margin-bottom: 16px; }
    .card-body { padding: 16px; }
    .mb-3 { margin-bottom: 16px; }
    .p-0 { padding: 0 !important; }
    .filter-row { display: flex; gap: 10px; flex-wrap: wrap; }
    .form-control { padding: 8px 12px; border: 1px solid #dee2e6; border-radius: 6px; font-size: .85rem; width: 100%; }
    .form-control:focus { outline: none; border-color: #D4AF37; }
    .btn { padding: 8px 16px; border: none; border-radius: 6px; cursor: pointer; font-size: .85rem; }
    .btn-sm { padding: 4px 10px; font-size: .78rem; }
    .btn-gold { background: #D4AF37; color: #fff; }
    .btn-gold:hover { background: #B8960C; }
    .btn-outline { background: #f8f9fa; border: 1px solid #dee2e6; color: #333; }
    .btn-outline:hover { background: #e9ecef; }
    .table-responsive { overflow-x: auto; }
    .adm-table { width: 100%; border-collapse: collapse; }
    .adm-table th, .adm-table td { padding: 10px 14px; border-bottom: 1px solid #f0f0f0; font-size: .85rem; vertical-align: top; }
    .adm-table thead th { background: #f8f9fa; font-weight: 600; white-space: nowrap; }
    .adm-table tbody tr:hover { background: rgba(212,175,55,.05); }
    .stars { color: #ddd; font-size: 1rem; }
    .stars .filled { color: #ffc107; }
    .target-badge { display: inline-block; padding: 3px 8px; border-radius: 4px; font-size: .73rem; color: #fff; margin-bottom: 2px; }
    .bg-product { background: #6c757d; }
    .bg-service { background: #17a2b8; }
    .bg-staff { background: #28a745; }
    .bg-appointment { background: #6f42c1; }
    .review-content { max-width: 250px; }
    .review-content p { margin: 4px 0 0; font-size: .82rem; color: #555; }
    .review-images { display: flex; gap: 4px; margin-top: 6px; }
    .review-thumb { width: 40px; height: 40px; border-radius: 4px; object-fit: cover; }
    .reply-box { background: #f8f9fa; padding: 8px; border-radius: 6px; font-size: .82rem; max-width: 200px; }
    .reply-box p { margin: 0; }
    .reply-form { display: flex; flex-direction: column; gap: 6px; max-width: 200px; }
    .reply-form textarea { font-size: .82rem; resize: none; }
    .reply-actions { display: flex; gap: 4px; }
    .text-muted { color: #6c757d; }
    .text-center { text-align: center; }
    .py-4 { padding: 30px 0; }
    .btn-icon { border: none; padding: 6px 10px; border-radius: 4px; cursor: pointer; font-size: .8rem; }
    .btn-danger { background: #dc3545; color: #fff; }
    .btn-danger:hover { background: #c82333; }
    .pagination { display: flex; justify-content: center; gap: 4px; margin-top: 16px; }
    .pagination button { padding: 6px 12px; border: 1px solid #dee2e6; background: #fff; border-radius: 4px; cursor: pointer; font-size: .85rem; }
    .pagination button.active { background: #D4AF37; color: #fff; border-color: #D4AF37; }
    .pagination button:disabled { opacity: .5; cursor: not-allowed; }
  `]
})
export class AdminReviewsComponent implements OnInit {
  reviews: any[] = [];
  filterApproved = '';
  pagination = { page: 1, limit: 20, total: 0, pages: 0 };
  apiUrl = '';

  constructor(private api: ApiService) {
    this.apiUrl = (this.api as any).apiUrl?.replace('/api', '') || 'http://localhost:5000';
  }

  ngOnInit(): void {
    this.loadReviews();
  }

  loadReviews(): void {
    const params: any = { page: this.pagination.page, limit: this.pagination.limit };
    if (this.filterApproved) params.isApproved = this.filterApproved;

    this.api.get('/reviews', params).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.reviews = (res.data?.reviews || []).map((r: any) => ({ ...r, showReplyForm: false, replyText: '' }));
          if (res.data?.pagination) this.pagination = { ...this.pagination, ...res.data.pagination };
        }
      },
      error: () => {}
    });
  }

  replyReview(review: any): void {
    if (!review.replyText?.trim()) return;
    this.api.put(`/reviews/${review._id}/reply`, { reply: review.replyText }).subscribe({
      next: (res: any) => {
        if (res.success) {
          review.reply = review.replyText;
          review.repliedAt = new Date();
          review.showReplyForm = false;
        }
      },
      error: () => alert('Lỗi khi phản hồi')
    });
  }

  deleteReview(review: any): void {
    if (!confirm('Xóa đánh giá này?')) return;
    this.api.delete(`/reviews/${review._id}`).subscribe({
      next: () => this.loadReviews(),
      error: () => alert('Lỗi khi xóa')
    });
  }

  goPage(p: number): void {
    this.pagination.page = p;
    this.loadReviews();
  }

  getPages(): number[] {
    const pages = [];
    for (let i = 1; i <= this.pagination.pages; i++) pages.push(i);
    return pages;
  }
}
