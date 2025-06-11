import { ResolveFn } from '@angular/router';
import { Member } from '../_models/member';
import { inject } from '@angular/core';
import { MembersService } from '../_services/members.service';
import { catchError, of } from 'rxjs';

export const memberDetailedResolver: ResolveFn<Member | null> = (route, state) => {
  const memberService = inject(MembersService);
  const username = route.paramMap.get('username');
  if(!username) return of(null);
  
  return memberService.getMember(username).pipe(
    catchError(error => {
      console.error('Error loading member:', error);
      return of(null);
    })
  );
};
