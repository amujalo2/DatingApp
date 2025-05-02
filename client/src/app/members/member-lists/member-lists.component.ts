import { Component, inject, OnInit } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { MemberCardComponent } from "../../members/member-card/member-card.component";
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { FormsModule } from '@angular/forms';
import { ButtonsModule } from 'ngx-bootstrap/buttons';

@Component({
  selector: 'app-member-lists',
  imports: [MemberCardComponent, PaginationModule, FormsModule, ButtonsModule],
  templateUrl: './member-lists.component.html',
  styleUrl: './member-lists.component.css'
})
export class MemberListsComponent implements OnInit{
  membersService = inject(MembersService);
  genderList = [{value: 'male', display: 'Males'}, {value: 'female', display: 'Females'}]
  ngOnInit(): void {
    if (!this.membersService.paginatedResult()) this.loadMembers();
  }
  loadMembers() {
    this.membersService.getMembers();
  }
  resetFilters(){
    this.membersService.resetUserParams();
    this.loadMembers();
  }
  pageChange(event: any){
    if (this.membersService.userParams().pageNumber != event.page){
      this.membersService.userParams().pageNumber = event.page;
      this.loadMembers();
    }

  }
}
