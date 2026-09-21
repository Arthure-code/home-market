import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { Router, provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, of } from 'rxjs';
import { ProductForm } from './product-form';
import { PhotoUploader } from '../photo-uploader/photo-uploader';
import { Product, ProductDraft } from '../product';
import { ProductService } from '../product.service';

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

describe('ProductForm', () => {
  let fixture: ComponentFixture<ProductForm>;
  let calls: { name: string; draft?: ProductDraft }[];
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
  const pick = async (id: string, value: string) => {
    const el = root().querySelector<HTMLSelectElement>(`#${id}`)!;
    el.value = value;
    el.dispatchEvent(new Event('change'));
    await fixture.whenStable();
  };
  const open = async (id?: string) => {
    fixture = TestBed.createComponent(ProductForm);
    if (id) fixture.componentRef.setInput('id', id);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  };

  beforeEach(async () => {
    calls = [];
    getAnswer = of(listed);
    toastr = { success: vi.fn(), error: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [ProductForm],
      providers: [
        provideRouter([]),
        {
          provide: ProductService,
          useValue: {
            get: () => getAnswer,
            categories: () =>
              of([
                { name: 'Kitchen', count: 5 },
                { name: 'Sports & outdoors', count: 1 },
              ]),
            create: (draft: ProductDraft) => {
              calls.push({ name: 'create', draft });
              return of({ ...listed, id: 31 });
            },
            update: (id: number, draft: ProductDraft) => {
              calls.push({ name: `update ${id}`, draft });
              return of(listed);
            },
          },
        },
        { provide: ToastrService, useValue: toastr },
      ],
    }).compileComponents();
    navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
  });

  it('lists a new product with what was typed and opens its page', async () => {
    await open();
    expect(root().querySelector('h1')?.textContent?.trim()).toBe('Sell a product');

    await type('title', 'Bicycle helmet');
    await type('price', '45');
    await type('brand', 'Ride');
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
    expect(calls).toEqual([]);
    expect(toastr.error).toHaveBeenCalledWith('Pick a category');

    await pick('category', 'Sports & outdoors');
    await type('stock', '3');
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    expect(calls[0].name).toBe('create');
    expect(calls[0].draft).toMatchObject({
      title: 'Bicycle helmet',
      price: 45,
      brand: 'Ride',
      category: 'Sports & outdoors',
      stock: 3,
      photo: '',
    });
    expect(toastr.success).toHaveBeenCalledWith('Product listed');
    expect(navigate).toHaveBeenCalledWith(['/products', 31]);
  });

  it('loads my product for editing and puts the changes', async () => {
    await open('30');
    expect(root().querySelector('h1')?.textContent?.trim()).toBe('Edit product');
    expect(root().querySelector<HTMLInputElement>('#title')?.value).toBe('Bicycle helmet');
    expect(root().querySelector('[data-testid="photo-preview"]')).not.toBeNull();

    await type('price', '40');
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    expect(calls[0].name).toBe('update 30');
    expect(calls[0].draft).toMatchObject({ price: 40, photo: 'abc.jpg' });
    expect(navigate).toHaveBeenCalledWith(['/products', 30]);
  });

  it('refuses to edit a product that is not mine', async () => {
    getAnswer = of({ ...listed, mine: false });

    await open('30');

    expect(toastr.error).toHaveBeenCalledWith('You can only edit your own products');
    expect(navigate).toHaveBeenCalledWith(['/products', 30]);
  });

  it('keeps the name of an uploaded photo in the draft', async () => {
    await open();

    const uploader = fixture.debugElement.query(By.directive(PhotoUploader));
    (uploader.componentInstance as PhotoUploader).uploaded.emit({
      photo: 'new.png',
      url: 'http://localhost:5130/images/new.png',
    });
    await fixture.whenStable();
    await type('title', 'Bicycle helmet');
    await type('price', '45');
    await pick('category', 'Sports & outdoors');
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    expect(calls[0].draft).toMatchObject({ title: 'Bicycle helmet', photo: 'new.png' });
    expect(root().querySelector('[data-testid="photo-preview"]')?.getAttribute('src')).toContain(
      'new.png',
    );
  });
});
