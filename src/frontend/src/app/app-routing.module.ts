import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';

import { DefaultLayoutComponent } from './components/layout';
import { HomeComponent } from './components/home/home.component';
import { VendorNewComponent, VendorNewSuccessComponent, VendorListComponent, VendorViewComponent } from './components/vendor';
import { AdminBusinessListComponent, AdminBusinessViewComponent } from './components/admin';
import { ErrorComponent } from './components/shared/error/error.component';

const routes: Routes = [
  {
    path: '',
    component: DefaultLayoutComponent,
    children: [
      {
        path: 'vendors',
        component: VendorListComponent,
        canActivate: [MsalGuard],
      },
      {
        path: 'vendors/:id',
        component: VendorViewComponent,
        canActivate: [MsalGuard],
      },
      {
        path: 'new-vendor',
        component: VendorNewComponent,
        canActivate: [MsalGuard],
      },
      {
        path: 'new-vendor/success',
        component: VendorNewSuccessComponent,
        canActivate: [MsalGuard],
      },
      {
        path: 'admin/businesses',
        component: AdminBusinessListComponent,
        canActivate: [MsalGuard],
      },
      {
        path: 'admin/businesses/:id',
        component: AdminBusinessViewComponent,
        canActivate: [MsalGuard],
      },
      {
        path: '',
        component: HomeComponent,
      },
      {
        path: 'error',
        component: ErrorComponent,
      },
      {
        path: '**',
        redirectTo: '/error',
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
