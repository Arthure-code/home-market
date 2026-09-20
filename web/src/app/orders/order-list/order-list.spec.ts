import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';
import { OrderList } from './order-list';
import { Order } from '../order';
import { OrderService } from '../order.service';

const orders = [
  {
    id: 8,
    placedAt: '2026-09-20T14:00:00Z',
    total: 105.78,
    lines: [{ title: 'Fan' }, { title: 'Mug' }, { title: 'Lamp' }],
  },
  { id: 7, placedAt: '2026-09-18T10:00:00Z', total: 13.8, lines: [{ title: 'Mug' }] },
] as Order[];

describe('OrderList', () => {
  let fixture: ComponentFixture<OrderList>;
  let answer: Order[];

  const root = () => fixture.nativeElement as HTMLElement;
  const show = async () => {
    fixture = TestBed.createComponent(OrderList);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  };

  beforeEach(async () => {
    answer = orders;
    await TestBed.configureTestingModule({
      imports: [OrderList],
      providers: [
        provideRouter([]),
        { provide: OrderService, useValue: { mine: () => of(answer) } },
        { provide: ToastrService, useValue: { error: vi.fn() } },
      ],
    }).compileComponents();
  });

  it('lists my orders with a one-line summary each', async () => {
    await show();

    const rows = root().querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('#8');
    expect(rows[0].textContent).toContain('Fan and 2 more');
    expect(rows[0].textContent).toContain('$105.78');
    expect(rows[1].textContent).toContain('Mug');
    expect(rows[1].textContent).not.toContain('more');
    expect(rows[0].querySelector('a')?.getAttribute('href')).toBe('/orders/8');
  });

  it('says so when there is none', async () => {
    answer = [];
    await show();

    expect(root().querySelector('[data-testid="empty"]')).not.toBeNull();
  });
});
