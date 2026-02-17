import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface CartItem {
  product: any;
  quantity: number;
  itemType?: 'product' | 'pet';
}

@Injectable({ providedIn: 'root' })
export class CartService {
  private cartItems: CartItem[] = [];
  private cartSubject = new BehaviorSubject<CartItem[]>([]);
  public cart$ = this.cartSubject.asObservable();

  constructor() {
    const saved = localStorage.getItem('cart');
    if (saved) {
      this.cartItems = JSON.parse(saved);
      this.cartSubject.next(this.cartItems);
    }
  }

  get items(): CartItem[] {
    return this.cartItems;
  }

  get totalItems(): number {
    return this.cartItems.reduce((sum, item) => sum + item.quantity, 0);
  }

  get totalAmount(): number {
    return this.cartItems.reduce((sum, item) => {
      const price = item.itemType === 'pet' ? (item.product.listingPrice || 0) : (item.product.price || 0);
      return sum + price * item.quantity;
    }, 0);
  }

  addToCart(product: any, quantity: number = 1): void {
    const existing = this.cartItems.find(i => i.product._id === product._id);
    if (existing) {
      existing.quantity += quantity;
    } else {
      this.cartItems.push({ product, quantity });
    }
    this.save();
  }

  addPetToCart(pet: any): void {
    const existing = this.cartItems.find(i => i.product._id === pet._id && i.itemType === 'pet');
    if (existing) return; // pet can only be added once
    this.cartItems.push({ product: pet, quantity: 1, itemType: 'pet' });
    this.save();
  }

  updateQuantity(productId: string, quantity: number): void {
    const item = this.cartItems.find(i => i.product._id === productId);
    if (item) {
      if (quantity <= 0) {
        this.removeItem(productId);
      } else {
        item.quantity = quantity;
        this.save();
      }
    }
  }

  removeItem(productId: string): void {
    this.cartItems = this.cartItems.filter(i => i.product._id !== productId);
    this.save();
  }

  clearCart(): void {
    this.cartItems = [];
    this.save();
  }

  private save(): void {
    localStorage.setItem('cart', JSON.stringify(this.cartItems));
    this.cartSubject.next([...this.cartItems]);
  }
}
