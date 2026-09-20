import { DatePipe } from '@angular/common';
import { Component, effect, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { MessageDetail } from '../message';
import { MessageService } from '../message.service';

// One message, reached by its id. The API answers only its sender and
// its recipient, so a message that is not mine reads as missing. Opening
// a message I received marks it read.
@Component({
  selector: 'app-message-view',
  imports: [DatePipe, RouterLink],
  templateUrl: './message-view.html',
  styleUrl: './message-view.css',
})
export class MessageView {
  private readonly service = inject(MessageService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);

  readonly id = input.required<string>();
  readonly message = signal<MessageDetail | null>(null);

  constructor() {
    effect(() => this.load(Number(this.id())));
  }

  private load(id: number): void {
    this.service.get(id).subscribe({
      next: (message) => {
        this.message.set(message);
        if (!message.mine && message.readAt === null) this.markRead(message);
      },
      error: () => {
        this.toastr.error('There is no such message in your folders.');
        this.router.navigateByUrl('/messages');
      },
    });
  }

  private markRead(message: MessageDetail): void {
    this.service.markRead(message.id).subscribe({
      next: () => this.message.set({ ...message, readAt: new Date().toISOString() }),
      error: () => {
        // The message is shown anyway; it will read as new next time.
      },
    });
  }
}
