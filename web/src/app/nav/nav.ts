import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { Category } from '../models/product';
import { CartService } from '../services/cart.service';
import { ProductService } from '../services/product.service';
import { SessionService } from '../services/session.service';

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
  categories: Category[] = [];
  query = '';

  constructor(
    public session: SessionService,
    public cart: CartService,
    private products: ProductService,
    private router: Router,
  ) {}

  // A member who comes back with a session still has a cart to show.
  ngOnInit(): void {
    this.products.categories().subscribe({
      next: (categories) => (this.categories = categories),
      error: () => (this.categories = []),
    });
    if (this.session.signedIn) {
      this.cart.load().subscribe({ error: () => this.cart.clear() });
    }
  }

  search(): void {
    const q = this.query.trim();
    this.router.navigate(['/products'], { queryParams: q ? { q } : {} });
  }

  signOut(): void {
    this.session.signOut();
    this.cart.clear();
    this.router.navigateByUrl('/');
  }
}
