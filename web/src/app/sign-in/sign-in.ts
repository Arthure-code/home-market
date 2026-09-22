import { Component, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../helpers/api-message';
import { Credentials } from '../models/session';
import { SessionService } from '../services/session.service';

// The sign-in page. It is where a visitor lands when they try something
// that needs an account; once in, they go back where they were.
@Component({
  selector: 'app-sign-in',
  imports: [FormsModule, RouterLink],
  templateUrl: './sign-in.html',
})
export class SignIn {
  private readonly session = inject(SessionService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);

  readonly returnUrl = input<string>();
  protected readonly credentials: Credentials = { userName: '', password: '' };
  protected readonly busy = signal(false);

  protected signIn(): void {
    if (!this.credentials.userName.trim() || !this.credentials.password) {
      this.toastr.error('Please enter your user name and password');
      return;
    }
    this.busy.set(true);
    this.session.signIn(this.credentials).subscribe({
      next: () => {
        this.credentials.password = '';
        this.router.navigateByUrl(this.returnUrl() || '/products');
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.busy.set(false);
      },
    });
  }
}
