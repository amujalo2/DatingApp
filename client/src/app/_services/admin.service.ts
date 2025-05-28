import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { User } from '../_models/user';
import { Photo } from '../_models/Photo';
import { Tag } from '../_models/Tag';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);

  getTags() {
    return this.http.get<Tag[]>(this.baseUrl + 'admin/get-tags');
  }
  addTag(tag: Tag) {
    return this.http.post(this.baseUrl + 'admin/create-tag', tag);
  }
  removeTag(tagName: string) {
    return this.http.delete(this.baseUrl + `admin/delete-tag/${tagName}`, {
      responseType: 'text',
    });
  }
  getUserWithRoles() {
    return this.http.get<User[]>(this.baseUrl + 'admin/users-with-roles');
  }
  updateUserRoles(username: string, roles: string[]) {
    return this.http.post<string[]>(this.baseUrl + 'admin/edit-roles/' 
      + username + '?roles=' + roles, {});
  }
  getPhotosForApproval() {
    return this.http.get<Photo[]>(this.baseUrl + 'admin/photos-to-moderate');
  }
  approvePhoto(photoId: number) {
    return this.http.post(this.baseUrl + 'admin/approve-photo/' + photoId, {});
  }
  rejectPhoto(photoId: number) {
    return this.http.post(this.baseUrl + 'admin/reject-photo/' + photoId, {});
  }
}
