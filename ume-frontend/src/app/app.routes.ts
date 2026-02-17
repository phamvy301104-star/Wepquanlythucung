import { Routes } from '@angular/router';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { authGuard, adminGuard, guestGuard } from './guards/auth.guard';
import { AuthService } from './services/auth.service';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent) },
  { path: 'login', loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent), canActivate: [guestGuard] },
  { path: 'register', loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent), canActivate: [guestGuard] },
  { path: 'products', loadComponent: () => import('./pages/products/products.component').then(m => m.ProductsComponent) },
  { path: 'products/:id', loadComponent: () => import('./pages/product-detail/product-detail.component').then(m => m.ProductDetailComponent) },
  { path: 'services', loadComponent: () => import('./pages/services/services.component').then(m => m.ServicesComponent) },
  { path: 'services/:id', loadComponent: () => import('./pages/service-detail/service-detail.component').then(m => m.ServiceDetailComponent) },
  { path: 'booking', loadComponent: () => import('./pages/booking/booking.component').then(m => m.BookingComponent), canActivate: [authGuard] },
  { path: 'pets', loadComponent: () => import('./pages/pets/pets.component').then(m => m.PetsComponent) },
  { path: 'pets/:id', loadComponent: () => import('./pages/pet-detail/pet-detail.component').then(m => m.PetDetailComponent) },
  { path: 'cart', loadComponent: () => import('./pages/cart/cart.component').then(m => m.CartComponent) },
  { path: 'checkout', loadComponent: () => import('./pages/checkout/checkout.component').then(m => m.CheckoutComponent), canActivate: [authGuard] },
  { path: 'checkout-success', loadComponent: () => import('./pages/checkout-success/checkout-success.component').then(m => m.CheckoutSuccessComponent) },
  { path: 'contact', loadComponent: () => import('./pages/contact/contact.component').then(m => m.ContactComponent) },
  { path: 'profile', loadComponent: () => import('./pages/profile/profile.component').then(m => m.ProfileComponent), canActivate: [authGuard] },
  { path: 'my-appointments', loadComponent: () => import('./pages/my-appointments/my-appointments.component').then(m => m.MyAppointmentsComponent), canActivate: [authGuard] },
  { path: 'my-orders', loadComponent: () => import('./pages/my-orders/my-orders.component').then(m => m.MyOrdersComponent), canActivate: [authGuard] },
  { path: 'my-pets', loadComponent: () => import('./pages/my-pets/my-pets.component').then(m => m.MyPetsComponent), canActivate: [authGuard] },
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadComponent: () => import('./pages/admin/admin-layout/admin-layout.component').then(m => m.AdminLayoutComponent),
    children: [
      { path: '', loadComponent: () => import('./pages/admin/dashboard/dashboard.component').then(m => m.DashboardComponent), canActivate: [() => { const auth = inject(AuthService); const router = inject(Router); if (auth.isAdmin) return true; router.navigate(['/admin/products']); return false; }] },
      { path: 'products', loadComponent: () => import('./pages/admin/admin-products/admin-products.component').then(m => m.AdminProductsComponent) },
      { path: 'categories', loadComponent: () => import('./pages/admin/admin-categories/admin-categories.component').then(m => m.AdminCategoriesComponent) },
      { path: 'brands', loadComponent: () => import('./pages/admin/admin-brands/admin-brands.component').then(m => m.AdminBrandsComponent) },
      { path: 'services', loadComponent: () => import('./pages/admin/admin-services/admin-services.component').then(m => m.AdminServicesComponent) },
      { path: 'staff', loadComponent: () => import('./pages/admin/admin-staff/admin-staff.component').then(m => m.AdminStaffComponent) },
      { path: 'appointments', loadComponent: () => import('./pages/admin/admin-appointments/admin-appointments.component').then(m => m.AdminAppointmentsComponent) },
      { path: 'orders', loadComponent: () => import('./pages/admin/admin-orders/admin-orders.component').then(m => m.AdminOrdersComponent) },
      { path: 'users', loadComponent: () => import('./pages/admin/admin-users/admin-users.component').then(m => m.AdminUsersComponent) },
      { path: 'pets', loadComponent: () => import('./pages/admin/admin-pets/admin-pets.component').then(m => m.AdminPetsComponent) },
      { path: 'reviews', loadComponent: () => import('./pages/admin/admin-reviews/admin-reviews.component').then(m => m.AdminReviewsComponent) },
      { path: 'promotions', loadComponent: () => import('./pages/admin/admin-promotions/admin-promotions.component').then(m => m.AdminPromotionsComponent) },
      { path: 'reports', loadComponent: () => import('./pages/admin/admin-reports/admin-reports.component').then(m => m.AdminReportsComponent) },
      { path: 'settings', loadComponent: () => import('./pages/admin/admin-settings/admin-settings.component').then(m => m.AdminSettingsComponent) },
    ]
  },
  { path: '**', redirectTo: '' }
];
