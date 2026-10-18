import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { MonitorService } from '../../monitoring/monitor.service';

// Preserve typed HttpErrorResponse for each feature to show contextual feedback.
// Do not redirect on 403 or automatically retry non-idempotent operations.
export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const monitor = inject(MonitorService);
  const startedAt = performance.now();

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      const message = error.status === 0
        ? 'Request failed before a response was received'
        : `${request.method} request failed with status ${error.status}`;

      monitor.logException('HttpClient', new Error(message), {
        method: request.method,
        url: request.url.split(/[?#]/, 1)[0],
        status: error.status,
        durationMs: Math.round(performance.now() - startedAt)
      });

      return throwError(() => error);
    })
  );
};
