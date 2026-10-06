import { Component, inject, OnInit, signal } from '@angular/core';
import { StockApiService } from '../../services/product-type-api.service';
import { emitErrorToast } from '../../components/error-toast/error-toast.events';

@Component({
  selector: 'app-stock',
  imports: [],
  templateUrl: './stock.html',
  styleUrl: './stock.css',
})
export class Stock {
  private readonly api = inject(StockApiService)

  protected loading = signal(false)

  ngOnInit(): void {
    this.loading.set(true)
    this.api.getAll().subscribe({
      error: err => {
        emitErrorToast('Erro ao buscar itens no estoque', err.message)
        console.error(err);

      }
    })
    this.loading.set(false)
  }
}
