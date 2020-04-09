import { FlexLayoutModule } from '@angular/flex-layout';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { BrowserModule } from '@angular/platform-browser';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MsalModule, MsalInterceptor } from '@azure/msal-angular';
import { LogLevel, Logger, CryptoUtils } from 'msal';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AppMaterialModule } from './core';
import { DefaultLayoutComponent } from './components/layout';
import { VendorService } from 'src/app/providers';
import {
  SpinnerComponent,
  HeaderComponent,
  SidebarComponent,
} from './components/shared';
import {
  VendorDetailComponent,
  VendorListComponent,
  VendorPaymentsComponent,
} from './components/vendor';

export function loggerCallback(logLevel, message, piiEnabled) {
  console.log(message);
}

@NgModule({
  declarations: [
    AppComponent,
    DefaultLayoutComponent,
    SpinnerComponent,
    HeaderComponent,
    SidebarComponent,
    VendorListComponent,
    VendorDetailComponent,
    VendorPaymentsComponent,
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    AppMaterialModule,
    FlexLayoutModule,
    MsalModule.forRoot(
      {
        auth: {
          clientId: '1cc0426e-f8d7-4ddb-94b5-18185c09a6bd',
          authority:
            'https://soscafe.b2clogin.com/tfp/soscafe.onmicrosoft.com/b2c_1_signupsignin',
          validateAuthority: false,
          redirectUri: window.location.origin,
          postLogoutRedirectUri: window.location.origin,
          navigateToLoginRequestUrl: true,
        },
        cache: {
          cacheLocation: 'localStorage',
          storeAuthStateInCookie: true, // set to true for IE 11
        },
        framework: {
          isAngular: true,
        },
        system: {
          logger: new Logger(loggerCallback, {
              correlationId: CryptoUtils.createNewGuid(),
              level: LogLevel.Verbose,
              piiLoggingEnabled: true,
          })
        },
      },
      {
        popUp: false,
        consentScopes: [
          'openid',
          'profile',
          'https://soscafe.onmicrosoft.com/vendorfunctionsapis/user_impersonation',
        ],
        protectedResourceMap: [
          [
            'https://vendorapi.soscafe.nz/',
            [
              'https://soscafe.onmicrosoft.com/vendorfunctionsapis/user_impersonation'
            ]
          ]
        ],
        extraQueryParameters: {},
      }
    ),
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: MsalInterceptor,
      multi: true,
    },
    VendorService,
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
