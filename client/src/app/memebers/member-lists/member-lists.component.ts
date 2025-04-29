import { Component, inject, OnInit } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { Member } from '../../_models/member';
import { MemberCardComponent } from "../../members/member-card/member-card.component";

@Component({
  selector: 'app-member-lists',
  imports: [MemberCardComponent],
  templateUrl: './member-lists.component.html',
  styleUrl: './member-lists.component.css'
})
export class MemberListsComponent implements OnInit{

  private membersService = inject(MembersService);
  members: Member[] = [];
  ngOnInit(): void {
   this.loadMembers();
  }
  loadMembers() {
    this.membersService.getMembers().subscribe({
      next: members => this.members = members
    });
  }
}
