import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { User } from '../_models/user';
import { environment } from '../../environments/environment';
import { LikesService } from './likes.service';
import { PresenceService } from './presence.service';
import { AuthStoreService } from './auth-store.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private http = inject(HttpClient);
  private authStore = inject(AuthStoreService);
  private presenceService = inject(PresenceService);
  baseUrl = environment.apiUrl;

  /**
   * Login user
   */
  login(model: any): Observable<any> {
    return this.http.post(this.baseUrl + 'account/login', model).pipe(
      map(user => {
        if (user) {
          this.authStore.setCurrentUser(user as User);
        }
        return user;
      })
    );
  }

  /**
   * Register new user
   */
  register(model: any): Observable<any> {
    return this.http.post(this.baseUrl + 'account/register', model).pipe(
      map(user => {
        if (user) {
          this.authStore.setCurrentUser(user as User);
        }
        return user;
      })
    );
  }


  /**
   * Logout user and cleanup
   */
  logout(): void {
    // Clear auth state using AuthStoreService
    this.authStore.clearAuthState();
    this.presenceService.stopHubConnection();
  }
}