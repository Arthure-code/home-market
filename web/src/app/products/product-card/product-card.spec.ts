import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ProductCard } from './product-card';
import { SessionService } from '../../auth/session.service';
import { Product } from '../product';

const mug: Product = {
  id: 23,
  title: 'Ceramic mug, white, 350 ml',
  brand: 'Kettl',
  maker: 'Kettl Home',
  description: 'Thick walls.',
  price: 12,
  category: 'Kitchen',
  stock: 12,
  photoUrl: 'https://images.unsplash.com/photo-1',
  seller: 'homemarket',
  createdAt: '2026-09-01T00:00:00Z',
  updatedAt: '2026-09-01T00:00:00Z',
  likes: 2,
  liked: false,
  mine: false,
};

describe('ProductCard', () => {
  let fixture: ComponentFixture<ProductCard>;
  let signedIn = false;

  const root = () => fixture.nativeElement as HTMLElement;
  const show = async (product: Product) => {
    fixture = TestBed.createComponent(ProductCard);
    fixture.componentRef.setInput('product', product);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductCard],
      providers: [
        provideRouter([]),
        { provide: SessionService, useValue: { signedIn: () => signedIn } },
      ],
    }).compileComponents();
  });

  it('shows the photo at card size, the title and the price', async () => {
    await show(mug);

    const img = root().querySelector('img');
    expect(img?.getAttribute('src')).toBe(
      'https://images.unsplash.com/photo-1?auto=format&fit=crop&w=400&h=300&q=70',
    );
    expect(img?.getAttribute('alt')).toBe(mug.title);
    expect(root().querySelector('h2')?.textContent?.trim()).toBe(mug.title);
    expect(root().textContent).toContain('$12');
  });

  it('opens the page from the photo and gives a visitor the quick view only', async () => {
    signedIn = false;
    await show(mug);
    const viewed: Product[] = [];
    fixture.componentInstance.quickView.subscribe((p) => viewed.push(p));

    expect(root().querySelector('[data-testid="open"]')?.getAttribute('href')).toBe('/products/23');
    expect(root().querySelectorAll('.list-inline-item').length).toBe(1);
    expect(root().querySelector('[data-testid="like"]')).toBeNull();
    root().querySelector<HTMLButtonElement>('[data-testid="quick-view"]')?.click();
    expect(viewed).toEqual([mug]);
  });

  it('gives a member a like button that shows the count and emits the product', async () => {
    signedIn = true;
    await show(mug);
    const emitted: Product[] = [];
    fixture.componentInstance.toggleLike.subscribe((p) => emitted.push(p));

    const like = root().querySelector<HTMLButtonElement>('[data-testid="like"]');
    expect(like?.textContent?.trim()).toBe('2');
    expect(like?.getAttribute('aria-pressed')).toBe('false');
    like?.click();
    expect(emitted).toEqual([mug]);
  });

  it('gives a member a cart button that emits the product, greyed when sold out', async () => {
    signedIn = true;
    await show(mug);
    const emitted: Product[] = [];
    fixture.componentInstance.addToCart.subscribe((p) => emitted.push(p));

    expect(root().querySelectorAll('.list-inline-item').length).toBe(3);
    expect(root().querySelector('[data-testid="stock"]')?.textContent?.trim()).toBe('In stock');
    root().querySelector<HTMLButtonElement>('[data-testid="add-to-cart"]')?.click();
    expect(emitted).toEqual([mug]);

    await show({ ...mug, stock: 0 });
    expect(root().querySelector('[data-testid="stock"]')?.textContent?.trim()).toBe('Out of stock');
    expect(root().querySelector<HTMLButtonElement>('[data-testid="add-to-cart"]')?.disabled).toBe(
      true,
    );

    signedIn = false;
    await show(mug);
    expect(root().querySelector('[data-testid="add-to-cart"]')).toBeNull();
  });

  it('gives the seller an edit link instead of a like', async () => {
    signedIn = true;
    await show({ ...mug, mine: true });

    expect(root().querySelector('[data-testid="like"]')).toBeNull();
    expect(root().querySelector('[data-testid="add-to-cart"]')).toBeNull();
    expect(root().querySelector('a[href="/products/23/edit"]')).not.toBeNull();
  });

  it('draws a placeholder when there is no photo', async () => {
    await show({ ...mug, photoUrl: '' });

    expect(root().querySelector('img')).toBeNull();
    expect(root().querySelector('.no-photo')?.getAttribute('aria-label')).toBe(
      `${mug.title}, no photo`,
    );
  });
});
