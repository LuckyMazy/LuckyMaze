import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { map, take } from 'rxjs';

export const adminGuard: CanActivateFn = () => {
  const oidcSecurityService = inject(OidcSecurityService);
  const router = inject(Router);

  return oidcSecurityService.userData$.pipe(
    take(1),
    map(({ userData }) => {
      const groups = userData?.groups || [];
      const roles = userData?.roles || [];
      const roleClaim = userData?.role || '';
      if (groups.includes('admin') || roles.includes('admin') || roleClaim === 'admin' || roleClaim === 'Admin') {
        return true;
      }
      
      return router.createUrlTree(['/']);
    })
  );
};
