import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../helpers/api-message';
import { Credentials } from '../models/session';
import { SessionService } from '../services/session.service';

// Creating an account, then signing in with it right away.
@Component({
  selector: 'app-sign-up',
  imports: [FormsModule, RouterLink],
  templateUrl: './sign-up.html',
})
export class SignUp {
  @Input() returnUrl?: string;
  credentials: Credentials = { userName: '', password: '' };
  confirmation = '';
  busy = false;

  constructor(
    private readonly session: SessionService,
    private readonly router: Router,
    private readonly toastr: ToastrService,
  ) {}

  signUp(): void {
    if (this.credentials.password !== this.confirmation) {
      this.toastr.error('The two passwords differ');
      return;
    }
    this.busy = true;
    this.session.register(this.credentials).subscribe({
      next: () => this.signIn(),
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.busy = false;
      },
    });
  }

  private signIn(): void {
    this.session.signIn(this.credentials).subscribe({
      next: () => {
        this.toastr.success(`Welcome, ${this.credentials.userName.trim()}`);
        this.router.navigateByUrl(this.returnUrl || '/products');
      },
      error: () => {
        this.toastr.success('Account created, please sign in');
        this.router.navigateByUrl('/sign-in');
      },
    });
  }
}
