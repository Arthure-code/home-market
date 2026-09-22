import { Component, EventEmitter, Output } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../helpers/api-message';
import { UploadedPhoto } from '../models/product';
import { ProductService } from '../services/product.service';

// Drop or pick an image, send it, hand back the name the API gave it.
@Component({
  selector: 'app-photo-uploader',
  imports: [],
  templateUrl: './photo-uploader.html',
  styleUrl: './photo-uploader.css',
})
export class PhotoUploader {
  @Output() uploaded = new EventEmitter<UploadedPhoto>();
  file: File | null = null;
  busy = false;

  constructor(
    private readonly service: ProductService,
    private readonly toastr: ToastrService,
  ) {}

  onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.pick(event.dataTransfer?.files?.[0]);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.pick(input.files?.[0]);
    input.value = '';
  }

  clear(): void {
    this.file = null;
  }

  upload(): void {
    const file = this.file;
    if (!file) return;
    this.busy = true;
    this.service.uploadPhoto(file).subscribe({
      next: (photo) => {
        this.toastr.success('Photo uploaded, save the product to keep it');
        this.uploaded.emit(photo);
        this.clear();
        this.busy = false;
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.busy = false;
      },
    });
  }

  private pick(file: File | undefined): void {
    if (!file) return;
    if (!file.type.startsWith('image/')) {
      this.toastr.error('Please choose an image file');
      return;
    }
    this.file = file;
  }
}
