import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { SessionService } from './session.service';

// What happens when someone who is not signed in tries something that
// needs an account: a word, the sign-in page, and the way back to where
// they were. Pages and buttons ask it before acting.
@Injectable({ providedIn: 'root' })
export class SignInPrompt {
  constructor(
    private session: SessionService,
    private router: Router,
    private toastr: ToastrService,
  ) {}

  // True when the caller may go on; false after sending them to sign in.
  ensure(): boolean {
    if (this.session.signedIn) return true;
    this.toastr.info('Please sign in to continue');
    this.router.navigate(['/sign-in'], { queryParams: { returnUrl: this.router.url } });
    return false;
  }
}
