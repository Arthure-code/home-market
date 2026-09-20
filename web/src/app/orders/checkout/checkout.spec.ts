import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, of, throwError } from 'rxjs';
import { Checkout } from './checkout';
import { Cart, EMPTY_CART } from '../../cart/cart';
import { CartService } from '../../cart/cart.service';
import { Checkout as CheckoutDraft, Order } from '../order';
import { OrderService } from '../order.service';

const oneFan: Cart = {
  lines: [
    {
      productId: 4,
      title: 'Desk fan',
      photoUrl: '',
      seller: 'nadia',
      unitPrice: 59,
      quantity: 1,
      stock: 2,
      lineTotal: 59,
    },
  ],
  itemCount: 1,
  subtotal: 59,
  gst: 2.95,
  qst: 5.89,
  total: 67.84,
};

describe('Checkout', () => {
  let fixture: ComponentFixture<Checkout>;
  let cart: ReturnType<typeof signal<Cart>>;
  let answer: Observable<Order>;
  let placed: CheckoutDraft[];
  let cleared: number;
  let toastr: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

  const root = () => fixture.nativeElement as HTMLElement;
  const type = async (id: string, value: string) => {
    const el = root().querySelector<HTMLInputElement>(`#${id}`)!;
    el.value = value;
    el.dispatchEvent(new Event('input'));
    await fixture.whenStable();
  };
  const fill = async () => {
    await type('fullName', 'Nadia Roy');
    await type('street', '12 rue des Érables');
    await type('city', 'Québec');
    await type('postalCode', 'G1R 2B3');
    await type('cardNumber', '4242 4242 4242 4242');
    await type('cardHolderName', 'Nadia Roy');
    await type('expiry', '12/39');
    await type('securityCode', '123');
  };
  const submit = async () => {
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
  };
  const show = async () => {
    fixture = TestBed.createComponent(Checkout);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  };

  beforeEach(async () => {
    cart = signal<Cart>(oneFan);
    answer = of({ id: 7 } as Order);
    placed = [];
    cleared = 0;
    toastr = { success: vi.fn(), error: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [Checkout],
      providers: [
        provideRouter([]),
        { provide: CartService, useValue: { cart, clear: () => cleared++ } },
        {
          provide: OrderService,
          useValue: {
            place: (draft: CheckoutDraft) => {
              placed.push({ ...draft });
              return answer;
            },
          },
        },
        { provide: ToastrService, useValue: toastr },
      ],
    }).compileComponents();
  });

  it('shows the summary and refuses to order with a field empty', async () => {
    await show();
    expect(root().querySelector('[data-testid="total"]')?.textContent?.trim()).toBe('$67.84');

    await type('fullName', 'Nadia Roy');
    await submit();

    expect(placed).toEqual([]);
    expect(toastr.error).toHaveBeenCalledWith('Please fill in every field');
  });

  it('places the order, forgets the card, empties the cart and opens the order', async () => {
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    await show();
    await fill();
    await submit();

    expect(placed).toEqual([
      {
        fullName: 'Nadia Roy',
        street: '12 rue des Érables',
        city: 'Québec',
        province: 'QC',
        postalCode: 'G1R 2B3',
        country: 'Canada',
        cardNumber: '4242 4242 4242 4242',
        cardHolderName: 'Nadia Roy',
        expiry: '12/39',
        securityCode: '123',
      },
    ]);
    expect(fixture.componentInstance.draft.cardNumber).toBe('');
    expect(fixture.componentInstance.draft.cardHolderName).toBe('');
    expect(fixture.componentInstance.draft.securityCode).toBe('');
    expect(cleared).toBe(1);
    expect(toastr.success).toHaveBeenCalledWith('Order placed. Thank you!');
    expect(navigate).toHaveBeenCalledWith(['/orders', 7]);
  });

  it('shows why a card was refused and keeps the cart', async () => {
    answer = throwError(
      () => new HttpErrorResponse({ status: 402, error: { message: 'Your card was declined.' } }),
    );
    await show();
    await fill();
    await submit();

    expect(toastr.error).toHaveBeenCalledWith('Your card was declined.');
    expect(cleared).toBe(0);
    expect(fixture.componentInstance.placing()).toBe(false);
  });

  it('says so when the cart is empty', async () => {
    cart.set(EMPTY_CART);
    await show();

    expect(root().querySelector('[data-testid="empty"]')).not.toBeNull();
    expect(root().querySelector('form')).toBeNull();
  });
});
