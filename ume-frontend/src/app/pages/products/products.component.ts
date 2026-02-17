import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { CartService } from '../../services/cart.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './products.component.html',
  styleUrl: './products.component.scss'
})
export class ProductsComponent implements OnInit {
  products: any[] = [];
  categories: any[] = [];
  brands: any[] = [];
  loading = false;

  // Filters
  searchTerm = '';
  selectedCategory = '';
  selectedBrand = '';
  minPrice: number | null = null;
  maxPrice: number | null = null;
  sortBy = '-createdAt';

  // Pagination
  currentPage = 1;
  totalPages = 1;
  totalItems = 0;
  limit = 12;

  constructor(
    private apiService: ApiService,
    private cartService: CartService,
    private toastr: ToastrService
  ) {}

  getImg(path: string): string { return this.apiService.getImageUrl(path); }

  ngOnInit(): void {
    this.loadCategories();
    this.loadBrands();
    this.loadProducts();
  }

  loadCategories(): void {
    this.apiService.getCategories().subscribe({
      next: (res: any) => {
        this.categories = res.data?.categories || res.data || [];
      }
    });
  }

  loadBrands(): void {
    this.apiService.getBrands().subscribe({
      next: (res: any) => {
        this.brands = res.data?.brands || res.data || [];
      }
    });
  }

  loadProducts(): void {
    this.loading = true;
    const params: any = {
      page: this.currentPage,
      limit: this.limit,
      sort: this.sortBy
    };
    if (this.searchTerm) params.search = this.searchTerm;
    if (this.selectedCategory) params.category = this.selectedCategory;
    if (this.selectedBrand) params.brand = this.selectedBrand;
    if (this.minPrice !== null) params.minPrice = this.minPrice;
    if (this.maxPrice !== null) params.maxPrice = this.maxPrice;

    this.apiService.getProducts(params).subscribe({
      next: (res: any) => {
        this.products = res.data?.products || res.data?.items || res.data || [];
        const pagination = res.data?.pagination;
        if (pagination) {
          this.totalPages = pagination.pages || 1;
          this.totalItems = pagination.total || 0;
        } else {
          this.totalItems = this.products.length;
        }
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadProducts();
  }

  onCategoryFilter(categoryId: string): void {
    this.selectedCategory = categoryId;
    this.currentPage = 1;
    this.loadProducts();
  }

  onBrandFilter(brandId: string): void {
    this.selectedBrand = brandId;
    this.currentPage = 1;
    this.loadProducts();
  }

  onPriceFilter(): void {
    this.currentPage = 1;
    this.loadProducts();
  }

  onSortChange(): void {
    this.currentPage = 1;
    this.loadProducts();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadProducts();
    }
  }

  get pages(): number[] {
    const p: number[] = [];
    const start = Math.max(1, this.currentPage - 2);
    const end = Math.min(this.totalPages, this.currentPage + 2);
    for (let i = start; i <= end; i++) p.push(i);
    return p;
  }

  addToCart(product: any): void {
    this.cartService.addToCart(product, 1);
    this.toastr.success('Đã thêm vào giỏ hàng!', 'Thành công');
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
  }

  getStars(rating: number): number[] {
    return Array(5).fill(0).map((_, i) => i < Math.round(rating) ? 1 : 0);
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedCategory = '';
    this.selectedBrand = '';
    this.minPrice = null;
    this.maxPrice = null;
    this.sortBy = '-createdAt';
    this.currentPage = 1;
    this.loadProducts();
  }
}
