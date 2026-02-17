import { Component, NgZone, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../../environments/environment';

declare const google: any;

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements AfterViewInit, OnDestroy {
  email = '';
  password = '';
  showPassword = false;
  loading = false;
  private gsiInitialized = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private toastr: ToastrService,
    private ngZone: NgZone
  ) {}

  ngAfterViewInit(): void {
    this.loadGoogleScript();
  }

  ngOnDestroy(): void {
    // Cleanup if needed
  }

  private loadGoogleScript(): void {
    // Check if script already loaded
    if (typeof google !== 'undefined' && google.accounts) {
      this.initializeGSI();
      return;
    }

    // Check if script tag already exists
    const existing = document.querySelector('script[src*="accounts.google.com/gsi/client"]');
    if (existing) {
      existing.addEventListener('load', () => this.initializeGSI());
      // If already loaded
      if (typeof google !== 'undefined' && google.accounts) {
        this.initializeGSI();
      }
      return;
    }

    const script = document.createElement('script');
    script.src = 'https://accounts.google.com/gsi/client';
    script.async = true;
    script.defer = true;
    script.onload = () => this.initializeGSI();
    document.head.appendChild(script);
  }

  private initializeGSI(): void {
    if (this.gsiInitialized) return;
    this.gsiInitialized = true;

    try {
      google.accounts.id.initialize({
        client_id: environment.googleClientId,
        callback: (response: any) => this.handleGoogleCredential(response),
        auto_select: false,
        cancel_on_tap_outside: true
      });

      // Render the Google sign-in button
      const btnContainer = document.getElementById('google-signin-btn');
      if (btnContainer) {
        google.accounts.id.renderButton(btnContainer, {
          type: 'standard',
          theme: 'outline',
          size: 'large',
          text: 'signin_with',
          shape: 'rectangular',
          logo_alignment: 'left',
          width: '100%'
        });
      }
    } catch (e) {
      console.error('GSI init error:', e);
    }
  }

  private handleGoogleCredential(response: any): void {
    if (!response.credential) {
      this.ngZone.run(() => {
        this.toastr.error('Không nhận được thông tin từ Google');
      });
      return;
    }

    this.ngZone.run(() => {
      this.loading = true;
      this.authService.googleLogin(response.credential).subscribe({
        next: () => {
          this.toastr.success('Đăng nhập Google thành công!');
          this.router.navigate(['/']);
        },
        error: (err) => {
          this.loading = false;
          this.toastr.error(err.error?.message || 'Đăng nhập Google thất bại');
        }
      });
    });
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  login(): void {
    if (!this.email || !this.password) {
      this.toastr.warning('Vui lòng nhập đầy đủ thông tin');
      return;
    }

    this.loading = true;
    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: () => {
        this.toastr.success('Đăng nhập thành công!');
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.loading = false;
        this.toastr.error(err.error?.message || 'Đăng nhập thất bại. Vui lòng thử lại.');
      }
    });
  }

  googleLogin(): void {
    // Fallback: trigger One Tap prompt if rendered button not visible
    try {
      google.accounts.id.prompt();
    } catch (e) {
      this.toastr.error('Google Sign-In chưa sẵn sàng. Vui lòng thử lại.');
    }
  }

  facebookLogin(): void {
    this.toastr.info('Tính năng đăng nhập Facebook đang được phát triển.');
  }
}
