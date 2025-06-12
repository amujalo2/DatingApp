import { Directive, inject, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { AuthStoreService } from '../_services/auth-store.service';

@Directive({
  selector: '[appHasRole]' //*appHasRole
})
export class HasRoleDirective implements OnInit{
  @Input() appHasRole: string[] = [];
  private authStoreService = inject(AuthStoreService);
  private viewContainerRef = inject(ViewContainerRef);
  private templateRef = inject(TemplateRef);
  
  ngOnInit(): void {
    if (this.authStoreService.roles()?.some((r: string) => this.appHasRole.includes(r))) {
      this.viewContainerRef.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainerRef.clear();
    }
  }
}
