import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface UserDto { id: string; email: string; firstName: string; lastName: string; roles: string[]; isActive: boolean; }
export interface AuthResponse { accessToken: string; refreshToken: string; user: UserDto; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/auth`;
  readonly currentUser$ = new BehaviorSubject<UserDto | null>(this.readUser());

  login(payload: { email: string; password: string }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.base}/login`, payload).pipe(tap(r => this.storeAuth(r)));
  }

  register(payload: { email: string; password: string; firstName: string; lastName: string }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.base}/register`, payload).pipe(tap(r => this.storeAuth(r)));
  }

  refreshToken(): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.base}/refresh`, { refreshToken: localStorage.getItem('refreshToken') }).pipe(tap(r => this.storeAuth(r)));
  }

  logout(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    this.currentUser$.next(null);
  }

  hasRole(role: string): boolean {
    return this.currentUser$.value?.roles.includes(role) ?? false;
  }

  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  private storeAuth(data: AuthResponse): void {
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('user', JSON.stringify(data.user));
    this.currentUser$.next(data.user);
  }

  private readUser(): UserDto | null {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) as UserDto : null;
  }
}
