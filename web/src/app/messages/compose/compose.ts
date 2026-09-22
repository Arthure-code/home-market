import { Component, Input, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../helpers/api-message';
import { MessageDraft } from '../../models/message';
import { MessageService } from '../../services/message.service';

// A new message. The recipient and the subject may come from the address
// (the Message the seller button, the Reply button) and can be changed;
// the sender is whoever holds the token.
@Component({
  selector: 'app-compose',
  imports: [FormsModule, RouterLink],
  templateUrl: './compose.html',
})
export class Compose implements OnInit {
  @Input() to?: string;
  @Input() subject?: string;
  draft: MessageDraft = { to: '', subject: '', body: '' };
  sending = false;

  constructor(
    private readonly service: MessageService,
    private readonly router: Router,
    private readonly toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.draft.to = this.to ?? '';
    this.draft.subject = this.subject ?? '';
  }

  send(): void {
    const draft = this.draft;
    if (!draft.to.trim() || !draft.subject.trim() || !draft.body.trim()) {
      this.toastr.error('Please fill in the recipient, the subject and the message');
      return;
    }
    this.sending = true;
    this.service.send(draft).subscribe({
      next: () => {
        this.toastr.success(`Message sent to ${draft.to.trim()}`);
        this.router.navigateByUrl('/messages');
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.sending = false;
      },
    });
  }
}
