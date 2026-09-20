import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, of, throwError } from 'rxjs';
import { ProductDetail } from './product-detail';
import { SessionService } from '../../auth/session.service';
import { Product } from '../product';
import { ProductService } from '../product.service';

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
        { provide: SessionService, useValue: { signedIn: () => signedIn } },
        { provide: ToastrService, useValue: toastr },
      ],
    }).compileComponents();
  });

  it('shows the product named in the address, with the likes and a hint for a visitor', async () => {
    await open('4');

    expect(calls).toEqual(['get 4']);
    expect(root().querySelector('h1')?.textContent?.trim()).toBe('Desk fan, chrome');
    expect(text('price')).toBe('$59');
    expect(text('likes')).toBe('3 likes, sign in to add yours');
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
    await open('4');
    expect(root().querySelector('[data-testid="message"]')?.getAttribute('href')).toBe(
      '/messages/new/nadia?subject=About%20Desk%20fan,%20chrome',
    );

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

  it('says how many are left, and whose listing it is', async () => {
    answer = of({ ...fan, stock: 2 });
    await open('4');
    expect(text('stock')).toBe('Only 2 left in stock');

    signedIn = true;
    answer = of({ ...fan, mine: true });
    await open('4');
    expect(root().querySelector('[data-testid="own"]')).not.toBeNull();

    answer = of({ ...fan, stock: 0 });
    await open('4');
    expect(text('stock')).toBe('Out of stock');
  });

  it('says when there is no such product', async () => {
    answer = throwError(() => new HttpErrorResponse({ status: 404 }));
    await open('99');

    expect(text('missing')).toBe('There is no product with that number.');
  });
});
