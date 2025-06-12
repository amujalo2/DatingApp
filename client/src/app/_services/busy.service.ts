import { inject, Injectable } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BusyService {

  busyRequestCount = 0;
  private spinnerService = inject(NgxSpinnerService);
  private loadingSubject = new BehaviorSubject<boolean>(false);
  loading$ = this.loadingSubject.asObservable();

  busy() {
    this.busyRequestCount++;
    this.loadingSubject.next(true);
    this.spinnerService.show(undefined, {
      type: 'line-scale-pulse-out-rapid',
      size: 'medium',
      bdColor: 'rgba(247,228,177,0.5)',
      color: '#008080'
    });
  }

  idle() {
    this.busyRequestCount--;
    if (this.busyRequestCount <= 0) {
      this.busyRequestCount = 0;
      this.loadingSubject.next(false);
      this.spinnerService.hide();
    }
  }

}
