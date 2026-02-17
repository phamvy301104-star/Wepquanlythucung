import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-my-appointments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './my-appointments.component.html',
  styleUrl: './my-appointments.component.scss'
})
export class MyAppointmentsComponent implements OnInit {
  appointments: any[] = [];
  loading = false;
  selectedStatus = '';
  cancelReason = '';
  cancellingId: string | null = null;

  // Success modal
  showSuccessModal = false;
  successMessage = '';

  // Review
  showReviewModal = false;
  reviewingAppointment: any = null;
  reviewRating = 5;
  reviewComment = '';
  reviewSubmitting = false;
  reviewedMap: { [key: string]: boolean } = {};
  hoverRating = 0;

  statuses = [
    { value: '', label: 'Tất cả' },
    { value: 'Pending', label: 'Chờ xác nhận' },
    { value: 'Confirmed', label: 'Đã xác nhận' },
    { value: 'InProgress', label: 'Đang thực hiện' },
    { value: 'Completed', label: 'Hoàn thành' },
    { value: 'Cancelled', label: 'Đã hủy' }
  ];

  constructor(
    private apiService: ApiService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadAppointments();
  }

  loadAppointments(): void {
    this.loading = true;
    const params: any = {};
    if (this.selectedStatus) params.status = this.selectedStatus;

    this.apiService.getMyAppointments(params).subscribe({
      next: (res: any) => {
        this.appointments = res.data?.appointments || res.data?.items || [];
        this.loading = false;
        // Check which completed appointments already have reviews
        this.appointments.forEach(apt => {
          if (apt.status === 'Completed') {
            this.checkReviewed(apt._id);
          }
        });
      },
      error: () => { this.loading = false; }
    });
  }

  checkReviewed(appointmentId: string): void {
    this.apiService.get(`/reviews/check/${appointmentId}`).subscribe({
      next: (res: any) => {
        this.reviewedMap[appointmentId] = res.data?.reviewed || false;
      },
      error: () => {}
    });
  }

  filterByStatus(status: string): void {
    this.selectedStatus = status;
    this.loadAppointments();
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
    this.apiService.cancelAppointment(this.cancellingId, { cancelReason: this.cancelReason }).subscribe({
      next: () => {
        this.cancellingId = null;
        this.successMessage = 'Đã hủy lịch hẹn thành công!';
        this.showSuccessModal = true;
        this.loadAppointments();
      },
      error: (err: any) => {
        this.toastr.error(err.error?.message || 'Hủy lịch hẹn thất bại', 'Lỗi');
      }
    });
  }

  // Review methods
  openReviewModal(apt: any): void {
    this.reviewingAppointment = apt;
    this.reviewRating = 5;
    this.reviewComment = '';
    this.hoverRating = 0;
    this.showReviewModal = true;
  }

  closeReviewModal(): void {
    this.showReviewModal = false;
    this.reviewingAppointment = null;
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
    if (!this.reviewingAppointment) return;
    this.reviewSubmitting = true;

    const formData = new FormData();
    formData.append('appointmentId', this.reviewingAppointment._id);
    formData.append('rating', this.reviewRating.toString());
    formData.append('comment', this.reviewComment);

    // Attach the first service from the appointment
    if (this.reviewingAppointment.services?.length > 0) {
      const svc = this.reviewingAppointment.services[0];
      const serviceId = svc.service?._id || svc.service;
      if (serviceId) formData.append('serviceId', serviceId);
    }

    this.apiService.createReview(formData).subscribe({
      next: () => {
        this.toastr.success('Cảm ơn bạn đã đánh giá!', 'Thành công');
        this.reviewedMap[this.reviewingAppointment._id] = true;
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
      InProgress: 'Đang thực hiện',
      Completed: 'Hoàn thành',
      Cancelled: 'Đã hủy'
    };
    return map[status] || status;
  }

  getStatusClass(status: string): string {
    const map: any = {
      Pending: 'pending',
      Confirmed: 'confirmed',
      InProgress: 'in-progress',
      Completed: 'completed',
      Cancelled: 'cancelled'
    };
    return map[status] || '';
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString('vi-VN', {
      year: 'numeric', month: '2-digit', day: '2-digit'
    });
  }
}
