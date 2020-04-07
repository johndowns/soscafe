import { Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { VendorService } from 'src/app/providers';
import { VendorSummary } from 'src/app/model';

@Component({
  selector: 'app-vendor-list',
  templateUrl: './vendor-list.component.html',
})
export class VendorListComponent implements OnInit {
  displayedColumns: string[] = ['businessName'];
  dataSource: MatTableDataSource<VendorSummary>;
  @ViewChild(MatPaginator, { static: true })
  paginator: MatPaginator;
  @ViewChild(MatSort, { static: true })
  sort: MatSort;

  constructor(private vendorService: VendorService) {}
  ngOnInit() {
    this.vendorService.getVendors().subscribe(
      (res) => {
        this.dataSource = new MatTableDataSource(res);
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
      },
      (err) => console.log('HTTP Error', err),
      () => console.log('HTTP request completed.')
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
