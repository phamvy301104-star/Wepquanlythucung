import { lazy, Suspense } from 'react';
import { BrowserRouter, Routes, Route, Outlet } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { AuthProvider } from './contexts/AuthContext';
import { CartProvider } from './contexts/CartContext';
import { RequireAuth, RequireAdmin, RequireGuest } from './guards/Guards';
import Navbar from './components/Navbar/Navbar';
import Footer from './components/Footer/Footer';
import './styles/global.scss';

// Lazy load pages
const Home = lazy(() => import('./pages/home/Home'));
const Login = lazy(() => import('./pages/login/Login'));
const Register = lazy(() => import('./pages/register/Register'));
const Products = lazy(() => import('./pages/products/Products'));
const ProductDetail = lazy(() => import('./pages/product-detail/ProductDetail'));
const Services = lazy(() => import('./pages/services/Services'));
const ServiceDetail = lazy(() => import('./pages/service-detail/ServiceDetail'));
const Booking = lazy(() => import('./pages/booking/Booking'));
const Pets = lazy(() => import('./pages/pets/Pets'));
const PetDetail = lazy(() => import('./pages/pet-detail/PetDetail'));
const Cart = lazy(() => import('./pages/cart/Cart'));
const Checkout = lazy(() => import('./pages/checkout/Checkout'));
const CheckoutSuccess = lazy(() => import('./pages/checkout-success/CheckoutSuccess'));
const Contact = lazy(() => import('./pages/contact/Contact'));
const PetRecognition = lazy(() => import('./pages/pet-recognition/PetRecognition'));
const Profile = lazy(() => import('./pages/profile/Profile'));
const MyAppointments = lazy(() => import('./pages/my-appointments/MyAppointments'));
const MyOrders = lazy(() => import('./pages/my-orders/MyOrders'));
const MyPets = lazy(() => import('./pages/my-pets/MyPets'));


// Admin pages
const AdminLayout = lazy(() => import('./pages/admin/layout/AdminLayout'));
const Dashboard = lazy(() => import('./pages/admin/dashboard/Dashboard'));
const AdminProducts = lazy(() => import('./pages/admin/products/AdminProducts'));
const AdminCategories = lazy(() => import('./pages/admin/categories/AdminCategories'));
const AdminBrands = lazy(() => import('./pages/admin/brands/AdminBrands'));
const AdminServices = lazy(() => import('./pages/admin/services/AdminServices'));
const AdminStaff = lazy(() => import('./pages/admin/staff/AdminStaff'));
const AdminAppointments = lazy(() => import('./pages/admin/appointments/AdminAppointments'));
const AdminOrders = lazy(() => import('./pages/admin/orders/AdminOrders'));
const AdminUsers = lazy(() => import('./pages/admin/users/AdminUsers'));
const AdminPets = lazy(() => import('./pages/admin/pets/AdminPets'));
const AdminReviews = lazy(() => import('./pages/admin/reviews/AdminReviews'));
const AdminPromotions = lazy(() => import('./pages/admin/promotions/AdminPromotions'));
const AdminReports = lazy(() => import('./pages/admin/reports/AdminReports'));
const AdminContacts = lazy(() => import('./pages/admin/contacts/AdminContacts'));
const AdminSettings = lazy(() => import('./pages/admin/settings/AdminSettings'));

function Loading() {
  return (
    <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '60vh' }}>
      <div className="spinner" />
    </div>
  );
}

function PublicLayout() {
  return (
    <>
      <Navbar />
      <main style={{ paddingTop: '76px', minHeight: '60vh' }}>
        <Suspense fallback={<Loading />}>
          <Outlet />
        </Suspense>
      </main>
      <Footer />
    </>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <CartProvider>
          <Toaster position="top-right" toastOptions={{ duration: 3000 }} />
          <Routes>
            <Route element={<PublicLayout />}>
              <Route path="/" element={<Home />} />
              <Route path="/login" element={<RequireGuest><Login /></RequireGuest>} />
              <Route path="/register" element={<RequireGuest><Register /></RequireGuest>} />
              <Route path="/products" element={<Products />} />
              <Route path="/products/:id" element={<ProductDetail />} />
              <Route path="/services" element={<Services />} />
              <Route path="/services/:id" element={<ServiceDetail />} />
              <Route path="/booking" element={<RequireAuth><Booking /></RequireAuth>} />
              <Route path="/pets" element={<Pets />} />
              <Route path="/pets/:id" element={<PetDetail />} />
              <Route path="/cart" element={<Cart />} />
              <Route path="/checkout" element={<RequireAuth><Checkout /></RequireAuth>} />
              <Route path="/checkout-success" element={<CheckoutSuccess />} />
              <Route path="/contact" element={<Contact />} />
              <Route path="/pet-recognition" element={<PetRecognition />} />
              <Route path="/profile" element={<RequireAuth><Profile /></RequireAuth>} />
              <Route path="/my-appointments" element={<RequireAuth><MyAppointments /></RequireAuth>} />
              <Route path="/my-orders" element={<RequireAuth><MyOrders /></RequireAuth>} />
              <Route path="/my-pets" element={<RequireAuth><MyPets /></RequireAuth>} />
            </Route>

            <Route path="/admin" element={<RequireAdmin><Suspense fallback={<Loading />}><AdminLayout /></Suspense></RequireAdmin>}>
              <Route index element={<Dashboard />} />
              <Route path="products" element={<AdminProducts />} />
              <Route path="categories" element={<AdminCategories />} />
              <Route path="brands" element={<AdminBrands />} />
              <Route path="services" element={<AdminServices />} />
              <Route path="staff" element={<AdminStaff />} />
              <Route path="appointments" element={<AdminAppointments />} />
              <Route path="orders" element={<AdminOrders />} />
              <Route path="users" element={<AdminUsers />} />
              <Route path="pets" element={<AdminPets />} />
              <Route path="reviews" element={<AdminReviews />} />
              <Route path="promotions" element={<AdminPromotions />} />
              <Route path="reports" element={<AdminReports />} />
              <Route path="contacts" element={<AdminContacts />} />
              <Route path="settings" element={<AdminSettings />} />
            </Route>


            <Route path="*" element={<PublicLayout />} />
          </Routes>
        </CartProvider>
      </AuthProvider>
    </BrowserRouter>
  );
}
