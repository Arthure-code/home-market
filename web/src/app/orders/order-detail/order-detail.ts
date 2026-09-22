import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, effect, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Order } from '../../models/order';
import { OrderService } from '../../services/order.service';

// One of my orders, reached by its number: the lines as they were sold,
// the totals, the address and the card by its last four digits. The API
// answers its buyer only; anyone else's order reads as missing.
@Component({
  selector: 'app-order-detail',
  imports: [CurrencyPipe, DatePipe, RouterLink],
  templateUrl: './order-detail.html',
})
export class OrderDetail {
  private readonly service = inject(OrderService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);

  readonly id = input.required<string>();
  protected readonly order = signal<Order | null>(null);

  constructor() {
    effect(() => this.load(Number(this.id())));
  }

  private load(id: number): void {
    this.service.get(id).subscribe({
      next: (order) => this.order.set(order),
      error: () => {
        this.toastr.error('There is no such order among yours.');
        this.router.navigateByUrl('/orders');
      },
    });
  }
}
