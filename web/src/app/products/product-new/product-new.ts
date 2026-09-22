import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../helpers/api-message';
import { ProductDraft } from '../../models/product';
import { ProductService } from '../../services/product.service';
import { ProductForm } from '../product-form/product-form';

// Sell a product: an empty draft, created on submit, then its page.
@Component({
  selector: 'app-product-new',
  imports: [ProductForm],
  templateUrl: './product-new.html',
})
export class ProductNew {
  private readonly service = inject(ProductService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);

  protected readonly draft: ProductDraft = {
    title: '',
    brand: '',
    maker: '',
    description: '',
    price: 0,
    category: '',
    stock: 1,
    photo: '',
  };
  protected readonly saving = signal(false);

  protected create(): void {
    this.saving.set(true);
    this.service.create(this.draft).subscribe({
      next: (product) => {
        this.toastr.success('Product listed');
        this.router.navigate(['/products', product.id]);
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.saving.set(false);
      },
    });
  }
}
