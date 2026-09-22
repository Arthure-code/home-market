import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
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
  private readonly service = inject(OrderService);
  private readonly toastr = inject(ToastrService);

  protected readonly sales = signal<Sale[]>([]);
  protected readonly loading = signal(true);
  protected readonly revenue = computed(() =>
    this.sales().reduce((sum, s) => sum + s.lineTotal, 0),
  );

  ngOnInit(): void {
    this.service.sales().subscribe({
      next: (sales) => {
        this.sales.set(sales);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.loading.set(false);
      },
    });
  }
}
