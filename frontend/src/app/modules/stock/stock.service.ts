import { Injectable, signal } from '@angular/core';
import type { Item } from './item';

@Injectable({ providedIn: 'root' })
export class StockService {
  private readonly items = signal<Item[]>(this.seed());

  readonly all = this.items.asReadonly();

  getById(id: number): Item | undefined {
    return this.items().find(i => i.id === id);
  }

  add(item: Omit<Item, 'id'>): boolean {
    const nextId = Math.max(0, ...this.items().map(i => i.id)) + 1;
    this.items.update(list => [...list, { ...item, id: nextId }]);
    return true;
  }

  update(id: number, data: Partial<Omit<Item, 'id'>>): boolean {
    this.items.update(list =>
      list.map(i => (i.id === id ? { ...i, ...data } : i))
    );
    return true;
  }

  delete(id: number): boolean {
    this.items.update(list => list.filter(i => i.id !== id));
    return true;
  }

  adjustQuantity(id: number, delta: number, description: string): boolean {
    this.items.update(list =>
      list.map(i => {
        if (i.id !== id) return i;
        const newQty = i.quantity + delta;
        if (newQty < 0) return i;
        return { ...i, quantity: newQty };
      })
    );
    return true;
  }

  private seed(): Item[] {
    return [
      { id: 1, name: 'Arroz', description: 'Solito', category: 'Alimentos', quantity: 1, unit: 'unidades', minQuantity: 0, donor: 'Mercado Solidário', entryDate: new Date('2026-06-01'), expiryDate: new Date('2027-06-01') },
      { id: 2, name: 'Arroz', description: 'Solito', category: 'Alimentos', quantity: 1, unit: 'unidades', minQuantity: 0, donor: 'Mercado Solidário', entryDate: new Date('2026-06-01'), expiryDate: new Date('2027-06-01') },
      { id: 3, name: 'Arroz', description: 'Namorado', category: 'Alimentos', quantity: 1, unit: 'unidades', minQuantity: 0, donor: 'Campanha da Igreja', entryDate: new Date('2026-06-10'), expiryDate: new Date('2027-12-10') },
      { id: 4, name: 'Arroz', description: 'Namorado', category: 'Alimentos', quantity: 1, unit: 'unidades', minQuantity: 0, donor: 'Doador Anônimo', entryDate: new Date('2026-07-15'), expiryDate: new Date('2027-07-15') },
      { id: 5, name: 'Arroz', description: 'Bom Pastor', category: 'Alimentos', quantity: 1, unit: 'unidades', minQuantity: 0, donor: 'Supermercado Centro', entryDate: new Date('2026-05-20'), expiryDate: new Date('2027-05-20') },
      { id: 6, name: 'Óleo', description: 'Soja 900ml', category: 'Alimentos', quantity: 1, unit: 'unidades', minQuantity: 0, donor: 'Doador Anônimo', entryDate: new Date('2026-07-01'), expiryDate: new Date('2027-01-01') },
      { id: 7, name: 'Leite', description: 'Em Pó 400g', category: 'Alimentos', quantity: 1, unit: 'unidades', minQuantity: 0, donor: 'Farmácia Popular', entryDate: new Date('2026-06-15'), expiryDate: null },
      { id: 8, name: 'Sabonete', description: 'Líquido 200ml', category: 'Higiene', quantity: 1, unit: 'unidades', minQuantity: 0, donor: 'Doador Anônimo', entryDate: new Date('2026-06-20'), expiryDate: null },
    ];
  }
}