import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
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
export class OrderDetail implements OnInit {
  @Input({ required: true }) id!: string;
  order: Order | null = null;

  constructor(
    private readonly service: OrderService,
    private readonly router: Router,
    private readonly toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.service.get(Number(this.id)).subscribe({
      next: (order) => (this.order = order),
      error: () => {
        this.toastr.error('There is no such order among yours.');
        this.router.navigateByUrl('/orders');
      },
    });
  }
}
