import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-vendor-new-success',
  templateUrl: './vendor-new-success.component.html',
})
export class VendorNewSuccessComponent implements OnInit {

  businessName: string;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
  ) {
    this.route.queryParams.subscribe(params => {
      this.businessName = params['businessName'];
    });
  }

  ngOnInit() {
    setTimeout(() => {
        this.router.navigate(['/']);
    }, 10000);  //5s
  }
}
