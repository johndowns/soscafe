import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormControl, Validators, FormGroup } from '@angular/forms';
import { Location } from '@angular/common';
import { VendorService } from 'src/app/providers';
import { VendorDetail, UpdateVendorDetails } from 'src/app/model';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-vendor-detail',
  templateUrl: './vendor-detail.component.html',
})
export class VendorDetailComponent implements OnInit {
  public termsAndConditionsAccepted = false;
  public bankAccountNumber: FormControl;
  public workInProgress = false;
  private vendorId: string;

  BankAccountNumberRegExPattern = '[0-9]{2}[- ]?[0-9]{4}[- ]?[0-9]{7}[- ]?[0-9]{2,3}';

  public vendorForm = new FormGroup({
    id: new FormControl(''),
    businessName: new FormControl(''),
    registeredDate: new FormControl(''),
    contactName: new FormControl(''),
    emailAddress: new FormControl(''),
    phoneNumber: new FormControl(''),
    bankAccountNumber: new FormControl('', [Validators.required, Validators.pattern(this.BankAccountNumberRegExPattern)]),
    termsAccepted: new FormControl('', [Validators.required]),
  });

  constructor(
    private location: Location,
    private snackBar: MatSnackBar,
    private vendorService: VendorService,
    private route: ActivatedRoute
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
          termsAccepted: false,
        });
      },
      (err) => console.log('HTTP Error', err),
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
