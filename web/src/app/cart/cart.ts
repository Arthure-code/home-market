// The cart as the API keeps and totals it: one line per product, the
// stock of each so the page can offer the right quantities, and the
// taxes charged in Quebec.
export interface CartLine {
  productId: number;
  title: string;
  photoUrl: string;
  seller: string;
  unitPrice: number;
  quantity: number;
  stock: number;
  lineTotal: number;
}

export interface Cart {
  lines: CartLine[];
  itemCount: number;
  subtotal: number;
  gst: number;
  qst: number;
  total: number;
}

export const EMPTY_CART: Cart = { lines: [], itemCount: 0, subtotal: 0, gst: 0, qst: 0, total: 0 };

// Ten of a thing is the most one order takes.
export const MAX_QUANTITY = 10;
