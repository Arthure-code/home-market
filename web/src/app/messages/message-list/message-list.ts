import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
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
  folder: Folder = 'inbox';
  messages: Message[] = [];
  loading = true;

  constructor(
    private service: MessageService,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.show('inbox');
  }

  get unread(): number {
    return this.messages.filter((m) => this.isNew(m)).length;
  }

  show(folder: Folder): void {
    this.folder = folder;
    this.loading = true;
    this.service.folder(folder).subscribe({
      next: (messages) => {
        this.messages = messages;
        this.loading = false;
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.loading = false;
      },
    });
  }

  isNew(message: Message): boolean {
    return !message.mine && message.readAt === null;
  }
}
