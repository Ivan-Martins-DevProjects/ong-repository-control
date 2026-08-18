import { Injectable, signal } from '@angular/core';
import type { Movement } from './item';

@Injectable({ providedIn: 'root' })
export class StockMovementService {
  private readonly movements = signal<Movement[]>(this.seed());
  readonly all = this.movements.asReadonly();

  addEntry(itemId: number, itemName: string, quantity: number, description: string): void {
    this.movements.update(list => [{ id: Date.now(), itemId, itemName, type: 'entry', quantity, date: new Date(), description }, ...list]);
  }

  addExit(itemId: number, itemName: string, quantity: number, description: string): void {
    this.movements.update(list => [{ id: Date.now(), itemId, itemName, type: 'exit', quantity, date: new Date(), description }, ...list]);
  }

  private seed(): Movement[] {
    return [
      { id: 1, itemId: 1, itemName: 'Arroz - Pacote 5Kg', type: 'entry', quantity: 50, date: new Date('2026-06-01'), description: 'Doação Mercado Solidário' },
      { id: 2, itemId: 1, itemName: 'Arroz - Pacote 5Kg', type: 'exit', quantity: 10, date: new Date('2026-06-15'), description: 'Distribuição famílias' },
      { id: 3, itemId: 2, itemName: 'Feijão - Carioca 1Kg', type: 'entry', quantity: 80, date: new Date('2026-06-10'), description: 'Campanha da Igreja' },
      { id: 4, itemId: 2, itemName: 'Feijão - Carioca 1Kg', type: 'exit', quantity: 20, date: new Date('2026-06-20'), description: 'Cesta básica' },
    ];
  }
}