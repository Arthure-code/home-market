import { HttpErrorResponse } from '@angular/common/http';

// The API answers errors as { message }, or as the validation problem
// details ASP.NET Core writes for a bad request.
export function apiMessage(
  error: unknown,
  fallback = 'Something went wrong. Please try again.',
): string {
  if (!(error instanceof HttpErrorResponse)) return fallback;
  if (error.status === 0) return 'The server did not answer. Check that the API is running.';
  if (error.status === 429) return 'Too many attempts. Please wait a minute.';
  const body = error.error as { message?: string; errors?: Record<string, string[]> } | null;
  if (body?.message) return body.message;
  const first = body?.errors && Object.values(body.errors)[0]?.[0];
  return first ?? fallback;
}
