import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../helpers/api-message';
import { MAX_QUANTITY } from '../../models/cart';
import { Product } from '../../models/product';
import { StockLabelPipe } from '../../pipes/stock-label-pipe';
import { CartService } from '../../services/cart.service';
import { ProductService, canBeMessaged } from '../../services/product.service';
import { SignInPrompt } from '../../services/sign-in-prompt';

// One product on its own page, reached by its id, with its buy box. A
// member can like it, write to its seller and put some in the cart; its
// seller can edit or remove it.
@Component({
  selector: 'app-product-detail',
  imports: [CurrencyPipe, DatePipe, RouterLink, StockLabelPipe],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.css',
})
export class ProductDetail implements OnInit {
  @Input({ required: true }) id!: string;
  product: Product | null = null;
  missing = false;
  quantity = 1;
  adding = false;
  readonly canBeMessaged = canBeMessaged;

  constructor(
    private service: ProductService,
    private cart: CartService,
    private prompt: SignInPrompt,
    private router: Router,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.service.get(Number(this.id)).subscribe({
      next: (product) => (this.product = product),
      error: () => (this.missing = true),
    });
  }

  // One to ten, never more than the stock.
  get quantities(): number[] {
    const most = Math.min(this.product?.stock ?? 0, MAX_QUANTITY);
    return Array.from({ length: most }, (_, i) => i + 1);
  }

  toggleLike(): void {
    const product = this.product;
    if (!product || !this.prompt.ensure()) return;
    const liked = !product.liked;
    this.service.setLike(product.id, liked).subscribe({
      next: () => {
        this.product = { ...product, liked, likes: product.likes + (liked ? 1 : -1) };
        this.toastr.success(liked ? 'Added to your likes' : 'Removed from your likes');
      },
      error: (error: unknown) => this.toastr.error(apiMessage(error)),
    });
  }

  addToCart(): void {
    const product = this.product;
    if (!product || !this.prompt.ensure()) return;
    this.adding = true;
    this.cart.add(product.id, this.quantity).subscribe({
      next: () => {
        this.toastr.success('Added to your cart');
        this.router.navigateByUrl('/cart');
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.adding = false;
      },
    });
  }

  // The Message button is a link for a member and a prompt for a visitor.
  message(): void {
    const product = this.product;
    if (!product || !this.prompt.ensure()) return;
    this.router.navigate(['/messages/new', product.seller], {
      queryParams: { subject: `About ${product.title}` },
    });
  }

  remove(): void {
    const product = this.product;
    if (!product) return;
    this.service.delete(product.id).subscribe({
      next: () => {
        this.toastr.success('Listing removed');
        this.router.navigateByUrl('/my-products');
      },
      error: (error: unknown) => this.toastr.error(apiMessage(error)),
    });
  }
}
