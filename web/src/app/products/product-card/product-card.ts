import { CurrencyPipe } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Product } from '../product';

// One product on the grid. The photo and the title open its page; the
// buttons are a quick view, a like and a cart for everyone, edit for the
// seller. A visitor who likes or adds is sent to sign in by the list.
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
}
