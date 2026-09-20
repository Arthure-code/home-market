import { CurrencyPipe } from '@angular/common';
import { Component, inject, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SessionService } from '../../auth/session.service';
import { Product } from '../product';

// One product on the grid. The photo and the title open its page; the
// buttons are a quick view for everyone, like and cart for a member,
// edit for the seller.
@Component({
  selector: 'app-product-card',
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './product-card.html',
  styleUrl: './product-card.css',
})
export class ProductCard {
  readonly product = input.required<Product>();
  readonly toggleLike = output<Product>();
  readonly addToCart = output<Product>();
  readonly quickView = output<Product>();
  readonly signedIn = inject(SessionService).signedIn;
}
