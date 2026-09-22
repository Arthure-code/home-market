import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ACCOUNTS_URL, SessionService } from './session.service';
import { Session } from '../models/session';

const session: Session = {
  userName: 'nadia',
  token: 'signed.token.value',
  expiresAt: new Date(Date.now() + 3_600_000).toISOString(),
};

describe('SessionService', () => {
  let service: SessionService;
  let http: HttpTestingController;

  const setUp = () => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(SessionService);
    http = TestBed.inject(HttpTestingController);
  };

  beforeEach(() => {
    sessionStorage.clear();
    setUp();
  });

  afterEach(() => http.verify());

  it('starts signed out with no token', () => {
    expect(service.signedIn()).toBe(false);
    expect(service.token()).toBeNull();
    expect(service.userName()).toBe('');
  });

  it('posts the account to open', () => {
    service.register({ userName: 'nadia', password: 'correct horse battery' }).subscribe();

    const request = http.expectOne(ACCOUNTS_URL);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ userName: 'nadia', password: 'correct horse battery' });
    request.flush({ message: 'Account created.' }, { status: 201, statusText: 'Created' });
  });

  it('keeps the session returned by the login and stores it for the tab', () => {
    service.signIn({ userName: 'nadia', password: 'correct horse battery' }).subscribe();
    http.expectOne(`${ACCOUNTS_URL}/login`).flush(session);

    expect(service.signedIn()).toBe(true);
    expect(service.userName()).toBe('nadia');
    expect(service.token()).toBe('signed.token.value');
    expect(JSON.parse(sessionStorage.getItem('home-market.session') ?? '{}')).toEqual(session);
  });

  it('restores a stored session that has not expired', () => {
    sessionStorage.setItem('home-market.session', JSON.stringify(session));
    TestBed.resetTestingModule();
    setUp();

    expect(service.signedIn()).toBe(true);
    expect(service.userName()).toBe('nadia');
  });

  it('drops a stored session that has expired', () => {
    sessionStorage.setItem(
      'home-market.session',
      JSON.stringify({ ...session, expiresAt: new Date(Date.now() - 1000).toISOString() }),
    );
    TestBed.resetTestingModule();
    setUp();

    expect(service.signedIn()).toBe(false);
  });

  it('forgets everything on sign out', () => {
    service.signIn({ userName: 'nadia', password: 'correct horse battery' }).subscribe();
    http.expectOne(`${ACCOUNTS_URL}/login`).flush(session);

    service.signOut();

    expect(service.signedIn()).toBe(false);
    expect(sessionStorage.getItem('home-market.session')).toBeNull();
  });
});
