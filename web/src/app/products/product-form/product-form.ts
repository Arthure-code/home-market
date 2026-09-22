import { Component, OnInit, inject, input, linkedSignal, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../helpers/api-message';
import { Category, ProductDraft, UploadedPhoto } from '../../models/product';
import { PhotoUploader } from '../../photo-uploader/photo-uploader';
import { ProductService } from '../../services/product.service';

// The fields of a listing, shared by Sell and Edit. It fills the draft
// it is given, shows the photo and takes a new one; the page that owns
// it decides what to do when it is submitted.
@Component({
  selector: 'app-product-form',
  imports: [FormsModule, PhotoUploader, RouterLink],
  templateUrl: './product-form.html',
})
export class ProductForm implements OnInit {
  private readonly service = inject(ProductService);
  private readonly toastr = inject(ToastrService);

  readonly heading = input.required<string>();
  readonly submitLabel = input.required<string>();
  readonly draft = input.required<ProductDraft>();
  readonly photoUrl = input('');
  readonly saving = input(false);
  readonly submitted = output<void>();

  protected readonly categories = signal<Category[]>([]);
  protected readonly preview = linkedSignal(() => this.photoUrl());

  ngOnInit(): void {
    this.service.categories().subscribe({
      next: (categories) => this.categories.set(categories),
      error: (error: unknown) => this.toastr.error(apiMessage(error)),
    });
  }

  protected submit(): void {
    if (!this.draft().category) {
      this.toastr.error('Pick a category');
      return;
    }
    this.submitted.emit();
  }

  protected photoUploaded(photo: UploadedPhoto): void {
    this.draft().photo = photo.photo;
    this.preview.set(photo.url);
  }
}
