import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ProductTypeApiService } from '../../services/product-type-api.service';

interface Category { id: number; name: string; unit: string; }

@Component({
  selector: 'app-categories-page',
  imports: [FormsModule],
  templateUrl: './categories-page.html',
  styleUrl: './categories-page.css'
})
export class CategoriesPage {
  private readonly api = inject(ProductTypeApiService);

  protected readonly categories = signal<Category[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected newCategory = '';

  protected readonly editing = signal<Category | null>(null);
  protected editingName = '';
  protected readonly saving = signal(false);

  protected readonly deleting = signal<Category | null>(null);
  protected readonly deletingBusy = signal(false);

  constructor() { void this.load(); }

  private async load(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      this.categories.set(await firstValueFrom(this.api.getCategories()));
    } catch {
      this.error.set('Não foi possível carregar as categorias.');
    } finally {
      this.loading.set(false);
    }
  }

  protected async addCategory(): Promise<void> {
    const name = this.newCategory.trim();
    if (!name || this.saving()) return;
    this.saving.set(true);
    try {
      await firstValueFrom(this.api.createCategory(name));
      this.newCategory = '';
      await this.load();
    } catch {
      this.error.set('Não foi possível criar a categoria.');
    } finally {
      this.saving.set(false);
    }
  }

  protected openEdit(cat: Category): void {
    this.editing.set(cat);
    this.editingName = cat.name;
  }
  protected closeEdit(): void { this.editing.set(null); }

  protected async saveEdit(): Promise<void> {
    const target = this.editing();
    if (!target || !this.editingName.trim() || this.saving()) return;
    this.saving.set(true);
    try {
      await firstValueFrom(this.api.updateCategory(target.id, this.editingName.trim()));
      this.closeEdit();
      await this.load();
    } catch {
      this.error.set('Não foi possível atualizar a categoria.');
    } finally {
      this.saving.set(false);
    }
  }

  protected requestDelete(cat: Category): void { this.deleting.set(cat); }
  protected closeDelete(): void { this.deleting.set(null); }

  protected async doDelete(cat: Category): Promise<void> {
    if (this.deletingBusy()) return;
    this.deletingBusy.set(true);
    try {
      await firstValueFrom(this.api.deleteCategory(cat.name));
      this.deleting.set(null);
      await this.load();
    } catch {
      this.error.set('Não foi possível excluir a categoria.');
    } finally {
      this.deletingBusy.set(false);
    }
  }
}