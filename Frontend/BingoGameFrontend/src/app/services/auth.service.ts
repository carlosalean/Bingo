import { Injectable } from '@angular/core';
import { ApiService, LoginDto, RegisterDto } from './api.service';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { HttpHeaders } from '@angular/common/http';

export interface User {
  id: string;
  username: string;
  email: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(private apiService: ApiService, private router: Router) { }

  login(credentials: LoginDto): Observable<any> {
    return this.apiService.postLogin(credentials).pipe(
      tap((response: any) => {
        if (response.token) {
          localStorage.setItem('token', response.token);
          localStorage.setItem('user', JSON.stringify(response.user));
        }
      })
    );
  }

  register(userData: RegisterDto): Observable<any> {
    return this.apiService.postRegister(userData).pipe(
      tap((response: any) => {
        if (response.token) {
          localStorage.setItem('token', response.token);
          localStorage.setItem('user', JSON.stringify(response.user));
        }
      })
    );
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getUser(): User | null {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }

  getCurrentUserId(): string | null {
    const user = this.getUser();
    return user ? user.id : null;
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    return !!token;
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.router.navigate(['/login']);
  }

  // Stub for token verification (can be enhanced with JWT decode or API call)
  isTokenValid(): boolean {
    const token = this.getToken();
    if (!token) return false;
    // Simple check: if token exists, assume valid for now (enhance with expiry check)
    return true;
  }

  getGuestToken(): Observable<any> {
    return this.apiService.postGuest().pipe(
      tap((response: any) => {
        if (response.token) {
          localStorage.setItem('guestToken', response.token);
        }
      })
    );
  }
}