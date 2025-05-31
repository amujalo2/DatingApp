import { NgFor, NgIf } from '@angular/common';
import { Component, inject, OnInit, signal, input, output } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { ToastrService } from 'ngx-toastr';
import { Photo } from '../../_models/Photo';
import { Tag } from '../../_models/Tag';

@Component({
  selector: 'app-tag-modal',
  imports: [NgIf, NgFor],
  templateUrl: './tag-modal.component.html',
  styleUrls: ['./tag-modal.component.css'],
  standalone: true
})
export class TagModalComponent implements OnInit {
  private memberService = inject(MembersService);
  private toastrService = inject(ToastrService);
  
  // Inputs
  photo = input<Photo | null>(null);
  isVisible = input<boolean>(false);
  
  // Outputs
  modalClosed = output<void>();
  photoUpdated = output<Photo>();
  
  // Internal state
  availableTags = signal<string[]>([]);
  selectedTagNames = signal<string[]>([]);
  isLoading = signal<boolean>(false);
  originalSelectedTags: string[] = [];

  ngOnInit(): void {
    this.loadAvailableTags();
  }

  ngOnChanges(): void {
    // Reset modal state when photo changes
    if (this.photo()) {
      this.initializeSelectedTags();
    }
  }

  private initializeSelectedTags(): void {
    const currentPhoto = this.photo();
    if (currentPhoto?.tags) {
      const tagNames = currentPhoto.tags.map(t => t.name);
      this.selectedTagNames.set([...tagNames]);
      this.originalSelectedTags = [...tagNames];
    } else {
      this.selectedTagNames.set([]);
      this.originalSelectedTags = [];
    }
  }

  private loadAvailableTags(): void {
    this.memberService.getAllTags().subscribe({
      next: (tags) => {
        this.availableTags.set(tags);
      },
      error: (err) => {
        console.error('Failed to load tags', err);
        this.toastrService.error('Failed to load available tags');
      }
    });
  }

  toggleTagSelection(tagName: string): void {
    const currentTags = this.selectedTagNames();
    const index = currentTags.indexOf(tagName);
    
    if (index > -1) {
      // Remove tag
      const updatedTags = currentTags.filter(t => t !== tagName);
      this.selectedTagNames.set(updatedTags);
    } else {
      // Add tag
      this.selectedTagNames.set([...currentTags, tagName]);
    }
  }

  isTagSelected(tagName: string): boolean {
    return this.selectedTagNames().includes(tagName);
  }

  hasChanges(): boolean {
    const current = [...this.selectedTagNames()].sort();
    const original = [...this.originalSelectedTags].sort();
    return JSON.stringify(current) !== JSON.stringify(original);
  }

  closeModal(): void {
    this.resetModalState();
    this.modalClosed.emit();
  }

  saveChanges(): void {
    const currentPhoto = this.photo();
    if (!currentPhoto) {
      this.toastrService.error('No photo selected');
      return;
    }

    if (!this.hasChanges()) {
      this.closeModal();
      return;
    }

    this.isLoading.set(true);
    
    this.memberService.addTagToPhoto(currentPhoto.id, this.selectedTagNames()).subscribe({
      next: () => {
        this.toastrService.success('Tags updated successfully');
        
        // Create updated photo object
        const updatedPhoto: Photo = {
          ...currentPhoto,
          tags: this.selectedTagNames().map(name => ({ name } as Tag))
        };
        
        this.photoUpdated.emit(updatedPhoto);
        this.closeModal();
      },
      error: (err) => {
        console.error('Error updating tags:', err);
        this.toastrService.error('Failed to update tags');
      },
      complete: () => {
        this.isLoading.set(false);
      }
    });
  }

  private resetModalState(): void {
    this.selectedTagNames.set([]);
    this.originalSelectedTags = [];
    this.isLoading.set(false);
  }
}