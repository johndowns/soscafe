import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormControl, Validators, FormGroup } from '@angular/forms';
import { Location } from '@angular/common';
import { VendorService } from 'src/app/providers';
import { VendorDetail, UpdateVendorDetails } from 'src/app/model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ErrorHandlerService } from 'src/app/services/error-handler/error-handler.service';
import { MsalService, BroadcastService } from '@azure/msal-angular';

@Component({
  selector: 'app-admin-business-detail',
  templateUrl: './admin-business-detail.component.html',
})
export class AdminBusinessDetailComponent implements OnInit {
  public hasAgreedToTerms: boolean;
  public isClickAndCollect: boolean;
  public bankAccountNumber: FormControl;
  public workInProgress = false;
  private vendorId: string;
  loggedIn = false;
  userName = '';
  userEmail= '';
  isAdmin;

  BankAccountNumberRegExPattern = '[0-9]{2}[- ]?[0-9]{4}[- ]?[0-9]{7}[- ]?[0-9]{2,3}';

  public vendorForm = new FormGroup({
    id: new FormControl(''),
    businessName: new FormControl(''),
    registeredDate: new FormControl(''),
    contactName: new FormControl(''),
    emailAddress: new FormControl(''),
    phoneNumber: new FormControl(''),
    bankAccountNumber: new FormControl('', [Validators.required, Validators.pattern(this.BankAccountNumberRegExPattern)]),
    hasAgreedToTerms: new FormControl(''),
    isClickAndCollect: new FormControl(''),
    internalTag: new FormControl(''),
  });

  constructor(
    private location: Location,
    private snackBar: MatSnackBar,
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
    console.log(userAccount);
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

  ngOnInit(): void {
    this.checkAccount();

    this.broadcastService.subscribe('msal:loginSuccess', payload => {
      this.checkAccount();
    });

    this.workInProgress = true;
    this.vendorId = this.route.snapshot.params.id;
    this.vendorService.getVendorAdmin(this.vendorId).subscribe(
      (res) => {
        this.vendorForm.patchValue({
          id: res.id,
          businessName: res.businessName,
          registeredDate: new Date(res.registeredDate).toLocaleDateString('en-NZ'),
          contactName: res.contactName,
          emailAddress: res.emailAddress,
          phoneNumber: res.phoneNumber,
          bankAccountNumber: res.bankAccountNumber,
          hasAgreedToTerms: res.hasAgreedToTerms,
          isClickAndCollect: res.isClickAndCollect,
          InternalTag: new FormControl(''),
        });
        this.hasAgreedToTerms = res.hasAgreedToTerms;
        console.log(res);
        console.log(this.hasAgreedToTerms);
      },
      (err) => {
        console.log('LOG HTTP Error', err);
        this.errorService.handleError(err);
      },
      () => {
        this.workInProgress = false;
      }
    );
  }

  onCancelClick() {
    this.goBack();
  }

  goBack() {
    this.location.back();
  }

  onSubmit(vendorDetail: VendorDetail) {
    this.workInProgress = true;
    const updateVendorDetails: UpdateVendorDetails = {
      ...vendorDetail,
      dateAcceptedTerms: new Date().toISOString(),
    };

    this.vendorService
      .updateVendorAdmin(this.vendorId, updateVendorDetails)
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
    const message = isSucess ? 'Your details have been updated.' : 'Failed to update.';
    this.snackBar.open(message, 'OK', {
      duration: 3000,
    });
  }
}
