import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRoute } from '@angular/router';
import { environment } from '../../environments/environment'
import { ConstantService } from '../services/constant.service';
import * as _ from 'lodash';
import { v4 as uuidv4 } from 'uuid';
import * as jwt from 'jwt-decode';


@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  signedIn: boolean = false;

  constructor(
    public router: Router,
    public constantService: ConstantService,
    public activatedRoute: ActivatedRoute
  ) {
    if (sessionStorage.hasOwnProperty('access_token')) {
      // Check for token validity
      if(Number(sessionStorage.getItem('expires')) < Math.floor(Date.now()/1000)){
        sessionStorage.clear();
        this.signedIn = false;
      }
      else{
        this.getDecodedAccessToken();
        this.signedIn = true;
      }
    }
    else {
      this.activatedRoute.queryParams.subscribe(params => {        
        if(_.has(params, 'access_token')){
          // Access token acquired
          sessionStorage.setItem('access_token',params['access_token']);
          // Subtracting 3 seconds for precaution
          let decoded_token = this.getDecodedAccessToken();
          sessionStorage.setItem('expires', decoded_token.exp.toString());          
          // Signed in
          this.signedIn = true;
        }
        else{
          this.signedIn = false;
        }
      });
    }
  }

  canActivate() {    
    if (!this.signedIn) {
      let auth_url = `https://${environment.msal.tenant}.b2clogin.com/${environment.msal.tenant}.onmicrosoft.com/${environment.msal.policy}/oauth2/v2.0/authorize?client_id=${environment.msal.auth.clientId}&response_type=token&redirect_uri=${environment.appBaseUrl}&response_mode=query&scope=${environment.msal.consentScopes.join(' ')}&state=d748356b-1aed-4894-968d-0c356c4ab077&nonce=${uuidv4()}`;

      window.location.href = encodeURI(auth_url);      
    }
    return this.signedIn;
  }

  getDecodedAccessToken(): any {
    try {
      let j = jwt(sessionStorage.getItem('access_token'));
      _.set(this.constantService, 'userName', _.get(j,'name'));
      _.set(this.constantService, 'userEmail', _.get(j,'emails.0'));
      _.set(this.constantService, 'isAdmin', _.get(j,'extension_IsAdmin', false));
      _.set(this.constantService, 'loggedIn', true);
      
      return j;
    }
    catch (e) {
      console.log(e);
      return null;
    }
  }

}
