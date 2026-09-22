import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Cart } from '../models/cart';
import { CART_URL, CartService } from './cart.service';

const twoMugs: Cart = {
  lines: [
    {
      productId: 23,
      title: 'Ceramic mug',
      photoUrl: '',
      seller: 'homemarket',
      unitPrice: 12,
      quantity: 2,
      stock: 60,
      lineTotal: 24,
    },
  ],
  itemCount: 2,
  subtotal: 24,
  gst: 1.2,
  qst: 2.39,
  total: 27.59,
};

describe('CartService', () => {
  let service: CartService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(CartService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('starts empty and holds what the API answers', () => {
    expect(service.count()).toBe(0);

    service.load().subscribe();
    http.expectOne(CART_URL).flush(twoMugs);

    expect(service.cart()).toEqual(twoMugs);
    expect(service.count()).toBe(2);
  });

  it('adds, changes and removes lines, each answer replacing the cart', () => {
    service.add(23, 2).subscribe();
    const added = http.expectOne(`${CART_URL}/lines`);
    expect(added.request.method).toBe('POST');
    expect(added.request.body).toEqual({ productId: 23, quantity: 2 });
    added.flush(twoMugs);
    expect(service.count()).toBe(2);

    service.setQuantity(23, 5).subscribe();
    const changed = http.expectOne(`${CART_URL}/lines/23`);
    expect(changed.request.method).toBe('PUT');
    expect(changed.request.body).toEqual({ quantity: 5 });
    changed.flush({ ...twoMugs, itemCount: 5 });
    expect(service.count()).toBe(5);

    service.remove(23).subscribe();
    const removed = http.expectOne(`${CART_URL}/lines/23`);
    expect(removed.request.method).toBe('DELETE');
    removed.flush({ ...twoMugs, lines: [], itemCount: 0 });
    expect(service.count()).toBe(0);
  });

  it('forgets everything when cleared', () => {
    service.load().subscribe();
    http.expectOne(CART_URL).flush(twoMugs);

    service.clear();

    expect(service.cart().lines).toEqual([]);
    expect(service.count()).toBe(0);
  });
});
