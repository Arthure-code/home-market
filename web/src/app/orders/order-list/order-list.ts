import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../helpers/api-message';
import { Order } from '../../models/order';
import { OrderService } from '../../services/order.service';

// Everything I bought, newest first.
@Component({
  selector: 'app-order-list',
  imports: [CurrencyPipe, DatePipe, RouterLink],
  templateUrl: './order-list.html',
})
export class OrderList implements OnInit {
  orders: Order[] = [];
  loading = true;

  constructor(
    private readonly service: OrderService,
    private readonly toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.service.mine().subscribe({
      next: (orders) => {
        this.orders = orders;
        this.loading = false;
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.loading = false;
      },
    });
  }

  summary(order: Order): string {
    const first = order.lines[0]?.title ?? '';
    const more = order.lines.length - 1;
    return more > 0 ? `${first} and ${more} more` : first;
  }
}
