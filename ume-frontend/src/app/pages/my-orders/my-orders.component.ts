import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-my-orders',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './my-orders.component.html',
  styleUrl: './my-orders.component.scss'
})
export class MyOrdersComponent implements OnInit {
  orders: any[] = [];
  loading = false;
  selectedStatus = '';
  cancellingId: string | null = null;
  cancelReason = '';
  selectedOrder: any = null;

  // Review
  showReviewModal = false;
  reviewingOrder: any = null;
  reviewingItem: any = null;
  reviewRating = 5;
  reviewComment = '';
  reviewSubmitting = false;
  hoverRating = 0;
  orderReviewedMap: { [orderId: string]: string[] } = {}; // orderId -> reviewed productId[]

  statuses = [
    { value: '', label: 'Tất cả' },
    { value: 'Pending', label: 'Chờ xác nhận' },
    { value: 'Confirmed', label: 'Đã xác nhận' },
    { value: 'Processing', label: 'Đang xử lý' },
    { value: 'Shipping', label: 'Đang giao' },
    { value: 'Completed', label: 'Hoàn thành' },
    { value: 'Cancelled', label: 'Đã hủy' }
  ];

  constructor(
    private apiService: ApiService,
    private toastr: ToastrService
  ) {}

  getImg(path: string): string { return this.apiService.getImageUrl(path); }

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    const params: any = {};
    if (this.selectedStatus) params.status = this.selectedStatus;

    this.apiService.getMyOrders(params).subscribe({
      next: (res: any) => {
        this.orders = res.data?.orders || res.data?.items || (Array.isArray(res.data) ? res.data : []);
        this.loading = false;
        // Check reviews for completed orders
        this.orders.forEach(order => {
          if (order.status === 'Completed') {
            this.checkOrderReviews(order._id);
          }
        });
      },
      error: () => { this.loading = false; }
    });
  }

  checkOrderReviews(orderId: string): void {
    this.apiService.get(`/reviews/check-order/${orderId}`).subscribe({
      next: (res: any) => {
        this.orderReviewedMap[orderId] = res.data?.reviewedProductIds || [];
      },
      error: () => {}
    });
  }

  hasUnreviewedItems(order: any): boolean {
    if (order.status !== 'Completed') return false;
    const reviewed = this.orderReviewedMap[order._id] || [];
    return (order.items || []).some((item: any) => {
      const pid = item.product?._id || item.product;
      return pid && !reviewed.includes(pid.toString());
    });
  }

  isItemReviewed(order: any, item: any): boolean {
    const reviewed = this.orderReviewedMap[order._id] || [];
    const pid = item.product?._id || item.product;
    return pid ? reviewed.includes(pid.toString()) : false;
  }

  filterByStatus(status: string): void {
    this.selectedStatus = status;
    this.loadOrders();
  }

  showCancelDialog(id: string): void {
    this.cancellingId = id;
    this.cancelReason = '';
  }

  closeCancelDialog(): void {
    this.cancellingId = null;
  }

  confirmCancel(): void {
    if (!this.cancellingId) return;
    this.apiService.cancelOrder(this.cancellingId, { cancelReason: this.cancelReason }).subscribe({
      next: () => {
        this.toastr.success('Đã hủy đơn hàng thành công', 'Thành công');
        this.cancellingId = null;
        this.loadOrders();
      },
      error: (err: any) => {
        this.toastr.error(err.error?.message || 'Hủy đơn hàng thất bại', 'Lỗi');
      }
    });
  }

  // Review methods
  openReviewModal(order: any, item: any): void {
    this.reviewingOrder = order;
    this.reviewingItem = item;
    this.reviewRating = 5;
    this.reviewComment = '';
    this.hoverRating = 0;
    this.showReviewModal = true;
  }

  closeReviewModal(): void {
    this.showReviewModal = false;
    this.reviewingOrder = null;
    this.reviewingItem = null;
  }

  setRating(star: number): void {
    this.reviewRating = star;
  }

  setHoverRating(star: number): void {
    this.hoverRating = star;
  }

  resetHover(): void {
    this.hoverRating = 0;
  }

  getStarClass(star: number): string {
    const active = this.hoverRating || this.reviewRating;
    return star <= active ? 'star active' : 'star';
  }

  getRatingLabel(): string {
    const active = this.hoverRating || this.reviewRating;
    const labels: { [key: number]: string } = {
      1: 'Rất không hài lòng',
      2: 'Không hài lòng',
      3: 'Bình thường',
      4: 'Hài lòng',
      5: 'Rất hài lòng'
    };
    return labels[active] || '';
  }

  submitReview(): void {
    if (!this.reviewingOrder || !this.reviewingItem) return;
    this.reviewSubmitting = true;

    const productId = this.reviewingItem.product?._id || this.reviewingItem.product;
    const formData = new FormData();
    formData.append('productId', productId);
    formData.append('orderId', this.reviewingOrder._id);
    formData.append('rating', this.reviewRating.toString());
    formData.append('comment', this.reviewComment);

    this.apiService.createReview(formData).subscribe({
      next: () => {
        this.toastr.success('Cảm ơn bạn đã đánh giá!', 'Thành công');
        // Update reviewed map
        if (!this.orderReviewedMap[this.reviewingOrder._id]) {
          this.orderReviewedMap[this.reviewingOrder._id] = [];
        }
        this.orderReviewedMap[this.reviewingOrder._id].push(productId);
        this.closeReviewModal();
        this.reviewSubmitting = false;
      },
      error: (err: any) => {
        this.reviewSubmitting = false;
        this.toastr.error(err.error?.message || 'Gửi đánh giá thất bại', 'Lỗi');
      }
    });
  }

  getStatusLabel(status: string): string {
    const map: any = {
      Pending: 'Chờ xác nhận',
      Confirmed: 'Đã xác nhận',
      Processing: 'Đang xử lý',
      Shipping: 'Đang giao',
      Completed: 'Hoàn thành',
      Cancelled: 'Đã hủy'
    };
    return map[status] || status;
  }

  getStatusClass(status: string): string {
    const map: any = {
      Pending: 'pending',
      Confirmed: 'confirmed',
      Processing: 'processing',
      Shipping: 'shipping',
      Completed: 'completed',
      Cancelled: 'cancelled'
    };
    return map[status] || '';
  }

  getItemsSummary(order: any): string {
    const items = order.items || [];
    if (items.length === 0) return '';
    const first = items[0].productName || items[0].product?.name || 'Sản phẩm';
    if (items.length === 1) return first;
    return `${first} và ${items.length - 1} sản phẩm khác`;
  }

  viewOrder(order: any): void {
    if (order._id) {
      this.apiService.getOrder(order._id).subscribe({
        next: (res: any) => { this.selectedOrder = res.data?.order || res.data || order; },
        error: () => { this.selectedOrder = order; }
      });
    } else {
      this.selectedOrder = order;
    }
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString('vi-VN', {
      year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit'
    });
  }
}
