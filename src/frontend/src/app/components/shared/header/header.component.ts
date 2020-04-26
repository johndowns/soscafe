import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { environment } from '../../../../environments/environment'
import * as _ from 'lodash';
import { ConstantService } from 'src/app/services/constant.service';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
})
export class HeaderComponent implements OnInit {

  loggedIn = false;
  userName = '';
  userEmail = '';
  isAdmin;

  _: any = _;

  constructor(
    private router: Router,
    private location: Location,
    public constantService: ConstantService
  ) { }

  ngOnInit(): void {}

  onSignIn() {
  }

  onSignOut() {
    // Clearing local storage
    sessionStorage.clear();
    // Signing out of MS
    let url = `https://${environment.msal.tenant}.b2clogin.com/${environment.msal.tenant}.onmicrosoft.com/${environment.msal.policy}/oauth2/v2.0/logout?post_logout_redirect_uri=${environment.appBaseUrl}`;
    window.location.href = encodeURI(url);
  }

  goToAdmin() {
    this.router.navigate(['/admin/businesses']);
  }

  onNewVendor() {
    this.router.navigate(['/new-vendor']);
  }
}
