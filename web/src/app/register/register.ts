import { Component, inject, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../auth/api-message';
import { Credentials } from '../auth/session';
import { SessionService } from '../auth/session.service';

@Component({
  selector: 'app-register',
  imports: [FormsModule],
  templateUrl: './register.html',
})
export class Register {
  private readonly session = inject(SessionService);
  private readonly toastr = inject(ToastrService);

  readonly done = output<void>();
  readonly credentials: Credentials = { userName: '', password: '' };

  register(): void {
    this.session.register(this.credentials).subscribe({
      next: () => {
        this.toastr.success('Account created, sign in from the bar above');
        this.done.emit();
      },
      error: (error: unknown) => this.toastr.error(apiMessage(error)),
    });
  }
}
