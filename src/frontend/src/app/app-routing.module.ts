import { NgModule, Component } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
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
      },
      {
        path: 'vendors/:id',
        component: VendorDetailComponent,
      },
      {
        path: '',
        component: VendorListComponent
      }
    ],
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
