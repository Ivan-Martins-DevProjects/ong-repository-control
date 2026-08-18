import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface InboundProcess {
  id: number;
  name: string;
  description: string;
  startDate: string;
  endDate: string;
  status: 'active' | 'paused' | 'completed' | 'cancelled';
  type: 'entry' | 'exit';
  createdAt: string;
}

export interface InboundItem {
  id: number;
  processId: number;
  productTypeId: number | null;
  itemId: number | null;
  name: string;
  quantity: number;
  unit: string;
  expiryDate: string | null;
  createdAt: string;
}

export interface PagedResult<T> {
  data: T[];
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;
}

export interface CreateProcessPayload {
  name: string;
  description: string;
  startDate: string;
  endDate: string;
  type: 'entry' | 'exit';
}

export interface AddItemPayload {
  productTypeId: number | null;
  itemId: number | null;
  name: string;
  quantity: number;
  unit: string;
  expiryDate: string | null;
}

@Injectable({ providedIn: 'root' })
export class InboundApiService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/inbound';

  getAll(page = 1, pageSize = 20) {
    return this.http.get<PagedResult<InboundProcess>>(`${this.base}?page=${page}&pageSize=${pageSize}`);
  }

  getAllUnpaged() {
    return this.http.get<InboundProcess[]>(`${this.base}/all`);
  }

  getById(id: number) {
    return this.http.get<InboundProcess>(`${this.base}/${id}`);
  }

  create(data: CreateProcessPayload) {
    return this.http.post<InboundProcess>(this.base, data);
  }

  pause(id: number) {
    return this.http.patch<InboundProcess>(`${this.base}/${id}/pause`, {});
  }

  resume(id: number) {
    return this.http.patch<InboundProcess>(`${this.base}/${id}/resume`, {});
  }

  finish(id: number) {
    return this.http.patch<InboundProcess>(`${this.base}/${id}/finish`, {});
  }

  cancel(id: number) {
    return this.http.patch<InboundProcess>(`${this.base}/${id}/cancel`, {});
  }

  getItems(processId: number) {
    return this.http.get<InboundItem[]>(`${this.base}/${processId}/items`);
  }

  getItemsPaged(processId: number, page = 1, pageSize = 50) {
    return this.http.get<PagedResult<InboundItem>>(`${this.base}/${processId}/items?page=${page}&pageSize=${pageSize}`);
  }

  addItem(processId: number, data: AddItemPayload) {
    return this.http.post<InboundItem>(`${this.base}/${processId}/items`, data);
  }

  deleteItem(processId: number, itemId: number) {
    return this.http.delete(`${this.base}/${processId}/items/${itemId}`);
  }
}
