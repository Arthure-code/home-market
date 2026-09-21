import { HttpErrorResponse } from '@angular/common/http';

// The API answers every error as a Problem Details document (RFC 9457):
// a detail for a refusal with a reason, a title otherwise, and for a
// request that failed validation, the errors field by field.
interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

export function apiMessage(
  error: unknown,
  fallback = 'Something went wrong. Please try again.',
): string {
  if (!(error instanceof HttpErrorResponse)) return fallback;
  if (error.status === 0) return 'The server did not answer. Check that the API is running.';
  if (error.status === 429) return 'Too many attempts. Please wait a minute.';
  const problem = error.error as ProblemDetails | null;
  if (problem?.detail) return problem.detail;
  const first = problem?.errors && Object.values(problem.errors)[0]?.[0];
  return first ?? fallback;
}
