import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthApi } from '../../auth-api';

export const roleGuard: CanActivateFn = async (route, state) => {
  const auth = inject(AuthApi);
  const router = inject(Router);

  if (!auth.session() && !(await auth.restoreSession())) {
    return router.createUrlTree(['/login'], {
      queryParams: { returnUrl: state.url }
    });
  }

  const requiredRoles = route.data['roles'] as string[] | undefined;
  if (!requiredRoles?.length) {
    return router.createUrlTree(['/unauthorized']);
  }

  const grantedRoles = auth.session()?.roles ?? [];
  return requiredRoles.some(role => grantedRoles.includes(role))
    ? true
    : router.createUrlTree(['/unauthorized']);
};
