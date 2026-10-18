import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';

export type AuthPage = 'login' | 'register' | 'forgot';

@Component({
  selector: 'bams-auth-panel',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './auth-panel.component.html'
})
export class AuthPanelComponent {
  readonly page = input.required<AuthPage>();
  readonly busy = input(false);
  readonly error = input('');
  readonly message = input('');
  readonly pageRequested = output<AuthPage>();
  readonly loginRequested = output<{ email: string; password: string }>();
  readonly registerRequested = output<{ firstName: string; lastName: string; email: string; password: string; phone?: string }>();
  readonly forgotRequested = output<string>();

  firstName = '';
  lastName = '';
  email = '';
  password = '';
  phone = '';

  login(): void {
    this.loginRequested.emit({ email: this.email, password: this.password });
  }

  register(): void {
    this.registerRequested.emit({
      firstName: this.firstName,
      lastName: this.lastName,
      email: this.email,
      password: this.password,
      phone: this.phone || undefined
    });
  }

  forgot(): void {
    this.forgotRequested.emit(this.email);
  }
}
