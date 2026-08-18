import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { forkJoin } from 'rxjs';
import { ItemFilter, MOVEMENT_FILTER_FIELDS, PROCESS_FILTER_FIELDS, type FilterValues } from '../../components/item-filter/item-filter';
import { InboundApiService, type InboundProcess, type InboundItem } from '../../services/inbound-api.service';
import { ProductTypeApiService, StockApiService, MovementApiService, type Movement } from '../../services/product-type-api.service';

@Component({
  selector: 'app-history-page',
  imports: [FormsModule, DatePipe, ItemFilter],
  templateUrl: './history-page.html',
  styleUrl: './history-page.css'
})
export class HistoryPage {
  private readonly inboundApi = inject(InboundApiService);
  private readonly productTypeApi = inject(ProductTypeApiService);
  private readonly stockApi = inject(StockApiService);
  private readonly movementApi = inject(MovementApiService);

  protected readonly loading = signal(false);
  protected readonly processes = signal<InboundProcess[]>([]);
  protected readonly processPage = signal(1);
  protected readonly processTotalPages = signal(1);
  protected readonly inboundItems = signal<InboundItem[]>([]);
  protected readonly itemPage = signal(1);
  protected readonly itemTotalPages = signal(1);
  protected readonly productTypes = signal<{ id: number; name: string; category: string }[]>([]);
  protected readonly stockItems = signal<any[]>([]);
  protected readonly movements = signal<any[]>([]);
  protected readonly movementPage = signal(1);
  protected readonly movementTotalPages = signal(1);
  protected readonly movementDetail = signal<Movement | null>(null);
  protected readonly movementDetailItems = signal<Movement[]>([]);
  protected readonly movementDetailPage = signal(1);
  protected readonly movementDetailTotalPages = signal(1);
  protected readonly movementDetailLoading = signal(false);

  protected readonly activeProcess = signal<InboundProcess | null>(null);
  protected readonly processDetailView = signal<InboundProcess | null>(null);
  protected readonly currentSection = signal<'movements' | 'processes'>('movements');

  private nextItemId = 1;
  protected countName = ''; countExpiry = ''; countTypeId: number | null = null; countUnit = 'unidades';
  protected countSearchResults = signal<{ id: number; name: string; quantity: number; unit: string }[]>([]);

  protected exitTypeId: number | null = null; exitSearch = '';
  protected exitTempItems = signal<any[]>([]);

  protected movementSearch = '';
  protected readonly movementSuggestions = signal<any[]>([]);
  protected readonly movementSelected = signal<any | null>(null);
  protected movementQty = 1;
  protected movementDesc = '';
  protected readonly movementBusy = signal(false);
  protected readonly movementError = signal('');
  protected readonly showMovementModal = signal(false);
  protected readonly movementType = signal<'entry' | 'exit'>('entry');
  protected readonly showCreateModal = signal(false);
  protected readonly createStep = signal<'type' | 'form'>('type');
  protected readonly showCountModal = signal(false);
  protected formName = ''; formDesc = ''; formStart = ''; formEnd = '';
  protected formType: 'entry' | 'exit' = 'entry';

  protected readonly showDetailed = signal(true);
  protected readonly processItems = computed(() => {
    const p = this.processDetailView();
    return p ? this.inboundItems().filter(i => i.processId === p.id) : [];
  });

  protected readonly summaryGroups = computed(() => {
    const items = this.processItems();
    const map = new Map<string, number>();
    for (const i of items) {
      const key = i.name;
      map.set(key, (map.get(key) ?? 0) + 1);
    }
    return Array.from(map.entries()).map(([name, count]) => ({ name, count }));
  });

  protected toggleDetailView(): void { this.showDetailed.update(v => !v); }

  protected readonly filterFields = PROCESS_FILTER_FIELDS;
  protected readonly activeProcessFilter = signal<FilterValues>({});

  protected readonly movementFilterFields = MOVEMENT_FILTER_FIELDS;
  protected readonly activeMovementFilter = signal<FilterValues>({});

  protected readonly filteredProcesses = computed(() => {
    let list = this.processes().filter(p => p.status === 'active' || p.status === 'paused');
    const f = this.activeProcessFilter();
    if (f['type'] === 'entry' || f['type'] === 'exit') list = list.filter(p => p.type === f['type']);
    if (f['status'] === 'active' || f['status'] === 'paused') list = list.filter(p => p.status === f['status']);
    if (f['name']) {
      const q = f['name'].toLowerCase().trim();
      if (q) list = list.filter(p => p.name.toLowerCase().includes(q) || p.description.toLowerCase().includes(q));
    }
    if (f['startDateStart']) list = list.filter(p => p.startDate >= f['startDateStart']);
    if (f['startDateEnd']) list = list.filter(p => p.startDate <= f['startDateEnd']);
    if (f['endDateStart']) list = list.filter(p => p.endDate >= f['endDateStart']);
    if (f['endDateEnd']) list = list.filter(p => p.endDate <= f['endDateEnd']);
    return list;
  });

  protected onApplyProcessFilter(v: FilterValues): void { this.activeProcessFilter.set(v); }
  protected onClearProcessFilter(): void { this.activeProcessFilter.set({}); }

  protected onApplyMovementFilter(v: FilterValues): void {
    this.activeMovementFilter.set(v);
    this.movementPage.set(1);
    this.loadMovements();
  }
  protected onClearMovementFilter(): void {
    this.activeMovementFilter.set({});
    this.movementPage.set(1);
    this.loadMovements();
  }

  protected readonly exitSuggestions = signal<any[]>([]);
  protected exitTempPage = signal(1);
  protected readonly exitTempPages = computed(() => Math.max(1, Math.ceil(this.exitTempItems().length / 5)));
  protected readonly exitPaginatedTemp = computed(() => {
    const p = this.exitTempPage();
    return this.exitTempItems().slice((p - 1) * 5, p * 5);
  });

  protected onExitTypeChange(): void {
    this.exitSearch = '';
    this.exitSuggestions.set([]);
  }

  protected onExitSearchInput(): void {
    const q = this.exitSearch.trim();
    if (q.length < 3) { this.exitSuggestions.set([]); return; }
    this.stockApi.search(q, this.exitTypeId).subscribe({
      next: list => {
        const tempIds = new Set(this.exitTempItems().map(i => i.id));
        this.exitSuggestions.set(list.filter(i => !tempIds.has(i.id)));
      },
      error: () => this.exitSuggestions.set([]),
    });
  }

  protected goExitTempPage(p: number): void { this.exitTempPage.set(p); }

  protected addToExitList(item: any) {
    this.exitTempItems.update(list => list.some(i => i.id === item.id) ? list : [...list, item]);
  }

  protected selectExitSuggestion(item: any): void {
    this.addToExitList(item);
    this.exitSearch = '';
    this.exitSuggestions.set([]);
    this.exitTempPage.set(this.exitTempPages());
  }

  protected removeFromExitList(itemId: number) {
    this.exitTempItems.update(list => list.filter(i => i.id !== itemId));
    if (this.exitTempPage() > this.exitTempPages()) {
      this.exitTempPage.set(this.exitTempPages());
    }
  }

  protected readonly categories = signal<{ id: number; name: string; unit: string }[]>([]);

  protected get selectedType(): any { return this.productTypes().find(t => t.id === this.countTypeId); }
  protected getTypeName(id: number | null): string { return this.productTypes().find(t => t.id === id)?.name ?? ''; }
  protected onTypeChange(): void { this.countUnit = this.unitForType(this.countTypeId); }

  private unitForType(typeId: number | null): string {
    const type = this.productTypes().find(t => t.id === typeId);
    if (!type) return 'unidades';
    const cat = this.categories().find(c => c.name === type.category);
    return cat?.unit || 'unidades';
  }

  protected openMovement(type: 'entry' | 'exit') {
    this.movementType.set(type);
    this.movementSearch = '';
    this.movementSuggestions.set([]);
    this.movementSelected.set(null);
    this.movementQty = 1;
    this.movementDesc = '';
    this.movementError.set('');
    this.showMovementModal.set(true);
  }
  protected closeMovementModal() { this.showMovementModal.set(false); }

  protected onMovementSearchInput(): void {
    const q = this.movementSearch.trim();
    if (q.length < 3) { this.movementSuggestions.set([]); return; }
    this.stockApi.search(q).subscribe({
      next: list => this.movementSuggestions.set(list),
      error: () => this.movementSuggestions.set([]),
    });
  }

  protected selectMovementSuggestion(item: any): void {
    this.movementSelected.set(item);
    this.movementSearch = '';
    this.movementSuggestions.set([]);
    this.movementQty = 1;
    this.movementError.set('');
  }

  protected submitMovement(): void {
    const item = this.movementSelected();
    if (!item || this.movementBusy()) return;
    if (this.movementType() === 'exit' && this.movementQty > item.quantity) {
      this.movementError.set('Quantidade insuficiente em estoque.');
      return;
    }
    this.movementBusy.set(true);
    this.movementError.set('');
    const dto = { quantity: this.movementQty, description: this.movementDesc.trim() };
    const req = this.movementType() === 'entry'
      ? this.stockApi.registerEntry(item.id, dto)
      : this.stockApi.registerExit(item.id, dto);
    req.subscribe({
      next: () => {
        this.closeMovementModal();
        this.loadStockItems();
        this.loadMovements();
      },
      error: () => {
        this.movementError.set(this.movementType() === 'exit'
          ? 'Quantidade insuficiente em estoque.'
          : 'Não foi possível registrar a movimentação.');
        this.movementBusy.set(false);
      },
    });
  }

  protected openCreate() {
    this.createStep.set('type');
    this.showCreateModal.set(true);
  }
  protected selectCreateType(type: 'entry' | 'exit') {
    const today = new Date().toISOString().split('T')[0];
    this.formType = type;
    this.formName = ''; this.formDesc = ''; this.formStart = today; this.formEnd = today;
    this.createStep.set('form');
  }
  protected closeCreate() { this.showCreateModal.set(false); }
  protected submitCreate() {
    this.inboundApi.create({
      name: this.formName, description: this.formDesc,
      startDate: this.formStart, endDate: this.formEnd, type: this.formType
    }).subscribe(p => {
      this.processes.update(list => [p, ...list]);
      this.closeCreate();
    });
  }

  protected startCounting(p: InboundProcess) {
    this.activeProcess.set(p);
    if (p.type === 'exit') {
      this.exitTypeId = null; this.exitSearch = '';
      this.exitTempItems.set([]);
      this.exitSuggestions.set([]);
      this.exitTempPage.set(1);
    } else {
      this.countName = ''; this.countExpiry = ''; this.countTypeId = null;
      this.countUnit = 'unidades'; this.countSearchResults.set([]);
    }
    this.showCountModal.set(true);
  }
  protected closeCounting() { this.showCountModal.set(false); }

  protected onCountNameInput() {
    const q = this.countName.toLowerCase().trim();
    const tid = this.countTypeId;
    if (!q || !tid) { this.countSearchResults.set([]); return; }
    const items = this.stockItems().filter(i =>
      i.productTypeId === tid &&
      (i.name.toLowerCase().includes(q) || (i.description || '').toLowerCase().includes(q))
    );
    this.countSearchResults.set(items.slice(0, 8));
  }
  protected selectStock(item: any) { this.countName = item.name; this.countSearchResults.set([]); }

  protected submitItem() {
    if (!this.countName.trim() || !this.countTypeId) return;
    const p = this.activeProcess(); if (!p) return;
    this.inboundApi.addItem(p.id, {
      productTypeId: this.countTypeId, itemId: null,
      name: this.countName, quantity: 1, unit: this.countUnit,
      expiryDate: this.countExpiry || null
    }).subscribe(item => {
      this.inboundItems.update(list => [...list, item]);
      this.countExpiry = ''; this.countSearchResults.set([]);
    });
  }

  protected submitExitItems() {
    const p = this.activeProcess(); if (!p) return;
    const list = this.exitTempItems();
    if (list.length === 0) return;
    const adds = list.map(item => this.inboundApi.addItem(p.id, {
      productTypeId: item.productTypeId ?? this.exitTypeId,
      itemId: item.id,
      name: item.name,
      quantity: 1,
      unit: item.unit || 'unidades',
      expiryDate: item.expiryDate || null,
    }));
    forkJoin(adds).subscribe(() => {
      this.inboundApi.finish(p.id).subscribe(() => {
        this.loadProcesses();
        this.loadStockItems();
        this.loadMovements();
        this.activeProcess.set(null); this.closeCounting();
      });
    });
  }

  protected resumeProcess(p: InboundProcess) {
    this.inboundApi.resume(p.id).subscribe(updated => {
      this.processes.update(list => list.map(x => x.id === p.id ? updated : x));
    });
  }

  protected pauseCounting() {
    const p = this.activeProcess(); if (!p) return;
    this.inboundApi.pause(p.id).subscribe(updated => {
      this.processes.update(list => list.map(x => x.id === p.id ? updated : x));
      this.activeProcess.set(null); this.closeCounting();
    });
  }

  protected finishCounting() {
    const p = this.activeProcess(); if (!p) return;
    this.inboundApi.finish(p.id).subscribe(() => {
      this.loadProcesses();
      this.loadStockItems();
      this.loadMovements();
      this.activeProcess.set(null); this.closeCounting();
    });
  }

  protected cancelProcess(p: InboundProcess) {
    this.inboundApi.cancel(p.id).subscribe(() => {
      this.loadProcesses();
      this.inboundItems.update(list => list.filter(i => i.processId !== p.id));
      if (this.activeProcess()?.id === p.id) {
        this.activeProcess.set(null); this.closeCounting();
      }
    });
  }

  protected viewProcess(p: InboundProcess) {
    this.processDetailView.set(p);
    this.itemPage.set(1);
    this.loadItems(p.id);
  }
  protected backToProcessList() { this.processDetailView.set(null); }

  protected removeItem(processId: number, itemId: number) {
    this.inboundApi.deleteItem(processId, itemId).subscribe(() => {
      this.loadItems(processId);
    });
  }

  protected switchSection(s: 'movements' | 'processes') {
    this.currentSection.set(s);
    if (s === 'processes') this.loadProcesses();
    if (s === 'movements') this.loadMovements();
  }

  protected goProcessPage(p: number) {
    this.processPage.set(p);
    this.loadProcesses();
  }

  protected goItemPage(p: number) {
    this.itemPage.set(p);
    if (this.processDetailView()) this.loadItems(this.processDetailView()!.id);
  }

  protected goMovementPage(p: number) {
    this.movementPage.set(p);
    this.loadMovements();
  }

  protected openMovementDetail(m: Movement): void {
    this.movementDetail.set(m);
    this.movementDetailPage.set(1);
    this.loadMovementDetailItems(m.id);
  }
  protected closeMovementDetail(): void { this.movementDetail.set(null); }
  protected goMovementDetailPage(p: number): void {
    this.movementDetailPage.set(p);
    const m = this.movementDetail();
    if (m) this.loadMovementDetailItems(m.id);
  }

  private loadMovementDetailItems(id: number): void {
    this.movementDetailLoading.set(true);
    this.movementApi.getGroupItems(id, this.movementDetailPage(), 10).subscribe({
      next: r => {
        this.movementDetailItems.set(r.data);
        this.movementDetailTotalPages.set(r.totalPages);
        this.movementDetailLoading.set(false);
      },
      error: () => this.movementDetailLoading.set(false),
    });
  }

  protected loadItems(processId: number) {
    this.inboundApi.getItemsPaged(processId, this.itemPage(), 10).subscribe(r => {
      this.inboundItems.update(list => [...list.filter(i => i.processId !== processId), ...r.data]);
      this.itemTotalPages.set(r.totalPages);
    });
  }

  private loadProcesses() {
    this.loading.set(true);
    this.inboundApi.getAll(this.processPage(), 10).subscribe({
      next: r => {
        this.processes.set(r.data);
        this.processTotalPages.set(r.totalPages);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  private loadStockItems() {
    this.stockApi.getAllUnpaged().subscribe({
      next: list => this.stockItems.set(list),
    });
  }

  private loadMovements() {
    const f = this.activeMovementFilter();
    const params: Record<string, string> = {};
    if (f['name']) params['q'] = f['name'];
    if (f['source']) params['source'] = f['source'];
    if (f['type']) params['type'] = f['type'];
    if (f['dateStart']) params['from'] = f['dateStart'];
    if (f['dateEnd']) params['to'] = f['dateEnd'];
    this.movementApi.getAll(this.movementPage(), 10, params).subscribe({
      next: r => {
        this.movements.set(r.data);
        this.movementTotalPages.set(r.totalPages);
      },
    });
  }

  private loadProductTypes() {
    this.productTypeApi.getAll().subscribe({
      next: list => this.productTypes.set(list.map(pt => ({ id: pt.id, name: pt.name, category: pt.category }))),
    });
    this.productTypeApi.getCategories().subscribe({
      next: list => this.categories.set(list),
    });
  }

  constructor() {
    this.loadProductTypes();
    this.loadStockItems();
  }
}
