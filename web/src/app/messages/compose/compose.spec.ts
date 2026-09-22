import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, of, throwError } from 'rxjs';
import { Compose } from './compose';
import { MessageDetail, MessageDraft } from '../../models/message';
import { MessageService } from '../../services/message.service';

describe('Compose', () => {
  let fixture: ComponentFixture<Compose>;
  let answer: Observable<MessageDetail>;
  let sent: MessageDraft[];
  let toastr: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

  const root = () => fixture.nativeElement as HTMLElement;
  const field = (id: string) => root().querySelector<HTMLInputElement>(`#${id}`);
  const open = async (to?: string, subject?: string) => {
    fixture = TestBed.createComponent(Compose);
    if (to !== undefined) fixture.componentRef.setInput('to', to);
    if (subject !== undefined) fixture.componentRef.setInput('subject', subject);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  };
  const type = async (id: string, value: string) => {
    const input = field(id)!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    await fixture.whenStable();
  };

  beforeEach(async () => {
    answer = of({} as MessageDetail);
    sent = [];
    toastr = { success: vi.fn(), error: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [Compose],
      providers: [
        provideRouter([]),
        {
          provide: MessageService,
          useValue: {
            send: (draft: MessageDraft) => {
              sent.push(draft);
              return answer;
            },
          },
        },
        { provide: ToastrService, useValue: toastr },
      ],
    }).compileComponents();
  });

  it('fills the recipient and the subject from the address', async () => {
    await open('lena', 'About the kettle');

    expect(field('to')?.value).toBe('lena');
    expect(field('subject')?.value).toBe('About the kettle');
    expect(field('body')?.value).toBe('');
  });

  it('refuses to send with a field empty', async () => {
    await open();
    await type('to', 'lena');

    root().querySelector<HTMLButtonElement>('[data-testid="send"]')?.click();
    await fixture.whenStable();

    expect(sent).toEqual([]);
    expect(toastr.error).toHaveBeenCalledWith(
      'Please fill in the recipient, the subject and the message',
    );
  });

  it('sends what was typed and goes to the messages', async () => {
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    await open('lena');
    await type('subject', 'About the kettle');
    await type('body', 'Is it still available?');

    root().querySelector<HTMLButtonElement>('[data-testid="send"]')?.click();
    await fixture.whenStable();

    expect(sent).toEqual([
      { to: 'lena', subject: 'About the kettle', body: 'Is it still available?' },
    ]);
    expect(toastr.success).toHaveBeenCalledWith('Message sent to lena');
    expect(navigate).toHaveBeenCalledWith('/messages');
  });

  it('shows the API message when the recipient does not exist', async () => {
    answer = throwError(
      () =>
        new HttpErrorResponse({ status: 404, error: { detail: 'No member has that user name.' } }),
    );
    await open('nobody', 'Hi');
    await type('body', 'Hi');

    root().querySelector<HTMLButtonElement>('[data-testid="send"]')?.click();
    await fixture.whenStable();

    expect(toastr.error).toHaveBeenCalledWith('No member has that user name.');
    expect(root().querySelector<HTMLButtonElement>('[data-testid="send"]')?.disabled).toBe(false);
  });
});
