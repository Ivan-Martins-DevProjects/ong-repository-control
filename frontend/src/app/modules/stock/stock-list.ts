import { Component, input, output, inject } from '@angular/core';
import { Router } from '@angular/router';

export interface TypeGroup { id: number; name: string; count: number; total: number; category: string; }

@Component({
  selector: 'app-stock-list',
  imports: [],
  templateUrl: './stock-list.html',
  styleUrl: './stock-list.css'
})
export class StockList {
  private readonly router = inject(Router);
  readonly categories = input<TypeGroup[]>([]);
  readonly loading = input(false);
  readonly add = output();
  readonly edit = output<TypeGroup>();
  readonly remove = output<TypeGroup>();

  openItems(type: TypeGroup): void {
    void this.router.navigate(['/estoque', type.name]);
  }
}