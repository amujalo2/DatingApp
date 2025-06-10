import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { HasRoleDirective } from '../_directives/has-role.directive';
import { CommonModule } from '@angular/common';
import { AuthStoreService } from '../_services/auth-store.service';

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
  login() {
    this.accountService.login(this.model).subscribe({
      next: _ => this.router.navigateByUrl('/members'),
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
  currentUser() {
    return this.authStoreService.currentUser();
  }
}
