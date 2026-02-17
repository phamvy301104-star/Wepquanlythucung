import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-services',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './services.component.html',
  styleUrl: './services.component.scss'
})
export class ServicesComponent implements OnInit {
  services: any[] = [];
  categories: any[] = [];
  selectedCategory = '';
  loading = false;

  constructor(
    private apiService: ApiService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['category']) {
        this.selectedCategory = params['category'];
      }
      this.loadCategories();
      this.loadServices();
    });
  }

  loadCategories(): void {
    this.apiService.getServiceCategories().subscribe({
      next: (res: any) => {
        this.categories = res.data?.items || res.data || [];
      }
    });
  }

  loadServices(): void {
    this.loading = true;
    const params: any = {};
    if (this.selectedCategory) params.category = this.selectedCategory;

    this.apiService.getServices(params).subscribe({
      next: (res: any) => {
        this.services = res.data?.items || res.data || [];
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  filterByCategory(catId: string): void {
    this.selectedCategory = catId;
    this.loadServices();
  }

  viewServiceDetail(serviceId: string): void {
    this.router.navigate(['/services', serviceId]);
  }

  bookService(serviceId: string): void {
    this.router.navigate(['/booking'], { queryParams: { service: serviceId } });
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
  }

  getImageUrl(path: string, placeholder?: string): string {
    return this.apiService.getImageUrl(path, placeholder);
  }

  formatDuration(minutes: number): string {
    if (minutes < 60) return `${minutes} phút`;
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return m > 0 ? `${h} giờ ${m} phút` : `${h} giờ`;
  }

  getStars(rating: number): number[] {
    return Array(5).fill(0).map((_, i) => i < Math.round(rating) ? 1 : 0);
  }
}
