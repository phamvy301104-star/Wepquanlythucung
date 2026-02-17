import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-checkout-success',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './checkout-success.component.html',
  styleUrl: './checkout-success.component.scss'
})
export class CheckoutSuccessComponent implements OnInit {
  order: any = null;

  constructor(private router: Router) {}

  ngOnInit(): void {
    const orderData = sessionStorage.getItem('newOrder');
    if (!orderData) {
      this.router.navigate(['/']);
      return;
    }
    this.order = JSON.parse(orderData);
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
  }

  continueShopping(): void {
    sessionStorage.removeItem('newOrder');
    this.router.navigate(['/products']);
  }

  goHome(): void {
    sessionStorage.removeItem('newOrder');
    this.router.navigate(['/']);
  }
}
