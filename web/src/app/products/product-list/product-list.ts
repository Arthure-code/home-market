import { Component, effect, inject, input, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../auth/api-message';
import { CartService } from '../../cart/cart.service';
import { Product } from '../product';
import { ProductCard } from '../product-card/product-card';
import { ProductService } from '../product.service';
import { QuickView } from '../quick-view/quick-view';

// The catalogue: every product on a grid of cards. The same grid serves
// the front page, the catalogue page (narrowed by a search or a
// category) and the two lists of a member.
@Component({
  selector: 'app-product-list',
  imports: [ProductCard, QuickView],
  templateUrl: './product-list.html',
})
export class ProductList {
  private readonly service = inject(ProductService);
  private readonly toastr = inject(ToastrService);
  private readonly cart = inject(CartService);
  private readonly router = inject(Router);

  // Which list to show: the whole catalogue, my listings, or my likes.
  readonly source = input<'all' | 'mine' | 'liked'>('all');
  readonly query = input<string>();
  readonly category = input<string>();
  readonly products = signal<Product[]>([]);
  readonly loading = signal(true);
  // The product open in the quick view, if any. It follows the list, so a
  // like given in the dialog shows on its card too.
  readonly viewing = signal<Product | null>(null);

  constructor() {
    effect(() => this.load());
  }

  toggleLike(product: Product): void {
    const liked = !product.liked;
    this.service.setLike(product.id, liked).subscribe({
      next: () => {
        const changed = { ...product, liked, likes: product.likes + (liked ? 1 : -1) };
        this.products.update((list) => list.map((p) => (p.id === product.id ? changed : p)));
        if (this.viewing()?.id === product.id) this.viewing.set(changed);
        this.toastr.success(liked ? 'Added to your likes' : 'Removed from your likes');
      },
      error: (error: unknown) => this.toastr.error(apiMessage(error)),
    });
  }

  // One of it goes in the cart, and the cart opens.
  addToCart(product: Product): void {
    this.cart.add(product.id).subscribe({
      next: () => {
        this.toastr.success('Added to your cart');
        this.router.navigateByUrl('/cart');
      },
      error: (error: unknown) => this.toastr.error(apiMessage(error)),
    });
  }

  private load(): void {
    const calls = {
      all: () => this.service.list({ q: this.query(), category: this.category() }),
      mine: () => this.service.mine(),
      liked: () => this.service.liked(),
    };
    this.loading.set(true);
    calls[this.source()]().subscribe({
      next: (products) => {
        this.products.set(products);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.toastr.error(apiMessage(error));
      },
    });
  }
}
