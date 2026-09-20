import { Component, OnInit, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../auth/api-message';
import { Credentials } from '../auth/session';
import { SessionService } from '../auth/session.service';
import { CartService } from '../cart/cart.service';
import { Category } from '../products/product';
import { ProductService } from '../products/product.service';

// The two bars at the top of every page. The first holds the brand, the
// search box, and for a visitor the sign-in form, for a member the
// messages, the account menu and the cart with its count. The second
// lists the categories of the shop.
@Component({
  selector: 'app-nav',
  imports: [FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav implements OnInit {
  private readonly session = inject(SessionService);
  private readonly products = inject(ProductService);
  private readonly cart = inject(CartService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);

  readonly signedIn = this.session.signedIn;
  readonly userName = this.session.userName;
  readonly cartCount = this.cart.count;
  readonly categories = signal<Category[]>([]);
  readonly credentials: Credentials = { userName: '', password: '' };
  query = '';

  constructor() {
    // The cart is fetched when a member arrives or signs in, and dropped
    // when they leave.
    effect(() => {
      if (this.signedIn()) {
        this.cart.load().subscribe({ error: () => this.cart.clear() });
      } else {
        this.cart.clear();
      }
    });
  }

  ngOnInit(): void {
    this.products.categories().subscribe({
      next: (categories) => this.categories.set(categories),
      error: () => this.categories.set([]),
    });
  }

  search(): void {
    const q = this.query.trim();
    this.router.navigate(['/products'], { queryParams: q ? { q } : {} });
  }

  signIn(): void {
    if (!this.credentials.userName || !this.credentials.password) {
      this.toastr.error('Please enter your user name and password');
      return;
    }
    this.session.signIn(this.credentials).subscribe({
      next: () => {
        this.credentials.password = '';
        this.router.navigateByUrl('/products');
      },
      error: (error: unknown) => this.toastr.error(apiMessage(error)),
    });
  }

  signOut(): void {
    this.session.signOut();
    this.router.navigateByUrl('/');
  }
}
