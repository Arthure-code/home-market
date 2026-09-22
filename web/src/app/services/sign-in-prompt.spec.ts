import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { SignInPrompt } from './sign-in-prompt';
import { SessionService } from './session.service';

describe('SignInPrompt', () => {
  const configure = (signedIn: boolean) => {
    const toastr = { info: vi.fn() };
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: SessionService, useValue: { signedIn } },
        { provide: ToastrService, useValue: toastr },
      ],
    });
    return toastr;
  };

  it('lets a member go on and says nothing', () => {
    const toastr = configure(true);
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate');

    expect(TestBed.inject(SignInPrompt).ensure()).toBe(true);
    expect(toastr.info).not.toHaveBeenCalled();
    expect(navigate).not.toHaveBeenCalled();
  });

  it('stops a visitor, says so, and sends them to sign in with the way back', () => {
    const toastr = configure(false);
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'url', 'get').mockReturnValue('/products/4');
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    expect(TestBed.inject(SignInPrompt).ensure()).toBe(false);
    expect(toastr.info).toHaveBeenCalledWith('Please sign in to continue');
    expect(navigate).toHaveBeenCalledWith(['/sign-in'], {
      queryParams: { returnUrl: '/products/4' },
    });
  });
});
