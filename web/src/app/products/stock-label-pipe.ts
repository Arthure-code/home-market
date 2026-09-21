import { Pipe, PipeTransform } from '@angular/core';

// What a stock says to a shopper: in stock, only a few left, or none.
// The long form is for the product page and the quick view.
@Pipe({ name: 'stockLabel' })
export class StockLabelPipe implements PipeTransform {
  transform(stock: number, form: 'short' | 'long' = 'short'): string {
    if (stock === 0) return 'Out of stock';
    if (stock <= 3) return form === 'long' ? `Only ${stock} left in stock` : `Only ${stock} left`;
    return 'In stock';
  }
}
