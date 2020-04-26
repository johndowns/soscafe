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
      // Freshness
      if(this.tokenIsFresh()){
        sessionStorage.clear();
        this.signedIn = false;
      }
      else{
        this.setUser();
        this.signedIn = true;
      }
    }
    else {
      this.activatedRoute.queryParams.subscribe(params => {        
        if(_.has(params, 'access_token')){
          // Checking nonce
          if(this.tokenHasValidNonce(params['access_token'], sessionStorage.getItem('nonce'))){
            // Access token acquired
            sessionStorage.setItem('access_token',params['access_token']);
            // Subtracting 3 seconds for precaution
            let decoded_token = this.getDecodedAccessToken(params['access_token']);
            sessionStorage.setItem('expires', decoded_token.exp.toString());  
            // Set user
            this.setUser();
            // Signed in
            this.signedIn = true;
          }
          else{
            this.signedIn = false;
          }
        }
        else{
          this.signedIn = false;
        }
      });
    }
  }

  canActivate() {    
    if (!this.signedIn) {
      let nonce = uuidv4();
      sessionStorage.setItem('nonce', nonce);
      let auth_url = `https://${environment.msal.tenant}.b2clogin.com/${environment.msal.tenant}.onmicrosoft.com/${environment.msal.policy}/oauth2/v2.0/authorize?client_id=${environment.msal.auth.clientId}&response_type=token&redirect_uri=${environment.appBaseUrl}&response_mode=query&scope=${environment.msal.consentScopes.join(' ')}&state=${nonce}&nonce=${nonce}`;

      window.location.href = encodeURI(auth_url);      
    }
    return this.signedIn;
  }

  tokenIsFresh(){
    return Number(sessionStorage.getItem('expires')) < Math.floor(Date.now()/1000);
  }

  tokenHasValidNonce(token, nonce){
    return nonce === _.get(this.getDecodedAccessToken(token), 'nonce', false);
  }

  setUser(){
    let j = this.getDecodedAccessToken(sessionStorage.getItem('access_token'));
    _.set(this.constantService, 'userName', _.get(j,'name'));
    _.set(this.constantService, 'userEmail', _.get(j,'emails.0'));
    _.set(this.constantService, 'isAdmin', _.get(j,'extension_IsAdmin', false));
    _.set(this.constantService, 'loggedIn', true);
  }

  getDecodedAccessToken(token): any {
    try {
      return jwt(token);
    }
    catch (e) {
      console.log(e);
      return null;
    }
  }

}
