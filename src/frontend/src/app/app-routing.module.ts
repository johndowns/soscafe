import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';

import { DefaultLayoutComponent } from './components/layout';
import { VendorDetailComponent, VendorListComponent } from './components/vendor';

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
        path: '',
        component: VendorListComponent,
        canActivate: [
          MsalGuard
        ],
      }
    ],
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {useHash: true})],
  exports: [RouterModule]
})
export class AppRoutingModule { }
