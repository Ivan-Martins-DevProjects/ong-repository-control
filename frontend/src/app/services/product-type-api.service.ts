import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpContext, HttpContextToken } from '@angular/common/http';
import type { PagedResult } from './inbound-api.service';
import { API_CONTEXT, API_VERSION, SKIP_AUTH, ApiVersion, BasicService } from './base-service';

export interface ProductType {
  id: number;
  name: string;
  category: string;
  itemCount: number;
  totalQuantity: number;
  createdAt: string;
}

export interface StockItem {
  id: number;
  productTypeId: number;
  name: string;
  description: string;
  category: string;
  quantity: number;
  unit: string;
  minQuantity: number;
  donor: string;
  entryDate: string;
  expiryDate: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface Movement {
  id: number;
  itemId: number;
  itemName: string;
  type: 'entry' | 'exit';
  quantity: number;
  date: string;
  description: string;
  source: 'process' | 'item';
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export abstract class ProductTypeApiService extends BasicService {
  private readonly context = "product_service"

  getAll() {
    return this.http.get<ProductType[]>('/get-all', {
      context: new HttpContext()
        .set(API_CONTEXT, this.context)
        .set(API_VERSION, ApiVersion.V1)
    });
  }

  getCategories() {
    return this.http.get<{ id: number; name: string; unit: string }[]>('/api/categories');
  }

  createCategory(name: string) {
    return this.http.post<{ id: number; name: string; unit: string }>('/api/categories', { name });
  }

  updateCategory(id: number, name: string) {
    return this.http.put<{ id: number; name: string; unit: string }>(`/api/categories/${id}`, { name });
  }

  deleteCategory(name: string) {
    return this.http.delete<void>(`/api/categories/${encodeURIComponent(name)}`);
  }

  getAllPaged(page = 1, pageSize = 10) {
    return this.http.get<PagedResult<ProductType>>(`/get-all-paged/page=${page}&page_size=${pageSize}`, {
      context: new HttpContext()
        .set(API_CONTEXT, this.context)
        .set(API_VERSION, ApiVersion.V1)
    });
  }

  create(data: { name: string; category: string }) {
    return this.http.post<ProductType>('/api/product-types', data);
  }

  update(id: number, data: { name: string; category: string }) {
    return this.http.put<ProductType>(`/api/product-types/${id}`, data);
  }

  delete(id: number) {
    return this.http.delete<void>(`/api/product-types/${id}`);
  }

  getItemsByType(typeId: number) {
    return this.http.get<StockItem[]>(`/api/product-types/${typeId}/items`);
  }
}

@Injectable({ providedIn: 'root' })
export class StockApiService {
  private readonly http = inject(HttpClient);
  private readonly context = "stock_service"

  getAll(page = 1, pageSize = 20) {
    return this.http.get<PagedResult<StockItem>>(`/list-all-items?page=${page}&pageSize=${pageSize}`, {
      context: new HttpContext()
        .set(API_CONTEXT, this.context)
        .set(API_VERSION, ApiVersion.V1)
        .set(SKIP_AUTH, false)
    });
  }

  getAllUnpaged() {
    return this.http.get<StockItem[]>('/api/stock/all');
  }

  search(q: string, productTypeId?: number | null) {
    const type = productTypeId ? `&productTypeId=${productTypeId}` : '';
    return this.http.get<StockItem[]>(`/api/stock/search?q=${encodeURIComponent(q)}${type}`);
  }

  getById(id: number) {
    return this.http.get<StockItem>(`/api/stock/${id}`);
  }

  registerEntry(itemId: number, data: { quantity: number; description: string }) {
    return this.http.post<StockItem>(`/api/stock/${itemId}/entry`, data);
  }

  registerExit(itemId: number, data: { quantity: number; description: string }) {
    return this.http.post<StockItem>(`/api/stock/${itemId}/exit`, data);
  }
}

@Injectable({ providedIn: 'root' })
export class MovementApiService {
  private readonly http = inject(HttpClient);

  getAll(page = 1, pageSize = 20, filters: Record<string, string> = {}) {
    let url = `/api/movements?page=${page}&pageSize=${pageSize}`;
    for (const [k, v] of Object.entries(filters)) {
      if (v) url += `&${k}=${encodeURIComponent(v)}`;
    }
    return this.http.get<PagedResult<Movement>>(url);
  }

  getGroupItems(id: number, page = 1, pageSize = 10) {
    return this.http.get<PagedResult<Movement>>(`/api/movements/${id}/items?page=${page}&pageSize=${pageSize}`);
  }
}

export interface DashboardData {
  totalTypes: number;
  totalUnits: number;
  expiringSoon: number;
  recentMovements: Movement[];
  monthlyData: { label: string; entries: number; exits: number }[];
  pieData: { entries: number; exits: number };
}

@Injectable({ providedIn: 'root' })
export class DashboardApiService {
  private readonly http = inject(HttpClient);

  get() {
    return this.http.get<DashboardData>('/api/dashboard');
  }
}
