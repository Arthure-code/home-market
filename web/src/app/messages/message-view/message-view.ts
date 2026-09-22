import { DatePipe } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { MessageDetail } from '../../models/message';
import { MessageService } from '../../services/message.service';

// One message, reached by its id. The API answers only its sender and
// its recipient, so a message that is not mine reads as missing. Opening
// a message I received marks it read.
@Component({
  selector: 'app-message-view',
  imports: [DatePipe, RouterLink],
  templateUrl: './message-view.html',
  styleUrl: './message-view.css',
})
export class MessageView implements OnInit {
  @Input({ required: true }) id!: string;
  message: MessageDetail | null = null;

  constructor(
    private service: MessageService,
    private router: Router,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.service.get(Number(this.id)).subscribe({
      next: (message) => {
        this.message = message;
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
      next: () => (this.message = { ...message, readAt: new Date().toISOString() }),
      error: () => {
        // The message is shown anyway; it will read as new next time.
      },
    });
  }
}
