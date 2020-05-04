import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormControl, Validators, FormGroup } from '@angular/forms';
import { Location } from '@angular/common';
// import { DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE } from '@angular/material/core';
import { VendorService } from 'src/app/providers';
import { VendorDetail, UpdateVendorDetails } from 'src/app/model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ErrorHandlerService } from 'src/app/services/error-handler/error-handler.service';
import { ConstantService } from 'src/app/services/constant.service';
import * as _ from 'lodash';

@Component({
  selector: 'app-admin-business-detail',
  templateUrl: './admin-business-detail.component.html',
})
export class AdminBusinessDetailComponent implements OnInit {
  public hasAgreedToTerms: boolean;
  public level1Closed: boolean;
  public level2Closed: boolean;
  public level3Closed: boolean;
  public isClickAndCollect: boolean;
  public isHidden: boolean;
  public bankAccountNumber: FormControl;
  public workInProgress = false;
  private vendorId: string;
  private registeredDate: string;

  loggedIn = false;
  userName = '';
  userEmail = '';
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
    clickAndCollectUrl: new FormControl(''),
    level1Closed: new FormControl(''),
    level2Closed: new FormControl(''),
    level3Closed: new FormControl(''),
    level1Delivery: new FormControl(''),
    level2Delivery: new FormControl(''),
    level3Delivery: new FormControl(''),
    level1ClickAndCollect: new FormControl(''),
    level2ClickAndCollect: new FormControl(''),
    level3ClickAndCollect: new FormControl(''),
    level1Open: new FormControl(''),
    level2Open: new FormControl(''),
    level3Open: new FormControl(''),
    isHidden: new FormControl(''),
  });

  constructor(
    private location: Location,
    private snackBar: MatSnackBar,
    private vendorService: VendorService,
    private errorService: ErrorHandlerService,
    private route: ActivatedRoute,
    private router: Router,
    private constantService: ConstantService,
  ) {}

  checkAccount() {
    if (!_.get(this.constantService, 'isAdmin', false)){
      this.router.navigate(['/error?error=404%20Not%20Found&si=true']);
    }
  }

  ngOnInit(): void {
    this.checkAccount();

    this.workInProgress = true;
    this.vendorId = this.route.snapshot.params.id;
    this.vendorService.getVendorAdmin(this.vendorId).subscribe(
      (res) => {
        console.log(res);
        this.registeredDate = res.registeredDate;
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
          internalTag: res.internalTag,
          clickAndCollectUrl: res.clickAndCollectUrl,
          level1Closed: res.level1Closed,
          level2Closed: res.level2Closed,
          level3Closed: res.level3Closed,
          level1Delivery: res.level1Delivery,
          level2Delivery: res.level2Delivery,
          level3Delivery: res.level3Delivery,
          level1ClickAndCollect: res.level1ClickAndCollect,
          level2ClickAndCollect: res.level2ClickAndCollect,
          level3ClickAndCollect: res.level3ClickAndCollect,
          level1Open: res.level1Open,
          level2Open: res.level2Open,
          level3Open: res.level3Open,
          isHidden: res.isHidden,
        });
        this.isHidden = res.isHidden;
        this.level1Closed = res.level1Closed;
        this.level2Closed = res.level2Closed;
        this.level3Closed = res.level3Closed;
        this.hasAgreedToTerms = res.hasAgreedToTerms;
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

  level3StatusChange(e) {
    this.level3Closed = e.checked;
  }

  level2StatusChange(e) {
    this.level2Closed = e.checked;
  }

  level1StatusChange(e) {
    this.level1Closed = e.checked;
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
      registeredDate: this.registeredDate,
      dateAcceptedTerms: new Date().toISOString(),
    };

    console.log(updateVendorDetails);
    this.vendorService
      .updateVendorAdmin(this.vendorId, updateVendorDetails)
      .subscribe(
        () => {
          document.getElementsByTagName('mat-sidenav-content')[0].scrollTop = 0;
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
    window.scroll(0, 0);
    const message = isSucess ? 'Your details have been updated.' : 'Failed to update.';
    this.snackBar.open(message, 'OK', {
      duration: 3000,
    });
  }

  hideBusiness() {
    this.vendorForm.patchValue({
      isHidden: true,
    });
    this.isHidden = true;
    this.onSubmit(this.vendorForm.value);
  }

  undoHideBusiness() {
    this.vendorForm.patchValue({
      isHidden: false,
    });
    this.isHidden = false;
    this.onSubmit(this.vendorForm.value);
  }
}
