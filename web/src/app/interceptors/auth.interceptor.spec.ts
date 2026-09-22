import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { authInterceptor } from './auth.interceptor';
import { SessionService } from '../services/session.service';

describe('authInterceptor', () => {
  let http: HttpClient;
  let backend: HttpTestingController;

  // The interceptor reads the session the service restored at start-up,
  // so a stored session is written before the injector is built.
  const setUp = (token?: string) => {
    sessionStorage.clear();
    if (token) {
      sessionStorage.setItem(
        'home-market.session',
        JSON.stringify({
          userName: 'nadia',
          token,
          expiresAt: new Date(Date.now() + 60_000).toISOString(),
        }),
      );
    }
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
  };

  afterEach(() => backend.verify());

  it('sends no Authorization header while signed out', () => {
    setUp();

    http.get('/api/memos').subscribe();

    const request = backend.expectOne('/api/memos');
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush([]);
  });

  it('adds the bearer token once signed in', () => {
    setUp('abc');

    http.get('/api/memos').subscribe();

    const request = backend.expectOne('/api/memos');
    expect(request.request.headers.get('Authorization')).toBe('Bearer abc');
    request.flush([]);
  });

  it('ends the session and goes home when the API refuses the token', () => {
    setUp('stale');
    const session = TestBed.inject(SessionService);
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    let failed = false;
    http.get('/api/memos').subscribe({ error: () => (failed = true) });
    backend.expectOne('/api/memos').flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(failed).toBe(true);
    expect(session.signedIn()).toBe(false);
    expect(navigate).toHaveBeenCalledWith(['/']);
  });

  it('leaves a 401 to the sign-in page alone', () => {
    setUp();
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    http.post('/api/accounts/login', {}).subscribe({ error: () => undefined });
    backend.expectOne('/api/accounts/login').flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(navigate).not.toHaveBeenCalled();
  });
});
