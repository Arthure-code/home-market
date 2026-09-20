import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { SessionService } from './session.service';

// Sends the token with every call and, when the API says it is no longer
// good, ends the session and goes back to the sign-in page.
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const session = inject(SessionService);
  const router = inject(Router);

  const token = session.token();
  const outgoing = token
    ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : request;

  return next(outgoing).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401 && token) {
        session.signOut();
        router.navigate(['/']);
      }
      return throwError(() => error);
    }),
  );
};
