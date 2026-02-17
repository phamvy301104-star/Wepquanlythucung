import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from './services/auth.service';
import { CartService } from './services/cart.service';
import { ApiService } from './services/api.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  mobileMenuOpen = false;
  userDropdownOpen = false;

  settings: any = {};
  serviceCategories: any[] = [];

  constructor(public auth: AuthService, public cart: CartService, private apiService: ApiService) {}

  ngOnInit(): void {
    this.apiService.getSettings().subscribe({
      next: (res: any) => this.settings = res.data || res,
      error: () => {}
    });
    this.apiService.getServiceCategories().subscribe({
      next: (res: any) => this.serviceCategories = (res.data?.items || res.data || res || []).slice(0, 5),
      error: () => {}
    });
  }

  getImg(path: string): string { return this.apiService.getImageUrl(path); }

  toggleMenu(): void {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  toggleUserDropdown(): void {
    this.userDropdownOpen = !this.userDropdownOpen;
  }

  logout(): void {
    this.auth.logout();
    this.userDropdownOpen = false;
  }
}
