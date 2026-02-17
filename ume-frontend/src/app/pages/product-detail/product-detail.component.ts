import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { CartService } from '../../services/cart.service';
import { AuthService } from '../../services/auth.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.scss'
})
export class ProductDetailComponent implements OnInit {
  product: any = null;
  reviews: any[] = [];
  loading = true;
  quantity = 1;
  selectedImage = '';
  reviewPage = 1;
  reviewTotalPages = 1;

  // Write review
  showReviewForm = false;
  newReviewRating = 5;
  newReviewComment = '';
  hoverRating = 0;
  reviewSubmitting = false;

  constructor(
    private route: ActivatedRoute,
    private apiService: ApiService,
    private cartService: CartService,
    private toastr: ToastrService,
    public authService: AuthService
  ) {}

  getImg(path: string): string { return this.apiService.getImageUrl(path); }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.loadProduct(params['id']);
      this.loadReviews(params['id']);
    });
  }

  loadProduct(id: string): void {
    this.loading = true;
    this.apiService.getProduct(id).subscribe({
      next: (res: any) => {
        this.product = res.data || res;
        this.selectedImage = this.getImg(this.product.imageUrl || this.product.mainImage);
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  loadReviews(productId: string): void {
    this.apiService.getProductReviews(productId, { page: this.reviewPage, limit: 5 }).subscribe({
      next: (res: any) => {
        this.reviews = res.data?.items || res.data || [];
        this.reviewTotalPages = res.data?.pagination?.pages || 1;
      }
    });
  }

  selectImage(img: string): void {
    this.selectedImage = img;
  }

  get allImages(): string[] {
    if (!this.product) return [];
    const imgs = [this.getImg(this.product.imageUrl || this.product.mainImage)];
    if (this.product.additionalImages) {
      imgs.push(...this.product.additionalImages.map((img: string) => this.getImg(img)));
    }
    return imgs.filter(Boolean);
  }

  increaseQty(): void {
    if (this.quantity < (this.product?.stockQuantity || 99)) this.quantity++;
  }

  decreaseQty(): void {
    if (this.quantity > 1) this.quantity--;
  }

  addToCart(): void {
    if (!this.product) return;
    this.cartService.addToCart(this.product, this.quantity);
    this.toastr.success(`Đã thêm ${this.quantity} sản phẩm vào giỏ hàng!`, 'Thành công');
  }

  getDiscountPercent(): number {
    if (!this.product?.originalPrice || this.product.originalPrice <= this.product.price) return 0;
    return Math.round((1 - this.product.price / this.product.originalPrice) * 100);
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
  }

  getStars(rating: number): number[] {
    return Array(5).fill(0).map((_, i) => i < Math.round(rating) ? 1 : 0);
  }

  getTimeAgo(date: string): string {
    const diff = Date.now() - new Date(date).getTime();
    const days = Math.floor(diff / 86400000);
    if (days < 1) return 'Hôm nay';
    if (days < 30) return `${days} ngày trước`;
    if (days < 365) return `${Math.floor(days / 30)} tháng trước`;
    return `${Math.floor(days / 365)} năm trước`;
  }

  // Review form methods
  toggleReviewForm(): void {
    this.showReviewForm = !this.showReviewForm;
    if (this.showReviewForm) {
      this.newReviewRating = 5;
      this.newReviewComment = '';
      this.hoverRating = 0;
    }
  }

  setNewRating(star: number): void {
    this.newReviewRating = star;
  }

  setHoverRating(star: number): void {
    this.hoverRating = star;
  }

  resetHover(): void {
    this.hoverRating = 0;
  }

  getNewStarClass(star: number): string {
    const active = this.hoverRating || this.newReviewRating;
    return star <= active ? 'star active' : 'star';
  }

  getNewRatingLabel(): string {
    const active = this.hoverRating || this.newReviewRating;
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
    if (!this.product) return;
    this.reviewSubmitting = true;

    const formData = new FormData();
    formData.append('productId', this.product._id);
    formData.append('rating', this.newReviewRating.toString());
    formData.append('comment', this.newReviewComment);

    this.apiService.createReview(formData).subscribe({
      next: () => {
        this.toastr.success('Cảm ơn bạn đã đánh giá!', 'Thành công');
        this.showReviewForm = false;
        this.reviewSubmitting = false;
        this.newReviewComment = '';
        this.newReviewRating = 5;
        // Reload reviews
        this.loadReviews(this.product._id);
        this.loadProduct(this.product._id);
      },
      error: (err: any) => {
        this.reviewSubmitting = false;
        this.toastr.error(err.error?.message || 'Gửi đánh giá thất bại', 'Lỗi');
      }
    });
  }
}
