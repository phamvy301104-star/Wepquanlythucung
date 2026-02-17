import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { User, AuthResponse, LoginRequest, RegisterRequest, ApiResponse } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = environment.apiUrl + '/auth';
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    const user = localStorage.getItem('user');
    if (user) {
      this.currentUserSubject.next(JSON.parse(user));
    }
  }

  get currentUser(): User | null {
    return this.currentUserSubject.value;
  }

  get isLoggedIn(): boolean {
    return !!this.getToken();
  }

  get isAdmin(): boolean {
    return this.currentUser?.role === 'Admin';
  }

  get isStaff(): boolean {
    return this.currentUser?.role === 'Staff';
  }

  get isAdminOrStaff(): boolean {
    return this.currentUser?.role === 'Admin' || this.currentUser?.role === 'Staff';
  }

  getToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(res => this.handleAuth(res))
    );
  }

  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, data).pipe(
      tap(res => this.handleAuth(res))
    );
  }

  googleLogin(token: string, isAccessToken = false): Observable<AuthResponse> {
    const body = isAccessToken ? { accessToken: token } : { idToken: token };
    return this.http.post<AuthResponse>(`${this.apiUrl}/google`, body).pipe(
      tap(res => this.handleAuth(res))
    );
  }

  facebookLogin(accessToken: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/facebook`, { accessToken }).pipe(
      tap(res => this.handleAuth(res))
    );
  }

  refreshToken(): Observable<any> {
    const refreshToken = localStorage.getItem('refreshToken');
    return this.http.post<any>(`${this.apiUrl}/refresh-token`, { refreshToken }).pipe(
      tap(res => {
        if (res.success) {
          localStorage.setItem('accessToken', res.data.accessToken);
          localStorage.setItem('refreshToken', res.data.refreshToken);
        }
      })
    );
  }

  getProfile(): Observable<ApiResponse<User>> {
    return this.http.get<ApiResponse<User>>(`${this.apiUrl}/profile`);
  }

  updateProfile(data: Partial<User>): Observable<ApiResponse<User>> {
    return this.http.put<ApiResponse<User>>(`${this.apiUrl}/profile`, data).pipe(
      tap(res => {
        if (res.success) {
          this.currentUserSubject.next(res.data);
          localStorage.setItem('user', JSON.stringify(res.data));
        }
      })
    );
  }

  changePassword(currentPassword: string, newPassword: string): Observable<ApiResponse<any>> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/change-password`, { currentPassword, newPassword });
  }

  logout(): void {
    const refreshToken = localStorage.getItem('refreshToken');
    this.http.post(`${this.apiUrl}/logout`, { refreshToken }).subscribe();
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);
    window.location.href = '/login';
  }

  handleAuthFromGoogle(data: { accessToken: string; refreshToken: string; user: any }): void {
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('user', JSON.stringify(data.user));
    this.currentUserSubject.next(data.user);
  }

  private handleAuth(res: AuthResponse): void {
    if (res.success && res.data) {
      localStorage.setItem('accessToken', res.data.accessToken);
      localStorage.setItem('refreshToken', res.data.refreshToken);
      localStorage.setItem('user', JSON.stringify(res.data.user));
      this.currentUserSubject.next(res.data.user);
    }
  }
}
