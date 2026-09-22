import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ToastrService } from 'ngx-toastr';
import { Observable, of, throwError } from 'rxjs';
import { PhotoUploader } from './photo-uploader';
import { UploadedPhoto } from '../models/product';
import { ProductService } from '../services/product.service';

const image = new File(['bytes'], 'lamp.png', { type: 'image/png' });
const text = new File(['hello'], 'notes.txt', { type: 'text/plain' });

describe('PhotoUploader', () => {
  let fixture: ComponentFixture<PhotoUploader>;
  let sent: File[];
  let answer: Observable<UploadedPhoto>;
  let toastr: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };
  let emitted: UploadedPhoto[];

  const root = () => fixture.nativeElement as HTMLElement;
  const fileName = () => root().querySelector('[data-testid="file-name"]')?.textContent?.trim();
  const uploadButton = () => root().querySelector<HTMLButtonElement>('[data-testid="upload"]');
  const choose = async (file: File) => {
    const input = root().querySelector<HTMLInputElement>('input[type="file"]')!;
    Object.defineProperty(input, 'files', { value: [file], configurable: true });
    input.dispatchEvent(new Event('change'));
    await fixture.whenStable();
  };
  const drop = async (file: File) => {
    const event = new Event('drop', { cancelable: true }) as DragEvent;
    Object.defineProperty(event, 'dataTransfer', { value: { files: [file] } });
    root().querySelector('.drop-zone')!.dispatchEvent(event);
    await fixture.whenStable();
    return event;
  };

  beforeEach(async () => {
    sent = [];
    emitted = [];
    answer = of({ photo: 'abc.png', url: 'http://localhost:5130/images/abc.png' });
    toastr = { success: vi.fn(), error: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [PhotoUploader],
      providers: [
        {
          provide: ProductService,
          useValue: {
            uploadPhoto: (file: File) => {
              sent.push(file);
              return answer;
            },
          },
        },
        { provide: ToastrService, useValue: toastr },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(PhotoUploader);
    fixture.componentInstance.uploaded.subscribe((photo) => emitted.push(photo));
    fixture.autoDetectChanges();
    await fixture.whenStable();
  });

  it('offers to upload once an image is chosen', async () => {
    expect(uploadButton()).toBeNull();

    await choose(image);

    expect(fileName()).toBe('lamp.png');
    expect(uploadButton()).not.toBeNull();
  });

  it('refuses a file that is not an image', async () => {
    await choose(text);

    expect(toastr.error).toHaveBeenCalledWith('Please choose an image file');
    expect(uploadButton()).toBeNull();
  });

  it('takes a dropped image and keeps the browser from opening it', async () => {
    const event = await drop(image);

    expect(event.defaultPrevented).toBe(true);
    expect(fileName()).toBe('lamp.png');
  });

  it('sends the image and hands back the name the API chose', async () => {
    await choose(image);

    uploadButton()!.click();
    await fixture.whenStable();

    expect(sent).toEqual([image]);
    expect(emitted).toEqual([{ photo: 'abc.png', url: 'http://localhost:5130/images/abc.png' }]);
    expect(toastr.success).toHaveBeenCalled();
    expect(uploadButton()).toBeNull();
  });

  it('says why when the API refuses, and keeps the file for another try', async () => {
    answer = throwError(
      () =>
        new HttpErrorResponse({
          status: 400,
          error: { errors: { photo: ['That file is not a JPEG, PNG, GIF or WebP image.'] } },
        }),
    );
    await choose(image);

    uploadButton()!.click();
    await fixture.whenStable();

    expect(toastr.error).toHaveBeenCalledWith('That file is not a JPEG, PNG, GIF or WebP image.');
    expect(emitted).toEqual([]);
    expect(fileName()).toBe('lamp.png');
    expect(uploadButton()!.disabled).toBe(false);
  });

  it('forgets the file on cancel', async () => {
    await choose(image);

    root().querySelector<HTMLButtonElement>('.btn-danger')!.click();
    await fixture.whenStable();

    expect(uploadButton()).toBeNull();
  });
});
