import { FlexLayoutModule } from '@angular/flex-layout';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { BrowserModule } from '@angular/platform-browser';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MsalModule, MsalInterceptor } from '@azure/msal-angular';
import { LogLevel, Logger, CryptoUtils } from 'msal';
import { isIE } from './config';

import { environment as env, environment } from '../environments/environment';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AppMaterialModule } from './core';
import { YesNoPipe } from './core/yes-nop-pipe';
import { DefaultLayoutComponent } from './components/layout';
import { VendorService } from 'src/app/providers';
import {
  SpinnerComponent,
  HeaderComponent,
  SidebarComponent,
} from './components/shared';
import {
  VendorListComponent,
  VendorViewComponent,
  VendorDetailComponent,
  VendorPaymentsComponent,
  VendorVouchersComponent,
  VendorNewSuccessComponent,
  VendorNewComponent,
} from './components/vendor';
import { HomeComponent } from './components/home/home.component';
import { ErrorComponent } from './components/shared/error/error.component';

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
    VendorViewComponent,
    VendorDetailComponent,
    VendorPaymentsComponent,
    VendorVouchersComponent,
    VendorNewComponent,
    VendorNewSuccessComponent,
    HomeComponent,
    YesNoPipe,
    ErrorComponent,
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
          clientId: env.msal.auth.clientId,
          authority: env.msal.auth.authority,
          validateAuthority: env.msal.auth.validateAuthority,
          redirectUri: env.appBaseUrl,
          postLogoutRedirectUri: env.appBaseUrl,
          navigateToLoginRequestUrl: env.msal.auth.navigateToLoginRequestUrl,
        },
        cache: {
          cacheLocation: 'localStorage',
          storeAuthStateInCookie: isIE
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
        consentScopes: env.msal.consentScopes,
        protectedResourceMap: [
          [
            env.apiBaseUrl,
            [
              env.msal.appScope
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
