import { Component, inject, OnInit, signal } from '@angular/core';
import { StockApiService } from '../../services/product-type-api.service';

@Component({
  selector: 'app-stock',
  imports: [],
  templateUrl: './stock.html',
  styleUrl: './stock.css',
})
export class Stock {
  private readonly api = inject(StockApiService)

  protected loading = signal(true)

  ngOnInit(): void {
    this.api.getAll().subscribe({
      error: err => {
        alert("Erro ao buscar lista de items")
        console.error("Erro ao buscar lista de items: ", err)
      }
    })
  }
}
