import { Component, OnInit, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { SessionService } from '../auth/session.service';
import { CartService } from '../cart/cart.service';
import { Category } from '../products/product';
import { ProductService } from '../products/product.service';

// The two bars at the top of every page. The first holds the brand, the
// search box, the cart, and a Sign in link for a visitor or the messages
// and the account menu for a member. The second lists the categories of
// the shop.
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

  protected readonly signedIn = this.session.signedIn;
  protected readonly userName = this.session.userName;
  protected readonly cartCount = this.cart.count;
  protected readonly categories = signal<Category[]>([]);
  protected query = '';

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

  protected search(): void {
    const q = this.query.trim();
    this.router.navigate(['/products'], { queryParams: q ? { q } : {} });
  }

  protected signOut(): void {
    this.session.signOut();
    this.router.navigateByUrl('/');
  }
}
