import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../services/api.service';
import { ToastrService } from 'ngx-toastr';
import { SafeUrlPipe } from '../../../pipes/safe-url.pipe';

@Component({
  selector: 'app-admin-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, SafeUrlPipe],
  templateUrl: './admin-settings.component.html',
  styleUrl: './admin-settings.component.scss'
})
export class AdminSettingsComponent implements OnInit {
  settings: any = {
    storeName: '',
    storeDescription: '',
    address: '',
    phone: '',
    email: '',
    workingHours: '',
    facebook: '',
    instagram: '',
    tiktok: '',
    youtube: '',
    zalo: '',
    mapEmbedUrl: '',
    shippingStandardPrice: 30000,
    shippingExpressPrice: 50000,
    freeShipStandardThreshold: 500000,
    freeShipExpressThreshold: 1000000,
    shippingPolicy: '',
    returnPolicy: '',
    codEnabled: true,
    codDescription: '',
    bankTransferEnabled: true,
    bankName: '',
    bankAccountNumber: '',
    bankAccountHolder: '',
    bankBranch: '',
    bankDescription: '',
    bankQrImage: ''
  };

  loading = true;
  saving = false;
  uploadingQr = false;
  activeTab = 'contact';

  constructor(
    private api: ApiService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadSettings();
  }

  loadSettings(): void {
    this.loading = true;
    this.api.getSettings().subscribe({
      next: (res: any) => {
        if (res.success) {
          this.settings = { ...this.settings, ...res.data };
        }
        this.loading = false;
      },
      error: () => {
        this.toastr.error('Không thể tải cài đặt');
        this.loading = false;
      }
    });
  }

  saveSettings(): void {
    this.saving = true;
    this.api.updateSettings(this.settings).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.toastr.success('Cập nhật cài đặt thành công!');
          this.settings = { ...this.settings, ...res.data };
        }
        this.saving = false;
      },
      error: () => {
        this.toastr.error('Lỗi khi cập nhật cài đặt');
        this.saving = false;
      }
    });
  }

  onQrFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || !input.files[0]) return;

    const file = input.files[0];
    if (file.size > 5 * 1024 * 1024) {
      this.toastr.warning('File quá lớn, tối đa 5MB');
      return;
    }

    this.uploadingQr = true;
    this.api.uploadFile(file, 'settings').subscribe({
      next: (res: any) => {
        this.uploadingQr = false;
        if (res.success) {
          this.settings.bankQrImage = res.data.url;
          this.toastr.success('Upload mã QR thành công!');
        }
      },
      error: () => {
        this.uploadingQr = false;
        this.toastr.error('Upload mã QR thất bại');
      }
    });
  }

  removeQrImage(): void {
    this.settings.bankQrImage = '';
  }

  getImg(path: string): string {
    return this.api.getImageUrl(path);
  }

  formatPrice(val: number): string {
    return new Intl.NumberFormat('vi-VN').format(val || 0);
  }
}
