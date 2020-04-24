import { FlexLayoutModule } from '@angular/flex-layout';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { BrowserModule } from '@angular/platform-browser';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
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
import {
  AdminBusinessListComponent,
  AdminBusinessViewComponent,
  AdminBusinessDetailComponent,
  AdminBusinessPaymentsComponent,
  AdminBusinessVouchersComponent,
} from './components/admin';
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
    AdminBusinessListComponent,
    AdminBusinessViewComponent,
    AdminBusinessDetailComponent,
    AdminBusinessPaymentsComponent,
    AdminBusinessVouchersComponent,
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
    FlexLayoutModule
  ],
  providers: [
    VendorService
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
