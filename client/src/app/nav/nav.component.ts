import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { HasRoleDirective } from '../_directives/has-role.directive';
import { CommonModule } from '@angular/common';
import { AuthStoreService } from '../_services/auth-store.service';
import { Observable } from 'rxjs';
import { User } from '../_models/user';

@Component({
  selector: 'app-nav',
  standalone: true,
  imports: [FormsModule, BsDropdownModule, RouterLink, HasRoleDirective, CommonModule],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.css'
})
export class NavComponent {
  private accountService = inject(AccountService);
  private authStoreService = inject(AuthStoreService);
  private router = inject(Router);
  private toastr = inject(ToastrService);
  isCollapsed = true;

  model: any = {};

  currentUser$: Observable<User | null> = this.authStoreService.currentUser$;
  isLoggedIn$: Observable<boolean> = this.authStoreService.isLoggedIn$;
  isAdmin$: Observable<boolean> = this.authStoreService.hasRole('Admin');
  isModerator$: Observable<boolean> = this.authStoreService.hasRole('Moderator');
  userRoles$: Observable<string[]> = this.authStoreService.userRoles$;

  login() {
    this.accountService.login(this.model).subscribe({
      next: _ => {
        this.router.navigateByUrl('/members');
        this.model = {};
      },
      error: error => {
        this.toastr.error(error.error.message);
      }
    });
  }
  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
  toggleNavbar() {
    this.isCollapsed = !this.isCollapsed;
  }
}
