import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

import { VendorSummary, VendorDetail, UpdateVendorDetails } from 'src/app/model';

@Injectable({
  providedIn: 'root'
})
export class VendorService {

  private vendorsBaseUrl = 'https://vendorapi.soscafe.nz';

  constructor(private http: HttpClient) { }

  getVendors(): Observable<VendorSummary[]> {
    return this.http.get<VendorSummary[]>(`${this.vendorsBaseUrl}/vendors`);
  }

  getVendor(vendorId: string): Observable<VendorDetail> {
    return this.http.get<VendorDetail>(`${this.vendorsBaseUrl}/vendors/${vendorId}`);
  }

  updateVendor(vendorId: string, updateVendorDetails: UpdateVendorDetails): Observable<VendorDetail> {
    return this.http.put<UpdateVendorDetails>(`${this.vendorsBaseUrl}/vendors/${vendorId}`, updateVendorDetails);
  }
}
