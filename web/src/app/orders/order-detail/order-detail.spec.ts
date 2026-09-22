import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, of, throwError } from 'rxjs';
import { OrderDetail } from './order-detail';
import { Order } from '../../models/order';
import { OrderService } from '../../services/order.service';

const order: Order = {
  id: 7,
  placedAt: '2026-09-20T14:00:00Z',
  buyer: 'eve',
  fullName: 'Nadia Roy',
  street: '12 rue des Érables',
  city: 'Québec',
  province: 'QC',
  postalCode: 'G1R 2B3',
  country: 'Canada',
  cardBrand: 'Visa',
  cardLast4: '4242',
  subtotal: 92,
  gst: 4.6,
  qst: 9.18,
  total: 105.78,
  itemCount: 3,
  lines: [
    { productId: 30, title: 'Fan', seller: 'dan', unitPrice: 40, quantity: 2, lineTotal: 80 },
    {
      productId: null,
      title: 'Mug',
      seller: 'homemarket',
      unitPrice: 12,
      quantity: 1,
      lineTotal: 12,
    },
  ],
};

describe('OrderDetail', () => {
  let fixture: ComponentFixture<OrderDetail>;
  let answer: Observable<Order>;
  let toastr: { error: ReturnType<typeof vi.fn> };

  const root = () => fixture.nativeElement as HTMLElement;
  const text = (testId: string) =>
    root().querySelector(`[data-testid="${testId}"]`)?.textContent?.replace(/\s+/g, ' ').trim();
  const open = async (id: string) => {
    fixture = TestBed.createComponent(OrderDetail);
    fixture.componentRef.setInput('id', id);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  };

  beforeEach(async () => {
    answer = of(order);
    toastr = { error: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [OrderDetail],
      providers: [
        provideRouter([]),
        { provide: OrderService, useValue: { get: () => answer } },
        { provide: ToastrService, useValue: toastr },
      ],
    }).compileComponents();
  });

  it('shows the lines, the totals, the address and the card by its last digits', async () => {
    await open('7');

    expect(root().querySelector('h1')?.textContent?.trim()).toBe('Order #7');
    expect(root().querySelectorAll('tbody tr').length).toBe(2);
    expect(root().querySelector('tbody a')?.getAttribute('href')).toBe('/products/30');
    expect(root().querySelectorAll('tbody a').length).toBe(1);
    expect(text('total')).toBe('$105.78');
    expect(text('card')).toBe('Paid with Visa ending in 4242');
    expect(text('address')).toBe('Nadia Roy 12 rue des Érables Québec, QC G1R 2B3 Canada');
  });

  it('goes back to the list when the order is not mine', async () => {
    answer = throwError(() => new HttpErrorResponse({ status: 404 }));
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    await open('99');

    expect(toastr.error).toHaveBeenCalledWith('There is no such order among yours.');
    expect(navigate).toHaveBeenCalledWith('/orders');
  });
});
