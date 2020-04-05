import { Component, OnInit } from '@angular/core';
import { VendorService } from 'src/app/providers';
import { VendorDetail } from 'src/app/model';
import { ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-vendor-detail',
  templateUrl: './vendor-detail.component.html',
})
export class VendorDetailComponent implements OnInit {
  public vendorDetail: VendorDetail = null;

  constructor(
    private vendorService: VendorService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const vendorId = this.route.snapshot.params.id;
    this.vendorService.getVendor(vendorId).subscribe(
      (res) => {
        this.vendorDetail = res;
      },
      (err) => console.log('HTTP Error', err),
      () => console.log('HTTP request completed.')
    );
  }
}
