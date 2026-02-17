import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-my-pets',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './my-pets.component.html',
  styleUrl: './my-pets.component.scss'
})
export class MyPetsComponent implements OnInit {
  pets: any[] = [];
  loading = false;

  showModal = false;
  editingPet: any = null;
  petForm: any = {
    name: '',
    type: 'Dog',
    breed: '',
    age: null,
    ageUnit: 'months',
    weight: null,
    gender: 'Male',
    color: '',
    description: '',
    healthNotes: '',
    vaccinated: false,
    neutered: false,
    listingType: 'None',
    listingPrice: 0,
    listingDescription: ''
  };
  selectedImage: File | null = null;
  imagePreview: string | null = null;
  submitting = false;

  petTypes = [
    { value: 'Dog', label: 'Chó' },
    { value: 'Cat', label: 'Mèo' },
    { value: 'Bird', label: 'Chim' },
    { value: 'Fish', label: 'Cá' },
    { value: 'Hamster', label: 'Hamster' },
    { value: 'Rabbit', label: 'Thỏ' },
    { value: 'Other', label: 'Khác' }
  ];
  genderOptions = [
    { value: 'Male', label: 'Đực' },
    { value: 'Female', label: 'Cái' },
    { value: 'Unknown', label: 'Chưa rõ' }
  ];
  ageUnits = [
    { value: 'months', label: 'Tháng' },
    { value: 'years', label: 'Năm' }
  ];
  listingTypes = [
    { value: 'None', label: 'Không đăng' },
    { value: 'Sale', label: 'Đăng bán' },
    { value: 'Adoption', label: 'Cho nhận nuôi' }
  ];

  constructor(
    private apiService: ApiService,
    private toastr: ToastrService
  ) {}

  getImg(path: string): string { return this.apiService.getImageUrl(path); }

  ngOnInit(): void {
    this.loadPets();
  }

  loadPets(): void {
    this.loading = true;
    this.apiService.getMyPets().subscribe({
      next: (res: any) => {
        this.pets = res.data?.pets || res.data || [];
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  openAddModal(): void {
    this.editingPet = null;
    this.petForm = {
      name: '', type: 'Dog', breed: '', age: null, ageUnit: 'months',
      weight: null, gender: 'Male', color: '', description: '',
      healthNotes: '', vaccinated: false, neutered: false,
      listingType: 'None', listingPrice: 0, listingDescription: ''
    };
    this.selectedImage = null;
    this.imagePreview = null;
    this.showModal = true;
  }

  openEditModal(pet: any): void {
    this.editingPet = pet;
    this.petForm = {
      name: pet.name, type: pet.type, breed: pet.breed,
      age: pet.age, ageUnit: pet.ageUnit || 'months',
      weight: pet.weight, gender: pet.gender, color: pet.color,
      description: pet.description, healthNotes: pet.healthNotes,
      vaccinated: pet.vaccinated, neutered: pet.neutered,
      listingType: pet.listingType || 'None', listingPrice: pet.listingPrice || 0,
      listingDescription: pet.listingDescription || ''
    };
    this.selectedImage = null;
    this.imagePreview = pet.imageUrl ? this.getImg(pet.imageUrl) : null;
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
  }

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.selectedImage = input.files[0];
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imagePreview = e.target.result;
      };
      reader.readAsDataURL(input.files[0]);
    }
  }

  savePet(): void {
    if (!this.petForm.name) {
      this.toastr.warning('Vui lòng nhập tên thú cưng', 'Thông báo');
      return;
    }

    this.submitting = true;
    const formData = new FormData();
    Object.keys(this.petForm).forEach(key => {
      if (this.petForm[key] !== null && this.petForm[key] !== undefined) {
        formData.append(key, this.petForm[key].toString());
      }
    });
    if (this.selectedImage) {
      formData.append('image', this.selectedImage);
    }

    const request = this.editingPet
      ? this.apiService.updatePet(this.editingPet._id, formData)
      : this.apiService.createPet(formData);

    request.subscribe({
      next: () => {
        this.submitting = false;
        this.showModal = false;
        this.toastr.success(
          this.editingPet ? 'Cập nhật thú cưng thành công!' : 'Thêm thú cưng thành công!',
          'Thành công'
        );
        this.loadPets();
      },
      error: (err: any) => {
        this.submitting = false;
        this.toastr.error(err.error?.message || 'Thao tác thất bại', 'Lỗi');
      }
    });
  }

  deletePet(pet: any): void {
    if (!confirm(`Bạn có chắc muốn xóa ${pet.name}?`)) return;
    this.apiService.deletePet(pet._id).subscribe({
      next: () => {
        this.toastr.success('Đã xóa thú cưng', 'Thành công');
        this.loadPets();
      },
      error: (err: any) => {
        this.toastr.error(err.error?.message || 'Xóa thất bại', 'Lỗi');
      }
    });
  }

  getAgeLabel(pet: any): string {
    if (!pet.age) return 'Không rõ';
    const unit = pet.ageUnit === 'years' ? 'tuổi' : 'tháng';
    return `${pet.age} ${unit}`;
  }

  getTypeName(type: string): string {
    const m: any = { Dog: 'Chó', Cat: 'Mèo', Bird: 'Chim', Fish: 'Cá', Hamster: 'Hamster', Rabbit: 'Thỏ', Other: 'Khác' };
    return m[type] || type;
  }

  getGenderName(g: string): string {
    const m: any = { Male: 'Đực', Female: 'Cái', Unknown: 'Chưa rõ' };
    return m[g] || g;
  }

  getListingLabel(t: string): string {
    const m: any = { None: 'Không đăng', Sale: 'Đang bán', Adoption: 'Cho nhận nuôi' };
    return m[t] || '';
  }
}
