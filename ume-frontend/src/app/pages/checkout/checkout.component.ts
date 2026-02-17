import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CartService, CartItem } from '../../services/cart.service';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.scss'
})
export class CheckoutComponent implements OnInit {
  cartItems: CartItem[] = [];
  shippingFee = 30000;
  submitting = false;
  shippingMethod = 'standard';
  sameAsUser = true;

  // Settings from admin
  settings: any = {};

  // Shipping address
  shippingAddress = {
    fullName: '',
    phone: '',
    email: '',
    address: '',
    ward: '',
    district: '',
    city: ''
  };

  paymentMethod = 'COD';
  notes = '';
  promoCode = '';
  discount = 0;
  promoApplied = false;
  promoName = '';
  applyingPromo = false;

  constructor(
    public cartService: CartService,
    private apiService: ApiService,
    private authService: AuthService,
    private toastr: ToastrService,
    private router: Router
  ) {}

  getImg(path: string): string { return this.apiService.getImageUrl(path); }

  ngOnInit(): void {
    this.cartItems = this.cartService.items;
    if (this.cartItems.length === 0) {
      this.router.navigate(['/cart']);
      return;
    }

    // Load settings
    this.apiService.getSettings().subscribe({
      next: (res: any) => {
        this.settings = res.data || res;
        // Apply shipping prices from settings
        this.shippingFee = this.shippingMethod === 'express'
          ? (this.settings.shippingExpressPrice || 50000)
          : (this.settings.shippingStandardPrice || 30000);
        // Set default payment method
        if (this.settings.codEnabled === false && this.settings.bankTransferEnabled !== false) {
          this.paymentMethod = 'BankTransfer';
        }
      },
      error: () => {}
    });

    // Pre-fill from user profile
    const user = this.authService.currentUser;
    if (user) {
      this.shippingAddress.fullName = user.fullName || '';
      this.shippingAddress.phone = user.phoneNumber || '';
      this.shippingAddress.email = user.email || '';
      if (user.address) {
        this.shippingAddress.address = user.address.street || '';
        this.shippingAddress.ward = user.address.ward || '';
        this.shippingAddress.district = user.address.district || '';
        this.shippingAddress.city = user.address.city || '';
      }
    }
  }

  get standardPrice(): number {
    return this.settings.shippingStandardPrice || 30000;
  }
  get expressPrice(): number {
    return this.settings.shippingExpressPrice || 50000;
  }
  get freeShipStandard(): number {
    return this.settings.freeShipStandardThreshold || 500000;
  }
  get freeShipExpress(): number {
    return this.settings.freeShipExpressThreshold || 1000000;
  }

  get shippingPolicyLines(): string[] {
    if (this.settings.shippingPolicy) {
      return this.settings.shippingPolicy.split('\n').filter((l: string) => l.trim());
    }
    return [];
  }

  get subtotal(): number {
    return this.cartService.totalAmount;
  }

  get total(): number {
    return this.subtotal + this.shippingFee - this.discount;
  }

  selectShippingMethod(method: string): void {
    this.shippingMethod = method;
    this.shippingFee = method === 'express' ? this.expressPrice : this.standardPrice;
  }

  applyPromoCode(): void {
    if (!this.promoCode.trim()) {
      this.toastr.warning('Vui lòng nhập mã khuyến mãi');
      return;
    }
    this.applyingPromo = true;
    this.apiService.validatePromoCode(this.promoCode, this.subtotal).subscribe({
      next: (res: any) => {
        this.applyingPromo = false;
        if (res.success) {
          this.discount = res.data.discount || 0;
          this.promoApplied = true;
          this.promoName = res.data.promotion?.name || this.promoCode;
          if (res.data.promotion?.type === 'FreeShipping') {
            this.discount = this.shippingFee;
            this.toastr.success('Áp dụng miễn phí vận chuyển thành công!');
          } else {
            this.toastr.success(`Áp dụng mã "${this.promoCode.toUpperCase()}" thành công! Giảm ${this.formatPrice(this.discount)}`);
          }
        }
      },
      error: (err: any) => {
        this.applyingPromo = false;
        this.discount = 0;
        this.promoApplied = false;
        this.promoName = '';
        this.toastr.error(err.error?.message || 'Mã khuyến mãi không hợp lệ');
      }
    });
  }

  removePromoCode(): void {
    this.promoCode = '';
    this.discount = 0;
    this.promoApplied = false;
    this.promoName = '';
    this.toastr.info('Đã hủy mã khuyến mãi');
  }

  getItemPrice(item: CartItem): number {
    return item.itemType === 'pet' ? (item.product.listingPrice || 0) : item.product.price;
  }

  isFormValid(): boolean {
    return !!(
      this.shippingAddress.fullName &&
      this.shippingAddress.phone &&
      this.shippingAddress.email &&
      this.shippingAddress.address
    );
  }

  placeOrder(): void {
    if (!this.isFormValid()) {
      this.toastr.warning('Vui lòng điền đầy đủ thông tin giao hàng', 'Thông báo');
      return;
    }

    this.submitting = true;
    const orderData: any = {
      items: this.cartItems.map(item => {
        if (item.itemType === 'pet') {
          return { petId: item.product._id, quantity: 1 };
        }
        return { productId: item.product._id, quantity: item.quantity };
      }),
      shippingAddress: this.shippingAddress,
      shippingFee: this.shippingFee,
      paymentMethod: this.paymentMethod,
      notes: this.notes
    };
    if (this.promoApplied && this.promoCode) {
      orderData.promotionCode = this.promoCode;
    }

    this.apiService.createOrder(orderData).subscribe({
      next: (res: any) => {
        this.submitting = false;
        const order = res.data || res;
        this.cartService.clearCart();
        
        // Store order data and redirect to success page
        sessionStorage.setItem('newOrder', JSON.stringify({
          id: order._id,
          code: order.orderCode,
          total: order.total,
          items: order.items,
          shippingAddress: order.shippingAddress,
          paymentMethod: order.paymentMethod
        }));
        
        this.router.navigate(['/checkout-success']);
      },
      error: (err: any) => {
        this.submitting = false;
        this.toastr.error(err.error?.message || 'Đặt hàng thất bại', 'Lỗi');
      }
    });
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
  }
}
