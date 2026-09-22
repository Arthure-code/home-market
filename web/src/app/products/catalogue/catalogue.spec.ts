import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';
import { Catalogue } from './catalogue';
import { SignInPrompt } from '../../auth/sign-in-prompt';
import { ProductFilter } from '../product';
import { ProductService } from '../product.service';

describe('Catalogue', () => {
  let fixture: ComponentFixture<Catalogue>;
  let listedWith: ProductFilter[];

  const title = () =>
    (fixture.nativeElement as HTMLElement)
      .querySelector('[data-testid="title"]')
      ?.textContent?.trim();
  const open = async (inputs: { q?: string; category?: string }) => {
    fixture = TestBed.createComponent(Catalogue);
    if (inputs.q !== undefined) fixture.componentRef.setInput('q', inputs.q);
    if (inputs.category !== undefined) fixture.componentRef.setInput('category', inputs.category);
    fixture.autoDetectChanges();
    await fixture.whenStable();
  };

  beforeEach(async () => {
    listedWith = [];
    await TestBed.configureTestingModule({
      imports: [Catalogue],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: ProductService,
          useValue: {
            list: (filter: ProductFilter) => {
              listedWith.push(filter);
              return of([]);
            },
          },
        },
        { provide: SignInPrompt, useValue: { ensure: () => true } },
        { provide: ToastrService, useValue: { success: vi.fn(), error: vi.fn() } },
      ],
    }).compileComponents();
  });

  it('is titled Catalogue when nothing narrows it', async () => {
    await open({});

    expect(title()).toBe('Catalogue');
  });

  it('names the search when one comes from the address', async () => {
    await open({ q: '  lamp ' });

    expect(title()).toBe('Results for "lamp"');
    expect(listedWith.at(-1)).toEqual({ q: '  lamp ', category: undefined });
  });

  it('takes the category as its title, before any search text', async () => {
    await open({ q: 'lamp', category: 'Home' });

    expect(title()).toBe('Home');
    expect(listedWith.at(-1)).toEqual({ q: 'lamp', category: 'Home' });
  });
});
