import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Checkout, Order, Sale } from './order';

export const ORDERS_URL = 'http://localhost:5130/api/orders';

// Orders of the signed-in member: placing one from the cart, reading
// mine back, and the lines others bought from me.
@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);

  place(checkout: Checkout): Observable<Order> {
    return this.http.post<Order>(ORDERS_URL, checkout);
  }

  mine(): Observable<Order[]> {
    return this.http.get<Order[]>(ORDERS_URL);
  }

  get(id: number): Observable<Order> {
    return this.http.get<Order>(`${ORDERS_URL}/${id}`);
  }

  sales(): Observable<Sale[]> {
    return this.http.get<Sale[]>(`${ORDERS_URL}/sales`);
  }
}
