import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  fullName = '';
  email = '';
  phoneNumber = '';
  password = '';
  confirmPassword = '';
  showPassword = false;
  showConfirmPassword = false;
  acceptTerms = false;
  loading = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPassword(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  register(): void {
    if (!this.fullName || !this.email || !this.password || !this.confirmPassword) {
      this.toastr.warning('Vui lòng nhập đầy đủ thông tin');
      return;
    }

    if (this.password.length < 6) {
      this.toastr.warning('Mật khẩu phải có ít nhất 6 ký tự');
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.toastr.warning('Mật khẩu xác nhận không khớp');
      return;
    }

    if (!this.acceptTerms) {
      this.toastr.warning('Vui lòng đồng ý với điều khoản sử dụng');
      return;
    }

    this.loading = true;
    this.authService.register({
      fullName: this.fullName,
      email: this.email,
      password: this.password,
      phoneNumber: this.phoneNumber || undefined
    }).subscribe({
      next: () => {
        this.toastr.success('Đăng ký thành công! Chào mừng bạn đến với PetCare.');
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.loading = false;
        this.toastr.error(err.error?.message || 'Đăng ký thất bại. Vui lòng thử lại.');
      }
    });
  }
}
