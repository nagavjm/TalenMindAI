import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthConfig, AuthResponse, LoginRequest, RegisterRequest } from '../models/auth.models';

// The JWT itself lives only in a HttpOnly cookie set by the API (not accessible to JS).
// We keep a small, non-sensitive copy of the user's profile in localStorage purely so the
// UI can render the logged-in state without an extra round-trip; it is not used for auth.
const USER_KEY = 'tma_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = `${environment.apiBaseUrl}/auth`;
  readonly currentUser = signal<AuthResponse | null>(this.loadUser());

  // Feature-flag driven auth config (Basic vs. SSO), fetched from the API so it can be
  // toggled server-side (appsettings) without requiring a frontend rebuild/redeploy.
  readonly authConfig = signal<AuthConfig>({ authMode: 'Basic' });

  constructor(private http: HttpClient) {}

  loadAuthConfig(): Observable<AuthConfig> {
    return this.http.get<AuthConfig>(`${this.baseUrl}/config`).pipe(tap((config) => this.authConfig.set(config)));
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/register`, request, { withCredentials: true })
      .pipe(tap((res) => this.persist(res)));
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/login`, request, { withCredentials: true })
      .pipe(tap((res) => this.persist(res)));
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/logout`, {}, { withCredentials: true }).pipe(
      tap(() => {
        localStorage.removeItem(USER_KEY);
        this.currentUser.set(null);
      })
    );
  }

  isAuthenticated(): boolean {
    return !!this.currentUser();
  }

  isAdmin(): boolean {
    return this.currentUser()?.role === 'Admin';
  }

  private persist(res: AuthResponse): void {
    localStorage.setItem(USER_KEY, JSON.stringify(res));
    this.currentUser.set(res);
  }

  private loadUser(): AuthResponse | null {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? (JSON.parse(raw) as AuthResponse) : null;
  }
}
