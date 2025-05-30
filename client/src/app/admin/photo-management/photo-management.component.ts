import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { Photo } from '../../_models/Photo';
import { ToastrModule, ToastrService } from 'ngx-toastr';
import { MessageService } from '../../_services/message.service';
import { FormsModule, NgForm } from '@angular/forms';
import { AccountService } from '../../_services/account.service';
import { Tag } from '../../_models/Tag';
import { PhotoApprovalStats } from '../../_models/photoApprovalStats';
import { CommonModule } from '@angular/common';
import { HasRoleDirective } from '../../_directives/has-role.directive';

@Component({
  selector: 'app-photo-management',
  imports: [ToastrModule, ToastrModule, FormsModule, CommonModule, HasRoleDirective],
  templateUrl: './photo-management.component.html',
  styleUrl: './photo-management.component.css',
})
export class PhotoManagementComponent implements OnInit {
  private adminService = inject(AdminService);
  private toastrService = inject(ToastrService);
  private messageService = inject(MessageService);
  private accountService = inject(AccountService);
  selectedPhoto: Photo | null = null;
  isModalOpen = false;
  photos: Photo[] = [];
  isAnonymous: boolean = false;
  adminMessage: string = '';
  tags: Tag[] = [];
  newTag: Tag = {} as Tag;
  selectedTag: Tag = {} as Tag;
  filteredPhotos: any[] = [];
  usersWithoutMainPhoto: string[] = [];
  photoStats: PhotoApprovalStats[] = [];

  ngOnInit(): void {
    this.approvePhotosForApproval();
    this.getTags();
    this.filteredPhotos = this.photos;
  }
  getUsersWithoutMainPhoto() {
    this.adminService.getUsersWithoutMainPhoto().subscribe({
      next: users => {
        this.usersWithoutMainPhoto = users;
      }
    });
    return this.usersWithoutMainPhoto;
  }

  getPhotoStats() {
    this.adminService.getPhotoStats().subscribe({
      next: stats => {
        this.photoStats = stats;
      }
    });
    return this.photoStats;
  }
  approvePhotosForApproval() {
    this.adminService.getPhotosForApproval().subscribe({
      next: (response) => {
        this.photos = response;
        this.filterPhotosByTag();
      },
      error: (er) => {
        this.toastrService.error('Failed to load photos');
        console.log(er);
      },
    });
  }
  approvePhoto(photoId: number) {
    if (this.isAnonymous) {
      if (this.adminMessage != '') {
        this.sendMessage();
      } else {
        this.toastrService.error(
          'Type in the message, that you have decided to send!'
        );
      }
    }
    this.adminService.approvePhoto(photoId).subscribe({
      next: () =>
        this.photos.splice(
          this.photos.findIndex((p) => p.id === photoId),
          1
        ),
      error: (er) => {
        this.toastrService.error('Failed to approve photo');
        console.log(er);
      },
    });
  }
  rejectPhoto(photoId: number) {
    if (this.isAnonymous) {
      if (this.adminMessage != '') {
        this.sendMessage();
      } else {
        this.toastrService.error(
          'Type in the message, that you have decided to send!'
        );
      }
    }
    this.adminService.rejectPhoto(photoId).subscribe({
      next: () =>
        this.photos.splice(
          this.photos.findIndex((p) => p.id === photoId),
          1
        ),
      error: (er) => {
        this.toastrService.error('Failed to reject photo');
        console.log(er);
      },
    });
  }
  sendMessage() {
    const formattedMessage = `Regarding your photo: ${this.adminMessage}`;
    if (this.selectedPhoto?.username) {
      const user = this.accountService.currentUser;
      if (!user) {
        this.toastrService.error('You must be logged in to send messages');
        return;
      }
      this.messageService
        .sendMessage(this.selectedPhoto.username, formattedMessage)
        ?.then(() => {
          console.log(
            `Anonymous message sent to ${this.selectedPhoto?.username}`
          );
        })
        .catch((error) => {
          console.error('Error sending message:', error);
          this.toastrService.error('Failed to send notification message');
        });
    } else {
      this.toastrService.error('No username selected for the photo');
    }
  }
  openPhotoModal(photo: Photo): void {
    this.selectedPhoto = photo;
    this.isModalOpen = true;
    this.isAnonymous = false;
    this.adminMessage = '';
    const user = this.accountService.currentUser();
    if (!user) return;
    this.messageService.createHubConnection(user, this.selectedPhoto.username);
    document.body.classList.add('modal-open');
  }

  closePhotoModal(): void {
    this.messageService.stopHubConnection();
    this.isModalOpen = false;
    document.body.classList.remove('modal-open');
    setTimeout(() => {
      this.selectedPhoto = null;
      this.isAnonymous = false;
      this.adminMessage = '';
    }, 300);
  }
  createTag(form: NgForm) {
    if (form.invalid) return;
    this.adminService.addTag(this.newTag).subscribe({
      next: () => {
        this.newTag.name = '';
        this.toastrService.success('Tag successfully created');
        this.getTags();
        form.resetForm();
      },
      error: (err) => {
        if (err.status === 400) {
          this.toastrService.error("You can't create duplicates!");
        } else {
          this.toastrService.error('Something unexpected happened: ' + err);
        }
      },
    });
  }
  getTags() {
    return this.adminService.getTags().subscribe({
      next: (response) => {
        this.tags = response;
      },
      error: (error) =>
        this.toastrService.error('Something unexpected happened: ' + error),
    });
  }
  removeTag(tagName: string) {
    if (!confirm(`Are you sure you want to delete the tag "${tagName}"?`))
      return;
    this.adminService.removeTag(tagName).subscribe({
      next: () => {
        this.toastrService.success('Tag successfully removed');
        this.getTags();
        this.approvePhotosForApproval(); // osveži slike ako treba
      },
      error: (error) => {
        if (error.status === 400) {
          this.toastrService.error("You can't remove a tag that is in use!");
        } else {
          this.toastrService.error('Something unexpected happened: ' + error);
        }
      },
    });
  }
  filterPhotosByTag() {
    if (!this.selectedTag) {
      this.filteredPhotos = this.photos;
    } else {
      this.filteredPhotos = this.photos.filter(
        (photo) =>
          photo.tags && photo.tags.some((tag) => tag === this.selectedTag)
      );
    }
  }
}
