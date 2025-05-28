import { Component, inject, input, OnInit, output } from '@angular/core';
import { Member } from '../../_models/member';
import { DecimalPipe, NgClass, NgFor, NgIf, NgStyle } from '@angular/common';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { AccountService } from '../../_services/account.service';
import { environment } from '../../../environments/environment';
import { Photo } from '../../_models/Photo';
import { MembersService } from '../../_services/members.service';
import { ToastrService } from 'ngx-toastr';
import { Tag } from '../../_models/Tag';

@Component({
  selector: 'app-photo-editor',
  imports: [NgIf, NgFor, NgStyle, NgClass, FileUploadModule, DecimalPipe],
  templateUrl: './photo-editor.component.html',
  styleUrl: './photo-editor.component.css'
})
export class PhotoEditorComponent implements OnInit {
  private memberService = inject(MembersService);
  private accountService = inject(AccountService);
  private toastService = inject(ToastrService);
  
  member = input.required<Member>();
  userPhotos: Photo[] = [];
  uploader?: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  memberChange = output<Member>();
  
  // Modal properties
  showTagModal = false;
  selectedPhoto: Photo | null = null;
  selectedTagNames: string[] = [];
  availableTags: string[] = [];
  originalSelectedTags: string[] = [];

  ngOnInit(): void {
    if (!this.member().photoUrl) {
      this.member().photoUrl = 'https://th.bing.com/th/id/OIP.PoS7waY4-VeqgNuBSxVUogAAAA?rs=1&pid=ImgDetMain';
    }
    this.initializeUploader();
    this.getUserPhotos();
  }

  fileOverBase(e: any) {
    this.hasBaseDropZoneOver = e;
  }

  deletePhoto(photo: Photo) {
    if (confirm('Are you sure you want to delete this photo?')) {
      this.memberService.deletePhoto(photo).subscribe({
        next: _ => {
          // Remove from userPhotos array
          this.userPhotos = this.userPhotos.filter(p => p.id !== photo.id);
          
          // Update member object
          const updatedMember = { ...this.member() };
          updatedMember.photos = updatedMember.photos.filter(p => p.id !== photo.id);
          
          // If deleted photo was main, set first available photo as main
          if (photo.isMain && updatedMember.photos.length > 0) {
            updatedMember.photos[0].isMain = true;
            updatedMember.photoUrl = updatedMember.photos[0].url;
            
            // Update current user photo if needed
            const user = this.accountService.currentUser();
            if (user) {
              user.photoUrl = updatedMember.photos[0].url;
              this.accountService.setCurrentUser(user);
            }
          } else if (photo.isMain && updatedMember.photos.length === 0) {
            // If no photos left, set default photo
            updatedMember.photoUrl = 'https://th.bing.com/th/id/OIP.PoS7waY4-VeqgNuBSxVUogAAAA?rs=1&pid=ImgDetMain';
            
            const user = this.accountService.currentUser();
            if (user) {
              user.photoUrl = updatedMember.photoUrl;
              this.accountService.setCurrentUser(user);
            }
          }
          
          this.memberChange.emit(updatedMember);
          this.toastService.success('Photo deleted successfully');
        },
        error: err => {
          console.error('Error deleting photo:', err);
          this.toastService.error('Failed to delete photo');
        }
      });
    }
  }

  getUserPhotos() {
    this.memberService.getPhotosWithTags().subscribe({
      next: photos => {
        this.userPhotos = photos;
        console.log('User photos loaded:', this.userPhotos);
      },
      error: err => {
        console.error('Error loading photos:', err);
        this.toastService.error('Failed to load photos');
      }
    });
  }

  setMainPhoto(photo: Photo) {
    this.memberService.setMainPhoto(photo).subscribe({
      next: _ => {
        // Update current user photo
        const user = this.accountService.currentUser();
        if (user) {
          user.photoUrl = photo.url;
          this.accountService.setCurrentUser(user);
        }

        // Update member object
        const updatedMember = { ...this.member() };
        updatedMember.photoUrl = photo.url;
        updatedMember.photos.forEach(p => {
          p.isMain = p.id === photo.id;
        });

        // Update userPhotos array
        this.userPhotos.forEach(p => {
          p.isMain = p.id === photo.id;
        });

        this.memberChange.emit(updatedMember);
        this.toastService.success('Main photo updated successfully');
      },
      error: err => {
        console.error('Error setting main photo:', err);
        this.toastService.error('Failed to set main photo');
      }
    });
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/add-photo',  
      authToken: 'Bearer ' + this.accountService.currentUser()?.token,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 1 * 1024 * 1024, // 1 MB
    });
    
    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    };

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      const photo = JSON.parse(response);
      
      // Add to userPhotos array
      this.userPhotos.push(photo);
      
      // Update member object
      const updatedMember = {...this.member()}
      updatedMember.photos.push(photo);
      
      if (photo.isMain){
        const user = this.accountService.currentUser();
        if (user) {
          user.photoUrl = photo.url;
          this.accountService.setCurrentUser(user);
        }
        
        updatedMember.photoUrl = photo.url;
        updatedMember.photos.forEach(p => {
          p.isMain = p.id === photo.id;
        });
        
        // Update userPhotos array
        this.userPhotos.forEach(p => {
          p.isMain = p.id === photo.id;
        });
      }
      
      this.memberChange.emit(updatedMember);
      this.toastService.success('Photo uploaded successfully');
    };

    this.uploader.onErrorItem = (item, response, status, headers) => {
      console.error('Upload error:', response);
      this.toastService.error('Failed to upload photo');
    };
  }

  trackPhoto(index: number, photo: Photo) {
    return photo.id;
  }

  // Modal methods
  openTagModal(photo: Photo) {
    this.selectedPhoto = photo;
    this.selectedTagNames = photo.tags ? photo.tags.map(t => t.name) : [];
    this.originalSelectedTags = [...this.selectedTagNames];
    this.showTagModal = true;

    if (!this.availableTags.length) {
      this.loadAvailableTags();
    }
  }

  closeTagModal() {
    this.showTagModal = false;
    this.selectedPhoto = null;
    this.selectedTagNames = [];
    this.originalSelectedTags = [];
  }

  loadAvailableTags() {
    this.memberService.getAllTags().subscribe({
      next: tags => {
        this.availableTags = tags;
      },
      error: err => {
        console.error('Failed to load tags', err);
        this.toastService.error('Failed to load available tags');
      }
    });
  }

  toggleTagSelection(tag: string) {
    const index = this.selectedTagNames.indexOf(tag);
    if (index > -1) {
      this.selectedTagNames.splice(index, 1);
    } else {
      this.selectedTagNames.push(tag);
    }
  }

  savePhotoTags() {
    if (!this.selectedPhoto) return;

    this.memberService.addTagToPhoto(this.selectedPhoto.id, this.selectedTagNames).subscribe({
      next: () => {
        this.toastService.success('Tags updated successfully');
        
        // Update the photo in userPhotos array
        const photoIndex = this.userPhotos.findIndex(p => p.id === this.selectedPhoto!.id);
        if (photoIndex > -1) {
          this.userPhotos[photoIndex].tags = this.selectedTagNames.map(name => ({ name } as Tag));
        }
        
        // Update the photo in member's photos array
        const memberPhotoIndex = this.member().photos.findIndex(p => p.id === this.selectedPhoto!.id);
        if (memberPhotoIndex > -1) {
          const updatedMember = { ...this.member() };
          updatedMember.photos[memberPhotoIndex].tags = this.selectedTagNames.map(name => ({ name } as Tag));
          this.memberChange.emit(updatedMember);
        }
        
        this.closeTagModal();
      },
      error: err => {
        console.error('Error updating tags:', err);
        this.toastService.error('Failed to update tags');
      }
    });
  }

  removeTagFromPhoto(photo: Photo, tagName: string) {
    if (confirm(`Are you sure you want to remove the tag "${tagName}" from this photo?`)) {
      const updatedTags = photo.tags?.filter(t => t.name !== tagName).map(t => t.name) || [];
      this.memberService.addTagToPhoto(photo.id, updatedTags).subscribe({
        next: () => {
          this.toastService.success('Tag removed successfully');
          const photoIndex = this.userPhotos.findIndex(p => p.id === photo.id);
          if (photoIndex > -1) {
            this.userPhotos[photoIndex].tags = updatedTags.map(name => ({ name } as Tag));
          }
        },
        error: err => {
          console.error('Error removing tag:', err);
          this.toastService.error('Failed to remove tag');
        }
      });
    }
  }
}