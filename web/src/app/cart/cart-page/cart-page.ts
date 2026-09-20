import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../auth/api-message';
import { CartLine, MAX_QUANTITY } from '../cart';
import { CartService } from '../cart.service';

// My cart: one line per product with its quantity to change or remove,
// and the totals as the API adds them up.
@Component({
  selector: 'app-cart-page',
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './cart-page.html',
  styleUrl: './cart-page.css',
})
export class CartPage {
  private readonly service = inject(CartService);
  private readonly toastr = inject(ToastrService);

  readonly cart = this.service.cart;
  readonly busy = signal(false);
  readonly empty = computed(() => this.cart().lines.length === 0);

  // One to ten, never more than the stock, and at least what is there.
  quantities(line: CartLine): number[] {
    const most = Math.max(Math.min(line.stock, MAX_QUANTITY), line.quantity);
    return Array.from({ length: most }, (_, i) => i + 1);
  }

  setQuantity(line: CartLine, quantity: number): void {
    this.busy.set(true);
    this.service.setQuantity(line.productId, quantity).subscribe({
      next: () => this.busy.set(false),
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.busy.set(false);
      },
    });
  }

  remove(line: CartLine): void {
    this.busy.set(true);
    this.service.remove(line.productId).subscribe({
      next: () => {
        this.toastr.success(`${line.title} removed from your cart`);
        this.busy.set(false);
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.busy.set(false);
      },
    });
  }
}
