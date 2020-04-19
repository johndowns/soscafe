import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { VendorPaymentSummary } from 'src/app/model';
import { VendorService } from 'src/app/providers';
import { saveAs } from 'file-saver';

@Component({
  selector: 'app-admin-business-payments',
  templateUrl: './admin-business-payments.component.html',
})
export class AdminBusinessPaymentsComponent implements OnInit {
  displayedColumns: string[] = [
    'paymentId',
    'paymentDate',
    'bankAccountNumber',
    'paymentAmount',
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
    this.vendorService.getVendorPaymentsAdmin(this.vendorId).subscribe(
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
      .downloadVendorPaymentsCsvAdmin(this.vendorId)
      .subscribe((blob) => {
        saveAs(blob, 'payments.csv', {
          type: 'text/csv'
       });
      });
  }
}
