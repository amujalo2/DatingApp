import { Component, HostListener, inject, OnInit, ViewChild } from '@angular/core';
import { Member } from '../../_models/member';
import { AccountService } from '../../_services/account.service';
import { MembersService } from '../../_services/members.service';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { FormsModule, NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { PhotoEditorComponent } from "../photo-editor/photo-editor.component";
import { DatePipe } from '@angular/common';
import { TimeagoModule } from 'ngx-timeago';
import { AuthStoreService } from '../../_services/auth-store.service';

@Component({
  selector: 'app-member-edit',
  imports: [TabsModule, FormsModule, PhotoEditorComponent, DatePipe, TimeagoModule],
  templateUrl: './member-edit.component.html',
  styleUrl: './member-edit.component.css'
})
export class MemberEditComponent implements OnInit {
  @ViewChild('editForm') editForm?: NgForm;
  @HostListener('window:beforeunload', ['$event']) notify($event: any) {
    if (this.editForm?.dirty) {
      $event.returnValue = true;
    }
  }
  member?: Member;
  private authStoreService = inject(AuthStoreService);
  private memberService = inject(MembersService);
  private toastr = inject(ToastrService);
  ngOnInit(): void {
    this.loadMember();
  }
  loadMember() {
    const user = this.authStoreService.currentUser();
    if(!user) return;
    this.memberService.getMember(user.username).subscribe(member => {
      this.member = member;
    });
  }
  updateMember() {
    this.memberService.updateMember(this.editForm?.value).subscribe({
      next: () => {
        this.toastr.success('Profile updated successfully!');
        this.editForm?.reset(this.member);
      }
    });
    this.toastr.success('Profile updated successfully!');
    this.editForm?.reset(this.member);
  }

  onMemberChange(event: Member) {
    this.member = event;
  }
}
