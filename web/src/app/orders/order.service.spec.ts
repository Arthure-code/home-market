import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Checkout, Order, Sale } from './order';
import { ORDERS_URL, OrderService } from './order.service';

const checkout: Checkout = {
  fullName: 'Eve Martin',
  street: '12 Rue Nord',
  city: 'Montreal',
  province: 'QC',
  postalCode: 'H2X 1Y4',
  country: 'Canada',
  cardNumber: '4242 4242 4242 4242',
  cardHolderName: 'Eve Martin',
  expiry: '12/29',
  securityCode: '123',
};

const placed: Order = {
  id: 5,
  placedAt: '2026-09-22T10:00:00Z',
  buyer: 'eve',
  fullName: 'Eve Martin',
  street: '12 Rue Nord',
  city: 'Montreal',
  province: 'QC',
  postalCode: 'H2X 1Y4',
  country: 'Canada',
  cardBrand: 'Visa',
  cardLast4: '4242',
  subtotal: 149,
  gst: 7.45,
  qst: 14.86,
  total: 171.31,
  itemCount: 3,
  lines: [],
};

describe('OrderService', () => {
  let service: OrderService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(OrderService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('places an order by posting the checkout as typed', () => {
    let answer: Order | undefined;
    service.place(checkout).subscribe((order) => (answer = order));

    const request = http.expectOne(ORDERS_URL);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(checkout);
    request.flush(placed);

    expect(answer).toEqual(placed);
  });

  it('reads my orders, one order, and my sales from their own addresses', () => {
    let mine: Order[] = [];
    let one: Order | undefined;
    let sales: Sale[] = [];
    service.mine().subscribe((orders) => (mine = orders));
    service.get(5).subscribe((order) => (one = order));
    service.sales().subscribe((lines) => (sales = lines));

    http.expectOne({ method: 'GET', url: ORDERS_URL }).flush([placed]);
    http.expectOne({ method: 'GET', url: `${ORDERS_URL}/5` }).flush(placed);
    http.expectOne({ method: 'GET', url: `${ORDERS_URL}/sales` }).flush([]);

    expect(mine).toEqual([placed]);
    expect(one).toEqual(placed);
    expect(sales).toEqual([]);
  });
});
