// photo-dashboard.component.ts
import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule, NgFor, NgIf } from '@angular/common';
import { PhotoFeedService } from '../../_services/photo-feed.service';
import { Photo } from '../../_models/Photo';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-photo-dashboard',
  standalone: true,
  imports: [CommonModule, NgFor, NgIf],
  templateUrl: './photo-dashboard.component.html',
  styleUrl: './photo-dashboard.component.css'
})
export class PhotoDashboardComponent implements OnInit, OnDestroy {
  private photoFeedService = inject(PhotoFeedService);
  private destroy$ = new Subject<void>();

  photos: Photo[] = [];
  loading = true;

  ngOnInit(): void {
    this.photoFeedService.approvedPhotoFeed$
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (photos) => {
          this.photos = photos;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading photos:', error);
          this.loading = false;
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.photoFeedService.destroy();
  }
}