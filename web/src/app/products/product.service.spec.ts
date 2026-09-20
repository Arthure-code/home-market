import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { PRODUCTS_URL, ProductService } from './product.service';

describe('ProductService', () => {
  let service: ProductService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ProductService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('reads the catalogue, one product, my listings and my likes', () => {
    service.list().subscribe();
    service.get(7).subscribe();
    service.mine().subscribe();
    service.liked().subscribe();

    http.expectOne(PRODUCTS_URL).flush([]);
    http.expectOne(`${PRODUCTS_URL}/7`).flush({});
    http.expectOne(`${PRODUCTS_URL}/mine`).flush([]);
    http.expectOne(`${PRODUCTS_URL}/liked`).flush([]);
  });

  it('narrows the catalogue with a search text and a category, and reads the categories', () => {
    service.list({ q: 'kettle', category: 'Kitchen' }).subscribe();
    service.list({ q: '' }).subscribe();
    service.categories().subscribe();

    http.expectOne(`${PRODUCTS_URL}?q=kettle&category=Kitchen`).flush([]);
    http.expectOne(PRODUCTS_URL).flush([]);
    http.expectOne(`${PRODUCTS_URL}/categories`).flush([]);
  });

  it('posts a new listing and puts a changed one', () => {
    const draft = {
      title: 'Mug',
      brand: '',
      maker: '',
      description: '',
      price: 12,
      category: 'Kitchen',
      stock: 3,
      photo: '',
    };
    service.create(draft).subscribe();
    service.update(4, draft).subscribe();

    const created = http.expectOne(PRODUCTS_URL);
    expect(created.request.method).toBe('POST');
    expect(created.request.body).toEqual(draft);
    created.flush({});
    const updated = http.expectOne(`${PRODUCTS_URL}/4`);
    expect(updated.request.method).toBe('PUT');
    updated.flush({});
  });

  it('likes with a PUT and unlikes with a DELETE on the same address', () => {
    service.setLike(7, true).subscribe();
    service.setLike(7, false).subscribe();

    const like = http.expectOne((r) => r.url === `${PRODUCTS_URL}/7/like` && r.method === 'PUT');
    const unlike = http.expectOne(
      (r) => r.url === `${PRODUCTS_URL}/7/like` && r.method === 'DELETE',
    );
    like.flush(null, { status: 204, statusText: 'No Content' });
    unlike.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('sends a photo as multipart form data', () => {
    const file = new File(['x'], 'mug.jpg', { type: 'image/jpeg' });
    service.uploadPhoto(file).subscribe();

    const request = http.expectOne(`${PRODUCTS_URL}/photos`);
    expect(request.request.body).toBeInstanceOf(FormData);
    expect((request.request.body as FormData).get('photo')).toBe(file);
    request.flush({ photo: 'abc.jpg', url: 'http://localhost:5130/images/abc.jpg' });
  });
});
