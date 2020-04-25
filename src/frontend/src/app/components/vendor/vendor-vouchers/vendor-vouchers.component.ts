import { VendorVouchersSummary, UpdateVoucherDetails } from 'src/app/model';
import { Component, OnInit, ViewChild } from '@angular/core';
import { VendorService } from 'src/app/providers';
import { ActivatedRoute } from '@angular/router';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { MatSnackBar } from '@angular/material/snack-bar';
import { saveAs } from 'file-saver';

@Component({
  selector: 'app-vendor-vouchers',
  templateUrl: './vendor-vouchers.component.html',
})
export class VendorVouchersComponent implements OnInit {
  displayedColumns: string[] = [
    'redeemVoucher',
    'orderRef',
    'orderDate',
    'customerName',
    'customerEmailAddress',
    'customerRegion',
    'voucherId',
    'voucherDescription',
    'voucherQuantity',
    'voucherGross',
    'voucherFees',
    'voucherNet',
    'customerAcceptsMarketing',
    'voucherIsDonation',
  ];
  dataSource: MatTableDataSource<VendorVouchersSummary>;
  @ViewChild(MatPaginator, { static: true })
  paginator: MatPaginator;
  @ViewChild(MatSort, { static: true })
  sort: MatSort;
  public workInProgress = false;
  private vendorId: string;
  private voucherId: string;
  private isRedeemed: boolean;
  private dateRedeemed: Date;

  constructor(
    private vendorService: VendorService,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
  ) {}

  ngOnInit() {
    this.workInProgress = true;
    this.vendorId = this.route.snapshot.params.id;
    this.vendorService.getVendorVouchers(this.vendorId).subscribe(
      (res) => {
        this.dataSource = new MatTableDataSource(res);
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;

        console.log(res);

        if (res.dateRedeemed === null){
          this.isRedeemed = false;
        }
        else {
          this.isRedeemed = true;
        }
      },
      (err) => console.error('HTTP Error', err),
      () => {
        this.workInProgress = false;
      }
    );
  }

  redeemVoucher(value, vendorVouchersSummary: VendorVouchersSummary) {
    console.log(value);
    this.workInProgress = true;

    const updateVoucherDetails: UpdateVoucherDetails = {
      ...vendorVouchersSummary,
      dateRedeemed: new Date().toISOString(),
    };

    this.voucherId = value;

    this.vendorService
      .updateVoucher(this.vendorId, this.voucherId, this.dateRedeemed, updateVoucherDetails)
      .subscribe(
        () => {
          this.onSubmitConfirmation(true);
        },
        (err) => {
          console.error('HTTP Error', err);
          this.onSubmitConfirmation(false);
        },
        () => {
          this.workInProgress = false;
        }
      );
  }

  undoRedeemVoucher(value, vendorVouchersSummary: VendorVouchersSummary) {
    this.workInProgress = true;
    const updateVoucherDetails: UpdateVoucherDetails = {
      ...vendorVouchersSummary,
      dateRedeemed: null,
    };

    this.vendorService
      .updateVoucher(this.vendorId, this.voucherId, this.dateRedeemed, updateVoucherDetails)
      .subscribe(
        () => {
          this.onSubmitConfirmation(true);
        },
        (err) => {
          console.error('HTTP Error', err);
          this.onSubmitConfirmation(false);
        },
        () => {
          this.workInProgress = false;
        }
      );
  }

  onSubmitConfirmation(isSucess: boolean) {
    window.scroll(0,0);
    const message = isSucess ? 'Voucher details have been updated.' : 'Failed to update.';
    this.snackBar.open(message, 'OK', {
      duration: 3000,
    });
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
      .downloadVendorVouchersCsv(this.vendorId)
      .subscribe((blob) => {
        saveAs(blob, 'vouchers.csv', {
          type: 'text/csv'
       });
      });
  }
}
