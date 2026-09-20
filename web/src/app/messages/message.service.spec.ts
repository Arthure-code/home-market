import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { MESSAGES_URL, MessageService } from './message.service';

describe('MessageService', () => {
  let service: MessageService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(MessageService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('reads the inbox, the sent folder and one message', () => {
    service.folder('inbox').subscribe();
    service.folder('sent').subscribe();
    service.get(3).subscribe();

    http.expectOne(`${MESSAGES_URL}/inbox`).flush([]);
    http.expectOne(`${MESSAGES_URL}/sent`).flush([]);
    http.expectOne(`${MESSAGES_URL}/3`).flush({});
  });

  it('posts a message without any sender in it', () => {
    const draft = { to: 'marc', subject: 'About the kettle', body: 'Still available?' };
    service.send(draft).subscribe();

    const request = http.expectOne(MESSAGES_URL);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(draft);
    request.flush({});
  });

  it('marks a message read with a PUT', () => {
    service.markRead(3).subscribe();

    const request = http.expectOne(`${MESSAGES_URL}/3/read`);
    expect(request.request.method).toBe('PUT');
    request.flush(null, { status: 204, statusText: 'No Content' });
  });
});
