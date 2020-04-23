import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, Router, ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';
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
    console.log(this.router.url);
    if (localStorage.hasOwnProperty('token')) {
      // Check for token validity
      this.signedIn = true;
    }
    else {
      this.activatedRoute.queryParams.subscribe(params => {
        if(_.has(params,'id_token')){
          const id_token = params['id_token'];
          let tokenInfo = this.getDecodedAccessToken(id_token); // decode token
          console.log('Token Info', tokenInfo);
          localStorage.setItem('token', JSON.stringify(tokenInfo));
          this.signedIn = true;
        }
        else{
          this.signedIn = false;
        }
      });
    }
  }

  canActivate() {
    alert(this.signedIn + '---->' + this.router.url);
    if (!this.signedIn) {
      let auth_url = `https://${environment.msal.tenant}.b2clogin.com/${environment.msal.tenant}.onmicrosoft.com/${environment.msal.policy}/oauth2/v2.0/authorize?client_id=${environment.msal.auth.clientId}&response_type=id_token&redirect_uri=${environment.appBaseUrl}&response_mode=query&scope=${environment.msal.consentScopes.join(' ')}&state=d748356b-1aed-4894-968d-0c356c4ab077&nonce=${uuidv4()}`;
      window.location.href = encodeURI(auth_url);
    }
    return this.signedIn;
  }

  getDecodedAccessToken(token: string): any {
    try{
        return jwt(token);
    }
    catch(Error){
        return null;
    }
  }

}
