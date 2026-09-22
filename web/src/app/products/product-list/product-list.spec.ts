import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, of } from 'rxjs';
import { ProductList } from './product-list';
import { SignInPrompt } from '../../services/sign-in-prompt';
import { Product } from '../../models/product';
import { ProductService } from '../../services/product.service';

const product = (id: number, liked = false): Product => ({
  id,
  title: `Product ${id}`,
  brand: '',
  maker: '',
  description: '',
  price: 10 * id,
  category: 'Kitchen',
  stock: 12,
  photoUrl: '',
  seller: 'homemarket',
  createdAt: '',
  updatedAt: '',
  likes: liked ? 1 : 0,
  liked,
  mine: false,
});

class ServiceStub {
  calls: string[] = [];
  list(): Observable<Product[]> {
    this.calls.push('list');
    return of([product(1), product(2, true), product(3)]);
  }
  mine(): Observable<Product[]> {
    this.calls.push('mine');
    return of([product(9)]);
  }
  liked(): Observable<Product[]> {
    this.calls.push('liked');
    return of([]);
  }
  setLike(id: number, liked: boolean): Observable<void> {
    this.calls.push(`like ${id} ${liked}`);
    return of(undefined);
  }
}

describe('ProductList', () => {
  let fixture: ComponentFixture<ProductList>;
  let stub: ServiceStub;
  let toastr: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };
  let signedIn: boolean;
  let prompted: number;

  const root = () => fixture.nativeElement as HTMLElement;
  const cards = () => root().querySelectorAll('app-product-card');
  const show = async (source?: 'all' | 'mine' | 'liked') => {
    fixture = TestBed.createComponent(ProductList);
    if (source) fixture.componentRef.setInput('source', source);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  };

  beforeEach(async () => {
    stub = new ServiceStub();
    toastr = { success: vi.fn(), error: vi.fn() };
    signedIn = true;
    prompted = 0;
    await TestBed.configureTestingModule({
      imports: [ProductList],
      providers: [
        provideRouter([]),
        { provide: ProductService, useValue: stub },
        {
          provide: SignInPrompt,
          useValue: {
            ensure: () => {
              if (!signedIn) prompted++;
              return signedIn;
            },
          },
        },
        { provide: ToastrService, useValue: toastr },
      ],
    }).compileComponents();
  });

  it('shows the whole catalogue by default', async () => {
    await show();

    expect(stub.calls).toEqual(['list']);
    expect(cards().length).toBe(3);
  });

  it('shows my listings or my likes when asked', async () => {
    await show('mine');
    expect(stub.calls).toEqual(['mine']);
    expect(cards().length).toBe(1);

    await show('liked');
    expect(stub.calls).toEqual(['mine', 'liked']);
    expect(root().querySelector('[data-testid="empty"]')).not.toBeNull();
  });

  it('likes and unlikes through the service and updates the count in place', async () => {
    await show();

    cards()[0].querySelector<HTMLButtonElement>('[data-testid="like"]')?.click();
    await fixture.whenStable();
    expect(stub.calls).toEqual(['list', 'like 1 true']);
    expect(cards()[0].querySelector('[data-testid="like"]')?.getAttribute('aria-pressed')).toBe(
      'true',
    );
    expect(cards()[0].querySelector('[data-testid="like"]')?.textContent?.trim()).toBe('1');
    expect(toastr.success).toHaveBeenCalledWith('Added to your likes');

    cards()[1].querySelector<HTMLButtonElement>('[data-testid="like"]')?.click();
    await fixture.whenStable();
    expect(stub.calls).toEqual(['list', 'like 1 true', 'like 2 false']);
    expect(cards()[1].querySelector('[data-testid="like"]')?.getAttribute('aria-pressed')).toBe(
      'false',
    );
  });

  it('sends a visitor who likes to sign in, without calling the service', async () => {
    signedIn = false;
    await show();

    cards()[0].querySelector<HTMLButtonElement>('[data-testid="like"]')?.click();
    await fixture.whenStable();

    expect(prompted).toBe(1);
    expect(stub.calls).toEqual(['list']);
  });
});
