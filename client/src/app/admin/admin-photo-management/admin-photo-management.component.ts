import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { Photo } from '../../_models/Photo';
import { ToastrModule, ToastrService } from 'ngx-toastr';
import { FormsModule, NgForm } from '@angular/forms';
import { Tag } from '../../_models/Tag';
import { CommonModule } from '@angular/common';
import { HasRoleDirective } from '../../_directives/has-role.directive';
import { AdminPhotoStatsComponent } from '../admin-photo-stats/admin-photo-stats.component';
import { PhotoModalComponent } from '../../modals/photo-modal/photo-modal.component';

@Component({
  selector: 'app-admin-photo-management',
  imports: [
    ToastrModule,
    FormsModule,
    CommonModule,
    HasRoleDirective,
    AdminPhotoStatsComponent,
    PhotoModalComponent
  ],
  templateUrl: './admin-photo-management.component.html',
  styleUrl: './admin-photo-management.component.css',
})
export class AdminPhotoManagementComponent implements OnInit {
  private adminService = inject(AdminService);
  private toastrService = inject(ToastrService);
  
  selectedPhoto: Photo | null = null;
  isModalOpen = false;
  photos: Photo[] = [];
  tags: Tag[] = [];
  newTag: Tag = {} as Tag;
  selectedTag: Tag = {} as Tag;
  filteredPhotos: any[] = [];
  
  ngOnInit(): void {
    this.loadPhotosForApproval();
    this.getTags();
  }

  loadPhotosForApproval() {
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

  openPhotoModal(photo: Photo): void {
    this.selectedPhoto = photo;
    this.isModalOpen = true;
  }

  closePhotoModal(): void {
    this.isModalOpen = false;
    setTimeout(() => {
      this.selectedPhoto = null;
    }, 300);
  }

  onPhotoApproved(photoId: number): void {
    this.photos = this.photos.filter(p => p.id !== photoId);
    this.filterPhotosByTag();
  }

  onPhotoRejected(photoId: number): void {
    this.photos = this.photos.filter(p => p.id !== photoId);
    this.filterPhotosByTag();
  }

  approvePhoto(photoId: number) {
    this.adminService.approvePhoto(photoId).subscribe({
      next: () => {
        this.photos = this.photos.filter(p => p.id !== photoId);
        this.filterPhotosByTag();
      },
      error: (er) => {
        this.toastrService.error('Failed to approve photo');
        console.log(er);
      },
    });
  }

  rejectPhoto(photoId: number) {
    this.adminService.rejectPhoto(photoId).subscribe({
      next: () => {
        this.photos = this.photos.filter(p => p.id !== photoId);
        this.filterPhotosByTag();
      },
      error: (er) => {
        this.toastrService.error('Failed to reject photo');
        console.log(er);
      },
    });
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
        this.loadPhotosForApproval();
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