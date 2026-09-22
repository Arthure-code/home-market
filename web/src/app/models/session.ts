// What the API returns at sign-in and what the client keeps for the
// session: the signed token and when it stops working.
export interface Session {
  userName: string;
  token: string;
  expiresAt: string;
}

export interface Credentials {
  userName: string;
  password: string;
}
