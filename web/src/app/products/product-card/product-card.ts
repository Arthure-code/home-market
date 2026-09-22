import { CurrencyPipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Product } from '../../models/product';
import { StockLabelPipe } from '../../pipes/stock-label-pipe';

// One product on the grid. The photo and the title open its page; the
// buttons are a quick view, a like and a cart for everyone, edit for the
// seller. A visitor who likes or adds is sent to sign in by the list.
@Component({
  selector: 'app-product-card',
  imports: [CurrencyPipe, RouterLink, StockLabelPipe],
  templateUrl: './product-card.html',
  styleUrl: './product-card.css',
})
export class ProductCard {
  @Input({ required: true }) product!: Product;
  @Output() toggleLike = new EventEmitter<Product>();
  @Output() addToCart = new EventEmitter<Product>();
  @Output() quickView = new EventEmitter<Product>();
}
