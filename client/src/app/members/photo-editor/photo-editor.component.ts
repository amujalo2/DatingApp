import { Component, inject, input, OnInit, output } from '@angular/core';
import { Member } from '../../_models/member';
import { CommonModule, DecimalPipe, NgClass, NgFor, NgIf, NgStyle } from '@angular/common';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { environment } from '../../../environments/environment';
import { Photo } from '../../_models/Photo';
import { MembersService } from '../../_services/members.service';
import { ToastrService } from 'ngx-toastr';
import { Tag } from '../../_models/Tag';
import { TagModalComponent } from '../../modals/tag-modal/tag-modal.component';
import { AuthStoreService } from '../../_services/auth-store.service';
import { BehaviorSubject, combineLatest, map, Observable } from 'rxjs';

@Component({
  selector: 'app-photo-editor',
  imports: [NgIf, NgFor, NgStyle, NgClass, FileUploadModule, DecimalPipe, TagModalComponent, CommonModule],
  templateUrl: './photo-editor.component.html',
  styleUrl: './photo-editor.component.css'
})
export class PhotoEditorComponent implements OnInit {
  private memberService = inject(MembersService);
  private authStoreService = inject(AuthStoreService);
  private toastService = inject(ToastrService);
  
  private selectedTagsSubject = new BehaviorSubject<string[]>([]);
  private approvalStatusSubject = new BehaviorSubject<'all' | 'approved' | 'unapproved'>('all');
  private userPhotosSubject = new BehaviorSubject<Photo[]>([]);

  getSelectedTagsSubjectValue() {
    return this.selectedTagsSubject.value;
  }

  getApprovalStatusSubjectValue() {
    return this.approvalStatusSubject.value;
  }

  selectedTags$ = this.selectedTagsSubject.asObservable();
  approvalStatus$ = this.approvalStatusSubject.asObservable();
  userPhotos$ = this.userPhotosSubject.asObservable();

  filteredPhotos$: Observable<Photo[]> = combineLatest([
    this.userPhotos$,
    this.selectedTags$,
    this.approvalStatus$
  ]).pipe(
    map(([photos, selectedTags, approvalStatus]) => {
      return photos.filter(photo => {
        // Filter po statusu
        const statusMatch =
          approvalStatus === 'all' ||
          (approvalStatus === 'approved' && photo.isApproved) ||
          (approvalStatus === 'unapproved' && !photo.isApproved);

        // Filter po tagovima
        const tagsMatch =
          selectedTags.length === 0 ||
          (photo.tags && selectedTags.every(tag => photo.tags?.some(t => t.name === tag)));

        return statusMatch && tagsMatch;
      });
    })
  );

  member = input.required<Member>();
  userPhotos: Photo[] = [];
  uploader?: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  memberChange = output<Member>();
  
  // Modal properties
  showTagModal = false;
  selectedPhoto: Photo | null = null;

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
          this.userPhotos = this.userPhotos.filter(p => p.id !== photo.id);
          this.userPhotosSubject.next([...this.userPhotos]); 
          
          const updatedMember = { ...this.member() };
          updatedMember.photos = updatedMember.photos.filter(p => p.id !== photo.id);

          // If deleted photo was main, set first available photo as main
          if (photo.isMain && updatedMember.photos.length > 0) {
            updatedMember.photos[0].isMain = true;
            updatedMember.photoUrl = updatedMember.photos[0].url;
            
            // Update current user photo if needed
            const user = this.authStoreService.currentUser();
            if (user) {
              user.photoUrl = updatedMember.photos[0].url;
              this.authStoreService.setCurrentUser(user);
            }
          } else if (photo.isMain && updatedMember.photos.length === 0) {
            updatedMember.photoUrl = 'https://th.bing.com/th/id/OIP.PoS7waY4-VeqgNuBSxVUogAAAA?rs=1&pid=ImgDetMain';

            const user = this.authStoreService.currentUser();
            if (user) {
              user.photoUrl = updatedMember.photoUrl;
              this.authStoreService.setCurrentUser(user);
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
        this.userPhotosSubject.next(photos); 
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
        const user = this.authStoreService.currentUser();
        if (user) {
          user.photoUrl = photo.url;
          this.authStoreService.setCurrentUser(user);
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
      authToken: 'Bearer ' + this.authStoreService.currentUser()?.token,
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
      this.userPhotosSubject.next([...this.userPhotos]);
      // Update member object
      const updatedMember = {...this.member()}
      updatedMember.photos.push(photo);
      
      if (photo.isMain){
        const user = this.authStoreService.currentUser();
        if (user) {
          user.photoUrl = photo.url;
          this.authStoreService.setCurrentUser(user);
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
    this.showTagModal = true;
  }

  closeTagModal() {
    this.showTagModal = false;
    this.selectedPhoto = null;
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
  onPhotoUpdated(updatedPhoto: Photo) {
    const photoIndex = this.userPhotos.findIndex(p => p.id === updatedPhoto.id);
    if (photoIndex > -1) {
      this.userPhotos[photoIndex] = updatedPhoto;
      this.userPhotosSubject.next([...this.userPhotos]); 
    }
    
    const memberPhotoIndex = this.member().photos.findIndex(p => p.id === updatedPhoto.id);
    if (memberPhotoIndex > -1) {
      const updatedMember = { ...this.member() };
      updatedMember.photos[memberPhotoIndex] = updatedPhoto;
      this.memberChange.emit(updatedMember);
    }
  }
  setSelectedTags(tags: string[]) {
    this.selectedTagsSubject.next(tags);
  }

  setApprovalStatus(status: 'all' | 'approved' | 'unapproved') {
    this.approvalStatusSubject.next(status);
  }
  getAllUniqueTags(): string[] {
    const allTags = new Set<string>();
    this.userPhotos.forEach(photo => {
      photo.tags?.forEach(tag => allTags.add(tag.name));
    });
    return Array.from(allTags).sort();
  }
  isTagSelected(tagName: string): boolean {
    return this.selectedTagsSubject.value.includes(tagName);
  }
  toggleTag(tagName: string, isSelected: boolean): void {
    const currentTags = this.selectedTagsSubject.value;
    let updatedTags: string[];
    
    if (isSelected) {
      updatedTags = currentTags.includes(tagName) 
        ? currentTags 
        : [...currentTags, tagName];
    } else {
      updatedTags = currentTags.filter(tag => tag !== tagName);
    }
    
    this.selectedTagsSubject.next(updatedTags);
  }
  clearAllFilters(): void {
    this.selectedTagsSubject.next([]);
    this.approvalStatusSubject.next('all');
  }
}