import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StockService } from './stock.service';
import { ItemFilter, STOCK_FILTER_FIELDS, type FilterValues } from '../../components/item-filter/item-filter';
import type { Item } from './item';

@Component({
  selector: 'app-stock-items-page',
  imports: [DatePipe, FormsModule, RouterLink, ItemFilter],
  templateUrl: './stock-items-page.html',
  styleUrl: './stock-items-page.css'
})
export class StockItemsPage {
  private readonly stock = inject(StockService);
  private readonly route = inject(ActivatedRoute);
  protected readonly typeName = decodeURIComponent(this.route.snapshot.paramMap.get('name') ?? '');
  protected readonly showDetailed = signal(true);

  protected readonly filterFields = STOCK_FILTER_FIELDS;
  protected readonly activeFilter = signal<FilterValues>({});

  readonly allItems = computed(() => this.stock.all().filter(i => i.name === this.typeName));

  readonly currentCategory = computed(() => this.allItems()[0]?.category ?? null);

  readonly items = computed(() => {
    const f = this.activeFilter();
    let list = this.allItems();
    const donor = (f['donor'] ?? '').toLowerCase().trim();
    if (donor) list = list.filter(i => i.donor.toLowerCase().includes(donor));
    if (f['entryDateStart']) list = list.filter(i => new Date(i.entryDate) >= new Date(f['entryDateStart']));
    if (f['entryDateEnd']) list = list.filter(i => new Date(i.entryDate) <= new Date(f['entryDateEnd']));
    if (f['expiryDateStart']) list = list.filter(i => i.expiryDate && new Date(i.expiryDate) >= new Date(f['expiryDateStart']));
    if (f['expiryDateEnd']) list = list.filter(i => i.expiryDate && new Date(i.expiryDate) <= new Date(f['expiryDateEnd']));
    return list;
  });

  readonly summaryGroups = computed(() => {
    const map = new Map<string, number>();
    for (const i of this.items()) {
      const d = i.description || i.name;
      map.set(d, (map.get(d) ?? 0) + 1);
    }
    return Array.from(map.entries()).map(([description, count]) => ({ description, count }));
  });

  protected toggleView(): void { this.showDetailed.update(v => !v); }

  protected onApplyFilter(v: FilterValues): void { this.activeFilter.set(v); }
  protected onClearFilter(): void { this.activeFilter.set({}); }

  protected readonly showForm = signal(false);
  protected formVariant = ''; formEntry = ''; formExpiry = ''; formDonor = '';
  protected editingItemId: number | null = null;

  protected readonly showDelete = signal(false);
  protected deletingItemId = 0;
  protected deletingItemName = '';

  protected openAdd(): void {
    this.formVariant = ''; this.formDonor = '';
    this.formEntry = new Date().toISOString().split('T')[0];
    this.formExpiry = ''; this.editingItemId = null;
    this.showForm.set(true);
  }

  protected editItem(item: Item): void {
    this.formVariant = item.description || item.name;
    this.formEntry = new Date(item.entryDate).toISOString().split('T')[0];
    this.formExpiry = item.expiryDate ? new Date(item.expiryDate).toISOString().split('T')[0] : '';
    this.formDonor = item.donor;
    this.editingItemId = item.id;
    this.showForm.set(true);
  }

  protected closeForm(): void { this.showForm.set(false); }

  protected saveItem(): void {
    const data = {
      name: this.typeName, description: this.formVariant, category: this.currentCategory() ?? 'Outros',
      quantity: 1, unit: 'unidades', minQuantity: 0,
      donor: this.formDonor, entryDate: new Date(this.formEntry),
      expiryDate: this.formExpiry ? new Date(this.formExpiry) : null,
    };
    if (this.editingItemId) this.stock.update(this.editingItemId, data);
    else this.stock.add(data);
    this.closeForm();
  }

  protected confirmDelete(item: Item): void {
    this.deletingItemId = item.id;
    this.deletingItemName = item.description || item.name;
    this.showDelete.set(true);
  }
  protected cancelDelete(): void { this.showDelete.set(false); }
  protected doDelete(): void { this.stock.delete(this.deletingItemId); this.cancelDelete(); }
}