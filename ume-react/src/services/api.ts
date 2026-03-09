import axios from 'axios';
import config, { getBaseUrl } from '../config';

const api = axios.create({
  baseURL: config.apiUrl,
});

// Request interceptor - add auth token
api.interceptors.request.use((cfg) => {
  const token = localStorage.getItem('accessToken');
  if (token) cfg.headers.Authorization = `Bearer ${token}`;
  return cfg;
});

// Response interceptor - handle 401
api.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

/* ---------- Helpers ---------- */
const defaultPlaceholder = '/assets/images/no-image.svg';

export function getImageUrl(path?: string, placeholder?: string): string {
  if (!path) return placeholder || defaultPlaceholder;
  if (path.startsWith('http://') || path.startsWith('https://')) return path;
  return getBaseUrl() + path;
}

function buildParams(params?: Record<string, any>) {
  if (!params) return undefined;
  const cleaned: Record<string, string> = {};
  Object.keys(params).forEach((k) => {
    if (params[k] !== null && params[k] !== undefined && params[k] !== '') {
      cleaned[k] = String(params[k]);
    }
  });
  return cleaned;
}

/* ==================== API Methods ==================== */

// Auth
export const authApi = {
  login: (data: any) => api.post('/auth/login', data),
  register: (data: any) => api.post('/auth/register', data),
  googleLogin: (data: any) => api.post('/auth/google', data),
  facebookLogin: (data: any) => api.post('/auth/facebook', data),
  refreshToken: (refreshToken: string) => api.post('/auth/refresh-token', { refreshToken }),
  getProfile: () => api.get('/auth/profile'),
  updateProfile: (data: any) => api.put('/auth/profile', data),
  changePassword: (data: any) => api.put('/auth/change-password', data),
  logout: (refreshToken: string) => api.post('/auth/logout', { refreshToken }),
};

// Products
export const productApi = {
  getAll: (params?: any) => api.get('/products', { params: buildParams(params) }),
  getProducts: (params?: any) => api.get('/products', { params: buildParams(params) }),
  getById: (id: string) => api.get(`/products/${id}`),
  getFeatured: () => api.get('/products', { params: { limit: '8', isFeatured: 'true' } }),
  create: (data: FormData) => api.post('/products', data),
  update: (id: string, data: FormData) => api.put(`/products/${id}`, data),
  delete: (id: string) => api.delete(`/products/${id}`),
};

// Categories
export const categoryApi = {
  getAll: (params?: any) => api.get('/categories', { params: buildParams(params) }),
  getTree: () => api.get('/categories/tree'),
  create: (data: FormData) => api.post('/categories', data),
  update: (id: string, data: FormData) => api.put(`/categories/${id}`, data),
  delete: (id: string) => api.delete(`/categories/${id}`),
};

// Brands
export const brandApi = {
  getAll: (params?: any) => api.get('/brands', { params: buildParams(params) }),
  create: (data: FormData) => api.post('/brands', data),
  update: (id: string, data: FormData) => api.put(`/brands/${id}`, data),
  delete: (id: string) => api.delete(`/brands/${id}`),
};

// Services
export const serviceApi = {
  getAll: (params?: any) => api.get('/services', { params: buildParams(params) }),
  getServices: (params?: any) => api.get('/services', { params: buildParams(params) }),
  getById: (id: string) => api.get(`/services/${id}`),
  getCategories: () => api.get('/services/categories'),
  getServiceCategories: () => api.get('/services/categories'),
  createServiceCategory: (data: any) => api.post('/services/categories', data),
  updateServiceCategory: (id: string, data: any) => api.put(`/services/categories/${id}`, data),
  deleteServiceCategory: (id: string) => api.delete(`/services/categories/${id}`),
  create: (data: FormData) => api.post('/services', data),
  update: (id: string, data: FormData) => api.put(`/services/${id}`, data),
  delete: (id: string) => api.delete(`/services/${id}`),
};

// Staff
export const staffApi = {
  getAll: (params?: any) => api.get('/staff', { params: buildParams(params) }),
  getById: (id: string) => api.get(`/staff/${id}`),
  getAvailable: (params?: any) => api.get('/staff/available', { params: buildParams(params) }),
  create: (data: FormData) => api.post('/staff', data),
  update: (id: string, data: FormData) => api.put(`/staff/${id}`, data),
  delete: (id: string) => api.delete(`/staff/${id}`),
};

// Appointments
export const appointmentApi = {
  getAll: (params?: any) => api.get('/appointments', { params: buildParams(params) }),
  getById: (id: string) => api.get(`/appointments/${id}`),
  getMy: (params?: any) => api.get('/appointments/my', { params: buildParams(params) }),
  getMyAppointments: (params?: any) => api.get('/appointments/my', { params: buildParams(params) }),
  create: (data: any) => api.post('/appointments', data),
  update: (id: string, data: any) => api.put(`/appointments/${id}`, data),
  updateStatus: (id: string, status: string, cancelReason?: string) => api.put(`/appointments/${id}/status`, { status, cancelReason }),
  cancel: (id: string, data: any) => api.put(`/appointments/${id}/cancel`, data),
  delete: (id: string) => api.delete(`/appointments/${id}`),
};

// Orders
export const orderApi = {
  getAll: (params?: any) => api.get('/orders', { params: buildParams(params) }),
  getById: (id: string) => api.get(`/orders/${id}`),
  getMy: (params?: any) => api.get('/orders/my', { params: buildParams(params) }),
  getMyOrders: (params?: any) => api.get('/orders/my', { params: buildParams(params) }),
  create: (data: any) => api.post('/orders', data),
  updateStatus: (id: string, status: string, cancelReason?: string) => api.put(`/orders/${id}/status`, { status, cancelReason }),
  cancel: (id: string, data: any) => api.put(`/orders/${id}/cancel`, data),
  delete: (id: string) => api.delete(`/orders/${id}`),
};

// Pets
export const petApi = {
  getAll: (params?: any) => api.get('/pets', { params: buildParams(params) }),
  getById: (id: string) => api.get(`/pets/${id}`),
  getMy: () => api.get('/pets/my'),
  getMyPets: () => api.get('/pets/my'),
  getListings: (params?: any) => api.get('/pets/listings', { params: buildParams(params) }),
  create: (data: FormData) => api.post('/pets', data),
  update: (id: string, data: FormData) => api.put(`/pets/${id}`, data),
  delete: (id: string) => api.delete(`/pets/${id}`),
};

// Notifications
export const notificationApi = {
  getAll: (params?: any) => api.get('/notifications', { params: buildParams(params) }),
  getUnreadCount: () => api.get('/notifications/unread-count'),
  markAsRead: (id: string) => api.put(`/notifications/${id}/read`, {}),
  markAllAsRead: () => api.put('/notifications/read-all', {}),
};

// Reviews
export const reviewApi = {
  getAll: (params?: any) => api.get('/reviews', { params: buildParams(params) }),
  getProductReviews: (productId: string, params?: any) => api.get(`/reviews/product/${productId}`, { params: buildParams(params) }),
  getByProduct: (productId: string, params?: any) => api.get(`/reviews/product/${productId}`, { params: buildParams(params) }),
  getServiceReviews: (serviceId: string, params?: any) => api.get(`/reviews/service/${serviceId}`, { params: buildParams(params) }),
  getByService: (serviceId: string, params?: any) => api.get(`/reviews/service/${serviceId}`, { params: buildParams(params) }),
  create: (data: FormData) => api.post('/reviews', data),
  reply: (id: string, data: any) => api.put(`/reviews/${id}/reply`, data),
  delete: (id: string) => api.delete(`/reviews/${id}`),
};

// Admin
export const adminApi = {
  getDashboard: () => api.get('/admin/dashboard'),
  getRevenueChart: (params?: any) => api.get('/admin/revenue-chart', { params: buildParams(params) }),
  getReports: (params?: any) => api.get('/admin/reports', { params: buildParams(params) }),
};

// Promotions
export const promotionApi = {
  getAll: (params?: any) => api.get('/promotions', { params: buildParams(params) }),
  getById: (id: string) => api.get(`/promotions/${id}`),
  create: (data: any) => api.post('/promotions', data),
  update: (id: string, data: any) => api.put(`/promotions/${id}`, data),
  delete: (id: string) => api.delete(`/promotions/${id}`),
  validate: (code: string, orderAmount: number) => api.post('/promotions/validate', { code, orderAmount }),
};

// Settings
export const settingsApi = {
  get: () => api.get('/settings'),
  getSettings: () => api.get('/settings'),
  update: (data: any) => api.put('/settings', data),
  updateSettings: (data: any) => api.put('/settings', data),
};

// Users (admin)
export const userApi = {
  getAll: (params?: any) => api.get('/users', { params: buildParams(params) }),
  create: (data: any) => api.post('/users', data),
  update: (id: string, data: any) => api.put(`/users/${id}`, data),
  delete: (id: string) => api.delete(`/users/${id}`),
};

// Contact
export const contactApi = {
  getAll: (params?: any) => api.get('/contacts', { params: buildParams(params) }),
  submit: (data: any) => api.post('/contacts', data),
  update: (id: string, data: any) => api.put(`/contacts/${id}`, data),
  updateStatus: (id: string, status: string, adminNote?: string) => api.put(`/contacts/${id}`, { status, adminNote }),
  delete: (id: string) => api.delete(`/contacts/${id}`),
};

// Upload
export const uploadApi = {
  upload: (data: File | FormData, folder = 'general') => {
    if (data instanceof FormData) {
      return api.post(`/upload?folder=${encodeURIComponent(folder)}`, data);
    }
    const formData = new FormData();
    formData.append('file', data);
    return api.post(`/upload?folder=${encodeURIComponent(folder)}`, formData);
  },
};

// AI
export const aiApi = {
  detectPets: (file: File) => {
    const formData = new FormData();
    formData.append('image', file);
    return api.post('/ai/detect-pets', formData);
  },
  health: () => api.get('/ai/health'),
};

// Chat AI
export const chatApi = {
  sendMessage: (message: string, sessionId?: string) =>
    api.post('/chat', { message, sessionId }),
  clearHistory: (sessionId: string) =>
    api.delete(`/chat/${sessionId}`),
};

export default api;
