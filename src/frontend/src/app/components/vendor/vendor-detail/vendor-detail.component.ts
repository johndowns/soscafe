import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormControl, Validators, FormGroup } from '@angular/forms';
import { Location } from '@angular/common';
import { VendorService } from 'src/app/providers';
import { VendorDetail, UpdateVendorDetails } from 'src/app/model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ErrorHandlerService } from 'src/app/services/error-handler/error-handler.service';

@Component({
  selector: 'app-vendor-detail',
  templateUrl: './vendor-detail.component.html',
})
export class VendorDetailComponent implements OnInit {
  public hasAgreedToTerms: boolean;
  public isClickAndCollect: boolean;
  public bankAccountNumber: FormControl;
  public workInProgress = false;
  private vendorId: string;

  level3Closed = false;
  level2Closed = false;
  level1Closed = false;

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
    clickAndCollectURL: new FormControl(''),
    level1Closed: new FormControl(''),
    level2Closed: new FormControl(''),
    level3Closed: new FormControl(''),
    level1Status: new FormControl(''),
    level2Status: new FormControl(''),
    level3Status: new FormControl(''),
    level1Delivery: new FormControl(''),
    level2Delivery: new FormControl(''),
    level3Delivery: new FormControl(''),
    level1ClickAndCollect: new FormControl(''),
    level2ClickAndCollect: new FormControl(''),
    level3ClickAndCollect: new FormControl(''),
    level1Open: new FormControl(''),
    level2Open: new FormControl(''),
    level3Open: new FormControl(''),
  });

  constructor(
    private location: Location,
    private snackBar: MatSnackBar,
    private vendorService: VendorService,
    private route: ActivatedRoute,
    private errorService: ErrorHandlerService
  ) {}

  ngOnInit(): void {
    this.workInProgress = true;

    this.vendorId = this.route.snapshot.params.id;
    this.vendorService.getVendor(this.vendorId).subscribe(
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
        });
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
      .updateVendor(this.vendorId, updateVendorDetails)
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
