import { ErrorHandler } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from '../../../shared/auth/interceptors/auth.interceptor';
import { errorInterceptor } from '../../../shared/auth/interceptors/error.interceptor';
import { provideRouter, withRouterConfig } from '@angular/router';
import { routes } from './app.routes';
import { AppComponent } from './app.component';
import { MonitorService, MonitoringErrorHandler } from '../../../shared/monitoring/monitor.service';

bootstrapApplication(AppComponent, {
  providers: [provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    { provide: ErrorHandler, useClass: MonitoringErrorHandler },
    provideRouter(routes, withRouterConfig({ onSameUrlNavigation: 'ignore' }))]
}).then(application => {
  application.injector.get(MonitorService).logEvent('Bootstrap', 'Application started', { application: 'customer' });
}).catch(error => console.error(error));
