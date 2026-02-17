import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-service-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './service-detail.component.html',
  styleUrl: './service-detail.component.scss'
})
export class ServiceDetailComponent implements OnInit {
  service: any = null;
  reviews: any[] = [];
  loading = true;
  reviewPage = 1;
  reviewTotalPages = 1;
  isLoggedIn = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService,
    private authService: AuthService,
    private toastr: ToastrService
  ) {}

  getImg(path: string): string {
    return this.apiService.getImageUrl(path, 'assets/images/no-service.svg');
  }

  ngOnInit(): void {
    this.isLoggedIn = this.authService.isLoggedIn;
    this.route.params.subscribe(params => {
      this.loadService(params['id']);
      this.loadReviews(params['id']);
    });
  }

  loadService(id: string): void {
    this.loading = true;
    this.apiService.getService(id).subscribe({
      next: (res: any) => {
        this.service = res.data || res;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Không tìm thấy dịch vụ');
        this.router.navigate(['/services']);
      }
    });
  }

  loadReviews(serviceId: string): void {
    this.apiService.getServiceReviews(serviceId, { page: this.reviewPage, limit: 5 }).subscribe({
      next: (res: any) => {
        this.reviews = res.data?.items || res.data || [];
        this.reviewTotalPages = res.data?.pagination?.pages || 1;
      },
      error: () => {
        this.reviews = [];
      }
    });
  }

  bookService(): void {
    if (!this.authService.isLoggedIn) {
      this.toastr.warning('Vui lòng đăng nhập để đặt dịch vụ');
      this.router.navigate(['/login']);
      return;
    }
    this.router.navigate(['/booking'], { queryParams: { service: this.service._id } });
  }

  shareService(method: string): void {
    const url = window.location.href;
    const text = `Dịch vụ ${this.service.name} - ${this.formatPrice(this.service.price)}`;

    switch (method) {
      case 'facebook':
        window.open(`https://www.facebook.com/sharer/sharer.php?u=${url}`, '_blank');
        break;
      case 'twitter':
        window.open(`https://twitter.com/intent/tweet?url=${url}&text=${text}`, '_blank');
        break;
      case 'copy':
        navigator.clipboard.writeText(url);
        this.toastr.success('Đã sao chép link!');
        break;
    }
  }

  loadMoreReviews(): void {
    if (this.reviewPage < this.reviewTotalPages) {
      this.reviewPage++;
      this.loadReviews(this.service._id);
    }
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
  }

  formatDuration(minutes: number): string {
    if (!minutes) return 'N/A';
    if (minutes < 60) return `${minutes} phút`;
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return m > 0 ? `${h} giờ ${m} phút` : `${h} giờ`;
  }

  getStars(rating: number): number[] {
    return Array(5).fill(0).map((_, i) => i < Math.round(rating) ? 1 : 0);
  }

  getPetTypes(): string {
    if (!this.service?.petTypes) return 'N/A';
    return Array.isArray(this.service.petTypes) 
      ? this.service.petTypes.join(', ')
      : this.service.petTypes;
  }
}
