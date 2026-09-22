import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { Cart, EMPTY_CART } from '../models/cart';

export const CART_URL = 'http://localhost:5130/api/cart';

// The cart of the signed-in member, held here so the badge in the bar
// and the cart page read the same thing. The API adds up; every call
// answers with the whole cart and replaces what is held here.
@Injectable({ providedIn: 'root' })
export class CartService {
  cart: Cart = EMPTY_CART;

  constructor(private readonly http: HttpClient) {}

  get count(): number {
    return this.cart.itemCount;
  }

  load(): Observable<Cart> {
    return this.http.get<Cart>(CART_URL).pipe(tap((cart) => (this.cart = cart)));
  }

  add(productId: number, quantity = 1): Observable<Cart> {
    return this.http
      .post<Cart>(`${CART_URL}/lines`, { productId, quantity })
      .pipe(tap((cart) => (this.cart = cart)));
  }

  setQuantity(productId: number, quantity: number): Observable<Cart> {
    return this.http
      .put<Cart>(`${CART_URL}/lines/${productId}`, { quantity })
      .pipe(tap((cart) => (this.cart = cart)));
  }

  remove(productId: number): Observable<Cart> {
    return this.http
      .delete<Cart>(`${CART_URL}/lines/${productId}`)
      .pipe(tap((cart) => (this.cart = cart)));
  }

  // After an order, or when signing out: nothing left to show.
  clear(): void {
    this.cart = EMPTY_CART;
  }
}
