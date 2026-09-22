import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError, map, of } from 'rxjs';
import { apiMessage } from '../helpers/api-message';
import { ProductService } from '../services/product.service';

// Only the seller reaches the edit page of a listing. Anyone else is
// told so and sent to the product; a bad or unknown id goes back to
// the catalogue.
export const ownerGuard: CanActivateFn = (route) => {
  const products = inject(ProductService);
  const router = inject(Router);
  const toastr = inject(ToastrService);

  const id = Number(route.paramMap.get('id'));
  if (!Number.isInteger(id) || id <= 0) {
    toastr.error('That link is not valid');
    return router.parseUrl('/products');
  }

  return products.get(id).pipe(
    map((product) => {
      if (product.mine) return true;
      toastr.error('You can only edit your own products');
      return router.parseUrl('/products/' + id);
    }),
    catchError((error: unknown) => {
      toastr.error(apiMessage(error));
      return of(router.parseUrl('/products'));
    }),
  );
};
