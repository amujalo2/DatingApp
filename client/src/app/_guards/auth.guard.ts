import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { AuthStoreService } from '../_services/auth-store.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authStoreService = inject(AuthStoreService);
  const toastr = inject(ToastrService);

  if (authStoreService.currentUser()) {
    return true;
  } else {
    toastr.error('You are not logged in!');
    return false;
  }
  return true;
};
