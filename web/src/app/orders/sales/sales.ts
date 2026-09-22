import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../helpers/api-message';
import { Sale } from '../../models/order';
import { OrderService } from '../../services/order.service';

// What other members bought from me, newest first, with where to send
// it. The buyer's card is not part of it.
@Component({
  selector: 'app-sales',
  imports: [CurrencyPipe, DatePipe, RouterLink],
  templateUrl: './sales.html',
})
export class Sales implements OnInit {
  sales: Sale[] = [];
  loading = true;

  constructor(
    private readonly service: OrderService,
    private readonly toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.service.sales().subscribe({
      next: (sales) => {
        this.sales = sales;
        this.loading = false;
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.loading = false;
      },
    });
  }

  get revenue(): number {
    return this.sales.reduce((sum, s) => sum + s.lineTotal, 0);
  }
}
