import { CurrencyPipe } from '@angular/common';
import {
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  ViewChild,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { Product } from '../../models/product';
import { StockLabelPipe } from '../../pipes/stock-label-pipe';

// A look at one product without leaving the grid: the photo, the price,
// the stock, the description, and the same like and cart buttons as the
// card, in a native dialog. A link opens the full page.
@Component({
  selector: 'app-quick-view',
  imports: [CurrencyPipe, RouterLink, StockLabelPipe],
  templateUrl: './quick-view.html',
  styleUrl: './quick-view.css',
})
export class QuickView implements OnChanges {
  @Input() product: Product | null = null;
  @Output() closed = new EventEmitter<void>();
  @Output() toggleLike = new EventEmitter<Product>();
  @Output() addToCart = new EventEmitter<Product>();

  @ViewChild('dialog', { static: true }) dialog!: ElementRef<HTMLDialogElement>;

  // The dialog opens when a product comes in and closes when it is
  // taken away.
  ngOnChanges(): void {
    const dialog = this.dialog.nativeElement;
    if (this.product) {
      if (!dialog.open) dialog.showModal();
    } else if (dialog.open) {
      dialog.close();
    }
  }

  close(): void {
    this.dialog.nativeElement.close();
  }

  // Escape and a click outside close the dialog on their own (closedby);
  // the parent is told either way so it forgets the product.
  onClosed(): void {
    this.closed.emit();
  }
}
