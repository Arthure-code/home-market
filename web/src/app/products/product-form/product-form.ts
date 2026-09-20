import { Component, OnInit, computed, effect, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../auth/api-message';
import { PhotoUploader } from '../photo-uploader/photo-uploader';
import { Category, ProductDraft, UploadedPhoto } from '../product';
import { ProductService } from '../product.service';

const EMPTY: ProductDraft = {
  title: '',
  brand: '',
  maker: '',
  description: '',
  price: 0,
  category: '',
  stock: 1,
  photo: '',
};

// One form for a new listing and for changing one of mine. With an id
// in the address it loads the product; if the product is not mine the
// API says so and the form goes back to the catalogue.
@Component({
  selector: 'app-product-form',
  imports: [FormsModule, PhotoUploader, RouterLink],
  templateUrl: './product-form.html',
})
export class ProductForm implements OnInit {
  private readonly service = inject(ProductService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);

  readonly id = input<string>();
  readonly draft = signal<ProductDraft>({ ...EMPTY });
  readonly photoUrl = signal('');
  readonly saving = signal(false);
  readonly editing = computed(() => this.id() !== undefined);
  readonly categories = signal<Category[]>([]);

  constructor() {
    effect(() => {
      const id = this.id();
      if (id !== undefined) this.load(Number(id));
    });
  }

  ngOnInit(): void {
    this.service.categories().subscribe({
      next: (categories) => this.categories.set(categories),
      error: (error: unknown) => this.toastr.error(apiMessage(error)),
    });
  }

  save(): void {
    if (!this.draft().category) {
      this.toastr.error('Pick a category');
      return;
    }
    this.saving.set(true);
    const id = this.id();
    const call =
      id === undefined
        ? this.service.create(this.draft())
        : this.service.update(Number(id), this.draft());
    call.subscribe({
      next: (product) => {
        this.toastr.success(id === undefined ? 'Product listed' : 'Product updated');
        this.router.navigate(['/products', product.id]);
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.saving.set(false);
      },
    });
  }

  photoUploaded(photo: UploadedPhoto): void {
    this.draft.update((draft) => ({ ...draft, photo: photo.photo }));
    this.photoUrl.set(photo.url);
  }

  private load(id: number): void {
    this.service.get(id).subscribe({
      next: (product) => {
        if (!product.mine) {
          this.toastr.error('You can only edit your own products');
          this.router.navigate(['/products', id]);
          return;
        }
        this.draft.set({
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
        });
        this.photoUrl.set(product.photoUrl);
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.router.navigateByUrl('/products');
      },
    });
  }
}
