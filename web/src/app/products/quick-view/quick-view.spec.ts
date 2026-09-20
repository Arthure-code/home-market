import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { QuickView } from './quick-view';
import { SessionService } from '../../auth/session.service';
import { Product } from '../product';

const lamp: Product = {
  id: 11,
  title: 'Table lamp, brass',
  brand: 'Lumo',
  maker: 'Lumo Lighting',
  description: 'Warm light.\nDimmable.',
  price: 65,
  category: 'Home',
  stock: 6,
  photoUrl: 'https://images.unsplash.com/photo-11',
  seller: 'homemarket',
  createdAt: '2026-09-01T00:00:00Z',
  updatedAt: '2026-09-01T00:00:00Z',
  likes: 2,
  liked: false,
  mine: false,
};

describe('QuickView', () => {
  let fixture: ComponentFixture<QuickView>;
  let signedIn = false;

  const root = () => fixture.nativeElement as HTMLElement;
  const dialog = () => root().querySelector('dialog')!;
  const show = async (product: Product | null) => {
    fixture = TestBed.createComponent(QuickView);
    fixture.componentRef.setInput('product', product);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  };

  // jsdom draws the dialog but knows neither showModal nor close.
  beforeAll(() => {
    HTMLDialogElement.prototype.showModal = function (this: HTMLDialogElement) {
      this.open = true;
    };
    HTMLDialogElement.prototype.close = function (this: HTMLDialogElement) {
      this.open = false;
      this.dispatchEvent(new Event('close'));
    };
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [QuickView],
      providers: [
        provideRouter([]),
        { provide: SessionService, useValue: { signedIn: () => signedIn } },
      ],
    }).compileComponents();
  });

  it('stays closed without a product and opens with one', async () => {
    await show(null);
    expect(dialog().open).toBe(false);

    fixture.componentRef.setInput('product', lamp);
    await fixture.whenStable();
    expect(dialog().open).toBe(true);
    expect(root().querySelector('h2')?.textContent?.trim()).toBe('Table lamp, brass');
    expect(root().querySelector('[data-testid="price"]')?.textContent?.trim()).toBe('$65');
    expect(root().querySelector('[data-testid="stock"]')?.textContent?.trim()).toBe('In stock');
    expect(root().querySelector('[data-testid="full-page"]')?.getAttribute('href')).toBe(
      '/products/11',
    );
  });

  it('tells the parent when it closes, and offers like and cart to a member', async () => {
    signedIn = true;
    await show(lamp);
    const closed: number[] = [];
    const carted: Product[] = [];
    fixture.componentInstance.closed.subscribe(() => closed.push(1));
    fixture.componentInstance.addToCart.subscribe((p) => carted.push(p));

    root().querySelector<HTMLButtonElement>('[data-testid="add-to-cart"]')?.click();
    expect(carted).toEqual([lamp]);
    expect(root().querySelector('[data-testid="like"]')?.textContent?.trim()).toBe('2');

    root().querySelector<HTMLButtonElement>('[data-testid="close"]')?.click();
    await fixture.whenStable();
    expect(dialog().open).toBe(false);
    expect(closed).toEqual([1]);
  });

  it('gives a visitor the look and the link only', async () => {
    signedIn = false;
    await show(lamp);

    expect(root().querySelector('[data-testid="add-to-cart"]')).toBeNull();
    expect(root().querySelector('[data-testid="like"]')).toBeNull();
    expect(root().querySelector('[data-testid="full-page"]')).not.toBeNull();
  });
});
