import { Component, inject, output, signal } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { apiMessage } from '../../auth/api-message';
import { UploadedPhoto } from '../product';
import { ProductService } from '../product.service';

// Drop or pick an image, send it, hand back the name the API gave it.
@Component({
  selector: 'app-photo-uploader',
  imports: [],
  templateUrl: './photo-uploader.html',
  styleUrl: './photo-uploader.css',
})
export class PhotoUploader {
  private readonly service = inject(ProductService);
  private readonly toastr = inject(ToastrService);

  readonly uploaded = output<UploadedPhoto>();
  protected readonly file = signal<File | null>(null);
  protected readonly busy = signal(false);

  protected onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  protected onDrop(event: DragEvent): void {
    event.preventDefault();
    this.pick(event.dataTransfer?.files?.[0]);
  }

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.pick(input.files?.[0]);
    input.value = '';
  }

  protected clear(): void {
    this.file.set(null);
  }

  protected upload(): void {
    const file = this.file();
    if (!file) return;
    this.busy.set(true);
    this.service.uploadPhoto(file).subscribe({
      next: (photo) => {
        this.toastr.success('Photo uploaded, save the product to keep it');
        this.uploaded.emit(photo);
        this.clear();
        this.busy.set(false);
      },
      error: (error: unknown) => {
        this.toastr.error(apiMessage(error));
        this.busy.set(false);
      },
    });
  }

  private pick(file: File | undefined): void {
    if (!file) return;
    if (!file.type.startsWith('image/')) {
      this.toastr.error('Please choose an image file');
      return;
    }
    this.file.set(file);
  }
}
