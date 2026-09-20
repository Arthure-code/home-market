import { Component, effect, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../auth/api-message';
import { MessageDraft } from '../message';
import { MessageService } from '../message.service';

// A new message. The recipient and the subject may come from the address
// (the Message the seller button, the Reply button) and can be changed;
// the sender is whoever holds the token.
@Component({
  selector: 'app-compose',
  imports: [FormsModule, RouterLink],
  templateUrl: './compose.html',
})
export class Compose {
  private readonly service = inject(MessageService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);

  readonly to = input<string>();
  readonly subject = input<string>();
  readonly draft = signal<MessageDraft>({ to: '', subject: '', body: '' });
  readonly sending = signal(false);

  constructor() {
    effect(() => {
      this.draft.update((d) => ({ ...d, to: this.to() ?? '', subject: this.subject() ?? '' }));
    });
  }

  send(): void {
    const draft = this.draft();
    if (!draft.to.trim() || !draft.subject.trim() || !draft.body.trim()) {
      this.toastr.error('Please fill in the recipient, the subject and the message');
      return;
    }
    this.sending.set(true);
    this.service.send(draft).subscribe({
      next: () => {
        this.toastr.success(`Message sent to ${draft.to.trim()}`);
        this.router.navigateByUrl('/messages');
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.sending.set(false);
      },
    });
  }
}
