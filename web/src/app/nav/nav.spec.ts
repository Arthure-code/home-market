import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';
import { Nav } from './nav';
import { SessionService } from '../services/session.service';
import { EMPTY_CART } from '../models/cart';
import { CartService } from '../services/cart.service';
import { ProductService } from '../services/product.service';

describe('Nav', () => {
  let fixture: ComponentFixture<Nav>;
  let signedIn: ReturnType<typeof signal<boolean>>;
  let cartCalls: string[];
  let count: ReturnType<typeof signal<number>>;

  const root = () => fixture.nativeElement as HTMLElement;
  const show = async () => {
    fixture = TestBed.createComponent(Nav);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  };

  beforeEach(async () => {
    signedIn = signal(false);
    cartCalls = [];
    count = signal(0);
    await TestBed.configureTestingModule({
      imports: [Nav],
      providers: [
        provideRouter([]),
        {
          provide: SessionService,
          useValue: { signedIn, userName: () => 'nadia', signOut: () => signedIn.set(false) },
        },
        {
          provide: CartService,
          useValue: {
            count,
            load: () => {
              cartCalls.push('load');
              count.set(3);
              return of(EMPTY_CART);
            },
            clear: () => {
              cartCalls.push('clear');
              count.set(0);
            },
          },
        },
        {
          provide: ProductService,
          useValue: {
            categories: () =>
              of([
                { name: 'Electronics', count: 6 },
                { name: 'Kitchen', count: 5 },
              ]),
          },
        },
        { provide: ToastrService, useValue: { error: vi.fn() } },
      ],
    }).compileComponents();
  });

  it('gives a visitor the search box, a Sign in link, the cart and the categories', async () => {
    await show();

    expect(root().querySelector('[data-testid="search"]')).not.toBeNull();
    expect(root().querySelector('[data-testid="sign-in-link"]')?.getAttribute('href')).toBe(
      '/sign-in',
    );
    expect(root().querySelector('[data-testid="account"]')).toBeNull();
    expect(root().querySelector('[data-testid="cart-link"]')?.getAttribute('href')).toBe('/cart');
    expect(root().querySelector('[data-testid="cart-count"]')).toBeNull();
    const links = Array.from(root().querySelectorAll('.categories a')).map((a) =>
      a.textContent?.trim(),
    );
    expect(links).toEqual(['All', 'Electronics', 'Kitchen']);
    expect(root().querySelector('.categories a[href="/products?category=Kitchen"]')).not.toBeNull();
    expect(cartCalls).toEqual(['clear']);
  });

  it('gives a member the account menu, the cart with its count, and Sell', async () => {
    signedIn.set(true);
    await show();

    expect(root().querySelector('[data-testid="sign-in-link"]')).toBeNull();
    expect(root().querySelector('[data-testid="account"]')?.textContent).toContain('nadia');
    expect(cartCalls).toEqual(['load']);
    expect(root().querySelector('[data-testid="cart-link"]')?.classList.contains('cart-pill')).toBe(
      true,
    );
    expect(root().querySelector('[data-testid="cart-count"]')?.textContent?.trim()).toBe('3');
    expect(root().querySelector('.categories a[href="/products/new"]')).not.toBeNull();

    root().querySelector<HTMLButtonElement>('[data-testid="sign-out"]')?.click();
    await fixture.whenStable();
    expect(cartCalls).toEqual(['load', 'clear']);
    expect(root().querySelector('[data-testid="sign-in-link"]')).not.toBeNull();
  });

  it('shows the cart as a plain icon while it is empty', async () => {
    signedIn.set(true);
    count.set(0);
    vi.spyOn(TestBed.inject(CartService), 'load').mockReturnValue(of(EMPTY_CART));
    await show();

    expect(root().querySelector('[data-testid="cart-link"]')?.classList.contains('cart-pill')).toBe(
      false,
    );
    expect(root().querySelector('[data-testid="cart-count"]')).toBeNull();
  });

  it('searches by going to the catalogue with the text in the address', async () => {
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    await show();

    const input = root().querySelector<HTMLInputElement>('#nav-search')!;
    input.value = '  kettle ';
    input.dispatchEvent(new Event('input'));
    await fixture.whenStable();
    root().querySelector('[data-testid="search"]')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    expect(navigate).toHaveBeenCalledWith(['/products'], { queryParams: { q: 'kettle' } });
  });
});
