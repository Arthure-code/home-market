import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
} from '@angular/core';
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
export class ProductForm implements OnInit, OnChanges {
  @Input({ required: true }) heading!: string;
  @Input({ required: true }) submitLabel!: string;
  @Input({ required: true }) draft!: ProductDraft;
  @Input() photoUrl = '';
  @Input() saving = false;
  @Output() submitted = new EventEmitter<void>();

  categories: Category[] = [];
  preview = '';

  constructor(
    private readonly service: ProductService,
    private readonly toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.service.categories().subscribe({
      next: (categories) => (this.categories = categories),
      error: (error: unknown) => this.toastr.error(apiMessage(error)),
    });
  }

  // The photo shown is the one the page hands over, until a new one is
  // uploaded.
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['photoUrl']) this.preview = this.photoUrl;
  }

  submit(): void {
    if (!this.draft.category) {
      this.toastr.error('Pick a category');
      return;
    }
    this.submitted.emit();
  }

  photoUploaded(photo: UploadedPhoto): void {
    this.draft.photo = photo.photo;
    this.preview = photo.url;
  }
}
