import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { Photo } from '../../_models/Photo';
import { ToastrModule, ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-photo-management',
  imports: [ToastrModule],
  templateUrl: './photo-management.component.html',
  styleUrl: './photo-management.component.css'
})
export class PhotoManagementComponent implements OnInit {
  private adminservice = inject(AdminService);
  private toastrService = inject(ToastrService)
  selectedPhoto: Photo | null = null;
  isModalOpen = false;
  photos: Photo[] = [];

  ngOnInit(): void {
    this.approvePhotosForApproval();
  }
  approvePhotosForApproval() {
    this.adminservice.getPhotosForApproval().subscribe({
      next: p => this.photos = p,
      error: er => {
          this.toastrService.error('Failed to load photos');
          console.log(er);
        }
    });
  }
  approvePhoto(photoId: number) {
    this.adminservice.approvePhoto(photoId).subscribe({
      next:  () => this.photos.splice(this.photos.findIndex(p => p.id === photoId), 1),
      error: er => {
        this.toastrService.error('Failed to approve photo');
        console.log(er);
      }
    });
  }
  rejectPhoto(photoId: number) {
    this.adminservice.rejectPhoto(photoId).subscribe({
      next:  () => this.photos.splice(this.photos.findIndex(p => p.id === photoId), 1),
      error: er => {
        this.toastrService.error('Failed to reject photo');
        console.log(er);
      }
    });
  }
  openPhotoModal(photo: Photo): void {
    this.selectedPhoto = photo;
    this.isModalOpen = true;
    document.body.classList.add('modal-open');
  }

  closePhotoModal(): void {
    this.isModalOpen = false;
    document.body.classList.remove('modal-open');
    setTimeout(() => {
      this.selectedPhoto = null;
    }, 300);
  }
}
