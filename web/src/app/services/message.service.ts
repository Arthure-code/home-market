import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Message, MessageDetail, MessageDraft } from '../models/message';

export const MESSAGES_URL = 'http://localhost:5130/api/messages';

export type Folder = 'inbox' | 'sent';

// Every call is for the signed-in account; the interceptor attaches the
// token and the API reads who I am from it.
@Injectable({ providedIn: 'root' })
export class MessageService {
  constructor(private http: HttpClient) {}

  folder(folder: Folder): Observable<Message[]> {
    return this.http.get<Message[]>(`${MESSAGES_URL}/${folder}`);
  }

  get(id: number): Observable<MessageDetail> {
    return this.http.get<MessageDetail>(`${MESSAGES_URL}/${id}`);
  }

  send(draft: MessageDraft): Observable<MessageDetail> {
    return this.http.post<MessageDetail>(MESSAGES_URL, draft);
  }

  markRead(id: number): Observable<void> {
    return this.http.put<void>(`${MESSAGES_URL}/${id}/read`, null);
  }
}
