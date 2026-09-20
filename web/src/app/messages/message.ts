// A line of the inbox or of the sent folder, as the API lists it. Mine
// is true for a message I sent; readAt is null until the recipient opens
// it.
export interface Message {
  id: number;
  from: string;
  to: string;
  subject: string;
  sentAt: string;
  readAt: string | null;
  mine: boolean;
}

export interface MessageDetail extends Message {
  body: string;
}

// What a member sends. The sender is never in it: the API knows me from
// the token.
export interface MessageDraft {
  to: string;
  subject: string;
  body: string;
}
