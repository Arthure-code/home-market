import { HttpErrorResponse } from '@angular/common/http';
import { apiMessage } from './api-message';

describe('apiMessage', () => {
  const error = (status: number, body: unknown) => new HttpErrorResponse({ status, error: body });

  it('reads the message the API writes', () => {
    expect(apiMessage(error(409, { message: 'That user name is taken.' }))).toBe(
      'That user name is taken.',
    );
  });

  it('reads the first validation error of a bad request', () => {
    expect(apiMessage(error(400, { errors: { Password: ['Too short.'] } }))).toBe('Too short.');
  });

  it('names the server being down and the rate limit', () => {
    expect(apiMessage(error(0, null))).toContain('did not answer');
    expect(apiMessage(error(429, null))).toContain('Too many attempts');
  });

  it('falls back to a generic sentence', () => {
    expect(apiMessage(new Error('boom'))).toBe('Something went wrong. Please try again.');
    expect(apiMessage(error(500, null))).toBe('Something went wrong. Please try again.');
  });
});
