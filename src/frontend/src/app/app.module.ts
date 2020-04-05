import { VendorService } from 'src/app/providers';
import { FlexLayoutModule } from '@angular/flex-layout';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';

import { AppComponent } from './app.component';
import { AppMaterialModule } from './core';
import { DefaultLayoutComponent } from './components/layout';
import {
  SpinnerComponent,
  HeaderComponent,
  SidebarComponent,
} from './components/shared';
import {
  VendorDetailComponent,
  VendorListComponent,
} from './components/vendor';
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    AppComponent,
    DefaultLayoutComponent,
    SpinnerComponent,
    HeaderComponent,
    SidebarComponent,
    VendorListComponent,
    VendorDetailComponent,
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    AppMaterialModule,
    FlexLayoutModule,
  ],
  providers: [VendorService],
  bootstrap: [AppComponent],
})
export class AppModule {}
