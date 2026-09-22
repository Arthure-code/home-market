// What the checkout form sends: where to ship and how to pay. The card
// goes to the API once and is never kept, there or here.
export interface Checkout {
  fullName: string;
  street: string;
  city: string;
  province: string;
  postalCode: string;
  country: string;
  cardNumber: string;
  cardHolderName: string;
  expiry: string;
  securityCode: string;
}

export interface OrderLine {
  productId: number | null;
  title: string;
  seller: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

// An order as its buyer reads it back: the lines at the price of the
// day, the totals, the address, and the card by its brand and last four
// digits only.
export interface Order {
  id: number;
  placedAt: string;
  buyer: string;
  fullName: string;
  street: string;
  city: string;
  province: string;
  postalCode: string;
  country: string;
  cardBrand: string;
  cardLast4: string;
  subtotal: number;
  gst: number;
  qst: number;
  total: number;
  itemCount: number;
  lines: OrderLine[];
}

// One line another member bought from me.
export interface Sale {
  orderId: number;
  placedAt: string;
  buyer: string;
  productId: number | null;
  title: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
  shipTo: string;
}
