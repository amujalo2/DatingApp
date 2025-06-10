import { HttpInterceptorFn } from '@angular/common/http';
import { AccountService } from '../_services/account.service';
import { inject } from '@angular/core';
import { AuthStoreService } from '../_services/auth-store.service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const authStoreService = inject(AuthStoreService);
  if (authStoreService.currentUser())
  {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${authStoreService.currentUser()?.token}`
      }
    });

  }
  return next(req);
};
