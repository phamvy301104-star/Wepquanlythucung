import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ApiService } from '../../services/api.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  profile: any = {};
  loading = true;
  saving = false;

  // Tabs
  activeTab = 'info'; // info | password | orders | appointments | pets

  // Stats
  orderCount = 0;
  appointmentCount = 0;
  petCount = 0;

  // Change password
  currentPassword = '';
  newPassword = '';
  confirmPassword = '';
  changingPassword = false;

  avatarPreview: string | null = null;
  selectedAvatarFile: File | null = null;

  // Orders, appointments, pets lists for tabs
  orders: any[] = [];
  appointments: any[] = [];
  pets: any[] = [];
  loadingOrders = false;
  loadingAppointments = false;
  loadingPets = false;

  constructor(
    private authService: AuthService,
    private apiService: ApiService,
    private toastr: ToastrService
  ) {}

  getImg(path: string): string { return this.apiService.getImageUrl(path); }

  ngOnInit(): void {
    this.loadProfile();
    this.loadStats();
  }

  loadProfile(): void {
    this.loading = true;
    this.authService.getProfile().subscribe({
      next: (res: any) => {
        this.profile = res.data || res;
        if (!this.profile.address) this.profile.address = {};
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  loadStats(): void {
    this.apiService.getMyOrders().subscribe({
      next: (res: any) => {
        const data = res.data || res;
        this.orderCount = Array.isArray(data) ? data.length : (data.orders?.length || 0);
      }
    });
    this.apiService.getMyAppointments().subscribe({
      next: (res: any) => {
        const data = res.data || res;
        this.appointmentCount = Array.isArray(data) ? data.length : (data.appointments?.length || 0);
      }
    });
    this.apiService.getMyPets().subscribe({
      next: (res: any) => {
        const data = res.data || res;
        this.petCount = Array.isArray(data) ? data.length : 0;
      }
    });
  }

  switchTab(tab: string): void {
    this.activeTab = tab;
    if (tab === 'orders' && this.orders.length === 0) this.loadOrders();
    if (tab === 'appointments' && this.appointments.length === 0) this.loadAppointments();
    if (tab === 'pets' && this.pets.length === 0) this.loadPets();
  }

  loadOrders(): void {
    this.loadingOrders = true;
    this.apiService.getMyOrders().subscribe({
      next: (res: any) => {
        const data = res.data || res;
        this.orders = Array.isArray(data) ? data : (data.orders || []);
        this.loadingOrders = false;
      },
      error: () => { this.loadingOrders = false; }
    });
  }

  loadAppointments(): void {
    this.loadingAppointments = true;
    this.apiService.getMyAppointments().subscribe({
      next: (res: any) => {
        const data = res.data || res;
        this.appointments = Array.isArray(data) ? data : (data.appointments || []);
        this.loadingAppointments = false;
      },
      error: () => { this.loadingAppointments = false; }
    });
  }

  loadPets(): void {
    this.loadingPets = true;
    this.apiService.getMyPets().subscribe({
      next: (res: any) => {
        const data = res.data || res;
        this.pets = Array.isArray(data) ? data : [];
        this.loadingPets = false;
      },
      error: () => { this.loadingPets = false; }
    });
  }

  getInitials(): string {
    if (!this.profile.fullName) return 'U';
    return this.profile.fullName.split(' ').map((w: string) => w.charAt(0)).join('').toUpperCase().substring(0, 2);
  }

  getFullAddress(): string {
    if (!this.profile.address) return '';
    if (typeof this.profile.address === 'string') return this.profile.address;
    const parts = [this.profile.address.street, this.profile.address.ward, this.profile.address.district, this.profile.address.city].filter(Boolean);
    return parts.join(', ');
  }

  getRoleBadges(): string[] {
    const roles: string[] = [];
    if (this.profile.role === 'admin') roles.push('Admin');
    roles.push('Khách hàng');
    return roles;
  }

  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.selectedAvatarFile = input.files[0];
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.avatarPreview = e.target.result;
      };
      reader.readAsDataURL(input.files[0]);
    }
  }

  saveProfile(): void {
    this.saving = true;

    if (this.selectedAvatarFile) {
      this.apiService.uploadFile(this.selectedAvatarFile, 'avatars').subscribe({
        next: (uploadRes: any) => {
          this.profile.avatarUrl = uploadRes.data?.url || uploadRes.url;
          this.updateProfileData();
        },
        error: () => {
          this.updateProfileData();
        }
      });
    } else {
      this.updateProfileData();
    }
  }

  private updateProfileData(): void {
    const data = {
      fullName: this.profile.fullName,
      phoneNumber: this.profile.phoneNumber,
      gender: this.profile.gender,
      dateOfBirth: this.profile.dateOfBirth,
      avatarUrl: this.profile.avatarUrl,
      address: this.profile.address
    };

    this.authService.updateProfile(data).subscribe({
      next: () => {
        this.saving = false;
        this.selectedAvatarFile = null;
        this.avatarPreview = null;
        this.toastr.success('Cập nhật hồ sơ thành công!', 'Thành công');
        this.loadProfile();
      },
      error: (err: any) => {
        this.saving = false;
        this.toastr.error(err.error?.message || 'Cập nhật thất bại', 'Lỗi');
      }
    });
  }

  changePassword(): void {
    if (!this.currentPassword || !this.newPassword) {
      this.toastr.warning('Vui lòng nhập đầy đủ thông tin', 'Thông báo');
      return;
    }
    if (this.newPassword !== this.confirmPassword) {
      this.toastr.warning('Mật khẩu xác nhận không khớp', 'Thông báo');
      return;
    }
    if (this.newPassword.length < 6) {
      this.toastr.warning('Mật khẩu mới phải có ít nhất 6 ký tự', 'Thông báo');
      return;
    }

    this.changingPassword = true;
    this.authService.changePassword(this.currentPassword, this.newPassword).subscribe({
      next: () => {
        this.changingPassword = false;
        this.currentPassword = '';
        this.newPassword = '';
        this.confirmPassword = '';
        this.toastr.success('Đổi mật khẩu thành công!', 'Thành công');
      },
      error: (err: any) => {
        this.changingPassword = false;
        this.toastr.error(err.error?.message || 'Đổi mật khẩu thất bại', 'Lỗi');
      }
    });
  }

  getStatusLabel(status: string): string {
    const map: any = {
      pending: 'Chờ xử lý', confirmed: 'Đã xác nhận', processing: 'Đang xử lý',
      shipped: 'Đang giao', delivered: 'Đã giao', completed: 'Hoàn thành',
      cancelled: 'Đã hủy', scheduled: 'Đã lên lịch', 'in-progress': 'Đang thực hiện'
    };
    return map[status] || status;
  }

  getStatusClass(status: string): string {
    const map: any = {
      pending: 'warning', confirmed: 'info', processing: 'primary',
      shipped: 'info', delivered: 'success', completed: 'success',
      cancelled: 'danger', scheduled: 'info', 'in-progress': 'primary'
    };
    return map[status] || 'secondary';
  }
}
