import { Component, OnInit, inject } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { PhotoApprovalStats } from '../../_models/photoApprovalStats';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-photo-stats',
  templateUrl: './admin-photo-stats.component.html',
  standalone: true,
  imports: [CommonModule],
})
export class AdminPhotoStatsComponent implements OnInit {
  private adminService = inject(AdminService);
  usersWithoutMainPhoto: string[] = [];
  photoStats: PhotoApprovalStats[] = [];

  ngOnInit(): void {
    this.getUsersWithoutMainPhoto();
    this.getPhotoStats();
  }

  getUsersWithoutMainPhoto() {
    this.adminService.getUsersWithoutMainPhoto().subscribe({
      next: (users) => {
        this.usersWithoutMainPhoto = users;
      },
    });
  }

  getPhotoStats() {
    this.adminService.getPhotoStats().subscribe({
      next: (stats) => {
        this.photoStats = stats;
      },
    });
  }
}
