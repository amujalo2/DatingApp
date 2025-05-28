import { Component, inject, input, OnInit, output } from '@angular/core';
import { Member } from '../../_models/member';
import { DecimalPipe, NgClass, NgFor, NgIf, NgStyle } from '@angular/common';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { AccountService } from '../../_services/account.service';
import { environment } from '../../../environments/environment';
import { Photo } from '../../_models/Photo';
import { MembersService } from '../../_services/members.service';
import { ToastrService } from 'ngx-toastr';
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
  editingPhotoId: number | null = null;
  selectedTagNames: string[] = [];
  availableTags: string[] = [];

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
    this.memberService.deletePhoto(photo).subscribe({
      next: _ => {
        const updatedMember = { ...this.member() };
        updatedMember.photos = updatedMember.photos.filter(p => p.id !== photo.id);
        this.memberChange.emit(updatedMember);
      }
    });
  }
  getUserPhotos() {
    this.memberService.getPhotosWithTags().subscribe({
      next: photos => {
        this.userPhotos = photos;
      },
    });
  }
  setMainPhoto(photo: Photo) {
    this.memberService.setMainPhoto(photo).subscribe({
      next: _ => {
        const user = this.accountService.currentUser();
        if (user) {
          user.photoUrl = photo.url;
          this.accountService.setCurrentUser(user);
        }
        const updatedMember = { ...this.member() };
        updatedMember.photoUrl = photo.url;
        updatedMember.photos.forEach(p => {
          if (p.isMain) p.isMain = false;
          if (p.id === photo.id) p.isMain = true;
        });
        this.memberChange.emit(updatedMember);
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
      const updatedMember = {...this.member()}
      updatedMember.photos.push(photo);
      this.memberChange.emit(updatedMember);
      if (photo.isMain){
        const user = this.accountService.currentUser();
        if (user) {
          user.photoUrl = photo.url;
          this.accountService.setCurrentUser(user);
        }
        updatedMember.photoUrl = photo.url;
        updatedMember.photos.forEach(p => {
          if (p.isMain) p.isMain = false;
          if (p.id === photo.id) p.isMain = true;
        });
        this.memberChange.emit(updatedMember);
      }
    }
  }
  trackPhoto(index: number, photo: Photo) {
    return photo.id;
  }
  loadAvailableTags() {
    this.memberService.getAllTags().subscribe({
      next: tags => (this.availableTags = tags),
      error: err => console.error('Failed to load tags', err),
    });
  }
  openTagEditor(photo: Photo) {
    if (this.editingPhotoId === photo.id) {
      this.editingPhotoId = null;
      this.selectedTagNames = [];
      return;
    }

    this.editingPhotoId = photo.id;
    this.selectedTagNames = photo.tags ? [...photo.tags] : [];

    if (!this.availableTags.length) {
      this.loadAvailableTags();
      console.log(this.availableTags);
    }
  }
  toggleTagEditor(photoId: number) {
    if (this.editingPhotoId === photoId) {
      this.editingPhotoId = null;
    } else {
      this.editingPhotoId = photoId;
    }
  }

  toggleTagSelection(tag: string) {
    const index = this.selectedTagNames.indexOf(tag);
    if (index > -1) {
      this.selectedTagNames.splice(index, 1);
    } else {
      this.selectedTagNames.push(tag);
    }
  }

  removeTagFromPhoto(photo: Photo, tag: string) {
    const updatedTags = photo.tags?.filter(t => t !== tag) || [];
    this.memberService.addTagToPhoto(photo.id, updatedTags).subscribe({
      next: () => {
        this.toastService.success('Tag removed successfully');
        photo.tags = updatedTags;
        const memberPhotoIndex = this.member().photos.findIndex(
          p => p.id === photo.id
        );
        if (memberPhotoIndex > -1) {
          this.member().photos[memberPhotoIndex].tags = [...updatedTags];
        }
        this.memberChange.emit({
          ...this.member(),
          photos: [...this.member().photos],
        });
      },
      error: err => {
        this.toastService.error('Failed to remove tag');
        console.error(err);
      },
    });
  }

  submitTagsForPhoto(photo: Photo) {
    this.memberService
      .addTagToPhoto(photo.id, this.selectedTagNames)
      .subscribe({
        next: () => {
          this.toastService.success('Tags updated successfully');
          const photoIndex = this.userPhotos.findIndex(p => p.id === photo.id);
          if (photoIndex > -1) {
            this.userPhotos[photoIndex].tags = [...this.selectedTagNames];
          }
          const memberPhotoIndex = this.member().photos.findIndex(
            p => p.id === photo.id
          );
          if (memberPhotoIndex > -1) {
            this.member().photos[memberPhotoIndex].tags = [
              ...this.selectedTagNames,
            ];
          }
          this.memberChange.emit({
            ...this.member(),
            photos: [...this.member().photos],
          });
          this.editingPhotoId = null;
          this.selectedTagNames = [];
        },
        error: err => {
          this.toastService.error('Failed to update tags');
          console.error(err);
        },
      });
  }

  getAllTags() {
    this.memberService.getAllTags().subscribe({
      next: tags => {
        this.availableTags = tags;
        console.log('Available tags:', this.availableTags);
        console.log(tags);
      },
      error: err => {
        console.error('Failed to load tags', err);
      },
    });
  }
}
