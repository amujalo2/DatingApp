import { Component, EventEmitter, inject, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Photo } from '../../_models/Photo';
import { AdminService } from '../../_services/admin.service';
import { MessageService } from '../../_services/message.service';
import { AccountService } from '../../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { AuthStoreService } from '../../_services/auth-store.service';

@Component({
  selector: 'app-photo-modal',
  imports: [CommonModule, FormsModule],
  templateUrl: './photo-modal.component.html',
  styleUrl: './photo-modal.component.css'
})
export class PhotoModalComponent implements OnChanges {
  @Input() selectedPhoto: Photo | null = null;
  @Input() isModalOpen: boolean = false;
  @Output() closeModal = new EventEmitter<void>();
  @Output() photoApproved = new EventEmitter<number>();
  @Output() photoRejected = new EventEmitter<number>();

  private adminService = inject(AdminService);
  private messageService = inject(MessageService);
  private toastrService = inject(ToastrService);
  private authStoreService = inject(AuthStoreService);
  isAnonymous: boolean = false;
  adminMessage: string = '';

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isModalOpen'] && this.isModalOpen && this.selectedPhoto) {
      this.openModal();
    } else if (changes['isModalOpen'] && !this.isModalOpen) {
      this.closeModalInternal();
    }
  }

  private openModal(): void {
    this.isAnonymous = false;
    this.adminMessage = '';
    const user = this.authStoreService.currentUser();
    if (!user || !this.selectedPhoto) return;
    this.messageService.createHubConnection(user, this.selectedPhoto.username);
    document.body.classList.add('modal-open');
  }

  closePhotoModal(): void {
    this.closeModal.emit();
  }

  private closeModalInternal(): void {
    this.messageService.stopHubConnection();
    document.body.classList.remove('modal-open');
    setTimeout(() => {
      this.isAnonymous = false;
      this.adminMessage = '';
    }, 300);
  }

  approvePhoto(): void {
    if (!this.selectedPhoto) return;
    
    if (this.isAnonymous) {
      if (this.adminMessage !== '') {
        this.sendMessage();
      } else {
        this.toastrService.error('Type in the message, that you have decided to send!');
        return;
      }
    }

    this.adminService.approvePhoto(this.selectedPhoto.id).subscribe({
      next: () => {
        this.photoApproved.emit(this.selectedPhoto!.id);
        this.closePhotoModal();
      },
      error: (er) => {
        this.toastrService.error('Failed to approve photo');
        console.log(er);
      }
    });
  }

  rejectPhoto(): void {
    if (!this.selectedPhoto) return;
    
    if (this.isAnonymous) {
      if (this.adminMessage !== '') {
        this.sendMessage();
      } else {
        this.toastrService.error('Type in the message, that you have decided to send!');
        return;
      }
    }

    this.adminService.rejectPhoto(this.selectedPhoto.id).subscribe({
      next: () => {
        this.photoRejected.emit(this.selectedPhoto!.id);
        this.closePhotoModal();
      },
      error: (er) => {
        this.toastrService.error('Failed to reject photo');
        console.log(er);
      }
    });
  }

  private sendMessage(): void {
    const formattedMessage = `Regarding your photo: ${this.adminMessage}`;
    if (this.selectedPhoto?.username) {
      const user = this.authStoreService.currentUser();
      if (!user) {
        this.toastrService.error('You must be logged in to send messages');
        return;
      }
      this.messageService
        .sendMessage(this.selectedPhoto.username, formattedMessage)
        ?.then(() => {
          console.log(`Anonymous message sent to ${this.selectedPhoto?.username}`);
        })
        .catch((error) => {
          console.error('Error sending message:', error);
          this.toastrService.error('Failed to send notification message');
        });
    } else {
      this.toastrService.error('No username selected for the photo');
    }
  }
}