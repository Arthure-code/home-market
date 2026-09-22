import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { Router, provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';
import { ProductNew } from './product-new';
import { Product, ProductDraft } from '../../models/product';
import { PhotoUploader } from '../../photo-uploader/photo-uploader';
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

describe('ProductNew', () => {
  let fixture: ComponentFixture<ProductNew>;
  let created: ProductDraft[];
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
  const submit = async () => {
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
  };

  beforeEach(async () => {
    created = [];
    toastr = { success: vi.fn(), error: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [ProductNew],
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
            create: (draft: ProductDraft) => {
              created.push({ ...draft });
              return of({ ...listed, id: 31 });
            },
          },
        },
        { provide: ToastrService, useValue: toastr },
      ],
    }).compileComponents();
    navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    fixture = TestBed.createComponent(ProductNew);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  });

  it('lists a new product with what was typed and opens its page', async () => {
    expect(root().querySelector('h1')?.textContent?.trim()).toBe('Sell a product');

    await type('title', 'Bicycle helmet');
    await type('price', '45');
    await type('brand', 'Ride');
    await pick('category', 'Sports & outdoors');
    await type('stock', '3');
    await submit();

    expect(created).toEqual([
      {
        title: 'Bicycle helmet',
        brand: 'Ride',
        maker: '',
        description: '',
        price: 45,
        category: 'Sports & outdoors',
        stock: 3,
        photo: '',
      },
    ]);
    expect(toastr.success).toHaveBeenCalledWith('Product listed');
    expect(navigate).toHaveBeenCalledWith(['/products', 31]);
  });

  it('asks for a category before listing anything', async () => {
    await type('title', 'Bicycle helmet');
    await type('price', '45');
    await submit();

    expect(created).toEqual([]);
    expect(toastr.error).toHaveBeenCalledWith('Pick a category');
  });

  it('keeps the name of an uploaded photo in the draft and shows it', async () => {
    const uploader = fixture.debugElement.query(By.directive(PhotoUploader));
    (uploader.componentInstance as PhotoUploader).uploaded.emit({
      photo: 'new.png',
      url: 'http://localhost:5130/images/new.png',
    });
    await fixture.whenStable();
    await type('title', 'Bicycle helmet');
    await type('price', '45');
    await pick('category', 'Sports & outdoors');
    await submit();

    expect(created[0]).toMatchObject({ title: 'Bicycle helmet', photo: 'new.png' });
    expect(root().querySelector('[data-testid="photo-preview"]')?.getAttribute('src')).toContain(
      'new.png',
    );
  });
});
