import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';
import { CartService } from '../../services/cart.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-pets',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './pets.component.html',
  styleUrl: './pets.component.scss'
})
export class PetsComponent implements OnInit {
  pets: any[] = [];
  loading = false;

  selectedType = '';
  selectedListingType = '';
  petTypes = [
    { value: 'Dog', label: 'Chó' },
    { value: 'Cat', label: 'Mèo' },
    { value: 'Bird', label: 'Chim' },
    { value: 'Fish', label: 'Cá' },
    { value: 'Hamster', label: 'Hamster' },
    { value: 'Rabbit', label: 'Thỏ' },
    { value: 'Other', label: 'Khác' }
  ];
  searchText = '';
  listingTypes = [
    { value: 'Sale', label: 'Đang bán' },
    { value: 'Adoption', label: 'Nhận nuôi' }
  ];

  constructor(
    private apiService: ApiService,
    public auth: AuthService,
    private cartService: CartService,
    private toastr: ToastrService,
    private router: Router
  ) {}

  getImg(path: string): string { return this.apiService.getImageUrl(path); }

  ngOnInit(): void {
    this.loadPets();
  }

  loadPets(): void {
    this.loading = true;
    const params: any = { limit: 200 };
    if (this.selectedType) params.type = this.selectedType;
    if (this.selectedListingType) params.listingType = this.selectedListingType;
    if (this.searchText) params.search = this.searchText;

    this.apiService.getPets(params).subscribe({
      next: (res: any) => {
        this.pets = res.data?.pets || res.data || [];
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  filterByType(type: string): void {
    this.selectedType = this.selectedType === type ? '' : type;
    this.loadPets();
  }

  filterByListingType(type: string): void {
    this.selectedListingType = this.selectedListingType === type ? '' : type;
    this.loadPets();
  }

  formatPrice(price: number): string {
    if (!price) return 'Miễn phí';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
  }

  getListingTypeLabel(type: string): string {
    const m: any = { Sale: 'Đang bán', Adoption: 'Nhận nuôi', None: '' };
    return m[type] || '';
  }

  getAgeLabel(pet: any): string {
    if (!pet.age) return '';
    const unit = pet.ageUnit === 'years' ? 'tuổi' : 'tháng';
    return `${pet.age} ${unit}`;
  }

  getTypeName(type: string): string {
    const m: any = { Dog: 'Chó', Cat: 'Mèo', Bird: 'Chim', Fish: 'Cá', Hamster: 'Hamster', Rabbit: 'Thỏ', Other: 'Khác' };
    return m[type] || type;
  }

  getGenderName(g: string): string {
    const m: any = { Male: 'Đực', Female: 'Cái', Unknown: 'Chưa rõ' };
    return m[g] || g;
  }

  addToCart(pet: any): void {
    this.cartService.addPetToCart(pet);
    this.toastr.success(`Đã thêm ${pet.name} vào giỏ hàng!`, 'Thành công');
  }

  viewPetDetail(petId: string): void {
    this.router.navigate(['/pets', petId]);
  }

  isPetInCart(pet: any): boolean {
    return this.cartService.items.some(i => i.product._id === pet._id && i.itemType === 'pet');
  }
}
