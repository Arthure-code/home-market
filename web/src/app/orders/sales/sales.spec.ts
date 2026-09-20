import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';
import { Sales } from './sales';
import { Sale } from '../order';
import { OrderService } from '../order.service';

const sales: Sale[] = [
  {
    orderId: 8,
    placedAt: '2026-09-20T14:00:00Z',
    buyer: 'eve',
    productId: 30,
    title: 'Fan',
    unitPrice: 40,
    quantity: 2,
    lineTotal: 80,
    shipTo: 'Nadia Roy, 12 rue des Érables, Québec, QC G1R 2B3',
  },
  {
    orderId: 5,
    placedAt: '2026-09-12T14:00:00Z',
    buyer: 'omar',
    productId: null,
    title: 'Old lamp',
    unitPrice: 15,
    quantity: 1,
    lineTotal: 15,
    shipTo: 'Omar B., 3 Main St, Lévis, QC G6V 1A1',
  },
];

describe('Sales', () => {
  let fixture: ComponentFixture<Sales>;
  let answer: Sale[];

  const root = () => fixture.nativeElement as HTMLElement;
  const show = async () => {
    fixture = TestBed.createComponent(Sales);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  };

  beforeEach(async () => {
    answer = sales;
    await TestBed.configureTestingModule({
      imports: [Sales],
      providers: [
        provideRouter([]),
        { provide: OrderService, useValue: { sales: () => of(answer) } },
        { provide: ToastrService, useValue: { error: vi.fn() } },
      ],
    }).compileComponents();
  });

  it('lists what I sold, links the products still on sale, and adds it up', async () => {
    await show();

    const rows = root().querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('Fan');
    expect(rows[0].textContent).toContain('eve');
    expect(rows[0].textContent).toContain('Nadia Roy');
    expect(rows[0].querySelector('a')?.getAttribute('href')).toBe('/products/30');
    expect(rows[1].querySelector('a')).toBeNull();
    expect(root().querySelector('[data-testid="revenue"]')?.textContent).toContain('$95.00');
  });

  it('says so when nothing sold', async () => {
    answer = [];
    await show();

    expect(root().querySelector('[data-testid="empty"]')).not.toBeNull();
    expect(root().querySelector('[data-testid="revenue"]')).toBeNull();
  });
});
