import { Component, inject } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-roles-modal',
  imports: [],
  templateUrl: './roles-modal.component.html',
  styleUrl: './roles-modal.component.css'
})
export class RolesModalComponent {
  private bsModalRef = inject(BsModalRef);
  title = '';
  availableRoles: string[] = [];
  selectedRoles: string[] = [];
  username: string = ''
  rolesUpdated = false;
  updateChecked(checkValue: string) {
    if(this.selectedRoles.includes(checkValue)) {
      this.selectedRoles = this.selectedRoles.filter(r => r !== checkValue)
    } else {
      this.selectedRoles.push(checkValue);
    }
  }
  onSelectRoles() {
    this.rolesUpdated = true;
    this.bsModalRef.hide();
  }
  hide() {
    this.bsModalRef.hide();
  }
}
