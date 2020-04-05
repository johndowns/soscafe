import { VendorSummary, VendorDetail } from 'src/app/model';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class VendorService {

  private vendorsBaseUrl = '/api/vendors';

  constructor(private http: HttpClient) { }

  getVendors(): Observable<VendorSummary[]> {
    return this.http.get<VendorSummary[]>(this.vendorsBaseUrl);
  }

  getVendor(vendorId: string): Observable<VendorDetail> {
    return this.http.get<VendorDetail>(`${this.vendorsBaseUrl}/${vendorId}`);
  }
}
