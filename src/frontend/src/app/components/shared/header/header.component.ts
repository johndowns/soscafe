import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import * as jwt from 'jwt-decode';
import * as _ from 'lodash';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
})
export class HeaderComponent implements OnInit {

  loggedIn = false;
  userName = '';
  userEmail = '';
  isAdmin;

  constructor(
    private router: Router,
    private location: Location,
  ) { }

  ngOnInit(): void {
    this.checkAccount();

    // this.broadcastService.subscribe('msal:loginSuccess', payload => {
    //   this.checkAccount();
    // });
  }

  checkAccount() {
    let t = this.getDecodedAccessToken();
    // Loose validation
    if (_.has(t, 'iss')) {
      this.userName = _.get(t, 'name');
      this.userEmail = _.get(t, 'emails.0');
      this.isAdmin = _.get(t, 'extension_IsAdmin', false);
      this.loggedIn = true;
    }
  }

  onSignIn() {
    // const isIE = window.navigator.userAgent.indexOf('MSIE ') > -1 || window.navigator.userAgent.indexOf('Trident/') > -1;
    // if (isIE) {
    //   this.authService.loginRedirect();
    // } else {
    //   this.authService.loginPopup();
    // }
  }

  onSignOut() {
    // this.authService.logout();
  }

  goToAdmin() {
    this.router.navigate(['/admin/businesses']);
  }

  onNewVendor() {
    this.router.navigate(['/new-vendor']);
  }

  getDecodedAccessToken(): any {
    try {
      return jwt(localStorage.getItem('access_token'));
    }
    catch (e) {
      console.log(e);
      return null;
    }
  }
}
