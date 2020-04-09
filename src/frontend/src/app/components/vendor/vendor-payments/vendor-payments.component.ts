import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { VendorPaymentSummary } from 'src/app/model';
import { VendorService } from 'src/app/providers';

@Component({
  selector: 'app-vendor-payments',
  templateUrl: './vendor-payments.component.html',
})
export class VendorPaymentsComponent implements OnInit {
  displayedColumns: string[] = [
    'paymentId',
    'paymentDate',
    'bankAccountNumber',
    'grossPayment',
    'fees',
    'netPayment',
  ];
  dataSource: MatTableDataSource<VendorPaymentSummary>;
  @ViewChild(MatPaginator, { static: true })
  paginator: MatPaginator;
  @ViewChild(MatSort, { static: true })
  sort: MatSort;
  public workInProgress = false;
  private vendorId: string;

  constructor(
    private vendorService: VendorService,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    this.workInProgress = true;
    this.vendorId = this.route.snapshot.params.id;
    this.vendorService.getVendorPayments(this.vendorId).subscribe(
      (res) => {
        this.dataSource = new MatTableDataSource(res);
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
      },
      (err) => console.error('HTTP Error', err),
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

  download() {
    this.vendorService
      .downloadVendorPaymentsCsv(this.vendorId)
      .subscribe((response) => {
        // It is necessary to create a new blob object with mime-type explicitly set
        // otherwise only Chrome works like it should
        const newBlob = new Blob([response], { type: 'text/csv' });

        // IE doesn't allow using a blob object directly as link href
        // instead it is necessary to use msSaveOrOpenBlob
        if (window.navigator && window.navigator.msSaveOrOpenBlob) {
          window.navigator.msSaveOrOpenBlob(newBlob);
          return;
        }

        // For other browsers:
        // Create a link pointing to the ObjectURL containing the blob.
        const downloadURL = URL.createObjectURL(newBlob);
        window.open(downloadURL);
      });
  }
}
