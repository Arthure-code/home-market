import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../auth/api-message';
import { Message } from '../message';
import { Folder, MessageService } from '../message.service';

// My messages: the inbox, or what I sent, one table for both. An unread
// line in the inbox is shown in bold with a New badge.
@Component({
  selector: 'app-message-list',
  imports: [DatePipe, RouterLink],
  templateUrl: './message-list.html',
})
export class MessageList implements OnInit {
  private readonly service = inject(MessageService);
  private readonly toastr = inject(ToastrService);

  readonly folder = signal<Folder>('inbox');
  readonly messages = signal<Message[]>([]);
  readonly loading = signal(true);
  readonly unread = computed(
    () => this.messages().filter((m) => !m.mine && m.readAt === null).length,
  );

  ngOnInit(): void {
    this.show('inbox');
  }

  show(folder: Folder): void {
    this.folder.set(folder);
    this.loading.set(true);
    this.service.folder(folder).subscribe({
      next: (messages) => {
        this.messages.set(messages);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.loading.set(false);
      },
    });
  }

  isNew(message: Message): boolean {
    return !message.mine && message.readAt === null;
  }
}
