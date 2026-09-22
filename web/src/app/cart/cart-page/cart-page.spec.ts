import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';
import { CartPage } from './cart-page';
import { Cart, EMPTY_CART } from '../../models/cart';
import { CartService } from '../../services/cart.service';

const filled: Cart = {
  lines: [
    {
      productId: 23,
      title: 'Ceramic mug',
      photoUrl: 'https://images.unsplash.com/photo-23',
      seller: 'homemarket',
      unitPrice: 12,
      quantity: 2,
      stock: 60,
      lineTotal: 24,
    },
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
  itemCount: 3,
  subtotal: 83,
  gst: 4.15,
  qst: 8.28,
  total: 95.43,
};

describe('CartPage', () => {
  let fixture: ComponentFixture<CartPage>;
  let cart: ReturnType<typeof signal<Cart>>;
  let calls: string[];
  let toastr: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

  const root = () => fixture.nativeElement as HTMLElement;
  const text = (testId: string) =>
    root().querySelector(`[data-testid="${testId}"]`)?.textContent?.replace(/\s+/g, ' ').trim();
  const show = async () => {
    fixture = TestBed.createComponent(CartPage);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  };

  beforeEach(async () => {
    cart = signal<Cart>(filled);
    calls = [];
    toastr = { success: vi.fn(), error: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [CartPage],
      providers: [
        provideRouter([]),
        {
          provide: CartService,
          useValue: {
            cart,
            setQuantity: (id: number, quantity: number) => {
              calls.push(`set ${id} ${quantity}`);
              return of(filled);
            },
            remove: (id: number) => {
              calls.push(`remove ${id}`);
              cart.set(EMPTY_CART);
              return of(EMPTY_CART);
            },
          },
        },
        { provide: ToastrService, useValue: toastr },
      ],
    }).compileComponents();
  });

  it('lists the lines with their totals and the taxes the API added up', async () => {
    await show();

    expect(root().querySelectorAll('li[data-testid^="line-"]').length).toBe(2);
    expect(text('line-23')).toContain('Ceramic mug');
    expect(text('line-23')).toContain('$24.00');
    expect(text('line-23')).toContain('$12.00 each');
    expect(text('line-4')).toContain('Only 2 left');
    expect(text('subtotal')).toBe('$83.00');
    expect(text('total')).toBe('$95.43');
    expect(root().querySelector('[data-testid="checkout"]')?.getAttribute('href')).toBe(
      '/checkout',
    );
  });

  it('offers quantities up to the stock, ten at most', async () => {
    await show();

    const options = (testId: string) =>
      Array.from(
        root().querySelectorAll(`[data-testid="${testId}"] [data-testid="quantity"] option`),
      ).map((o) => o.textContent?.trim());
    expect(options('line-23').length).toBe(10);
    expect(options('line-4')).toEqual(['1', '2']);
  });

  it('changes a quantity and removes a line through the service', async () => {
    await show();

    const select = root().querySelector<HTMLSelectElement>(
      '[data-testid="line-23"] [data-testid="quantity"]',
    )!;
    select.value = '5';
    select.dispatchEvent(new Event('change'));
    await fixture.whenStable();
    expect(calls).toEqual(['set 23 5']);

    root()
      .querySelector<HTMLButtonElement>('[data-testid="line-4"] [data-testid="remove"]')
      ?.click();
    await fixture.whenStable();
    expect(calls).toEqual(['set 23 5', 'remove 4']);
    expect(toastr.success).toHaveBeenCalledWith('Desk fan removed from your cart');
    expect(text('empty')).toBe('Your cart is empty.');
  });
});
