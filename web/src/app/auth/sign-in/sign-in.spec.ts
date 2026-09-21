import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, of, throwError } from 'rxjs';
import { SignIn } from './sign-in';
import { Credentials, Session } from '../session';
import { SessionService } from '../session.service';

describe('SignIn', () => {
  let fixture: ComponentFixture<SignIn>;
  let answer: Observable<Session>;
  let attempts: Credentials[];
  let toastr: { error: ReturnType<typeof vi.fn> };

  const root = () => fixture.nativeElement as HTMLElement;
  const type = async (id: string, value: string) => {
    const el = root().querySelector<HTMLInputElement>(`#${id}`)!;
    el.value = value;
    el.dispatchEvent(new Event('input'));
    await fixture.whenStable();
  };
  const submit = async () => {
    root().querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
  };
  const open = async (returnUrl?: string) => {
    fixture = TestBed.createComponent(SignIn);
    if (returnUrl !== undefined) fixture.componentRef.setInput('returnUrl', returnUrl);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  };

  beforeEach(async () => {
    answer = of({ userName: 'nadia', token: 't', expiresAt: '2099-01-01T00:00:00Z' });
    attempts = [];
    toastr = { error: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [SignIn],
      providers: [
        provideRouter([]),
        {
          provide: SessionService,
          useValue: {
            signIn: (credentials: Credentials) => {
              attempts.push({ ...credentials });
              return answer;
            },
          },
        },
        { provide: ToastrService, useValue: toastr },
      ],
    }).compileComponents();
  });

  it('signs in and goes back where the visitor was', async () => {
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    await open('/products/4');
    await type('userName', 'nadia');
    await type('password', 'correct horse battery');
    await submit();

    expect(attempts).toEqual([{ userName: 'nadia', password: 'correct horse battery' }]);
    expect(navigate).toHaveBeenCalledWith('/products/4');
    expect(root().querySelector('[data-testid="sign-up-link"]')?.getAttribute('href')).toBe(
      '/sign-up?returnUrl=%2Fproducts%2F4',
    );
  });

  it('goes to the catalogue when nothing was asked for', async () => {
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    await open();
    await type('userName', 'nadia');
    await type('password', 'correct horse battery');
    await submit();

    expect(navigate).toHaveBeenCalledWith('/products');
  });

  it('refuses an empty form and shows the API message for a wrong password', async () => {
    await open();
    await submit();
    expect(attempts).toEqual([]);
    expect(toastr.error).toHaveBeenCalledWith('Please enter your user name and password');

    answer = throwError(
      () =>
        new HttpErrorResponse({ status: 401, error: { detail: 'Wrong user name or password.' } }),
    );
    await type('userName', 'nadia');
    await type('password', 'wrong');
    await submit();
    expect(toastr.error).toHaveBeenCalledWith('Wrong user name or password.');
    expect(root().querySelector<HTMLButtonElement>('[data-testid="sign-in"]')?.disabled).toBe(
      false,
    );
  });
});
