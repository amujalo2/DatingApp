import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { Photo } from '../../_models/Photo';
import { ToastrModule, ToastrService } from 'ngx-toastr';
import { MessageService } from '../../_services/message.service';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../_services/account.service';

@Component({
  selector: 'app-photo-management',
  imports: [ToastrModule, ToastrModule, FormsModule],
  templateUrl: './photo-management.component.html',
  styleUrl: './photo-management.component.css'
})
export class PhotoManagementComponent implements OnInit {
  private adminservice = inject(AdminService);
  private toastrService = inject(ToastrService);
  private messageService = inject(MessageService);
  private accountService = inject(AccountService);
  selectedPhoto: Photo | null = null;
  isModalOpen = false;
  photos: Photo[] = [];
  isAnonymous: boolean = false;
  adminMessage: string = '';
  

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
    if(this.isAnonymous){
      if(this.adminMessage != "") {
        this.sendMessage();
      } else {
        this.toastrService.error("Type in the message, that you have decided to send!");
      }
    }
    this.adminservice.approvePhoto(photoId).subscribe({
      next:  () => this.photos.splice(this.photos.findIndex(p => p.id === photoId), 1),
      error: er => {
        this.toastrService.error('Failed to approve photo');
        console.log(er);
      }
    });
  }
  rejectPhoto(photoId: number) {
    if(this.isAnonymous){
      if(this.adminMessage != "") {
        this.sendMessage();
      } else {
        this.toastrService.error("Type in the message, that you have decided to send!");
      }
    }
    this.adminservice.rejectPhoto(photoId).subscribe({
      next:  () => this.photos.splice(this.photos.findIndex(p => p.id === photoId), 1),
      error: er => {
        this.toastrService.error('Failed to reject photo');
        console.log(er);
      }
    });
  }
  sendMessage(){
    const formattedMessage = `Regarding your photo: ${this.adminMessage}`;
    if (this.selectedPhoto?.username) {
      this.messageService.sendMessage(this.selectedPhoto.username, formattedMessage)
        ?.then(() => {
          console.log(`Anonymous message sent to ${this.selectedPhoto?.username}`);
        })
        .catch(error => {
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
    this.adminMessage = "";
    document.body.classList.add('modal-open');
  }

  closePhotoModal(): void {
    this.isModalOpen = false;
    document.body.classList.remove('modal-open');
    setTimeout(() => {
      this.selectedPhoto = null;
      this.isAnonymous = false;
      this.adminMessage = "";
    }, 300);
  }
}
