import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  Router,
  RouterStateSnapshot,
  UrlTree,
  provideRouter,
} from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { authGuard } from './auth.guard';
import { SessionService } from './session.service';

describe('authGuard', () => {
  const run = () =>
    TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url: '/memos' } as RouterStateSnapshot),
    );

  const configure = (signedIn: boolean) => {
    const toastr = { info: vi.fn() };
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: SessionService, useValue: { signedIn: () => signedIn } },
        { provide: ToastrService, useValue: toastr },
      ],
    });
    return toastr;
  };

  it('lets a signed-in visitor through', () => {
    const toastr = configure(true);

    expect(run()).toBe(true);
    expect(toastr.info).not.toHaveBeenCalled();
  });

  it('sends a visitor who is not signed in to the sign-in page, with a message', () => {
    const toastr = configure(false);

    const result = run() as UrlTree;

    expect(result instanceof UrlTree).toBe(true);
    expect(TestBed.inject(Router).serializeUrl(result)).toBe('/sign-in?returnUrl=%2Fmemos');
    expect(toastr.info).toHaveBeenCalledWith('Please sign in to continue');
  });
});
