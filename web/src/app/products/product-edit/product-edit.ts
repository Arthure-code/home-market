import { Component, effect, inject, input, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../helpers/api-message';
import { Product, ProductDraft } from '../../models/product';
import { ProductService } from '../../services/product.service';
import { ProductForm } from '../product-form/product-form';

// Edit one of my products: loaded by the id in the address (the owner
// guard has already checked it is mine), updated on submit, then its
// page.
@Component({
  selector: 'app-product-edit',
  imports: [ProductForm],
  templateUrl: './product-edit.html',
})
export class ProductEdit {
  private readonly service = inject(ProductService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);

  readonly id = input.required<string>();
  protected readonly draft = signal<ProductDraft | null>(null);
  protected readonly photoUrl = signal('');
  protected readonly saving = signal(false);

  constructor() {
    effect(() => this.load(Number(this.id())));
  }

  protected update(): void {
    const draft = this.draft();
    if (!draft) return;
    this.saving.set(true);
    this.service.update(Number(this.id()), draft).subscribe({
      next: (product) => {
        this.toastr.success('Product updated');
        this.router.navigate(['/products', product.id]);
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.saving.set(false);
      },
    });
  }

  private load(id: number): void {
    this.service.get(id).subscribe({
      next: (product) => {
        this.draft.set(ProductEdit.draftOf(product));
        this.photoUrl.set(product.photoUrl);
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.router.navigateByUrl('/products');
      },
    });
  }

  // The draft names the photo the way the API wants it back: a linked
  // address as is, an uploaded file by its name.
  private static draftOf(product: Product): ProductDraft {
    return {
      title: product.title,
      brand: product.brand,
      maker: product.maker,
      description: product.description,
      price: product.price,
      category: product.category,
      stock: product.stock,
      photo: product.photoUrl.startsWith('https://')
        ? product.photoUrl
        : (product.photoUrl.split('/').pop() ?? ''),
    };
  }
}
