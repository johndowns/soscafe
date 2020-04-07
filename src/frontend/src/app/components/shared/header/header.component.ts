import { Component, OnInit } from '@angular/core';
import { MsalService, BroadcastService } from '@azure/msal-angular';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
})
export class HeaderComponent implements OnInit {

  loggedIn = false;
  userName = '';

  constructor(
    private broadcastService: BroadcastService,
    private authService: MsalService) {}

  ngOnInit(): void {
    this.checkAccount();

    this.broadcastService.subscribe('msal:loginSuccess', payload => {
      this.checkAccount();
    });
  }

  checkAccount() {
    const userAccount = this.authService.getAccount();
    this.loggedIn = !!userAccount;
    if (this.loggedIn) {
      this.userName = userAccount.name;
    }

    console.log(this.authService.getAccount());
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
}
