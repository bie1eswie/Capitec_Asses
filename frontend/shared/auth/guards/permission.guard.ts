import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthApi } from '../../auth-api';

/** Permission claims must originate from the verified API session, never from route data. */
export const permissionGuard: CanActivateFn = async (route, state) => {
  const auth = inject(AuthApi);
  const router = inject(Router);

  if (!auth.session() && !(await auth.restoreSession())) {
    return router.createUrlTree(['/login'], {
      queryParams: { returnUrl: state.url }
    });
  }

  const required = route.data['permissions'] as string[] | undefined;
  if (!required?.length) {
    return router.createUrlTree(['/unauthorized']);
  }

  const granted = auth.session()?.permissions ?? [];
  return required.every(permission => granted.includes(permission))
    ? true
    : router.createUrlTree(['/unauthorized']);
};
