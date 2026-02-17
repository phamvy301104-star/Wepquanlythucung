import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './booking.component.html',
  styleUrl: './booking.component.scss'
})
export class BookingComponent implements OnInit {
  currentStep = 1;
  totalSteps = 5;

  // Step 1
  services: any[] = [];
  selectedServices: any[] = [];

  // Step 2
  availableStaff: any[] = [];
  selectedStaff: any = null;

  // Step 3
  selectedDate = '';
  selectedTime = '';
  availableTimeSlots: string[] = [];
  minDate = '';

  // Step 4
  myPets: any[] = [];
  selectedPet: any = null;
  skipPet = false;
  describePet = false;
  petDescription = { name: '', type: '', gender: '', age: 0, weight: 0, notes: '' };
  petGenders = [
    { value: 'Male', label: 'Đực' },
    { value: 'Female', label: 'Cái' },
    { value: 'Neutered', label: 'Đã triệt sản' }
  ];
  petTypes = [
    { value: 'Dog', label: 'Chó' },
    { value: 'Cat', label: 'Mèo' },
    { value: 'Bird', label: 'Chim' },
    { value: 'Hamster', label: 'Hamster' },
    { value: 'Rabbit', label: 'Thỏ' },
    { value: 'Fish', label: 'Cá' },
    { value: 'Other', label: 'Khác' }
  ];

  // Step 5
  notes = '';
  submitting = false;
  showSuccessModal = false;
  bookedCode = '';

  loading = false;

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private toastr: ToastrService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  getImg(path: string): string { return this.apiService.getImageUrl(path); }

  ngOnInit(): void {
    const today = new Date();
    this.minDate = today.toISOString().split('T')[0];
    this.selectedDate = this.minDate;
    this.loadServices();
    this.loadMyPets();

    this.route.queryParams.subscribe(params => {
      if (params['service']) {
        this.preselectService(params['service']);
      }
    });
  }

  loadServices(): void {
    this.loading = true;
    this.apiService.getServices({ limit: 100 }).subscribe({
      next: (res: any) => {
        this.services = res.data?.items || res.data || [];
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  preselectService(serviceId: string): void {
    setTimeout(() => {
      const service = this.services.find(s => s._id === serviceId);
      if (service) {
        this.toggleService(service);
      }
    }, 500);
  }

  loadMyPets(): void {
    this.apiService.getMyPets().subscribe({
      next: (res: any) => {
        this.myPets = res.data?.items || res.data || [];
      }
    });
  }

  loadAvailableStaff(): void {
    const serviceIds = this.selectedServices.map(s => s._id);
    this.apiService.getAvailableStaff({ services: serviceIds.join(','), date: this.selectedDate }).subscribe({
      next: (res: any) => {
        this.availableStaff = res.data?.items || res.data || [];
      }
    });
  }

  toggleService(service: any): void {
    const idx = this.selectedServices.findIndex(s => s._id === service._id);
    if (idx > -1) {
      this.selectedServices.splice(idx, 1);
    } else {
      this.selectedServices.push(service);
    }
  }

  isServiceSelected(serviceId: string): boolean {
    return this.selectedServices.some(s => s._id === serviceId);
  }

  get totalAmount(): number {
    return this.selectedServices.reduce((sum, s) => sum + (s.price || 0), 0);
  }

  get totalDuration(): number {
    return this.selectedServices.reduce((sum, s) => sum + (s.durationMinutes || 0), 0);
  }

  generateTimeSlots(): void {
    this.availableTimeSlots = [];
    for (let h = 8; h <= 19; h++) {
      this.availableTimeSlots.push(`${h.toString().padStart(2, '0')}:00`);
      if (h < 19) {
        this.availableTimeSlots.push(`${h.toString().padStart(2, '0')}:30`);
      }
    }
  }

  onDateChange(): void {
    this.generateTimeSlots();
    this.selectedTime = '';
    if (this.selectedDate) {
      this.loadAvailableStaff();
    }
  }

  selectStaff(staff: any): void {
    this.selectedStaff = staff;
  }

  selectPet(pet: any): void {
    this.selectedPet = pet;
    this.skipPet = false;
    this.describePet = false;
  }

  toggleDescribePet(): void {
    this.describePet = !this.describePet;
    if (this.describePet) {
      this.selectedPet = null;
      this.skipPet = false;
    }
  }

  nextStep(): void {
    if (this.currentStep < this.totalSteps) {
      if (this.currentStep === 1 && this.selectedServices.length === 0) {
        this.toastr.warning('Vui lòng chọn ít nhất một dịch vụ', 'Thông báo');
        return;
      }
      if (this.currentStep === 3) {
        if (!this.selectedDate) {
          this.toastr.warning('Vui lòng chọn ngày', 'Thông báo');
          return;
        }
        if (!this.selectedTime) {
          this.toastr.warning('Vui lòng chọn giờ', 'Thông báo');
          return;
        }
      }
      this.currentStep++;
      if (this.currentStep === 2) this.loadAvailableStaff();
      if (this.currentStep === 3) this.generateTimeSlots();
    }
  }

  prevStep(): void {
    if (this.currentStep > 1) this.currentStep--;
  }

  submitBooking(): void {
    this.submitting = true;
    const data: any = {
      services: this.selectedServices.map(s => s._id),
      appointmentDate: this.selectedDate,
      startTime: this.selectedTime,
      notes: this.notes
    };
    if (this.selectedStaff) data.staffId = this.selectedStaff._id;
    if (this.selectedPet) data.petId = this.selectedPet._id;
    if (this.describePet && this.petDescription.name) {
      data.petDescription = this.petDescription;
    }

    this.apiService.createAppointment(data).subscribe({
      next: (res: any) => {
        this.submitting = false;
        this.bookedCode = res.data?.appointmentCode || res.data?.appointment?.appointmentCode || '';
        this.showSuccessModal = true;
      },
      error: (err: any) => {
        this.submitting = false;
        this.toastr.error(err.error?.message || 'Đặt lịch thất bại', 'Lỗi');
      }
    });
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
  }

  formatDuration(minutes: number): string {
    if (minutes < 60) return `${minutes} phút`;
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return m > 0 ? `${h}h${m}p` : `${h} giờ`;
  }
}
