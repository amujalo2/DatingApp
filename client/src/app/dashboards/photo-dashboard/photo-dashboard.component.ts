import { Component, inject } from '@angular/core';
import { CommonModule, NgFor, NgIf } from '@angular/common';
import { PhotoFeedService } from '../../_services/photo-feed.service';
import { Photo } from '../../_models/Photo';

@Component({
  selector: 'app-photo-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './photo-dashboard.component.html',
  styleUrl: './photo-dashboard.component.css'
})
export class PhotoDashboardComponent {
  // private photoFeedService = inject(PhotoFeedService)
  // photos$ = this.photoFeedService.getPhotoFeed();
}