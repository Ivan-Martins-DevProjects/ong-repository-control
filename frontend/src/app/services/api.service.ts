import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

export enum ApiEndpoint {
  AuthLogin = '/api/auth/login',
  AuthLogout = '/api/auth/logout',
  Inbound = '/api/inbound',
  InboundAll = '/api/inbound/all',
  Stock = '/api/stock',
  StockAll = '/api/stock/all',
  Movements = '/api/movements',
  ProductTypes = '/api/product-types',
}

function path(ep: string, params?: Record<string, string | number>): string {
  if (!params) return ep;
  let p = ep;
  for (const [k, v] of Object.entries(params)) p = p.replace(`{${k}}`, String(v));
  return p;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);

  get<T>(endpoint: ApiEndpoint | string, urlParams?: Record<string, string | number>, queryParams?: Record<string, string | number>) {
    let p = path(endpoint, urlParams);
    let hp = new HttpParams();
    if (queryParams) for (const [k, v] of Object.entries(queryParams)) hp = hp.set(k, String(v));
    return this.http.get<T>(p, { params: hp, withCredentials: true });
  }

  post<T>(endpoint: ApiEndpoint | string, body?: unknown, urlParams?: Record<string, string | number>) {
    return this.http.post<T>(path(endpoint, urlParams), body ?? {}, { withCredentials: true });
  }

  patch<T>(endpoint: ApiEndpoint | string, body?: unknown, urlParams?: Record<string, string | number>) {
    return this.http.patch<T>(path(endpoint, urlParams), body ?? {}, { withCredentials: true });
  }

  delete<T>(endpoint: ApiEndpoint | string, urlParams?: Record<string, string | number>) {
    return this.http.delete<T>(path(endpoint, urlParams), { withCredentials: true });
  }
}
