import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, firstValueFrom, tap } from 'rxjs';

export interface UserSession {
  userId: string;
  email: string;
  roles: string[];
  /** Optional server-issued permission claims. Missing permissions deny privileged routes. */
  permissions?: string[];
  accessToken: string;
  accessTokenExpiresAt: string;
}

@Injectable({ providedIn: 'root' })
export class AuthApi {
  private readonly http = inject(HttpClient);
  readonly session = signal<UserSession | null>(null);
  readonly restoring = signal(false);
  private refreshing: Promise<boolean> | null = null;
  private restoringSession: Promise<boolean> | null = null;
  private csrfToken: string | null = null;

  login(email: string, password: string): Observable<UserSession> {
    return this.http.post<UserSession>('/api/v1/accounts/login', { email, password }, {
      withCredentials: true
    }).pipe(tap(session => {
      this.session.set(session);
      this.csrfToken = null;
    }));
  }

  register(data: { firstName: string; lastName: string; email: string; password: string; phone?: string }): Observable<unknown> {
    return this.http.post('/api/v1/accounts/register', data);
  }

  forgotPassword(email: string): Observable<unknown> {
    return this.http.post('/api/v1/accounts/password/forgot', { email });
  }

  resetPassword(userId: string, token: string, newPassword: string): Observable<unknown> {
    return this.http.post('/api/v1/accounts/password/reset', { userId, token, newPassword });
  }

  get accessToken(): string | null {
    return this.session()?.accessToken ?? null;
  }

  private async getCsrfToken(): Promise<string> {
    if (this.csrfToken) {
      return this.csrfToken;
    }

    const response = await firstValueFrom(this.http.get<{ requestToken: string }>(
      '/api/v1/accounts/csrf-token', { withCredentials: true }
    ));
    this.csrfToken = response.requestToken;
    return response.requestToken;
  }

  /** Access tokens live only in memory; the refresh credential is HttpOnly. */
  refresh(): Promise<boolean> {
    if (this.refreshing) {
      return this.refreshing;
    }

    this.refreshing = this.performRefresh().finally(() => {
      this.refreshing = null;
    });
    return this.refreshing;
  }

  private async performRefresh(): Promise<boolean> {
    try {
      const csrf = await this.getCsrfToken();
      const session = await firstValueFrom(this.http.post<UserSession>(
        '/api/v1/accounts/refresh', {}, {
          withCredentials: true,
          headers: { 'X-CSRF-TOKEN': csrf }
        }
      ));
      this.session.set(session);
      this.csrfToken = null;
      return true;
    } catch {
      this.session.set(null);
      return false;
    }
  }

  restoreSession(): Promise<boolean> {
    if (this.session()) {
      return Promise.resolve(true);
    }

    if (this.restoringSession) {
      return this.restoringSession;
    }

    this.restoring.set(true);
    this.restoringSession = this.refresh().finally(() => {
      this.restoring.set(false);
      this.restoringSession = null;
    });

    return this.restoringSession;
  }

  /** Returns false if server-side revocation could not be confirmed. */
  async logout(): Promise<boolean> {
    try {
      const csrf = await this.getCsrfToken();
      await firstValueFrom(this.http.post('/api/v1/accounts/logout', {}, {
        withCredentials: true,
        headers: { 'X-CSRF-TOKEN': csrf }
      }));
      this.session.set(null);
      return true;
    } catch {
      // Discard the access token locally even if the server is unreachable.
      // Do not claim that the server-side refresh session was revoked.
      this.session.set(null);
      return false;
    }
  }
}

