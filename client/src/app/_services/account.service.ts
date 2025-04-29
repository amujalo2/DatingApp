import { HttpClient } from '@angular/common/http';
import { inject, Injectable, model, signal } from '@angular/core';
import { map, single } from 'rxjs';
import { User } from '../_models/user';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;
  currentUser = signal<User | null>(null);
  login(model: any) {
    return this.http.post(this.baseUrl + 'account/login', model).pipe(
      map(user => {
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUser.set(user as User);
        }  
      }));
  }
  register(model: any) {
    return this.http.post(this.baseUrl + 'account/register', model).pipe(
      map(user => {
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUser.set(user as User);
        }  
        return user;
      }));
  }
  logout() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
  } 
}
