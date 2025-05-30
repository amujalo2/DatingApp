import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { User } from '../../_models/user';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from '../../modals/roles-modal/roles-modal.component';
import { PhotoApprovalStats } from '../../_models/photoApprovalStats';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-user-management',
  imports: [CommonModule],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.css'
})
export class UserManagementComponent implements OnInit{
  private adminService = inject(AdminService);
  private modalService = inject(BsModalService)
  bsModalRef: BsModalRef<RolesModalComponent> = new BsModalRef<RolesModalComponent>();
  users: User[] = [];
  usersWithoutMainPhoto: string[] = [];
  photoStats: PhotoApprovalStats[] = [];
  
  ngOnInit(): void {
    this.getUserWithRoles();
    this.getUsersWithoutMainPhoto();
    this.getPhotoStats();
  }
  getUsersWithoutMainPhoto() {
    this.adminService.getUsersWithoutMainPhoto().subscribe({
      next: users => {
        this.usersWithoutMainPhoto = users;
      }
    });
  }

  getPhotoStats() {
    this.adminService.getPhotoStats().subscribe({
      next: stats => {
        this.photoStats = stats
        console.log(this.photoStats);
      }
    });
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
