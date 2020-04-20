import { Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { FormControl, Validators, FormGroup } from '@angular/forms';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { VendorService } from 'src/app/providers';
import { VendorSummary } from 'src/app/model';
import { ErrorHandlerService } from 'src/app/services/error-handler/error-handler.service';
import { saveAs } from 'file-saver';
import { MsalService, BroadcastService } from '@azure/msal-angular';
import { Router, ActivatedRoute } from '@angular/router';

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
  public searchType: string;
  public notFound = false;
  public hideTable = true;
  loggedIn = false;
  userName = '';
  userEmail= '';
  isAdmin;

  public businessSearchForm = new FormGroup({
    search: new FormControl(''),
    searchType: new FormControl('')
  });

  constructor(
    private vendorService: VendorService,
    private errorService: ErrorHandlerService,
    private broadcastService: BroadcastService,
    private authService: MsalService,
    private route: ActivatedRoute,
    private router: Router,
  ) {}

  checkAccount() {
    const userAccount = this.authService.getAccount();
    this.loggedIn = !!userAccount;
    if (this.loggedIn) {
      if (userAccount.idToken.extension_IsAdmin === null) {
        this.isAdmin = false;
        this.router.navigate(['/error?error=404%20Not%20Found&si=true']);
      }
      else {
        this.isAdmin = userAccount.idToken.extension_IsAdmin;
        if (this.isAdmin) {
          //DO NOTHING
        }
        else {
          this.router.navigate(['/error?error=404%20Not%20Found&si=true']);
        }
      }
    }
    else {
      this.router.navigate(['/error?error=404%20Not%20Found&si=true']);
    }
  }

  ngOnInit() {
    this.checkAccount();

    this.broadcastService.subscribe('msal:loginSuccess', payload => {
      this.checkAccount();
    });

    this.workInProgress = false;
  }

  onSubmit(){
    this.workInProgress = true;
    this.hideTable = false;
    this.searchTerm = this.businessSearchForm.value.search;
    this.searchType = this.businessSearchForm.value.searchType;
    this.vendorService.searchVendorAdmin(this.searchTerm, this.searchType).subscribe(
      (res) => {
        if (res.length === 0) {
          this.notFound = true;
          this.dataSource = null;
        }
        else {
          this.dataSource = new MatTableDataSource(res);
          this.notFound = false;
        }
      },
      (err) => {
        if (err.status === 404) {
          this.notFound = true;
          this.workInProgress = false;
        }
        else {
          console.error('HTTP Error', err);
          this.errorService.handleError(err);
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

  download() {
    this.vendorService
      .getVendorListAdmin()
      .subscribe((blob) => {
        saveAs(blob, 'businessesListAdmin.csv', {
          type: 'text/csv'
       });
      });
  }
}
