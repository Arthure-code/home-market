import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../helpers/api-message';
import { Product } from '../../models/product';
import { CartService } from '../../services/cart.service';
import { ProductService } from '../../services/product.service';
import { SignInPrompt } from '../../services/sign-in-prompt';
import { ProductCard } from '../product-card/product-card';
import { QuickView } from '../quick-view/quick-view';

// The catalogue: every product on a grid of cards. The same grid serves
// the front page, the catalogue page (narrowed by a search or a
// category) and the two lists of a member.
@Component({
  selector: 'app-product-list',
  imports: [ProductCard, QuickView],
  templateUrl: './product-list.html',
})
export class ProductList implements OnInit, OnChanges {
  // Which list to show: the whole catalogue, my listings, or my likes.
  @Input() source: 'all' | 'mine' | 'liked' = 'all';
  @Input() query?: string;
  @Input() category?: string;
  products: Product[] = [];
  loading = true;
  // The product open in the quick view, if any. It follows the list, so a
  // like given in the dialog shows on its card too.
  viewing: Product | null = null;

  constructor(
    private service: ProductService,
    private cart: CartService,
    private prompt: SignInPrompt,
    private router: Router,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.load();
  }

  // Loads again when the search or the category in the address changes;
  // the first values are taken care of by ngOnInit.
  ngOnChanges(changes: SimpleChanges): void {
    if (Object.values(changes).some((change) => !change.firstChange)) this.load();
  }

  toggleLike(product: Product): void {
    if (!this.prompt.ensure()) return;
    const liked = !product.liked;
    this.service.setLike(product.id, liked).subscribe({
      next: () => {
        const changed = { ...product, liked, likes: product.likes + (liked ? 1 : -1) };
        this.products = this.products.map((p) => (p.id === product.id ? changed : p));
        if (this.viewing?.id === product.id) this.viewing = changed;
        this.toastr.success(liked ? 'Added to your likes' : 'Removed from your likes');
      },
      error: (error: unknown) => this.toastr.error(apiMessage(error)),
    });
  }

  // One of it goes in the cart, and the cart opens.
  addToCart(product: Product): void {
    if (!this.prompt.ensure()) return;
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
      all: () => this.service.list({ q: this.query, category: this.category }),
      mine: () => this.service.mine(),
      liked: () => this.service.liked(),
    };
    this.loading = true;
    calls[this.source]().subscribe({
      next: (products) => {
        this.products = products;
        this.loading = false;
      },
      error: (error: unknown) => {
        this.loading = false;
        this.toastr.error(apiMessage(error));
      },
    });
  }
}
