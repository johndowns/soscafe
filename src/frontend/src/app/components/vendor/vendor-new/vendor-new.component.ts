import { environment as env } from 'src/environments/environment';
import { Component, OnInit, Inject, Injectable } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormControl, Validators, FormGroup } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http'
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
  public hasAgreedToTerms = false;
  public isClickAndCollect = false;
  public bankAccountNumber: FormControl;
  public newVendorForm: FormGroup;
  public workInProgress = false;
  private vendorId: string;
  private httpOptions;

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
    // private vendorsBaseUrl = env.apiBaseUrl,
  ) {
    this.newVendorForm = this.formBuilder.group({
      businessName: new FormControl('', [Validators.required]),
      type: new FormControl('', [Validators.required]),
      phoneNumber: new FormControl('', [Validators.required]),
      description: new FormControl('', [Validators.required]),
      city: new FormControl('', [Validators.required]),
      businessPhotoUrl: new FormControl(''),
      bankAccountNumber: new FormControl('', [Validators.required, Validators.pattern(this.BankAccountNumberRegExPattern)]),
      hasAgreedToTerms: new FormControl('', [Validators.required]),
      isClickAndCollect: new FormControl(false),
    });
    this.httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        'Authorization': 'Bearer '+localStorage.getItem('access_token')
      })
    }
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

    this.http.post(`${env.apiBaseUrl}vendors`, this.newVendorForm.value, this.httpOptions).subscribe(
        () => {
          this.workInProgress = false;
        },
        (err) => {
          console.error('HTTP Error', err);
          this.errorService.handleError(err);
          this.onSubmitConfirmation(false);
        },
        () => {
          this.onSubmitConfirmation(true);
        }
      );
  }

  onSubmitConfirmation(isSucess: boolean) {
    window.scroll(0,0);

    if (isSucess === true){
      this.router.navigate(['/new-vendor/success'], { queryParams: { businessName: this.newVendorForm.value.businessName, si:true } });
    }
    else {
      this.snackBar.open('Something went wrong.', 'OK', {
        duration: 5000,
      });
    }
  }
}
