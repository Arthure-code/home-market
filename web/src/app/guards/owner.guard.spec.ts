import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  Router,
  RouterStateSnapshot,
  UrlTree,
  convertToParamMap,
  provideRouter,
} from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, firstValueFrom, of, throwError } from 'rxjs';
import { ownerGuard } from './owner.guard';
import { Product } from '../models/product';
import { ProductService } from '../services/product.service';

const helmet: Product = {
  id: 30,
  title: 'Bicycle helmet',
  brand: '',
  maker: '',
  description: '',
  price: 45,
  category: 'Sports & outdoors',
  stock: 2,
  photoUrl: '',
  seller: 'nadia',
  createdAt: '',
  updatedAt: '',
  likes: 0,
  liked: false,
  mine: true,
};

describe('ownerGuard', () => {
  const run = async (id: string) => {
    const route = { paramMap: convertToParamMap({ id }) } as ActivatedRouteSnapshot;
    const result = TestBed.runInInjectionContext(() =>
      ownerGuard(route, {} as RouterStateSnapshot),
    );
    return result instanceof Observable ? firstValueFrom(result) : result;
  };
  const url = (result: unknown) => TestBed.inject(Router).serializeUrl(result as UrlTree);

  const configure = (answer: Observable<Product>) => {
    const toastr = { error: vi.fn() };
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: ProductService, useValue: { get: () => answer } },
        { provide: ToastrService, useValue: toastr },
      ],
    });
    return toastr;
  };

  it('lets the seller edit their own product', async () => {
    const toastr = configure(of(helmet));

    expect(await run('30')).toBe(true);
    expect(toastr.error).not.toHaveBeenCalled();
  });

  it('sends anyone else to the product page, with a message', async () => {
    const toastr = configure(of({ ...helmet, mine: false }));

    expect(url(await run('30'))).toBe('/products/30');
    expect(toastr.error).toHaveBeenCalledWith('You can only edit your own products');
  });

  it('goes back to the catalogue for a bad or unknown id', async () => {
    const toastr = configure(throwError(() => new HttpErrorResponse({ status: 404 })));

    expect(url(await run('abc'))).toBe('/products');
    expect(url(await run('999'))).toBe('/products');
    expect(toastr.error).toHaveBeenCalledTimes(2);
  });
});
