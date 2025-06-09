import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { User } from '../../_models/user';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from '../../modals/roles-modal/roles-modal.component';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-user-management',
  imports: [CommonModule],
  templateUrl: './admin-user-management.component.html',
  styleUrl: './admin-user-management.component.css'
})
export class AdminUserManagementComponent implements OnInit{
  private adminService = inject(AdminService);
  private modalService = inject(BsModalService)
  bsModalRef: BsModalRef<RolesModalComponent> = new BsModalRef<RolesModalComponent>();
  users: User[] = [];
  
  ngOnInit(): void {
    this.getUserWithRoles();
  }
  
  getUserWithRoles() {
    this.adminService.getUserWithRoles().subscribe({
      next: users => this.users = users
    });
  }
  openRolesModal(user: User) {
    const initialState: ModalOptions = {
      class: 'modal-lg',
      initialState: {
        title: 'User roles',
        username: user.username,
        selectedRoles: [...user.roles],
        availableRoles: ['Admin', 'Moderator', 'Member'],
        users: this.users,
        rolesUpdated: false
      }
    }
    this.bsModalRef = this.modalService.show(RolesModalComponent, initialState);
    this.bsModalRef.onHide?.subscribe({
      next: () => {
        if (this.bsModalRef.content && this.bsModalRef.content.rolesUpdated) {
          const selectedRoles = this.bsModalRef.content.selectedRoles;
          this.adminService.updateUserRoles(user.username, selectedRoles).subscribe({
            next: roles => user.roles = roles
          })
        }
      }
    })
  }
}
