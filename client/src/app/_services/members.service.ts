import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { of, tap } from 'rxjs';
import { Photo } from '../_models/Photo';
import { PaginatedResult } from '../_models/pagination';
import { ParseError } from '@angular/compiler';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';
import { Tag } from '../_models/Tag';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private http = inject(HttpClient);
  private accountService = inject(AccountService)
  baseUrl = environment.apiUrl;
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null);
  memberCache = new Map();
  user = this.accountService.currentUser();
  userParams = signal<UserParams>(new UserParams(this.user));
  
  resetUserParams(){
    this.userParams.set(new UserParams(this.user));
  }

  getMembers() {
    const response = this.memberCache.get(Object.values(this.userParams()).join('-'));
    
    if (response) return setPaginatedResponse(response, this.paginatedResult);

    let params = setPaginationHeaders(this.userParams().pageNumber, this.userParams().pageSize);
    params = params.append('minAge', this.userParams().minAge.toString());
    params = params.append('maxAge', this.userParams().maxAge.toString());
    params = params.append('gender', this.userParams().gender);
    params = params.append('orderBy', this.userParams().orderBy);
    return this.http.get<Member[]>(this.baseUrl + 'users', { params, observe: 'response' }).subscribe({
      next: (response) => {
        setPaginatedResponse(response, this.paginatedResult);
        this.memberCache.set(Object.values(this.userParams()).join('-'), response);
      }
    });
  }

  

  getMember(username: string) {
    const member: Member = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.body),  [])
      .find((m: Member) => m.username == username)

    if (member) return of(member);
    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }
  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe();
  }
  setMainPhoto(photo: Photo) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photo.id, {}).pipe();
  }
  deletePhoto(photo: Photo) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photo.id).pipe();
  } 
  getTagsForPhoto(photoId: number) {
    return this.http.get<Tag[]>(this.baseUrl + 'users/tags/' + photoId);
  }
  getPhotosWithTags() {
    return this.http.get<Photo[]>(this.baseUrl + 'users/photos-tags');
  }
  getAllTags() {
    return this.http.get<string[]>(this.baseUrl + 'users/tags');
  }
  addTagToPhoto(photoId: number, tags: string[]) {
    return this.http.post(
      `${this.baseUrl}users/assign-tags/${photoId}`,
      JSON.stringify(tags),
      {
        headers: {
          'Content-Type': 'application/json',
        },
      }
    );
  }
}
