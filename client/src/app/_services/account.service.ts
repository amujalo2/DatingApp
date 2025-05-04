import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, model, signal } from '@angular/core';
import { map, single } from 'rxjs';
import { User } from '../_models/user';
import { environment } from '../../environments/environment';
import { LikesService } from './likes.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private http = inject(HttpClient);
  private likesService= inject(LikesService)
  baseUrl = environment.apiUrl;
  currentUser = signal<User | null>(null);
  roles = computed(() => {
    const user = this.currentUser();
    if (user && user.token) {
      const role = JSON.parse(atob(user.token.split('.')[1])).role;
      return Array.isArray(role) ? role : [role];
    }
    return [];
  })
  login(model: any) {
    return this.http.post(this.baseUrl + 'account/login', model).pipe(
      map(user => {
        if (user) {
          this.setCurrentUser(user as User);
        }  
      }));
  }
  register(model: any) {
    return this.http.post(this.baseUrl + 'account/register', model).pipe(
      map(user => {
        if (user) {
          this.setCurrentUser(user as User);
        }  
        return user;
      }));
  }
  setCurrentUser(user: User) {
    if (!user.photoUrl) {
      user.photoUrl = 'https://th.bing.com/th/id/OIP.PoS7waY4-VeqgNuBSxVUogAAAA?rs=1&pid=ImgDetMain';
    }
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user as User);
    this.likesService.getLikeIds();
  }
  logout() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
  } 
}
