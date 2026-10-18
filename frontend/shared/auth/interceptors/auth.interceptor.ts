import { inject } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';
import { catchError, from, switchMap, throwError } from 'rxjs';
import { AuthApi } from '../../auth-api';

const publicAccountEndpoints = new Set([
  '/api/v1/accounts/login',
  '/api/v1/accounts/register',
  '/api/v1/accounts/refresh',
  '/api/v1/accounts/password/forgot',
  '/api/v1/accounts/password/reset'
]);
const csrfTokenEndpoint = '/api/v1/accounts/csrf-token';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthApi);
  const pathname = request.url.split('?')[0];
  const isCsrfTokenRequest = pathname === csrfTokenEndpoint;
  const isProtectedApi = pathname.startsWith('/api/') &&
    !publicAccountEndpoints.has(pathname) && !isCsrfTokenRequest;
  const shouldAttachAccessToken = isProtectedApi || isCsrfTokenRequest;
  const token = auth.accessToken;

  if (!shouldAttachAccessToken || !token) {
    return next(request);
  }

  const authorizedRequest = request.clone({
    setHeaders: { Authorization: `Bearer ${token}` }
  });

  return next(authorizedRequest).pipe(
    catchError(error => {
      if (error.status !== 401 || !isProtectedApi || pathname === '/api/v1/accounts/logout') {
        return throwError(() => error);
      }

      return from(auth.refresh()).pipe(
        switchMap(refreshed => {
          const updatedToken = auth.accessToken;
          if (!refreshed || !updatedToken) {
            return throwError(() => error);
          }

          return next(request.clone({
            setHeaders: { Authorization: `Bearer ${updatedToken}` }
          }));
        })
      );
    })
  );
};
