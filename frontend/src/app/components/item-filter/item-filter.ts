import { Component, computed, input, output, signal, effect } from '@angular/core';
import { FormsModule } from '@angular/forms';

export interface FilterField {
  key: string;
  label: string;
  type: 'text' | 'date' | 'select';
  options?: { value: string; label: string }[];
  inline?: boolean;
}

export interface FilterValues {
  [key: string]: string;
}

export const STOCK_FILTER_FIELDS: FilterField[] = [
  { key: 'donor', label: 'Doador', type: 'text' },
  { key: 'entryDate', label: 'Entrada', type: 'date' },
  { key: 'expiryDate', label: 'Validade', type: 'date' },
];

export const MOVEMENT_FILTER_FIELDS: FilterField[] = [
  { key: 'name', label: 'Nome/Descrição', type: 'text' },
  { key: 'date', label: 'Data', type: 'date' },
  { key: 'source', label: 'Tipo', type: 'select', inline: true, options: [
    { value: '', label: 'Todos' },
    { value: 'item', label: 'Unitária' },
    { value: 'process', label: 'Processo' },
  ]},
  { key: 'type', label: 'Fluxo', type: 'select', inline: true, options: [
    { value: '', label: 'Todos' },
    { value: 'entry', label: 'Entrada' },
    { value: 'exit', label: 'Saída' },
  ]},
];

export const PROCESS_FILTER_FIELDS: FilterField[] = [
  { key: 'name', label: 'Nome', type: 'text' },
  { key: 'startDate', label: 'Início', type: 'date' },
  { key: 'endDate', label: 'Término', type: 'date' },
  { key: 'type', label: 'Tipo', type: 'select', inline: true, options: [
    { value: '', label: 'Todos' },
    { value: 'entry', label: 'Entrada' },
    { value: 'exit', label: 'Saída' },
  ]},
  { key: 'status', label: 'Status', type: 'select', inline: true, options: [
    { value: '', label: 'Todos' },
    { value: 'active', label: 'Ativo' },
    { value: 'paused', label: 'Pausado' },
    { value: 'completed', label: 'Finalizado' },
    { value: 'cancelled', label: 'Cancelado' },
  ]},
];

@Component({
  selector: 'app-item-filter',
  imports: [FormsModule],
  templateUrl: './item-filter.html',
  styleUrl: './item-filter.css'
})
export class ItemFilter {
  readonly fields = input<FilterField[]>([]);
  readonly filterValues = input<FilterValues>({});
  readonly expanded = input(false);
  readonly apply = output<FilterValues>();
  readonly clear = output<void>();

  protected open = signal(false);
  protected values: Record<string, string> = {};
  protected readonly activeCount = signal(0);

  protected readonly rows = computed(() => {
    const result: FilterField[][] = [];
    for (const f of this.fields()) {
      if (f.inline && result.length > 0 && result[result.length - 1].every(x => x.inline)) {
        result[result.length - 1].push(f);
      } else {
        result.push([f]);
      }
    }
    return result;
  });

  constructor() {
    effect(() => { if (this.expanded()) this.open.set(true); });
    effect(() => { this.values = { ...this.filterValues() }; });
  }

  protected toggle(): void {
    if (this.expanded()) return;
    this.open.update(v => !v);
    if (this.open()) this.values = { ...this.filterValues() };
  }

  protected getVal(key: string): string { return this.values[key] ?? ''; }
  protected setVal(key: string, val: string): void { this.values[key] = val; }

  protected doApply(): void {
    const count = Object.values(this.values).filter(v => !!v).length;
    this.activeCount.set(count);
    this.apply.emit({ ...this.values });
    if (!this.expanded()) this.open.set(false);
  }

  protected doClear(): void {
    this.values = {};
    this.activeCount.set(0);
    this.clear.emit();
  }
}
