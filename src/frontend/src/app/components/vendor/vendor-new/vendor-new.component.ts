import { Component, OnInit, Inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormControl, Validators, FormGroup } from '@angular/forms';
import { HttpClient } from '@angular/common/http'
import { Location } from '@angular/common';
import { VendorService } from 'src/app/providers';
import { VendorDetail, UpdateVendorDetails } from 'src/app/model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { ErrorHandlerService } from 'src/app/services/error-handler/error-handler.service';

@Component({
  selector: 'app-vendor-new',
  templateUrl: './vendor-new.component.html'
})
export class VendorNewComponent implements OnInit {
  public termsAndConditionsAccepted = false;
  public bankAccountNumber: FormControl;
  public newVendorForm: FormGroup;
  public workInProgress = false;
  private vendorId: string;

  BankAccountNumberRegExPattern = '[0-9]{2}[- ]?[0-9]{4}[- ]?[0-9]{7}[- ]?[0-9]{2,3}';

  constructor(
    // private dialog: MatDialog,
    private location: Location,
    private snackBar: MatSnackBar,
    private vendorService: VendorService,
    private route: ActivatedRoute,
    private router: Router,
    private errorService: ErrorHandlerService,
    private formBuilder: FormBuilder,
    private http: HttpClient,
  ) {
    this.newVendorForm = this.formBuilder.group({
      businessName: new FormControl('', [Validators.required]),
      type: new FormControl('', [Validators.required]),
      phoneNumber: new FormControl('', [Validators.required]),
      description: new FormControl('', [Validators.required]),
      city: new FormControl('', [Validators.required]),
      businessPhotoUrl: new FormControl('', [Validators.required]),
      bankAccountNumber: new FormControl('', [Validators.required, Validators.pattern(this.BankAccountNumberRegExPattern)]),
      hasAcceptedTerms: new FormControl('', [Validators.required]),
    })
  }

  ngOnInit() {
    this.workInProgress = false;
  }

  onCancelClick() {
    this.goBack();
  }

  goBack() {
    this.location.back();
  }

  onSubmit() {
    this.workInProgress = true;

    var formData: any = new FormData();

    formData.append('businessName', this.newVendorForm.get('businessName').value);
    formData.append('type', this.newVendorForm.get('type').value);
    formData.append('phoneNumber', this.newVendorForm.get('phoneNumber').value);
    formData.append('description', this.newVendorForm.get('description').value);
    formData.append('city', this.newVendorForm.get('city').value);
    formData.append('businessPhotoUrl', this.newVendorForm.get('businessPhotoUrl').value);
    formData.append('bankAccountNumber', this.newVendorForm.get('bankAccountNumber').value);
    formData.append('hasAcceptedTerms', this.newVendorForm.get('hasAcceptedTerms').value);

    console.log(formData);

    this.http.post('https://soscafevendor-test.azurewebsites.net/vendors', formData).subscribe(
        () => {
          this.onSubmitConfirmation(true);
        },
        (err) => {
          console.error('HTTP Error', err);
          this.onSubmitConfirmation(false);
          this.errorService.handleError(err);
        },
        () => {
          this.workInProgress = false;
        }
      );
  }

  onSubmitConfirmation(isSucess: boolean) {
    window.scroll(0,0);

    if (isSucess === true){
      // this.dialog.open(VenderNewSuccessDialog, {
      //   width: '250px'
      // });
    }
    else {
      this.snackBar.open('Something went wrong.', 'OK', {
        duration: 5000,
      });
    }
  }
}

@Component({
  selector: 'vendor-new-success-dialog',
  templateUrl: 'vendor-new-success.component.html',
})
export class VenderNewSuccessDialog {

  constructor(
    public dialogRef: MatDialogRef<VenderNewSuccessDialog>,
    private router: Router,
  ){}

  onClick(): void {
    this.dialogRef.close();
    this.router.navigate(['/']);
  }

}
