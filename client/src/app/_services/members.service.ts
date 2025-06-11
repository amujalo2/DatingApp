import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { of, tap, shareReplay, switchMap, Observable } from 'rxjs';
import { Photo } from '../_models/Photo';
import { PaginatedResult } from '../_models/pagination';
import { ParseError } from '@angular/compiler';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';
import { Tag } from '../_models/Tag';
import { AuthStoreService } from './auth-store.service';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  private http = inject(HttpClient);
  private authStoreService = inject(AuthStoreService);
  baseUrl = environment.apiUrl;
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null);
  memberCache = new Map();

  private user$ = this.authStoreService.currentUser$;

  userParams$ = this.user$.pipe(
    switchMap((user) => of(new UserParams(user))),
    shareReplay(1)
  );

  userParams = signal<UserParams>(new UserParams(null));

  constructor() {
    this.user$.subscribe((user) => {
      this.userParams.set(new UserParams(user));
    });
  }

  resetUserParams() {
    this.user$
      .pipe(
        switchMap((user) => {
          const newParams = new UserParams(user);
          this.userParams.set(newParams);
          return of(newParams);
        })
      )
      .subscribe();
  }

  getMembers(): Observable<HttpResponse<Member[]>> {
  const currentParams = this.userParams();
  const cacheKey = Object.values(currentParams).join('-');
  const response = this.memberCache.get(cacheKey);

  if (response) {
    setPaginatedResponse(response, this.paginatedResult);
    return of(response);
  }

  let params = setPaginationHeaders(currentParams.pageNumber, currentParams.pageSize);
  params = params.append('minAge', currentParams.minAge.toString());
  params = params.append('maxAge', currentParams.maxAge.toString());
  params = params.append('gender', currentParams.gender);
  params = params.append('orderBy', currentParams.orderBy);

  return this.http.get<Member[]>(this.baseUrl + 'users', { params, observe: 'response' }).pipe(
    tap(response => {
      setPaginatedResponse(response, this.paginatedResult);
      this.memberCache.set(cacheKey, response);
    }),
    shareReplay(1)
  );
}

  getMember(username: string): Observable<Member> {
    const member: Member = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.body), [])
      .find((m: Member) => m.username === username);

    if (member) {
      return of(member);
    }

    return this.http
      .get<Member>(this.baseUrl + 'users/' + username)
      .pipe(shareReplay(1));
  }

  updateMember(member: Member): Observable<any> {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      tap(() => {
        this.invalidateMemberCache(member.username);
      }),
      shareReplay(1)
    );
  }

  setMainPhoto(photo: Photo): Observable<any> {
    return this.http
      .put(this.baseUrl + 'users/set-main-photo/' + photo.id, {})
      .pipe(
        tap(() => {
          this.invalidateAllMemberCache();
        }),
        shareReplay(1)
      );
  }

  deletePhoto(photo: Photo): Observable<any> {
    return this.http
      .delete(this.baseUrl + 'users/delete-photo/' + photo.id)
      .pipe(
        tap(() => {
          this.invalidateAllMemberCache();
        }),
        shareReplay(1)
      );
  }

  getTagsForPhoto(photoId: number): Observable<Tag[]> {
    return this.http
      .get<Tag[]>(this.baseUrl + 'users/tags/' + photoId)
      .pipe(shareReplay(1));
  }

  getPhotosWithTags(): Observable<Photo[]> {
    return this.http
      .get<Photo[]>(this.baseUrl + 'users/photos-tags')
      .pipe(shareReplay(1));
  }

  getAllTags(): Observable<string[]> {
    return this.http
      .get<string[]>(this.baseUrl + 'users/tags')
      .pipe(shareReplay(1));
  }

  addTagToPhoto(photoId: number, tags: string[]): Observable<any> {
    return this.http
      .post(
        `${this.baseUrl}users/assign-tags/${photoId}`,
        JSON.stringify(tags),
        {
          headers: {
            'Content-Type': 'application/json',
          },
        }
      )
      .pipe(
        tap(() => {
          this.invalidateTagCache();
        }),
        shareReplay(1)
      );
  }

  private invalidateMemberCache(username?: string): void {
    if (username) {
      for (const [key, response] of this.memberCache.entries()) {
        const members = response.body as Member[];
        if (members.some((m) => m.username === username)) {
          this.memberCache.delete(key);
        }
      }
    }
  }

  private invalidateAllMemberCache(): void {
    this.memberCache.clear();
  }

  private invalidateTagCache(): void {
    this.invalidateAllMemberCache();
  }

  getCurrentUser$(): Observable<any> {
    return this.authStoreService.currentUser$;
  }
}
