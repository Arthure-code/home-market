import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { Credentials, Session } from '../models/session';

export const ACCOUNTS_URL = 'http://localhost:5130/api/accounts';
const STORAGE_KEY = 'home-market.session';

// The session lives in sessionStorage: it survives a page reload and
// dies with the tab. An expired token is dropped rather than sent.
@Injectable({ providedIn: 'root' })
export class SessionService {
  session: Session | null = restore();

  constructor(private readonly http: HttpClient) {}

  get signedIn(): boolean {
    return this.session !== null;
  }

  get userName(): string {
    return this.session?.userName ?? '';
  }

  token(): string | null {
    return this.session?.token ?? null;
  }

  register(credentials: Credentials): Observable<unknown> {
    return this.http.post(ACCOUNTS_URL, credentials);
  }

  signIn(credentials: Credentials): Observable<Session> {
    return this.http.post<Session>(`${ACCOUNTS_URL}/login`, credentials).pipe(
      tap((session) => {
        this.session = session;
        try {
          sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session));
        } catch {
          // Storage can be unavailable; the session then lasts the page.
        }
      }),
    );
  }

  signOut(): void {
    this.session = null;
    try {
      sessionStorage.removeItem(STORAGE_KEY);
    } catch {
      // Nothing to remove.
    }
  }
}

function restore(): Session | null {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    const session = JSON.parse(raw) as Session;
    return new Date(session.expiresAt) > new Date() ? session : null;
  } catch {
    return null;
  }
}
