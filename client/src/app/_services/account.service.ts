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
  private likesService = inject(LikesService);
  private presenceService = inject(PresenceService);
  private authStore = inject(AuthStoreService);
  
  baseUrl = environment.apiUrl;

  // Expose auth store properties for backward compatibility
  get currentUser() {
    return this.authStore.currentUser;
  }

  get roles() {
    return this.authStore.roles;
  }

  // Expose reactive observables
  get currentUser$() {
    return this.authStore.currentUser$;
  }

  get isLoggedIn$() {
    return this.authStore.isLoggedIn$;
  }

  get userRoles$() {
    return this.authStore.userRoles$;
  }

  /**
   * Login user
   */
  login(model: any): Observable<any> {
    return this.http.post(this.baseUrl + 'account/login', model).pipe(
      map(user => {
        if (user) {
          this.setCurrentUser(user as User);
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
          this.setCurrentUser(user as User);
        }
        return user;
      })
    );
  }

  /**
   * Set current user and initialize related services
   */
  setCurrentUser(user: User): void {
    // Use AuthStoreService to manage the user state
    this.authStore.setCurrentUser(user);
    
    // Initialize related services
    this.likesService.getLikeIds();
    this.presenceService.createHubConnection(user);
  }

  /**
   * Logout user and cleanup
   */
  logout(): void {
    // Clear auth state using AuthStoreService
    this.authStore.clearAuthState();
    
    // Stop presence service connection
    this.presenceService.stopHubConnection();
  }

  /**
   * Get current user synchronously
   */
  getCurrentUser(): User | null {
    return this.authStore.getCurrentUser();
  }

  /**
   * Check if user is logged in synchronously
   */
  isLoggedIn(): boolean {
    return this.authStore.isLoggedIn();
  }

  /**
   * Update user profile
   */
  updateUserProfile(updatedUser: Partial<User>): void {
    this.authStore.updateUserProfile(updatedUser);
  }

  /**
   * Check if user has specific role
   */
  hasRole(role: string): Observable<boolean> {
    return this.authStore.hasRole(role);
  }

  /**
   * Check if user has specific role (synchronous)
   */
  hasRoleSync(role: string): boolean {
    return this.authStore.hasRoleSync(role);
  }

  /**
   * Refresh current user data
   */
  refreshUser(): void {
    this.authStore.refreshUser();
  }
}