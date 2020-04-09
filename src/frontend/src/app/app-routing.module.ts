import { HomeComponent } from './components/home/home.component';
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';

import { DefaultLayoutComponent } from './components/layout';
import { VendorDetailComponent, VendorListComponent, VendorPaymentsComponent } from './components/vendor';

const routes: Routes = [
  {
    path: '',
    component: DefaultLayoutComponent,
    children: [
      {
        path: 'vendors',
        component: VendorListComponent,
        canActivate: [
          MsalGuard
        ],
      },
      {
        path: 'vendors/:id',
        component: VendorDetailComponent,
        canActivate: [
          MsalGuard
        ],
      },
      {
        path: 'vendors/:id/payments',
        component: VendorPaymentsComponent,
        canActivate: [
          MsalGuard
        ],
      },
      {
        path: '',
        component: HomeComponent,
      }
    ],
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
