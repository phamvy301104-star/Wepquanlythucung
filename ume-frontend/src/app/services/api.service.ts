import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private apiUrl = environment.apiUrl;
  private baseUrl = environment.apiUrl.replace('/api', '');

  constructor(private http: HttpClient) {}

  private defaultPlaceholder = 'assets/images/no-image.svg';

  /** Convert relative image paths to full URLs */
  getImageUrl(path: string, placeholder?: string): string {
    if (!path) return placeholder || this.defaultPlaceholder;
    if (path.startsWith('http://') || path.startsWith('https://')) return path;
    return this.baseUrl + path;
  }

  // Products
  getProducts(params?: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/products`, { params: this.buildParams(params) });
  }

  getProduct(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/products/${id}`);
  }

  getFeaturedProducts(): Observable<any> {
    return this.http.get(`${this.apiUrl}/products/featured`);
  }

  createProduct(data: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/products`, data);
  }

  updateProduct(id: string, data: FormData): Observable<any> {
    return this.http.put(`${this.apiUrl}/products/${id}`, data);
  }

  deleteProduct(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/products/${id}`);
  }

  // Categories
  getCategories(params?: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/categories`, { params: this.buildParams(params) });
  }

  getCategoryTree(): Observable<any> {
    return this.http.get(`${this.apiUrl}/categories/tree`);
  }

  createCategory(data: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/categories`, data);
  }

  updateCategory(id: string, data: FormData): Observable<any> {
    return this.http.put(`${this.apiUrl}/categories/${id}`, data);
  }

  deleteCategory(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/categories/${id}`);
  }

  // Brands
  getBrands(params?: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/brands`, { params: this.buildParams(params) });
  }

  createBrand(data: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/brands`, data);
  }

  updateBrand(id: string, data: FormData): Observable<any> {
    return this.http.put(`${this.apiUrl}/brands/${id}`, data);
  }

  deleteBrand(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/brands/${id}`);
  }

  // Services
  getServices(params?: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/services`, { params: this.buildParams(params) });
  }

  getService(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/services/${id}`);
  }

  getServiceCategories(): Observable<any> {
    return this.http.get(`${this.apiUrl}/services/categories`);
  }

  createService(data: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/services`, data);
  }

  updateService(id: string, data: FormData): Observable<any> {
    return this.http.put(`${this.apiUrl}/services/${id}`, data);
  }

  deleteService(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/services/${id}`);
  }

  // Staff
  getStaffList(params?: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/staff`, { params: this.buildParams(params) });
  }

  getStaff(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/staff/${id}`);
  }

  getAvailableStaff(params?: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/staff/available`, { params: this.buildParams(params) });
  }

  createStaff(data: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/staff`, data);
  }

  updateStaff(id: string, data: FormData): Observable<any> {
    return this.http.put(`${this.apiUrl}/staff/${id}`, data);
  }

  deleteStaff(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/staff/${id}`);
  }

  // Appointments
  getAppointments(params?: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/appointments`, { params: this.buildParams(params) });
  }

  getAppointment(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/appointments/${id}`);
  }

  getMyAppointments(params?: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/appointments/my`, { params: this.buildParams(params) });
  }

  createAppointment(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/appointments`, data);
  }

  updateAppointmentStatus(id: string, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/appointments/${id}/status`, data);
  }

  cancelAppointment(id: string, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/appointments/${id}/cancel`, data);
  }

  deleteAppointment(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/appointments/${id}`);
  }

  // Orders
  getOrders(params?: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/orders`, { params: this.buildParams(params) });
  }

  getOrder(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/orders/${id}`);
  }

  getMyOrders(params?: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/orders/my`, { params: this.buildParams(params) });
  }

  createOrder(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/orders`, data);
  }

  updateOrderStatus(id: string, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/orders/${id}/status`, data);
  }

  cancelOrder(id: string, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/orders/${id}/cancel`, data);
  }

  deleteOrder(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/orders/${id}`);
  }

  // Pets
  getPets(params?: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/pets`, { params: this.buildParams(params) });
  }

  getPet(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/pets/${id}`);
  }

  getMyPets(): Observable<any> {
    return this.http.get(`${this.apiUrl}/pets/my`);
  }

  getPetListings(params?: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/pets/listings`, { params: this.buildParams(params) });
  }

  createPet(data: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/pets`, data);
  }

  updatePet(id: string, data: FormData): Observable<any> {
    return this.http.put(`${this.apiUrl}/pets/${id}`, data);
  }

  deletePet(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/pets/${id}`);
  }

  // Notifications
  getNotifications(params?: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/notifications`, { params: this.buildParams(params) });
  }

  getUnreadCount(): Observable<any> {
    return this.http.get(`${this.apiUrl}/notifications/unread-count`);
  }

  markAsRead(id: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/notifications/${id}/read`, {});
  }

  markAllAsRead(): Observable<any> {
    return this.http.put(`${this.apiUrl}/notifications/read-all`, {});
  }

  // Reviews
  getProductReviews(productId: string, params?: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/reviews/product/${productId}`, { params: this.buildParams(params) });
  }

  getServiceReviews(serviceId: string, params?: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/reviews/service/${serviceId}`, { params: this.buildParams(params) });
  }

  createReview(data: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/reviews`, data);
  }

  // Admin
  getDashboard(): Observable<any> {
    return this.http.get(`${this.apiUrl}/admin/dashboard`);
  }

  getRevenueChart(params?: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/admin/revenue-chart`, { params: this.buildParams(params) });
  }

  // Promotions
  validatePromoCode(code: string, orderAmount: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/promotions/validate`, { code, orderAmount });
  }

  // Settings
  getSettings(): Observable<any> {
    return this.http.get(`${this.apiUrl}/settings`);
  }

  updateSettings(data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/settings`, data);
  }

  // Users (admin)
  getUsers(params?: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/users`, { params: this.buildParams(params) });
  }

  createUser(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/users`, data);
  }

  updateUser(id: string, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/users/${id}`, data);
  }

  deleteUser(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/users/${id}`);
  }

  // Upload
  uploadFile(file: File, folder: string = 'general'): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.apiUrl}/upload?folder=${encodeURIComponent(folder)}`, formData);
  }

  // Generic methods
  get(path: string, params?: any): Observable<any> {
    return this.http.get(`${this.apiUrl}${path}`, { params: this.buildParams(params) });
  }

  post(path: string, data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}${path}`, data);
  }

  put(path: string, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}${path}`, data);
  }

  delete(path: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}${path}`);
  }

  private buildParams(params?: any): HttpParams {
    let httpParams = new HttpParams();
    if (params) {
      Object.keys(params).forEach(key => {
        if (params[key] !== null && params[key] !== undefined && params[key] !== '') {
          httpParams = httpParams.set(key, params[key].toString());
        }
      });
    }
    return httpParams;
  }
}
