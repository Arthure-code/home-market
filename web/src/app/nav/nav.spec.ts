import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';
import { Nav } from './nav';
import { SessionService } from '../auth/session.service';
import { ProductService } from '../products/product.service';

describe('Nav', () => {
  let fixture: ComponentFixture<Nav>;
  let signedIn: ReturnType<typeof signal<boolean>>;

  const root = () => fixture.nativeElement as HTMLElement;
  const show = async () => {
    fixture = TestBed.createComponent(Nav);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  };

  beforeEach(async () => {
    signedIn = signal(false);
    await TestBed.configureTestingModule({
      imports: [Nav],
      providers: [
        provideRouter([]),
        {
          provide: SessionService,
          useValue: { signedIn, userName: () => 'nadia', signOut: () => signedIn.set(false) },
        },
        {
          provide: ProductService,
          useValue: {
            categories: () =>
              of([
                { name: 'Electronics', count: 6 },
                { name: 'Kitchen', count: 5 },
              ]),
          },
        },
        { provide: ToastrService, useValue: { error: vi.fn() } },
      ],
    }).compileComponents();
  });

  it('gives a visitor the search box, the sign-in form and the categories', async () => {
    await show();

    expect(root().querySelector('[data-testid="search"]')).not.toBeNull();
    expect(root().querySelector('[data-testid="sign-in"]')).not.toBeNull();
    const links = Array.from(root().querySelectorAll('.categories a')).map((a) =>
      a.textContent?.trim(),
    );
    expect(links).toEqual(['All', 'Electronics', 'Kitchen']);
    expect(root().querySelector('.categories a[href="/products?category=Kitchen"]')).not.toBeNull();
  });

  it('gives a member the account menu and Sell, and signs out', async () => {
    signedIn.set(true);
    await show();

    expect(root().querySelector('[data-testid="sign-in"]')).toBeNull();
    expect(root().querySelector('[data-testid="account"]')?.textContent).toContain('nadia');
    expect(root().querySelector('.categories a[href="/products/new"]')).not.toBeNull();

    root().querySelector<HTMLButtonElement>('[data-testid="sign-out"]')?.click();
    await fixture.whenStable();
    expect(root().querySelector('[data-testid="sign-in"]')).not.toBeNull();
  });

  it('searches by going to the catalogue with the text in the address', async () => {
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    await show();

    const input = root().querySelector<HTMLInputElement>('#nav-search')!;
    input.value = '  kettle ';
    input.dispatchEvent(new Event('input'));
    await fixture.whenStable();
    root().querySelector('[data-testid="search"]')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    expect(navigate).toHaveBeenCalledWith(['/products'], { queryParams: { q: 'kettle' } });
  });
});
