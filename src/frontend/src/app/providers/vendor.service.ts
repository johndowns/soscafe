import { environment as env } from './../../environments/environment';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

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

  constructor(private http: HttpClient) {}

  getVendors(): Observable<VendorSummary[]> {
    return this.http.get<VendorSummary[]>(`${this.vendorsBaseUrl}/vendors`);
  }

  getVendor(vendorId: string): Observable<VendorDetail> {
    return this.http.get<VendorDetail>(
      `${this.vendorsBaseUrl}/vendors/${vendorId}`
    );
  }

  getVendorPayments(vendorId: string): Observable<VendorPaymentSummary[]> {
    return this.http.get<VendorPaymentSummary[]>(
      `${this.vendorsBaseUrl}/vendors/${vendorId}/payments`
    );
  }

  downloadVendorPaymentsCsv(
    vendorId: string
  ): any {
    return this.http.get(
      `${this.vendorsBaseUrl}/vendors/${vendorId}/payments/csv`,
      { responseType: 'blob' }
    );
  }

  getVendorVouchers(vendorId: string): Observable<VendorVouchersSummary[]> {
    return this.http.get<VendorVouchersSummary[]>(
      `${this.vendorsBaseUrl}/vendors/${vendorId}/vouchers`
    );
  }

  downloadVendorVouchersCsv(vendorId: string): any {
    return this.http.get(
      `${this.vendorsBaseUrl}/vendors/${vendorId}/vouchers/csv`,
      { responseType: 'blob' }
    );
  }

  updateVendor(
    vendorId: string,
    updateVendorDetails: UpdateVendorDetails
  ): Observable<VendorDetail> {
    return this.http.put<UpdateVendorDetails>(
      `${this.vendorsBaseUrl}/vendors/${vendorId}`,
      updateVendorDetails
    );
  }

  searchVendorAdmin(searchTerm: string, searchType: string): Observable<VendorSummary[]> {
    return this.http.get<VendorSummary[]>(`${this.vendorsBaseUrl}/internal/vendors?${searchType}=${searchTerm}`);
  }

  getVendorAdmin(vendorId: string): Observable<VendorDetail> {
    return this.http.get<VendorDetail>(
      `${this.vendorsBaseUrl}/internal/vendors/${vendorId}`
    );
  }

  updateVendorAdmin(
    vendorId: string,
    updateVendorDetails: UpdateVendorDetails
  ): Observable<VendorDetail> {
    return this.http.put<UpdateVendorDetails>(
      `${this.vendorsBaseUrl}/internal/vendors/${vendorId}`,
      updateVendorDetails
    );
  }

  getVendorListAdmin() {
    return this.http.get(
      `${this.vendorsBaseUrl}/internal/vendors/csv`,
      { responseType: 'blob' }
    );
  }

}
