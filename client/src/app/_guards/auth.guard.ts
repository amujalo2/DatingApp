import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';

export const authGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const teastr = inject(ToastrService);

  if (accountService.currentUser()) {
    return true;
  } else {
    teastr.error('You are not logged in!');
    return false;
  }
  return true;
};
