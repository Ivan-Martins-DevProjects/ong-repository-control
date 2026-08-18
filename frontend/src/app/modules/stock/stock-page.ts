import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { StockList, type TypeGroup } from './stock-list';
import { ItemFilter, type FilterField, type FilterValues } from '../../components/item-filter/item-filter';
import { ProductTypeApiService, type ProductType } from '../../services/product-type-api.service';

@Component({
  selector: 'app-stock-page',
  imports: [StockList, FormsModule, ItemFilter],
  templateUrl: './stock-page.html',
  styleUrl: './stock-page.css'
})
export class StockPage {
  private readonly api = inject(ProductTypeApiService);

  protected readonly types = signal<ProductType[]>([]);
  protected readonly categoryOptions = signal<string[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');

  protected readonly activeStockFilter = signal<FilterValues>({});
  protected readonly stockFilterFields = computed<FilterField[]>(() => [
    { key: 'name', label: 'Nome', type: 'text' },
    { key: 'category', label: 'Categoria', type: 'select', options: [
      { value: '', label: 'Todas' },
      ...this.categoryOptions().map(c => ({ value: c, label: c })),
      { value: 'Sem categoria', label: 'Sem categoria' },
    ]},
    { key: 'sort', label: 'Ordenar por', type: 'select', inline: true, options: [
      { value: '', label: 'Padrão' },
      { value: 'total_desc', label: 'Quantidade (maior)' },
      { value: 'total_asc', label: 'Quantidade (menor)' },
      { value: 'name_asc', label: 'Nome (A–Z)' },
      { value: 'name_desc', label: 'Nome (Z–A)' },
    ]},
  ]);

  protected readonly page = signal(1);
  protected readonly pageSize = 10;

  protected readonly filteredTypes = computed(() => {
    const f = this.activeStockFilter();
    const q = (f['name'] || '').toLowerCase().trim();
    const cat = f['category'] || '';
    let list = this.types();
    if (q) list = list.filter(t => t.name.toLowerCase().includes(q));
    if (cat) list = list.filter(t => (t.category || 'Sem categoria') === cat);
    const sort = f['sort'];
    if (sort === 'total_asc') list = [...list].sort((a, b) => a.totalQuantity - b.totalQuantity);
    if (sort === 'total_desc') list = [...list].sort((a, b) => b.totalQuantity - a.totalQuantity);
    if (sort === 'name_asc') list = [...list].sort((a, b) => a.name.localeCompare(b.name));
    if (sort === 'name_desc') list = [...list].sort((a, b) => b.name.localeCompare(a.name));
    return list;
  });

  protected readonly totalPages = computed(() => Math.max(1, Math.ceil(this.filteredTypes().length / this.pageSize)));

  protected readonly pageTypes = computed(() => {
    const p = this.page();
    return this.filteredTypes().slice((p - 1) * this.pageSize, p * this.pageSize);
  });

  protected readonly showForm = signal(false);
  protected readonly editing = signal<ProductType | null>(null);
  protected formName = '';
  protected formCategory = 'Outros';
  protected readonly formCats = signal<string[]>([]);
  protected readonly saving = signal(false);

  protected readonly deleting = signal<TypeGroup | null>(null);
  protected readonly deletingBusy = signal(false);

  readonly groups = computed<TypeGroup[]>(() =>
    this.pageTypes().map(t => ({
      id: t.id,
      name: t.name,
      count: t.itemCount,
      total: t.totalQuantity,
      category: t.category || 'Sem categoria',
    }))
  );

  constructor() {
    void this.load();
    void this.loadCategories();
  }

  private async loadCategories(): Promise<void> {
    try {
      const list = await firstValueFrom(this.api.getCategories());
      this.categoryOptions.set(list.map(c => c.name));
    } catch {
      this.categoryOptions.set([]);
    }
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      this.types.set(await firstValueFrom(this.api.getAll()));
    } catch {
      this.error.set('Não foi possível carregar os tipos de produto.');
    } finally {
      this.loading.set(false);
    }
  }

  protected goPage(p: number): void {
    if (p < 1 || p > this.totalPages() || p === this.page()) return;
    this.page.set(p);
  }

  protected onApplyStockFilter(v: FilterValues): void {
    this.activeStockFilter.set(v);
    this.page.set(1);
  }

  protected onClearStockFilter(): void {
    this.activeStockFilter.set({});
    this.page.set(1);
  }

  protected openAdd(): void {
    this.editing.set(null);
    this.formName = '';
    this.formCategory = '';
    this.formCats.set(['', ...this.categoryOptions()]);
    this.showForm.set(true);
  }

  protected openEdit(group: TypeGroup): void {
    const type = this.types().find(t => t.id === group.id) ?? null;
    this.editing.set(type);
    this.formName = group.name;
    this.formCategory = type?.category ?? group.category;
    if (this.formCategory === 'Sem categoria') this.formCategory = '';
    this.formCats.set([...new Set(['', this.formCategory, ...this.categoryOptions()])]);
    this.showForm.set(true);
  }

  protected closeForm(): void { this.editing.set(null); this.showForm.set(false); }

  protected async saveItem(): Promise<void> {
    if (!this.formName.trim() || this.saving()) return;
    this.saving.set(true);
    try {
      const data = { name: this.formName.trim(), category: this.formCategory || '' };
      const target = this.editing();
      if (target) {
        await firstValueFrom(this.api.update(target.id, data));
      } else {
        await firstValueFrom(this.api.create(data));
      }
      this.closeForm();
      await this.load();
    } catch {
      this.error.set('Não foi possível salvar o tipo de produto.');
    } finally {
      this.saving.set(false);
    }
  }

  protected requestDelete(group: TypeGroup): void {
    if (group.count > 0) {
      this.deleting.set(group);
      return;
    }
    void this.doDelete(group);
  }

  protected closeDelete(): void { this.deleting.set(null); }

  protected async doDelete(group: TypeGroup): Promise<void> {
    if (this.deletingBusy()) return;
    this.deletingBusy.set(true);
    try {
      await firstValueFrom(this.api.delete(group.id));
      this.deleting.set(null);
      await this.load();
    } catch {
      this.error.set('Não foi possível excluir o tipo de produto.');
    } finally {
      this.deletingBusy.set(false);
    }
  }
}