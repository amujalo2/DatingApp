import { Injectable, computed, inject, signal } from '@angular/core';
import { BehaviorSubject, Observable, map } from 'rxjs';
import { User } from '../_models/user';
import { PresenceService } from './presence.service';
import { LikesService } from './likes.service';

@Injectable({
  providedIn: 'root'
})
export class AuthStoreService {
  private likesService = inject(LikesService);
  private presenceService = inject(PresenceService);
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  
  public currentUser$: Observable<User | null> = this.currentUserSubject.asObservable();
  
  public isLoggedIn$: Observable<boolean> = this.currentUser$.pipe(
    map(user => !!user && !!user.token)
  );
  
  public userRoles$: Observable<string[]> = this.currentUser$.pipe(
    map(user => {
      if (user && user.token) {
        try {
          const payload = JSON.parse(atob(user.token.split('.')[1]));
          const role = payload.role;
          return Array.isArray(role) ? role : [role];
        } catch (error) {
          console.error('Error parsing token:', error);
          return [];
        }
      }
      return [];
    })
  );
  
  // Signal for backward compatibility with existing code
  currentUser = signal<User | null>(null);
  
  // Computed roles using signal (backward compatibility)
  roles = computed(() => {
    const user = this.currentUser();
    if (user && user.token) {
      try {
        const role = JSON.parse(atob(user.token.split('.')[1])).role;
        return Array.isArray(role) ? role : [role];
      } catch (error) {
        console.error('Error parsing token:', error);
        return [];
      }
    }
    return [];
  });

  constructor() {
    this.initializeAuthState();
  }

  /**
   * Initialize authentication state from localStorage
   * This runs on app startup to rehydrate the auth state
   */
  private initializeAuthState(): void {
    try {
      const storedUser = localStorage.getItem('user');
      if (storedUser) {
        const user: User = JSON.parse(storedUser);
        // Validate that the stored user has required properties
        if (user && user.token) {
          this.currentUserSubject.next(user);
          this.currentUser.set(user);
        } else {
          // Invalid user data, clear storage
          this.clearAuthState();
        }
      }
    } catch (error) {
      console.error('Error initializing auth state:', error);
      this.clearAuthState();
    }
  }

  /**
   * Get current user value synchronously (for cases where you need immediate access)
   */
  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  /**
   * Check if user is currently logged in (synchronous)
   */
  isLoggedIn(): boolean {
    const user = this.getCurrentUser();
    return !!user && !!user.token;
  }

  /**
   * Set current user and persist to localStorage
   */
  setCurrentUser(user: User): void {
    try {
      localStorage.setItem('user', JSON.stringify(user));
      
      this.currentUserSubject.next(user);
      this.currentUser.set(user); 
      this.likesService.getLikeIds();
      this.presenceService.createHubConnection(user);
    } catch (error) {
      console.error('Error setting current user:', error);
    }
  }

  /**
   * Clear all authentication state
   */
  clearAuthState(): void {
    try {
      localStorage.removeItem('user');
      this.currentUserSubject.next(null);
      this.currentUser.set(null);
    } catch (error) {
      console.error('Error clearing auth state:', error);
    }
  }

  /**
   * Update user profile data
   */
  updateUserProfile(updatedUser: Partial<User>): void {
    const currentUser = this.getCurrentUser();
    if (currentUser) {
      const newUser = { ...currentUser, ...updatedUser };
      this.setCurrentUser(newUser);
    }
  }

  /**
   * Refresh user data (useful for profile updates)
   */
  refreshUser(): void {
    const currentUser = this.getCurrentUser();
    if (currentUser) {
      // Re-fetch user data from storage if needed
      this.initializeAuthState();
    }
  }

  /**
   * Check if user has specific role
   */
  hasRole(role: string): Observable<boolean> {
    return this.userRoles$.pipe(
      map(roles => roles.includes(role))
    );
  }

  /**
   * Check if user has specific role (synchronous)
   */
  hasRoleSync(role: string): boolean {
    const user = this.getCurrentUser();
    if (user && user.token) {
      try {
        const payload = JSON.parse(atob(user.token.split('.')[1]));
        const userRoles = payload.role;
        const roles = Array.isArray(userRoles) ? userRoles : [userRoles];
        return roles.includes(role);
      } catch (error) {
        console.error('Error parsing token:', error);
        return false;
      }
    }
    return false;
  }
}