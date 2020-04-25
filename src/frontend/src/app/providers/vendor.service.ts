import { environment as env } from './../../environments/environment';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import {
  VendorSummary,
  VendorDetail,
  UpdateVendorDetails,
  VendorPaymentSummary,
  VendorVouchersSummary,
} from 'src/app/model';

@Injectable({
  providedIn: 'root',
})
export class VendorService {

  private vendorsBaseUrl = env.apiBaseUrl;
  private httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + localStorage.getItem('access_token')
    })
  }

  constructor(private http: HttpClient) {
  }

  getVendors(): Observable<VendorSummary[]> {
    return this.http.get<VendorSummary[]>(`${this.vendorsBaseUrl}/vendors`, this.httpOptions);
  }

  getVendor(vendorId: string): Observable<VendorDetail> {
    return this.http.get<VendorDetail>(`${this.vendorsBaseUrl}/vendors/${vendorId}`, this.httpOptions);
  }

  getVendorPayments(vendorId: string): Observable<VendorPaymentSummary[]> {
    return this.http.get<VendorPaymentSummary[]>(
      `${this.vendorsBaseUrl}/vendors/${vendorId}/payments`,
      this.httpOptions
    );
  }

  downloadVendorPaymentsCsv(
    vendorId: string
  ): any {
    return this.http.get(
      `${this.vendorsBaseUrl}/vendors/${vendorId}/payments/csv`,
      {
        headers: new HttpHeaders({
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + localStorage.getItem('access_token')
        }),
        responseType: 'blob'
      }
    );
  }

  getVendorVouchers(vendorId: string): Observable<VendorVouchersSummary[]> {
    return this.http.get<VendorVouchersSummary[]>(
      `${this.vendorsBaseUrl}/vendors/${vendorId}/vouchers`,
      this.httpOptions
    );
  }

  downloadVendorVouchersCsv(vendorId: string): any {
    return this.http.get(
      `${this.vendorsBaseUrl}/vendors/${vendorId}/vouchers/csv`,
      {
        headers: new HttpHeaders({
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + localStorage.getItem('access_token')
        }),
        responseType: 'blob'
      }
    );
  }

  updateVendor(
    vendorId: string,
    updateVendorDetails: UpdateVendorDetails
  ): Observable<VendorDetail> {
    return this.http.put<UpdateVendorDetails>(
      `${this.vendorsBaseUrl}/vendors/${vendorId}`,
      updateVendorDetails,
      this.httpOptions
    );
  }

  //ADMIN VIEW FUCNTIONS

  searchVendorAdmin(searchTerm: string, searchType: string): Observable<VendorSummary[]> {
    return this.http.get<VendorSummary[]>(`${this.vendorsBaseUrl}/internal/vendors?${searchType}=${searchTerm}`, this.httpOptions);
  }

  getVendorAdmin(vendorId: string): Observable<VendorDetail> {
    return this.http.get<VendorDetail>(
      `${this.vendorsBaseUrl}/internal/vendors/${vendorId}`,
      this.httpOptions
    );
  }

  getVendorPaymentsAdmin(vendorId: string): Observable<VendorPaymentSummary[]> {
    return this.http.get<VendorPaymentSummary[]>(
      `${this.vendorsBaseUrl}/internal/vendors/${vendorId}/payments`,
      this.httpOptions
    );
  }

  downloadVendorPaymentsCsvAdmin(
    vendorId: string
  ): any {
    return this.http.get(
      `${this.vendorsBaseUrl}/vendors/${vendorId}/payments/csv`,
      {
        headers: new HttpHeaders({
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + localStorage.getItem('access_token')
        }),
        responseType: 'blob'
      }
    );
  }

  getVendorVouchersAdmin(vendorId: string): Observable<VendorVouchersSummary[]> {
    return this.http.get<VendorVouchersSummary[]>(
      `${this.vendorsBaseUrl}/internal/vendors/${vendorId}/vouchers`,
      this.httpOptions
    );
  }

  downloadVendorVouchersCsvAdmin(vendorId: string): any {
    return this.http.get(
      `${this.vendorsBaseUrl}/vendors/${vendorId}/vouchers/csv`,
      {
          headers: new HttpHeaders({
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + localStorage.getItem('access_token')
        }),
        responseType: 'blob'
      }
    );
  }

  updateVendorAdmin(
    vendorId: string,
    updateVendorDetails: UpdateVendorDetails
  ): Observable<VendorDetail> {
    return this.http.put<UpdateVendorDetails>(
      `${this.vendorsBaseUrl}/internal/vendors/${vendorId}`,
      updateVendorDetails,
      this.httpOptions
    );
  }

  getVendorListAdmin() {
    return this.http.get(
      `${this.vendorsBaseUrl}/internal/vendors/csv`,
      {
        headers: new HttpHeaders({
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + localStorage.getItem('access_token')
        }),
        responseType: 'blob'
      }
    );
  }

}
