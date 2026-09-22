import { Component, Input, OnInit } from '@angular/core';
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
export class ProductEdit implements OnInit {
  @Input({ required: true }) id!: string;
  draft: ProductDraft | null = null;
  photoUrl = '';
  saving = false;

  constructor(
    private service: ProductService,
    private router: Router,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.service.get(Number(this.id)).subscribe({
      next: (product) => {
        this.draft = ProductEdit.draftOf(product);
        this.photoUrl = product.photoUrl;
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.router.navigateByUrl('/products');
      },
    });
  }

  update(): void {
    const draft = this.draft;
    if (!draft) return;
    this.saving = true;
    this.service.update(Number(this.id), draft).subscribe({
      next: (product) => {
        this.toastr.success('Product updated');
        this.router.navigate(['/products', product.id]);
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.saving = false;
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
