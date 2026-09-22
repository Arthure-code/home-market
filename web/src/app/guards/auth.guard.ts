import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { SessionService } from '../services/session.service';

// Pages that need an account send a visitor to sign in, and bring them
// back afterwards.
export const authGuard: CanActivateFn = (_route, state) => {
  const session = inject(SessionService);
  if (session.signedIn()) return true;

  inject(ToastrService).info('Please sign in to continue');
  return inject(Router).createUrlTree(['/sign-in'], { queryParams: { returnUrl: state.url } });
};
