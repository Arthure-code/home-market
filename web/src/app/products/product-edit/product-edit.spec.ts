import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, of } from 'rxjs';
import { ProductEdit } from './product-edit';
import { Product, ProductDraft } from '../../models/product';
import { ProductService } from '../../services/product.service';

const listed: Product = {
  id: 30,
  title: 'Bicycle helmet',
  brand: 'Ride',
  maker: 'Ride Co.',
  description: 'Size M.',
  price: 45,
  category: 'Kitchen',
  stock: 12,
  photoUrl: 'http://localhost:5130/images/abc.jpg',
  seller: 'nadia',
  createdAt: '',
  updatedAt: '',
  likes: 0,
  liked: false,
  mine: true,
};

describe('ProductEdit', () => {
  let fixture: ComponentFixture<ProductEdit>;
  let updated: { id: number; draft: ProductDraft }[];
  let getAnswer: Observable<Product>;
  let toastr: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };
  let navigate: ReturnType<typeof vi.spyOn>;

  const root = () => fixture.nativeElement as HTMLElement;
  const type = async (id: string, value: string) => {
    const el = root().querySelector<HTMLInputElement>(`#${id}`)!;
    el.value = value;
    el.dispatchEvent(new Event('input'));
    await fixture.whenStable();
  };
  const submit = async () => {
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
  };
  const open = async () => {
    fixture = TestBed.createComponent(ProductEdit);
    fixture.componentRef.setInput('id', '30');
    fixture.autoDetectChanges();
    await fixture.whenStable();
  };

  beforeEach(async () => {
    updated = [];
    getAnswer = of(listed);
    toastr = { success: vi.fn(), error: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [ProductEdit],
      providers: [
        provideRouter([]),
        {
          provide: ProductService,
          useValue: {
            categories: () =>
              of([
                { name: 'Kitchen', count: 5 },
                { name: 'Sports & outdoors', count: 1 },
              ]),
            get: () => getAnswer,
            update: (id: number, draft: ProductDraft) => {
              updated.push({ id, draft: { ...draft } });
              return of(listed);
            },
          },
        },
        { provide: ToastrService, useValue: toastr },
      ],
    }).compileComponents();
    navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
  });

  it('loads my product, with its photo named the way the API wants it back', async () => {
    await open();

    expect(root().querySelector('h1')?.textContent?.trim()).toBe('Edit product');
    expect(root().querySelector<HTMLInputElement>('#title')?.value).toBe('Bicycle helmet');
    expect(root().querySelector<HTMLSelectElement>('#category')?.value).toBe('Kitchen');
    expect(root().querySelector('[data-testid="photo-preview"]')?.getAttribute('src')).toContain(
      'abc.jpg',
    );
  });

  it('puts the changes and opens the product page', async () => {
    await open();

    await type('price', '40');
    await submit();

    expect(updated).toEqual([
      {
        id: 30,
        draft: {
          title: 'Bicycle helmet',
          brand: 'Ride',
          maker: 'Ride Co.',
          description: 'Size M.',
          price: 40,
          category: 'Kitchen',
          stock: 12,
          photo: 'abc.jpg',
        },
      },
    ]);
    expect(toastr.success).toHaveBeenCalledWith('Product updated');
    expect(navigate).toHaveBeenCalledWith(['/products', 30]);
  });

  it('goes back to the catalogue when the product cannot be loaded', async () => {
    getAnswer = new Observable((subscriber) => subscriber.error(new Error('gone')));
    const navigateByUrl = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);

    await open();

    expect(root().querySelector('form')).toBeNull();
    expect(toastr.error).toHaveBeenCalled();
    expect(navigateByUrl).toHaveBeenCalledWith('/products');
  });
});
