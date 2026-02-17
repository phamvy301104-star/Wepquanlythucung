export interface User {
  _id: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  avatarUrl?: string;
  role: 'Customer' | 'Admin' | 'Staff';
  gender?: string;
  dateOfBirth?: string;
  address?: {
    street?: string;
    ward?: string;
    district?: string;
    city?: string;
  };
  isActive: boolean;
  createdAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
  phoneNumber?: string;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  data: {
    user: User;
    accessToken: string;
    refreshToken: string;
  };
}

export interface Product {
  _id: string;
  name: string;
  slug: string;
  sku: string;
  description: string;
  shortDescription: string;
  price: number;
  originalPrice?: number;
  costPrice?: number;
  stockQuantity: number;
  imageUrl: string;
  additionalImages: string[];
  category: Category;
  brand: Brand;
  isFeatured: boolean;
  isActive: boolean;
  averageRating: number;
  totalReviews: number;
  viewCount: number;
  soldCount: number;
  createdAt: string;
}

export interface Category {
  _id: string;
  name: string;
  slug: string;
  description?: string;
  imageUrl?: string;
  parentCategory?: Category;
  children?: Category[];
  productCount?: number;
  isActive: boolean;
}

export interface Brand {
  _id: string;
  name: string;
  slug: string;
  description?: string;
  logoUrl?: string;
  productCount?: number;
  isActive: boolean;
}

export interface Service {
  _id: string;
  name: string;
  description: string;
  price: number;
  duration: number;
  imageUrl: string;
  category: ServiceCategory;
  isActive: boolean;
  averageRating: number;
  totalReviews: number;
}

export interface ServiceCategory {
  _id: string;
  name: string;
  description?: string;
  imageUrl?: string;
  parent?: ServiceCategory;
  isActive: boolean;
}

export interface Staff {
  _id: string;
  staffCode: string;
  fullName: string;
  nickName: string;
  avatarUrl: string;
  position: string;
  level: string;
  specialties: string;
  bio: string;
  averageRating: number;
  totalReviews: number;
  services: Service[];
  schedule: StaffSchedule[];
  status: string;
}

export interface StaffSchedule {
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  breakStartTime?: string;
  breakEndTime?: string;
  isWorkingDay: boolean;
}

export interface Appointment {
  _id: string;
  appointmentCode: string;
  customer: User;
  staff: Staff;
  pet?: Pet;
  appointmentDate: string;
  startTime: string;
  endTime?: string;
  services: AppointmentService[];
  totalAmount: number;
  finalAmount: number;
  status: string;
  paymentStatus: string;
  notes?: string;
  createdAt: string;
}

export interface AppointmentService {
  service: Service;
  serviceName: string;
  price: number;
  duration: number;
}

export interface Order {
  _id: string;
  orderCode: string;
  customer: User;
  items: OrderItem[];
  subtotal: number;
  shippingFee: number;
  totalAmount: number;
  status: string;
  paymentMethod: string;
  paymentStatus: string;
  shippingAddress: ShippingAddress;
  notes?: string;
  createdAt: string;
}

export interface OrderItem {
  product: Product;
  productName: string;
  productImage: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface ShippingAddress {
  fullName: string;
  phone: string;
  address: string;
  ward?: string;
  district?: string;
  city?: string;
}

export interface Pet {
  _id: string;
  owner: User;
  name: string;
  type: string;
  breed: string;
  age: number;
  ageUnit: string;
  weight: number;
  gender: string;
  color: string;
  imageUrl: string;
  additionalImages: string[];
  description: string;
  healthNotes?: string;
  vaccinated: boolean;
  neutered: boolean;
  listingType: string;
  listingPrice?: number;
  listingStatus?: string;
  createdAt: string;
}

export interface Notification {
  _id: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}

export interface Review {
  _id: string;
  customer: User;
  rating: number;
  title?: string;
  comment: string;
  images: string[];
  reply?: string;
  createdAt: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data: T;
}

export interface PaginatedResponse<T> {
  success: boolean;
  data: {
    [key: string]: T[];
    pagination: any;
  };
}

export interface Pagination {
  total: number;
  page: number;
  limit: number;
  pages: number;
}
