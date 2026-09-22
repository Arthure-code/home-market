import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../helpers/api-message';
import { SignInPrompt } from '../../services/sign-in-prompt';
import { MAX_QUANTITY } from '../../models/cart';
import { CartService } from '../../services/cart.service';
import { Product } from '../../models/product';
import { StockLabelPipe } from '../../pipes/stock-label-pipe';
import { ProductService, canBeMessaged } from '../../services/product.service';

// One product on its own page, reached by its id, with its buy box. A
// member can like it, write to its seller and put some in the cart; its
// seller can edit or remove it.
@Component({
  selector: 'app-product-detail',
  imports: [CurrencyPipe, DatePipe, RouterLink, StockLabelPipe],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.css',
})
export class ProductDetail {
  private readonly service = inject(ProductService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);
  private readonly cart = inject(CartService);
  private readonly prompt = inject(SignInPrompt);

  readonly id = input.required<string>();
  protected readonly product = signal<Product | null>(null);
  protected readonly missing = signal(false);
  protected readonly quantity = signal(1);
  protected readonly adding = signal(false);
  // One to ten, never more than the stock.
  protected readonly quantities = computed(() => {
    const most = Math.min(this.product()?.stock ?? 0, MAX_QUANTITY);
    return Array.from({ length: most }, (_, i) => i + 1);
  });
  protected readonly canBeMessaged = canBeMessaged;

  constructor() {
    effect(() => this.load(Number(this.id())));
  }

  protected toggleLike(): void {
    const product = this.product();
    if (!product || !this.prompt.ensure()) return;
    const liked = !product.liked;
    this.service.setLike(product.id, liked).subscribe({
      next: () => {
        this.product.set({ ...product, liked, likes: product.likes + (liked ? 1 : -1) });
        this.toastr.success(liked ? 'Added to your likes' : 'Removed from your likes');
      },
      error: (error: unknown) => this.toastr.error(apiMessage(error)),
    });
  }

  protected addToCart(): void {
    const product = this.product();
    if (!product || !this.prompt.ensure()) return;
    this.adding.set(true);
    this.cart.add(product.id, this.quantity()).subscribe({
      next: () => {
        this.toastr.success('Added to your cart');
        this.router.navigateByUrl('/cart');
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.adding.set(false);
      },
    });
  }

  // The Message button is a link for a member and a prompt for a visitor.
  protected message(): void {
    const product = this.product();
    if (!product || !this.prompt.ensure()) return;
    this.router.navigate(['/messages/new', product.seller], {
      queryParams: { subject: `About ${product.title}` },
    });
  }

  protected remove(): void {
    const product = this.product();
    if (!product) return;
    this.service.delete(product.id).subscribe({
      next: () => {
        this.toastr.success('Listing removed');
        this.router.navigateByUrl('/my-products');
      },
      error: (error: unknown) => this.toastr.error(apiMessage(error)),
    });
  }

  private load(id: number): void {
    this.service.get(id).subscribe({
      next: (product) => this.product.set(product),
      error: () => this.missing.set(true),
    });
  }
}
