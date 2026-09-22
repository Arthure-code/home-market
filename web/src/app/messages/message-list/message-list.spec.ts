import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';
import { MessageList } from './message-list';
import { Message } from '../../models/message';
import { Folder, MessageService } from '../../services/message.service';

const message = (id: number, mine: boolean, readAt: string | null = null): Message => ({
  id,
  from: mine ? 'me' : 'lena',
  to: mine ? 'lena' : 'me',
  subject: `Subject ${id}`,
  sentAt: '2026-09-10T14:00:00Z',
  readAt,
  mine,
});

describe('MessageList', () => {
  let fixture: ComponentFixture<MessageList>;
  let calls: Folder[];

  const root = () => fixture.nativeElement as HTMLElement;
  const text = (testId: string) =>
    root().querySelector(`[data-testid="${testId}"]`)?.textContent?.replace(/\s+/g, ' ').trim();
  const rows = () => root().querySelectorAll('tbody tr');

  beforeEach(async () => {
    calls = [];
    await TestBed.configureTestingModule({
      imports: [MessageList],
      providers: [
        provideRouter([]),
        {
          provide: MessageService,
          useValue: {
            folder: (folder: Folder) => {
              calls.push(folder);
              return of(
                folder === 'inbox'
                  ? [message(2, false), message(1, false, '2026-09-11T09:00:00Z')]
                  : [message(3, true)],
              );
            },
          },
        },
        { provide: ToastrService, useValue: { error: vi.fn() } },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(MessageList);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  });

  it('opens on the inbox, newest first, the unread line in bold with a badge', () => {
    expect(calls).toEqual(['inbox']);
    expect(rows().length).toBe(2);
    expect(rows()[0].classList.contains('fw-bold')).toBe(true);
    expect(rows()[0].textContent).toContain('New');
    expect(rows()[0].textContent).toContain('lena');
    expect(rows()[1].classList.contains('fw-bold')).toBe(false);
    expect(text('unread')).toBe('1');
    expect(root().querySelector('a[href="/messages/2"]')).not.toBeNull();
  });

  it('switches to the sent folder, where nothing is new', async () => {
    root().querySelector<HTMLButtonElement>('[data-testid="sent"]')?.click();
    await fixture.whenStable();

    expect(calls).toEqual(['inbox', 'sent']);
    expect(rows().length).toBe(1);
    expect(rows()[0].classList.contains('fw-bold')).toBe(false);
    expect(root().querySelector('thead')?.textContent).toContain('To');
    expect(root().querySelector('[data-testid="unread"]')).toBeNull();
  });
});
