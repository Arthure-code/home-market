import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../helpers/api-message';
import { Message } from '../../models/message';
import { Folder, MessageService } from '../../services/message.service';

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

  protected readonly folder = signal<Folder>('inbox');
  protected readonly messages = signal<Message[]>([]);
  protected readonly loading = signal(true);
  protected readonly unread = computed(
    () => this.messages().filter((m) => !m.mine && m.readAt === null).length,
  );

  ngOnInit(): void {
    this.show('inbox');
  }

  protected show(folder: Folder): void {
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

  protected isNew(message: Message): boolean {
    return !message.mine && message.readAt === null;
  }
}
