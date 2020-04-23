import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import * as jwt from 'jwt-decode';
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
}
