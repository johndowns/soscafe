import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class ErrorHandlerService {
  public errorMessage: string = '';

  constructor(private router: Router) { }

  public handleError = (err: HttpErrorResponse) => {
    if(err.status === 500){
      this.handle500Error(err);
    }
    else if(err.status === 404){
      this.handle404Error(err)
    }
    else{
      this.handleOtherError(err);
    }
  }

  private handle500Error = (err: HttpErrorResponse) => {
    this.createErrorMessage(err);
    this.router.navigate(['/error']);
  }

  private handle404Error = (err: HttpErrorResponse) => {
    this.createErrorMessage(err);
    this.router.navigate(['/error']);
  }

  private handleOtherError = (err: HttpErrorResponse) => {
    this.createErrorMessage(err);
    this.router.navigate(['/error']);
  }

  private createErrorMessage(err: HttpErrorResponse){
    this.errorMessage = err.error ? err.error : err.statusText;
  }
}
