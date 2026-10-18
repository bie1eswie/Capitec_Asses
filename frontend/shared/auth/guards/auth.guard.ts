import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthApi } from '../../auth-api';

export const authGuard: CanActivateFn = async (_route, state) => {
  const auth = inject(AuthApi);
  const router = inject(Router);

  if (auth.session() || await auth.restoreSession()) {
    return true;
  }

  return router.createUrlTree(['/login'], {
    queryParams: { returnUrl: state.url }
  });
};
