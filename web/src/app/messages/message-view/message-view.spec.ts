import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, of, throwError } from 'rxjs';
import { MessageView } from './message-view';
import { MessageDetail } from '../message';
import { MessageService } from '../message.service';

const received: MessageDetail = {
  id: 7,
  from: 'lena',
  to: 'me',
  subject: 'About the kettle',
  body: 'Is it still available?',
  sentAt: '2026-09-10T14:00:00Z',
  readAt: null,
  mine: false,
};

describe('MessageView', () => {
  let fixture: ComponentFixture<MessageView>;
  let answer: Observable<MessageDetail>;
  let calls: string[];
  let toastr: { error: ReturnType<typeof vi.fn> };

  const root = () => fixture.nativeElement as HTMLElement;
  const text = (testId: string) =>
    root().querySelector(`[data-testid="${testId}"]`)?.textContent?.replace(/\s+/g, ' ').trim();
  const open = async (id: string) => {
    fixture = TestBed.createComponent(MessageView);
    fixture.componentRef.setInput('id', id);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  };

  beforeEach(async () => {
    answer = of(received);
    calls = [];
    toastr = { error: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [MessageView],
      providers: [
        provideRouter([]),
        {
          provide: MessageService,
          useValue: {
            get: (id: number) => {
              calls.push(`get ${id}`);
              return answer;
            },
            markRead: (id: number) => {
              calls.push(`read ${id}`);
              return of(undefined);
            },
          },
        },
        { provide: ToastrService, useValue: toastr },
      ],
    }).compileComponents();
  });

  it('shows a received message, marks it read and offers a reply', async () => {
    await open('7');

    expect(calls).toEqual(['get 7', 'read 7']);
    expect(root().querySelector('h1')?.textContent?.trim()).toBe('About the kettle');
    expect(text('parties')).toContain('From lena to me');
    expect(text('body')).toBe('Is it still available?');
    expect(fixture.componentInstance.message()?.readAt).not.toBeNull();
    expect(root().querySelector('[data-testid="reply"]')?.getAttribute('href')).toBe(
      '/messages/new/lena?subject=Re:%20About%20the%20kettle',
    );
  });

  it('shows a sent message without marking it or offering a reply', async () => {
    answer = of({ ...received, from: 'me', to: 'lena', mine: true });
    await open('7');

    expect(calls).toEqual(['get 7']);
    expect(root().querySelector('[data-testid="reply"]')).toBeNull();
  });

  it('goes back to the list when the message is not mine or does not exist', async () => {
    answer = throwError(() => new HttpErrorResponse({ status: 404 }));
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    await open('99');

    expect(toastr.error).toHaveBeenCalledWith('There is no such message in your folders.');
    expect(navigate).toHaveBeenCalledWith('/messages');
  });
});
