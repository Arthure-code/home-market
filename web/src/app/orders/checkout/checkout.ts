import { CurrencyPipe } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../helpers/api-message';
import { Cart } from '../../models/cart';
import { Checkout as CheckoutDraft } from '../../models/order';
import { CartService } from '../../services/cart.service';
import { OrderService } from '../../services/order.service';

// Where to ship and how to pay, next to what the cart adds up to. The
// card goes to the API with the order and nowhere else; a refused card
// leaves the cart as it was.
@Component({
  selector: 'app-checkout',
  imports: [CurrencyPipe, FormsModule, RouterLink],
  templateUrl: './checkout.html',
})
export class Checkout {
  placing = false;
  draft: CheckoutDraft = {
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

  constructor(
    private cartService: CartService,
    private orders: OrderService,
    private router: Router,
    private toastr: ToastrService,
  ) {}

  get cart(): Cart {
    return this.cartService.cart;
  }

  get empty(): boolean {
    return this.cart.lines.length === 0;
  }

  placeOrder(): void {
    const d = this.draft;
    if (Object.values(d).some((value) => !value.trim())) {
      this.toastr.error('Please fill in every field');
      return;
    }
    this.placing = true;
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
        this.placing = false;
      },
    });
  }
}
