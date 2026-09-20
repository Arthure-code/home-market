import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { SessionService } from './session.service';

export const authGuard: CanActivateFn = () => {
  const session = inject(SessionService);
  if (session.signedIn()) return true;

  inject(ToastrService).error('Please sign in first');
  return inject(Router).createUrlTree(['/']);
};
