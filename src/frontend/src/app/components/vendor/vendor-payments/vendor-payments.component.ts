import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { VendorPaymentSummary } from 'src/app/model';
import { VendorService } from 'src/app/providers';

@Component({
  selector: 'app-vendor-payment',
  templateUrl: './vendor-payments.component.html',
})
export class VendorPaymentsComponent implements OnInit {
  displayedColumns: string[] = ['paymentId', 'paymentDate', 'Gross', 'Fees', 'Net'];
  dataSource: MatTableDataSource<VendorPaymentSummary>;
  @ViewChild(MatPaginator, { static: true })
  paginator: MatPaginator;
  @ViewChild(MatSort, { static: true })
  sort: MatSort;
  public workInProgress = false;
  private vendorId: string;

  constructor(private vendorService: VendorService, private route: ActivatedRoute) {}
  ngOnInit() {
    this.workInProgress = true;
    this.vendorId = this.route.snapshot.params.id;
    this.vendorService.getVendorPayments(this.vendorId).subscribe(
      (res) => {
        console.log(res);
        this.dataSource = new MatTableDataSource(res);
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
      },
      (err) => console.log('HTTP Error', err),
      () => {
        this.workInProgress = false;
      }
    );
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }
}
