import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit {
  featuredServices: any[] = [];
  featuredProducts: any[] = [];

  stats = [
    { icon: 'bi bi-stars', value: '10+', label: 'Năm kinh nghiệm' },
    { icon: 'bi bi-people', value: '50+', label: 'Bác sĩ & Nhân viên' },
    { icon: 'bi bi-heart', value: '10K+', label: 'Thú cưng được chăm sóc' },
    { icon: 'bi bi-award', value: '100+', label: 'Dịch vụ' }
  ];

  whyChooseUs = [
    {
      icon: 'bi bi-award',
      title: 'Bác sĩ thú y giỏi',
      description: 'Đội ngũ bác sĩ thú y được đào tạo bài bản, nhiều năm kinh nghiệm'
    },
    {
      icon: 'bi bi-gem',
      title: 'Sản phẩm an toàn',
      description: 'Sử dụng sản phẩm chính hãng, an toàn cho thú cưng'
    },
    {
      icon: 'bi bi-palette',
      title: 'Không gian thoáng mát',
      description: 'Thiết kế hiện đại, sạch sẽ, thoải mái cho thú cưng'
    },
    {
      icon: 'bi bi-heart',
      title: 'Yêu thú cưng',
      description: 'Chăm sóc thú cưng với tình yêu thương như chính boss của mình'
    }
  ];

  defaultServices = [
    {
      name: 'Tắm gội & Vệ sinh',
      description: 'Dịch vụ tắm gội, vệ sinh tai, cắt móng cho thú cưng',
      price: '150.000đ',
      duration: '45 phút',
      image: 'https://images.unsplash.com/photo-1516734212186-a967f81ad0d7?ixlib=rb-4.0.3&auto=format&fit=crop&w=600&q=80'
    },
    {
      name: 'Cắt tỉa & Tạo kiểu lông',
      description: 'Cắt tỉa, tạo kiểu lông chuyên nghiệp theo nhiều phong cách',
      price: '300.000đ',
      duration: '90 phút',
      image: 'https://images.unsplash.com/photo-1591946614720-90a587da4a36?ixlib=rb-4.0.3&auto=format&fit=crop&w=600&q=80'
    },
    {
      name: 'Khách sạn thú cưng',
      description: 'Gửi thú cưng an toàn, thoải mái khi bạn đi công tác/du lịch',
      price: '200.000đ/ngày',
      duration: '24h',
      image: 'https://images.unsplash.com/photo-1601758228041-f3b2795255f1?ixlib=rb-4.0.3&auto=format&fit=crop&w=600&q=80'
    },
    {
      name: 'Khám sức khỏe thú y',
      description: 'Khám tổng quát, chẩn đoán bệnh và tư vấn sức khỏe cho thú cưng',
      price: '200.000đ',
      duration: '30 phút',
      image: 'https://images.unsplash.com/photo-1628009368231-7bb7cfcb0def?ixlib=rb-4.0.3&auto=format&fit=crop&w=600&q=80'
    },
    {
      name: 'Tiêm phòng vaccine',
      description: 'Tiêm phòng đầy đủ các loại vaccine cần thiết cho thú cưng',
      price: '150.000đ',
      duration: '15 phút',
      image: 'https://images.unsplash.com/photo-1587300003388-59208cc962cb?ixlib=rb-4.0.3&auto=format&fit=crop&w=600&q=80'
    },
    {
      name: 'Spa cao cấp',
      description: 'Trọn gói tắm, cắt tỉa, massage và chăm sóc đặc biệt',
      price: '400.000đ',
      duration: '120 phút',
      image: 'https://images.unsplash.com/photo-1548199973-03cce0bbc87b?ixlib=rb-4.0.3&auto=format&fit=crop&w=600&q=80'
    }
  ];

  defaultProducts = [
    {
      name: 'Hạt Royal Canin cho chó',
      price: '450.000đ',
      originalPrice: '560.000đ',
      discount: '-20%',
      image: 'https://images.unsplash.com/photo-1589924691995-400dc9ecc119?ixlib=rb-4.0.3&auto=format&fit=crop&w=400&q=80'
    },
    {
      name: 'Sữa tắm Bio cho mèo',
      price: '180.000đ',
      originalPrice: null,
      discount: null,
      image: 'https://images.unsplash.com/photo-1583337130417-3346a1be7dee?ixlib=rb-4.0.3&auto=format&fit=crop&w=400&q=80'
    },
    {
      name: 'Bóng đồ chơi cho chó',
      price: '85.000đ',
      originalPrice: '100.000đ',
      discount: '-15%',
      image: 'https://images.unsplash.com/photo-1535294435445-d7249524ef2e?ixlib=rb-4.0.3&auto=format&fit=crop&w=400&q=80'
    },
    {
      name: 'Vòng cổ thời trang',
      price: '120.000đ',
      originalPrice: null,
      discount: null,
      image: 'https://images.unsplash.com/photo-1567612529009-afe25813a308?ixlib=rb-4.0.3&auto=format&fit=crop&w=400&q=80'
    }
  ];

  testimonials = [
    {
      name: 'Ngọc Anh',
      title: 'Khách hàng thân thiết',
      text: 'Dịch vụ tuyệt vời! Bé cún nhà mình được chăm sóc rất tốt. Nhân viên rất yêu thú cưng và nhiệt tình. Chắc chắn sẽ quay lại!',
      avatar: 'https://randomuser.me/api/portraits/women/44.jpg',
      stars: [1, 2, 3, 4, 5],
      halfStar: false
    },
    {
      name: 'Minh Tuấn',
      title: 'Doanh nhân',
      text: 'Không gian spa thú cưng rất sạch sẽ và thoáng mát. Bé mèo nhà mình được tắm gội, cắt lông rất đẹp. Highly recommend!',
      avatar: 'https://randomuser.me/api/portraits/men/32.jpg',
      stars: [1, 2, 3, 4, 5],
      halfStar: false
    },
    {
      name: 'Thu Hà',
      title: 'Giáo viên',
      text: 'Gửi boss ở khách sạn thú cưng khi đi du lịch, được cập nhật hình ảnh hàng ngày. Rất yên tâm. Cảm ơn PetCare nhiều!',
      avatar: 'https://randomuser.me/api/portraits/women/68.jpg',
      stars: [1, 2, 3, 4],
      halfStar: true
    }
  ];

  constructor(private api: ApiService) {}

  getImg(path: string): string { return this.api.getImageUrl(path); }

  ngOnInit(): void {
    this.loadFeaturedData();
  }

  loadFeaturedData(): void {
    this.api.get('/services?limit=6&isFeatured=true').subscribe({
      next: (res: any) => {
        this.featuredServices = res.data?.items || res.data || [];
      },
      error: () => {
        this.featuredServices = [];
      }
    });

    this.api.get('/products?limit=8&isFeatured=true').subscribe({
      next: (res: any) => {
        this.featuredProducts = res.data?.items || res.data || [];
      },
      error: () => {
        this.featuredProducts = [];
      }
    });
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
  }
}
