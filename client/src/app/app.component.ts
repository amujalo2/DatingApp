import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavComponent } from "./nav/nav.component";
import { AccountService } from './_services/account.service';
import { NgxSpinnerComponent } from 'ngx-spinner';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavComponent, NgxSpinnerComponent, FontAwesomeModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit{
  private accountService = inject(AccountService);
  ngOnInit(): void {
    this.setCurrentUser();
  }
  setCurrentUser() {
    const userStirng = localStorage.getItem('user');
    if (!userStirng) return
    const user = JSON.parse(userStirng);
    this.accountService.setCurrentUser(user);
  }
  
}
