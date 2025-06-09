import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { HasRoleDirective } from '../../_directives/has-role.directive';

import { AdminUserManagementComponent } from '../admin-user-management/admin-user-management.component';
import { AdminPhotoManagementComponent } from '../admin-photo-management/admin-photo-management.component';

@Component({
  selector: 'app-admin-panel',
  imports: [TabsModule, HasRoleDirective, AdminUserManagementComponent, AdminPhotoManagementComponent],
  templateUrl: './admin-panel.component.html',
  styleUrl: './admin-panel.component.css'
})
export class AdminPanelComponent {
  
}
