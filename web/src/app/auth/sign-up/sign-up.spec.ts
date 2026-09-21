import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, of, throwError } from 'rxjs';
import { SignUp } from './sign-up';
import { Credentials, Session } from '../session';
import { SessionService } from '../session.service';

describe('SignUp', () => {
  let fixture: ComponentFixture<SignUp>;
  let registered: Observable<unknown>;
  let calls: string[];
  let toastr: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

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
    fixture = TestBed.createComponent(SignUp);
    if (returnUrl !== undefined) fixture.componentRef.setInput('returnUrl', returnUrl);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  };

  beforeEach(async () => {
    registered = of({});
    calls = [];
    toastr = { success: vi.fn(), error: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [SignUp],
      providers: [
        provideRouter([]),
        {
          provide: SessionService,
          useValue: {
            register: (credentials: Credentials) => {
              calls.push(`register ${credentials.userName}`);
              return registered;
            },
            signIn: (credentials: Credentials): Observable<Session> => {
              calls.push(`sign in ${credentials.userName}`);
              return of({ userName: credentials.userName, token: 't', expiresAt: '2099-01-01' });
            },
          },
        },
        { provide: ToastrService, useValue: toastr },
      ],
    }).compileComponents();
  });

  it('creates the account, signs in with it and goes back where the visitor was', async () => {
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    await open('/cart');
    await type('userName', 'nadia');
    await type('password', 'correct horse battery');
    await type('confirmation', 'correct horse battery');
    await submit();

    expect(calls).toEqual(['register nadia', 'sign in nadia']);
    expect(toastr.success).toHaveBeenCalledWith('Welcome, nadia');
    expect(navigate).toHaveBeenCalledWith('/cart');
  });

  it('refuses two different passwords without calling the API', async () => {
    await open();
    await type('userName', 'nadia');
    await type('password', 'correct horse battery');
    await type('confirmation', 'correct horse batery');
    await submit();

    expect(calls).toEqual([]);
    expect(toastr.error).toHaveBeenCalledWith('The two passwords differ');
  });

  it('shows the API message when the name is taken', async () => {
    registered = throwError(
      () => new HttpErrorResponse({ status: 409, error: { message: 'That user name is taken.' } }),
    );
    await open();
    await type('userName', 'nadia');
    await type('password', 'correct horse battery');
    await type('confirmation', 'correct horse battery');
    await submit();

    expect(calls).toEqual(['register nadia']);
    expect(toastr.error).toHaveBeenCalledWith('That user name is taken.');
    expect(fixture.componentInstance.busy()).toBe(false);
  });
});
