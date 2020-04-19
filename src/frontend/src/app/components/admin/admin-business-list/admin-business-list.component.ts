import { Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { FormControl, Validators, FormGroup } from '@angular/forms';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { VendorService } from 'src/app/providers';
import { VendorSummary } from 'src/app/model';

@Component({
  selector: 'app-admin-business-list',
  templateUrl: './admin-business-list.component.html',
})
export class AdminBusinessListComponent implements OnInit {
  displayedColumns: string[] = ['businessName'];
  dataSource: MatTableDataSource<VendorSummary>;
  @ViewChild(MatPaginator, { static: true })
  paginator: MatPaginator;
  @ViewChild(MatSort, { static: true })
  sort: MatSort;
  public workInProgress = false;
  public searchTerm: string;
  public notFound = false;
  public hideTable = true;

  public businessSearchForm = new FormGroup({
    search: new FormControl(''),
  });

  constructor(private vendorService: VendorService) {}
  ngOnInit() {
    this.workInProgress = false;
  }

  onSubmit(){
    this.workInProgress = true;
    this.hideTable = false;
    this.searchTerm = this.businessSearchForm.value.search;
    console.log(this.searchTerm);
    this.vendorService.searchVendorAdmin(this.searchTerm).subscribe(
      (res) => {
        this.dataSource = new MatTableDataSource(res);
      },
      (err) => {
        if (err.status === 404) {
          this.notFound = true;
          this.workInProgress = false;
        }
        else {
          console.error('HTTP Error', err);
          this.errorService.handleError(err);
          this.onSubmitConfirmation(false);
        }
      },
      () => {
        this.workInProgress = false;
      }
    );
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }
}
