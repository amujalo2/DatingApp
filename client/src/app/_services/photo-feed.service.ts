// photo-feed.service.ts
import { Injectable, inject, OnDestroy } from '@angular/core';
import { 
  interval, 
  Observable, 
  switchMap, 
  distinctUntilChanged, 
  shareReplay, 
  startWith,
  catchError,
  of,
  Subject,
  takeUntil,
  map
} from 'rxjs';
import { Photo } from '../_models/Photo';
import { MembersService } from './members.service';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class PhotoFeedService implements OnDestroy {
  private membersService = inject(MembersService);
  private destroy$ = new Subject<void>();
  private isDestroyed = false;
  
  constructor(private http: HttpClient) {}
  
  private getApprovedPhotos(): Observable<Photo[]> {
    return this.http
      .get<Photo[]>(this.membersService.baseUrl + 'users/approved-photos')
      .pipe(
        map(photos => photos.filter(photo => photo.isApproved)),
        catchError(error => {
          console.error('Error fetching photos:', error);
          return of([]);
        })
      );
  }
  
  approvedPhotoFeed$: Observable<Photo[]> = interval(10000).pipe(
    startWith(0),
    switchMap(() => {
      if (this.isDestroyed) {
        return of([]);
      }
      return this.getApprovedPhotos();
    }),
    distinctUntilChanged((prev, curr) => JSON.stringify(prev) === JSON.stringify(curr)),
    shareReplay({ bufferSize: 1, refCount: true }), 
    takeUntil(this.destroy$)
  );

  getPhotosCount$(): Observable<number> {
    return this.approvedPhotoFeed$.pipe(
      map(photos => photos.length)
    );
  }

  getPhotosByMember$(username: string): Observable<Photo[]> {
    return this.approvedPhotoFeed$.pipe(
      map(photos => photos.filter(photo => photo.username === username))
    );
  }

  destroy(): void {
    if (!this.isDestroyed) {
      this.isDestroyed = true;
      this.destroy$.next();
      this.destroy$.complete();
    }
  }

  ngOnDestroy(): void {
    this.destroy();
  }
}