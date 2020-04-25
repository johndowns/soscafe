import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { DefaultLayoutComponent } from './components/layout';
import { HomeComponent } from './components/home/home.component';
import { VendorNewComponent, VendorNewSuccessComponent, VendorListComponent, VendorViewComponent } from './components/vendor';
import { AdminBusinessListComponent, AdminBusinessViewComponent } from './components/admin';
import { ErrorComponent } from './components/shared/error/error.component';
import { AuthGuard } from './guard/auth.guard';

const routes: Routes = [
  {
    path: '',
    component: DefaultLayoutComponent,
    children: [
      {
        path: 'vendors',
        component: VendorListComponent,
        canActivate: [AuthGuard],
      },
      {
        path: 'vendors/:id',
        component: VendorViewComponent,
        canActivate: [AuthGuard],
      },
      {
        path: 'new-vendor',
        component: VendorNewComponent,
        canActivate: [AuthGuard],
      },
      {
        path: 'new-vendor/success',
        component: VendorNewSuccessComponent,
        canActivate: [AuthGuard],
      },
      {
        path: 'admin/businesses',
        component: AdminBusinessListComponent,
        canActivate: [AuthGuard],
      },
      {
        path: 'admin/businesses/:id',
        component: AdminBusinessViewComponent,
        canActivate: [AuthGuard],
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
