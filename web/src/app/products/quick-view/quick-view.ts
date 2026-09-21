import { CurrencyPipe } from '@angular/common';
import { Component, ElementRef, effect, input, output, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Product } from '../product';

// A look at one product without leaving the grid: the photo, the price,
// the stock, the description, and the same like and cart buttons as the
// card, in a native dialog. A link opens the full page.
@Component({
  selector: 'app-quick-view',
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './quick-view.html',
  styleUrl: './quick-view.css',
})
export class QuickView {
  readonly product = input<Product | null>(null);
  readonly closed = output<void>();
  readonly toggleLike = output<Product>();
  readonly addToCart = output<Product>();

  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');

  constructor() {
    effect(() => {
      const dialog = this.dialog().nativeElement;
      if (this.product()) {
        if (!dialog.open) dialog.showModal();
      } else if (dialog.open) {
        dialog.close();
      }
    });
  }

  close(): void {
    this.dialog().nativeElement.close();
  }

  // Escape and a click outside close the dialog on their own (closedby);
  // the parent is told either way so it forgets the product.
  onClosed(): void {
    this.closed.emit();
  }
}
