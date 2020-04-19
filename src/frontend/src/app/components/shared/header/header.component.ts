import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { MsalService, BroadcastService } from '@azure/msal-angular';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
})
export class HeaderComponent implements OnInit {

  loggedIn = false;
  userName = '';
  userEmail= '';
  isAdmin;

  constructor(
    private broadcastService: BroadcastService,
    private authService: MsalService,
    private router: Router,
    private location: Location,
  ) {}

  ngOnInit(): void {
    this.checkAccount();

    this.broadcastService.subscribe('msal:loginSuccess', payload => {
      this.checkAccount();
    });
  }

  checkAccount() {
    const userAccount = this.authService.getAccount();
    this.loggedIn = !!userAccount;
    console.log(userAccount);
    if (this.loggedIn) {
      this.userName = userAccount.name;
      this.userEmail = userAccount.idToken.emails[0];
      this.isAdmin = userAccount.idToken.extension_IsAdmin;
    }
  }

  onSignIn() {
    const isIE = window.navigator.userAgent.indexOf('MSIE ') > -1 || window.navigator.userAgent.indexOf('Trident/') > -1;
    if (isIE) {
      this.authService.loginRedirect();
    } else {
      this.authService.loginPopup();
    }
  }

  onSignOut() {
    this.authService.logout();
  }

  goToAdmin() {
    this.router.navigate(['/admin/businesses']);
  }

  onNewVendor() {
    this.router.navigate(['/new-vendor']);
  }
}
