import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CartService, CartItem } from '../../services/cart.service';
import { ApiService } from '../../services/api.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.scss'
})
export class CartComponent implements OnInit, OnDestroy {
  cartItems: CartItem[] = [];
  private sub!: Subscription;

  shippingFee = 30000;

  constructor(public cartService: CartService, private apiService: ApiService) {}

  getImg(path: string): string { return this.apiService.getImageUrl(path); }

  ngOnInit(): void {
    this.sub = this.cartService.cart$.subscribe(items => {
      this.cartItems = items;
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  get subtotal(): number {
    return this.cartService.totalAmount;
  }

  get total(): number {
    return this.cartItems.length > 0 ? this.subtotal + this.shippingFee : 0;
  }

  increaseQty(item: CartItem): void {
    this.cartService.updateQuantity(item.product._id, item.quantity + 1);
  }

  decreaseQty(item: CartItem): void {
    this.cartService.updateQuantity(item.product._id, item.quantity - 1);
  }

  removeItem(item: CartItem): void {
    this.cartService.removeItem(item.product._id);
  }

  clearCart(): void {
    this.cartService.clearCart();
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
  }

  getItemPrice(item: CartItem): number {
    return item.itemType === 'pet' ? (item.product.listingPrice || 0) : item.product.price;
  }
}
