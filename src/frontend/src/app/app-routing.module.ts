import { NgModule } from '@angular/core';
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
        redirectTo: 'vendors',
        pathMatch: 'full'
      }
    ],
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
