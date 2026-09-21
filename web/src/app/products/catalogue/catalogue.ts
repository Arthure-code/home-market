import { Component, computed, input } from '@angular/core';
import { ProductList } from '../product-list/product-list';

// The catalogue page: everything on sale, or what a search or a category
// keeps of it. Both come from the address, so a result can be shared.
@Component({
  selector: 'app-catalogue',
  imports: [ProductList],
  templateUrl: './catalogue.html',
})
export class Catalogue {
  readonly q = input<string>();
  readonly category = input<string>();
  protected readonly title = computed(() => {
    if (this.category()) return this.category();
    const q = this.q()?.trim();
    return q ? `Results for "${q}"` : 'Catalogue';
  });
}
