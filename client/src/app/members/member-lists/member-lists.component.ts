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
  membersService = inject(MembersService);
  members: Member[] = [];
  ngOnInit(): void {
    if (this.membersService.members().length === 0)
      this.loadMembers();
  }
  loadMembers() {
    this.membersService.getMembers();
  }
}
