import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, of, throwError } from 'rxjs';
import { ProductDetail } from './product-detail';
import { SignInPrompt } from '../../services/sign-in-prompt';
import { EMPTY_CART } from '../../models/cart';
import { CartService } from '../../services/cart.service';
import { Product } from '../../models/product';
import { ProductService } from '../../services/product.service';

const fan: Product = {
  id: 4,
  title: 'Desk fan, chrome',
  brand: 'Breeza',
  maker: 'Breeza Appliances',
  description: 'Three speeds.',
  price: 59,
  category: 'Kitchen',
  stock: 12,
  photoUrl: 'https://images.unsplash.com/photo-4',
  seller: 'homemarket',
  createdAt: '2026-09-01T00:00:00Z',
  updatedAt: '2026-09-01T00:00:00Z',
  likes: 3,
  liked: false,
  mine: false,
};

describe('ProductDetail', () => {
  let fixture: ComponentFixture<ProductDetail>;
  let answer: Observable<Product>;
  let signedIn: boolean;
  let calls: string[];
  let toastr: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

  const root = () => fixture.nativeElement as HTMLElement;
  const text = (testId: string) =>
    root().querySelector(`[data-testid="${testId}"]`)?.textContent?.replace(/\s+/g, ' ').trim();
  const open = async (id: string) => {
    fixture = TestBed.createComponent(ProductDetail);
    fixture.componentRef.setInput('id', id);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  };

  beforeEach(async () => {
    answer = of(fan);
    signedIn = false;
    calls = [];
    toastr = { success: vi.fn(), error: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [ProductDetail],
      providers: [
        provideRouter([]),
        {
          provide: ProductService,
          useValue: {
            get: (id: number) => {
              calls.push(`get ${id}`);
              return answer;
            },
            setLike: (id: number, liked: boolean) => {
              calls.push(`like ${id} ${liked}`);
              return of(undefined);
            },
            delete: (id: number) => {
              calls.push(`delete ${id}`);
              return of(undefined);
            },
          },
        },
        {
          provide: SignInPrompt,
          useValue: {
            ensure: () => {
              if (!signedIn) calls.push('prompt');
              return signedIn;
            },
          },
        },
        {
          provide: CartService,
          useValue: {
            add: (id: number, quantity: number) => {
              calls.push(`cart ${id} ${quantity}`);
              return of(EMPTY_CART);
            },
          },
        },
        { provide: ToastrService, useValue: toastr },
      ],
    }).compileComponents();
  });

  it('shows the product named in the address, its price and its likes', async () => {
    await open('4');

    expect(calls).toEqual(['get 4']);
    expect(root().querySelector('h1')?.textContent?.trim()).toBe('Desk fan, chrome');
    expect(text('price')).toBe('$59');
    expect(text('like')).toBe('3');
    expect(root().querySelector('[data-testid="back"]')?.getAttribute('href')).toBe('/products');
  });

  it('lets a member like and unlike, keeping the count', async () => {
    signedIn = true;
    await open('4');

    root().querySelector<HTMLButtonElement>('[data-testid="like"]')?.click();
    await fixture.whenStable();
    expect(calls).toEqual(['get 4', 'like 4 true']);
    expect(text('like')).toBe('4');
    expect(root().querySelector('[data-testid="like"]')?.getAttribute('aria-pressed')).toBe('true');

    root().querySelector<HTMLButtonElement>('[data-testid="like"]')?.click();
    await fixture.whenStable();
    expect(text('like')).toBe('3');
    expect(root().querySelector('[data-testid="like"]')?.getAttribute('aria-pressed')).toBe(
      'false',
    );
  });

  it('lets a member write to the seller, unless the seller is the store', async () => {
    signedIn = true;
    answer = of({ ...fan, seller: 'nadia' });
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    await open('4');
    root().querySelector<HTMLButtonElement>('[data-testid="message"]')?.click();
    expect(navigate).toHaveBeenCalledWith(['/messages/new', 'nadia'], {
      queryParams: { subject: 'About Desk fan, chrome' },
    });

    answer = of(fan);
    await open('4');
    expect(root().querySelector('[data-testid="message"]')).toBeNull();
  });

  it('gives the seller edit and remove, and removing goes to my products', async () => {
    signedIn = true;
    answer = of({ ...fan, mine: true });
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    await open('4');

    expect(root().querySelector('[data-testid="like"]')).toBeNull();
    expect(root().querySelector('[data-testid="edit"]')).not.toBeNull();
    root().querySelector<HTMLButtonElement>('[data-testid="remove"]')?.click();
    await fixture.whenStable();

    expect(calls).toEqual(['get 4', 'delete 4']);
    expect(toastr.success).toHaveBeenCalledWith('Listing removed');
    expect(navigate).toHaveBeenCalledWith('/my-products');
  });

  it('lets a member pick a quantity up to the stock and add to the cart', async () => {
    signedIn = true;
    answer = of({ ...fan, stock: 2 });
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    await open('4');

    expect(text('stock')).toBe('Only 2 left in stock');
    const select = root().querySelector<HTMLSelectElement>('[data-testid="quantity"]')!;
    expect(Array.from(select.options).map((o) => o.value)).toEqual(['1', '2']);
    select.value = '2';
    select.dispatchEvent(new Event('change'));
    await fixture.whenStable();
    root().querySelector<HTMLButtonElement>('[data-testid="add-to-cart"]')?.click();
    await fixture.whenStable();

    expect(calls).toEqual(['get 4', 'cart 4 2']);
    expect(toastr.success).toHaveBeenCalledWith('Added to your cart');
    expect(navigate).toHaveBeenCalledWith('/cart');
  });

  it('sends a visitor who adds to the cart to sign in, without calling the service', async () => {
    signedIn = false;
    await open('4');

    root().querySelector<HTMLButtonElement>('[data-testid="add-to-cart"]')?.click();
    await fixture.whenStable();

    expect(calls).toEqual(['get 4', 'prompt']);
  });

  it('offers no cart to the seller, or when sold out', async () => {
    signedIn = true;
    answer = of({ ...fan, mine: true });
    await open('4');
    expect(root().querySelector('[data-testid="own"]')).not.toBeNull();
    expect(root().querySelector('[data-testid="add-to-cart"]')).toBeNull();

    answer = of({ ...fan, stock: 0 });
    await open('4');
    expect(text('stock')).toBe('Out of stock');
    expect(root().querySelector('[data-testid="add-to-cart"]')).toBeNull();
  });

  it('says when there is no such product', async () => {
    answer = throwError(() => new HttpErrorResponse({ status: 404 }));
    await open('99');

    expect(text('missing')).toBe('There is no product with that number.');
  });
});
