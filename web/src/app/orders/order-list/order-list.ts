import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../auth/api-message';
import { Order } from '../order';
import { OrderService } from '../order.service';

// Everything I bought, newest first.
@Component({
  selector: 'app-order-list',
  imports: [CurrencyPipe, DatePipe, RouterLink],
  templateUrl: './order-list.html',
})
export class OrderList implements OnInit {
  private readonly service = inject(OrderService);
  private readonly toastr = inject(ToastrService);

  readonly orders = signal<Order[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.service.mine().subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.loading.set(false);
      },
    });
  }

  summary(order: Order): string {
    const first = order.lines[0]?.title ?? '';
    const more = order.lines.length - 1;
    return more > 0 ? `${first} and ${more} more` : first;
  }
}
