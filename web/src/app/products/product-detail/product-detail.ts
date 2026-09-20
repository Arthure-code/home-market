import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, effect, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../auth/api-message';
import { SessionService } from '../../auth/session.service';
import { Product } from '../product';
import { ProductService } from '../product.service';

// One product on its own page, reached by its id, with its price and its
// stock. A member can like it; its seller can edit or remove it.
@Component({
  selector: 'app-product-detail',
  imports: [CurrencyPipe, DatePipe, RouterLink],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.css',
})
export class ProductDetail {
  private readonly service = inject(ProductService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);

  readonly id = input.required<string>();
  readonly product = signal<Product | null>(null);
  readonly missing = signal(false);
  readonly signedIn = inject(SessionService).signedIn;

  constructor() {
    effect(() => this.load(Number(this.id())));
  }

  toggleLike(): void {
    const product = this.product();
    if (!product) return;
    const liked = !product.liked;
    this.service.setLike(product.id, liked).subscribe({
      next: () => {
        this.product.set({ ...product, liked, likes: product.likes + (liked ? 1 : -1) });
        this.toastr.success(liked ? 'Added to your likes' : 'Removed from your likes');
      },
      error: (error: unknown) => this.toastr.error(apiMessage(error)),
    });
  }

  remove(): void {
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
