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
  BehaviorSubject,
  takeUntil,
  Subject
} from 'rxjs';
import { Photo } from '../_models/Photo';
import { MembersService } from './members.service';

@Injectable({
  providedIn: 'root'
})
export class PhotoFeedService implements OnDestroy {
  private membersService = inject(MembersService);
  private destroy$ = new Subject<void>();
  
  // Control stream state
  private isStreamActive$ = new BehaviorSubject<boolean>(false);
  
  photoFeed$: Observable<Photo[]> = interval(10000).pipe(
    startWith(0), 
    switchMap(() => this.membersService.getPhotosWithTags()),
    distinctUntilChanged((prev, curr) => 
      JSON.stringify(prev) === JSON.stringify(curr)
    ),
    catchError(error => {
      console.error('Error fetching photos:', error);
      return of([]);
    }),
    shareReplay(1), 
    takeUntil(this.destroy$)
  );

  // Filtered stream for approved photos only
  approvedPhotoFeed$: Observable<Photo[]> = this.photoFeed$.pipe(
    switchMap(photos => of(photos.filter(photo => photo.isApproved))),
    shareReplay(1)
  );

  // Stream control methods
  startPhotoFeed(): void {
    this.isStreamActive$.next(true);
  }

  stopPhotoFeed(): void {
    this.isStreamActive$.next(false);
  }

  getStreamStatus(): Observable<boolean> {
    return this.isStreamActive$.asObservable();
  }

  // Get latest photos count
  getPhotosCount$(): Observable<number> {
    return this.approvedPhotoFeed$.pipe(
      switchMap(photos => of(photos.length)),
      shareReplay(1)
    );
  }

  // Get photos by specific member
  getPhotosByMember$(username: string): Observable<Photo[]> {
    return this.approvedPhotoFeed$.pipe(
      switchMap(photos => of(photos.filter(photo => 
        photo.username === username
      ))),
      shareReplay(1)
    );
  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.isStreamActive$.complete();
  }
}