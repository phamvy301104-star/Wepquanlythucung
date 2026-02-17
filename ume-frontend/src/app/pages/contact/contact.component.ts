import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { ApiService } from '../../services/api.service';
import { SafeUrlPipe } from '../../pipes/safe-url.pipe';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, FormsModule, SafeUrlPipe],
  templateUrl: './contact.component.html',
  styleUrl: './contact.component.scss'
})
export class ContactComponent implements OnInit {
  contactForm = {
    fullName: '',
    phone: '',
    email: '',
    subject: '',
    message: ''
  };
  sending = false;

  // Settings from API
  settings: any = {
    address: '',
    phone: '',
    email: '',
    workingHours: '',
    facebook: '',
    instagram: '',
    tiktok: '',
    youtube: '',
    zalo: '',
    mapEmbedUrl: ''
  };

  subjects = [
    'Tư vấn dịch vụ',
    'Hỏi về sản phẩm',
    'Đặt lịch hẹn',
    'Khiếu nại / Góp ý',
    'Hợp tác kinh doanh',
    'Khác'
  ];

  constructor(
    private toastr: ToastrService,
    private api: ApiService
  ) {}

  ngOnInit(): void {
    this.loadSettings();
  }

  loadSettings(): void {
    this.api.getSettings().subscribe({
      next: (res: any) => {
        if (res.success) {
          this.settings = res.data;
        }
      }
    });
  }

  sendMessage(): void {
    if (!this.contactForm.fullName || !this.contactForm.phone || !this.contactForm.email || !this.contactForm.message) {
      this.toastr.warning('Vui lòng điền đầy đủ thông tin bắt buộc', 'Thông báo');
      return;
    }

    this.sending = true;

    // Simulate sending (can integrate with backend API later)
    setTimeout(() => {
      this.sending = false;
      this.toastr.success('Tin nhắn đã được gửi thành công! Chúng tôi sẽ phản hồi sớm nhất.', 'Thành công');
      this.contactForm = { fullName: '', phone: '', email: '', subject: '', message: '' };
    }, 1500);
  }
}
