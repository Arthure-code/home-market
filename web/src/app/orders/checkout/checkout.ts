import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../auth/api-message';
import { CartService } from '../../cart/cart.service';
import { Checkout as CheckoutDraft } from '../order';
import { OrderService } from '../order.service';

// Where to ship and how to pay, next to what the cart adds up to. The
// card goes to the API with the order and nowhere else; a refused card
// leaves the cart as it was.
@Component({
  selector: 'app-checkout',
  imports: [CurrencyPipe, FormsModule, RouterLink],
  templateUrl: './checkout.html',
})
export class Checkout {
  private readonly cartService = inject(CartService);
  private readonly orders = inject(OrderService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);

  readonly cart = this.cartService.cart;
  readonly empty = computed(() => this.cart().lines.length === 0);
  readonly placing = signal(false);
  readonly draft: CheckoutDraft = {
    fullName: '',
    street: '',
    city: '',
    province: 'QC',
    postalCode: '',
    country: 'Canada',
    cardNumber: '',
    cardHolderName: '',
    expiry: '',
    securityCode: '',
  };

  placeOrder(): void {
    const d = this.draft;
    const missing = Object.entries(d).find(([, value]) => !value.trim());
    if (missing) {
      this.toastr.error('Please fill in every field');
      return;
    }
    this.placing.set(true);
    this.orders.place(d).subscribe({
      next: (order) => {
        this.cartService.clear();
        this.draft.cardNumber = '';
        this.draft.cardHolderName = '';
        this.draft.expiry = '';
        this.draft.securityCode = '';
        this.toastr.success('Order placed. Thank you!');
        this.router.navigate(['/orders', order.id]);
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.placing.set(false);
      },
    });
  }
}
