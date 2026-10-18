import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthApi } from '../../auth-api';

// Guards public sign-in pages for users who already have an account session.
// This does not permit anonymous appointment booking.
export const guestGuard: CanActivateFn = async () => {
  const auth = inject(AuthApi);
  const router = inject(Router);

  if (auth.session() || await auth.restoreSession()) {
    return router.createUrlTree(['/home']);
  }

  return true;
};
