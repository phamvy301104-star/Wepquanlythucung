import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { CartService } from '../../services/cart.service';
import { AuthService } from '../../services/auth.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-pet-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './pet-detail.component.html',
  styleUrl: './pet-detail.component.scss'
})
export class PetDetailComponent implements OnInit {
  pet: any = null;
  similarPets: any[] = [];
  loading = true;
  qrCodeUrl = '';
  isOwner = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService,
    private cartService: CartService,
    public authService: AuthService,
    private toastr: ToastrService
  ) {}

  getImg(path: string): string {
    return this.apiService.getImageUrl(path, 'assets/images/default-pet.svg');
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.loadPet(params['id']);
    });
  }

  loadPet(id: string): void {
    this.loading = true;
    this.apiService.getPet(id).subscribe({
      next: (res: any) => {
        this.pet = res.data || res;
        this.isOwner = this.authService.isLoggedIn && this.pet?.owner?._id === this.authService.currentUser?._id;
        this.loading = false;
        this.generateQRCode();
        this.loadSimilarPets();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Không tìm thấy thú cưng');
        this.router.navigate(['/pets']);
      }
    });
  }

  loadSimilarPets(): void {
    if (!this.pet) return;
    // Don't show similar pets for customers viewing their own pet
    if (this.isOwner && !this.authService.isAdminOrStaff) return;
    this.apiService.getPets({ type: this.pet.type, limit: 4 }).subscribe({
      next: (res: any) => {
        const all = res.data?.pets || res.data || [];
        this.similarPets = all.filter((p: any) => p._id !== this.pet._id).slice(0, 4);
      }
    });
  }

  generateQRCode(): void {
    const url = window.location.href;
    this.qrCodeUrl = `https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=${encodeURIComponent(url)}`;
  }

  addToCart(): void {
    if (!this.authService.isLoggedIn) {
      this.toastr.warning('Vui lòng đăng nhập');
      this.router.navigate(['/login']);
      return;
    }
    this.cartService.addPetToCart(this.pet);
    this.toastr.success(`Đã thêm ${this.pet.name} vào giỏ hàng!`);
  }

  isPetInCart(): boolean {
    return this.cartService.items.some(i => i.product._id === this.pet?._id && i.itemType === 'pet');
  }

  viewPetDetail(petId: string): void {
    this.router.navigate(['/pets', petId]);
  }

  getTypeName(type: string): string {
    const m: any = { Dog: 'Chó', Cat: 'Mèo', Bird: 'Chim', Fish: 'Cá', Hamster: 'Hamster', Rabbit: 'Thỏ', Other: 'Khác' };
    return m[type] || type;
  }

  getGenderName(g: string): string {
    const m: any = { Male: 'Đực', Female: 'Cái', Unknown: 'Chưa rõ' };
    return m[g] || g;
  }

  getAgeLabel(pet: any): string {
    if (!pet?.age) return 'N/A';
    const unit = pet.ageUnit === 'years' ? 'tuổi' : 'tháng';
    return `${pet.age} ${unit}`;
  }

  formatPrice(price: number): string {
    if (!price) return 'Miễn phí';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
  }

  getPetCode(): string {
    if (!this.pet) return '';
    if (this.pet.code) return this.pet.code;
    const date = new Date(this.pet.createdAt);
    const y = date.getFullYear().toString().slice(-2);
    const m = (date.getMonth() + 1).toString().padStart(2, '0');
    const d = date.getDate().toString().padStart(2, '0');
    const id = this.pet._id?.slice(-4) || '0000';
    return `PET${y}${m}${d}${id}`;
  }

  getListingTypeLabel(type: string): string {
    const m: any = { Sale: 'Đang bán', Adoption: 'Nhận nuôi', None: '' };
    return m[type] || '';
  }
}
